namespace WebDriverBidi.Protocol;

using System.Threading;
using PinchHitter;
using WebDriverBidi.TestUtilities;

[TestFixture]
public class ConnectionTests
{
    private Server? server;
    private string lastServerReceivedData = string.Empty;
    private string lastConnectionReceivedData = string.Empty;
    private string connectionId = string.Empty;
    private readonly AutoResetEvent serverReceiveSyncEvent = new(false);
    private readonly AutoResetEvent connectionReceiveSyncEvent = new(false);
    private readonly AutoResetEvent connectionSyncEvent = new(false);

    [SetUp]
    public void InitializeServer()
    {
        this.connectionId = string.Empty;
        this.lastServerReceivedData = string.Empty;
        this.lastConnectionReceivedData = string.Empty;
        this.connectionReceiveSyncEvent.Reset();
        this.serverReceiveSyncEvent.Reset();
        this.connectionSyncEvent.Reset();
        this.server = new Server();
        this.server.ClientConnected += OnClientConnected;
        this.server.Start();
    }

    [TearDown]
    public void DisposeServer()
    {
        if (this.server is not null)
        {
            this.server.Stop();
            this.server.DataReceived -= OnSocketDataReceived;
            this.server.ClientConnected -= OnClientConnected;
            this.server = null;
        }
    }

