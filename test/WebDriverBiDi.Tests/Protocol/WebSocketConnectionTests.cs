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
    private byte[] lastConnectionReceivedData = [];
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
        this.lastConnectionReceivedData = [];
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
        Assert.Equal(ConnectionType.WebSocket, connection.ConnectionType);
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
        Assert.Contains($"{0.05} seconds", (await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await connection.StartAsync($"ws://localhost:{port}", TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestConnectionCanSendData()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new();
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.serverDataReceivedObserver = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("Hello world"u8.ToArray(), TestContext.Current.CancellationToken);
        this.WaitForServerToReceiveData(TimeSpan.FromSeconds(4));
        await server.SendWebSocketDataAsync(registeredConnectionId, "Hello back");
        this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(4));
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.Contains(allLogs,
            e => e.Message.StartsWith("SEND >>> ") && e.Level == WebDriverBiDiLogLevel.Debug);
        Assert.Contains(allLogs,
            e => e.Message.StartsWith("RECV <<< ") && e.Level == WebDriverBiDiLogLevel.Debug);
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
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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

        string serverWebSocketUrl = $"ws://localhost:{server.Port}";
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
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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
            $"Opening connection to URL ws://localhost:{server.Port}",
            "Connection opened",
            "Closing connection",
            "Unexpected error during receive of data: The remote party closed the WebSocket connection without completing the close handshake.",
            "Client state is Aborted"
        ];

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnConnectionError.AddObserver(e =>
        {
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        List<string> connectionLog = [];
        connection.OnLogMessage.AddObserver(e =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await server.StopAsync();

        // Wait for the client's receive loop to detect the abrupt close (WebSocketException)
        // before calling StopAsync. This ensures client.State is Aborted when StopAsync runs,
        // so we take the early-exit path and avoid calling CloseOutputAsync on an Aborted socket.
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equivalent(expectedLogEntries, connectionLog);
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
            $"Opening connection to URL ws://localhost:{server.Port}",
            "Connection opened",
            "Closing connection",
            "Client state is CloseSent"  // We send close frame; server may not respond before timeout
        ];

        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
            BypassCloseClientWebSocket = false,
            StartupTimeout = TimeSpan.FromSeconds(1),
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

        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));

        // Receive loop is blocked in ReceiveHandler; client.State is still Open.
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equivalent(expectedLogEntries, connectionLog);
    }

    [Fact]
    public async Task TestConnectionInitiateWebSocketClose()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<string> expectedLogEntries =
        [
            $"Opening connection to URL ws://localhost:{server.Port}",
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
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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
            $"Opening connection to URL ws://localhost:{server.Port}",
            "Connection opened",
            "Acknowledging Close frame received from server (client state: CloseReceived)",
            "Ending processing loop in state Closed",
            "Closing connection",
            "Client state is Closed"
        ];

        List<string> connectionLog = [];
        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnLogMessage.AddObserver(e =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });

        IReadOnlyList<string> serverLog = server.Log;
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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
            $"Opening connection to URL ws://localhost:{server.Port}",
            "Connection opened",
            "Closing connection",
            "Unexpected error during receive of data: The remote party closed the WebSocket connection without completing the close handshake.",
            "Client state is Aborted"
        ];

        List<string> connectionLog = [];
        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnLogMessage.AddObserver(e =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });

        IReadOnlyList<string> serverLog = server.Log;
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnConnectionError.AddObserver(e =>
        {
            receivedErrorArgs = e;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);

        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        ServerEventObserver<ServerDataReceivedEventArgs> observer = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("First connection hello"u8.ToArray(), TestContext.Current.CancellationToken);
        string serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromMilliseconds(250));
        observer.Unobserve();
        Assert.Equal("First connection hello", serverReceivedData);

        await server.SendWebSocketDataAsync(registeredConnectionId, "First connection acknowledged");
        byte[] receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromMilliseconds(250));
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equal("First connection acknowledged"u8.ToArray(), receivedData);

        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        observer = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("Second connection hello"u8.ToArray(), TestContext.Current.CancellationToken);
        serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromMilliseconds(250));
        observer.Unobserve();
        Assert.Equal("Second connection hello", serverReceivedData);

        await server.SendWebSocketDataAsync(registeredConnectionId, "Second connection acknowledged");
        receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromMilliseconds(250));
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
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);

        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        ServerEventObserver<ServerDataReceivedEventArgs> observer = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("First connection hello"u8.ToArray(), TestContext.Current.CancellationToken);
        string serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromMilliseconds(250));
        observer.Unobserve();
        Assert.Equal("First connection hello", serverReceivedData);

        await server.SendWebSocketDataAsync(registeredConnectionId, "First connection acknowledged");
        byte[] receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromMilliseconds(250));
        server.IgnoreCloseConnectionRequest(registeredConnectionId, true);
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equal("First connection acknowledged"u8.ToArray(), receivedData);

        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        observer = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("Second connection hello"u8.ToArray(), TestContext.Current.CancellationToken);
        serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromMilliseconds(250));
        observer.Unobserve();
        Assert.Equal("Second connection hello", serverReceivedData);

        await server.SendWebSocketDataAsync(registeredConnectionId, "Second connection acknowledged");
        receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromMilliseconds(250));
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equal("Second connection acknowledged"u8.ToArray(), receivedData);
    }

    [Fact]
    public async Task TestCannotStartAlreadyStartedConnection()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.StartsWith($"The WebSocket is already connected to ws://localhost:{server.Port}", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken))).Message);
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

        List<string> connectionLog = [];
        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.Zero,
        };
        connection.OnLogMessage.AddObserver(e =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });

        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        server.IgnoreCloseConnectionRequest(registeredConnectionId, true);
        await connection.StopAsync(TestContext.Current.CancellationToken);

        // With ShutdownTimeout=Zero, CloseClientWebSocketAsync may throw OperationCanceledException
        // before logging "Client state is X". At minimum we get "Closing connection".
        Assert.Contains("Closing connection", connectionLog);
        Assert.True(connectionLog.Count >= 1);
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
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);

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
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
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
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.serverDataReceivedObserver = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("Hello world"u8.ToArray(), TestContext.Current.CancellationToken);
        this.WaitForServerToReceiveData(TimeSpan.FromSeconds(3));

        await connection.StopAsync(TestContext.Current.CancellationToken);
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task TestConnectionDiscardsPartialFragmentedMessageOnClose()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
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
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestConnectionCleansUpPartialFragmentedMessageOnException()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
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
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);
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
            SendWebSocketDataOverride = _ => throw new WebSocketException("Simulated WebSocket failure"),
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

        Task startTask = connection.StartAsync($"ws://localhost:{port}", cts.Token);
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
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        ConnectionDisconnectedEventArgs? receivedEventArgs = null;
        connection.OnRemoteDisconnected.AddObserver(e =>
        {
            receivedEventArgs = e with { };
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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

        await connection.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
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

    private string WaitForServerToRegisterConnection(TimeSpan timeout)
    {
        this.connectionSyncEvent.WaitOne(timeout);
        return this.connectionId;
    }

    private byte[] WaitForConnectionToReceiveData(TimeSpan timeout)
    {
        this.connectionReceiveSyncEvent.WaitOne(timeout);
        return this.lastConnectionReceivedData;
    }

    private string WaitForServerToReceiveData(TimeSpan timeout)
    {
        this.serverReceiveSyncEvent.WaitOne(timeout);
        return this.lastServerReceivedData;
    }
}
