namespace WebDriverBiDi;

using TestUtilities;

public class ObservableEventTests
{
    [Fact]
    public async Task TestCanAddHandler()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(e => { });
        Assert.Equal(1, testEventSource.TestObservableEvent.CurrentObserverCount);
    }

    [Fact]
    public async Task TestCanAddEventDataCollector()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddDataCollector();
        Assert.Equal(1, testEventSource.TestObservableEvent.CurrentObserverCount);
    }

    [Fact]
    public async Task TestCanRemoveObservableEventHandler()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => observedValue = e.EventValue);
        await testEventSource.RaiseTestEventAsync("myValue1");
        Assert.NotNull(observedValue);
        Assert.Equal("myValue1", observedValue);

        observer.Unobserve();
        await testEventSource.RaiseTestEventAsync("myValue2");
        Assert.Equal("myValue1", observedValue);
    }

    [Fact]
    public async Task TestDisposingEventDataCollectorRemovesObserver()
    {
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddDataCollector();
        Assert.Equal(1, testEventSource.TestObservableEvent.CurrentObserverCount);
        observer.Dispose();
        Assert.Equal(0, testEventSource.TestObservableEvent.CurrentObserverCount);
    }

    [Fact]
    public async Task TestCannotAddMoreThanMaxObserversUsingStandardObservers()
    {
        TestEventSource testEventSource = new(1);
        Assert.Equal(1u, testEventSource.TestObservableEvent.MaxObserverCount);
        Assert.Equal(0, testEventSource.TestObservableEvent.CurrentObserverCount);
        testEventSource.TestObservableEvent.AddObserver(e => { });
        Assert.Equal(1, testEventSource.TestObservableEvent.CurrentObserverCount);
        WebDriverBiDiException exception = Assert.ThrowsAny<WebDriverBiDiException>(() => testEventSource.TestObservableEvent.AddObserver(e => { }));
        Assert.Equal("This observable event only allows 1 observer.", exception.Message);

        testEventSource = new(2);
        Assert.Equal(2u, testEventSource.TestObservableEvent.MaxObserverCount);
        Assert.Equal(0, testEventSource.TestObservableEvent.CurrentObserverCount);
        testEventSource.TestObservableEvent.AddObserver(e => { });
        Assert.Equal(1, testEventSource.TestObservableEvent.CurrentObserverCount);
        testEventSource.TestObservableEvent.AddObserver(e => { });
        Assert.Equal(2, testEventSource.TestObservableEvent.CurrentObserverCount);
        exception = Assert.ThrowsAny<WebDriverBiDiException>(() => testEventSource.TestObservableEvent.AddObserver(e => { }));
        Assert.Equal("This observable event only allows 2 observers.", exception.Message);
    }

    [Fact]
    public async Task TestCannotAddMoreThanMaxObserversUsingEventDataCollectors()
    {
        TestEventSource testEventSource = new(1);
        Assert.Equal(1u, testEventSource.TestObservableEvent.MaxObserverCount);
        Assert.Equal(0, testEventSource.TestObservableEvent.CurrentObserverCount);
        testEventSource.TestObservableEvent.AddDataCollector();
        Assert.Equal(1, testEventSource.TestObservableEvent.CurrentObserverCount);
        Assert.Equal("This observable event only allows 1 observer.", Assert.ThrowsAny<WebDriverBiDiException>(() => testEventSource.TestObservableEvent.AddDataCollector()).Message);

        testEventSource = new(2);
        Assert.Equal(2u, testEventSource.TestObservableEvent.MaxObserverCount);
        Assert.Equal(0, testEventSource.TestObservableEvent.CurrentObserverCount);
        testEventSource.TestObservableEvent.AddDataCollector();
        Assert.Equal(1, testEventSource.TestObservableEvent.CurrentObserverCount);
        testEventSource.TestObservableEvent.AddDataCollector();
        Assert.Equal(2, testEventSource.TestObservableEvent.CurrentObserverCount);
        Assert.Equal("This observable event only allows 2 observers.", Assert.ThrowsAny<WebDriverBiDiException>(() => testEventSource.TestObservableEvent.AddDataCollector()).Message);
    }

    [Fact]
    public async Task TestCannotAddMoreThanMaxObserversUsingMixedObservers()
    {
        TestEventSource testEventSource = new(2);
        Assert.Equal(2u, testEventSource.TestObservableEvent.MaxObserverCount);
        Assert.Equal(0, testEventSource.TestObservableEvent.CurrentObserverCount);
        testEventSource.TestObservableEvent.AddObserver(e => { });
        Assert.Equal(1, testEventSource.TestObservableEvent.CurrentObserverCount);
        testEventSource.TestObservableEvent.AddDataCollector();
        Assert.Equal(2, testEventSource.TestObservableEvent.CurrentObserverCount);
        WebDriverBiDiException exception = Assert.ThrowsAny<WebDriverBiDiException>(() => testEventSource.TestObservableEvent.AddObserver(e => { }));
        Assert.Equal("This observable event only allows 2 observers.", exception.Message);
        Assert.Equal("This observable event only allows 2 observers.", Assert.ThrowsAny<WebDriverBiDiException>(() => testEventSource.TestObservableEvent.AddDataCollector()).Message);
    }

    [Fact]
    public async Task TestToStringReturnsDescriptionForEventObserver()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(e => { }, ObservableEventHandlerOptions.RunHandlerSynchronously, "My first handler");
        string eventSourceString = testEventSource.TestObservableEvent.ToString();
        Assert.Equal("ObservableEvent<TestObservableEventArgs> with observers:\n    My first handler", eventSourceString);
    }

    [Fact]
    public async Task TestToStringReturnsDefaultDescriptionForEventObserver()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(e => { });
        string eventSourceString = testEventSource.TestObservableEvent.ToString();
        Assert.StartsWith("ObservableEvent<TestObservableEventArgs> with observers:\n    EventObserver<TestObservableEventArgs> (id:", eventSourceString);
    }

    [Fact]
    public async Task TestToStringReturnsDescriptionForEventDataCollector()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddDataCollector(description: "My first collector");
        string eventSourceString = testEventSource.TestObservableEvent.ToString();
        Assert.Equal("ObservableEvent<TestObservableEventArgs> with observers:\n    My first collector", eventSourceString);
    }

    [Fact]
    public async Task TestToStringReturnsDefaultDescriptionForEventDataCollector()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddDataCollector();
        string eventSourceString = testEventSource.TestObservableEvent.ToString();
        Assert.StartsWith("ObservableEvent<TestObservableEventArgs> with observers:\n    EventDataCollector<TestObservableEventArgs> (id:", eventSourceString);
    }

    [Fact]
    public async Task TestEventName()
    {
        TestEventSource testEventSource = new();
        Assert.Equal("testModule.testEvent", testEventSource.TestObservableEvent.EventName);
    }

    [Fact]
    public async Task TestThrowingObserverDoesNotPreventSubsequentObserversFromBeingNotified()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(e => throw new InvalidOperationException("observer failure"));
        testEventSource.TestObservableEvent.AddObserver(e => observedValue = e.EventValue);

        Assert.Equal("observer failure", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await testEventSource.RaiseTestEventAsync("myValue"))).Message);
        Assert.Equal("myValue", observedValue);
    }

    [Fact]
    public async Task TestMultipleThrowingObserversProduceAggregateException()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(e => throw new InvalidOperationException("first failure"));
        testEventSource.TestObservableEvent.AddObserver(e => throw new ArgumentException("second failure"));

        AggregateException? caught = null;
        try
        {
            await testEventSource.RaiseTestEventAsync("myValue");
        }
        catch (AggregateException ex)
        {
            caught = ex;
        }

        Assert.NotNull(caught);

        Assert.Equal(2, caught.InnerExceptions.Count);
        Assert.IsType<InvalidOperationException>(caught.InnerExceptions[0]);
        Assert.Equal("first failure", caught.InnerExceptions[0].Message);
        Assert.IsType<ArgumentException>(caught.InnerExceptions[1]);
        Assert.Equal("second failure", caught.InnerExceptions[1].Message);
    }

    [Fact]
    public async Task TestThrowingAsyncObserverDoesNotPreventSubsequentObserversFromBeingNotified()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(e =>
        {
            return Task.FromException(new InvalidOperationException("async observer failure"));
        });
        testEventSource.TestObservableEvent.AddObserver(e => observedValue = e.EventValue);

        Assert.Equal("async observer failure", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await testEventSource.RaiseTestEventAsync("myValue"))).Message);
        Assert.Equal("myValue", observedValue);
    }

    [Fact]
    public async Task TestNoExceptionThrownWhenAllObserversSucceed()
    {
        string? firstValue = null;
        string? secondValue = null;
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(e => firstValue = e.EventValue);
        testEventSource.TestObservableEvent.AddObserver(e => secondValue = e.EventValue);

        await testEventSource.RaiseTestEventAsync("myValue");

        Assert.Equal("myValue", firstValue);
        Assert.Equal("myValue", secondValue);
    }

    [Fact]
    public async Task TestRemovingStandardObserverDoesNotDecrementDataCollectorCount()
    {
        // Verify that removing a standard (non-data-collector) observer does not
        // corrupt the dataCollectorCount, which would cause data collectors to lose
        // their guaranteed execution order ahead of standard handlers.
        int capturedEventDataCount = 0;
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
            e => capturedEventDataCount = collector.GetCollectedEventData().Count);
        Assert.Equal(2, testEventSource.TestObservableEvent.CurrentObserverCount);

        testEventSource.TestObservableEvent.RemoveObserver(observer.Id);
        Assert.Equal(1, testEventSource.TestObservableEvent.CurrentObserverCount);

        // Re-add the standard handler and raise an event. If dataCollectorCount were
        // wrongly decremented to 0, the sort would be skipped and the collector might
        // run after the handler, leaving capturedEventDataCount at 0.
        testEventSource.TestObservableEvent.AddObserver(
            e => capturedEventDataCount = collector.GetCollectedEventData().Count);
        await testEventSource.RaiseTestEventAsync("myValue");
        Assert.Equal(1, capturedEventDataCount);
    }

    [Fact]
    public async Task TestRemovingAlreadyRemovedObserverIsNoOp()
    {
        // Exercises the TryGetValue-returns-false branch in RemoveObserver: calling
        // RemoveObserver with an ID that is no longer in the dictionary must not
        // throw or corrupt observer count.
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => { });
        Assert.Equal(1, testEventSource.TestObservableEvent.CurrentObserverCount);

        testEventSource.TestObservableEvent.RemoveObserver(observer.Id);
        Assert.Equal(0, testEventSource.TestObservableEvent.CurrentObserverCount);

        // Second call with the same (now-absent) ID — TryGetValue returns false.
        testEventSource.TestObservableEvent.RemoveObserver(observer.Id);
        Assert.Equal(0, testEventSource.TestObservableEvent.CurrentObserverCount);
    }

    [Fact]
    public async Task TestDataCollectorsExecuteBeforeHandlers()
    {
        int capturedEventDataCount = 0;
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        testEventSource.TestObservableEvent.AddObserver(e => capturedEventDataCount = collector.GetCollectedEventData().Count);
        await testEventSource.RaiseTestEventAsync("myValue1");
        Assert.Equal(1, capturedEventDataCount);
    }

    [Fact]
    public async Task TestHandlersExecuteInAdditionOrder()
    {
        List<string> eventValues = [];
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(e => eventValues.Add("first"));
        testEventSource.TestObservableEvent.AddObserver(e => eventValues.Add("second"));

        await testEventSource.RaiseTestEventAsync("myValue");

        Assert.Equal(["first", "second"], eventValues);

        eventValues.Clear();
        observer.Unobserve();
        testEventSource.TestObservableEvent.AddObserver(e => eventValues.Add("first"));

        await testEventSource.RaiseTestEventAsync("myValue");

        Assert.Equal(["second", "first"], eventValues);
    }

    [Fact]
    public async Task TestActionHandlerRunAsynchronouslyDoesNotBlockNotification()
    {
        // An Action<T> handler has no Task of its own to become asynchronous, so the
        // RunHandlerAsynchronously option must queue the whole handler to the thread pool.
        // Otherwise the option would be a no-op and the handler would block the caller.
        using ManualResetEventSlim releaseHandler = new(false);
        TaskCompletionSource handlerStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
            e =>
            {
                handlerStarted.TrySetResult();
                releaseHandler.Wait();
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        observer.StartCapturingTasks();
        Task notifyTask = testEventSource.RaiseTestEventAsync("myValue");

        // Notification completes while the handler is still parked on the reset event; the
        // captured handler task is therefore still running when the wait returns.
        await notifyTask.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await handlerStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Task[] capturedTasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Task capturedTask = Assert.Single(capturedTasks);
        Assert.False(capturedTask.IsCompleted);

        releaseHandler.Set();
        await capturedTask.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.True(capturedTask.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task TestActionHandlerRunAsynchronouslyFaultIsReportedAsAsynchronousFault()
    {
        // Because the handler now runs on the thread pool, an exception it throws is a fault
        // of the queued task: notification itself does not throw, and the fault is surfaced
        // through the observer error reporter as an asynchronous, post-return fault.
        EventObserverErrorInfo? reportedErrorInfo = null;
        TaskCompletionSource reporterInvoked = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestEventSource testEventSource = new();
        testEventSource.SetObserverErrorReporter(errorInfo =>
        {
            reportedErrorInfo = errorInfo;
            reporterInvoked.TrySetResult();
            return Task.CompletedTask;
        });

        // Declared explicitly as Action<T>: a bare `e => throw ...` lambda would bind to the
        // Func<T, Task> overload, which is not the overload under test here.
        Action<TestObservableEventArgs> throwingHandler = e => throw new InvalidOperationException("action handler failure");
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
            throwingHandler,
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await testEventSource.RaiseTestEventAsync("myValue");
        await reporterInvoked.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.NotNull(reportedErrorInfo);
        Assert.Equal(observer.Id, reportedErrorInfo.ObserverId);
        InvalidOperationException exception = Assert.IsType<InvalidOperationException>(reportedErrorInfo.Exception);
        Assert.Equal("action handler failure", exception.Message);
        Assert.True(reportedErrorInfo.IsAsynchronousHandler);
        Assert.True(reportedErrorInfo.FaultOccurredAfterHandlerReturned);
    }

    [Fact]
    public async Task TestActionHandlerRunSynchronouslyExecutesInlineOnNotifyingThread()
    {
        int notifyingThreadId = Environment.CurrentManagedThreadId;
        int handlerThreadId = -1;
        bool handlerCompleted = false;
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(e =>
        {
            handlerThreadId = Environment.CurrentManagedThreadId;
            handlerCompleted = true;
        });

        await testEventSource.RaiseTestEventAsync("myValue");

        Assert.True(handlerCompleted);
        Assert.Equal(notifyingThreadId, handlerThreadId);
    }
}
