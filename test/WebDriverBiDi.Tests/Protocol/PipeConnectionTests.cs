namespace WebDriverBiDi.Protocol;

using System.Diagnostics;
using System.Text;
using WebDriverBiDi.TestUtilities;

public class PipeConnectionTests
{
    // A safety bound, not a timing expectation: the reads it guards complete as soon as the child echoes,
    // so this only turns a hang into a failure. It is deliberately far larger than any plausible dotnet
    // process start so that a slow or loaded CI machine cannot fail the test.
    private static readonly TimeSpan DataEchoSafetyBound = TimeSpan.FromSeconds(30);

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
        await using PipeConnection connection = new(testPipeServer);
        Assert.Equal(ConnectionKind.Pipes, connection.ConnectionKind);
    }

    [Fact]
    public async Task TestCanSendData()
    {
        using TestPipeServer testPipeServer = new();

        await using PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);

        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello"), TestContext.Current.CancellationToken);

        using CancellationTokenSource readCancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        readCancellation.CancelAfter(DataEchoSafetyBound);
        string output = await testPipeServer.ReadSentDataAsync("Hello".Length, readCancellation.Token);
        testPipeServer.Stop();

        Assert.Equal("Hello", output);
    }

    [Fact]
    public async Task TestCanReceiveData()
    {
        TaskCompletionSource remoteDisconnectedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestPipeServer testPipeServer = new();
        testPipeServer.Responses.Add("Acknowledged!");

        List<string> receivedData = [];
        await using PipeConnection connection = new(testPipeServer);
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
        await using PipeConnection connection = new(testPipeServer);
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
    public async Task TestConnectionDeliversNothingForZeroLengthMessage()
    {
        // A null terminator with no bytes before it frames a message with no content. Nothing may be
        // delivered for it, whether it leads the read or is one of a pair of adjacent terminators, and
        // the messages surrounding it must still arrive intact and in order.
        TaskCompletionSource remoteDisconnectedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer);

        byte[] pipeData = Encoding.UTF8.GetBytes("\0Acknowledged!\0\0Done\0");

        List<string> receivedData = [];
        connection.ReadHandler = (buffer, offset, count, callNumber) =>
        {
            if (callNumber == 1)
            {
                pipeData.CopyTo(buffer, offset);
                return Task.FromResult(pipeData.Length);
            }

            // Pipe closed by the remote end.
            return Task.FromResult(0);
        };

        connection.OnDataReceived.AddObserver(e =>
        {
            receivedData.Add(Encoding.UTF8.GetString(e.Data.ToArray()));
            return Task.CompletedTask;
        });
        connection.OnRemoteDisconnected.AddObserver(e =>
        {
            remoteDisconnectedTaskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);

        // The read reporting the pipe closed ends the receive loop, and every message framed by the
        // read before it has been dispatched by that point, so the list is final here without a
        // wall-clock wait.
        await remoteDisconnectedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        testPipeServer.Stop();
        await connection.StopAsync(TestContext.Current.CancellationToken);

        // The leading terminator and the adjacent pair each frame a zero-length message; only the two
        // messages carrying content are delivered.
        Assert.Equal(2, receivedData.Count);
        Assert.Equal("Acknowledged!", receivedData[0]);
        Assert.Equal("Done", receivedData[1]);
    }

    [Fact]
    public async Task TestStartingWithoutSettingExternalProcessThrows()
    {
        await using PipeConnection connection = new(new TestPipeServer());
        await Assert.ThrowsAnyAsync<WebDriverBiDiException>(() => connection.StartAsync("pipe", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestStartingWithAlreadyCanceledTokenThrows()
    {
        // Starting a pipe connection performs no cancellable I/O, so entry is the one point at which
        // the caller's token can be observed; a caller who has already given up is honored there.
        using TestPipeServer testPipeServer = new();
        await using PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        using CancellationTokenSource canceledTokenSource = new();
        canceledTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await connection.StartAsync("pipe://local", canceledTokenSource.Token));
        Assert.False(connection.IsActive);

        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestStartingWithoutStoppingThrows()
    {
        using TestPipeServer testPipeServer = new();
        await using PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await connection.StartAsync("pipe", TestContext.Current.CancellationToken));
        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestRemoteEndClosingMarksConnectionAsInactive()
    {
        using TestPipeServer testPipeServer = new();
        await using PipeConnection connection = new(testPipeServer);
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
        await using PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.False(connection.IsActive);
        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestCanStopWithoutStarting()
    {
        await using PipeConnection connection = new(new TestPipeServer());
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestCanStopRepeatedly()
    {
        await using PipeConnection connection = new(new TestPipeServer());
        await connection.StopAsync(TestContext.Current.CancellationToken);
        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestAFailedTransportShutdownStillCancelsTheConnection()
    {
        // The failure is reported to the caller, but the connection must not be left with a receive
        // loop running against a session its owner believes is finished with, so Connection.StopAsync
        // cancels and waits for the loop whether or not the transport-specific shutdown succeeded.
        using TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);

        CancellationToken sessionToken = connection.ObservedConnectionCancellationToken;
        connection.ThrowOnStop = true;

        await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await connection.StopAsync(TestContext.Current.CancellationToken));
        Assert.True(sessionToken.IsCancellationRequested);
        Assert.Equal(string.Empty, connection.ConnectionString);

        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestStoppingWithoutStartingCancelsTheConnectionAndStillPermitsAStart()
    {
        // Connection.StopAsync cancels the connection whatever state it was in, including one that
        // was never started, so that stopping always means the same thing. That is only safe because
        // StartAsync replaces the cancellation source: a session that inherited the canceled one
        // could never connect.
        using TestPipeServer testPipeServer = new();
        await using TestPipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);

        CancellationToken tokenBeforeStop = connection.ObservedConnectionCancellationToken;
        Assert.False(tokenBeforeStop.IsCancellationRequested);

        await connection.StopAsync(TestContext.Current.CancellationToken);
        Assert.True(tokenBeforeStop.IsCancellationRequested);

        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        Assert.True(connection.IsActive);
        Assert.False(connection.ObservedConnectionCancellationToken.IsCancellationRequested);

        await connection.StopAsync(TestContext.Current.CancellationToken);
        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestSendDataWithoutStartingThrows()
    {
        await using PipeConnection connection = new(new TestPipeServer());
        await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await connection.SendDataAsync(new byte[] { 1, 2, 3 }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestCanLogMessages()
    {
        List<string> receivedData = [];
        TaskCompletionSource remoteDisconnectedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestPipeServer testPipeServer = new();
        await using PipeConnection connection = new(testPipeServer);
        // This test asserts on Debug or Trace messages, which the default minimum level excludes.
        connection.LogLevel = WebDriverBiDiLogLevel.Trace;
        connection.OnDataReceived.AddObserver(e => Task.CompletedTask);
        object logLock = new();
        connection.OnLogMessage.AddObserver(e =>
        {
            lock (logLock)
            {
                receivedData.Add(e.Message);
            }
        });
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

        string[] logSnapshot;
        lock (logLock)
        {
            logSnapshot = [.. receivedData];
        }

        // Strict equivalence rather than ordered equality: the peer writes its canned responses as
        // soon as it starts, which is necessarily before StartAsync runs, so the receive loop can log
        // "RECV <<<" before the test thread logs "Pipes connection opened". Strict mode still rejects
        // a missing or an extra entry; only the relative order of those two is not guaranteed.
        Assert.Equal(9, logSnapshot.Length);
        Assert.Equivalent(new string[]
        {
            // The messages that bracket the session are logged by Connection.StartAsync and
            // Connection.StopAsync, which every connection shares; the rest come from the pipe
            // connection itself.
            "Opening Pipes connection to pipe://local",
            "Pipes connection opened",
            "SEND >>> Hello",
            "RECV <<< Acknowledged!",
            "Pipe closed by remote end",
            "Ending pipe receive loop",
            "Closing Pipes connection",

            // The end-of-file already marked the connection inactive, so the pipe connection's own
            // shutdown has nothing left to record.
            "Pipe connection is not active",
            "Pipes connection closed",
        }, logSnapshot, strict: true);
    }

    [Fact]
    public async Task TestStartAsyncRefusesSecondReceiveLoopWhileAbandonedLoopStillRuns()
    {
        // StopAsync abandons a receive loop that does not respond to cancellation (see its
        // remarks). Restarting while that loop is still blocked must be refused: a second
        // loop reading the same pipe would interleave reads arbitrarily and corrupt message
        // framing. Once the abandoned loop finally exits, a new session can start. (That a read
        // completing with data after cancellation is discarded rather than dispatched is covered by
        // TestReceiveLoopDiscardsDataReadAfterTheConnectionIsCanceled; the read released below
        // returns end-of-file, which dispatches nothing either way.)
        TaskCompletionSource<int> receiveBlockSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource receiveBlockEnteredSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource receiveLoopEndedSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        List<string> receivedData = [];
        using TestPipeServer testPipeServer = new();
        TestTimeProvider timeProvider = new();
        TestPipeConnection connection = new(testPipeServer, timeProvider)
        {
            ReceiveBlockSignal = receiveBlockSignal,
            ReceiveBlockEnteredSignal = receiveBlockEnteredSignal,
            ShutdownTimeout = TimeSpan.FromSeconds(10),
        };
        connection.OnDataReceived.AddObserver(e => receivedData.Add(Encoding.UTF8.GetString(e.Data.ToArray())));
        connection.OnLogMessage.AddObserver(e =>
        {
            if (e.Message.StartsWith("Ending pipe receive loop", StringComparison.Ordinal))
            {
                receiveLoopEndedSignal.TrySetResult();
            }

            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await receiveBlockEnteredSignal.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // StopAsync times out waiting for the blocked read and abandons the loop; the shutdown
        // timeout is elapsed on the virtual clock as soon as the stop arms it.
        Task stopTask = connection.StopAsync(TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(stopTask, connection.ShutdownTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await stopTask;
        Assert.False(connection.IsActive);

        // The restart gives the abandoned loop a bounded chance to finish, on the virtual clock,
        // before refusing.
        Task refusedStartTask = connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(refusedStartTask, connection.ShutdownTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        WebDriverBiDiConnectionException exception = await Assert.ThrowsAsync<WebDriverBiDiConnectionException>(() => refusedStartTask);
        Assert.Contains("receive loop from a previous session", exception.Message);

        // Release the blocked read with end-of-file. The loop observes its canceled token and exits.
        receiveBlockSignal.SetResult(0);
        await receiveLoopEndedSignal.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Empty(receivedData);

        // With the previous loop finished, a new session can start against the real pipe.
        connection.ReceiveBlockSignal = null;
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        Assert.True(connection.IsActive);

        // A pipe read does not observe cancellation, so this stop also ends by its shutdown timeout,
        // which is on the virtual clock.
        Task finalStopTask = connection.StopAsync(TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(finalStopTask, connection.ShutdownTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await finalStopTask;
        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestReceiveLoopDiscardsDataReadAfterTheConnectionIsCanceled()
    {
        // A pipe read does not reliably observe cancellation, so a read can complete with data after the
        // connection has been stopped. That data belongs to no session, and dispatching it would hand stale bytes
        // to the observers of a connection that is already stopped. The read below completes only once the
        // connection's own token has been canceled, and it completes with a whole message rather than with
        // end-of-file, so the loop takes its data path and only its cancellation check stands between the
        // message and the observers. An end-of-file read would not test that check at all, because the loop ends
        // on end-of-file without dispatching anything either way.
        List<string> receivedData = [];
        int remoteDisconnectedCount = 0;
        TaskCompletionSource readEnteredSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<int> readReturnedSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource receiveLoopEndedSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        List<LogMessageEventArgs> logs = [];
        using TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer);
        connection.ReadHandler = async (buffer, offset, count, callNumber) =>
        {
            // Released by the connection's own cancellation, so the data is delivered after the stop has canceled
            // the session, exactly as a read that ignored the token would deliver it.
            TaskCompletionSource readReleased = new(TaskCreationOptions.RunContinuationsAsynchronously);
            using CancellationTokenRegistration registration = connection.ObservedConnectionCancellationToken.Register(() => readReleased.TrySetResult());
            readEnteredSignal.TrySetResult();
            await readReleased.Task;

            byte[] staleMessage = Encoding.UTF8.GetBytes("stale message\0");
            Array.Copy(staleMessage, 0, buffer, offset, staleMessage.Length);
            readReturnedSignal.TrySetResult(staleMessage.Length);
            return staleMessage.Length;
        };
        connection.OnDataReceived.AddObserver(e => receivedData.Add(Encoding.UTF8.GetString(e.Data.ToArray())));
        connection.OnRemoteDisconnected.AddObserver(e =>
        {
            Interlocked.Increment(ref remoteDisconnectedCount);
        });
        connection.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            if (e.Message.StartsWith("Ending pipe receive loop", StringComparison.Ordinal))
            {
                receiveLoopEndedSignal.TrySetResult();
            }

            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);

        // The receive loop starts on its own task. Stopping before it reaches its first read would end the loop at
        // its while-condition, the read would never run, and the test would pass without exercising the check.
        await readEnteredSignal.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        await connection.StopAsync(TestContext.Current.CancellationToken);

        // The stop waits for the loop, and the loop ends as soon as the released read returns, so by now the read
        // has delivered its message and the loop has decided what to do with it.
        Assert.True(readReturnedSignal.Task.IsCompleted, "The blocked read did not return, so the loop never saw its data.");
        Assert.True(receiveLoopEndedSignal.Task.IsCompleted, "The receive loop did not end before the stop returned.");
        Assert.DoesNotContain(logs, log => log.Message.StartsWith("Timed out waiting", StringComparison.Ordinal));

        Assert.Empty(receivedData);

        // A stop that the loop observed is not a remote disconnection.
        Assert.Equal(0, remoteDisconnectedCount);

        testPipeServer.Stop();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task TestStopHonorsShutdownTimeoutWhenReceiveLoopDoesNotRespondToCancellation()
    {
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource<int> receiveBlockSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource receiveBlockEnteredSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource receiveLoopEndedSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestPipeServer testPipeServer = new();
        TestTimeProvider timeProvider = new();
        TestPipeConnection connection = new(testPipeServer, timeProvider)
        {
            ReceiveBlockSignal = receiveBlockSignal,
            ReceiveBlockEnteredSignal = receiveBlockEnteredSignal,
            ShutdownTimeout = TimeSpan.FromSeconds(10),
        };
        connection.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            if (e.Message.StartsWith("Ending pipe receive loop", StringComparison.Ordinal))
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
        // return once ShutdownTimeout elapses and log a warning. The timeout is elapsed on the
        // virtual clock as soon as the stop arms it.
        Task stopTask = connection.StopAsync(TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(stopTask, connection.ShutdownTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await stopTask;

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
        TestTimeProvider timeProvider = new();
        TestPipeConnection connection = new(testPipeServer, timeProvider)
        {
            BypassDataSend = false,
            SendBarrier = sendBarrier,
            DataTimeout = TimeSpan.FromSeconds(10),
        };

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.OnDataSendStarting.AddObserver(e => taskCompletionSource.TrySetResult());

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        Task firstSendTask = Task.Run(() => connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello"), TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);

        // Wait until the first send has acquired the semaphore and is blocked on the barrier,
        // then attempt a second send which must time out before the barrier releases.
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        // The data timeout is elapsed on the virtual clock as soon as the second send arms it.
        Task secondSendTask = connection.SendDataAsync(Encoding.UTF8.GetBytes("World"), TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(secondSendTask, connection.DataTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);

        // The message is asserted, not merely the exception type. This throw comes from
        // Connection.SendDataAsync, which serves every transport, so a message naming one of them
        // would be wrong here and nothing else in this test would notice.
        Assert.Equal("Timed out waiting to access connection for sending; only one send operation is permitted at a time.", (await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await secondSendTask)).Message);
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

        using CancellationTokenSource readCancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        readCancellation.CancelAfter(DataEchoSafetyBound);
        Assert.Equal("Hello", await testPipeServer.ReadSentDataAsync("Hello".Length, readCancellation.Token));

        await connection.StopAsync(TestContext.Current.CancellationToken);
        testPipeServer.Stop();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task TestSendDataThrowsWhenConnectionBecomesInactiveAfterSemaphoreAcquired()
    {
        int isActiveCallCount = 0;
        using TestPipeServer testPipeServer = new();
        await using TestPipeConnection connection = new(testPipeServer);

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);

        // Installed after the connection is started, because Connection.StartAsync refuses to start a
        // connection that already reports itself as active.
        connection.IsConnectionOpenOverride = () =>
        {
            int count = Interlocked.Increment(ref isActiveCallCount);
            return count <= 1;
        };

        WebDriverBiDiConnectionException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("data"), TestContext.Current.CancellationToken));
        Assert.Equal("The Pipes connection was closed before the send could be completed", exception.Message);

        testPipeServer.Stop();
    }

    [Fact]
    public async Task TestSendDataThrowsWhenCancellationTokenIsCanceled()
    {
        await using TestPipeConnection connection = new(new TestPipeServer())
        {
            IsConnectionOpenOverride = () => true,
        };
        using CancellationTokenSource cts = new();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("test"), cts.Token));
    }

    [Fact]
    public async Task TestSendDataWithDefaultCancellationTokenUsesConnectionToken()
    {
        using TestPipeServer testPipeServer = new();
        await using TestPipeConnection connection = new(testPipeServer);

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        connection.IsConnectionOpenOverride = () => true;

#pragma warning disable xUnit1051 // intentionally omits token to exercise the CancellationToken.None branch
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello world"));
#pragma warning restore xUnit1051
        testPipeServer.Stop();

        // The name of this test is a claim about which token the send used, so assert it: with no
        // caller token there is nothing to link, and the connection's own token is passed straight
        // down. Previously the test asserted nothing at all and passed whatever token was used.
        Assert.Equal(connection.ObservedConnectionCancellationToken, connection.LastSendCancellationToken);
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

        Assert.Contains("Pipes connection has been disposed", (await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestStopAfterDisposeDoesNothing()
    {
        using TestPipeServer testPipeServer = new();
        PipeConnection connection = new(testPipeServer);
        List<string> logMessages = [];
        connection.OnLogMessage.AddObserver(e => logMessages.Add(e.Message));
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.DisposeAsync();
        testPipeServer.Stop();
        logMessages.Clear();

        // Disposal has already stopped the connection and disposed its cancellation source, so a stop
        // after it neither throws (the source's Cancel would) nor raises the "Closing" log message.
        await connection.StopAsync(TestContext.Current.CancellationToken);

        Assert.Empty(logMessages);
        Assert.False(connection.IsActive);
    }

    [Fact]
    public async Task TestStartAfterServerProcessExitThrows()
    {
        using TestPipeServer testPipeServer = new();
        await using PipeConnection connection = new(testPipeServer);
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

        // StartAsync publishes the connection string before the connect and takes it back when the
        // connect fails, so a connection that never connected reports itself as connected to nothing.
        Assert.Equal(string.Empty, connection.ConnectionString);
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
        await using TestPipeConnection connection = new(testPipeServer)
        {
            ThrowIOExceptionOnSend = true,
            BypassDataSend = false,
        };

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        connection.IsConnectionOpenOverride = () => true;

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
            ThrowObjectDisposedExceptionOnSend = true,
            BypassDataSend = false,
        };

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        connection.IsConnectionOpenOverride = () => true;

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
        object logLock = new();
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestPipeServer testPipeServer = new();

        await using TestPipeConnection connection = new(testPipeServer)
        {
            ThrowIOExceptionOnReceive = true,
        };

        // The receive loop logs the failure before raising OnConnectionError, so the wait on the
        // error event below also orders the log message deterministically.
        connection.OnLogMessage.AddObserver(e =>
        {
            lock (logLock)
            {
                logs.Add(e);
            }

            return Task.CompletedTask;
        });
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

        // A failure that ends the receive loop is an error, not information: a consumer filtering at
        // Warn or above must still see it, and it is the message carrying the pipe-level detail.
        lock (logLock)
        {
            Assert.Contains(logs, log => log.Message.StartsWith("Unexpected error during receive of data", StringComparison.Ordinal) && log.Level == WebDriverBiDiLogLevel.Error);
        }
    }

    [Fact]
    public async Task TestReceiveDataRaisesErrorEventOnObjectDisposedException()
    {
        ConnectionErrorEventArgs? receivedErrorArgs = null;
        object logLock = new();
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestPipeServer testPipeServer = new();

        TestPipeConnection connection = new(testPipeServer)
        {
            ThrowObjectDisposedExceptionOnReceive = true,
        };

        // As in the IOException test above, the log message precedes the error event, so waiting on
        // the event is enough to order it.
        connection.OnLogMessage.AddObserver(e =>
        {
            lock (logLock)
            {
                logs.Add(e);
            }

            return Task.CompletedTask;
        });
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

        lock (logLock)
        {
            Assert.Contains(logs, log => log.Message.StartsWith("Unexpected error during receive of data", StringComparison.Ordinal) && log.Level == WebDriverBiDiLogLevel.Error);
        }
    }

    [Fact]
    public async Task TestDataReceivedObserverFailureDoesNotEndTheReceiveLoop()
    {
        // A failing observer of OnDataReceived is reported through the connection's observer-error reporter
        // rather than thrown into the receive loop, so the loop goes on delivering later messages, and no
        // connection error is raised.
        int remainingFailures = 1;
        TaskCompletionSource<string> secondMessageReceived = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int connectionErrorCount = 0;
        using TestPipeServer testPipeServer = new();
        testPipeServer.Responses.Add("first");
        testPipeServer.Responses.Add("second");

        await using PipeConnection connection = new(testPipeServer);
        connection.OnDataReceived.AddObserver(e =>
        {
            if (Interlocked.Exchange(ref remainingFailures, 0) == 1)
            {
                throw new InvalidOperationException("observer failure");
            }

            secondMessageReceived.TrySetResult(Encoding.UTF8.GetString(e.Data.Span));
        });
        connection.OnConnectionError.AddObserver(e =>
        {
            Interlocked.Increment(ref connectionErrorCount);
            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("hello"), TestContext.Current.CancellationToken);
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("hello again"), TestContext.Current.CancellationToken);

        Assert.Equal("second", await secondMessageReceived.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
        Assert.True(connection.IsActive);
        Assert.Equal(0, connectionErrorCount);
        testPipeServer.Stop();
        await connection.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestUnexpectedFailureInsideTheReceiveLoopRaisesErrorEvent()
    {
        // A failure inside the receive loop other than a pipe failure, here from a derived connection's read, is
        // captured there. Were it not, the loop would end without notice: the pipe would remain open while no
        // further message was ever delivered, which a caller awaiting a command response cannot distinguish
        // from a remote end that has simply gone quiet.
        ConnectionErrorEventArgs? receivedErrorArgs = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer)
        {
            ReadHandler = (buffer, offset, count, callNumber) => throw new InvalidOperationException("read failure"),
        };
        connection.OnConnectionError.AddObserver(e =>
        {
            receivedErrorArgs = e;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // The receive loop has exited, so the connection must report inactive even though the server process
        // is still running.
        Assert.False(connection.IsActive);
        testPipeServer.Stop();
        await connection.StopAsync(TestContext.Current.CancellationToken);
        await connection.DisposeAsync();

        Assert.NotNull(receivedErrorArgs);
        Assert.Equal("read failure", Assert.IsType<InvalidOperationException>(receivedErrorArgs.Exception).Message);
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

    [Fact]
    public async Task TestAMessageAndItsTerminatorReachThePipeInOneWrite()
    {
        // The null terminator frames the message, so the two must not be separable. Written as two
        // operations, a token canceled after the first completed would leave an unterminated message in
        // the pipe and put every later message one frame boundary out of step, with nothing able to
        // repair it. Asserting the count is what makes that impossible to reintroduce: a behavioral test
        // would have to cancel in the window between the two writes, and after this fix there is no such
        // window to aim at.
        using TestPipeServer testPipeServer = new();
        await using TestPipeConnection connection = new(testPipeServer) { BypassRealPipeWrite = true };

        byte[] message = Encoding.UTF8.GetBytes("{\"id\":1}");
        await connection.WriteFramedMessageAsync(message, TestContext.Current.CancellationToken);

        byte[] frame = Assert.Single(connection.RecordedPipeWrites);
        Assert.Equal(message.Length + 1, frame.Length);
        Assert.Equal("{\"id\":1}", Encoding.UTF8.GetString(frame, 0, message.Length));
        Assert.Equal(0, frame[message.Length]);
    }

    [Fact]
    public async Task TestAnEmptyMessageIsStillTerminated()
    {
        using TestPipeServer testPipeServer = new();
        await using TestPipeConnection connection = new(testPipeServer) { BypassRealPipeWrite = true };

        await connection.WriteFramedMessageAsync(ReadOnlyMemory<byte>.Empty, TestContext.Current.CancellationToken);

        byte[] frame = Assert.Single(connection.RecordedPipeWrites);
        Assert.Equal([0], frame);
    }

    [Fact]
    public async Task TestAnAlreadyCanceledSendWritesNoPartialFrame()
    {
        // Cancellation is honored up to the first byte. What must never happen is a canceled send that
        // has nonetheless put part of a frame into the pipe.
        using TestPipeServer testPipeServer = new();
        await using TestPipeConnection connection = new(testPipeServer) { BypassRealPipeWrite = true };
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await connection.WriteFramedMessageAsync(Encoding.UTF8.GetBytes("hello"), cancellationTokenSource.Token));

        // One write was attempted and it carried the whole frame; the cancellation stopped it before any
        // byte reached the pipe, rather than after a payload without its terminator.
        byte[] attempted = Assert.Single(connection.RecordedPipeWrites);
        Assert.Equal(6, attempted.Length);
        Assert.Equal(0, attempted[5]);
    }

    [Fact]
    public async Task TestTheFrameSentToTheRemoteEndIsNullTerminated()
    {
        // The same invariant observed from the other side of a real pipe, so that the framing test above
        // cannot pass against a connection that frames correctly but writes somewhere else.
        using TestPipeServer testPipeServer = new();
        await using PipeConnection connection = new(testPipeServer);
        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);

        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello"), TestContext.Current.CancellationToken);

        using CancellationTokenSource readCancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        readCancellation.CancelAfter(DataEchoSafetyBound);
        string output = await testPipeServer.ReadSentDataAsync("Hello".Length, readCancellation.Token);
        testPipeServer.Stop();

        Assert.Equal("Hello", output);
        await connection.StopAsync(TestContext.Current.CancellationToken);
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

    [Fact]
    public async Task TestSendWithZeroDataTimeoutFailsImmediatelyWhileAnotherSendIsInProgress()
    {
        // A zero DataTimeout keeps its non-blocking meaning: send access is taken only if it is
        // free at that instant, so a send that finds another in progress fails at once, without
        // arming any timer.
        using TestPipeServer testPipeServer = new();
        TaskCompletionSource sendBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestTimeProvider timeProvider = new();
        TestPipeConnection connection = new(testPipeServer, timeProvider)
        {
            BypassDataSend = false,
            SendBarrier = sendBarrier,
            DataTimeout = TimeSpan.Zero,
        };

        TaskCompletionSource firstSendStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.OnDataSendStarting.AddObserver(e => firstSendStarted.TrySetResult());

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        Task firstSendTask = Task.Run(() => connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello"), TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);
        await firstSendStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // The non-blocking refusal reports the same failure, in the same words, as the one that
        // waited out DataTimeout: what the caller could not get is access to send, and how long the
        // connection was willing to wait for it is not part of that.
        Assert.Equal("Timed out waiting to access connection for sending; only one send operation is permitted at a time.", (await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await connection.SendDataAsync(Encoding.UTF8.GetBytes("World"), TestContext.Current.CancellationToken))).Message);
        Assert.Equal(0, timeProvider.TimerCount);

        sendBarrier.SetResult();
        testPipeServer.Stop();
        try
        {
            await firstSendTask;
        }
        catch (WebDriverBiDiConnectionException)
        {
        }
    }

    [Fact]
    public async Task TestSendWaitsForInProgressSendToFinish()
    {
        // A send that finds another in progress waits for it, and proceeds once the semaphore is
        // released, without the data timeout elapsing.
        using TestPipeServer testPipeServer = new();
        TaskCompletionSource sendBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestPipeConnection connection = new(testPipeServer)
        {
            BypassDataSend = false,
            SendBarrier = sendBarrier,
            DataTimeout = TimeSpan.FromSeconds(30),
        };

        int sendsStarted = 0;
        TaskCompletionSource firstSendStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.OnDataSendStarting.AddObserver(e =>
        {
            Interlocked.Increment(ref sendsStarted);
            firstSendStarted.TrySetResult();
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        Task firstSendTask = Task.Run(() => connection.SendDataAsync(Encoding.UTF8.GetBytes("Hello"), TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);
        await firstSendStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // The second send is parked on the semaphore: it cannot have started while the first
        // still holds it.
        Task secondSendTask = connection.SendDataAsync(Encoding.UTF8.GetBytes("World"), TestContext.Current.CancellationToken);
        Assert.False(secondSendTask.IsCompleted);
        Assert.Equal(1, Volatile.Read(ref sendsStarted));

        sendBarrier.SetResult();
        await firstSendTask.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await secondSendTask.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(2, Volatile.Read(ref sendsStarted));

        await connection.StopAsync(TestContext.Current.CancellationToken);
        testPipeServer.Stop();
    }

    [Theory]
    [InlineData("io")]
    [InlineData("disposed")]
    [InlineData("other")]
    public async Task TestReadThatFailsAfterAStopIsNotReportedAsAConnectionError(string failureKind)
    {
        // A pipe read does not reliably observe cancellation on every target framework, so a stop can
        // leave one outstanding; the pipes are then disposed underneath it and it fails. The session was
        // ended on purpose, so that failure must not reach OnConnectionError, where a consumer would read
        // it as a stop that went wrong. It is logged at Debug instead.
        Exception failure = failureKind switch
        {
            "io" => new IOException("Pipe is broken."),
            "disposed" => new ObjectDisposedException("pipe"),
            _ => new InvalidOperationException("Simulated read failure"),
        };

        object logLock = new();
        List<LogMessageEventArgs> logs = [];
        List<ConnectionErrorEventArgs> errors = [];
        TaskCompletionSource readEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseRead = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestPipeServer testPipeServer = new();
        await using TestPipeConnection connection = new(testPipeServer)
        {
            LogLevel = WebDriverBiDiLogLevel.Debug,
        };

        connection.ReadHandler = async (buffer, offset, count, callNumber) =>
        {
            readEntered.TrySetResult();
            await releaseRead.Task;
            throw failure;
        };
        connection.OnLogMessage.AddObserver(e =>
        {
            lock (logLock)
            {
                logs.Add(e);
            }

            return Task.CompletedTask;
        });
        connection.OnConnectionError.AddObserver(e =>
        {
            lock (logLock)
            {
                errors.Add(e);
            }

            return Task.CompletedTask;
        });

        testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
        await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
        await readEntered.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Release the read only once the stop has canceled the session, so that the failure lands in the
        // catch blocks with cancellation already requested. That ordering is what the fix turns on, and
        // waiting for the token rather than for an elapsed time makes it deterministic.
        TaskCompletionSource sessionCanceled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using CancellationTokenRegistration registration = connection.SessionToken.Register(() => sessionCanceled.TrySetResult());
        Task stopTask = connection.StopAsync(TestContext.Current.CancellationToken);
        await sessionCanceled.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        releaseRead.SetResult();
        await stopTask.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        testPipeServer.Stop();

        lock (logLock)
        {
            Assert.True(errors.Count == 0, $"A read abandoned by the stop must not be reported as a connection error, but {errors.Count} were raised: {string.Join(", ", errors.Select(error => error.Exception.Message))}");
            Assert.Contains(logs, log => log.Level == WebDriverBiDiLogLevel.Debug
                && log.Message.Contains("after the connection was stopped", StringComparison.Ordinal)
                && log.Message.Contains(failure.Message, StringComparison.Ordinal));
            Assert.DoesNotContain(logs, log => log.Level == WebDriverBiDiLogLevel.Error);
        }
    }

    [Fact]
    public async Task TestDisposeWaitsForTheReceiveLoopWhenTheConnectionIsNoLongerOpen()
    {
        // A pipe reports itself closed once the server process exits, while its receive loop is still
        // running. Disposal has no shutdown to perform in that state, but it must still cancel the
        // session and wait for the loop: DisposeAsyncCore disposes the pipes, and a loop still reading
        // from them would fail against disposed handles.
        object logLock = new();
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource readEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseRead = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestPipeServer testPipeServer = new();
        TestPipeConnection connection = new(testPipeServer)
        {
            LogLevel = WebDriverBiDiLogLevel.Debug,
        };

        // The read ends only when the session is canceled, so a disposal that never cancels leaves the
        // loop running and the assertion below sees an incomplete task rather than hanging.
        connection.ReadHandler = async (buffer, offset, count, callNumber) =>
        {
            readEntered.TrySetResult();
            await releaseRead.Task;
            return 0;
        };
        connection.OnLogMessage.AddObserver(e =>
        {
            lock (logLock)
            {
                logs.Add(e);
            }

            return Task.CompletedTask;
        });

        try
        {
            testPipeServer.Start(connection.ReadPipeHandle, connection.WritePipeHandle);
            await connection.StartAsync("pipe://local", TestContext.Current.CancellationToken);
            await readEntered.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            using CancellationTokenRegistration registration = connection.SessionToken.Register(() => releaseRead.TrySetResult());

            // The server process has exited, as far as the connection can tell.
            connection.IsConnectionOpenOverride = () => false;
            await connection.DisposeAsync();

            Assert.True(connection.ReceiveTask is { IsCompleted: true }, "Disposal must cancel and wait for the receive loop when the connection is no longer open.");
            lock (logLock)
            {
                Assert.DoesNotContain(logs, log => log.Message.Contains("Timed out waiting", StringComparison.Ordinal));
            }
        }
        finally
        {
            // Releases the loop if the assertion above found it still running, so the test cannot leave
            // a read parked on the barrier.
            releaseRead.TrySetResult();
            testPipeServer.Stop();
            await connection.DisposeAsync();
        }
    }

}
