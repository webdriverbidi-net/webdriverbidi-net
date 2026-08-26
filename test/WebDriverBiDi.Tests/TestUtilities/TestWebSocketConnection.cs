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

    public bool BypassStart { get; set; } = true;

    public bool BypassStop { get; set; } = true;

    public bool BypassDataSend { get; set; } = true;

    public bool BypassCloseClientWebSocket { get; set; } = true;

    public bool ThrowOnStop { get; set; }

    public int StopCallCount => this.stopCallCount;

    public string? DataSent { get; set; }

    public TaskCompletionSource? SendBarrier { get; set; }

    public TimeSpan? StopDelay { get; set; }

    public TaskCompletionSource? StartBarrier { get; set; }

    public Func<ArraySegment<byte>, CancellationToken, int, Task<WebSocketReceiveResult>>? ReceiveHandler { get; set; }

    public Func<bool>? IsActiveOverride { get; set; }

    public Func<ReadOnlyMemory<byte>, Task>? SendWebSocketDataOverride { get; set; }

    /// <summary>
    /// Gets or sets a delegate that replaces the underlying WebSocket connect operation, for example
    /// to simulate a remote end that never completes the handshake.
    /// </summary>
    public Func<Uri, CancellationToken, Task>? ConnectWebSocketOverride { get; set; }

    public bool Disposed => this.IsDisposed;

    public override bool IsActive
    {
        get
        {
            if (this.IsActiveOverride is not null)
            {
                return this.IsActiveOverride();
            }

            if (this.ThrowOnStop)
            {
                return true;
            }

            return base.IsActive;
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
        await this.InvocableConnectionDataReceivedObservableEvent.InvokeNotifyObserversAsync(new ConnectionDataReceivedEventArgs(owner, length));
    }

    public async Task RaiseLogMessageEventAsync(string message, WebDriverBiDiLogLevel level)
    {
        await this.InvocableLogMessageObservableEvent.InvokeNotifyObserversAsync(new LogMessageEventArgs(message, level, "TestWebSocketConnection"));
    }

    public async Task RaiseConnectionErrorEventAsync(Exception exception)
    {
        await this.InvocableConnectionErrorObservableEvent.InvokeNotifyObserversAsync(new ConnectionErrorEventArgs(exception));
    }

    public async Task RaiseRemoteDisconnectedEventAsync()
    {
        await this.InvocableRemoteDisconnectedObservableEvent.InvokeNotifyObserversAsync(new ConnectionDisconnectedEventArgs());
    }

    public override async Task StartAsync(string url, CancellationToken cancellationToken = default)
    {
        this.ConnectionString = url;
        if (this.StartBarrier is not null)
        {
            await this.StartBarrier.Task.ConfigureAwait(false);
        }

        if (!this.BypassStart)
        {
            await base.StartAsync(url, cancellationToken).ConfigureAwait(false);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken = default)
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
            if (this.StopDelay.HasValue && this.StopDelay.Value > TimeSpan.Zero)
            {
                await Task.Delay(this.StopDelay.Value, cancellationToken).ConfigureAwait(false);
            }

            await base.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public override Task SendDataAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        if (this.BypassStart)
        {
            // Bypass the check to see if the connection has been started,
            // so that we can test the plumbing without needing an actual
            // WebSocket server active.
            return this.SendWebSocketDataAsync(data, cancellationToken);
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

    protected override async Task SendWebSocketDataAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
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
            await base.SendWebSocketDataAsync(data, cancellationToken).ConfigureAwait(false);
        }

        await this.dataSendCompleteInvocable.InvokeNotifyObserversAsync(new TestWebSocketConnectionDataSentEventArgs(this.DataSent));
    }

    protected override async Task<WebSocketReceiveResult> ReceiveWebSocketDataAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        int currentCall = Interlocked.Increment(ref this.receiveCallCount);
        if (this.ReceiveHandler is not null)
        {
            return await this.ReceiveHandler(buffer, cancellationToken, currentCall);
        }

        return await base.ReceiveWebSocketDataAsync(buffer, cancellationToken);
    }

    protected override async Task CloseClientWebSocketAsync(CancellationToken cancellationToken = default)
    {
        if (this.BypassCloseClientWebSocket)
        {
            return;
        }

        await base.CloseClientWebSocketAsync(cancellationToken).ConfigureAwait(false);
    }
}
