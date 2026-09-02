namespace WebDriverBiDi.Protocol;

using System.Diagnostics;
using System.Text;
using WebDriverBiDi.TestUtilities;

public class PipeConnectionTests
{
    [Fact]
    public void TestConstructorThrowsForNullProcessProvider()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new PipeConnection(null!));
        Assert.Equal("processProvider", exception.ParamName);
    }

    [Fact]
    public async Task TestConnectionType()
    {
        using TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        Assert.Equal(ConnectionKind.Pipes, connection.ConnectionKind);
    }

    [Fact]
    public async Task TestCanSendData()
    {
        using TestPipeServer testPipeServer = new();

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
        using TestPipeServer testPipeServer = new();
        testPipeServer.Responses.Add("Acknowledged!");

        List<string> receivedData = [];
        PipeConnection connection = new(testPipeServer);
        connection.OnDataReceived.AddObserver(e => receivedData.Add(Encoding.UTF8.GetString(e.Data.ToArray())));
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
        using TestPipeServer testPipeServer = new();
        testPipeServer.Responses.Add("Acknowledged!\\0More data");

        List<string> receivedData = [];
        PipeConnection connection = new(testPipeServer);
        connection.OnDataReceived.AddObserver(e => receivedData.Add(Encoding.UTF8.GetString(e.Data.ToArray())));
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
    public async Task TestReceivesMessageSpanningMultipleReadsLargerThanInitialBuffer()
    {
        // A single message delivered across three reads, each filling the read buffer, must be
        // accumulated in pooled memory (growing twice) and delivered once, intact, when the
        // null terminator finally arrives.
        TaskCompletionSource<byte[]> receivedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource remoteDisconnectedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer);

        int readSize = connection.BufferSize;
        byte[] expected = new byte[(readSize * 2) + (readSize / 2)];
        for (int i = 0; i < expected.Length; i++)
        {
            // Never emit a zero byte, which the pipe protocol treats as a message terminator.
            expected[i] = (byte)((i % 255) + 1);
        }

        int delivered = 0;
        connection.ReadHandler = (buffer, offset, count, callNumber) =>
        {
            if (delivered < expected.Length)
            {
                int chunk = Math.Min(count, expected.Length - delivered);
                Array.Copy(expected, delivered, buffer, offset, chunk);
                delivered += chunk;
                if (delivered == expected.Length && chunk < count)
                {
                    // Room remains in this read for the terminator.
                    buffer[offset + chunk] = 0;
                    chunk++;
                }

                return Task.FromResult(chunk);
            }

            // Pipe closed by the remote end.
            return Task.FromResult(0);
        };

        connection.OnDataReceived.AddObserver(e => receivedTaskCompletionSource.TrySetResult(e.Data.ToArray()));
        connection.OnRemoteDisconnected.AddObserver(e =>
        {
            remoteDisconnectedTaskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);

        byte[] received = await receivedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await remoteDisconnectedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        testPipeServer.Stop();
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(expected.Length, received.Length);
        Assert.True(received.AsSpan().SequenceEqual(expected), "Reassembled message content did not match the data that was read");
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
        using TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await connection.StartAsync("pipe", TestContext.Current.CancellationToken));
        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestRemoteEndClosingMarksConnectionAsInactive()
    {
        using TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        Assert.True(connection.IsActive);
        testPipeServer.Stop();
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestRemoteEndOfFileWhileProcessRunningMarksConnectionInactiveAndAllowsRestart()
    {
        // The remote end can close its end of the pipe while its process keeps running, so
        // IsActive cannot rely on the process check alone; the end-of-file must clear the
        // connection's active flag, and a subsequent StartAsync must be able to begin a
        // new session.
        TaskCompletionSource remoteDisconnectedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<int> secondSessionReadBlock = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer)
        {
            ReadHandler = (buffer, offset, count, callNumber) =>
                callNumber == 1 ? Task.FromResult(0) : secondSessionReadBlock.Task,
        };
        connection.OnRemoteDisconnected.AddObserver(e =>
        {
            remoteDisconnectedTaskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await remoteDisconnectedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // The server process is still running; only the pipe reached end-of-file.
        Assert.False(testPipeServer.PipeServerProcess!.HasExited);
        Assert.False(connection.IsActive);

        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        Assert.True(connection.IsActive);

        secondSessionReadBlock.SetResult(0);
        await connection.StopAsync(TestContext.Current.CancellationToken);
        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestCanStop()
    {
        using TestPipeServer testPipeServer = new();
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
        await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await connection.SendDataAsync(new byte[] { 1, 2, 3 }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestCanLogMessages()
    {
        List<string> receivedData = [];
        TaskCompletionSource remoteDisconnectedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestPipeServer testPipeServer = new();
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

            // The end-of-file already marked the connection inactive, so StopAsync
            // takes its already-closed early return.
            "Pipe connection already closed",
        }, receivedData);
    }

    [Fact]
    public async Task TestStartAsyncRefusesSecondReceiveLoopWhileAbandonedLoopStillRuns()
    {
        // StopAsync abandons a receive loop that does not respond to cancellation (see its
        // remarks). Restarting while that loop is still blocked must be refused: a second
        // loop reading the same pipe would interleave reads arbitrarily and corrupt message
        // framing. Once the abandoned loop finally exits — discarding its stale read result
        // rather than dispatching it — a new session can start.
        TaskCompletionSource<int> receiveBlockSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource receiveBlockEnteredSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource receiveLoopEndedSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        List<string> receivedData = [];
        using TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer)
        {
            ReceiveBlockSignal = receiveBlockSignal,
            ReceiveBlockEnteredSignal = receiveBlockEnteredSignal,
            ShutdownTimeout = TimeSpan.FromMilliseconds(50),
        };
        connection.OnDataReceived.AddObserver(e => receivedData.Add(Encoding.UTF8.GetString(e.Data.ToArray())));
        connection.OnLogMessage.AddObserver(e =>
        {
            if (e.Message == "Ending pipe receive loop")
            {
                receiveLoopEndedSignal.TrySetResult();
            }

            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await receiveBlockEnteredSignal.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // StopAsync times out waiting for the blocked read and abandons the loop.
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.False(connection.IsActive);

        WebDriverBiDiConnectionException exception = await Assert.ThrowsAsync<WebDriverBiDiConnectionException>(
            () => connection.StartAsync("pipe://local", TestContext.Current.CancellationToken));
        Assert.Contains("receive loop from a previous session", exception.Message);

        // Release the blocked read. The loop observes its canceled token and exits without
        // dispatching the read result.
        receiveBlockSignal.SetResult(0);
        await receiveLoopEndedSignal.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Empty(receivedData);

        // With the previous loop finished, a new session can start against the real pipe.
        connection.ReceiveBlockSignal = null;
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        Assert.True(connection.IsActive);

        await connection.StopAsync(TestContext.Current.CancellationToken);
        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestStopHonorsShutdownTimeoutWhenReceiveLoopDoesNotRespondToCancellation()
    {
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource<int> receiveBlockSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource receiveBlockEnteredSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource receiveLoopEndedSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer)
        {
            ReceiveBlockSignal = receiveBlockSignal,
            ReceiveBlockEnteredSignal = receiveBlockEnteredSignal,
            ShutdownTimeout = TimeSpan.FromMilliseconds(50),
        };
        connection.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            if (e.Message == "Ending pipe receive loop")
            {
                receiveLoopEndedSignal.TrySetResult();
            }

            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);

        // The background receive loop starts on its own task, so without waiting for this
        // signal, StopAsync could cancel the token before the loop ever reaches its first
        // read — the loop's own cancellation check would then exit it immediately, never
        // touching receiveBlockSignal, and defeating the point of this test. Waiting here
        // guarantees the loop is actually blocked in the read before shutdown begins.
        await receiveBlockEnteredSignal.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // The receive loop is blocked on receiveBlockSignal and ignores the cancellation
        // token that StopAsync signals, simulating a pipe read that does not unblock
        // promptly on cancellation. StopAsync must not hang waiting for it; it should
        // return once ShutdownTimeout elapses and log a warning.
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.False(connection.IsActive);
        Assert.Contains(logs, log =>
            log.Message == "Timed out waiting for Pipes connection receive loop to complete during shutdown"
            && log.Level == WebDriverBiDiLogLevel.Warn
            && log.ComponentName == Connection.LoggerComponentName);

        // Release the blocked receive loop and let it drain gracefully before tearing down,
        // so the background task does not outlive the test and leak into later tests.
        receiveBlockSignal.TrySetResult(0);
        await receiveLoopEndedSignal.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        testPipeServer.Stop();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task TestCanOnlySendOneMessageAtATime()
    {
        using TestPipeServer testPipeServer = new();
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
    public async Task TestReceiveLoopFaultIsObservedAndDoesNotRaiseUnobservedTaskException()
    {
        // The receive loop runs on a fire-and-forget task, and StopAsync only waits on it with
        // Task.WhenAny, which does not observe a fault. Without the fault-observing continuation
        // attached by Connection.ObserveReceiveLoopFault, a receive loop that faults leaves its
        // exception unobserved until the finalizer raises TaskScheduler.UnobservedTaskException,
        // which then surfaces as a failure in whatever unrelated test happens to force the next
        // garbage collection.
        using UnobservedTaskExceptionMonitor monitor = new("simulated receive loop fault");

        // The connection and its receive task are created inside a separate method so that
        // nothing roots them once it returns, letting the collection below finalize the task.
        await StartAndDisposeFaultingConnectionAsync();

        // Force garbage collection to trigger UnobservedTaskException
        // for any task whose exception was not observed.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.False(monitor.Raised, monitor.Exception?.ToString());

        static async Task StartAndDisposeFaultingConnectionAsync()
        {
            using TestPipeServer testPipeServer = new();
            TestPipeConnection connection = new(testPipeServer)
            {
                ReceiveLoopOuterFault = new InvalidOperationException("simulated receive loop fault"),
            };

            testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
            await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
            testPipeServer.Stop();
            await connection.DisposeAsync();
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
        using TestPipeServer testPipeServer = new();
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
        using TestPipeServer testPipeServer = new();
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
        using TestPipeServer testPipeServer = new();
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
        using TestPipeServer testPipeServer = new();
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
        using TestPipeServer testPipeServer = new();
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
        using TestPipeServer testPipeServer = new();
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
        Assert.Equal("The Pipes connection was closed before the send could be completed", exception.Message);

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
    public async Task TestSendDataWithDefaultCancellationTokenUsesConnectionToken()
    {
        using TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer)
        {
            IsActiveOverride = () => true,
        };

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);

#pragma warning disable xUnit1051 // intentionally omits token to exercise the CancellationToken.None branch
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello world"));
#pragma warning restore xUnit1051
        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestStartAfterDisposeThrows()
    {
        using TestPipeServer testPipeServer = new();
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
        using TestPipeServer testPipeServer = new();
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
        using TestPipeServer realServer = new();
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
        using TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer)
        {
            IsActiveOverride = () => true,
            ThrowIOExceptionOnSend = true,
            BypassDataSend = false,
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
        using TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer)
        {
            IsActiveOverride = () => true,
            ThrowObjectDisposedExceptionOnSend = true,
            BypassDataSend = false,
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
        using TestPipeServer testPipeServer = new();
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
        using TestPipeServer testPipeServer = new();

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

        // The receive loop has exited, so the connection must report inactive even though
        // the server process is still running.
        Assert.False(connection.IsActive);
        testPipeServer.Stop();

        Assert.NotNull(receivedErrorArgs);
        Assert.IsType<IOException>(receivedErrorArgs.Exception);
    }

    [Fact]
    public async Task TestReceiveDataRaisesErrorEventOnObjectDisposedException()
    {
        ConnectionErrorEventArgs? receivedErrorArgs = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestPipeServer testPipeServer = new();

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

        // The receive loop has exited, so the connection must report inactive even though
        // the server process is still running.
        Assert.False(connection.IsActive);
        testPipeServer.Stop();

        Assert.NotNull(receivedErrorArgs);
        Assert.IsType<ObjectDisposedException>(receivedErrorArgs.Exception);
    }

    [Fact]
    public async Task TestReceiveDataRaisesErrorEventOnDataReceivedObserverException()
    {
        // An exception from the observer of OnDataReceived is rethrown by the event into the
        // receive loop. Were it not caught there, the loop would end without notice: the pipe
        // would remain open while no further message was ever delivered, which a caller awaiting
        // a command response cannot distinguish from a remote end that has simply gone quiet.
        static Task ThrowOnDataReceived(ConnectionDataReceivedEventArgs e) => throw new InvalidOperationException("observer failure");

        ConnectionErrorEventArgs? receivedErrorArgs = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestPipeServer testPipeServer = new();
        testPipeServer.Responses.Add("Acknowledged!");

        PipeConnection connection = new(testPipeServer);
        connection.OnDataReceived.AddObserver(ThrowOnDataReceived);
        connection.OnConnectionError.AddObserver(e =>
        {
            receivedErrorArgs = e;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("hello"), TestContext.Current.CancellationToken);

        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // The receive loop has exited, so the connection must report inactive even though
        // the server process is still running.
        Assert.False(connection.IsActive);
        testPipeServer.Stop();
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(receivedErrorArgs);
        Assert.Equal("observer failure", Assert.IsType<InvalidOperationException>(receivedErrorArgs.Exception).Message);
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
