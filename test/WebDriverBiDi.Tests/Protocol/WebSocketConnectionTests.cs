namespace WebDriverBiDi.Protocol;

using System.Diagnostics;
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
        WebSocketConnection connection = new();
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

        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromMilliseconds(50)
        };
        Assert.Contains($"{0.05} seconds", (await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await connection.StartAsync($"ws://127.0.0.1:{port}", TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestConnectionStartupTimeoutBoundsHangingConnectAttempt()
    {
        // ClientWebSocket has no connect timeout of its own. A remote end that accepts the TCP
        // connection but never completes the handshake (or a black-holed host) must not hold
        // StartAsync open past StartupTimeout, so each attempt is bounded by the remaining budget.
        TimeSpan startupTimeout = TimeSpan.FromMilliseconds(200);
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            StartupTimeout = startupTimeout,
            ConnectWebSocketOverride = (uri, token) => Task.Delay(Timeout.InfiniteTimeSpan, token),
        };

        Stopwatch stopwatch = Stopwatch.StartNew();
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await connection.StartAsync("ws://127.0.0.1:1", TestContext.Current.CancellationToken));
        stopwatch.Stop();

        Assert.Contains($"{0.2} seconds", exception.Message);

        // The per-attempt deadline is a timer, which may fire a few milliseconds before this
        // test's stopwatch reads the full timeout, so allow a small tolerance on the lower bound.
        // The important assertion is the upper bound: the hanging attempt must not outlive the timeout.
        TimeSpan lowerBound = startupTimeout - TimeSpan.FromMilliseconds(50);
        Assert.True(stopwatch.Elapsed >= lowerBound, $"StartAsync returned after {stopwatch.Elapsed}, well before the startup timeout elapsed");
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(10), $"StartAsync took {stopwatch.Elapsed}; the hanging connect attempt was not bounded by StartupTimeout");
        Assert.False(connection.IsActive);
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
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            StartupTimeout = startupTimeout,
            ConnectWebSocketOverride = async (uri, token) =>
            {
                Interlocked.Increment(ref attemptCount);

                // Deliberately ignores the attempt's own cancellation token, so the attempt
                // outlives the startup budget and then fails the way a remote end that is not
                // listening does, rather than being canceled by its deadline.
                await Task.Delay(attemptDuration, TestContext.Current.CancellationToken);
                throw new WebSocketException("Simulated connection failure after the startup budget elapsed");
            },
        };

        Stopwatch stopwatch = Stopwatch.StartNew();
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(
            async () => await connection.StartAsync("ws://127.0.0.1:1", TestContext.Current.CancellationToken));
        stopwatch.Stop();

        Assert.Contains($"{0.1} seconds", exception.Message);
        Assert.Equal(1, attemptCount);

        // The attempt itself takes 150ms. An unclamped retry pause would add a further 500ms
        // before the loop noticed the budget was gone, so the bound below separates the two
        // outcomes with generous room for a slow machine on either side.
        Assert.True(
            stopwatch.Elapsed < TimeSpan.FromMilliseconds(450),
            $"StartAsync took {stopwatch.Elapsed}; the retry pause was not skipped once the startup budget was exhausted");
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestConnectionWithZeroStartupTimeoutTimesOutBeforeAttemptingToConnect()
    {
        // A zero startup budget leaves no time for even a first attempt.
        int attemptCount = 0;
        TestWebSocketConnection connection = new()
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

        WebSocketConnection connection = new();
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

        WebSocketConnection connection = new();
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

        WebSocketConnection connection = new();
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

        WebSocketConnection connection = new();
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
        WebSocketConnection connection = new();
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
        WebSocketConnection connection = new();
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

        Assert.Equal(5, logValues.Count);
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

        WebSocketConnection connection = new();
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
        WebSocketConnection connection = new();
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
        WebSocketConnection connection = new();
        Assert.False(connection.IsActive);
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestStopWithoutStartLogsClientStateNone()
    {
        List<string> connectionLog = [];
        WebSocketConnection connection = new();
        connection.OnLogMessage.AddObserver(e =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });
        await connection.StopAsync(TestContext.Current.CancellationToken);

        List<string> expectedLogEntries =
        [
            "Closing connection",
            "Client state is None"
        ];
        Assert.Equivalent(expectedLogEntries, connectionLog);
    }

    [Fact]
    public async Task TestStopForcesCancellationOfDataReceiveTask()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TestWebSocketConnection connection = new()
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
        WebSocketConnection connection = new();
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
        Assert.Equal(2, connectionLog.Count(item => item == "Closing connection"));
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
            $"Opening connection to URL ws://127.0.0.1:{server.Port}",
            "Connection opened",
            "Closing connection",
            "Unexpected error during receive of data: The remote party closed the WebSocket connection without completing the close handshake.",
            "Ending processing loop in state Aborted",
            "Client state is Aborted"
        ];

        object logLock = new();
        List<string> connectionLog = [];
        TaskCompletionSource receiveLoopEnded = new(TaskCreationOptions.RunContinuationsAsynchronously);
        WebSocketConnection connection = new()
        {
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnLogMessage.AddObserver(e =>
        {
            lock (logLock)
            {
                connectionLog.Add(e.Message);
            }

            if (e.Message == "Ending processing loop in state Aborted")
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

        string[] logSnapshot;
        lock (logLock)
        {
            logSnapshot = [.. connectionLog];
        }

        Assert.Equivalent(expectedLogEntries, logSnapshot);
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
        WebSocketConnection connection = new();
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
    public async Task TestConnectionStopWhileReceiveBlocked()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        // This test deterministically exercises the path where StopAsync is called while the
        // socket is still Open, so CloseClientWebSocketAsync runs (not the early-exit path).
        // The ReceiveHandler blocks until cancellation, keeping client.State == Open.
        List<string> expectedLogEntries =
        [
            $"Opening connection to URL ws://127.0.0.1:{server.Port}",
            "Connection opened",
            "Closing connection",
            "Client state is CloseSent",  // We send close frame; server may not respond before timeout
            "Ending processing loop in state CloseSent"
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

        Assert.Equivalent(expectedLogEntries, connectionLog);
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

        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
            BypassCloseClientWebSocket = true,
            ShutdownTimeout = TimeSpan.FromMilliseconds(500),
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

            Task stopTask = connection.StopAsync(TestContext.Current.CancellationToken);
            Task settledTask = await Task.WhenAny(stopTask, Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
            if (settledTask != stopTask)
            {
                Assert.Fail("StopAsync did not return within 5 seconds; the wait for the receive loop is not bounded by ShutdownTimeout.");
            }

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
            $"Opening connection to URL ws://127.0.0.1:{server.Port}",
            "Connection opened",
            "Closing connection",
            "Ending processing loop in state Closed",
            "Client state is Closed"
        ];

        List<string> connectionLog = [];
        WebSocketConnection connection = new();
        connection.OnLogMessage.AddObserver(e =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equivalent(expectedLogEntries, connectionLog);
    }

    [Fact]
    public async Task TestConnectionHandlesDisconnectInitiatedByRemoteEnd()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<string> expectedLogEntries =
        [
            $"Opening connection to URL ws://127.0.0.1:{server.Port}",
            "Connection opened",
            "Acknowledging Close frame received from server (client state: CloseReceived)",
            "Ending processing loop in state Closed",
            "Closing connection",
            "Client state is Closed"
        ];

        List<string> connectionLog = [];
        WebSocketConnection connection = new()
        {
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnLogMessage.AddObserver(e =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });

        IReadOnlyList<string> serverLog = server.Log;
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.OnRemoteDisconnected.AddObserver(e =>
        {
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        // Server initiated disconnection requires waiting for the client's receive
        // loop to complete (OnRemoteDisconnected fires after "Ending processing loop"
        // is logged), so that StopAsync does not race ahead of that log entry.
        await server.DisconnectAsync(registeredConnectionId);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equivalent(expectedLogEntries, connectionLog);
    }

    [Fact]
    public async Task TestConnectionHandlesHungRemoteEnd()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<string> expectedLogEntries =
        [
            $"Opening connection to URL ws://127.0.0.1:{server.Port}",
            "Connection opened",
            "Closing connection",
            "Unexpected error during receive of data: The remote party closed the WebSocket connection without completing the close handshake.",
            "Ending processing loop in state Aborted",
            "Client state is Aborted"
        ];

        List<string> connectionLog = [];
        WebSocketConnection connection = new();
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
        Assert.Equivalent(expectedLogEntries, connectionLog);
    }

    [Fact]
    public async Task TestConnectionRaisesErrorEventOnWebSocketException()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        ConnectionErrorEventArgs? receivedErrorArgs = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        WebSocketConnection connection = new()
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

        WebSocketConnection connection = new()
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

        WebSocketConnection connection = new()
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

        WebSocketConnection connection = new()
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
        // A failed connection attempt leaves the ClientWebSocket in the None state, so the branch
        // in StartAsync that replaces a Closed or Aborted socket does not run. The caller's
        // cleanup StopAsync still cancels the connection's CancellationTokenSource, so unless
        // StartAsync resets that source on every start, the retry below would fail immediately
        // with a TaskCanceledException and the connection could never be used again.
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

        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromMilliseconds(200),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);

        _ = await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(
            async () => await connection.StartAsync($"ws://127.0.0.1:{deadPort}", TestContext.Current.CancellationToken));

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
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equal("Acknowledged after failed attempt"u8.ToArray(), receivedData);
    }

    [Fact]
    public async Task TestCannotStartAlreadyStartedConnection()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new()
        {
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.StartsWith($"The WebSocket is already connected to ws://127.0.0.1:{server.Port}", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestStartAsyncThrowsForInvalidUrl()
    {
        WebSocketConnection connection = new();
        Assert.Contains("not a valid absolute URI", (await Assert.ThrowsAnyAsync<ArgumentException>(async () => await connection.StartAsync("not-a-valid-url", TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestStartAsyncThrowsForNonWebSocketUrl()
    {
        WebSocketConnection connection = new();
        Assert.Contains("The URI scheme must be 'ws' or 'wss'; received 'http'", (await Assert.ThrowsAnyAsync<ArgumentException>(async () => await connection.StartAsync("http://localhost:8080", TestContext.Current.CancellationToken))).Message);
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

        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromMilliseconds(10),
        };

        // We expect this to fail with a timeout, but it verifies that the connection
        // attempts to connect to the correct URL and that the URL is accepted as valid.
        await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await connection.StartAsync($"wss://localhost:{server.Port}", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestCannotSendDataOnAConnectionNotYetStarted()
    {
        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        Assert.StartsWith($"The WebSocket has not been initialized", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await connection.SendDataAsync("This send should fail"u8.ToArray(), TestContext.Current.CancellationToken))).Message);
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
        WebSocketConnection connection = new()
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
        // before logging "Client state is X". At minimum we get "Closing connection".
        Assert.Contains("Closing connection", logSnapshot);
        Assert.True(logSnapshot.Length >= 1);
    }

    [Fact]
    public async Task TestDataSendOperationsAreSynchronized()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TaskCompletionSource sendBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
            BypassDataSend = false,
            SendBarrier = sendBarrier,
            DataTimeout = TimeSpan.FromMilliseconds(20),
        };
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.OnDataSendStarting.AddObserver(e => taskCompletionSource.TrySetResult());

        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Task firstSendTask = Task.Run(() => connection.SendDataAsync("first data"u8.ToArray(), TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);

        // Wait until the first send has acquired the semaphore and is blocked on the barrier,
        // then attempt a second send which must time out before the barrier releases.
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal("Timed out waiting to access WebSocket for sending; only one send operation is permitted at a time.", (await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await connection.SendDataAsync("second data"u8.ToArray(), TestContext.Current.CancellationToken))).Message);
        sendBarrier.SetResult();
        await connection.StopAsync(TestContext.Current.CancellationToken);

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
        connection.OnLogMessage.AddObserver((e) => { });
        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        byte[] dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal("Hello, World!", Encoding.UTF8.GetString(dataReceivedByConnection));
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

            taskCompletionSource.TrySetResult();
            await Task.Delay(Timeout.Infinite, token);
            throw new OperationCanceledException(token);
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
    public async Task TestSendDataThrowsWhenConnectionBecomesInactiveAfterSemaphoreAcquired()
    {
        int isActiveCallCount = 0;
        TestWebSocketConnection connection = new()
        {
            IsActiveOverride = () =>
            {
                int count = Interlocked.Increment(ref isActiveCallCount);
                return count <= 1;
            },
        };
        await connection.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        connection.BypassStart = false;

        WebDriverBiDiConnectionException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await connection.SendDataAsync("data"u8.ToArray(), TestContext.Current.CancellationToken));
        Assert.Equal("The WebSocket connection was closed before the send could be completed", exception.Message);
    }

    [Fact]
    public async Task TestSendDataWrapsWebSocketExceptionInConnectionException()
    {
        TestWebSocketConnection connection = new()
        {
            IsActiveOverride = () => true,
            ThrowWebSocketExceptionOnSend = true,
            BypassDataSend = false,
        };
        await connection.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        connection.BypassStart = false;

        WebDriverBiDiConnectionException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await connection.SendDataAsync("data"u8.ToArray(), TestContext.Current.CancellationToken));
        Assert.Contains("Simulated WebSocket failure", exception.Message);
        Assert.IsType<WebSocketException>(exception.InnerException);
    }

    [Fact]
    public async Task TestStartAsyncThrowsWhenCancellationTokenIsCanceledDuringConnectionRetry()
    {
        int port;
        using (TcpListener portFinder = new(IPAddress.Loopback, 0))
        {
            portFinder.Start();
            port = ((IPEndPoint)portFinder.LocalEndpoint).Port;
            portFinder.Stop();
        }

        using CancellationTokenSource cts = new();
        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(5),
        };

        Task startTask = connection.StartAsync($"ws://127.0.0.1:{port}", cts.Token);
        await Task.Delay(TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken);
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await startTask);
    }

    [Fact]
    public async Task TestConnectionRaisesOnRemoteDisconnectedWhenServerGracefullyCloses()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        WebSocketConnection connection = new()
        {
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
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

        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.NotNull(receivedEventArgs);
        await connection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestAllLogOutputsAreProduced()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<string> connectionLog = [];
        WebSocketConnection connection = new();
        connection.OnLogMessage.AddObserver(e =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);

        await connection.StartAsync($"ws://127.0.0.1:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.Contains(connectionLog, s => s.StartsWith("Opening connection to URL "));
        Assert.Contains("Connection opened", connectionLog);
        Assert.Contains("Closing connection", connectionLog);
        Assert.Contains(connectionLog, s => s.StartsWith("Client state is "));
        Assert.Contains(connectionLog, s => s.StartsWith("Ending processing loop in state "));
    }

    [Fact]
    public async Task TestSendDataThrowsWhenCancellationTokenIsCanceled()
    {
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            IsActiveOverride = () => true,
        };
        using CancellationTokenSource cts = new();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("test"), cts.Token));
    }

    [Fact]
    public async Task TestSendDataWithDefaultCancellationTokenUsesConnectionToken()
    {
        TestWebSocketConnection connection = new()
        {
            IsActiveOverride = () => true,
        };
        await connection.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        connection.BypassStart = false;

        byte[] payload = """{"id":1,"method":"session.new","params":{}}"""u8.ToArray();
#pragma warning disable xUnit1051 // intentionally omits token to exercise the CancellationToken.None branch
        await connection.SendDataAsync(payload);
#pragma warning restore xUnit1051
        Assert.Equal("""{"id":1,"method":"session.new","params":{}}""", connection.DataSent);
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
