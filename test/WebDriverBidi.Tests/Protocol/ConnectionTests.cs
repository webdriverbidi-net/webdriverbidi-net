namespace WebDriverBidi.Protocol;

using System.Threading;
using PinchHitter;

[TestFixture]
public class ConnectionTests
{
    private Server? server;
    private string lastServerReceivedData = string.Empty;
    private string lastConnnectionReceivedData = string.Empty;
    private string connectionId = string.Empty;
    private readonly AutoResetEvent serverReceiveSyncEvent = new(false);
    private readonly AutoResetEvent connectionReceiveSyncEvent = new(false);
    private readonly AutoResetEvent connectionSyncEvent = new(false);

    [SetUp]
    public void InitializeServer()
    {
        this.connectionId = string.Empty;
        this.lastServerReceivedData = string.Empty;
        this.lastConnnectionReceivedData = string.Empty;
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
        Connection connection = new(TimeSpan.FromMilliseconds(250));
        Assert.That(async () => await connection.Start($"ws://localhost:{port}"), Throws.InstanceOf<TimeoutException>().With.Message.Contains(".25 seconds"));
    }

    [Test]
    public async Task TestConnectionCanSendData()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new();
        await connection.Start($"ws://localhost:{this.server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.server.DataReceived += OnSocketDataReceived;

        await connection.SendData("Hello world");
        string dataReceivedByServer = this.WaitForServerToReceiveData(TimeSpan.FromSeconds(3));

        Assert.That(dataReceivedByServer, Is.EqualTo("Hello world"));
        await connection.Stop();
    }

    [Test]
    public async Task TestConnectionCanReceiveData()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new();
        await connection.Start($"ws://localhost:{this.server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        connection.DataReceived += OnConnectionDataReceived;

        await this.server.SendData(registeredConnectionId, "Hello back");
        string dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));

        Assert.That(dataReceivedByConnection, Is.EqualTo("Hello back"));
        await connection.Stop();
    }

    [Test]
    public async Task TestConnectionReceivesDataOnBufferBoundary()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new();
        await connection.Start($"ws://localhost:{server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        connection.DataReceived += OnConnectionDataReceived;

        // Create a message on an exact boundary of the buffer
        string data = new('a', 2 * connection.BufferSize);
        await this.server.SendData(registeredConnectionId, data);
        string dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));

        Assert.That(dataReceivedByConnection, Is.EqualTo(data));
        await connection.Stop();
    }

    [Test]
    public async Task TestConnectionReceivesDataOnVeryLongMessage()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }


        Connection connection = new();
        await connection.Start($"ws://localhost:{this.server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        connection.DataReceived += OnConnectionDataReceived;

        // Create a message on an exact boundary of the buffer
        string data = new('a', 70000);
        await this.server.SendData(registeredConnectionId, data);
        string dataReceivedByConnection = this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(3));

        Assert.That(dataReceivedByConnection, Is.EqualTo(data));
        await connection.Stop();
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
        await connection.Start($"ws://localhost:{this.server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.server.DataReceived += OnSocketDataReceived;
        await connection.SendData("Hello world");
        this.WaitForServerToReceiveData(TimeSpan.FromSeconds(4));

        await this.server.SendData(registeredConnectionId, "Hello back");
        this.WaitForConnectionToReceiveData(TimeSpan.FromSeconds(4));
        await connection.Stop();

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
        await connection.Start($"ws://localhost:{this.server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        Assert.That(connection.IsActive, Is.True);
        await connection.Stop();
        Assert.That(connection.IsActive, Is.False);
    }

    [Test]
    public async Task TestConnectionStopCanBeCalledMultipleTimes()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new();
        connection.DataReceived += OnConnectionDataReceived;
        await connection.Start($"ws://localhost:{this.server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        await connection.Stop();
        await connection.Stop();
    }

    [Test]
    public async Task TestConnectionHandlesRemoteEndStop()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        connection.DataReceived += OnConnectionDataReceived;
        await connection.Start($"ws://localhost:{this.server.Port}");
        this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.server.Stop();
        await connection.Stop();
    }

    [Test]
    public async Task TestConnectionHandlesDisconnectInitiatedByRemoteEnd()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        List<string> connectionLog = new();
        Connection connection = new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        connection.LogMessage += (sender, e) =>
        {
            connectionLog.Add(e.Message);
        };

        IList<string> serverLog = this.server.Log;
        await connection.Start($"ws://localhost:{this.server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.server.DataReceived += OnSocketDataReceived;
    
        // Server initiated disconnection requires waiting for the
        // close websocket message to be received by the client.
        await this.server.Disconnect(registeredConnectionId);
        this.WaitForServerToReceiveData(TimeSpan.FromSeconds(1));
        await connection.Stop();
    }

    [Test]
    public async Task TestConnectionHandlesHungRemoteEnd()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        List<string> connectionLog = new();
        Connection connection = new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        connection.LogMessage += (sender, e) =>
        {
            connectionLog.Add(e.Message);
        };

        IList<string> serverLog = this.server.Log;
        await connection.Start($"ws://localhost:{this.server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.server.IgnoreCloseConnectionRequest(registeredConnectionId, true);
        await connection.Stop();
    }

    [Test]
    public async Task TestCanShutdownWhenCleanShutdownExceedsTimeout()
    {
        if (this.server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        List<string> connectionLog = new();
        Connection connection = new(TimeSpan.FromSeconds(1), TimeSpan.Zero);
        connection.LogMessage += (sender, e) =>
        {
            connectionLog.Add(e.Message);
        };

        IList<string> serverLog = this.server.Log;
        await connection.Start($"ws://localhost:{this.server.Port}");
        string registeredConnectionId = this.WaitForServerToRegisterConnection(TimeSpan.FromSeconds(1));
        this.server.IgnoreCloseConnectionRequest(registeredConnectionId, true);
        await connection.Stop();
    }

    private void OnSocketDataReceived(object? sender, ServerDataReceivedEventArgs e)
    {
        this.lastServerReceivedData = e.Data;
        this.serverReceiveSyncEvent.Set();
    }

    private void OnConnectionDataReceived(object? sender, ConnectionDataReceivedEventArgs e)
    {
        this.lastConnnectionReceivedData = e.Data;
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
        return this.lastConnnectionReceivedData;
    }

    private string WaitForServerToReceiveData(TimeSpan timeout)
    {
        this.serverReceiveSyncEvent.WaitOne(timeout);
        return this.lastServerReceivedData;
    }
}