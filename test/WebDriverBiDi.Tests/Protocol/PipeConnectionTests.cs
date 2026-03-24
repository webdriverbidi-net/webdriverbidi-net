namespace WebDriverBiDi.Protocol;

using System.Linq.Expressions;
using System.Text;
using NUnit.Framework.Internal;
using WebDriverBiDi.TestUtilities;

[TestFixture]
public class PipeConnectionTests
{
    [Test]
    public void TestConnectionType()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        Assert.That(connection.ConnectionType, Is.EqualTo(ConnectionType.Pipes));
    }

    [Test]
    public async Task TestCanSendData()
    {
        TestPipeServer testPipeServer = new();

        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);

        await connection.StartAsync("pipe://local");
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello"));
        bool dataSendSuccess = testPipeServer.WaitForDataSent(TimeSpan.FromSeconds(1));
        testPipeServer.Stop();
        Assert.That(dataSendSuccess, Is.True);

        string output = testPipeServer.GetSentData();
        Assert.That(output, Is.EqualTo("Hello"));
    }

    [Test]
    public async Task TestCanReceiveData()
    {
        TestPipeServer testPipeServer = new();
        testPipeServer.Responses.Add("Acknowledged!");

        List<string> receivedData = [];
        PipeConnection connection = new(testPipeServer);
        connection.OnDataReceived.AddObserver(e => receivedData.Add(Encoding.UTF8.GetString(e.Data)));

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
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
        PipeConnection connection = new(testPipeServer);
        connection.OnDataReceived.AddObserver(e => receivedData.Add(Encoding.UTF8.GetString(e.Data)));

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("hello"));
        testPipeServer.Stop();

        Assert.That(receivedData, Has.Count.EqualTo(1));
        Assert.That(receivedData[0], Is.EqualTo("Acknowledged!"));
    }

    [Test]
    public void TestStartingWithoutSettingExternalProcessThrows()
    {
        PipeConnection connection = new(new TestPipeServer());
        Assert.That(() => connection.StartAsync("pipe"), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public async Task TestStartingWithoutStoppingThrows()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");
        Assert.That(async () => await connection.StartAsync("pipe"), Throws.InstanceOf<WebDriverBiDiException>());
        testPipeServer.Stop();
    }

    [Test]
    public async Task TestRemoteEndClosingMarksConnectionAsInactive()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");
        Assert.That(connection.IsActive, Is.True);
        testPipeServer.Stop();
        Assert.That(connection.IsActive, Is.False);
    }

    [Test]
    public async Task TestCanStop()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");
        await connection.StopAsync();
        Assert.That(connection.IsActive, Is.False);
        testPipeServer.Stop();
    }

    [Test]
    public async Task TestCanStopWithoutStarting()
    {
        PipeConnection connection = new(new TestPipeServer());
        await connection.StopAsync();
        Assert.That(connection.IsActive, Is.False);
    }

    [Test]
    public async Task TestCanStopRepeatedly()
    {
        PipeConnection connection = new(new TestPipeServer());
        await connection.StopAsync();
        await connection.StopAsync();
        Assert.That(connection.IsActive, Is.False);
    }

    [Test]
    public async Task TestSendDataWithoutStartingThrows()
    {
        PipeConnection connection = new(new TestPipeServer());
        Assert.That(async () => await connection.SendDataAsync([1, 2, 3]), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public async Task TestCanLogMessages()
    {
        List<string> receivedData = [];
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        connection.OnLogMessage.AddObserver(e => receivedData.Add(e.Message));

        testPipeServer.Responses.Add("Acknowledged!");
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);

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
        TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer)
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

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");
        _ = Task.Run(() => connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello")));
        syncEvent.Wait();
        Assert.That(async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("World")), Throws.InstanceOf<WebDriverBiDiTimeoutException>());
        testPipeServer.Stop();
    }

    [Test]
    public void TestCanDispose()
    {
        PipeConnection connection = new(new TestPipeServer());
        Assert.That(async () => await connection.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestDoubleDisposeAsyncDoesNotThrow()
    {
        PipeConnection connection = new(new TestPipeServer());
        await connection.DisposeAsync();
        Assert.That(async () => await connection.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestDoubleDisposeAsyncAfterStartDoesNotThrow()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");
        await connection.DisposeAsync();
        testPipeServer.Stop();
        Assert.That(async () => await connection.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestIsDisposedPropertyIsSetAfterDispose()
    {
        TestPipeConnection connection = new(new TestPipeServer());
        Assert.That(connection.Disposed, Is.False);
        await connection.DisposeAsync();
        Assert.That(connection.Disposed, Is.True);
    }

    [Test]
    public async Task TestCanDisposeAsyncAfterStop()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");
        await connection.StopAsync();
        testPipeServer.Stop();
        Assert.That(async () => await connection.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestCanDisposeAsyncWithoutStopping()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");
        Assert.That(async () => await connection.DisposeAsync(), Throws.Nothing);
        Assert.That(connection.IsActive, Is.False);
        testPipeServer.Stop();
    }

    [Test]
    public async Task TestDisposeLogsExceptionFromStop()
    {
        List<LogMessageEventArgs> logs = [];
        TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer);
        connection.OnLogMessage.AddObserver((e) =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");
        connection.ThrowOnStop = true;
        await connection.DisposeAsync();
        testPipeServer.Stop();

        Assert.That(logs, Has.Some.Matches<LogMessageEventArgs>(
            log => log.Message.Contains("Unexpected exception during disposal")
                   && log.Message.Contains("Simulated stop failure")
                   && log.Level == WebDriverBiDiLogLevel.Warn
                   && log.ComponentName == "Connection"));
    }

    [Test]
    public async Task TestCanDisposeAsyncStartedConnectionAfterStop()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello"));
        testPipeServer.WaitForDataSent(TimeSpan.FromSeconds(1));
        await connection.StopAsync();
        testPipeServer.Stop();
        Assert.That(async () => await connection.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestSendDataThrowsWhenConnectionBecomesInactiveAfterSemaphoreAcquired()
    {
        int isActiveCallCount = 0;
        TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer)
        {
            IsActiveOverride = () =>
            {
                int count = Interlocked.Increment(ref isActiveCallCount);
                return count <= 1;
            },
        };

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");

        Assert.That(
            async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("data")),
            Throws.InstanceOf<WebDriverBiDiConnectionException>()
                .With.Message.EqualTo("The pipe connection was closed before the send could be completed"));

        testPipeServer.Stop();
    }

    [Test]
    public void TestSendDataThrowsWhenCancellationTokenIsCanceled()
    {
        TestPipeConnection connection = new(new TestPipeServer())
        {
            IsActiveOverride = () => true,
        };
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.That(async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("test"), cts.Token), Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task TestStartAfterDisposeThrows()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");
        await connection.DisposeAsync();
        testPipeServer.Stop();

        Assert.That(async () => await connection.StartAsync("pipe://local"), Throws.InstanceOf<WebDriverBiDiConnectionException>().With.Message.Contains("pipes have been disposed"));
    }

    [Test]
    public async Task TestStartAfterServerProcessExitThrows()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");
        testPipeServer.Stop();

        Assert.That(async () => await connection.StartAsync("pipe://local"), Throws.InstanceOf<WebDriverBiDiConnectionException>().With.Message.Contains("External process has already exited"));
    }

    [Test]
    public async Task TestSendDataWrapsIOExceptionInConnectionException()
    {
        TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer)
        {
            IsActiveOverride = () => true,
            ThrowIOExceptionOnSend = true,
        };

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");

        Assert.That(
            async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("data")),
            Throws.InstanceOf<WebDriverBiDiConnectionException>()
                .With.Message.Contains("An error occurred while sending data")
                .And.InnerException.InstanceOf<IOException>());

        testPipeServer.Stop();
    }

    [Test]
    public async Task TestSendDataWrapsObjectDisposedExceptionInConnectionException()
    {
        TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer)
        {
            IsActiveOverride = () => true,
            ThrowObjectDisposedExceptionOnSend = true,
        };

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");

        Assert.That(
            async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("data")),
            Throws.InstanceOf<WebDriverBiDiConnectionException>()
                .With.Message.Contains("An error occurred while sending data")
                .And.InnerException.InstanceOf<ObjectDisposedException>());

        testPipeServer.Stop();
    }

    [Test]
    public async Task TestPipeHandlesReturnEmptyStringAfterDisposal()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");
        await connection.DisposeAsync();

        // After disposal, pipes are null, so handles should return empty string
        using (Assert.EnterMultipleScope())
        {
            Assert.That(connection.ReadPipeHandle, Is.EqualTo(string.Empty));
            Assert.That(connection.WritePipeHandle, Is.EqualTo(string.Empty));
        }

        testPipeServer.Stop();
    }

    [Test]
    public async Task TestReceiveDataRaisesErrorEventOnIOException()
    {
        ConnectionErrorEventArgs? receivedErrorArgs = null;
        ManualResetEventSlim errorReceivedEvent = new(false);
        TestPipeServer testPipeServer = new();

        TestPipeConnection connection = new(testPipeServer)
        {
            ThrowIOExceptionOnReceive = true,
        };
        connection.OnConnectionError.AddObserver(e =>
        {
            receivedErrorArgs = e;
            errorReceivedEvent.Set();
            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");

        // Wait for error event (TestPipeConnection returns fake data on first read, then throws on second)
        bool errorReceived = errorReceivedEvent.Wait(TimeSpan.FromSeconds(3));
        testPipeServer.Stop();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(errorReceived, Is.True);
            Assert.That(receivedErrorArgs, Is.Not.Null);
            Assert.That(receivedErrorArgs!.Exception, Is.InstanceOf<IOException>());
        }
    }

    [Test]
    public async Task TestReceiveDataRaisesErrorEventOnObjectDisposedException()
    {
        ConnectionErrorEventArgs? receivedErrorArgs = null;
        ManualResetEventSlim errorReceivedEvent = new(false);
        TestPipeServer testPipeServer = new();

        TestPipeConnection connection = new(testPipeServer)
        {
            ThrowObjectDisposedExceptionOnReceive = true,
        };
        connection.OnConnectionError.AddObserver(e =>
        {
            receivedErrorArgs = e;
            errorReceivedEvent.Set();
            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local");

        // Wait for error event (TestPipeConnection returns fake data on first read, then throws on second)
        bool errorReceived = errorReceivedEvent.Wait(TimeSpan.FromSeconds(3));
        testPipeServer.Stop();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(errorReceived, Is.True);
            Assert.That(receivedErrorArgs, Is.Not.Null);
            Assert.That(receivedErrorArgs!.Exception, Is.InstanceOf<ObjectDisposedException>());
        }
    }

    [Test]
    public void TestPipesDisposedPropertySetterBothBranches()
    {
        TestPipeConnection connection = new(new TestPipeServer());

        // Test setting to true (one branch of ternary in setter)
        connection.PipesDisposed = true;
        Assert.That(connection.PipesDisposed, Is.True);

        // Test setting to false (other branch of ternary in setter)
        connection.PipesDisposed = false;
        Assert.That(connection.PipesDisposed, Is.False);

        // Test setting to true again to ensure it works both ways
        connection.PipesDisposed = true;
        Assert.That(connection.PipesDisposed, Is.True);
    }

    [Test]
    public void TestReadPipeHandleWhenPipesNotDisposed()
    {
        TestPipeConnection connection = new(new TestPipeServer());
        connection.PipesDisposed = false;

        // When pipes are not disposed, should return the actual handle
        string handle = connection.ReadPipeHandle;
        Assert.That(handle, Is.Not.Empty);
    }

    [Test]
    public void TestWritePipeHandleWhenPipesNotDisposed()
    {
        TestPipeConnection connection = new(new TestPipeServer());
        connection.PipesDisposed = false;

        // When pipes are not disposed, should return the actual handle
        string handle = connection.WritePipeHandle;
        Assert.That(handle, Is.Not.Empty);
    }

    [Test]
    public void TestReadPipeHandleWhenPipesDisposed()
    {
        TestPipeConnection connection = new(new TestPipeServer());
        connection.PipesDisposed = true;

        // When pipes are disposed, should return empty string
        string handle = connection.ReadPipeHandle;
        Assert.That(handle, Is.Empty);
    }

    [Test]
    public void TestWritePipeHandleWhenPipesDisposed()
    {
        TestPipeConnection connection = new(new TestPipeServer());
        connection.PipesDisposed = true;

        // When pipes are disposed, should return empty string
        string handle = connection.WritePipeHandle;
        Assert.That(handle, Is.Empty);
    }
}
