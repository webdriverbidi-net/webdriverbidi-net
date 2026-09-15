namespace WebDriverBiDi.TestUtilities;

using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using WebDriverBiDi.Protocol;

public class TestWebSocketConnection : WebSocketConnection
{
    private readonly ObservableEventInvocable<TestWebSocketConnectionDataSentEventArgs> dataSendCompleteInvocable = new("connection.DataSendComplete");
    private readonly ObservableEventInvocable<WebDriverBiDiEventArgs> dataSendStartingInvocable = new("connection.dataSendStarting");

    private int receiveCallCount;
    private int stopCallCount;

    // The value of BypassStart at the moment the session started. The receive loop runs on its own
    // task, so reading the mutable property from inside the loop would make its behavior depend on
    // whether a test flipped the property before that task was first scheduled; several tests do flip
    // it immediately after starting, in order to route sends through the real send path.
    private bool startBypassed;

    public TestWebSocketConnection(TimeProvider? timeProvider = null)
    {
        if (timeProvider is not null)
        {
            this.TimeProvider = timeProvider;
        }
    }

    public bool BypassStart { get; set; } = true;

    public bool BypassStop { get; set; } = true;

    public bool BypassDataSend { get; set; } = true;

    public bool BypassCloseClientWebSocket { get; set; } = true;

    public bool ThrowOnStop { get; set; }

    public int StopCallCount => this.stopCallCount;

    public string? DataSent { get; set; }

    /// <summary>
    /// Gets the cancellation token the connection passed down to the send, so a test can assert which
    /// token was actually used rather than only that the send completed.
    /// </summary>
    public CancellationToken LastSendCancellationToken { get; private set; }

    /// <summary>
    /// Gets the connection's own cancellation token, which is protected on the base class.
    /// </summary>
    public CancellationToken ObservedConnectionCancellationToken => this.ConnectionCancellationToken;

    public TaskCompletionSource? SendBarrier { get; set; }

    public TaskCompletionSource? StartBarrier { get; set; }

    public Func<ArraySegment<byte>, CancellationToken, int, Task<WebSocketReceiveResult>>? ReceiveHandler { get; set; }

    public Func<bool>? IsConnectionOpenOverride { get; set; }

    public Func<ReadOnlyMemory<byte>, Task>? SendWebSocketDataOverride { get; set; }

    public bool ThrowWebSocketExceptionOnSend { get; set; }

    /// <summary>
    /// Gets or sets a delegate that replaces the underlying WebSocket connect operation, for example
    /// to simulate a remote end that never completes the handshake.
    /// </summary>
    public Func<Uri, CancellationToken, Task>? ConnectWebSocketOverride { get; set; }

    public bool Disposed => this.IsDisposed;

    protected override bool IsConnectionOpen
    {
        get
        {
            if (this.IsConnectionOpenOverride is not null)
            {
                return this.IsConnectionOpenOverride();
            }

            if (this.ThrowOnStop)
            {
                return true;
            }

            return base.IsConnectionOpen;
        }
    }

    public ObservableEvent<WebDriverBiDiEventArgs> OnDataSendStarting => this.dataSendStartingInvocable;

    public ObservableEvent<TestWebSocketConnectionDataSentEventArgs> OnDataSendComplete => this.dataSendCompleteInvocable;