    [Test]
    public void TestConnectionFailure()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        int port = this.server.Port;
        DisposeServer();
        Connection connection = new()
        {
            StartupTimeout = TimeSpan.FromMilliseconds(250)
        };
        Assert.That(async () => await connection.StartAsync($"ws://localhost:{port}"), Throws.InstanceOf<TimeoutException>().With.Message.Contains(".25 seconds"));
    }

    [Test]
    public async Task TestConnectionCanSendData()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new();
        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.server.DataReceived += OnSocketDataReceived;

        await connection.SendDataAsync("Hello world");
        string dataReceivedByServer = this.WaitForServerToReceiveData(TimeSpan.FromSeconds(3));

        Assert.That(dataReceivedByServer, Is.EqualTo("Hello world"));
        await connection.StopAsync();
    }

    [Test]
    public async Task TestConnectionCanReceiveData()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new();
        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        connection.DataReceived += OnConnectionDataReceived;

        await this.server.SendData(registeredConnectionId, "Hello back");
        string dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));

        Assert.That(dataReceivedByConnection, Is.EqualTo("Hello back"));
        await connection.StopAsync();
    }

    [Test]
    public async Task TestConnectionReceivesDataOnBufferBoundary()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new();
        await connection.StartAsync($"ws://localhost:{server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        connection.DataReceived += OnConnectionDataReceived;

        // Create a message on an exact boundary of the buffer
        string data = new('a', 2 * connection.BufferSize);
        await this.server.SendData(registeredConnectionId, data);
        string dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));

        Assert.That(dataReceivedByConnection, Is.EqualTo(data));
        await connection.StopAsync();
    }

    [Test]
    public async Task TestConnectionReceivesDataOnVeryLongMessage()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new();
        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        connection.DataReceived += OnConnectionDataReceived;

        // Create a message on an exact boundary of the buffer
        string data = new('a', 70000);
        await this.server.SendData(registeredConnectionId, data);
        string dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));

        Assert.That(dataReceivedByConnection, Is.EqualTo(data));
        await connection.StopAsync();
    }

    [Test]
    public async Task TestConnectionLog()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        List<LogMessageEventArgs> logValues = new();
        Connection connection = new();
        connection.DataReceived += OnConnectionDataReceived;
        connection.LogMessage += (object? sender, LogMessageEventArgs e) => 
        {
            if (e.Level >= WebDriverBidiLogLevel.Info)
            {
                logValues.Add(e);
            }
        };
        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.server.DataReceived += OnSocketDataReceived;
        await connection.SendDataAsync("Hello world");
        this.WaitForServerToReceiveData(TimeSpan.FromSeconds(4));

        await this.server.SendData(registeredConnectionId, "Hello back");
        this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(4));
        await connection.StopAsync();

        List<string> messages = new();
        foreach (LogMessageEventArgs logValue in logValues)
        {
            messages.Add(logValue.Message);
        }

        Assert.That(logValues, Has.Count.EqualTo(5), $"Actual values: {string.Join("\n", messages.ToArray())}");
        foreach(LogMessageEventArgs args in logValues)
        {
            Assert.Multiple(() =>
            {
                Assert.That(args.Level, Is.EqualTo(WebDriverBidiLogLevel.Info));
                Assert.That(args.Message, Is.Not.Null);
            });
        }
    }

    [Test]
    public async Task TestIsActiveProperty()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new();
        Assert.That(connection.IsActive, Is.False);
        connection.DataReceived += OnConnectionDataReceived;
        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.That(connection.IsActive, Is.True);
        await connection.StopAsync();
        Assert.That(connection.IsActive, Is.False);
    }

    [Test]
    public async Task TestUrlProperty()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        string serverWebSocketUrl = $"ws://localhost:{this.server.Port}";
        Connection connection = new();
        Assert.That(connection.ConnectedUrl, Is.EqualTo(string.Empty));
        connection.DataReceived += OnConnectionDataReceived;
        await connection.StartAsync(serverWebSocketUrl);
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.That(connection.ConnectedUrl, Is.EqualTo(serverWebSocketUrl));
        await connection.StopAsync();
        Assert.That(connection.ConnectedUrl, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task TestStopWithoutStart()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new();
        Assert.That(connection.IsActive, Is.False);
        await connection.StopAsync();
        Assert.That(connection.IsActive, Is.False);
    }

    [Test]
    public async Task TestStopForcesCancellationOfDataReceiveTask()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        TestConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false
        };
        Assert.That(connection.IsActive, Is.False);
        connection.DataReceived += OnConnectionDataReceived;
        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.That(connection.IsActive, Is.True);
        connection.DataReceived += OnConnectionDataReceived;

        // Send data to the connection, which should force the receive data
        // task to enter a waiting state after receiving the first message.
        await this.server.SendData(registeredConnectionId, "Hello back");
        string dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));
        await connection.StopAsync();
        Assert.That(connection.IsActive, Is.False);
    }

    [Test]
    public async Task TestConnectionStopCanBeCalledMultipleTimes()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        List<string> connectionLog = new();
        Connection connection = new();
        connection.LogMessage += (sender, e) =>
        {
            connectionLog.Add(e.Message);
        };
        connection.DataReceived += OnConnectionDataReceived;
        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.StopAsync();
        await connection.StopAsync();
    }

    [Test]
    public async Task TestConnectionHandlesRemoteEndStop()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.DataReceived += OnConnectionDataReceived;
        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.server.Stop();
        await connection.StopAsync();
    }

    [Test]
    public async Task TestConnectionInitiateWebSocketClose()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        List<string> expectedLogEntries = new()
        {
            $"Opening connection to URL ws://localhost:{this.server.Port}",
            "Connection opened",
            "Closing connection",
            "Ending processing loop in state Closed",
            "Client state is Closed"
        };

        List<string> connectionLog = new();
        Connection connection = new();
        connection.LogMessage += (sender, e) =>
        {
            connectionLog.Add(e.Message);
        };
        connection.DataReceived += OnConnectionDataReceived;
        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.StopAsync();
        Assert.That(connectionLog, Is.EquivalentTo(expectedLogEntries));
    }

    [Test]
    public async Task TestConnectionHandlesDisconnectInitiatedByRemoteEnd()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        List<string> expectedLogEntries = new()
        {
            $"Opening connection to URL ws://localhost:{this.server.Port}",
            "Connection opened",
            "Acknowledging Close frame received from server (client state: CloseReceived)",
            "Ending processing loop in state Closed",
            "Closing connection",
            "Socket already closed (Socket state: Closed)"
        };

        List<string> connectionLog = new();
        Connection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.LogMessage += (sender, e) =>
        {
            connectionLog.Add(e.Message);
        };

        IList<string> serverLog = this.server.Log;
        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        ManualResetEvent disconnectEvent = new(false);
        this.server.ClientDisconnected += (sender, e) =>
        {
            if (e.ConnectionId == registeredConnectionId)
            {
                disconnectEvent.Set();
            }
        };
    
        // Server initiated disconnection requires waiting for the
        // close websocket message to be received by the client.
        await this.server.Disconnect(registeredConnectionId);
        disconnectEvent.WaitOne(TimeSpan.FromSeconds(1));
        await connection.StopAsync();
        Assert.That(connectionLog, Is.EquivalentTo(expectedLogEntries));
    }

    [Test]
    public async Task TestConnectionHandlesHungRemoteEnd()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        List<string> expectedLogEntries = new()
        {
            $"Opening connection to URL ws://localhost:{this.server.Port}",
            "Connection opened",
            "Closing connection",
            "Unexpected error during receive of data: The remote party closed the WebSocket connection without completing the close handshake.",
            "Client state is Aborted"
        };

        List<string> connectionLog = new();
        Connection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.LogMessage += (sender, e) =>
        {
            connectionLog.Add(e.Message);
        };

        IList<string> serverLog = this.server.Log;
        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.server.IgnoreCloseConnectionRequest(registeredConnectionId, true);
        await connection.StopAsync();
        Assert.That(connectionLog, Is.EquivalentTo(expectedLogEntries));
    }

    [Test]
    public async Task TestConnectionCanBeReusedAfterBeingShutDown()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.DataReceived += this.OnConnectionDataReceived;

        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.server.DataReceived += this.OnSocketDataReceived;

        await connection.SendDataAsync("First connection hello");
        string serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromMilliseconds(250));
        this.server.DataReceived -= this.OnSocketDataReceived;
        Assert.That(serverReceivedData, Is.EqualTo("First connection hello"));

        await this.server.SendData(registeredConnectionId, "First connection acknowledged");
        string receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromMilliseconds(250));
        await connection.StopAsync();
        Assert.That(receivedData, Is.EqualTo("First connection acknowledged"));

        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.server.DataReceived += this.OnSocketDataReceived;

        await connection.SendDataAsync("Second connection hello");
        serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromMilliseconds(250));
        this.server.DataReceived -= this.OnSocketDataReceived;
        Assert.That(serverReceivedData, Is.EqualTo("Second connection hello"));

        await this.server.SendData(registeredConnectionId, "Second connection acknowledged");
        receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromMilliseconds(250));
        await connection.StopAsync();
        Assert.That(receivedData, Is.EqualTo("Second connection acknowledged"));
    }

    [Test]
    public async Task TestConnectionCanBeReusedAfterBeingAborted()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        connection.DataReceived += this.OnConnectionDataReceived;

        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.server.DataReceived += this.OnSocketDataReceived;

        await connection.SendDataAsync("First connection hello");
        string serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromMilliseconds(250));
        this.server.DataReceived -= this.OnSocketDataReceived;
        Assert.That(serverReceivedData, Is.EqualTo("First connection hello"));

        await this.server.SendData(registeredConnectionId, "First connection acknowledged");
        string receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromMilliseconds(250));
        this.server.IgnoreCloseConnectionRequest(registeredConnectionId, true);
        await connection.StopAsync();
        Assert.That(receivedData, Is.EqualTo("First connection acknowledged"));

        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.server.DataReceived += this.OnSocketDataReceived;

        await connection.SendDataAsync("Second connection hello");
        serverReceivedData = this.WaitForServerToReceiveData(TimeSpan.FromMilliseconds(250));
        this.server.DataReceived -= this.OnSocketDataReceived;
        Assert.That(serverReceivedData, Is.EqualTo("Second connection hello"));

        await this.server.SendData(registeredConnectionId, "Second connection acknowledged");
        receivedData = this.WaitForConnectionToReceiveData(TimeSpan.FromMilliseconds(250));
        await connection.StopAsync();
        Assert.That(receivedData, Is.EqualTo("Second connection acknowledged"));
    }

    [Test]
    public async Task TestCannotStartAlreadyStartedConnection()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.That(async () => await connection.StartAsync($"ws://localhost:{this.server.Port}"), Throws.InstanceOf<WebDriverBidiException>().With.Message.StartsWith($"The WebSocket is already connected to ws://localhost:{this.server.Port}"));
    }

    [Test]
    public void TestCannotSendDataOnAConnectionNotYetStarted()
    {
        Connection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };
        Assert.That(async () => await connection.SendDataAsync($"This send should fail"), Throws.InstanceOf<WebDriverBidiException>().With.Message.StartsWith($"The WebSocket has not been initialized"));
    }

    [Test]
    public async Task TestCanShutdownWhenCleanShutdownExceedsTimeout()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        List<string> connectionLog = new();
        Connection connection = new()
        {
            StartupTimeout = TimeSpan.FromSeconds(1),
            ShutdownTimeout = TimeSpan.Zero,
        };
        connection.LogMessage += (sender, e) =>
        {
            connectionLog.Add(e.Message);
        };

        IList<string> serverLog = this.server.Log;
        await connection.StartAsync($"ws://localhost:{this.server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.server.IgnoreCloseConnectionRequest(registeredConnectionId, true);
        await connection.StopAsync();
    }

    [Test]
    public async Task TestDataSendOperationsAreSynchronized()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        TestConnection connection = new()
        {
            BypassStart = false,
            BypassStop = false,
            BypassDataSend = false,
            DataSendDelay = TimeSpan.FromMilliseconds(1000),
            DataTimeout = TimeSpan.FromMilliseconds(250),
        };
        await connection.StartAsync($"ws://localhost:{this.server.Port}");

        ManualResetEventSlim syncEvent = new(false);
        connection.DataSendStarting += (sender, e) =>
        {
            syncEvent.Set();
        };

        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        _ = Task.Run(() => connection.SendDataAsync("first data"));
        syncEvent.Wait();
        Assert.That(async () => await connection.SendDataAsync("second data"), Throws.InstanceOf<WebDriverBidiException>().With.Message.EqualTo("Timed out waiting to access WebSocket for sending; only one send operation is permitted at a time."));
        await connection.StopAsync();
    }

    private void OnSocketDataReceived(object? sender, ServerDataReceivedEventArgs e)
    {
        this.lastServerReceivedData = e.Data;
        this.serverReceiveSyncEvent.Set();
    }

    private void OnConnectionDataReceived(object? sender, ConnectionDataReceivedEventArgs e)
    {
        this.lastConnectionReceivedData = e.Data;
        this.connectionReceiveSyncEvent.Set();
    }

    private void OnClientConnected(object? sender, ClientConnectionEventArgs e)
    {
        this.connectionId = e.ConnectionId;
        this.connectionSyncEvent.Set();
    }

    private string WaitForServerToRegisterConnection(TimeSpan timeout)
    {
        this.connectionSyncEvent.WaitOne(timeout);
        return this.connectionId;
    }

    private string WaitForConnectionToReceiveData(TimeSpan timeout)
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