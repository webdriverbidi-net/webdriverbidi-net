namespace WebDriverBiDi.TestUtilities;

using System.IO;
using WebDriverBiDi.Protocol;

public class TestPipeConnection : PipeConnection
{
    private readonly ObservableEventInvocable<WebDriverBiDiEventArgs> dataSendStartingInvocable = new("connection.dataSendStaring");
    private int receiveCallCount;

    public TestPipeConnection(IPipeServerProcessProvider pipeServerProcessProvider)
        : base(pipeServerProcessProvider)
    {
    }

    public bool BypassDataSend { get; set; } = true;

    public bool ThrowOnStop { get; set; }

    public bool ThrowIOExceptionOnSend { get; set; }

    public bool ThrowObjectDisposedExceptionOnSend { get; set; }

    public bool ThrowIOExceptionOnReceive { get; set; }

    public bool ThrowObjectDisposedExceptionOnReceive { get; set; }

    public TaskCompletionSource? SendBarrier { get; set; }

    public Func<bool>? IsActiveOverride { get; set; }

    /// <summary>
    /// When set, <see cref="ReadPipeDataAsync"/> awaits this source instead of delegating
    /// to the real pipe, ignoring the cancellation token passed to it. This simulates a
    /// pipe read that does not unblock promptly when the connection's token is canceled,
    /// which is the scenario <see cref="PipeConnection.StopAsync"/>'s shutdown-timeout
    /// bound is meant to guard against.
    /// </summary>
    public TaskCompletionSource<int>? ReceiveBlockSignal { get; set; }

    /// <summary>
    /// When set, is completed the moment <see cref="ReadPipeDataAsync"/> enters the
    /// <see cref="ReceiveBlockSignal"/> path. The background receive loop starts on a
    /// separate task (see <c>PipeConnection.StartAsync</c>'s <c>Task.Run</c>), so without
    /// this signal a caller that calls <c>StopAsync</c> immediately after <c>StartAsync</c>
    /// races against that task ever reaching its first read: if cancellation is requested
    /// before the loop's first iteration, the loop's own
    /// <c>while (!connectionCancellationToken.IsCancellationRequested)</c> guard exits
    /// immediately and <see cref="ReceiveBlockSignal"/> is never touched, which defeats the
    /// point of blocking on it. Awaiting this signal before calling <c>StopAsync</c>
    /// eliminates that race.
    /// </summary>
    public TaskCompletionSource? ReceiveBlockEnteredSignal { get; set; }

    /// <summary>
    /// Gets or sets an exception that, when set, causes the background receive loop to fault
    /// immediately instead of running.
    /// </summary>
    /// <remarks>
    /// The real receive loop catches every exception it expects to see, so under normal
    /// operation <c>dataReceiveTask</c> never faults. This seam simulates an unrecoverable
    /// outer-loop fault so that the fault-observing continuation attached by
    /// <c>Connection.ObserveReceiveLoopFault</c> can be exercised.
    /// </remarks>
    public Exception? ReceiveLoopOuterFault { get; set; }

    /// <summary>
    /// Gets or sets a delegate that replaces the underlying pipe read. It receives the buffer,
    /// offset, count, and the one-based call number, and returns the number of bytes it wrote
    /// into the buffer (zero to signal that the pipe was closed).
    /// </summary>
    public Func<byte[], int, int, int, Task<int>>? ReadHandler { get; set; }

    public bool Disposed => this.IsDisposed;

    public async Task RaiseRemoteDisconnectedEventAsync()
    {
        await this.InvocableRemoteDisconnectedObservableEvent.InvokeNotifyObserversAsync(new ConnectionDisconnectedEventArgs());
    }

    public bool PipesDisposed
    {
        get => this.AreConnectionPipesDisposed;
        set => this.AreConnectionPipesDisposed = value;
    }

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

    public override Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (this.ThrowOnStop)
        {
            throw new WebDriverBiDiException("Simulated stop failure");
        }

