namespace WebDriverBiDi.Protocol;

using System.Text;
using WebDriverBiDi.TestUtilities;

/// <summary>
/// Tests of what every <see cref="Connection"/> gets from the base class, exercised through a minimal custom
/// connection rather than one of the connections the library ships, so that no transport-specific behavior can
/// supply what the base class is meant to guarantee.
/// </summary>
public class ConnectionTests
{
    private const string ConnectionString = "custom://remote";

    [Fact]
    public async Task TestConnectionErrorMakesOpenConnectionInactiveBeforeObserversAreNotified()
    {
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;
        await using TestReportingConnection connection = new();
        bool? isActiveWhenNotified = null;
        TaskCompletionSource notified = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.OnConnectionError.AddObserver(e =>
        {
            isActiveWhenNotified = connection.IsActive;
            notified.TrySetResult();
        });

        await connection.StartAsync(ConnectionString, testCancellationToken);
        Assert.True(connection.IsActive);

        connection.EndReceiveLoopWithConnectionError(new IOException("Simulated read failure"));
        await notified.Task.WaitAsync(TimeSpan.FromSeconds(5), testCancellationToken);

        // The value captured inside the observer pins the ordering: the connection was already inactive when the
        // observer ran. The channel is still open, so the inactivity comes from the receive loop alone.
        Assert.False(isActiveWhenNotified);
        Assert.False(connection.IsActive);
        Assert.True(connection.IsOpen);
    }

    [Fact]
    public async Task TestRemoteDisconnectMakesOpenConnectionInactiveBeforeObserversAreNotified()
    {
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;
        await using TestReportingConnection connection = new();
        bool? isActiveWhenNotified = null;
        TaskCompletionSource notified = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.OnRemoteDisconnected.AddObserver(e =>
        {
            isActiveWhenNotified = connection.IsActive;
            notified.TrySetResult();
        });

        await connection.StartAsync(ConnectionString, testCancellationToken);
        Assert.True(connection.IsActive);

        connection.EndReceiveLoopWithRemoteDisconnect();
        await notified.Task.WaitAsync(TimeSpan.FromSeconds(5), testCancellationToken);

        Assert.False(isActiveWhenNotified);
        Assert.False(connection.IsActive);
        Assert.True(connection.IsOpen);
    }

    [Fact]
    public async Task TestSendOnOpenConnectionWhoseReceiveLoopEndedFailsImmediately()
    {
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;
        await using TestReportingConnection connection = new();
        TaskCompletionSource notified = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.OnConnectionError.AddObserver(e =>
        {
            notified.TrySetResult();
        });

        await connection.StartAsync(ConnectionString, testCancellationToken);
        connection.EndReceiveLoopWithConnectionError(new IOException("Simulated read failure"));
        await notified.Task.WaitAsync(TimeSpan.FromSeconds(5), testCancellationToken);

        // A send on a connection nothing reads could never be answered, so it is refused rather than written.
        WebDriverBiDiConnectionException exception = await Assert.ThrowsAsync<WebDriverBiDiConnectionException>(async () => await connection.SendDataAsync("data"u8.ToArray(), testCancellationToken));
        Assert.Contains("its receive loop has ended", exception.Message);
    }

    [Fact]
    public async Task TestStartAfterReceiveLoopEndedStartsConnectionThatIsStillOpen()
    {
        // The connection is inactive but its channel is still open. Starting it must not be refused as starting a
        // connection that is already connected, and the new session must be judged by its own receive loop.
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;
        await using TestReportingConnection connection = new();
        TaskCompletionSource notified = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.OnConnectionError.AddObserver(e =>
        {
            notified.TrySetResult();
        });

        await connection.StartAsync(ConnectionString, testCancellationToken);
        connection.EndReceiveLoopWithConnectionError(new IOException("Simulated read failure"));
        await notified.Task.WaitAsync(TimeSpan.FromSeconds(5), testCancellationToken);
        Assert.False(connection.IsActive);

        await connection.StartAsync(ConnectionString, testCancellationToken);

        Assert.Equal(2, connection.StartConnectionCallCount);
        Assert.True(connection.IsActive);
    }

