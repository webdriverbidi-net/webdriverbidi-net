namespace WebDriverBidi;

using System.Threading;
using TestUtilities;
using WebDriverBidi.Protocol;

[TestFixture]
public class ConnectionTests
{
    private TestWebSocketServer? server;
    private string lastReceivedData = string.Empty;
    private readonly AutoResetEvent syncEvent = new(false);

    [SetUp]
    public void InitializeServer()
    {
        lastReceivedData = string.Empty;
        syncEvent.Reset();
        server = new TestWebSocketServer();
        server.DataReceived += OnSocketDataReceived;
        server.Start();
    }

    [TearDown]
    public void DisposeServer()
    {
        if (server is not null)
        {
            server.Stop();
            server.DataReceived -= OnSocketDataReceived;
            server = null;
        }
    }

    [Test]
    public void TestConnectionFailure()
    {
        if (server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        int port = server.Port;
        DisposeServer();
        Connection connection = new(TimeSpan.FromMilliseconds(250));
        Assert.That(async () => await connection.Start($"ws://localhost:{port}"), Throws.InstanceOf<TimeoutException>().With.Message.Contains(".25 seconds"));
    }

    [Test]
    public async Task TestConnectionCanSendData()
    {
        if (server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new();
        connection.DataReceived += OnConnectionDataReceived;
        await connection.Start($"ws://localhost:{server.Port}");

        await connection.SendData("Hello world");
        syncEvent.WaitOne(TimeSpan.FromSeconds(3));

        Assert.That(this.lastReceivedData, Is.EqualTo("Hello world"));
        await connection.Stop();
    }

    [Test]
    public async Task TestConnectionCanReceiveData()
    {
        if (server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new();
        connection.DataReceived += OnConnectionDataReceived;
        await connection.Start($"ws://localhost:{server.Port}");

        await server.SendData("Hello back");
        syncEvent.WaitOne(TimeSpan.FromSeconds(3));

        Assert.That(this.lastReceivedData, Is.EqualTo("Hello back"));
        await connection.Stop();
    }

    [Test]
    public async Task TestConnectionReceivesDataOnBufferBoundary()
    {
        if (server is null)
        {
            throw new WebDriverBidiException("No server available");
        }


        Connection connection = new();
        connection.DataReceived += OnConnectionDataReceived;
        await connection.Start($"ws://localhost:{server.Port}");

        // Create a message on an exact boundary of the buffer
        string data = new('a', 2 * connection.BufferSize);
        await server.SendData(data);
        syncEvent.WaitOne(TimeSpan.FromSeconds(3));

        Assert.That(this.lastReceivedData, Is.EqualTo(data));
        await connection.Stop();
    }

    [Test]
    public async Task TestConnectionLog()
    {
        if (server is null)
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
        await connection.Start($"ws://localhost:{server.Port}");
        await connection.SendData("Hello world");
        syncEvent.WaitOne(TimeSpan.FromSeconds(4));

        this.lastReceivedData = string.Empty;
        await server.SendData("Hello back");
        syncEvent.WaitOne();
        await connection.Stop();
        Assert.That(logValues, Has.Count.EqualTo(4));
        foreach(var args in logValues)
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
        if (server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new();
        Assert.That(connection.IsActive, Is.False);
        connection.DataReceived += OnConnectionDataReceived;
        await connection.Start($"ws://localhost:{server.Port}");
        Assert.That(connection.IsActive, Is.True);
        await connection.Stop();
        Assert.That(connection.IsActive, Is.False);
    }

    [Test]
    public async Task TestConnectionStopCanBeCalledMultipleTimes()
    {
        if (server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new();
        connection.DataReceived += OnConnectionDataReceived;
        await connection.Start($"ws://localhost:{server.Port}");
        await connection.Stop();
        await connection.Stop();
    }

    [Test]
    public async Task TestConnectionHandlesRemoteEndStop()
    {
        if (server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        connection.DataReceived += OnConnectionDataReceived;
        await connection.Start($"ws://localhost:{server.Port}");
        server.Stop();
        await connection.Stop();
    }

    [Test]
    public async Task TestConnectionHandlesHungRemoteEnd()
    {
        if (server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        List<string> connectionLog = new();
        Connection connection = new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        connection.LogMessage += (sender, e) =>
        {
            connectionLog.Add(e.Message);
        };

        List<string> serverLog = server.Log;
        await connection.Start($"ws://localhost:{server.Port}");
        await server.CloseClientSocket();
        await connection.Stop();
    }

    private void OnSocketDataReceived(object? sender, ConnectionDataReceivedEventArgs e)
    {
        this.lastReceivedData = e.Data;
        this.syncEvent.Set();
    }

    private void OnConnectionDataReceived(object? sender, ConnectionDataReceivedEventArgs e)
    {
        this.lastReceivedData = e.Data;
        this.syncEvent.Set();
    }
}