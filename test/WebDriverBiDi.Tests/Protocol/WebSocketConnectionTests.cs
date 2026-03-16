namespace WebDriverBiDi.Protocol;

using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using PinchHitter;
using WebDriverBiDi.TestUtilities;

[TestFixture]
public class WebSocketConnectionTests
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

    [SetUp]
    public async Task InitializeServer()
    {
        this.connectionId = string.Empty;
        this.lastServerReceivedData = string.Empty;
        this.lastConnectionReceivedData = [];
        this.connectionReceiveSyncEvent.Reset();
        this.serverReceiveSyncEvent.Reset();
        this.connectionSyncEvent.Reset();
    }

    [TearDown]
    public async Task DisposeServerAsync()
    {
        this.serverDataReceivedObserver?.Unobserve();
        this.serverDataReceivedObserver = null;

        this.clientConnectedObserver?.Unobserve();
        this.clientConnectedObserver = null;

        this.clientDisconnectedObserver?.Unobserve();
        this.clientDisconnectedObserver = null;
    }

    [Test]
    public void TestConnectionType()
    {
        WebSocketConnection connection = new();
        Assert.That(connection.ConnectionType, Is.EqualTo(ConnectionType.WebSocket));
    }

    [Test]
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
            StartupTimeout = TimeSpan.FromMilliseconds(250)
        };
        Assert.That(async () => await connection.StartAsync($"ws://localhost:{port}"), Throws.InstanceOf<WebDriverBiDiTimeoutException>().With.Message.Contains($"{0.25} seconds"));
    }

    [Test]
    public async Task TestConnectionCanSendData()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new();
        await connection.StartAsync($"ws://localhost:{server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.serverDataReceivedObserver = server.OnDataReceived.AddObserver(OnSocketDataReceived);

        await connection.SendDataAsync("Hello world"u8.ToArray());
        string dataReceivedByServer = this.WaitForServerToReceiveData(TimeSpan.FromSeconds(3));

        Assert.That(dataReceivedByServer, Is.EqualTo("Hello world"));
        await connection.StopAsync();
    }

    [Test]
    public async Task TestConnectionCanReceiveData()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new();
        await connection.StartAsync($"ws://localhost:{server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);

        await server.SendWebSocketDataAsync(registeredConnectionId, "Hello back");
        byte[] dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));

        Assert.That(dataReceivedByConnection, Is.EqualTo("Hello back"u8.ToArray()));
        await connection.StopAsync();
    }

    [Test]
    public async Task TestConnectionReceivesDataOnBufferBoundary()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new();
        await connection.StartAsync($"ws://localhost:{server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);

        // Create a message on an exact boundary of the buffer
        string data = new('a', 2 * connection.BufferSize);
        await server.SendWebSocketDataAsync(registeredConnectionId, data);
        byte[] dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));

        Assert.That(dataReceivedByConnection, Is.EqualTo(Encoding.UTF8.GetBytes(data)));
        await connection.StopAsync();
    }

    [Test]
    public async Task TestConnectionReceivesDataOnVeryLongMessage()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new();
        await connection.StartAsync($"ws://localhost:{server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);

        // Create a message on an exact boundary of the buffer
        string data = new('a', 70000);
        await server.SendWebSocketDataAsync(registeredConnectionId, data);
        byte[] dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));

        Assert.That(dataReceivedByConnection, Is.EqualTo(Encoding.UTF8.GetBytes(data)));
        await connection.StopAsync();
    }

    [Test]
    public async Task TestConnectionLogIncludesSendAndRecvDebugMessages()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<LogMessageEventArgs> allLogs = [];
        WebSocketConnection connection = new();
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);
        connection.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
        {
            allLogs.Add(e);
            return Task.CompletedTask;
        });
        await connection.StartAsync($"ws://localhost:{server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.serverDataReceivedObserver = server.OnDataReceived.AddObserver(OnSocketDataReceived);

        await connection.SendDataAsync("Hello world"u8.ToArray());
        this.WaitForServerToReceiveData(TimeSpan.FromSeconds(4));
        await server.SendWebSocketDataAsync(registeredConnectionId, "Hello back");
        this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(4));
        await connection.StopAsync();

        Assert.That(allLogs, Has.Some.Matches<LogMessageEventArgs>(
            e => e.Message.StartsWith("SEND >>> ") && e.Level == WebDriverBiDiLogLevel.Debug));
        Assert.That(allLogs, Has.Some.Matches<LogMessageEventArgs>(
            e => e.Message.StartsWith("RECV <<< ") && e.Level == WebDriverBiDiLogLevel.Debug));
    }

    [Test]
    public async Task TestConnectionLog()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<LogMessageEventArgs> logValues = [];
        WebSocketConnection connection = new();
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);
        connection.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
        {
            if (e.Level >= WebDriverBiDiLogLevel.Info)
            {
                logValues.Add(e);
            }

            return Task.CompletedTask;
        });
        await connection.StartAsync($"ws://localhost:{server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.serverDataReceivedObserver = server.OnDataReceived.AddObserver(OnSocketDataReceived);
        await connection.SendDataAsync("Hello world"u8.ToArray());
        this.WaitForServerToReceiveData(TimeSpan.FromSeconds(4));

        await server.SendWebSocketDataAsync(registeredConnectionId, "Hello back");
        this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(4));
        await connection.StopAsync();

        List<string> messages = [];
        foreach (LogMessageEventArgs logValue in logValues)
        {
            messages.Add(logValue.Message);
        }

        Assert.That(logValues, Has.Count.EqualTo(5), $"Actual values: {string.Join("\n", messages.ToArray())}");
        foreach (LogMessageEventArgs args in logValues)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(args.Level, Is.EqualTo(WebDriverBiDiLogLevel.Info));
                Assert.That(args.Message, Is.Not.Null);
            }
        }
    }

    [Test]
    public async Task TestIsActiveProperty()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new();
        Assert.That(connection.IsActive, Is.False);
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://localhost:{server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.That(connection.IsActive, Is.True);
        await connection.StopAsync();
        Assert.That(connection.IsActive, Is.False);
    }

    [Test]
    public async Task TestUrlProperty()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        string serverWebSocketUrl = $"ws://localhost:{server.Port}";
        WebSocketConnection connection = new();
        Assert.That(connection.ConnectionString, Is.EqualTo(string.Empty));
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);
        await connection.StartAsync(serverWebSocketUrl);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.That(connection.ConnectionString, Is.EqualTo(serverWebSocketUrl));
        await connection.StopAsync();
        Assert.That(connection.ConnectionString, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task TestStopWithoutStart()
    {
        WebSocketConnection connection = new();
        Assert.That(connection.IsActive, Is.False);
        await connection.StopAsync();
        Assert.That(connection.IsActive, Is.False);
    }

    [Test]
    public async Task TestStopWithoutStartLogsClientStateNone()
    {
        List<string> connectionLog = [];
        WebSocketConnection connection = new();
        connection.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });
        await connection.StopAsync();

        List<string> expectedLogEntries =
        [
            "Closing connection",
            "Client state is None"
        ];
        Assert.That(connectionLog, Is.EquivalentTo(expectedLogEntries));
    }

    [Test]
    public async Task TestStopForcesCancellationOfDataReceiveTask()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false
        };
        Assert.That(connection.IsActive, Is.False);
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://localhost:{server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.That(connection.IsActive, Is.True);

        // Send data to the connection, which should force the receive data
        // task to enter a waiting state after receiving the first message.
        await server.SendWebSocketDataAsync(registeredConnectionId, "Hello back");
        byte[] dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));
        await connection.StopAsync();
        Assert.That(connection.IsActive, Is.False);
    }

    [Test]
    public async Task TestConnectionStopCanBeCalledMultipleTimes()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<string> connectionLog = [];
        WebSocketConnection connection = new();
        connection.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://localhost:{server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.StopAsync();
        await connection.StopAsync();

        // First call: socket Open -> CloseClientWebSocketAsync -> "Client state is Closed"
        // Second call: socket Closed -> early-exit -> "Client state is Closed"
        // Also: "Ending processing loop in state Closed" from receive loop
        Assert.That(connectionLog, Has.Exactly(2).EqualTo("Closing connection"));
        Assert.That(connectionLog, Has.Exactly(2).EqualTo("Client state is Closed"));
        Assert.That(connectionLog, Has.Some.EqualTo("Ending processing loop in state Closed"));
    }

    [Test]
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

        ManualResetEvent clientDetectedErrorEvent = new(false);
        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnConnectionError.AddObserver((ConnectionErrorEventArgs e) =>
        {
            clientDetectedErrorEvent.Set();
            return Task.CompletedTask;
        });
        List<string> connectionLog = [];
        connection.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });
        await connection.StartAsync($"ws://localhost:{server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await server.StopAsync();

        // Wait for the client's receive loop to detect the abrupt close (WebSocketException)
        // before calling StopAsync. This ensures client.State is Aborted when StopAsync runs,
        // so we take the early-exit path and avoid calling CloseOutputAsync on an Aborted socket.
        bool clientDetectedError = clientDetectedErrorEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(clientDetectedError, Is.True, "Expected the client to detect the remote end stop within 1 second.");

        await connection.StopAsync();
        Assert.That(connectionLog, Is.EquivalentTo(expectedLogEntries));
    }

    [Test]
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
            StartupTimeout = TimeSpan.FromMilliseconds(250),
            ShutdownTimeout = TimeSpan.FromMilliseconds(250),
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
        connection.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });

        await connection.StartAsync($"ws://localhost:{server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));

        // Receive loop is blocked in ReceiveHandler; client.State is still Open.
        await connection.StopAsync();

        Assert.That(connectionLog, Is.EquivalentTo(expectedLogEntries));
    }

    [Test]
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
        connection.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://localhost:{server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.StopAsync();
        Assert.That(connectionLog, Is.EquivalentTo(expectedLogEntries));
    }

    [Test]
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
        connection.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });

        IReadOnlyList<string> serverLog = server.Log;
        await connection.StartAsync($"ws://localhost:{server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        ManualResetEvent disconnectEvent = new(false);
        this.clientDisconnectedObserver = server.OnClientDisconnected.AddObserver((e) =>
        {
            if (e.ConnectionId == registeredConnectionId)
            {
                disconnectEvent.Set();
            }
        });

        // Server initiated disconnection requires waiting for the
        // close websocket message to be received by the client.
        await server.DisconnectAsync(registeredConnectionId);
        disconnectEvent.WaitOne(TimeSpan.FromSeconds(1));
        await connection.StopAsync();
        Assert.That(connectionLog, Is.EquivalentTo(expectedLogEntries));
    }

    [Test]
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
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });

        IReadOnlyList<string> serverLog = server.Log;
        await connection.StartAsync($"ws://localhost:{server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        server.IgnoreCloseConnectionRequest(registeredConnectionId, true);
        await connection.StopAsync();
        Assert.That(connectionLog, Is.EquivalentTo(expectedLogEntries));
    }

    [Test]
    public async Task TestConnectionRaisesErrorEventOnWebSocketException()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        ConnectionErrorEventArgs? receivedErrorArgs = null;
        ManualResetEventSlim errorReceivedEvent = new(false);
        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnConnectionError.AddObserver((ConnectionErrorEventArgs e) =>
        {
            receivedErrorArgs = e;
            errorReceivedEvent.Set();
            return Task.CompletedTask;
        });

        await connection.StartAsync($"ws://localhost:{server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        server.IgnoreCloseConnectionRequest(this.connectionId, true);
        await connection.StopAsync();

        bool errorReceived = errorReceivedEvent.Wait(TimeSpan.FromSeconds(3));
        Assert.That(errorReceived, Is.True);
        Assert.That(receivedErrorArgs, Is.Not.Null);
        Assert.That(receivedErrorArgs.Exception, Is.InstanceOf<WebSocketException>());
    }

    [Test]
    public async Task TestConnectionCanBeReusedAfterBeingShutDown()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);

        await connection.StartAsync($"ws://localhost:{server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        ServerEventObserver<ServerDataReceivedEventArgs> observer = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("First connection hello"u8.ToArray());
        string serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromMilliseconds(250));
        observer.Unobserve();
        Assert.That(serverReceivedData, Is.EqualTo("First connection hello"));

        await server.SendWebSocketDataAsync(registeredConnectionId, "First connection acknowledged");
        byte[] receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromMilliseconds(250));
        await connection.StopAsync();
        Assert.That(receivedData, Is.EqualTo("First connection acknowledged"u8.ToArray()));

        await connection.StartAsync($"ws://localhost:{server.Port}");
        registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        observer = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("Second connection hello"u8.ToArray());
        serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromMilliseconds(250));
        observer.Unobserve();
        Assert.That(serverReceivedData, Is.EqualTo("Second connection hello"));

        await server.SendWebSocketDataAsync(registeredConnectionId, "Second connection acknowledged");
        receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromMilliseconds(250));
        await connection.StopAsync();
        Assert.That(receivedData, Is.EqualTo("Second connection acknowledged"u8.ToArray()));
    }

    [Test]
    public async Task TestConnectionCanBeReusedAfterBeingAborted()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);

        await connection.StartAsync($"ws://localhost:{server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        ServerEventObserver<ServerDataReceivedEventArgs> observer = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);

        await connection.SendDataAsync("First connection hello"u8.ToArray());
        string serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromMilliseconds(250));
        observer.Unobserve();
        Assert.That(serverReceivedData, Is.EqualTo("First connection hello"));

        await server.SendWebSocketDataAsync(registeredConnectionId, "First connection acknowledged");
        byte[] receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromMilliseconds(250));
        server.IgnoreCloseConnectionRequest(registeredConnectionId, true);
        await connection.StopAsync();
        Assert.That(receivedData, Is.EqualTo("First connection acknowledged"u8.ToArray()));

        await connection.StartAsync($"ws://localhost:{server.Port}");
        registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        observer = server.OnDataReceived.AddObserver(this.OnSocketDataReceived);
        
        await connection.SendDataAsync("Second connection hello"u8.ToArray());
        serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromMilliseconds(250));
        observer.Unobserve();
        Assert.That(serverReceivedData, Is.EqualTo("Second connection hello"));

        await server.SendWebSocketDataAsync(registeredConnectionId, "Second connection acknowledged");
        receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromMilliseconds(250));
        await connection.StopAsync();
        Assert.That(receivedData, Is.EqualTo("Second connection acknowledged"u8.ToArray()));
    }

    [Test]
    public async Task TestCannotStartAlreadyStartedConnection()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        await connection.StartAsync($"ws://localhost:{server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.That(async () => await connection.StartAsync($"ws://localhost:{server.Port}"), Throws.InstanceOf<WebDriverBiDiException>().With.Message.StartsWith($"The WebSocket is already connected to ws://localhost:{server.Port}"));
    }

    [Test]
    public void TestStartAsyncThrowsForInvalidUrl()
    {
        WebSocketConnection connection = new();
        Assert.That(async () => await connection.StartAsync("not-a-valid-url"), Throws.InstanceOf<ArgumentException>().With.Message.Contains("not a valid absolute URI"));
    }

    [Test]
    public void TestStartAsyncThrowsForNonWebSocketUrl()
    {
        WebSocketConnection connection = new();
        Assert.That(async () => await connection.StartAsync("http://localhost:8080"), Throws.InstanceOf<ArgumentException>().With.Message.Contains("The URI scheme must be 'ws' or 'wss'; received 'http'"));
    }

    [Test]
    public async Task TestCanStartWithSecuredWebSocketUrl()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromMilliseconds(10),
        };

        // Since the test server does not support secure WebSocket connections,
        // we expect this to fail with a timeout, but it verifies that the connection
        // attempts to connect to the correct URL and that the URL is accepted as valid.
        Assert.That(async () => await connection.StartAsync($"wss://localhost:{server.Port}"), Throws.InstanceOf<WebDriverBiDiTimeoutException>());
    }

    [Test]
    public void TestCannotSendDataOnAConnectionNotYetStarted()
    {
        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        Assert.That(async () => await connection.SendDataAsync("This send should fail"u8.ToArray()), Throws.InstanceOf<WebDriverBiDiException>().With.Message.StartsWith($"The WebSocket has not been initialized"));
    }

    [Test]
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
        connection.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });

        await connection.StartAsync($"ws://localhost:{server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        server.IgnoreCloseConnectionRequest(registeredConnectionId, true);
        await connection.StopAsync();

        // With ShutdownTimeout=Zero, CloseClientWebSocketAsync may throw OperationCanceledException
        // before logging "Client state is X". At minimum we get "Closing connection".
        Assert.That(connectionLog, Has.Some.EqualTo("Closing connection"));
        Assert.That(connectionLog, Has.Count.GreaterThanOrEqualTo(1));
    }

    [Test]
    public async Task TestDataSendOperationsAreSynchronized()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
            BypassDataSend = false,
            DataSendDelay = TimeSpan.FromMilliseconds(1000),
            DataTimeout = TimeSpan.FromMilliseconds(250),
        };
        await connection.StartAsync($"ws://localhost:{server.Port}");

        ManualResetEventSlim syncEvent = new(false);
        connection.DataSendStarting += (sender, e) =>
        {
            syncEvent.Set();
        };

        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        _ = Task.Run(() => connection.SendDataAsync("first data"u8.ToArray()));
        syncEvent.Wait();
        Assert.That(async () => await connection.SendDataAsync("second data"u8.ToArray()), Throws.InstanceOf<WebDriverBiDiTimeoutException>().With.Message.EqualTo("Timed out waiting to access WebSocket for sending; only one send operation is permitted at a time."));
        await connection.StopAsync();
    }

    [Test]
    public async Task TestCanDisposeAsyncWithoutStarting()
    {
        WebSocketConnection connection = new();
        Assert.That(async () => await connection.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestDoubleDisposeAsyncDoesNotThrow()
    {
        WebSocketConnection connection = new();
        await connection.DisposeAsync();
        Assert.That(async () => await connection.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestDoubleDisposeAsyncAfterStartDoesNotThrow()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new();
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://localhost:{server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.DisposeAsync();
        Assert.That(async () => await connection.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestIsDisposedPropertyIsSetAfterDispose()
    {
        TestWebSocketConnection connection = new();
        Assert.That(connection.Disposed, Is.False);
        await connection.DisposeAsync();
        Assert.That(connection.Disposed, Is.True);
    }

    [Test]
    public async Task TestCanDisposeAsyncAfterStop()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new();
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://localhost:{server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.StopAsync();
        Assert.That(async () => await connection.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestCanDisposeAsyncWithoutStoping()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new();
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://localhost:{server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.That(async () => await connection.DisposeAsync(), Throws.Nothing);
        Assert.That(connection.IsActive, Is.False);
    }

    [Test]
    public async Task TestDisposeLogsExceptionFromStop()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        connection.OnLogMessage.AddObserver((e) =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });
        await connection.StartAsync($"ws://localhost:{server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        connection.ThrowOnStop = true;
        connection.BypassStop = false;
        await connection.DisposeAsync();
        Assert.That(logs, Has.Some.Matches<LogMessageEventArgs>(
            log => log.Message.Contains("Unexpected exception during disposal")
                   && log.Message.Contains("Simulated stop failure")
                   && log.Level == WebDriverBiDiLogLevel.Warn
                   && log.ComponentName == "Connection"));
    }

    [Test]
    public async Task TestCanDisposeAsyncStartedConnectionAfterStop()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        WebSocketConnection connection = new();
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://localhost:{server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.serverDataReceivedObserver = server.OnDataReceived.AddObserver(OnSocketDataReceived);

        await connection.SendDataAsync("Hello world"u8.ToArray());
        this.WaitForServerToReceiveData(TimeSpan.FromSeconds(3));

        await connection.StopAsync();
        Assert.That(async () => await connection.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestConnectionDiscardsPartialFragmentedMessageOnClose()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        ManualResetEventSlim closeReturned = new(false);
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

            closeReturned.Set();
            await Task.Delay(Timeout.Infinite, token);
            throw new OperationCanceledException(token);
        };
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://localhost:{server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        closeReturned.Wait(TimeSpan.FromSeconds(3));
        await connection.StopAsync();
    }

    [Test]
    public async Task TestConnectionCleansUpPartialFragmentedMessageOnException()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        ManualResetEventSlim exceptionThrown = new(false);
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

            exceptionThrown.Set();
            throw new OperationCanceledException();
        };
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);
        await connection.StartAsync($"ws://localhost:{server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        exceptionThrown.Wait(TimeSpan.FromSeconds(3));
        await connection.StopAsync();
    }

    [Test]
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
        await connection.StartAsync("ws:localhost");
        connection.BypassStart = false;

        Assert.That(
            async () => await connection.SendDataAsync("data"u8.ToArray()),
            Throws.InstanceOf<WebDriverBiDiConnectionException>()
                .With.Message.EqualTo("The WebSocket connection was closed before the send could be completed"));
    }

    [Test]
    public async Task TestSendDataWrapsWebSocketExceptionInConnectionException()
    {
        TestWebSocketConnection connection = new()
        {
            IsActiveOverride = () => true,
            SendWebSocketDataOverride = _ => throw new WebSocketException("Simulated WebSocket failure"),
        };
        await connection.StartAsync("ws:localhost");
        connection.BypassStart = false;

        Assert.That(
            async () => await connection.SendDataAsync("data"u8.ToArray()),
            Throws.InstanceOf<WebDriverBiDiConnectionException>()
                .With.Message.Contains("Simulated WebSocket failure")
                .And.InnerException.InstanceOf<WebSocketException>());
    }

    [Test]
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
        await Task.Delay(100);
        cts.Cancel();

        Assert.That(async () => await startTask, Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task TestConnectionRaisesOnRemoteDisconnectedWhenServerGracefullyCloses()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        ManualResetEventSlim remoteDisconnectedEvent = new(false);
        WebSocketConnection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.OnRemoteDisconnected.AddObserver((ConnectionDisconnectedEventArgs _) =>
        {
            remoteDisconnectedEvent.Set();
            return Task.CompletedTask;
        });

        await connection.StartAsync($"ws://localhost:{server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.clientDisconnectedObserver = server.OnClientDisconnected.AddObserver(_ => { });

        await server.DisconnectAsync(registeredConnectionId);

        bool disconnected = remoteDisconnectedEvent.Wait(TimeSpan.FromSeconds(3));
        Assert.That(disconnected, Is.True, "Expected OnRemoteDisconnected to fire when server gracefully closes");
        await connection.StopAsync();
    }

    [Test]
    public async Task TestAllLogOutputsAreProduced()
    {
        await using Server server = this.CreateServer();
        await server.StartAsync();

        List<string> connectionLog = [];
        WebSocketConnection connection = new();
        connection.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
        {
            connectionLog.Add(e.Message);
            return Task.CompletedTask;
        });
        connection.OnDataReceived.AddObserver(OnConnectionDataReceivedAsync);

        await connection.StartAsync($"ws://localhost:{server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.StopAsync();

        Assert.That(connectionLog, Has.Some.Matches<string>(s => s.StartsWith("Opening connection to URL ")));
        Assert.That(connectionLog, Has.Some.EqualTo("Connection opened"));
        Assert.That(connectionLog, Has.Some.EqualTo("Closing connection"));
        Assert.That(connectionLog, Has.Some.Matches<string>(s => s.StartsWith("Client state is ")));
        Assert.That(connectionLog, Has.Some.Matches<string>(s => s.StartsWith("Ending processing loop in state ")));
    }

    [Test]
    public void TestSendDataThrowsWhenCancellationTokenIsCanceled()
    {
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            IsActiveOverride = () => true,
        };
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.That(async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("test"), cts.Token), Throws.InstanceOf<OperationCanceledException>());
    }

    private Server CreateServer()
    {
        Server server = new();
        server.OnClientConnected.AddObserver(OnClientConnected);
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
