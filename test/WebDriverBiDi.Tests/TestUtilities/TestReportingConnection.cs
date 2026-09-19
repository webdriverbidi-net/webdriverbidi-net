namespace WebDriverBiDi.TestUtilities;

using System.Buffers;
using WebDriverBiDi.Protocol;

/// <summary>
/// A minimal custom <see cref="Connection"/>, of the kind a consumer of the library writes, whose receive loop ends
/// only when a test tells it to, and which reports that end through the base class.
/// </summary>
/// <remarks>
/// Its channel stays open after the receive loop ends, until the connection is stopped. That is the situation
/// <see cref="Connection.IsActive"/> must report correctly without any help from the implementation: an open
/// connection that nothing reads.
/// </remarks>
public sealed class TestReportingConnection : Connection
{
    private TaskCompletionSource<Exception?> loopExitSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int isOpenFlag;
    private int startConnectionCallCount;
    private int stopConnectionCallCount;
    private int disposeCoreCallCount;

    public override ConnectionKind ConnectionKind => ConnectionKind.WebSocket;

    /// <summary>
    /// Gets a value indicating whether the connection's channel is open, which is what the connection reports as
    /// <see cref="IsConnectionOpen"/>.
    /// </summary>
    public bool IsOpen => Interlocked.CompareExchange(ref this.isOpenFlag, 0, 0) == 1;

    public int StartConnectionCallCount => Interlocked.CompareExchange(ref this.startConnectionCallCount, 0, 0);

    public int StopConnectionCallCount => Interlocked.CompareExchange(ref this.stopConnectionCallCount, 0, 0);

    public int DisposeCoreCallCount => Interlocked.CompareExchange(ref this.disposeCoreCallCount, 0, 0);

    /// <summary>
    /// Gets the task the current session's receive loop runs on, or <see langword="null"/> before the first start.
    /// </summary>
    public Task? ReceiveLoopTask => this.DataReceiveTask;

    protected override bool IsConnectionOpen => this.IsOpen;

    /// <summary>
    /// Ends the current session's receive loop by reporting that the remote end closed the connection.
    /// </summary>
    public void EndReceiveLoopWithRemoteDisconnect()
    {
        Interlocked.CompareExchange(ref this.loopExitSignal, null!, null!).TrySetResult(null);
    }

    /// <summary>
    /// Ends the current session's receive loop by reporting a connection error.
    /// </summary>
    /// <param name="exception">The error the loop reports.</param>
    public void EndReceiveLoopWithConnectionError(Exception exception)
    {
        Interlocked.CompareExchange(ref this.loopExitSignal, null!, null!).TrySetResult(exception);
    }

    /// <summary>
    /// Delivers a message held in pooled memory, as a receive loop that already holds a complete message would.
    /// </summary>
    /// <param name="messageOwner">The owner of the memory holding the message.</param>
    /// <param name="messageLength">The length of the message within that memory.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public Task DeliverMessageAsync(IMemoryOwner<byte> messageOwner, int messageLength)
    {
        return this.NotifyDataReceivedObserverAsync(messageOwner, messageLength);
    }

    /// <summary>
    /// Delivers a message accumulated in a <see cref="MessageBuffer"/>, as a receive loop that assembles messages
    /// piece by piece would.
    /// </summary>
    /// <param name="messageBuffer">The buffer holding the accumulated message.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public Task DeliverMessageBufferAsync(MessageBuffer messageBuffer)
    {
        return this.NotifyDataReceivedObserverAsync(messageBuffer);
    }

    protected override Task StartConnectionAsync(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref this.startConnectionCallCount);
        Interlocked.Exchange(ref this.loopExitSignal, new TaskCompletionSource<Exception?>(TaskCreationOptions.RunContinuationsAsynchronously));
        Interlocked.Exchange(ref this.isOpenFlag, 1);
        return Task.CompletedTask;
    }

    protected override Task StopConnectionAsync(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref this.stopConnectionCallCount);
        Interlocked.Exchange(ref this.isOpenFlag, 0);
        return Task.CompletedTask;
    }

    protected override Task SendConnectionDataAsync(ReadOnlyMemory<byte> messageBuffer, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    protected override async Task ReceiveDataAsync()
    {
        CancellationToken connectionCancellationToken = this.ConnectionCancellationToken;
        Task<Exception?> exitTask = Interlocked.CompareExchange(ref this.loopExitSignal, null!, null!).Task;
        Task cancellationTask = Task.Delay(Timeout.InfiniteTimeSpan, connectionCancellationToken);
        if (await Task.WhenAny(exitTask, cancellationTask).ConfigureAwait(false) != exitTask)
        {
            // Stopped locally, which ends the loop without reporting anything.
            return;
        }

        Exception? connectionError = await exitTask.ConfigureAwait(false);
        if (connectionError is null)
        {
            await this.NotifyRemoteDisconnectedObserversAsync().ConfigureAwait(false);
        }
        else
        {
            await this.NotifyConnectionErrorObserversAsync($"Unexpected error during receive of data: {connectionError.Message}", connectionError).ConfigureAwait(false);
        }
    }

    protected override ValueTask DisposeAsyncCore()
    {
        Interlocked.Increment(ref this.disposeCoreCallCount);
        return default;
    }
}