        return base.StopAsync(cancellationToken);
    }

    protected override Task ReceiveDataAsync()
    {
        if (this.ReceiveLoopOuterFault is not null)
        {
            return Task.FromException(this.ReceiveLoopOuterFault);
        }

        return base.ReceiveDataAsync();
    }

    /// <summary>
    /// Gets the cancellation token the connection passed down to the send, so a test can assert which
    /// token was actually used rather than only that the send completed.
    /// </summary>
    public CancellationToken LastSendCancellationToken { get; private set; }

    /// <summary>
    /// Gets the connection's own cancellation token, which is protected on the base class.
    /// </summary>
    public CancellationToken ObservedConnectionCancellationToken => this.ConnectionCancellationToken;

    protected override async Task SendConnectionDataAsync(ReadOnlyMemory<byte> messageBuffer, CancellationToken cancellationToken = default)
    {
        this.LastSendCancellationToken = cancellationToken;
        await this.dataSendStartingInvocable.InvokeNotifyObserversAsync(new WebDriverBiDiEventArgs());

        if (this.SendBarrier is not null)
        {
            await this.SendBarrier.Task.ConfigureAwait(false);
        }

        if (!this.BypassDataSend)
        {
            await base.SendConnectionDataAsync(messageBuffer, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets each write the connection issued to the pipe, one entry per call, so that a test can assert
    /// how many operations a message was split into and what each one carried.
    /// </summary>
    public List<byte[]> RecordedPipeWrites { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether recorded writes are also passed to the real pipe. Tests
    /// that only inspect framing leave this false, so that no pipe peer is required.
    /// </summary>
    public bool BypassRealPipeWrite { get; set; }

    /// <summary>
    /// Calls the connection's own <c>WritePipeDataAsync</c>, which is protected, so that a test can
    /// exercise the framing directly rather than through a started connection.
    /// </summary>
    /// <param name="messageBuffer">The message to frame and write.</param>
    /// <param name="cancellationToken">The token to pass to the write.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task WriteFramedMessageAsync(ReadOnlyMemory<byte> messageBuffer, CancellationToken cancellationToken = default)
    {
        await this.WritePipeDataAsync(messageBuffer, cancellationToken);
    }

    protected override async Task WriteToPipeAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        // Record before writing, so that a write that throws is still counted; a partial frame reaching
        // the pipe is exactly what the framing test needs to be able to see.
        byte[] written = new byte[count];
        Array.Copy(buffer, offset, written, 0, count);
        lock (this.RecordedPipeWrites)
        {
            this.RecordedPipeWrites.Add(written);
        }

        if (this.BypassRealPipeWrite)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return;
        }

        await base.WriteToPipeAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
    }

    protected override async Task WritePipeDataAsync(ReadOnlyMemory<byte> messageBuffer, CancellationToken cancellationToken = default)
    {
        if (this.ThrowIOExceptionOnSend)
        {
            throw new IOException("Simulated pipe write failure");
        }

        if (this.ThrowObjectDisposedExceptionOnSend)
        {
            throw new ObjectDisposedException("Simulated pipe disposed");
        }

        await base.WritePipeDataAsync(messageBuffer, cancellationToken).ConfigureAwait(false);
    }

    protected override Task<int> ReadPipeDataAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        if (this.ReceiveBlockSignal is not null)
        {
            this.ReceiveBlockEnteredSignal?.TrySetResult();
            return this.ReceiveBlockSignal.Task;
        }

        int currentCall = Interlocked.Increment(ref this.receiveCallCount);
        if (this.ReadHandler is not null)
        {
            return this.ReadHandler(buffer, offset, count, currentCall);
        }

        // For exception tests, return fake data on first call to allow second call to happen
        if (currentCall == 1 && (this.ThrowIOExceptionOnReceive || this.ThrowObjectDisposedExceptionOnReceive))
        {
            byte[] fakeData = System.Text.Encoding.UTF8.GetBytes("test\0");
            Array.Copy(fakeData, 0, buffer, offset, fakeData.Length);
            return Task.FromResult(fakeData.Length);
        }

        // Throw on second call
        if (currentCall > 1)
        {
            if (this.ThrowIOExceptionOnReceive)
            {
                throw new IOException("Simulated pipe read failure");
            }

            if (this.ThrowObjectDisposedExceptionOnReceive)
            {
                throw new ObjectDisposedException("Simulated pipe disposed during read");
            }
        }

        return base.ReadPipeDataAsync(buffer, offset, count, cancellationToken);
    }
}