    [Fact]
    public async Task TestThrowingLogObserverDoesNotDisruptTheConnectionLifecycle()
    {
        // A connection used without a transport reports a failing observer through its default reporter, which
        // records it rather than throwing it. A log observer that throws on every message therefore cannot keep
        // StartAsync from completing, StopAsync from canceling the connection and waiting for its receive loop,
        // or DisposeAsync from releasing the connection's resources.
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;
        TestReportingConnection connection = new();
        connection.OnLogMessage.AddObserver(e => throw new InvalidOperationException("Simulated log observer failure"));

        await connection.StartAsync(ConnectionString, testCancellationToken);
        Assert.True(connection.IsActive);
        Task receiveLoopTask = connection.ReceiveLoopTask!;

        await connection.StopAsync(testCancellationToken);
        Assert.Equal(1, connection.StopConnectionCallCount);
        Assert.True(receiveLoopTask.IsCompleted);
        Assert.False(connection.IsActive);

        await connection.StartAsync(ConnectionString, testCancellationToken);
        Assert.True(connection.IsActive);

        await connection.DisposeAsync();
        Assert.Equal(2, connection.StopConnectionCallCount);
        Assert.Equal(1, connection.DisposeCoreCallCount);
    }

    [Fact]
    public async Task TestConnectionErrorIsReportedWhenLoggingTheErrorFails()
    {
        // A log observer that throws on the error message is reported through the connection's observer-error
        // reporter rather than thrown, so the error observers are notified, and the receive loop ends normally.
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;
        await using TestReportingConnection connection = new();
        connection.OnLogMessage.AddObserver(e =>
        {
            if (e.Level == WebDriverBiDiLogLevel.Error)
            {
                throw new InvalidOperationException("Simulated log observer failure");
            }
        });

        TaskCompletionSource notified = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.OnConnectionError.AddObserver(e =>
        {
            notified.TrySetResult();
        });

        await connection.StartAsync(ConnectionString, testCancellationToken);
        Task receiveLoopTask = connection.ReceiveLoopTask!;
        connection.EndReceiveLoopWithConnectionError(new IOException("Simulated read failure"));

        await notified.Task.WaitAsync(TimeSpan.FromSeconds(5), testCancellationToken);
        await receiveLoopTask.WaitAsync(TimeSpan.FromSeconds(5), testCancellationToken);
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestDisposeStopsConnectionWhoseReceiveLoopEndedWhileItIsStillOpen()
    {
        // Disposal stops a connection whose channel is open, even though it is inactive, so the channel is shut
        // down before the connection's resources are released.
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;
        TestReportingConnection connection = new();
        TaskCompletionSource notified = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.OnConnectionError.AddObserver(e =>
        {
            notified.TrySetResult();
        });

        await connection.StartAsync(ConnectionString, testCancellationToken);
        connection.EndReceiveLoopWithConnectionError(new IOException("Simulated read failure"));
        await notified.Task.WaitAsync(TimeSpan.FromSeconds(5), testCancellationToken);

        await connection.DisposeAsync();

        Assert.Equal(1, connection.StopConnectionCallCount);
        Assert.False(connection.IsOpen);
    }

    [Fact]
    public async Task TestStopAfterDisposeDoesNothing()
    {
        // The base class disposes its cancellation source with the connection, and the stop sequence cancels
        // that source unconditionally; the public StopAsync must therefore refuse to run the sequence on a
        // disposed connection rather than let the cancel throw ObjectDisposedException.
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;
        TestReportingConnection connection = new();
        List<string> logMessages = [];
        connection.OnLogMessage.AddObserver(e => logMessages.Add(e.Message));
        await connection.StartAsync(ConnectionString, testCancellationToken);
        await connection.DisposeAsync();
        int stopCallsByDisposal = connection.StopConnectionCallCount;
        logMessages.Clear();

        await connection.StopAsync(testCancellationToken);

        // The transport-specific stop ran once, for the disposal, and not again for the stop.
        Assert.Equal(1, stopCallsByDisposal);
        Assert.Equal(1, connection.StopConnectionCallCount);
        Assert.Empty(logMessages);
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestStopOfNeverStartedDisposedConnectionDoesNothing()
    {
        // A connection disposed without ever being started was never stopped by the disposal either, so the
        // guard, not the disposal's own stop, is what keeps this from throwing.
        TestReportingConnection connection = new();
        List<string> logMessages = [];
        connection.OnLogMessage.AddObserver(e => logMessages.Add(e.Message));
        await connection.DisposeAsync();

        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(0, connection.StopConnectionCallCount);
        Assert.Empty(logMessages);
    }

    [Fact]
    public async Task TestDeliveringPooledMemoryPassesOwnershipToTheObserverAndLogsTheMessage()
    {
        await using TestReportingConnection connection = new()
        {
            LogLevel = WebDriverBiDiLogLevel.Trace,
        };
        List<string> logMessages = [];
        connection.OnLogMessage.AddObserver(e => logMessages.Add(e.Message));
        ConnectionDataReceivedEventArgs? delivered = null;
        connection.OnDataReceived.AddObserver(e => delivered = e);

        using TrackingMemoryOwner owner = new(Encoding.UTF8.GetBytes("Hello"));
        await connection.DeliverMessageAsync(owner, owner.Length);

        // The observer received the very memory the connection was handed, so no copy was made, and it now owns it.
        Assert.NotNull(delivered);
        Assert.Same(owner, delivered.BufferOwner);
        Assert.Equal(owner.Length, delivered.DataLength);
        Assert.False(owner.IsDisposed);
        Assert.Equal(["RECV <<< Hello"], logMessages);
    }

    [Fact]
    public async Task TestDeliveringPooledMemoryWithNoObserverReturnsTheMemoryWithoutLogging()
    {
        await using TestReportingConnection connection = new()
        {
            LogLevel = WebDriverBiDiLogLevel.Trace,
        };
        List<string> logMessages = [];
        connection.OnLogMessage.AddObserver(e => logMessages.Add(e.Message));

        TrackingMemoryOwner owner = new(Encoding.UTF8.GetBytes("Hello"));
        await connection.DeliverMessageAsync(owner, owner.Length);

        Assert.True(owner.IsDisposed);
        Assert.Empty(logMessages);
    }

    [Fact]
    public async Task TestDeliveringAMessageSucceedsWhenLoggingTheMessageFails()
    {
        // The message is logged before it is delivered. A log observer that throws is reported rather than
        // thrown, so the message is still delivered.
        await using TestReportingConnection connection = new()
        {
            LogLevel = WebDriverBiDiLogLevel.Trace,
        };
        connection.OnLogMessage.AddObserver(e => throw new InvalidOperationException("Simulated log observer failure"));
        int deliveredCount = 0;
        connection.OnDataReceived.AddObserver(e => Interlocked.Increment(ref deliveredCount));

        TrackingMemoryOwner owner = new(Encoding.UTF8.GetBytes("Hello"));
        await connection.DeliverMessageAsync(owner, owner.Length);

        Assert.Equal(1, deliveredCount);
    }

    [Fact]
    public async Task TestDeliveringNullPooledMemoryThrows()
    {
        await using TestReportingConnection connection = new();

        ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await connection.DeliverMessageAsync(null!, 0));
        Assert.Equal("messageOwner", exception.ParamName);
    }

    [Fact]
    public async Task TestDeliveringNullMessageBufferThrows()
    {
        await using TestReportingConnection connection = new();

        ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await connection.DeliverMessageBufferAsync(null!));
        Assert.Equal("messageBuffer", exception.ParamName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TestDeliveringPooledMemoryWithLengthOutsideTheMemoryThrowsAndReturnsTheMemory(bool isLengthPastTheEnd)
    {
        await using TestReportingConnection connection = new();
        int deliveredCount = 0;
        connection.OnDataReceived.AddObserver(e => Interlocked.Increment(ref deliveredCount));

        TrackingMemoryOwner owner = new(Encoding.UTF8.GetBytes("Hello"));
        int messageLength = isLengthPastTheEnd ? owner.Memory.Length + 1 : -1;
        ArgumentOutOfRangeException exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await connection.DeliverMessageAsync(owner, messageLength));

        Assert.Equal("messageLength", exception.ParamName);
        Assert.True(owner.IsDisposed);
        Assert.Equal(0, deliveredCount);
    }
}
