namespace WebDriverBidi;

using System.Threading;
using TestUtilities;

[TestFixture]
public class ConnectionTests
{
    private TestWebSocketServer? server;
    private string lastReceivedData = string.Empty;
    private AutoResetEvent syncEvent = new AutoResetEvent(false);

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
        Connection connection = new Connection(TimeSpan.FromMilliseconds(250));
        Assert.That(async () => await connection.Start($"ws://localhost:{port}"), Throws.InstanceOf<TimeoutException>().With.Message.Contains(".25 seconds"));
    }

    [Test]
    public async Task TestConnectionCanSendData()
    {
        if (server is null)
        {
            throw new WebDriverBidiException("No server available");
        }

        Connection connection = new Connection();
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

        Connection connection = new Connection();
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


        Connection connection = new Connection();
        connection.DataReceived += OnConnectionDataReceived;
        await connection.Start($"ws://localhost:{server.Port}");

        // Create a message on an exact boundary of the buffer
        string data = new string('a', 2 * connection.BufferSize);
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

        List<string> logValues = new List<string>();
        Connection connection = new Connection();
        connection.DataReceived += OnConnectionDataReceived;
        connection.LogMessage += (object? sender, LogMessageEventArgs e) => 
        {
            logValues.Add(e.Message);
        };
        await connection.Start($"ws://localhost:{server.Port}");
        await connection.SendData("Hello world");
        syncEvent.WaitOne(TimeSpan.FromSeconds(3));

        this.lastReceivedData = string.Empty;
        await server.SendData("Hello back");
        syncEvent.WaitOne();
        await connection.Stop();
        Assert.That(logValues, Has.Count.EqualTo(6));
    }

    private void OnSocketDataReceived(object? sender, DataReceivedEventArgs e)
    {
        this.lastReceivedData = e.Data;
        this.syncEvent.Set();
    }

    private void OnConnectionDataReceived(object? sender, DataReceivedEventArgs e)
    {
        this.lastReceivedData = e.Data;
        this.syncEvent.Set();
    }
}