namespace WebDriverBiDi.Protocol;

using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using PinchHitter;
using WebDriverBiDi.TestUtilities;

public class WebSocketConnectionTests : IAsyncDisposable
{
    private string lastServerReceivedData = string.Empty;
    private ReadOnlyMemory<byte> lastConnectionReceivedData = ReadOnlyMemory<byte>.Empty;
    private string connectionId = string.Empty;
    private readonly AutoResetEvent serverReceiveSyncEvent = new(false);
    private readonly AutoResetEvent connectionReceiveSyncEvent = new(false);
    private readonly AutoResetEvent connectionSyncEvent = new(false);
    private ServerEventObserver<ClientConnectionEventArgs>? clientConnectedObserver;
    private ServerEventObserver<ClientConnectionEventArgs>? clientDisconnectedObserver;
    private ServerEventObserver<ServerDataReceivedEventArgs>? serverDataReceivedObserver;
    public WebSocketConnectionTests()
    {
        this.connectionId = string.Empty;
        this.lastServerReceivedData = string.Empty;
        this.lastConnectionReceivedData = ReadOnlyMemory<byte>.Empty;
        this.connectionReceiveSyncEvent.Reset();
        this.serverReceiveSyncEvent.Reset();
        this.connectionSyncEvent.Reset();
    }
    public async ValueTask DisposeAsync()
    {
        this.serverDataReceivedObserver?.Unobserve();
        this.serverDataReceivedObserver = null;

        this.clientConnectedObserver?.Unobserve();
        this.clientConnectedObserver = null;

        this.clientDisconnectedObserver?.Unobserve();
        this.clientDisconnectedObserver = null;
    }

    [Fact]
    public async Task TestConnectionType()
    {
        await using WebSocketConnection connection = new();
        Assert.Equal(ConnectionKind.WebSocket, connection.ConnectionKind);
    }

    [Fact]
    public async Task TestConnectionFailure()
    {
        // Find an available port by briefly binding to port 0, then release it
        // before creating the Server so the port number is known in advance.
        // This is a slight race condition in theory, but in the context of
        // running tests in a controlled environment, it's unlikely to cause
        // issues and allows deterministic testing of starting a Server on a
        // specific port.
        int port;
        using (TcpListener portFinder = new(IPAddress.Loopback, 0))
        {
            portFinder.Start();
            port = ((IPEndPoint)portFinder.LocalEndpoint).Port;
            portFinder.Stop();
        }

        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new(timeProvider)
        {
            BypassStart = false,
            StartupTimeout = TimeSpan.FromMilliseconds(50),
        };

        // The startup budget is elapsed on the virtual clock as soon as the attempt arms it, whether
        // the refused socket fails the attempt first or the deadline cancels it.
        Task startTask = connection.StartAsync($"ws://127.0.0.1:{port}", TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(startTask, connection.StartupTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        Assert.Contains($"{0.05} seconds", (await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await startTask)).Message);
    }

    [Fact]
    public async Task TestConnectionStartupTimeoutBoundsHangingConnectAttempt()
    {
        // ClientWebSocket has no connect timeout of its own. A remote end that accepts the TCP
        // connection but never completes the handshake (or a black-holed host) must not hold
        // StartAsync open past StartupTimeout, so each attempt is bounded by the remaining budget.
        TimeSpan startupTimeout = TimeSpan.FromMilliseconds(200);
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new(timeProvider)
        {
            BypassStart = false,
            StartupTimeout = startupTimeout,
            ConnectWebSocketOverride = (uri, token) => Task.Delay(Timeout.InfiniteTimeSpan, token),
        };

        // The attempt can only end because its deadline, armed against the virtual clock, elapsed:
        // no real time passes, and an attempt that was not bounded would hang here until the
        // five-second guard inside AdvanceUntilCompletedAsync fired.
        Task startTask = connection.StartAsync("ws://127.0.0.1:1", TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(startTask, startupTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await startTask);

        Assert.Contains($"{0.2} seconds", exception.Message);
        Assert.False(connection.IsActive);

        // A connect that failed leaves nothing behind. StartAsync publishes the connection string
        // before the connect, so that the connect can name the remote end it is reaching for, and
        // takes it back when the attempt does not succeed; a connection that never connected must not
        // report itself as connected to anything.
        Assert.Equal(string.Empty, connection.ConnectionString);
    }

    [Fact]
    public void TestStartupTimeoutRejectsNegativeValue()
    {
        WebSocketConnection connection = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => connection.StartupTimeout = TimeSpan.FromMilliseconds(-5));
    }

    [Fact]
    public void TestStartupTimeoutRejectsInfiniteTimeSpan()
    {
        // The startup budget is computed by subtracting elapsed time, so an infinite (negative)
        // value would read as an already-expired budget rather than "wait forever"; it is rejected.
        WebSocketConnection connection = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => connection.StartupTimeout = Timeout.InfiniteTimeSpan);
    }