    public async Task RaiseDataReceivedEventAsync(string data)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent(bytes.Length);
        bytes.CopyTo(owner.Memory);
        await this.RaiseDataReceivedEventAsync(owner, bytes.Length);
    }

    public async Task RaiseDataReceivedEventAsync(IMemoryOwner<byte> owner, int length)
    {
        await this.NotifyDataReceivedObserverAsync(owner, length);
    }

    /// <summary>
    /// Raises a log message through the connection's own <c>LogAsync</c>, so that the message is subject to
    /// <see cref="Connection.LogLevel"/>, and carries the connection's component name, exactly as a message the
    /// connection itself emits does.
    /// </summary>
    /// <param name="message">The log message to raise.</param>
    /// <param name="level">The level at which to raise it.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task RaiseLogMessageEventAsync(string message, WebDriverBiDiLogLevel level)
    {
        await this.LogAsync(message, level);
    }

    /// <summary>
    /// Reports a connection error as the receive loop does, through the base class, so the connection is marked
    /// inactive before the error is logged and observers are notified.
    /// </summary>
    /// <param name="exception">The exception to report.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task RaiseConnectionErrorEventAsync(Exception exception)
    {
        await this.NotifyConnectionErrorObserversAsync($"Unexpected error during receive of data: {exception.Message}", exception);
    }

    /// <summary>
    /// Reports a remote disconnect as the receive loop does, through the base class, so the connection is marked
    /// inactive before observers are notified.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task RaiseRemoteDisconnectedEventAsync()
    {
        await this.NotifyRemoteDisconnectedObserversAsync();
    }

    /// <summary>
    /// Gets or sets a signal completed on entry to <see cref="StartAsync"/>, immediately before
    /// <see cref="StartBarrier"/> is awaited. A test that needs to act while the transport is in the
    /// Connecting state waits on this, does its work, and then releases the barrier, rather than
    /// guessing when the connect attempt has reached the connection.
    /// </summary>
    public TaskCompletionSource? StartBarrierReached { get; set; }

    protected override void ResolveConnectionString(string connectionString)
    {
        // A bypassed start never opens a socket, so the URL it is given need not be one the real
        // connection could open; several tests pass a placeholder deliberately.
        if (this.BypassStart)
        {
            return;
        }

        base.ResolveConnectionString(connectionString);
    }

    protected override async Task StartConnectionAsync(CancellationToken cancellationToken)
    {
        this.startBypassed = this.BypassStart;
        this.StartBarrierReached?.TrySetResult();
        if (this.StartBarrier is not null)
        {
            await this.StartBarrier.Task.ConfigureAwait(false);
        }

        if (!this.BypassStart)
        {
            await base.StartConnectionAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    protected override async Task StopConnectionAsync(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref this.stopCallCount);

        if (this.BypassStop)
        {
            return;
        }
        else if (this.ThrowOnStop)
        {
            throw new WebDriverBiDiException("Simulated stop failure");
        }
        else
        {
            await base.StopConnectionAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Runs the real receive loop only for a connection that really connected. With
    /// <see cref="BypassStart"/> set there is no open socket for the loop to read from, and
    /// <see cref="Connection.StartAsync"/> starts the receive task on every successful start, so the
    /// loop would immediately fail against an unconnected socket and report a connection error.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected override Task ReceiveDataAsync()
    {
        if (this.startBypassed)
        {
            return Task.CompletedTask;
        }

        return base.ReceiveDataAsync();
    }

    public override Task SendDataAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        if (this.BypassStart)
        {
            // Bypass the check to see if the connection has been started,
            // so that we can test the plumbing without needing an actual
            // WebSocket server active.
            return this.SendConnectionDataAsync(data, cancellationToken);
        }

        return base.SendDataAsync(data, cancellationToken);
    }

    protected override async Task ConnectWebSocketAsync(Uri websocketUri, CancellationToken cancellationToken)
    {
        if (this.ConnectWebSocketOverride is not null)
        {
            await this.ConnectWebSocketOverride(websocketUri, cancellationToken).ConfigureAwait(false);
            return;
        }

        await base.ConnectWebSocketAsync(websocketUri, cancellationToken).ConfigureAwait(false);
    }

    protected override async Task SendConnectionDataAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        this.LastSendCancellationToken = cancellationToken;
        if (this.SendWebSocketDataOverride is not null)
        {
            await this.SendWebSocketDataOverride(data).ConfigureAwait(false);
            return;
        }

        await this.dataSendStartingInvocable.InvokeNotifyObserversAsync(new WebDriverBiDiEventArgs());
        this.DataSent = Encoding.UTF8.GetString(data.Span);

        if (this.SendBarrier is not null)
        {
            await this.SendBarrier.Task.ConfigureAwait(false);
        }

        if (!this.BypassDataSend)
        {
            await base.SendConnectionDataAsync(data, cancellationToken).ConfigureAwait(false);
        }

        await this.dataSendCompleteInvocable.InvokeNotifyObserversAsync(new TestWebSocketConnectionDataSentEventArgs(this.DataSent));
    }

    protected override async Task WriteWebSocketDataAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        if (this.ThrowWebSocketExceptionOnSend)
        {
            throw new WebSocketException("Simulated WebSocket failure");
        }

        await base.WriteWebSocketDataAsync(data, cancellationToken).ConfigureAwait(false);
    }

    protected override async Task<WebSocketReceiveResult> ReadWebSocketDataAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        int currentCall = Interlocked.Increment(ref this.receiveCallCount);
        if (this.ReceiveHandler is not null)
        {
            return await this.ReceiveHandler(buffer, cancellationToken, currentCall);
        }

        return await base.ReadWebSocketDataAsync(buffer, cancellationToken);
    }

    protected override async Task CloseClientWebSocketAsync(CancellationToken cancellationToken = default)
    {
        if (this.BypassCloseClientWebSocket)
        {
            return;
        }

        await base.CloseClientWebSocketAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets or sets a delegate applied to each <see cref="ClientWebSocket"/> the connection creates, so a
    /// test can configure a socket exactly as a derived connection overriding
    /// <see cref="WebSocketConnection.CreateClientWebSocket"/> would. Being assigned in an object
    /// initializer, it is also state that exists only once construction has finished.
    /// </summary>
    public Action<ClientWebSocket>? ConfigureClientWebSocket { get; set; }

    /// <summary>
    /// Gets the sockets the connection obtained from <see cref="WebSocketConnection.CreateClientWebSocket"/>,
    /// in the order it obtained them.
    /// </summary>
    public List<ClientWebSocket> CreatedClientWebSockets { get; } = [];

    protected override ClientWebSocket CreateClientWebSocket()
    {
        ClientWebSocket socket = base.CreateClientWebSocket();
        this.ConfigureClientWebSocket?.Invoke(socket);
        lock (this.CreatedClientWebSockets)
        {
            this.CreatedClientWebSockets.Add(socket);
        }

        return socket;
    }
}
