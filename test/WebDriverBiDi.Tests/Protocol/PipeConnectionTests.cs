namespace WebDriverBiDi.Protocol;

using System.Text;
using NUnit.Framework.Internal;
using WebDriverBiDi.TestUtilities;

[TestFixture]
public class PipeConnectionTests
{
    [Test]
    public void TestConnectionType()
    {
        PipeConnection connection = new();
        Assert.That(connection.ConnectionType, Is.EqualTo(ConnectionType.Pipes));
    }

    [Test]
    public async Task TestCanSendData()
    {
        TestPipeServer testPipeServer = new();

        PipeConnection connection = new();
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);

        connection.SetExternalProcess(testPipeServer.ServerProcess!);
        await connection.StartAsync("pipe://local");
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello"));
        testPipeServer.Stop();

        string output = testPipeServer.GetSentData();
        Assert.That(output, Is.EqualTo("Hello"));
    }

    [Test]
    public async Task TestCanReceiveData()
    {
        TestPipeServer testPipeServer = new();
        testPipeServer.Responses.Add("Acknowledged!");

        List<string> receivedData = [];
        PipeConnection connection = new();
        connection.OnDataReceived.AddObserver(e => receivedData.Add(Encoding.UTF8.GetString(e.Data)));

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        connection.SetExternalProcess(testPipeServer.ServerProcess!);
        await connection.StartAsync("pipe://local");
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("hello"));
        testPipeServer.Stop();

        Assert.That(receivedData, Has.Count.EqualTo(1));
        Assert.That(receivedData[0], Is.EqualTo("Acknowledged!"));
    }

    [Test]
    public async Task TestReceivedDataTerminatedWithNullCharacter()
    {
        TestPipeServer testPipeServer = new();
        testPipeServer.Responses.Add("Acknowledged!\\0More data");

        List<string> receivedData = [];
        PipeConnection connection = new();
        connection.OnDataReceived.AddObserver(e => receivedData.Add(Encoding.UTF8.GetString(e.Data)));

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        connection.SetExternalProcess(testPipeServer.ServerProcess!);
        await connection.StartAsync("pipe://local");
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("hello"));
        testPipeServer.Stop();

        Assert.That(receivedData, Has.Count.EqualTo(1));
        Assert.That(receivedData[0], Is.EqualTo("Acknowledged!"));
    }

    [Test]
    public void TestStartingWithoutSettingExternalProcessThrows()
    {
        PipeConnection connection = new();
        Assert.That(() => connection.StartAsync("pipe"), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public async Task TestStartingWithoutStoppingThrows()
    {
        PipeConnection connection = new();
        TestPipeServer testPipeServer = new();
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        connection.SetExternalProcess(testPipeServer.ServerProcess!);
        await connection.StartAsync("pipe://local");
        Assert.That(async () => await connection.StartAsync("pipe"), Throws.InstanceOf<WebDriverBiDiException>());
        testPipeServer.Stop();
    }

    [Test]
    public async Task TestRemoteEndClosingMarksConnectionAsInactive()
    {
        PipeConnection connection = new();
        TestPipeServer testPipeServer = new();
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        connection.SetExternalProcess(testPipeServer.ServerProcess!);
        await connection.StartAsync("pipe://local");
        Assert.That(connection.IsActive, Is.True);
        testPipeServer.Stop();
        Assert.That(connection.IsActive, Is.False);
    }

    [Test]
    public async Task TestCanStop()
    {
        PipeConnection connection = new();
        TestPipeServer testPipeServer = new();
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        connection.SetExternalProcess(testPipeServer.ServerProcess!);
        await connection.StartAsync("pipe://local");
        await connection.StopAsync();
        Assert.That(connection.IsActive, Is.False);
        testPipeServer.Stop();
    }

    [Test]
    public async Task TestCanStopWithoutStarting()
    {
        PipeConnection connection = new();
        await connection.StopAsync();
        Assert.That(connection.IsActive, Is.False);
    }

    [Test]
    public async Task TestCanStopRepeatedly()
    {
        PipeConnection connection = new();
        await connection.StopAsync();
        await connection.StopAsync();
        Assert.That(connection.IsActive, Is.False);
    }

    [Test]
    public async Task TestSendDataWithoutStartingThrows()
    {
        PipeConnection connection = new();
        Assert.That(async () => await connection.SendDataAsync(new byte[] { 1, 2, 3 }), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public async Task TestCanLogMessages()
    {
        List<string> receivedData = [];
        PipeConnection connection = new();
        connection.OnLogMessage.AddObserver(e => receivedData.Add(e.Message));

        TestPipeServer testPipeServer = new();
        testPipeServer.Responses.Add("Acknowledged!");
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);

        connection.SetExternalProcess(testPipeServer.ServerProcess!);
        await connection.StartAsync("pipe://local");
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello"));
        testPipeServer.Stop();

        Assert.That(receivedData, Has.Count.EqualTo(6));
        Assert.That(receivedData, Is.EquivalentTo([
            "Starting pipe connection: pipe://local",
            "Pipe connection started",
            "SEND >>> Hello",
            "RECV <<< Acknowledged!",
            "Pipe closed by remote end",
            "Ending pipe receive loop",
        ]));
    }

    [Test]
    public async Task TestCanOnlySendOneMessageAtATime()
    {
        TestPipeConnection connection = new()
        {
            BypassDataSend = false,
            DataSendDelay = TimeSpan.FromMilliseconds(1000),
            DataTimeout = TimeSpan.FromMilliseconds(250),
        };

        ManualResetEventSlim syncEvent = new(false);
        connection.DataSendStarting += (sender, e) =>
        {
            syncEvent.Set();
        };

        TestPipeServer testPipeServer = new();
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        connection.SetExternalProcess(testPipeServer.ServerProcess!);
        await connection.StartAsync("pipe://local");
        _ = Task.Run(() => connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello")));
        syncEvent.Wait();
        Assert.That(async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("World")), Throws.InstanceOf<WebDriverBiDiTimeoutException>());
        testPipeServer.Stop();
    }
}