    [Fact]
    public void TestStartupTimeoutRejectsValueExceedingMaximum()
    {
        WebSocketConnection connection = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => connection.StartupTimeout = TimeSpan.FromDays(60));
    }

    [Fact]
    public void TestShutdownTimeoutRejectsNegativeValue()
    {
        WebSocketConnection connection = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => connection.ShutdownTimeout = TimeSpan.FromMilliseconds(-5));
    }

    [Fact]
    public void TestShutdownTimeoutAllowsInfiniteTimeSpan()
    {
        // Shutdown waits are bounded by Task.Delay/CancellationTokenSource, both of which treat
        // Timeout.InfiniteTimeSpan as "no timeout", so an infinite value is a valid "wait
        // indefinitely for a clean shutdown" setting.
        WebSocketConnection connection = new()
        {
            ShutdownTimeout = Timeout.InfiniteTimeSpan,
        };
        Assert.Equal(Timeout.InfiniteTimeSpan, connection.ShutdownTimeout);
    }

    [Fact]
    public void TestDataTimeoutRejectsNegativeValue()
    {
        WebSocketConnection connection = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => connection.DataTimeout = TimeSpan.FromMilliseconds(-5));
    }

    [Fact]
    public async Task TestConnectionSkipsRetryPauseWhenFailedAttemptConsumesStartupBudget()
    {
        // The pause between connection attempts is charged against StartupTimeout rather than
        // added to it. When an attempt fails only after the whole budget has been spent, there is
        // nothing left to charge the pause to, so StartAsync must give up immediately instead of
        // sleeping for the retry interval first.
        TimeSpan startupTimeout = TimeSpan.FromMilliseconds(100);
        TimeSpan attemptDuration = TimeSpan.FromMilliseconds(150);
        int attemptCount = 0;
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new(timeProvider)
        {
            BypassStart = false,
            StartupTimeout = startupTimeout,
            ConnectWebSocketOverride = (uri, token) =>
            {
                Interlocked.Increment(ref attemptCount);

                // The attempt spends more than the whole budget on the virtual clock and deliberately
                // ignores its own cancellation token (which that advance fires), so it outlives the
                // startup budget and then fails the way a remote end that is not listening does,
                // rather than being canceled by its deadline.
                timeProvider.Advance(attemptDuration);
                return Task.FromException(new WebSocketException("Simulated connection failure after the startup budget elapsed"));
            },
        };

        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(
            async () => await connection.StartAsync("ws://127.0.0.1:1", TestContext.Current.CancellationToken));

        Assert.Contains($"{0.1} seconds", exception.Message);
        Assert.Equal(1, attemptCount);

        // Assert the decision, not its duration: no pause was attempted at all. The attempt's deadline is
        // the only timer the connection armed; a pause would have armed a second. Measuring elapsed time
        // instead would only ever be evidence about the machine the test ran on.
        Assert.Equal(1, timeProvider.TimerCount);
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestConnectionWithZeroStartupTimeoutTimesOutBeforeAttemptingToConnect()
    {
        // A zero startup budget leaves no time for even a first attempt.
        int attemptCount = 0;
        await using TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            StartupTimeout = TimeSpan.Zero,
            ConnectWebSocketOverride = (uri, token) =>
            {
                Interlocked.Increment(ref attemptCount);
                return Task.CompletedTask;
            },
        };

        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(
            async () => await connection.StartAsync("ws://127.0.0.1:1", TestContext.Current.CancellationToken));

        Assert.Contains("within 0 seconds", exception.Message);
        Assert.Equal(0, attemptCount);
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestConnectionCallerCancellationDuringHangingConnectAttemptPropagates()
    {
        // Cancellation requested by the caller while a connect attempt is in flight must
        // surface as OperationCanceledException, not be misreported as a startup timeout.
        using CancellationTokenSource cancellationTokenSource = new();
        TaskCompletionSource connectStartedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            StartupTimeout = TimeSpan.FromSeconds(30),
            ConnectWebSocketOverride = (uri, token) =>
            {
                connectStartedTaskCompletionSource.TrySetResult();
                return Task.Delay(Timeout.InfiniteTimeSpan, token);
            },
        };

        Task startTask = connection.StartAsync("ws://127.0.0.1:1", cancellationTokenSource.Token);
        await connectStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await startTask.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestConnectionStopDuringHangingConnectAttemptPropagatesCancellation()
    {
        // Stopping the connection while a connect attempt is in flight cancels the
        // connection token; that cancellation must propagate rather than being treated
        // as a startup timeout.
        TaskCompletionSource connectStartedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
            StartupTimeout = TimeSpan.FromSeconds(30),
            ConnectWebSocketOverride = (uri, token) =>
            {
                connectStartedTaskCompletionSource.TrySetResult();
                return Task.Delay(Timeout.InfiniteTimeSpan, token);
            },
        };

        Task startTask = connection.StartAsync("ws://127.0.0.1:1", TestContext.Current.CancellationToken);
        await connectStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await startTask.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestConnectionCanSendData()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        await using WebSocketConnection connection = new();
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.serverDataReceivedObserver = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("Hello world"u8.ToArray(), TestContext.Current.CancellationToken);
        string dataReceivedByServer = this.WaitForServerToReceiveData(TimeSpan.FromSeconds(3));

        Assert.Equal("Hello world", dataReceivedByServer);
        await connection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestConnectionCanReceiveData()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        await using WebSocketConnection connection = new();
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);

        await server.SendWebSocketDataAsync(registeredConnectionId, "Hello back");
        byte[] dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));

        Assert.Equal("Hello back"u8.ToArray(), dataReceivedByConnection);
        await connection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestConnectionReceivesDataOnBufferBoundary()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        await using WebSocketConnection connection = new();
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);

        // Create a message on an exact boundary of the buffer
        string data = new('a', 2 * connection.BufferSize);
        await server.SendWebSocketDataAsync(registeredConnectionId, data);
        byte[] dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));

        Assert.Equal(Encoding.UTF8.GetBytes(data), dataReceivedByConnection);
        await connection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestConnectionReceivesDataOnVeryLongMessage()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        await using WebSocketConnection connection = new();
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);

        // Create a message on an exact boundary of the buffer
        string data = new('a', 70000);
        await server.SendWebSocketDataAsync(registeredConnectionId, data);
        byte[] dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));

        Assert.Equal(Encoding.UTF8.GetBytes(data), dataReceivedByConnection);
        await connection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestConnectionLogIncludesSendAndRecvDebugMessages()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<LogMessageEventArgs> allLogs = [];
        await using WebSocketConnection connection = new();
        // This test asserts on Debug or Trace messages, which the default minimum level excludes.
        connection.LogLevel = WebDriverBiDiLogLevel.Trace;
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);
        connection.OnLogMessage.AddObserver(e =>
        {
            allLogs.Add(e);
            return Task.CompletedTask;
        });
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.serverDataReceivedObserver = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("Hello world"u8.ToArray(), TestContext.Current.CancellationToken);
        this.WaitForServerToReceiveData(TimeSpan.FromSeconds(4));
        await server.SendWebSocketDataAsync(registeredConnectionId, "Hello back");
        this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(4));
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.Contains(allLogs,
            e => e.Message.StartsWith("SEND >>> ") && e.Level == WebDriverBiDiLogLevel.Trace);
        Assert.Contains(allLogs,
            e => e.Message.StartsWith("RECV <<< ") && e.Level == WebDriverBiDiLogLevel.Trace);
    }

    [Fact]
    public async Task TestConnectionLog()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<LogMessageEventArgs> logValues = [];
        await using WebSocketConnection connection = new();
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);
        connection.OnLogMessage.AddObserver(e =>
        {
            if (e.Level >= WebDriverBiDiLogLevel.Info)
            {
                logValues.Add(e);
            }

            return Task.CompletedTask;
        });
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.serverDataReceivedObserver = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);
        await connection.SendDataAsync("Hello world"u8.ToArray(), TestContext.Current.CancellationToken);
        this.WaitForServerToReceiveData(TimeSpan.FromSeconds(4));

        await server.SendWebSocketDataAsync(registeredConnectionId, "Hello back");
        this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(4));
        await connection.StopAsync(TestContext.Current.CancellationToken);

        List<string> messages = [];
        foreach (LogMessageEventArgs logValue in logValues)
        {
            messages.Add(logValue.Message);
        }

        Assert.Equal(6, logValues.Count);
        foreach (LogMessageEventArgs args in logValues)
        {

            Assert.Equal(WebDriverBiDiLogLevel.Info, args.Level);
            Assert.NotNull(args.Message);
        }
    }

    [Fact]
    public async Task TestIsActiveProperty()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        await using WebSocketConnection connection = new();
        Assert.False(connection.IsActive);
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.True(connection.IsActive);
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestUrlProperty()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        string serverWebSocketUrl = $"ws://127.0.0.1:{server.Port}";
        await using WebSocketConnection connection = new();
        Assert.Equal(string.Empty, connection.ConnectionString);
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);
        await connection.StartAsync(serverWebSocketUrl, TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.Equal(serverWebSocketUrl, connection.ConnectionString);
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equal(string.Empty, connection.ConnectionString);
    }

    [Fact]
    public async Task TestStopWithoutStart()
    {
        await using WebSocketConnection connection = new();
        Assert.False(connection.IsActive);
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestStartAsyncReportsAnUnusableConnectionStringAsAnArgumentExceptionNamingTheCallersParameter()
    {
        // A connection string this connection could never open is a different kind of failure from a
        // connect that did not succeed, and the exception type is what carries that distinction to the
        // caller: an ArgumentException means the value must be corrected before starting is worth
        // attempting again. The name it carries is the parameter of Connection.StartAsync, which is the
        // argument the caller actually passed, and not the name the transport's own hook gives it.
        await using WebSocketConnection connection = new();

        ArgumentException notAbsolute = await Assert.ThrowsAnyAsync<ArgumentException>(async () => await connection.StartAsync("not-a-valid-url", TestContext.Current.CancellationToken));
        Assert.Contains("not a valid absolute URI", notAbsolute.Message);
        Assert.Equal("connectionString", notAbsolute.ParamName);

        ArgumentException wrongScheme = await Assert.ThrowsAnyAsync<ArgumentException>(async () => await connection.StartAsync("http://localhost:9222", TestContext.Current.CancellationToken));
        Assert.Contains("must be 'ws' or 'wss'", wrongScheme.Message);
        Assert.Equal("connectionString", wrongScheme.ParamName);

        // The rejection is complete: nothing about the connection was changed by the attempt, so it is
        // still startable.
        Assert.False(connection.IsActive);
        Assert.Equal(string.Empty, connection.ConnectionString);
    }

    [Fact]
    public async Task TestStartAsyncRejectsAnUnusableConnectionStringWithoutWaitingForAnAbandonedReceiveLoop()
    {
        // Interpreting the connection string happens before StartAsync does anything that can take
        // time. The wait it would otherwise sit behind is the bounded wait for a previous session's
        // receive loop, which on a reconnect costs up to ShutdownTimeout, so a caller who passed a
        // malformed URL would pay that price to be told about a fault that costs nothing to detect.
        TaskCompletionSource<WebSocketReceiveResult> receiveBlockSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource receiveBlockEnteredSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new(timeProvider)
        {
            BypassStart = false,
            BypassStop = false,
            ShutdownTimeout = TimeSpan.FromSeconds(10),
            ConnectWebSocketOverride = (uri, cancellationToken) => Task.CompletedTask,
            ReceiveHandler = (buffer, cancellationToken, callCount) =>
            {
                receiveBlockEnteredSignal.TrySetResult();
                return receiveBlockSignal.Task;
            },
        };

        // Leave a receive loop abandoned in a read that ignores cancellation, which is the state that
        // makes the wait in StartAsync actually wait.
        await connection.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        await receiveBlockEnteredSignal.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Task stopTask = connection.StopAsync(TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(stopTask, connection.ShutdownTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await stopTask;

        // The restart is refused for the connection string alone. No virtual time is advanced here: if
        // the rejection were made after the wait for that abandoned loop, this task would still be
        // running, because only advancing the clock can end that wait.
        Task rejectedStartTask = connection.StartAsync("not-a-valid-url", TestContext.Current.CancellationToken);
        Assert.True(rejectedStartTask.IsCompleted, "StartAsync did not reject the malformed connection string immediately");
        ArgumentException exception = await Assert.ThrowsAnyAsync<ArgumentException>(() => rejectedStartTask);
        Assert.Contains("not a valid absolute URI", exception.Message);

        receiveBlockSignal.SetResult(new WebSocketReceiveResult(0, WebSocketMessageType.Close, true));
    }

    [Fact]
    public async Task TestStartAsyncRefusesSecondReceiveLoopWhileAbandonedLoopStillRuns()
    {
        // The guard against a previous session's receive loop still running belongs to
        // Connection.StartAsync, so it protects every connection rather than only the pipe
        // connection it was first written for. A second loop over the same socket would interleave
        // its reads with the abandoned one's arbitrarily, so the restart is refused while it runs.
        TaskCompletionSource<WebSocketReceiveResult> receiveBlockSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource receiveBlockEnteredSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new(timeProvider)
        {
            BypassStart = false,
            BypassStop = false,
            ShutdownTimeout = TimeSpan.FromSeconds(10),

            // Stand in for a remote end that accepts the connection, so that no real socket is needed.
            ConnectWebSocketOverride = (uri, cancellationToken) => Task.CompletedTask,

            // A read that ignores its cancellation token, which is what leaves a loop to be abandoned.
            ReceiveHandler = (buffer, cancellationToken, callCount) =>
            {
                receiveBlockEnteredSignal.TrySetResult();
                return receiveBlockSignal.Task;
            },
        };

        await connection.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        await receiveBlockEnteredSignal.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // StopAsync gives up on the blocked read at its shutdown timeout and abandons the loop; the
        // timeout is elapsed on the virtual clock as soon as the stop arms it.
        Task stopTask = connection.StopAsync(TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(stopTask, connection.ShutdownTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await stopTask;

        // The restart gives the abandoned loop the same bounded chance to finish before refusing.
        Task refusedStartTask = connection.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(refusedStartTask, connection.ShutdownTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        WebDriverBiDiConnectionException exception = await Assert.ThrowsAsync<WebDriverBiDiConnectionException>(() => refusedStartTask);
        Assert.Contains("receive loop from a previous session", exception.Message);

        // Releasing the read lets the abandoned loop observe its canceled token and exit, after which
        // a new session is permitted.
        receiveBlockSignal.SetResult(new WebSocketReceiveResult(0, WebSocketMessageType.Close, true));
        await connection.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        Assert.Equal("ws://localhost", connection.ConnectionString);

        Task finalStopTask = connection.StopAsync(TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(finalStopTask, connection.ShutdownTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await finalStopTask;
    }

    [Fact]
    public async Task TestStopWithoutStartLogsClientStateNone()
    {
        List<string> connectionLog = [];
        await using WebSocketConnection connection = new();
        // This test asserts on Debug or Trace messages, which the default minimum level excludes.
        connection.LogLevel = WebDriverBiDiLogLevel.Trace;
        connection.OnLogMessage.AddObserver(e =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });
        await connection.StopAsync(TestContext.Current.CancellationToken);

        List<string> expectedLogEntries =
        [
            "Closing WebSocket connection",
            "Client state is None",
            "WebSocket connection closed"
        ];
        Assert.Equal(expectedLogEntries, connectionLog);
    }

    [Fact]
    public async Task TestStopForcesCancellationOfDataReceiveTask()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        await using TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
            BypassCloseClientWebSocket = false,
        };
        Assert.False(connection.IsActive);
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.True(connection.IsActive);

        // Send data to the connection, which should force the receive data
        // task to enter a waiting state after receiving the first message.
        await server.SendWebSocketDataAsync(registeredConnectionId, "Hello back");
        byte[] dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestConnectionStopCanBeCalledMultipleTimes()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<string> connectionLog = [];
        await using WebSocketConnection connection = new();
        // This test asserts on Debug or Trace messages, which the default minimum level excludes.
        connection.LogLevel = WebDriverBiDiLogLevel.Trace;
        connection.OnLogMessage.AddObserver(e =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.StopAsync(TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);

        // First call: socket Open -> CloseClientWebSocketAsync -> "Client state is Closed"
        // Second call: socket Closed -> early-exit -> "Client state is Closed"
        // Also: "Ending processing loop in state Closed" from receive loop
        Assert.Equal(2, connectionLog.Count(item => item == "Closing WebSocket connection"));
        Assert.Equal(2, connectionLog.Count(item => item == "Client state is Closed"));
        Assert.Contains("Ending processing loop in state Closed", connectionLog);
    }

    [Fact]
    public async Task TestConnectionHandlesUnexpectedRemoteEndStop()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<string> expectedLogEntries =
        [
            $"Opening WebSocket connection to ws://127.0.0.1:{server.Port}",
            "WebSocket connection opened",

            // The remote end stops without a close handshake, so the receive loop faults and ends
            // before this test calls StopAsync; the closing entries follow, not precede, those two.
            "Unexpected error during receive of data: The remote party closed the WebSocket connection without completing the close handshake.",
            "Ending processing loop in state Aborted",
            "Closing WebSocket connection",
            "Client state is Aborted",
            "WebSocket connection closed"
        ];

        object logLock = new();
        List<LogMessageEventArgs> connectionLog = [];
        TaskCompletionSource receiveLoopEnded = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await using WebSocketConnection connection = new()
        {
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        // This test asserts on Debug or Trace messages, which the default minimum level excludes.
        connection.LogLevel = WebDriverBiDiLogLevel.Trace;
        connection.OnLogMessage.AddObserver(e =>
        {
            lock (logLock)
            {
                connectionLog.Add(e);
            }

            if (e.Message.StartsWith("Ending processing loop in state Aborted", StringComparison.Ordinal))
            {
                receiveLoopEnded.TrySetResult();
            }

            return Task.CompletedTask;
        });
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await server.StopAsync();

        // Wait for the receive loop to fully exit with the socket in the Aborted state before calling
        // StopAsync. Waiting only for OnConnectionError is not enough: the error is reported before the
        // socket has definitively transitioned to Aborted, so StopAsync could observe State == Open,
        // take the close path instead of the early-exit path, and never log "Client state is Aborted".
        // The receive loop logs "Ending processing loop in state Aborted" only once client.State is
        // Aborted (a terminal state), so gating on it guarantees StopAsync sees Aborted, and it also
        // ensures the loop is no longer logging concurrently with the assertion below.
        await receiveLoopEnded.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        await connection.StopAsync(TestContext.Current.CancellationToken);

        LogMessageEventArgs[] logSnapshot;
        lock (logLock)
        {
            logSnapshot = [.. connectionLog];
        }

        Assert.Equal(expectedLogEntries, logSnapshot.Select(log => log.Message));

        // The sequence assertion above compares message text only. A failure that ends the receive
        // loop is an error, not information, so that one entry's level is pinned separately: a
        // consumer filtering at Warn or above must still see the message carrying the socket-level
        // detail.
        Assert.Contains(logSnapshot, log => log.Message.StartsWith("Unexpected error during receive of data", StringComparison.Ordinal) && log.Level == WebDriverBiDiLogLevel.Error);
    }

    [Fact]
    public async Task TestReceiveDataRaisesErrorEventOnDataReceivedObserverException()
    {
        // An exception from the observer of OnDataReceived is rethrown by the event into the
        // receive loop. Were it not caught there, the loop would end without notice: the socket
        // would remain open while no further message was ever delivered, which a caller awaiting
        // a command response cannot distinguish from a remote end that has simply gone quiet.
        static Task ThrowOnDataReceived(ConnectionDataReceivedEventArgs e) => throw new InvalidOperationException("observer failure");

        await using Server server = this.CreateServer();
        await server.StartAsync();

        ConnectionErrorEventArgs? receivedErrorArgs = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await using WebSocketConnection connection = new();
        connection.OnDataReceived.AddObserver(ThrowOnDataReceived);
        connection.OnConnectionError.AddObserver(e =>
        {
            receivedErrorArgs = e;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await server.SendWebSocketDataAsync(registeredConnectionId, "Hello back");

        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(receivedErrorArgs);
        Assert.Equal("observer failure", Assert.IsType<InvalidOperationException>(receivedErrorArgs.Exception).Message);
    }

    [Fact]
    public async Task TestReceiveLoopEndedByObserverExceptionLeavesConnectionInactiveAndRestartable()
    {
        // The observer's exception ends the receive loop while the socket itself is still open: nothing
        // failed on the wire. A connection that went on reporting itself active afterwards would be
        // adopted, rather than reopened, by Transport.ConnectAsync, and the new session would send on a
        // socket that nothing reads. The connection must report itself inactive by the time the failure
        // is observable, and a restart must open a new session that delivers data.
        int remainingFailures = 1;
        Task OnDataReceivedAsync(ConnectionDataReceivedEventArgs e)
        {
            if (Interlocked.Exchange(ref remainingFailures, 0) == 1)
            {
                throw new InvalidOperationException("observer failure");
            }

            return this.OnConnectionDataReceivedAsync(e);
        }

        await using Server server = this.CreateServer();
        await server.StartAsync();

        bool? isActiveWhenErrorRaised = null;
        TaskCompletionSource errorRaisedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await using WebSocketConnection connection = new();
        connection.OnDataReceived.AddObserver(OnDataReceivedAsync);
        connection.OnConnectionError.AddObserver(e =>
        {
            isActiveWhenErrorRaised = connection.IsActive;
            errorRaisedTaskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        string connectionString = $"ws://127.0.0.1:{server.Port}";
        await connection.StartAsync(connectionString, TestContext.Current.CancellationToken);
        string firstConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await server.SendWebSocketDataAsync(firstConnectionId, "Hello back");
        await errorRaisedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // The value captured inside the error observer is what pins the ordering: the connection is
        // inactive before anyone is told about the failure, not merely by the time StopAsync runs.
        Assert.False(isActiveWhenErrorRaised);
        Assert.False(connection.IsActive);

        await connection.StopAsync(TestContext.Current.CancellationToken);
        await connection.StartAsync(connectionString, TestContext.Current.CancellationToken);
        string secondConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.NotEqual(firstConnectionId, secondConnectionId);

        await server.SendWebSocketDataAsync(secondConnectionId, "Hello again");
        Assert.Equal("Hello again"u8.ToArray(), this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3)));
        await connection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestConnectionStopWhileReceiveBlocked()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        // This test deterministically exercises the path where StopAsync is called while the
        // socket is still Open, so CloseClientWebSocketAsync runs (not the early-exit path).
        // The ReceiveHandler blocks until cancellation, keeping client.State == Open.
        List<string> expectedLogEntries =
        [
            $"Opening WebSocket connection to ws://127.0.0.1:{server.Port}",
            "WebSocket connection opened",
            "Closing WebSocket connection",
            "Client state is CloseSent",  // We send close frame; server may not respond before timeout
            "Ending processing loop in state CloseSent",
            "WebSocket connection closed"
        ];

        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
            BypassCloseClientWebSocket = false,
            ShutdownTimeout = TimeSpan.FromSeconds(1),
            ReceiveHandler = async (buffer, cancellationToken, callCount) =>
            {
                // Block until StopAsync cancels the token. Keeps client.State == Open.
                // Note that the return value is unreachable because the cancellation
                // will cause the receive loop to exit before processing the return,
                // but it satisfies the delegate signature.
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return new(0, WebSocketMessageType.Text, true);
            }
        };

        List<string> connectionLog = [];
        connection.OnLogMessage.AddObserver(e =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));

        // Receive loop is blocked in ReceiveHandler; client.State is still Open.
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(expectedLogEntries, connectionLog);
    }

    [Fact]
    public async Task TestConnectionStopBoundsWaitForUnresponsiveReceiveLoop()
    {
        // Regression guard for the receive-loop wait in StopAsync being bounded by ShutdownTimeout.
        // Unlike TestConnectionStopWhileReceiveBlocked, whose ReceiveHandler unblocks on cancellation,
        // this handler blocks on a test-controlled signal and ignores the connection's cancellation
        // token, so the receive loop does not finish when StopAsync cancels it. An unbounded
        // "await this.DataReceiveTask" would hang; a bounded wait returns after ShutdownTimeout and
        // logs a warning, leaving the receive task to finish on its own later.
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TaskCompletionSource<WebSocketReceiveResult> receiveLoopBlock = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource receiveHandlerEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new(timeProvider)
        {
            BypassStart = false,
            BypassStop = false,
            BypassCloseClientWebSocket = true,
            ShutdownTimeout = TimeSpan.FromSeconds(10),
            ReceiveHandler = (buffer, cancellationToken, callCount) =>
            {
                // Deliberately ignore the cancellation token so the receive loop stays blocked even
                // after StopAsync cancels the connection.
                receiveHandlerEntered.TrySetResult();
                return receiveLoopBlock.Task;
            },
        };

        object logLock = new();
        List<string> connectionLog = [];
        connection.OnLogMessage.AddObserver(e =>
        {
            lock (logLock)
            {
                connectionLog.Add(e.Message);
            }

            return Task.CompletedTask;
        });

        try
        {
            await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
            this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));

            // Ensure the receive loop is actually parked in the (uncancellable) handler before
            // stopping, so the wait deterministically reaches the ShutdownTimeout bound.
            await receiveHandlerEntered.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            // The shutdown timeout is elapsed on the virtual clock as soon as the stop arms it; a
            // stop whose wait was not bounded by ShutdownTimeout would hang here until the
            // five-second guard inside AdvanceUntilCompletedAsync fired.
            Task stopTask = connection.StopAsync(TestContext.Current.CancellationToken);
            await timeProvider.AdvanceUntilCompletedAsync(stopTask, connection.ShutdownTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
            await stopTask;

            string[] logSnapshot;
            lock (logLock)
            {
                logSnapshot = [.. connectionLog];
            }

            Assert.Contains("Timed out waiting for WebSocket connection receive loop to complete during shutdown", logSnapshot);
        }
        finally
        {
            // Release the receive loop so it can complete and not leak past the test.
            receiveLoopBlock.TrySetResult(new WebSocketReceiveResult(0, WebSocketMessageType.Close, true));
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task TestConnectionInitiateWebSocketClose()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<string> expectedLogEntries =
        [
            $"Opening WebSocket connection to ws://127.0.0.1:{server.Port}",
            "WebSocket connection opened",
            "Closing WebSocket connection",
            "Ending processing loop in state Closed",
            "Client state is Closed",
            "WebSocket connection closed"
        ];

        List<string> connectionLog = [];
        await using WebSocketConnection connection = new();
        connection.OnLogMessage.AddObserver(e =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equal(expectedLogEntries, connectionLog);
    }

    [Fact]
    public async Task TestConnectionHandlesDisconnectInitiatedByRemoteEnd()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<string> expectedLogEntries =
        [
            $"Opening WebSocket connection to ws://127.0.0.1:{server.Port}",
            "WebSocket connection opened",
            "Acknowledging Close frame received from server (client state: CloseReceived)",
            "Ending processing loop in state Closed",
            "Closing WebSocket connection",
            "Client state is Closed",
            "WebSocket connection closed"
        ];

        List<string> connectionLog = [];
        object logLock = new();
        TaskCompletionSource receiveLoopEnded = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await using WebSocketConnection connection = new()
        {
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        // This test asserts on Debug or Trace messages, which the default minimum level excludes.
        connection.LogLevel = WebDriverBiDiLogLevel.Trace;
        connection.OnLogMessage.AddObserver(e =>
        {
            lock (logLock)
            {
                connectionLog.Add(e.Message);
            }

            if (e.Message.StartsWith("Ending processing loop", StringComparison.Ordinal))
            {
                receiveLoopEnded.TrySetResult();
            }

            return Task.CompletedTask;
        });

        // This test has failed intermittently in CI with nothing but a timeout to go on, which cannot
        // distinguish a Close frame the server never sent (for instance, to a connection other than the
        // one the client is using) from one the client never read or whose receive faulted. The server
        // log records each connection and the byte count of each send, and the connection reports any
        // fault as an error, so both are read back in the failure message.
        Exception? connectionError = null;
        connection.OnConnectionError.AddObserver(e =>
        {
            connectionError = e.Exception;
            return Task.CompletedTask;
        });

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));

        // Gate on the receive loop's own final log rather than on OnRemoteDisconnected. The loop
        // raises that event from inside its try block and logs "Ending processing loop" afterwards in
        // its finally, so waiting on the event would let StopAsync log ahead of that entry.
        await server.DisconnectAsync(registeredConnectionId);
        try
        {
            await receiveLoopEnded.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        }
        catch (TimeoutException)
        {
            string log;
            lock (logLock)
            {
                log = connectionLog.Count == 0 ? "(none)" : string.Join(" | ", connectionLog);
            }

            IReadOnlyList<string> serverLog = server.Log;
            string serverLogText = serverLog.Count == 0 ? "(none)" : string.Join(" | ", serverLog);
            Assert.Fail($"The server disconnected connection {registeredConnectionId} but the receive loop never ended. Connection error: {connectionError?.GetType().Name ?? "(none)"}: {connectionError?.Message ?? string.Empty}. Connection log: {log}. Server log: {serverLogText}");
        }

        await connection.StopAsync(TestContext.Current.CancellationToken);

        string[] logSnapshot;
        lock (logLock)
        {
            logSnapshot = [.. connectionLog];
        }

        Assert.Equal(expectedLogEntries, logSnapshot);
    }

    [Fact]
    public async Task TestConnectionHandlesHungRemoteEnd()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<string> expectedLogEntries =
        [
            $"Opening WebSocket connection to ws://127.0.0.1:{server.Port}",
            "WebSocket connection opened",
            "Closing WebSocket connection",
            "Unexpected error during receive of data: The remote party closed the WebSocket connection without completing the close handshake.",
            "Ending processing loop in state Aborted",
            "Client state is Aborted",
            "WebSocket connection closed"
        ];

        List<string> connectionLog = [];
        await using WebSocketConnection connection = new();
        connection.OnLogMessage.AddObserver(e =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });

        IReadOnlyList<string> serverLog = server.Log;
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        server.IgnoreCloseConnectionRequest(registeredConnectionId, true);
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equal(expectedLogEntries, connectionLog);
    }

    [Fact]
    public async Task TestConnectionRaisesErrorEventOnWebSocketException()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        ConnectionErrorEventArgs? receivedErrorArgs = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await using WebSocketConnection connection = new()
        {
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnConnectionError.AddObserver(e =>
        {
            receivedErrorArgs = e;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        server.IgnoreCloseConnectionRequest(this.connectionId, true);
        await connection.StopAsync(TestContext.Current.CancellationToken);

        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.NotNull(receivedErrorArgs);
        Assert.IsType<WebSocketException>(receivedErrorArgs.Exception);
    }

    [Fact]
    public async Task TestConnectionCanBeReusedAfterBeingShutDown()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        await using WebSocketConnection connection = new()
        {
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        ServerEventObserver<ServerDataReceivedEventArgs> observer = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("First connection hello"u8.ToArray(), TestContext.Current.CancellationToken);
        string serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromSeconds(3));
        observer.Unobserve();
        Assert.Equal("First connection hello", serverReceivedData);

        await server.SendWebSocketDataAsync(registeredConnectionId, "First connection acknowledged");
        byte[] receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equal("First connection acknowledged"u8.ToArray(), receivedData);

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        observer = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("Second connection hello"u8.ToArray(), TestContext.Current.CancellationToken);
        serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromSeconds(3));
        observer.Unobserve();
        Assert.Equal("Second connection hello", serverReceivedData);

        await server.SendWebSocketDataAsync(registeredConnectionId, "Second connection acknowledged");
        receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equal("Second connection acknowledged"u8.ToArray(), receivedData);
    }

    [Fact]
    public async Task TestConnectionCanBeReusedAfterBeingAborted()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        await using WebSocketConnection connection = new()
        {
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        ServerEventObserver<ServerDataReceivedEventArgs> observer = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("First connection hello"u8.ToArray(), TestContext.Current.CancellationToken);
        string serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromSeconds(3));
        observer.Unobserve();
        Assert.Equal("First connection hello", serverReceivedData);

        await server.SendWebSocketDataAsync(registeredConnectionId, "First connection acknowledged");
        byte[] receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));
        server.IgnoreCloseConnectionRequest(registeredConnectionId, true);
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equal("First connection acknowledged"u8.ToArray(), receivedData);

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        // Use generous timeouts for the second connection: the abort path above takes the full
        // ShutdownTimeout (1 s) to complete, and on a loaded CI machine 250 ms is insufficient
        // for the server to register the new connection and exchange data.
        registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(3));
        observer = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("Second connection hello"u8.ToArray(), TestContext.Current.CancellationToken);
        serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromSeconds(3));
        observer.Unobserve();
        Assert.Equal("Second connection hello", serverReceivedData);

        await server.SendWebSocketDataAsync(registeredConnectionId, "Second connection acknowledged");
        receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equal("Second connection acknowledged"u8.ToArray(), receivedData);
    }

    [Fact]
    public async Task TestConnectionCanBeStartedAfterStoppingWithoutStarting()
    {
        // StopAsync cancels the connection's CancellationTokenSource unconditionally, even when
        // the connection was never started. If StartAsync did not reset that source, this
        // connection would begin its first session with cancellation already requested and the
        // connect attempt would fail immediately with a TaskCanceledException.
        await using Server server = this.CreateServer();
        await server.StartAsync();

        await using WebSocketConnection connection = new()
        {
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);

        await connection.StopAsync(TestContext.Current.CancellationToken);

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        ServerEventObserver<ServerDataReceivedEventArgs> observer = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("Hello after premature stop"u8.ToArray(), TestContext.Current.CancellationToken);
        string serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromSeconds(3));
        observer.Unobserve();
        Assert.Equal("Hello after premature stop", serverReceivedData);

        await server.SendWebSocketDataAsync(registeredConnectionId, "Acknowledged after premature stop");
        byte[] receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equal("Acknowledged after premature stop"u8.ToArray(), receivedData);
    }

    [Fact]
    public async Task TestConnectionCanBeStartedAfterFailedConnectionAttempt()
    {
        // Starting replaces the connection's ClientWebSocket, but not its CancellationTokenSource,
        // and the caller's cleanup StopAsync after the failed attempt cancels that source. Unless
        // StartAsync resets the source on every start, the retry below would fail immediately with
        // a TaskCanceledException and the connection could never be used again.
        //
        // Find an available port and release it before use, so that the first connection
        // attempt is made against a port on which nothing is listening. See the comment in
        // TestConnectionFailure regarding the theoretical race in this approach.
        int deadPort;
        using (TcpListener portFinder = new(IPAddress.Loopback, 0))
        {
            portFinder.Start();
            deadPort = ((IPEndPoint)portFinder.LocalEndpoint).Port;
            portFinder.Stop();
        }

        // The failed attempt runs on a virtual clock. Time is advanced only once the connection has armed
        // its pause before retrying, which it does only after the refused connect has failed, so the attempt
        // always takes the retry path instead of racing its own deadline, and no real time is spent waiting.
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new(timeProvider)
        {
            StartupTimeout = TimeSpan.FromMilliseconds(200),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
            BypassStart = false,
            BypassStop = false,
            BypassCloseClientWebSocket = false,
            BypassDataSend = false,
        };
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);

        Task failedStart = connection.StartAsync($"ws://127.0.0.1:{deadPort}", TestContext.Current.CancellationToken);

        // The first timer is the attempt's deadline and the second is the pause before the next attempt.
        await timeProvider.WaitForTimerCreatedAsync(1).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.False(failedStart.IsCompleted);
        timeProvider.Advance(connection.StartupTimeout);

        _ = await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await failedStart);
        Assert.Equal(2, timeProvider.TimerCount);

        // The caller cleans up after the failed attempt before retrying.
        await connection.StopAsync(TestContext.Current.CancellationToken);

        await using Server server = this.CreateServer();
        await server.StartAsync();

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        ServerEventObserver<ServerDataReceivedEventArgs> observer = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("Hello after failed attempt"u8.ToArray(), TestContext.Current.CancellationToken);
        string serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromSeconds(3));
        observer.Unobserve();
        Assert.Equal("Hello after failed attempt", serverReceivedData);

        await server.SendWebSocketDataAsync(registeredConnectionId, "Acknowledged after failed attempt");
        byte[] receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));

        // The clock is not advanced again, so the close completes on the server's reply rather than on the
        // shutdown timeout; the bound only turns a missing reply into a failure instead of a hang.
        await connection.StopAsync(TestContext.Current.CancellationToken).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal("Acknowledged after failed attempt"u8.ToArray(), receivedData);
    }

    [Fact]
    public async Task TestCreateClientWebSocketConfiguresTheSocketUsedForTheFirstSession()
    {
        // ClientWebSocket options can be set only before the socket connects, so a derived connection
        // configures them in CreateClientWebSocket. An override may depend on state that its own
        // constructor or an object initializer assigns -- here, the delegate set below -- so the
        // connection must not call it while the connection is still being constructed, and must obtain
        // the socket for the first session from it once construction is over. The configuration is
        // observed in the upgrade request the server receives, which is where a remote end that
        // requires it would look for it.
        const string headerName = "X-WebDriverBiDi-Test";
        TaskCompletionSource<string> upgradeRequestReceived = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await using Server server = this.CreateServer();
        ServerEventObserver<ServerDataReceivedEventArgs> upgradeObserver = server.OnDataReceived.AddObserver(e =>
        {
            if (e.Data.StartsWith("GET ", StringComparison.Ordinal))
            {
                upgradeRequestReceived.TrySetResult(e.Data);
            }
        });
        await server.StartAsync();

        await using TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
            BypassCloseClientWebSocket = false,
            ShutdownTimeout = TimeSpan.FromSeconds(1),
            ConfigureClientWebSocket = socket => socket.Options.SetRequestHeader(headerName, "first-session"),
        };
        int socketsCreatedDuringConstruction = connection.CreatedClientWebSockets.Count;

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string upgradeRequest = await upgradeRequestReceived.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);
        upgradeObserver.Unobserve();

        Assert.Contains($"{headerName}: first-session", upgradeRequest, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, socketsCreatedDuringConstruction);
        Assert.Single(connection.CreatedClientWebSockets);
    }

    [Fact]
    public async Task TestCreateClientWebSocketConfiguresTheSocketForEachLaterSession()
    {
        // A ClientWebSocket connects at most once, so a session started after a stop connects on a new
        // socket. That socket must also come from CreateClientWebSocket, and be configured afresh rather
        // than copied from the previous one: the header value below differs per call.
        const string headerName = "X-WebDriverBiDi-Test";
        TaskCompletionSource<string>[] upgradeRequestsReceived =
        [
            new(TaskCreationOptions.RunContinuationsAsynchronously),
            new(TaskCreationOptions.RunContinuationsAsynchronously),
        ];
        int upgradeRequestCount = 0;
        await using Server server = this.CreateServer();
        ServerEventObserver<ServerDataReceivedEventArgs> upgradeObserver = server.OnDataReceived.AddObserver(e =>
        {
            if (e.Data.StartsWith("GET ", StringComparison.Ordinal))
            {
                int index = Interlocked.Increment(ref upgradeRequestCount) - 1;
                if (index < upgradeRequestsReceived.Length)
                {
                    upgradeRequestsReceived[index].TrySetResult(e.Data);
                }
            }
        });
        await server.StartAsync();

        int configuredSocketCount = 0;
        await using TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
            BypassCloseClientWebSocket = false,
            ShutdownTimeout = TimeSpan.FromSeconds(1),
            ConfigureClientWebSocket = socket => socket.Options.SetRequestHeader(headerName, $"session-{Interlocked.Increment(ref configuredSocketCount)}"),
        };

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string firstUpgradeRequest = await upgradeRequestsReceived[0].Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string secondUpgradeRequest = await upgradeRequestsReceived[1].Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);
        upgradeObserver.Unobserve();

        Assert.Contains($"{headerName}: session-1", firstUpgradeRequest, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"{headerName}: session-2", secondUpgradeRequest, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, connection.CreatedClientWebSockets.Count);
        Assert.NotSame(connection.CreatedClientWebSockets[0], connection.CreatedClientWebSockets[1]);
    }

    [Fact]
    public async Task TestCreateClientWebSocketReplacesTheSocketAfterEachRefusedAttempt()
    {
        // A socket whose connect was refused cannot be used again, so each attempt within the startup
        // budget connects on a socket from CreateClientWebSocket, and every refusal replaces the socket
        // it used. The connection owns each socket it is given: a replaced socket is disposed at once,
        // and the socket it holds last is disposed with the connection. (A ClientWebSocket disposed
        // before it ever connected reports Closed; one that has not been disposed reports None.)
        //
        // The pause before each retry runs on the virtual clock and is charged against the startup budget,
        // so the budget has to outlast the pauses for three attempts to fit. Each attempt is refused at once
        // and takes no virtual time, and the test elapses each pause once the connection has armed it. The
        // attempts' deadlines and the pauses alternate, so the pauses are the second, fourth and sixth
        // timers. Attempts begin at 0, 500 and 1000 ms; the third attempt's pause is clamped to the 100 ms
        // left, after which no budget remains for a fourth.
        TimeSpan startupTimeout = TimeSpan.FromMilliseconds(1100);

        // The connection's interval between attempts. Advancing by it elapses every pause, including one
        // clamped to a shorter remaining budget.
        TimeSpan retryInterval = TimeSpan.FromMilliseconds(500);
        int attemptCount = 0;
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new(timeProvider)
        {
            BypassStart = false,
            StartupTimeout = startupTimeout,
            ConnectWebSocketOverride = (uri, token) =>
            {
                Interlocked.Increment(ref attemptCount);
                return Task.FromException(new WebSocketException("Simulated refused connection"));
            },
        };

        Task startTask = connection.StartAsync("ws://127.0.0.1:1", TestContext.Current.CancellationToken);
        for (int timersBeforePause = 1; timersBeforePause <= 5; timersBeforePause += 2)
        {
            await timeProvider.WaitForTimerCreatedAsync(timersBeforePause).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            timeProvider.Advance(retryInterval);
        }

        await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await startTask);

        Assert.Equal(3, attemptCount);
        Assert.Equal(6, timeProvider.TimerCount);
        List<ClientWebSocket> sockets = connection.CreatedClientWebSockets;
        Assert.Equal(4, sockets.Count);
        Assert.All(sockets.Take(3), socket => Assert.Equal(WebSocketState.Closed, socket.State));
        Assert.Equal(WebSocketState.None, sockets[3].State);

        await connection.DisposeAsync();
        Assert.Equal(WebSocketState.Closed, sockets[3].State);
    }

    [Fact]
    public async Task TestCreateClientWebSocketReplacesTheSocketAbandonedByAStartupTimeout()
    {
        // A connect still in progress when the startup budget runs out is canceled, and a canceled
        // connect leaves its socket unusable. The connection disposes that socket and replaces it through
        // CreateClientWebSocket, so the connection is left holding a usable socket for a later start.
        TimeSpan startupTimeout = TimeSpan.FromMilliseconds(200);
        TestTimeProvider timeProvider = new();
        await using TestWebSocketConnection connection = new(timeProvider)
        {
            BypassStart = false,
            StartupTimeout = startupTimeout,
            ConnectWebSocketOverride = (uri, token) => Task.Delay(Timeout.InfiniteTimeSpan, token),
        };

        Task startTask = connection.StartAsync("ws://127.0.0.1:1", TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(startTask, startupTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await startTask);

        List<ClientWebSocket> sockets = connection.CreatedClientWebSockets;
        Assert.Equal(2, sockets.Count);
        Assert.Equal(WebSocketState.Closed, sockets[0].State);
        Assert.Equal(WebSocketState.None, sockets[1].State);
    }

    [Fact]
    public async Task TestCannotStartAlreadyStartedConnection()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        await using WebSocketConnection connection = new()
        {
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.StartsWith($"The WebSocket connection is already connected to ws://127.0.0.1:{server.Port}", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestStartAsyncThrowsForInvalidUrl()
    {
        await using WebSocketConnection connection = new();
        Assert.Contains("not a valid absolute URI", (await Assert.ThrowsAnyAsync<ArgumentException>(async () => await connection.StartAsync("not-a-valid-url", TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestStartAsyncThrowsForNonWebSocketUrl()
    {
        await using WebSocketConnection connection = new();
        Assert.Contains("The URI scheme must be 'ws' or 'wss'; received 'http'", (await Assert.ThrowsAnyAsync<ArgumentException>(async () => await connection.StartAsync("http://localhost:8080", TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestStartAfterDisposeThrows()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new();
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        await connection.DisposeAsync();

        Assert.Contains("connection has been disposed", (await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await connection.StartAsync("ws://localost", TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestCanStartWithSecuredWebSocketUrl()
    {
        using RSA rsa = RSA.Create(2048);
        CertificateRequest request = new("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using X509Certificate2 ephemeral = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(1));

        // Export to PFX and reimport so the private key is stored in a key container
        // rather than as an ephemeral key. This is required on Windows (Schannel) for
        // SslStream.AuthenticateAsServerAsync to succeed.
        using X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12(ephemeral.Export(X509ContentType.Pfx), password: null);

        await using Server server = this.CreateServer();
        server.Certificate = certificate;
        await server.StartAsync();

        // The budget runs on the virtual clock. A 10 ms real budget was not only slow-by-a-little, it
        // was a race in both directions: a handshake that happened to complete inside it would have
        // failed the test.
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new(timeProvider)
        {
            BypassStart = false,
            StartupTimeout = TimeSpan.FromMilliseconds(10),
        };

        // We expect this to fail with a timeout, but it verifies that the connection
        // attempts to connect to the correct URL and that the URL is accepted as valid.
        Task startTask = connection.StartAsync($"wss://localhost:{server.Port}", TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(startTask, connection.StartupTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await startTask);
    }

    [Fact]
    public async Task TestCannotSendDataOnAConnectionNotYetStarted()
    {
        await using WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        Assert.StartsWith("The WebSocket connection is not active", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await connection.SendDataAsync("This send should fail"u8.ToArray(), TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestCannotSendDataOnAConnectionThatHasBeenClosed()
    {
        // The send guard fires on IsActive alone, which is false for a connection that was never started,
        // for one that has been closed, and for one whose receive loop has ended. It does not distinguish
        // them, so its message must describe all of them rather than telling a caller who did start the
        // connection that they forgot to.
        await using Server server = this.CreateServer();
        await server.StartAsync();

        await using WebSocketConnection connection = new();
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.StopAsync(TestContext.Current.CancellationToken);

        WebDriverBiDiConnectionException exception = await Assert.ThrowsAsync<WebDriverBiDiConnectionException>(async () => await connection.SendDataAsync("This send should fail"u8.ToArray(), TestContext.Current.CancellationToken));
        Assert.Contains("is not active", exception.Message);
        Assert.Contains("has not been started, it has already been closed, or its receive loop has ended", exception.Message);
    }

    [Fact]
    public async Task TestCanShutdownWhenCleanShutdownExceedsTimeout()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        // With ShutdownTimeout=Zero, StopAsync returns without waiting for the receive/close loop to
        // finish, so that background loop can still be appending log messages after StopAsync returns.
        // Guard the list and snapshot it under the same lock before asserting, so the assertion does
        // not enumerate the list while a background Add is mutating it.
        object logLock = new();
        List<string> connectionLog = [];
        await using WebSocketConnection connection = new()
        {
            ShutdownTimeout = TimeSpan.Zero,
        };
        connection.OnLogMessage.AddObserver(e =>
        {
            lock (logLock)
            {
                connectionLog.Add(e.Message);
            }

            return Task.CompletedTask;
        });

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        server.IgnoreCloseConnectionRequest(registeredConnectionId, true);
        await connection.StopAsync(TestContext.Current.CancellationToken);

        string[] logSnapshot;
        lock (logLock)
        {
            logSnapshot = [.. connectionLog];
        }

        // With ShutdownTimeout=Zero, CloseClientWebSocketAsync may throw OperationCanceledException
        // before logging "Client state is X". At minimum we get "Closing WebSocket connection".
        Assert.Contains("Closing WebSocket connection", logSnapshot);
        Assert.True(logSnapshot.Length >= 1);
    }

    [Fact]
    public async Task TestDataSendOperationsAreSynchronized()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TaskCompletionSource sendBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new(timeProvider)
        {
            BypassStart = false,
            BypassStop = false,
            BypassDataSend = false,
            SendBarrier = sendBarrier,
            DataTimeout = TimeSpan.FromSeconds(10),
        };
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.OnDataSendStarting.AddObserver(e => taskCompletionSource.TrySetResult());

        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Task firstSendTask = Task.Run(() => connection.SendDataAsync("first data"u8.ToArray(), TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);

        // Wait until the first send has acquired the semaphore and is blocked on the barrier,
        // then attempt a second send which must time out before the barrier releases.
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        // The data timeout is elapsed on the virtual clock as soon as the second send arms it.
        Task secondSendTask = connection.SendDataAsync("second data"u8.ToArray(), TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(secondSendTask, connection.DataTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        Assert.Equal("Timed out waiting to access connection for sending; only one send operation is permitted at a time.", (await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await secondSendTask)).Message);
        sendBarrier.SetResult();

        // The stop's own shutdown wait is also on the virtual clock, so elapse it the same way should
        // the receive loop not end promptly.
        Task stopTask = connection.StopAsync(TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(stopTask, connection.ShutdownTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await stopTask;

        // The first send may fault with a WebDriverBiDiConnectionException if StopAsync aborted
        // the WebSocket before the send completed. Observe the exception to prevent
        // UnobservedTaskException from being raised when the task is garbage-collected.
        try
        {
            await firstSendTask;
        }
        catch (WebDriverBiDiConnectionException)
        {
        }
        catch (OperationCanceledException)
        {
        }
    }

    [Fact]
    public async Task TestCanDisposeAsyncWithoutStarting()
    {
        WebSocketConnection connection = new();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task TestDoubleDisposeAsyncDoesNotThrow()
    {
        WebSocketConnection connection = new();
        await connection.DisposeAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task TestDoubleDisposeAsyncAfterStartDoesNotThrow()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new();
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.DisposeAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task TestIsDisposedPropertyIsSetAfterDispose()
    {
        TestWebSocketConnection connection = new();
        Assert.False(connection.Disposed);
        await connection.DisposeAsync();
        Assert.True(connection.Disposed);
    }

    [Fact]
    public async Task TestCanDisposeAsyncAfterStop()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new();
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.StopAsync(TestContext.Current.CancellationToken);
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task TestCanDisposeAsyncWithoutStoping()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new();
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.DisposeAsync();
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestDisposeLogsExceptionFromStop()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        connection.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });
        // TestWebSocketConnection bypasses the real connect by default, so there is no client
        // connection for the server to register and nothing to wait for here.
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        connection.ThrowOnStop = true;
        connection.BypassStop = false;
        await connection.DisposeAsync();
        Assert.Contains(logs,
            log => log.Message.Contains("Unexpected exception during disposal")
                   && log.Message.Contains("Simulated stop failure")
                   && log.Level == WebDriverBiDiLogLevel.Warn
                   && log.ComponentName == Connection.LoggerComponentName);
    }

    [Fact]
    public async Task TestCanDisposeAsyncStartedConnectionAfterStop()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new();
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.serverDataReceivedObserver = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("Hello world"u8.ToArray(), TestContext.Current.CancellationToken);
        this.WaitForServerToReceiveData(TimeSpan.FromSeconds(3));

        await connection.StopAsync(TestContext.Current.CancellationToken);
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task TestConnectionAssemblesFragmentedMessage()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
        };

        byte[] part1 = Encoding.UTF8.GetBytes("Hello");
        byte[] part2 = Encoding.UTF8.GetBytes(", World!");
        connection.ReceiveHandler = async (buffer, token, callNum) =>
        {
            if (callNum == 1)
            {
                part1.CopyTo(buffer.Array!, buffer.Offset);
                return await Task.FromResult(new WebSocketReceiveResult(part1.Length, WebSocketMessageType.Text, endOfMessage: false));
            }

            if (callNum == 2)
            {
                part2.CopyTo(buffer.Array!, buffer.Offset);
                return await Task.FromResult(new WebSocketReceiveResult(part2.Length, WebSocketMessageType.Text, endOfMessage: true));
            }

            taskCompletionSource.TrySetResult();
            await Task.Delay(Timeout.Infinite, token);
            throw new OperationCanceledException(token);
        };
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);

        // Trace also makes the connection log the reassembled message, so this covers the traffic
        // message for the multi-frame path as well as the reassembly itself.
        List<LogMessageEventArgs> logs = [];
        connection.LogLevel = WebDriverBiDiLogLevel.Trace;
        object logLock = new();
        connection.OnLogMessage.AddObserver((e) =>
        {
            lock (logLock)
            {
                logs.Add(e);
            }
        });
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        byte[] dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));
        await connection.StopAsync(TestContext.Current.CancellationToken);

        LogMessageEventArgs[] logSnapshot;
        lock (logLock)
        {
            logSnapshot = [.. logs];
        }

        Assert.Equal("Hello, World!", Encoding.UTF8.GetString(dataReceivedByConnection));
        Assert.Contains(logSnapshot, log => log.Level == WebDriverBiDiLogLevel.Trace && log.Message.Contains("RECV <<< Hello, World!"));
    }

    [Fact]
    public async Task TestConnectionAssemblesFragmentedMessageLargerThanInitialBuffer()
    {
        // Three frames, each the size of the receive buffer, force the pooled accumulator to
        // grow twice while assembling the message; every byte must survive both copies and the
        // completed message must be delivered as a single contiguous buffer.
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TaskCompletionSource<byte[]> receivedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource framesDeliveredTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
        };

        int frameSize = connection.BufferSize;
        byte[][] frames = new byte[3][];
        byte[] expected = new byte[frameSize * frames.Length];
        for (int frameIndex = 0; frameIndex < frames.Length; frameIndex++)
        {
            frames[frameIndex] = new byte[frameSize];
            for (int i = 0; i < frameSize; i++)
            {
                frames[frameIndex][i] = (byte)((frameIndex * 31 + i) % 256);
            }

            frames[frameIndex].CopyTo(expected, frameIndex * frameSize);
        }

        connection.ReceiveHandler = async (buffer, token, callNum) =>
        {
            if (callNum <= frames.Length)
            {
                byte[] frame = frames[callNum - 1];
                frame.CopyTo(buffer.Array!, buffer.Offset);
                return await Task.FromResult(new WebSocketReceiveResult(frame.Length, WebSocketMessageType.Binary, endOfMessage: callNum == frames.Length));
            }

            framesDeliveredTaskCompletionSource.TrySetResult();
            await Task.Delay(Timeout.Infinite, token);
            throw new OperationCanceledException(token);
        };
        connection.OnDataReceived.AddObserver(e => receivedTaskCompletionSource.TrySetResult(e.Data.ToArray()));
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));

        byte[] received = await receivedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await framesDeliveredTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(expected.Length, received.Length);
        Assert.True(received.AsSpan().SequenceEqual(expected), "Reassembled message content did not match the frames that were sent");
    }

    [Fact]
    public async Task TestConnectionDeliversNothingForPartialFragmentedMessageOnClose()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int deliveredMessageCount = 0;
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
        };
        connection.ReceiveHandler = async (buffer, token, callNum) =>
        {
            if (callNum == 1)
            {
                return await Task.FromResult(new WebSocketReceiveResult(10, WebSocketMessageType.Text, endOfMessage: false));
            }

            if (callNum == 2)
            {
                return await Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Close, endOfMessage: true));
            }

            await Task.Delay(Timeout.Infinite, token);
            throw new OperationCanceledException(token);
        };

        // The Close frame ends the receive loop, so the loop's own last log line is the point at which
        // the count below is final. Waiting for a third receive call would wait for one the loop has no
        // reason to make.
        connection.LogLevel = WebDriverBiDiLogLevel.Trace;
        connection.OnLogMessage.AddObserver(e =>
        {
            if (e.Message.StartsWith("Ending processing loop", StringComparison.Ordinal))
            {
                taskCompletionSource.TrySetResult();
            }

            return Task.CompletedTask;
        });
        connection.OnDataReceived.AddObserver(e =>
        {
            Interlocked.Increment(ref deliveredMessageCount);
            return this.OnConnectionDataReceivedAsync(e);
        });
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);

        // The partial fragment was never completed, so no message may have been delivered
        // (the connection discards the partial buffer rather than handing it on).
        Assert.Equal(0, deliveredMessageCount);
    }

    [Fact]
    public async Task TestConnectionDeliversNothingForPartialFragmentedMessageOnException()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int deliveredMessageCount = 0;
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
        };
        connection.ReceiveHandler = (buffer, token, callNum) =>
        {
            if (callNum == 1)
            {
                return Task.FromResult(new WebSocketReceiveResult(10, WebSocketMessageType.Text, endOfMessage: false));
            }

            taskCompletionSource.TrySetResult();
            throw new OperationCanceledException();
        };
        connection.OnDataReceived.AddObserver(e =>
        {
            Interlocked.Increment(ref deliveredMessageCount);
            return this.OnConnectionDataReceivedAsync(e);
        });
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);

        // The partial fragment was never completed, so no message may have been delivered
        // (the connection discards the partial buffer rather than handing it on).
        Assert.Equal(0, deliveredMessageCount);
    }

    [Fact]
    public async Task TestConnectionDeliversNothingForZeroLengthMessage()
    {
        // A frame carrying no bytes and marked as the end of a message frames a complete message with
        // no content. Nothing may be delivered for it, and it must leave the message that follows
        // untouched: an empty frame never starts an accumulation, so the next frame begins afresh.
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        List<byte[]> deliveredMessages = [];
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
        };

        byte[] message = Encoding.UTF8.GetBytes("Hello, World!");
        connection.ReceiveHandler = async (buffer, token, callNum) =>
        {
            if (callNum == 1)
            {
                return await Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Text, endOfMessage: true));
            }

            if (callNum == 2)
            {
                message.CopyTo(buffer.Array!, buffer.Offset);
                return await Task.FromResult(new WebSocketReceiveResult(message.Length, WebSocketMessageType.Text, endOfMessage: true));
            }

            await Task.Delay(Timeout.Infinite, token);
            throw new OperationCanceledException(token);
        };

        // The second frame is the deterministic completion point rather than a wall-clock wait: the
        // receive loop handles frames in order, so a delivery from the second frame proves the
        // zero-length frame ahead of it has already been processed.
        connection.OnDataReceived.AddObserver(e =>
        {
            deliveredMessages.Add(e.Data.ToArray());
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);

        // Only the frame with content produces a notification; the zero-length message produces none.
        byte[] delivered = Assert.Single(deliveredMessages);
        Assert.Equal("Hello, World!", Encoding.UTF8.GetString(delivered));
    }

    [Fact]
    public async Task TestConnectionDiscardsReceivedMessageWhenNoDataReceivedObserverIsAttached()
    {
        // With no observer on OnDataReceived there is nobody to take ownership of the message's pooled
        // memory, and notifying would drop the block rather than return it to the pool. The connection
        // must return it itself, and the receive loop must carry on normally afterwards, framing the
        // message that follows exactly as it would otherwise.
        await using Server server = this.CreateServer();
        await server.StartAsync();

        object logLock = new();
        List<LogMessageEventArgs> logs = [];
        List<ConnectionErrorEventArgs> connectionErrors = [];
        TaskCompletionSource bothMessagesProcessed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
        };

        byte[] firstMessage = Encoding.UTF8.GetBytes("Hello, World!");
        byte[] secondMessage = Encoding.UTF8.GetBytes("Goodbye, World!");
        connection.ReceiveHandler = async (buffer, token, callNum) =>
        {
            if (callNum == 1)
            {
                firstMessage.CopyTo(buffer.Array!, buffer.Offset);
                return await Task.FromResult(new WebSocketReceiveResult(firstMessage.Length, WebSocketMessageType.Text, endOfMessage: true));
            }

            if (callNum == 2)
            {
                secondMessage.CopyTo(buffer.Array!, buffer.Offset);
                return await Task.FromResult(new WebSocketReceiveResult(secondMessage.Length, WebSocketMessageType.Text, endOfMessage: true));
            }

            // A third receive call proves the loop is done with both messages and is asking for more,
            // which is the deterministic point at which the assertions below are final.
            bothMessagesProcessed.TrySetResult();
            await Task.Delay(Timeout.Infinite, token);
            throw new OperationCanceledException(token);
        };

        // No observer is added to OnDataReceived; that absence is the whole point of this test.
        // This test asserts on Trace messages, which the default minimum level excludes.
        connection.LogLevel = WebDriverBiDiLogLevel.Trace;
        connection.OnLogMessage.AddObserver(e =>
        {
            lock (logLock)
            {
                logs.Add(e);
            }

            return Task.CompletedTask;
        });
        connection.OnConnectionError.AddObserver(e =>
        {
            connectionErrors.Add(e);
            return Task.CompletedTask;
        });
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await bothMessagesProcessed.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // The send is the control for the receive assertion below: it proves Trace-level traffic logging
        // is genuinely on, so a missing RECV entry is the discard at work and not a dead log level.
        await connection.SendDataAsync("Anyone there?"u8.ToArray(), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);

        LogMessageEventArgs[] logSnapshot;
        lock (logLock)
        {
            logSnapshot = [.. logs];
        }

        Assert.Contains(logSnapshot, log => log.Level == WebDriverBiDiLogLevel.Trace && log.Message.StartsWith("SEND >>> ", StringComparison.Ordinal));
        Assert.DoesNotContain(logSnapshot, log => log.Message.StartsWith("RECV <<< ", StringComparison.Ordinal));

        // Disposing a block that was still in use, or disposing one twice, would surface here.
        Assert.Empty(connectionErrors);
    }

    [Fact]
    public async Task TestConnectionDoesNotDeliverMessageWhenTrafficLogObserverThrows()
    {
        // The received message is logged before it is handed on, so a log observer that throws takes
        // the delivery down with it: the notification never runs, and the failure travels out to the
        // receive loop, which reports a connection error and ends. The connection returns the
        // message's pooled memory before letting that failure propagate, though nothing observable
        // from here distinguishes that from leaving the block unreturned.
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TaskCompletionSource connectionErrorRaised = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int deliveredMessageCount = 0;
        List<ConnectionErrorEventArgs> connectionErrors = [];
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
        };

        byte[] message = Encoding.UTF8.GetBytes("Hello, World!");
        connection.ReceiveHandler = async (buffer, token, callNum) =>
        {
            if (callNum == 1)
            {
                message.CopyTo(buffer.Array!, buffer.Offset);
                return await Task.FromResult(new WebSocketReceiveResult(message.Length, WebSocketMessageType.Text, endOfMessage: true));
            }

            await Task.Delay(Timeout.Infinite, token);
            throw new OperationCanceledException(token);
        };

        // This test asserts on Trace messages, which the default minimum level excludes. Only the
        // traffic message throws: the loop logs its own error after the failure, and a log observer
        // that threw for every message would fail that logging too and obscure what is under test.
        connection.LogLevel = WebDriverBiDiLogLevel.Trace;
        connection.OnLogMessage.AddObserver(e =>
        {
            if (e.Message.StartsWith("RECV <<< ", StringComparison.Ordinal))
            {
                throw new WebDriverBiDiException("Simulated log observer failure");
            }

            return Task.CompletedTask;
        });
        connection.OnDataReceived.AddObserver(e =>
        {
            Interlocked.Increment(ref deliveredMessageCount);
            return Task.CompletedTask;
        });
        connection.OnConnectionError.AddObserver(e =>
        {
            connectionErrors.Add(e);
            connectionErrorRaised.TrySetResult();
            return Task.CompletedTask;
        });
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connectionErrorRaised.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);

        // The message is logged before it is delivered, so a failure to log it costs the delivery.
        Assert.Equal(0, deliveredMessageCount);
        ConnectionErrorEventArgs connectionError = Assert.Single(connectionErrors);
        Assert.Equal("Simulated log observer failure", connectionError.Exception.Message);
    }

    [Fact]
    public async Task TestSendDataThrowsWhenConnectionBecomesInactiveAfterSemaphoreAcquired()
    {
        int isActiveCallCount = 0;
        await using TestWebSocketConnection connection = new();
        await connection.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        connection.BypassStart = false;

        // Installed after the connection is started, because Connection.StartAsync refuses to start a
        // connection that already reports itself as active.
        connection.IsConnectionOpenOverride = () =>
        {
            int count = Interlocked.Increment(ref isActiveCallCount);
            return count <= 1;
        };

        WebDriverBiDiConnectionException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await connection.SendDataAsync("data"u8.ToArray(), TestContext.Current.CancellationToken));
        Assert.Equal("The WebSocket connection was closed before the send could be completed", exception.Message);
    }

    [Fact]
    public async Task TestSendDataWrapsWebSocketExceptionInConnectionException()
    {
        await using TestWebSocketConnection connection = new()
        {
            ThrowWebSocketExceptionOnSend = true,
            BypassDataSend = false,
        };
        await connection.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        connection.BypassStart = false;
        connection.IsConnectionOpenOverride = () => true;

        WebDriverBiDiConnectionException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await connection.SendDataAsync("data"u8.ToArray(), TestContext.Current.CancellationToken));
        Assert.Contains("Simulated WebSocket failure", exception.Message);
        Assert.IsType<WebSocketException>(exception.InnerException);
    }

    [Fact]
    public async Task TestStartAsyncThrowsWhenCancellationTokenIsCanceledDuringConnectionRetry()
    {
        // The first attempt is refused, so the connection enters its retry pause. The pause runs on a
        // virtual clock that is never advanced, so it stays open, and the token is canceled once the
        // pause's timer exists: while a retry is provably in progress rather than after a timed delay.
        using CancellationTokenSource cts = new();
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new(timeProvider)
        {
            BypassStart = false,
            StartupTimeout = TimeSpan.FromSeconds(5),
            ConnectWebSocketOverride = (uri, token) => Task.FromException(new WebSocketException("Simulated refused connection")),
        };

        Task startTask = connection.StartAsync("ws://127.0.0.1:1", cts.Token);

        // The first timer is the attempt's deadline and the second is the pause.
        await timeProvider.WaitForTimerCreatedAsync(1).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.False(startTask.IsCompleted);
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await startTask);
        Assert.Equal(2, timeProvider.TimerCount);
    }

    [Fact]
    public async Task TestConnectionRaisesOnRemoteDisconnectedWhenServerGracefullyCloses()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await using WebSocketConnection connection = new()
        {
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };

        // This test has failed intermittently in CI with nothing but a timeout to go on, which cannot
        // distinguish a close frame that never arrived from one whose receive faulted. Both leave the
        // event unraised, and only the second reports an error, so the connection's own account of what
        // happened is collected here and read back in the failure message.
        List<string> connectionLog = [];
        Exception? connectionError = null;
        connection.LogLevel = WebDriverBiDiLogLevel.Trace;
        connection.OnLogMessage.AddObserver(e =>
        {
            lock (connectionLog)
            {
                connectionLog.Add(e.Message);
            }

            return Task.CompletedTask;
        });
        connection.OnConnectionError.AddObserver(e =>
        {
            connectionError = e.Exception;
            return Task.CompletedTask;
        });

        ConnectionDisconnectedEventArgs? receivedEventArgs = null;
        connection.OnRemoteDisconnected.AddObserver(e =>
        {
            receivedEventArgs = e with { };
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.clientDisconnectedObserver = server.OnClientDisconnected.AddObserver(_ => { });

        await server.DisconnectAsync(registeredConnectionId);

        try
        {
            await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        }
        catch (TimeoutException)
        {
            string log;
            lock (connectionLog)
            {
                log = connectionLog.Count == 0 ? "(none)" : string.Join(" | ", connectionLog);
            }

            Assert.Fail(
                $"The server closed the connection but OnRemoteDisconnected was never raised. Connection error: {connectionError?.GetType().Name ?? "(none)"}: {connectionError?.Message ?? string.Empty}. Connection log: {log}");
        }

        Assert.NotNull(receivedEventArgs);
        await connection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestConnectionRaisesOnRemoteDisconnectedWhenCloseAcknowledgementLeavesSocketOpen()
    {
        // The remote end sends a Close frame while the socket itself has not seen one, so acknowledging it
        // leaves the socket in CloseSent rather than Closed. The remote end has gracefully closed all the
        // same, and a loop that waited for Closed would go on receiving on a connection the remote end has
        // already finished with, never reporting the disconnection at all.
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        ConnectionDisconnectedEventArgs? receivedEventArgs = null;
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
            BypassCloseClientWebSocket = false,
            ShutdownTimeout = TimeSpan.FromSeconds(1),
            ReceiveHandler = async (buffer, cancellationToken, callCount) =>
            {
                if (callCount == 1)
                {
                    return new WebSocketReceiveResult(0, WebSocketMessageType.Close, true);
                }

                // Reached only if the Close frame failed to end the loop, in which case the wait below
                // fails rather than hanging on a handler that never returns.
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                throw new OperationCanceledException(cancellationToken);
            },
        };
        connection.OnRemoteDisconnected.AddObserver(e =>
        {
            receivedEventArgs = e with { };
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));

        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.NotNull(receivedEventArgs);
        await connection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestConnectionDoesNotRaiseOnRemoteDisconnectedWhenLocallyClosed()
    {
        // A local close is completed by the server answering the close handshake, which ends the receive
        // loop exactly as a server-initiated close does. The event must still not be raised: the remote end
        // did not disconnect us.
        await using Server server = this.CreateServer();
        await server.StartAsync();

        int remoteDisconnectedCount = 0;
        await using WebSocketConnection connection = new()
        {
            ShutdownTimeout = TimeSpan.FromSeconds(5),
        };
        connection.OnRemoteDisconnected.AddObserver(e =>
        {
            Interlocked.Increment(ref remoteDisconnectedCount);
            return Task.CompletedTask;
        });

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));

        // StopAsync waits for the receive loop to finish, so by the time it returns the loop has passed
        // its graceful-exit check and would already have raised the event if it were going to.
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(0, Interlocked.CompareExchange(ref remoteDisconnectedCount, 0, 0));
    }

    [Fact]
    public async Task TestConnectionRaisesOnRemoteDisconnectedAfterRestartFollowingLocalClose()
    {
        // The local-close state must not leak into the next session: a server-initiated close after a
        // restart is still a remote disconnection.
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await using WebSocketConnection connection = new()
        {
            ShutdownTimeout = TimeSpan.FromSeconds(5),
        };
        connection.OnRemoteDisconnected.AddObserver(e =>
        {
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.StopAsync(TestContext.Current.CancellationToken);

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.clientDisconnectedObserver = server.OnClientDisconnected.AddObserver(_ => { });

        await server.DisconnectAsync(registeredConnectionId);

        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestAllLogOutputsAreProduced()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<string> connectionLog = [];
        await using WebSocketConnection connection = new();
        connection.OnLogMessage.AddObserver(e =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.Contains(connectionLog, s => s.StartsWith("Opening WebSocket connection to "));
        Assert.Contains("WebSocket connection opened", connectionLog);
        Assert.Contains("Closing WebSocket connection", connectionLog);
        Assert.Contains(connectionLog, s => s.StartsWith("Client state is "));
        Assert.Contains(connectionLog, s => s.StartsWith("Ending processing loop in state "));
    }

    [Fact]
    public async Task TestSendDataThrowsWhenCancellationTokenIsCanceled()
    {
        await using TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            IsConnectionOpenOverride = () => true,
        };
        using CancellationTokenSource cts = new();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("test"), cts.Token));
    }

    [Fact]
    public async Task TestSendDataWithDefaultCancellationTokenUsesConnectionToken()
    {
        await using TestWebSocketConnection connection = new();
        await connection.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        connection.BypassStart = false;
        connection.IsConnectionOpenOverride = () => true;

        byte[] payload = """{"id":1,"method":"session.new","params":{}}"""u8.ToArray();
#pragma warning disable xUnit1051 // intentionally omits token to exercise the CancellationToken.None branch
        await connection.SendDataAsync(payload);
#pragma warning restore xUnit1051
        Assert.Equal("""{"id":1,"method":"session.new","params":{}}""", connection.DataSent);

        // The name of this test is a claim about which token the send used, so assert it: with no
        // caller token there is nothing to link, and the connection's own token is passed straight
        // down. Previously the test asserted only that the payload arrived.
        Assert.Equal(connection.ObservedConnectionCancellationToken, connection.LastSendCancellationToken);
    }

    private Server CreateServer()
    {
        Server server = new();
        server.OnClientConnected.AddObserver(this.OnClientConnected);
        return server;
    }

    private void OnSocketDataReceived(ServerDataReceivedEventArgs e)
    {
        this.lastServerReceivedData = e.Data;
        this.serverReceiveSyncEvent.Set();
    }

    private Task OnConnectionDataReceivedAsync(ConnectionDataReceivedEventArgs e)
    {
        this.lastConnectionReceivedData = e.Data;
        this.connectionReceiveSyncEvent.Set();
        return Task.CompletedTask;
    }

    private void OnClientConnected(ClientConnectionEventArgs e)
    {
        this.connectionId = e.ConnectionId;
        this.connectionSyncEvent.Set();
    }

    // The three waits below assert that the thing waited for actually happened. Returning the
    // last recorded value on a timeout instead would hand the caller state left over from an
    // earlier step -- the previous session's payload in the connection-reuse tests, or the
    // initial empty value elsewhere -- so a wait that expired would surface as a value
    // comparison failing for reasons that have nothing to do with the values, rather than as
    // the timeout it is.
    private string WaitForServerToRegisterConnection(TimeSpan timeout)
    {
        Assert.True(this.connectionSyncEvent.WaitOne(timeout), $"Server did not register a client connection within {timeout.TotalMilliseconds} ms.");
        return this.connectionId;
    }

    private byte[] WaitForConnectionToReceiveData(TimeSpan timeout)
    {
        Assert.True(this.connectionReceiveSyncEvent.WaitOne(timeout), $"Connection did not receive data within {timeout.TotalMilliseconds} ms.");
        return this.lastConnectionReceivedData.ToArray();
    }

    private string WaitForServerToReceiveData(TimeSpan timeout)
    {
        Assert.True(this.serverReceiveSyncEvent.WaitOne(timeout), $"Server did not receive data within {timeout.TotalMilliseconds} ms.");
        return this.lastServerReceivedData;
    }
}
