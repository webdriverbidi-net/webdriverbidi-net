namespace WebDriverBiDi.Protocol;

using System.Diagnostics;
using System.Text;
using WebDriverBiDi.TestUtilities;

public class PipeConnectionTests
{
    [Fact]
    public async Task TestConnectionType()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        Assert.Equal(ConnectionType.Pipes, connection.ConnectionType);
    }

    [Fact]
    public async Task TestCanSendData()
    {
        TestPipeServer testPipeServer = new();

        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);

        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello"), TestContext.Current.CancellationToken);
        bool dataSendSuccess = testPipeServer.WaitForDataSent(TimeSpan.FromSeconds(1));
        testPipeServer.Stop();
        Assert.True(dataSendSuccess);

        string output = testPipeServer.GetSentData();
        Assert.Equal("Hello", output);
    }

    [Fact]
    public async Task TestCanReceiveData()
    {
        TaskCompletionSource remoteDisconnectedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestPipeServer testPipeServer = new();
        testPipeServer.Responses.Add("Acknowledged!");

        List<string> receivedData = [];
        PipeConnection connection = new(testPipeServer);
        connection.OnDataReceived.AddObserver(e => receivedData.Add(Encoding.UTF8.GetString(e.Data)));
        connection.OnRemoteDisconnected.AddObserver(e =>
        {
            remoteDisconnectedTaskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("hello"), TestContext.Current.CancellationToken);
        testPipeServer.Stop();

        await remoteDisconnectedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.Single(receivedData);
        Assert.Equal("Acknowledged!", receivedData[0]);
    }

    [Fact]
    public async Task TestReceivedDataTerminatedWithNullCharacter()
    {
        TaskCompletionSource remoteDisconnectedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestPipeServer testPipeServer = new();
        testPipeServer.Responses.Add("Acknowledged!\\0More data");

        List<string> receivedData = [];
        PipeConnection connection = new(testPipeServer);
        connection.OnDataReceived.AddObserver(e => receivedData.Add(Encoding.UTF8.GetString(e.Data)));
        connection.OnRemoteDisconnected.AddObserver(e =>
        {
            remoteDisconnectedTaskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("hello"), TestContext.Current.CancellationToken);
        testPipeServer.Stop();

        await remoteDisconnectedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.Single(receivedData);
        Assert.Equal("Acknowledged!", receivedData[0]);
    }

    [Fact]
    public async Task TestStartingWithoutSettingExternalProcessThrows()
    {
        PipeConnection connection = new(new TestPipeServer());
        await Assert.ThrowsAnyAsync<WebDriverBiDiException>(() => connection.StartAsync("pipe", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestStartingWithoutStoppingThrows()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await connection.StartAsync("pipe", TestContext.Current.CancellationToken));
        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestRemoteEndClosingMarksConnectionAsInactive()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        Assert.True(connection.IsActive);
        testPipeServer.Stop();
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestCanStop()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.False(connection.IsActive);
        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestCanStopWithoutStarting()
    {
        PipeConnection connection = new(new TestPipeServer());
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestCanStopRepeatedly()
    {
        PipeConnection connection = new(new TestPipeServer());
        await connection.StopAsync(TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestSendDataWithoutStartingThrows()
    {
        PipeConnection connection = new(new TestPipeServer());
        await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await connection.SendDataAsync([1, 2, 3], TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestCanLogMessages()
    {
        List<string> receivedData = [];
        TaskCompletionSource remoteDisconnectedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        connection.OnLogMessage.AddObserver(e => receivedData.Add(e.Message));
        connection.OnRemoteDisconnected.AddObserver(e =>
        {
            remoteDisconnectedTaskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        testPipeServer.Responses.Add("Acknowledged!");
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);

        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello"), TestContext.Current.CancellationToken);
        testPipeServer.Stop();

        // Wait for the receive loop to exit gracefully via EOF before calling StopAsync.
        // This ensures "Pipe closed by remote end" and "Ending pipe receive loop" are
        // logged before StopAsync's cancellation token can preempt the ReadAsync.
        await remoteDisconnectedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(8, receivedData.Count);
        Assert.Equivalent(new string[]
        {
            "Starting pipe connection: pipe://local",
            "Pipe connection started",
            "SEND >>> Hello",
            "RECV <<< Acknowledged!",
            "Pipe closed by remote end",
            "Ending pipe receive loop",
            "Closing pipe connection",
            "Pipe connection closed",
        }, receivedData);
    }

    [Fact]
    public async Task TestCanOnlySendOneMessageAtATime()
    {
        TestPipeServer testPipeServer = new();
        TaskCompletionSource sendBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestPipeConnection connection = new(testPipeServer)
        {
            BypassDataSend = false,
            SendBarrier = sendBarrier,
            DataTimeout = TimeSpan.FromMilliseconds(20),
        };

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.OnDataSendStarting.AddObserver(e => taskCompletionSource.TrySetResult());

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        Task firstSendTask = Task.Run(() => connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello"), TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);

        // Wait until the first send has acquired the semaphore and is blocked on the barrier,
        // then attempt a second send which must time out before the barrier releases.
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("World"), TestContext.Current.CancellationToken));
        sendBarrier.SetResult();
        testPipeServer.Stop();

        // The first send may fault with a WebDriverBiDiConnectionException if Stop closed
        // the pipe before the send completed. Observe the exception to prevent
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
    public async Task TestCanDispose()
    {
        PipeConnection connection = new(new TestPipeServer());
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task TestDoubleDisposeAsyncDoesNotThrow()
    {
        PipeConnection connection = new(new TestPipeServer());
        await connection.DisposeAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task TestDoubleDisposeAsyncAfterStartDoesNotThrow()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.DisposeAsync();
        testPipeServer.Stop();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task TestIsDisposedPropertyIsSetAfterDispose()
    {
        TestPipeConnection connection = new(new TestPipeServer());
        Assert.False(connection.Disposed);
        await connection.DisposeAsync();
        Assert.True(connection.Disposed);
    }

    [Fact]
    public async Task TestCanDisposeAsyncAfterStop()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);
        testPipeServer.Stop();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task TestCanDisposeAsyncWithoutStopping()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.DisposeAsync();
        Assert.False(connection.IsActive);
        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestDisposeLogsExceptionFromStop()
    {
        List<LogMessageEventArgs> logs = [];
        TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer);
        connection.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        connection.ThrowOnStop = true;
        await connection.DisposeAsync();
        testPipeServer.Stop();

        Assert.Contains(logs,
            log => log.Message.Contains("Unexpected exception during disposal")
                   && log.Message.Contains("Simulated stop failure")
                   && log.Level == WebDriverBiDiLogLevel.Warn
                   && log.ComponentName == Connection.LoggerComponentName);
    }

    [Fact]
    public async Task TestCanDisposeAsyncStartedConnectionAfterStop()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello"), TestContext.Current.CancellationToken);
        testPipeServer.WaitForDataSent(TimeSpan.FromSeconds(1));
        await connection.StopAsync(TestContext.Current.CancellationToken);
        testPipeServer.Stop();
        await connection.DisposeAsync();
    }

    [Fact]
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
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);

        WebDriverBiDiConnectionException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("data"), TestContext.Current.CancellationToken));
        Assert.Equal("The pipe connection was closed before the send could be completed", exception.Message);

        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestSendDataThrowsWhenCancellationTokenIsCanceled()
    {
        TestPipeConnection connection = new(new TestPipeServer())
        {
            IsActiveOverride = () => true,
        };
        using CancellationTokenSource cts = new();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("test"), cts.Token));
    }

    [Fact]
    public async Task TestStartAfterDisposeThrows()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.DisposeAsync();
        testPipeServer.Stop();

        Assert.Contains("pipes have been disposed", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestStartAfterServerProcessExitThrows()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        testPipeServer.Stop();

        Assert.Contains("External process has already exited or been disposed", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestStartWithDisposedServerProcessThrows()
    {
        // An unstarted Process instance throws InvalidOperationException when
        // HasExited is read. This covers the catch branch in IsProcessRunning
        // where the process reference has been disposed by its owner.
        UnstartedProcessPipeProvider provider = new();
        PipeConnection connection = new(provider);

        Assert.Contains("External process has already exited or been disposed", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestIsActiveFalseWhenProviderReturnsNullAfterStart()
    {
        // After a successful StartAsync, IsConnectionActive is true, so
        // IsActive reaches the process check. Flipping the provider to
        // return null exercises the null branch of IsProcessRunning
        // without tearing down the underlying pipe server.
        TestPipeServer realServer = new();
        MutableProcessPipeProvider wrapper = new(realServer);
        PipeConnection connection = new(wrapper);
        realServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        try
        {
            await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
            Assert.True(connection.IsActive);

            wrapper.ReturnNull = true;
            Assert.False(connection.IsActive);
        }
        finally
        {
            wrapper.ReturnNull = false;
            realServer.Stop();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task TestSendDataWrapsIOExceptionInConnectionException()
    {
        TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer)
        {
            IsActiveOverride = () => true,
            ThrowIOExceptionOnSend = true,
        };

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);

        WebDriverBiDiConnectionException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("data"), TestContext.Current.CancellationToken));
        Assert.Contains("An error occurred while sending data", exception.Message);
        Assert.IsType<IOException>(exception.InnerException);

        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestSendDataWrapsObjectDisposedExceptionInConnectionException()
    {
        TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer)
        {
            IsActiveOverride = () => true,
            ThrowObjectDisposedExceptionOnSend = true,
        };

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);

        WebDriverBiDiConnectionException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("data"), TestContext.Current.CancellationToken));
        Assert.Contains("An error occurred while sending data", exception.Message);
        Assert.IsType<ObjectDisposedException>(exception.InnerException);

        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestPipeHandlesReturnEmptyStringAfterDisposal()
    {
        TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.DisposeAsync();

        // After disposal, pipes are null, so handles should return empty string

        Assert.Equal(string.Empty, connection.ReadPipeHandle);
        Assert.Equal(string.Empty, connection.WritePipeHandle);

        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestReceiveDataRaisesErrorEventOnIOException()
    {
        ConnectionErrorEventArgs? receivedErrorArgs = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestPipeServer testPipeServer = new();

        TestPipeConnection connection = new(testPipeServer)
        {
            ThrowIOExceptionOnReceive = true,
        };
        connection.OnConnectionError.AddObserver(e =>
        {
            receivedErrorArgs = e;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);

        // Wait for error event (TestPipeConnection returns fake data on first read, then throws on second)
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        testPipeServer.Stop();

        Assert.NotNull(receivedErrorArgs);
        Assert.IsType<IOException>(receivedErrorArgs.Exception);
    }

    [Fact]
    public async Task TestReceiveDataRaisesErrorEventOnObjectDisposedException()
    {
        ConnectionErrorEventArgs? receivedErrorArgs = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestPipeServer testPipeServer = new();

        TestPipeConnection connection = new(testPipeServer)
        {
            ThrowObjectDisposedExceptionOnReceive = true,
        };
        connection.OnConnectionError.AddObserver(e =>
        {
            receivedErrorArgs = e;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);

        // Wait for error event (TestPipeConnection returns fake data on first read, then throws on second)
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        testPipeServer.Stop();

        Assert.NotNull(receivedErrorArgs);
        Assert.IsType<ObjectDisposedException>(receivedErrorArgs.Exception);
    }

    [Fact]
    public async Task TestPipesDisposedPropertySetterBothBranches()
    {
        TestPipeConnection connection = new(new TestPipeServer())
        {
            // Test setting to true (one branch of ternary in setter)
            PipesDisposed = true
        };
        Assert.True(connection.PipesDisposed);

        // Test setting to false (other branch of ternary in setter)
        connection.PipesDisposed = false;
        Assert.False(connection.PipesDisposed);

        // Test setting to true again to ensure it works both ways
        connection.PipesDisposed = true;
        Assert.True(connection.PipesDisposed);
    }

    [Fact]
    public async Task TestReadPipeHandleWhenPipesNotDisposed()
    {
        TestPipeConnection connection = new(new TestPipeServer());
        connection.PipesDisposed = false;

        // When pipes are not disposed, should return the actual handle
        string handle = connection.ReadPipeHandle;
        Assert.NotEmpty(handle);
    }

    [Fact]
    public async Task TestWritePipeHandleWhenPipesNotDisposed()
    {
        TestPipeConnection connection = new(new TestPipeServer());
        connection.PipesDisposed = false;

        // When pipes are not disposed, should return the actual handle
        string handle = connection.WritePipeHandle;
        Assert.NotEmpty(handle);
    }

    [Fact]
    public async Task TestReadPipeHandleWhenPipesDisposed()
    {
        TestPipeConnection connection = new(new TestPipeServer());
        connection.PipesDisposed = true;

        // When pipes are disposed, should return empty string
        string handle = connection.ReadPipeHandle;
        Assert.Empty(handle);
    }

    [Fact]
    public async Task TestWritePipeHandleWhenPipesDisposed()
    {
        TestPipeConnection connection = new(new TestPipeServer());
        connection.PipesDisposed = true;

        // When pipes are disposed, should return empty string
        string handle = connection.WritePipeHandle;
        Assert.Empty(handle);
    }

    private sealed class UnstartedProcessPipeProvider : IPipeServerProcessProvider
    {
        private readonly Process unstartedProcess = new();

        public Process? PipeServerProcess => this.unstartedProcess;
    }

    private sealed class MutableProcessPipeProvider : IPipeServerProcessProvider
    {
        private readonly IPipeServerProcessProvider inner;

        public MutableProcessPipeProvider(IPipeServerProcessProvider inner)
        {
            this.inner = inner;
        }

        public bool ReturnNull { get; set; }

        public Process? PipeServerProcess => this.ReturnNull ? null : this.inner.PipeServerProcess;
    }
}
