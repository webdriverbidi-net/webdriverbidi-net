namespace WebDriverBiDi.TestUtilities;

using System.IO;
using WebDriverBiDi.Protocol;

/// <summary>
/// A <see cref="WebSocketConnection"/> double with a real, test-driven receive loop.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <see cref="TestWebSocketConnection"/>, which raises connection events from the
/// test thread and bypasses the transport-specific parts of starting and stopping, this double
/// runs a receive loop on the connection's own receive task, started by
/// <see cref="Connection.StartAsync"/>, and raises <see cref="Connection.OnRemoteDisconnected"/> or
/// <see cref="Connection.OnConnectionError"/> <em>from inside that loop</em>, exactly as the
/// production receive loop does. Its shutdown reaches the same place a real
/// <see cref="WebSocketConnection"/> does after a remote close: there is no handshake left to
/// perform, so <see cref="Connection.StopAsync"/> cancels the connection and waits for the loop.
/// </para>
/// <para>
/// This models the scenario where the remote end closes the connection (or a read fails)
/// while <see cref="Transport.DisconnectAsync(CancellationToken)"/> is in progress, which is
/// only observable when the connection's shutdown genuinely waits for the receive loop.
/// </para>
/// </remarks>
public class TestReceiveLoopWebSocketConnection : WebSocketConnection
{
    private TaskCompletionSource<ReceiveLoopExit> loopExitSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int isActiveFlag;
    private int stopCallCount;

    /// <summary>
    /// The way the simulated receive loop ends.
    /// </summary>
    public enum ReceiveLoopExit
    {
        /// <summary>The remote end closed the connection; the loop raises OnRemoteDisconnected.</summary>
        RemoteClose,

        /// <summary>A read failed; the loop raises OnConnectionError.</summary>
        ConnectionError,
    }

    public override bool IsActive => Interlocked.CompareExchange(ref this.isActiveFlag, 0, 0) == 1;

    public int StopCallCount => Interlocked.CompareExchange(ref this.stopCallCount, 0, 0);

    public bool ReceiveLoopCompleted => this.DataReceiveTask?.IsCompleted ?? false;

    /// <summary>
    /// Gets a human-readable description of the receive task's status, for deadlock diagnostics.
    /// </summary>
    public string DataReceiveTaskStatusDescription => this.DataReceiveTask?.Status.ToString() ?? "not started";

    /// <summary>
    /// Ends the receive loop as if the remote end had closed the connection. The loop raises
    /// <see cref="Connection.OnRemoteDisconnected"/> on the receive task.
    /// </summary>
    public void SignalRemoteClose()
    {
        this.loopExitSignal.TrySetResult(ReceiveLoopExit.RemoteClose);
    }

    /// <summary>
    /// Ends the receive loop as if a read had failed. The loop raises
    /// <see cref="Connection.OnConnectionError"/> on the receive task.
    /// </summary>
    public void SignalConnectionError()
    {
        this.loopExitSignal.TrySetResult(ReceiveLoopExit.ConnectionError);
    }

    protected override Task StartConnectionAsync(CancellationToken cancellationToken)
    {
        this.loopExitSignal = new TaskCompletionSource<ReceiveLoopExit>(TaskCreationOptions.RunContinuationsAsynchronously);
        Interlocked.Exchange(ref this.isActiveFlag, 1);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Mirrors <see cref="WebSocketConnection"/> after the socket has already been closed by the remote
    /// end: there is no close handshake to perform, so the connection is simply marked inactive, and
    /// <see cref="Connection.StopAsync"/> then cancels the connection and waits for the receive loop.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected override Task StopConnectionAsync(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref this.stopCallCount);
        Interlocked.Exchange(ref this.isActiveFlag, 0);
        return Task.CompletedTask;
    }

    public override Task SendDataAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    protected override async Task ReceiveDataAsync()
    {
        CancellationToken connectionCancellationToken = this.ConnectionCancellationToken;
        Task<ReceiveLoopExit> exitTask = this.loopExitSignal.Task;
        Task cancellationTask = Task.Delay(Timeout.InfiniteTimeSpan, connectionCancellationToken);
        Task completedTask = await Task.WhenAny(exitTask, cancellationTask).ConfigureAwait(false);
        if (completedTask != exitTask || connectionCancellationToken.IsCancellationRequested)
        {
            // Stopped locally; the production loop does not raise remote-disconnect in this case.
            return;
        }

        // The socket is gone from this point on, as it would be after a Close frame or a failed read.
        Interlocked.Exchange(ref this.isActiveFlag, 0);
        if (exitTask.Result == ReceiveLoopExit.RemoteClose)
        {
            await this.InvocableRemoteDisconnectedObservableEvent.InvokeNotifyObserversAsync(new ConnectionDisconnectedEventArgs()).ConfigureAwait(false);
        }
        else
        {
            await this.InvocableConnectionErrorObservableEvent.InvokeNotifyObserversAsync(new ConnectionErrorEventArgs(new IOException("Simulated read failure"))).ConfigureAwait(false);
        }
    }
}
