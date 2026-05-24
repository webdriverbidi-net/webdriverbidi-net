namespace WebDriverBiDi;

using System.Collections.ObjectModel;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi.TestUtilities;

[Collection("EventSourceTests")]
public class EventObserverTests
{
    [Fact]
    public async Task TestObserverCanHandleObservableEvent()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(e => observedValue = e.EventValue);
        await testEventSource.RaiseTestEventAsync("myValue");
        Assert.NotNull(observedValue);
        Assert.Equal("myValue", observedValue);
    }

    [Fact]
    public async Task TestDisposeAfterDisposeAsyncDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        await observer.DisposeAsync();
        observer.Dispose();
    }

    [Fact]
    public async Task TestDoubleDisposeAsyncDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        await observer.DisposeAsync();
        await observer.DisposeAsync();
    }

    [Fact]
    public async Task TestDisposeAsyncAfterDisposeDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.Dispose();
        await observer.DisposeAsync();
    }

    [Fact]
    public async Task TestIsCapturingReturnsFalseWhenNotCapturing()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        Assert.False(observer.IsCapturing);
    }

    [Fact]
    public async Task TestIsCapturingReturnsTrueWhenCapturing()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.StartCapturingTasks();
        Assert.True(observer.IsCapturing);
        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestStartCapturingWithActiveCaptureThrows()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.StartCapturingTasks();
        Assert.Contains("already has an active capture session", Assert.ThrowsAny<WebDriverBiDiException>(() => observer.StartCapturingTasks()).Message);
        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestStopCapturingWithoutStartCapturingIsNoOp()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.StopCapturingTasks();
        Assert.False(observer.IsCapturing);
    }

    [Fact]
    public async Task TestCallingStopCapturingRepeatedlyDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.StartCapturingTasks();
        observer.StopCapturingTasks();
        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestWaitForCapturedTasksAsyncCountZeroThrows()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.StartCapturingTasks();
        Assert.Contains("must be greater than 0", (await Assert.ThrowsAnyAsync<ArgumentException>(async () => await observer.WaitForCapturedTasksAsync(0, TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken))).Message);
        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestWaitForCapturedTasksAsyncWithoutStartCapturingThrows()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        Assert.Contains("No capture session is active", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestWaitForCapturedTasksAsync()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");
        Task[] tasks = await observer.WaitForCapturedTasksAsync(2, TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken);
        Assert.Equal(2, tasks.Length);
        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestWaitForCapturedTasksAsyncCanTimeout()
    {
        TimeSpan timeout = TimeSpan.FromSeconds(1);
        FakeTimeProvider fakeTimeProvider = new();
        TestEventSource testEventSource = new(fakeTimeProvider);
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");

        // WaitForCapturedTasksAsync is now suspended at WaitToReadAsync with 1 of 2 tasks
        // collected and its CTS timer registered with fakeTimeProvider.
        Task<Task[]> waitTask = observer.WaitForCapturedTasksAsync(2, timeout, TestContext.Current.CancellationToken);
        fakeTimeProvider.Advance(timeout + TimeSpan.FromMilliseconds(1));
        Task[] tasks = await waitTask;

        _ = Assert.Single(tasks);
        Assert.True(observer.IsCapturing);
        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestWaitForCapturedTasksAsyncCanBeCancelled()
    {
        CancellationTokenSource cancellationTokenSource = new();
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        Task waitTask = observer.WaitForCapturedTasksAsync(2, TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        cancellationTokenSource.Cancel();
        Assert.Contains("Wait cancelled waiting for captured event handler tasks", (await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await waitTask)).Message);
        Assert.True(observer.IsCapturing);
        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestGetCapturedTasksWithWithoutStartCaptureReturnsEmptyArray()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        Task[] tasks = observer.GetCapturedTasks();
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task TestGetCapturedTasksWithActiveCaptureAndNoTasksReturnsEmptyArray()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.StartCapturingTasks();
        Task[] tasks = observer.GetCapturedTasks();
        Assert.Empty(tasks);
        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestGetCapturedTasksWithActiveCaptureReturnsPendingTasks()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");
        Task[] tasks = observer.GetCapturedTasks();
        Assert.Equal(2, tasks.Length);
        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestWaitForCapturedTasksCompleteAsync()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");
        bool fulfilled = await observer.WaitForCapturedTasksCompleteAsync(2, TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken);
        Assert.True(fulfilled);
        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestWaitForCapturedTasksCompleteAsyncCanTimeout()
    {
        TimeSpan timeout = TimeSpan.FromSeconds(1);
        FakeTimeProvider fakeTimeProvider = new();
        TestEventSource testEventSource = new(fakeTimeProvider);
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");

        Task<bool> waitTask = observer.WaitForCapturedTasksCompleteAsync(2, timeout, TestContext.Current.CancellationToken);
        fakeTimeProvider.Advance(timeout + TimeSpan.FromMilliseconds(1));
        bool fulfilled = await waitTask;

        Assert.False(fulfilled);
        Assert.True(observer.IsCapturing);
        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestWaitForCapturedTasksCompleteAsyncCanBeCancelledWaitingForTaskCapture()
    {
        CancellationTokenSource cancellationTokenSource = new();
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        Task waitTask = observer.WaitForCapturedTasksCompleteAsync(2, TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        cancellationTokenSource.Cancel();
        Assert.Contains("Wait cancelled waiting for captured event handler tasks", (await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await waitTask)).Message);
        Assert.True(observer.IsCapturing);
        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestWaitForCapturedTasksCompleteAsyncCanBeCancelledWaitingForTaskCompletion()
    {
        TaskCompletionSource bothStartedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int startedCount = 0;
        CancellationTokenSource cancellationTokenSource = new();
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
            async e =>
            {
                if (Interlocked.Increment(ref startedCount) == 2)
                {
                    bothStartedTaskCompletionSource.TrySetResult();
                }
                await Task.Delay(TimeSpan.FromSeconds(2));
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");

        // Ensure both handlers have started before cancelling.
        await bothStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Task waitTask = observer.WaitForCapturedTasksCompleteAsync(2, TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        cancellationTokenSource.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await waitTask);

        // WaitForAsync auto-closes the channel once count tasks are collected, so
        // IsCapturing is false by the time the task-completion wait is cancelled.
        Assert.False(observer.IsCapturing);
    }

    [Fact]
    public async Task TestWaitForCapturedTasksCompleteAsyncReturnsFalseWhenNoRemainingTimeAfterCapture()
    {
        TimeSpan timeout = TimeSpan.FromSeconds(1);
        FakeTimeProvider fakeTimeProvider = new();
        TestEventSource testEventSource = new(fakeTimeProvider);
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });

        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");

        // WaitForCapturedTasksAsync is now suspended waiting for a second task that never arrives.
        // Advancing the fake clock fires its CTS, causing it to time out and return the 1 task it
        // collected. tasksToWait.Length (1) != count (2), so WaitForCapturedTasksCompleteAsync
        // returns false via the final line without entering the remainingTime branch.
        Task<bool> waitTask = observer.WaitForCapturedTasksCompleteAsync(2, timeout, TestContext.Current.CancellationToken);
        fakeTimeProvider.Advance(timeout + TimeSpan.FromMilliseconds(1));
        bool fulfilled = await waitTask;

        Assert.False(fulfilled);
        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestWaitForCapturedTasksCompleteAsyncReturnsFalseWhenExecutionTimesOut()
    {
        TaskCompletionSource bothStartedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int startedCount = 0;
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
            async e =>
            {
                if (Interlocked.Increment(ref startedCount) == 2)
                {
                    bothStartedTaskCompletionSource.TrySetResult();
                }
                await Task.Delay(TimeSpan.FromSeconds(5));
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");

        // Ensure both handlers have started before waiting, so capture completes
        // immediately and the remaining timeout is spent waiting for slow execution.
        await bothStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        bool fulfilled = await observer.WaitForCapturedTasksCompleteAsync(2, TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken);
        Assert.False(fulfilled);

        // Capture session is auto-closed once count tasks are collected.
        Assert.False(observer.IsCapturing);
    }

    [Fact]
    public async Task TestWaitForCapturedTasksCompleteAsyncReturnsFalseWhenTimeExpiredDuringCapture()
    {
        TimeSpan timeout = TimeSpan.FromSeconds(1);

        // AutoAdvanceAmount advances the fake clock on every GetTimestamp() call.
        // The first call captures startTimestamp; the second (inside GetElapsedTime) reads
        // a value that is already timeout + 1 ms ahead, so remainingTime is guaranteed negative
        // without touching WaitForCapturedTasksAsync's own CancellationTokenSource.
        FakeTimeProvider fakeTimeProvider = new()
        {
            AutoAdvanceAmount = timeout + TimeSpan.FromMilliseconds(1)
        };
        TestEventSource testEventSource = new(fakeTimeProvider);
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });

        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");

        bool fulfilled = await observer.WaitForCapturedTasksCompleteAsync(2, timeout, TestContext.Current.CancellationToken);

        Assert.False(fulfilled);
        Assert.False(observer.IsCapturing);
    }

    [Fact]
    public async Task TestWaitForCapturedTasksAsyncPropagatesHandlerException()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
            (e) => Task.FromException(new InvalidOperationException("capture failure")),
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        observer.StartCapturingTasks();
        try
        {
            await testEventSource.RaiseTestEventAsync("myValue");
        }
        catch (InvalidOperationException)
        {
        }

        Assert.Equal("capture failure", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken))).Message);
        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestDisposeWithActiveCaptureDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.StartCapturingTasks();
        Assert.True(observer.IsCapturing);
        observer.Dispose();
        Assert.Equal(0, testEventSource.TestObservableEvent.CurrentObserverCount);
    }

    [Fact]
    public async Task TestDisposeAsyncWithActiveCaptureDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.StartCapturingTasks();
        Assert.True(observer.IsCapturing);
        await observer.DisposeAsync();
        Assert.Equal(0, testEventSource.TestObservableEvent.CurrentObserverCount);
    }

    [Fact]
    public async Task TestCapturingTasksFromSynchronousHandler()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
            (e) => { },
            ObservableEventHandlerOptions.RunHandlerSynchronously);
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue");
        Task[] tasks = observer.GetCapturedTasks();
        _ = Assert.Single(tasks);
        Assert.True(tasks[0].IsCompletedSuccessfully);
        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestCapturingTasksFromAsynchronousHandler()
    {
        bool handlerCompleted = false;
        TaskCompletionSource handlerGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
            async (e) =>
            {
                taskCompletionSource.TrySetResult();
                await handlerGate.Task;
                handlerCompleted = true;
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue");
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Task[] tasks = observer.GetCapturedTasks();
        _ = Assert.Single(tasks);
        Assert.False(tasks[0].IsCompleted);
        handlerGate.SetResult();
        await tasks[0];
        Assert.True(handlerCompleted);
        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestAsynchronousHandlerExceptionCanBeCaptured()
    {
        bool unobservedExceptionRaised = false;
        EventHandler<UnobservedTaskExceptionEventArgs> handler = (sender, e) =>
        {
            unobservedExceptionRaised = true;
            e.SetObserved();
        };
        TaskScheduler.UnobservedTaskException += handler;
        try
        {
            TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            TestEventSource testEventSource = new();
            EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
                async e =>
                {
                    await Task.Yield();
                    try
                    {
                        throw new InvalidOperationException("async capture failure");
                    }
                    finally
                    {
                        taskCompletionSource.TrySetResult();
                    }
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously);

            observer.StartCapturingTasks();
            await testEventSource.RaiseTestEventAsync("myValue");

            Task[] tasks = observer.GetCapturedTasks();
            _ = Assert.Single(tasks);

            // Wait for the handler to fault before triggering GC.
            await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            // Force garbage collection to trigger UnobservedTaskException
            // for any task whose exception was not observed.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.False(unobservedExceptionRaised);
            Assert.True(tasks[0].IsFaulted);
            AggregateException? aggregateException = tasks[0].Exception;
            Assert.NotNull(aggregateException);
            InvalidOperationException innerException = Assert.IsType<InvalidOperationException>(aggregateException.InnerException);
            Assert.Equal("async capture failure", innerException.Message);

            observer.StopCapturingTasks();
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= handler;
        }
    }

    [Fact]
    public async Task TestConcurrentWaitForForCapturedTasksAsyncCallsGetUniqueBatches()
    {
        // Two concurrent callers each asking for 3 tasks should receive unique,
        // non-interleaved batches — one gets tasks 1-3, the other gets tasks 4-6.
        //
        // Determinism guarantee: same technique as
        // TestWaitForCapturedTasksAsyncDoesNotRemovePendingTasksWhenConcurrentWaiterIsActive.
        // Prime with one event so waiter1 is inside its read loop (holding the semaphore,
        // waiting for its 2nd and 3rd tasks). waiter2 must then have already incremented
        // waitingReaderCount before we raise the remaining events. Raise 5 more: waiter1
        // collects 2 (completing its batch of 3), waiter2 collects 3.
        TaskCompletionSource firstTaskConsumedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int taskCount = 0;
        TestEventSource testEventSource = new();

        // Local function typed as Task so the Func<T,Task> overload is chosen rather than
        // Action<T>. An Action<T> wrapper always returns Task.CompletedTask (a singleton), which
        // would collapse all tasks in the HashSet to a count of 1. Task.FromResult(Guid.NewGuid())
        // creates a distinct Task<Guid> object on every invocation.
        Task DistinctTaskHandler(TestObservableEventArgs _)
        {
            if (Interlocked.Increment(ref taskCount) == 1)
            {
                firstTaskConsumedTaskCompletionSource.TrySetResult();
            }

            return Task.FromResult(Guid.NewGuid());
        }

        await using EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(DistinctTaskHandler);
        observer.StartCapturingTasks();

        // Start waiter1, raise one priming event so it is inside its read loop
        // waiting for tasks 2 and 3, then start waiter2.
        Task<Task[]> waiter1 = observer.WaitForCapturedTasksAsync(3, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await testEventSource.RaiseTestEventAsync("prime");
        await firstTaskConsumedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Task<Task[]> waiter2 = observer.WaitForCapturedTasksAsync(3, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Raise 5 more events: waiter1 gets 2 (completing its batch of 3),
        // waiter2 gets 3 (its full batch).
        for (int i = 0; i < 5; i++)
        {
            await testEventSource.RaiseTestEventAsync($"value{i}");
        }

        Task[][] results = await Task.WhenAll(waiter1, waiter2);
        Task[] batch1 = results[0];
        Task[] batch2 = results[1];

        Assert.Equal(3, batch1.Length);
        Assert.Equal(3, batch2.Length);

        // Every task should appear exactly once across both batches.
        HashSet<Task> allTasks = [.. batch1, .. batch2];
        Assert.Equal(6, allTasks.Count);
    }

    [Fact]
    public async Task TestWaitForCapturedTasksAsyncUnblocksOnStopCapturing()
    {
        // WaitForAsync blocked waiting for 3 tasks should return early (with however
        // many arrived so far) when StopCapturing is called from another thread.
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(_ => { });
        observer.StartCapturingTasks();

        // Raise only 1 event, then start a waiter that needs 3.
        await testEventSource.RaiseTestEventAsync("value1");
        Task<Task[]> waitTask = observer.WaitForCapturedTasksAsync(3, TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        // Stop the capture session from another thread — waiter should unblock.
        observer.StopCapturingTasks();

        Task[] tasks = await waitTask;
        _ = Assert.Single(tasks);
    }

    [Fact]
    public async Task TestWaitForAsyncAutoCloseWithStopCapturingAfterCountReached()
    {
        // Covers the ReferenceEquals-false branch inside WaitForAsync's auto-close block:
        // StopCapturing is called from outside immediately after the Nth task is collected,
        // so it may close the channel before WaitForAsync acquires captureLock to do so.
        // The result must be the same regardless of which side wins the race.
        TestEventSource testEventSource = new();
        static Task DistinctTaskHandler(TestObservableEventArgs _) => Task.FromResult(Guid.NewGuid());
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(DistinctTaskHandler);
        observer.StartCapturingTasks();

        for (int i = 0; i < 2; i++)
        {
            await testEventSource.RaiseTestEventAsync($"value{i}");
        }

        // WaitForAsync(2) will collect both tasks and then try to auto-close.
        // StopCapturing() races with that auto-close from the calling thread.
        Task<Task[]> waitTask = observer.WaitForCapturedTasksAsync(2, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        observer.StopCapturingTasks();

        Task[] tasks = await waitTask;

        Assert.Equal(2, tasks.Length);
        Assert.False(observer.IsCapturing);
    }

    [Fact]
    public async Task TestFaultedTaskCapturedAfterWaitForCapturedTasksAsyncDoesNotCauseUnobservedTaskException()
    {
        // A task that races into the channel buffer after WaitForAsync has collected its
        // Nth task (but before TryComplete closes the writer) would normally produce an
        // UnobservedTaskException when it faults, because CaptureTask marked it as
        // ShouldReportAsyncFault = false. The drain logic must attach a new continuation
        // that observes the fault so UnobservedTaskException is never raised.
        bool unobservedExceptionRaised = false;
        EventHandler<UnobservedTaskExceptionEventArgs> unobservedHandler = (sender, e) =>
        {
            unobservedExceptionRaised = true;
            e.SetObserved();
        };
        TaskScheduler.UnobservedTaskException += unobservedHandler;

        try
        {
            TestEventSource testEventSource = new();

            TaskCompletionSource handlerStartedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource allowFaultTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource handlerFaultedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

            EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
                async e =>
                {
                    if (e.EventValue == "raced")
                    {
                        handlerStartedTaskCompletionSource.TrySetResult();
                        try
                        {
                            await allowFaultTaskCompletionSource.Task;
                            throw new InvalidOperationException("raced task fault");
                        }
                        finally
                        {
                            handlerFaultedTaskCompletionSource.TrySetResult();
                        }
                    }
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously);

            observer.StartCapturingTasks();

            // Raise the event that WaitForAsync(1) will collect.
            await testEventSource.RaiseTestEventAsync("collected");

            // Raise the raced event — its task will be in-flight when the channel closes.
            await testEventSource.RaiseTestEventAsync("raced");
            await handlerStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            // WaitForAsync collects 1 task, auto-closes the channel, and drains the raced task.
            Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            _ = Assert.Single(tasks);

            // Let the raced handler fault and wait for it to complete before triggering GC.
            allowFaultTaskCompletionSource.TrySetResult();
            await handlerFaultedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            // Force garbage collection to trigger UnobservedTaskException
            // for any task whose exception was not observed.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.False(unobservedExceptionRaised);
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= unobservedHandler;
        }
    }

    [Fact]
    public async Task TestWaitForCapturedTasksAsyncAlreadyFaultedRacedTaskDoesNotCauseUnobservedTaskException()
    {
        // If the raced task is already faulted synchronously by the time the drain runs,
        // the direct observation path in ReportOrAttachFaultContinuation is taken.
        // Verify that path also prevents UnobservedTaskException.
        bool unobservedExceptionRaised = false;
        void UnobservedHandler(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            unobservedExceptionRaised = true;
            e.SetObserved();
        }

        TaskScheduler.UnobservedTaskException += UnobservedHandler;

        try
        {
            TestEventSource testEventSource = new();

            static Task RacedFaultHandler(TestObservableEventArgs e) => e.EventValue == "raced"
                ? Task.FromException(new InvalidOperationException("already faulted raced task"))
                : Task.CompletedTask;

            EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
                RacedFaultHandler,
                ObservableEventHandlerOptions.RunHandlerAsynchronously);

            observer.StartCapturingTasks();

            // Raise both events so both tasks land in the buffer before WaitForAsync reads.
            await testEventSource.RaiseTestEventAsync("collected");
            try
            {
                // The raced handler faults synchronously; RaiseTestEventAsync re-throws it.
                await testEventSource.RaiseTestEventAsync("raced");
            }
            catch (InvalidOperationException)
            {
            }

            // WaitForAsync collects 1 task, auto-closes, and drains the already-faulted raced task.
            Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            _ = Assert.Single(tasks);

            // Force garbage collection to trigger UnobservedTaskException
            // for any task whose exception was not observed.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.False(unobservedExceptionRaised);
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= UnobservedHandler;
        }
    }

    [Fact]
    public async Task TestWaitForCapturedTasksAsyncDoesNotRemovePendingTasksWhenConcurrentWaiterIsActive()
    {
        // When two WaitForAsync callers are both active (waitingReaderCount == 2),
        // the first to finish must NOT drain the channel — those tasks belong to the
        // second waiter.
        //
        // Determinism guarantee: both waiters need to have incremented waitingReaderCount
        // before the final events are raised. We achieve this by priming with one event
        // before starting waiter2. By the time waiter1 has consumed the first primed task
        // (observable via firstTaskConsumedTaskCompletionSource), it holds the semaphore
        // and is blocking inside WaitToReadAsync waiting for its second task. waiter2 must
        // therefore have already completed its synchronous preamble (incrementing
        // waitingReaderCount) and be blocked on captureReadSemaphore.WaitAsync. Both are
        // registered as active readers before the remaining events are raised.
        TaskCompletionSource firstTaskConsumedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int taskCount = 0;
        TestEventSource testEventSource = new();
        Task DistinctTaskHandler(TestObservableEventArgs _)
        {
            if (Interlocked.Increment(ref taskCount) == 1)
            {
                firstTaskConsumedTaskCompletionSource.TrySetResult();
            }

            return Task.FromResult(Guid.NewGuid());
        }

        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(DistinctTaskHandler);
        observer.StartCapturingTasks();

        // Start waiter1, raise one priming event so it is inside its read loop
        // waiting for a second task, then start waiter2.
        Task<Task[]> waiter1 = observer.WaitForCapturedTasksAsync(2, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await testEventSource.RaiseTestEventAsync("prime");
        await firstTaskConsumedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Task<Task[]> waiter2 = observer.WaitForCapturedTasksAsync(2, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Raise 3 more events: waiter1 gets 1 (to complete its batch of 2),
        // waiter2 gets 2 (its full batch).
        for (int i = 0; i < 3; i++)
        {
            await testEventSource.RaiseTestEventAsync($"value{i}");
        }

        Task[][] results = await Task.WhenAll(waiter1, waiter2);

        Assert.Equal(2, results[0].Length);
        Assert.Equal(2, results[1].Length);
        HashSet<Task> all = [.. results[0], .. results[1]];
        Assert.Equal(4, all.Count);

        observer.StopCapturingTasks();
    }

    [Fact]
    public async Task TestWaitForCapturedTasksAsyncAutoClosesChannelAfterCountReached()
    {
        // After WaitForAsync(N) collects N tasks, subsequent handler invocations should not
        // accumulate in the channel — the capture session is auto-closed.
        TestEventSource testEventSource = new();
        static Task DistinctTaskHandler(TestObservableEventArgs _) => Task.FromResult(Guid.NewGuid());
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(DistinctTaskHandler);
        observer.StartCapturingTasks();

        // Raise 3 events, wait for 2 — the Nth read auto-closes the channel.
        for (int i = 0; i < 3; i++)
        {
            await testEventSource.RaiseTestEventAsync($"value{i}");
        }

        Task[] firstBatch = await observer.WaitForCapturedTasksAsync(2, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Raise further events — these must not be captured because the channel is closed.
        for (int i = 3; i < 6; i++)
        {
            await testEventSource.RaiseTestEventAsync($"value{i}");
        }

        // GetCapturedTasks should return nothing (no active capture) and not the extra events.
        Task[] remaining = observer.GetCapturedTasks();

        Assert.Equal(2, firstBatch.Length);
        Assert.Empty(remaining);
        Assert.False(observer.IsCapturing);
    }

    [Fact]
    public async Task TestDisposeRemovesObserver()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => observedValue = e.EventValue);
        await testEventSource.RaiseTestEventAsync("myValue1");
        Assert.Equal("myValue1", observedValue);

        observer.Dispose();
        Assert.Equal(0, testEventSource.TestObservableEvent.CurrentObserverCount);

        await testEventSource.RaiseTestEventAsync("myValue2");
        Assert.Equal("myValue1", observedValue);
    }

    [Fact]
    public async Task TestUsingPatternRemovesObserver()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();

        using (EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => observedValue = e.EventValue))
        {
            await testEventSource.RaiseTestEventAsync("myValue1");

            Assert.Equal("myValue1", observedValue);
            Assert.Equal(1, testEventSource.TestObservableEvent.CurrentObserverCount);
        }

        Assert.Equal(0, testEventSource.TestObservableEvent.CurrentObserverCount);

        await testEventSource.RaiseTestEventAsync("myValue2");
        Assert.Equal("myValue1", observedValue);
    }

    [Fact]
    public async Task TestDoubleDisposeDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        observer.Dispose();
        observer.Dispose();
    }

    [Fact]
    public async Task TestDisposeAsyncRemovesObserver()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => observedValue = e.EventValue);
        await testEventSource.RaiseTestEventAsync("myValue1");
        Assert.Equal("myValue1", observedValue);

        await observer.DisposeAsync();
        Assert.Equal(0, testEventSource.TestObservableEvent.CurrentObserverCount);

        await testEventSource.RaiseTestEventAsync("myValue2");
        Assert.Equal("myValue1", observedValue);
    }

    [Fact]
    public async Task TestAwaitUsingPatternRemovesObserver()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();

        await using (EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => observedValue = e.EventValue))
        {
            await testEventSource.RaiseTestEventAsync("myValue1");

            Assert.Equal("myValue1", observedValue);
            Assert.Equal(1, testEventSource.TestObservableEvent.CurrentObserverCount);
        }

        Assert.Equal(0, testEventSource.TestObservableEvent.CurrentObserverCount);

        await testEventSource.RaiseTestEventAsync("myValue2");
        Assert.Equal("myValue1", observedValue);
    }

    [Fact]
    public async Task TestHandlerRunAsynchronouslyWithSynchronousExceptionPropagates()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(
            _ => Task.FromException(new InvalidOperationException("sync fire-and-forget failure")),
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        Assert.Equal("sync fire-and-forget failure", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await testEventSource.RaiseTestEventAsync("myValue"))).Message);
    }

    [Fact]
    public async Task TestHandlerRunAsynchronouslyWithSynchronousExceptionDoesNotPreventSubsequentObservers()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(
            _ => Task.FromException(new InvalidOperationException("sync fire-and-forget failure")),
            ObservableEventHandlerOptions.RunHandlerAsynchronously);
        testEventSource.TestObservableEvent.AddObserver(e => observedValue = e.EventValue);

        Assert.Equal("sync fire-and-forget failure", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await testEventSource.RaiseTestEventAsync("myValue"))).Message);
        Assert.Equal("myValue", observedValue);
    }

    [Fact]
    public async Task TestHandlerRunAsynchronouslyWithAsyncExceptionDoesNotCauseUnobservedTaskException()
    {
        bool unobservedExceptionRaised = false;
        void UnobservedHandler(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            unobservedExceptionRaised = true;
            e.SetObserved();
        }

        TaskScheduler.UnobservedTaskException += UnobservedHandler;
        try
        {
            TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            TestEventSource testEventSource = new();
            testEventSource.TestObservableEvent.AddObserver(
                async e =>
                {
                    await Task.Yield();
                    taskCompletionSource.TrySetResult();
                    throw new InvalidOperationException("async fire-and-forget failure");
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously);

            await testEventSource.RaiseTestEventAsync("myValue");

            // Wait for the handler to fault before triggering GC.
            await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            // Force garbage collection to trigger UnobservedTaskException
            // for any task whose exception was not observed.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.False(unobservedExceptionRaised);
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= UnobservedHandler;
        }
    }

    [Fact]
    public async Task TestHandlerRunAsynchronouslyWithAsyncSuccessCompletesWithoutError()
    {
        bool handlerCompleted = false;
        TaskCompletionSource handlerReachedAsyncTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource handlerFinishedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(
            async _ =>
            {
                handlerReachedAsyncTaskCompletionSource.TrySetResult();
                await Task.Yield();
                handlerCompleted = true;
                handlerFinishedTaskCompletionSource.TrySetResult();
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await testEventSource.RaiseTestEventAsync("myValue");

        // handlerReachedAsyncTaskCompletionSource is set before the first real await, so the
        // handler has started but has not yet set handlerCompleted.
        await handlerReachedAsyncTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.False(handlerCompleted);

        await handlerFinishedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.True(handlerCompleted);
    }

    [Fact]
    public async Task TestAsyncHandlerRaisesIncrementThenDecrementEventSourceEvent()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestEventListener listener = new();
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(
            async _ => await taskCompletionSource.Task,
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await testEventSource.RaiseTestEventAsync("myValue");

        // Wait for the increment event to land.
        for (int i = 0; i < 50 && listener.GetEventsForEventName("AsyncHandlerTaskCount").Count == 0; i++)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(20), TestContext.Current.CancellationToken);
        }

        List<System.Diagnostics.Tracing.EventWrittenEventArgs> incrementEvents = listener.GetEventsForEventName("AsyncHandlerTaskCount");
        Assert.NotEmpty(incrementEvents);
        int? incrementPayload = incrementEvents[0].Payload is [int c] ? c : null;
        Assert.NotNull(incrementPayload);

        taskCompletionSource.TrySetResult();

        // Wait for the decrement event to land.
        for (int i = 0; i < 50 && listener.GetEventsForEventName("AsyncHandlerTaskCount").Count < 2; i++)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(20), TestContext.Current.CancellationToken);
        }

        List<System.Diagnostics.Tracing.EventWrittenEventArgs> allEvents = listener.GetEventsForEventName("AsyncHandlerTaskCount");
        Assert.True(allEvents.Count >= 2);

        // The decrement value must be strictly less than the increment value, regardless
        // of any concurrent activity from other tests, because each increment is paired
        // with exactly one decrement.
        ReadOnlyCollection<object?>? firstPayload = allEvents[0].Payload;
        Assert.NotNull(firstPayload);
        int firstValue = Assert.IsType<int>(firstPayload[0]);
        ReadOnlyCollection<object?>? lastPayload = allEvents[^1].Payload;
        Assert.NotNull(lastPayload);
        int lastValue = Assert.IsType<int>(lastPayload[0]);
        Assert.True(lastValue < firstValue);
    }

    [Fact]
    public async Task TestAsyncHandlerFaultStillRaisesDecrementEventSourceEvent()
    {
        using TestEventListener listener = new();
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(
            async _ =>
            {
                await Task.Yield();
                throw new InvalidOperationException("async fault");
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await testEventSource.RaiseTestEventAsync("myValue");

        for (int i = 0; i < 50 && listener.GetEventsForEventName("AsyncHandlerTaskCount").Count < 2; i++)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(20), TestContext.Current.CancellationToken);
        }

        List<System.Diagnostics.Tracing.EventWrittenEventArgs> allEvents = listener.GetEventsForEventName("AsyncHandlerTaskCount");
        Assert.True(allEvents.Count >= 2);
    }

    [Fact]
    public async Task TestSynchronouslyCompletedAsyncHandlerDoesNotRaiseCounterEvents()
    {
        using TestEventListener listener = new();
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(
            _ => Task.CompletedTask,
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await testEventSource.RaiseTestEventAsync("myValue");

        // Give any would-be continuation a chance to fire.
        await Task.Delay(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);

        Assert.Empty(listener.GetEventsForEventName("AsyncHandlerTaskCount"));
    }

    [Fact]
    public async Task TestSynchronousHandlerDoesNotRaiseCounterEvents()
    {
        using TestEventListener listener = new();
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(
            _ => { },
            ObservableEventHandlerOptions.RunHandlerSynchronously);

        await testEventSource.RaiseTestEventAsync("myValue");

        await Task.Delay(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);

        Assert.Empty(listener.GetEventsForEventName("AsyncHandlerTaskCount"));
    }

    [Fact]
    public async Task TestConcurrentAddRemoveAgainstNotifyIsCoherent()
    {
        // Fan out a fixed number of add/remove and raise iterations against a
        // single ObservableEvent. The test asserts that (a) no exception leaks
        // from the observer-list mutation path — Task.WhenAll would rethrow —
        // and (b) after all churn has stopped, the observer count and final
        // raise invocations are exactly what we expect. Using fixed iteration
        // counts rather than a time window keeps the test strictly deterministic.
        const int registrationWorkers = 8;
        const int addRemoveIterationsPerWorker = 500;
        const int notificationWorkers = 4;
        const int raisesPerWorker = 200;

        TestEventSource testEventSource = new();
        ObservableEvent<TestObservableEventArgs> observable = testEventSource.TestObservableEvent;

        int steadyInvocations = 0;
        EventObserver<TestObservableEventArgs> steady1 = observable.AddObserver(_ => Interlocked.Increment(ref steadyInvocations));
        EventObserver<TestObservableEventArgs> steady2 = observable.AddObserver(_ => Interlocked.Increment(ref steadyInvocations));

        List<Task> workers = [];
        for (int i = 0; i < registrationWorkers; i++)
        {
            workers.Add(Task.Run(
                () =>
                {
                    for (int j = 0; j < addRemoveIterationsPerWorker; j++)
                    {
                        EventObserver<TestObservableEventArgs> transient = observable.AddObserver(_ => { });
                        transient.Unobserve();
                    }
                },
                TestContext.Current.CancellationToken));
        }

        for (int i = 0; i < notificationWorkers; i++)
        {
            workers.Add(Task.Run(
                async () =>
                {
                    for (int j = 0; j < raisesPerWorker; j++)
                    {
                        await testEventSource.RaiseTestEventAsync("stress");
                    }
                },
                TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(workers);

        // After all churn has ceased, only the two steady-state observers should remain.
        Assert.Equal(2, observable.CurrentObserverCount);

        // A final raise after the churn must invoke exactly the two steady observers.
        int invocationsBeforeFinalRaise = steadyInvocations;
        await testEventSource.RaiseTestEventAsync("post-stress");
        Assert.Equal(2, steadyInvocations - invocationsBeforeFinalRaise);

        steady1.Unobserve();
        steady2.Unobserve();
    }

    [Fact]
    public async Task TestAsyncFaultAfterHandlerReturnInvokesErrorReporter()
    {
        EventObserverErrorInfo? reportedErrorInfo = null;
        TaskCompletionSource reporterInvokedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestEventSource testEventSource = new();
        testEventSource.SetObserverErrorReporter(errorInfo =>
        {
            reportedErrorInfo = errorInfo with { };
            reporterInvokedTaskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        TaskCompletionSource handlerFaultedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
            async _ =>
            {
                await Task.Yield();
                handlerFaultedTaskCompletionSource.TrySetResult();
                throw new InvalidOperationException("async reporter test failure");
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously,
            "test observer description");

        await testEventSource.RaiseTestEventAsync("value");

        await handlerFaultedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await reporterInvokedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.NotNull(reportedErrorInfo);
        Assert.Equal("testModule.testEvent", reportedErrorInfo.ObservableEventName);
        Assert.Equal(observer.Id, reportedErrorInfo.ObserverId);
        Assert.Equal("test observer description", reportedErrorInfo.ObserverDescription);
        InvalidOperationException exception = Assert.IsType<InvalidOperationException>(reportedErrorInfo.Exception);
        Assert.Equal("async reporter test failure", exception.Message);
        Assert.True(reportedErrorInfo.IsAsynchronousHandler);
        Assert.True(reportedErrorInfo.FaultOccurredAfterHandlerReturned);
    }

    [Fact]
    public async Task TestSynchronousFaultOnAsyncHandlerDoesNotInvokeErrorReporter()
    {
        bool reporterInvoked = false;

        TestEventSource testEventSource = new();
        testEventSource.SetObserverErrorReporter(_ =>
        {
            reporterInvoked = true;
            return Task.CompletedTask;
        });

        testEventSource.TestObservableEvent.AddObserver(
            _ => Task.FromException(new InvalidOperationException("sync fault on async handler")),
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        // Synchronous fault propagates through NotifyObserversAsync directly
        // rather than through the error reporter.
        try
        {
            await testEventSource.RaiseTestEventAsync("value");
        }
        catch (InvalidOperationException)
        {
        }

        // Give any would-be reporter invocation a chance to arrive.
        await Task.Delay(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);

        Assert.False(reporterInvoked);
    }

    [Fact]
    public async Task TestToStringReturnsDescription()
    {
        TestEventSource testEventSource = new();
        await using EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { }, ObservableEventHandlerOptions.RunHandlerSynchronously, "My first handler");
        Assert.Equal("My first handler", observer.ToString());
    }

    [Fact]
    public async Task TestToStringReturnsDefaultDescription()
    {
        TestEventSource testEventSource = new();
        await using EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        Assert.StartsWith("EventObserver<TestObservableEventArgs> (id:", observer.ToString());
    }

    [Fact]
    public async Task TestCompareToNullReturnsPositive()
    {
        TestEventSource testEventSource = new();
        await using EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        Assert.True(observer.CompareTo(null) > 0);
    }
}
