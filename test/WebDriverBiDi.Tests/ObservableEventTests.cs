namespace WebDriverBiDi;

using TestUtilities;

[TestFixture]
public class ObservableEventTests
{
    [Test]
    public async Task TestCanHandleObservableEvent()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);
        await testEventSource.RaiseTestEventAsync("myValue");
        Assert.That(observedValue, Is.Not.Null);
        Assert.That(observedValue, Is.EqualTo("myValue"));
    }

    [Test]
    public async Task TestCanRemoveObservableEventHandler()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);
        await testEventSource.RaiseTestEventAsync("myValue1");
        Assert.That(observedValue, Is.Not.Null);
        Assert.That(observedValue, Is.EqualTo("myValue1"));

        observer.Unobserve();
        await testEventSource.RaiseTestEventAsync("myValue2");
        Assert.That(observedValue, Is.EqualTo("myValue1"));
    }

    [Test]
    public async Task TestDisposeRemovesObserver()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);
        await testEventSource.RaiseTestEventAsync("myValue1");
        Assert.That(observedValue, Is.EqualTo("myValue1"));

        observer.Dispose();
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));

        await testEventSource.RaiseTestEventAsync("myValue2");
        Assert.That(observedValue, Is.EqualTo("myValue1"));
    }

    [Test]
    public async Task TestUsingPatternRemovesObserver()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();

        using (EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue))
        {
            await testEventSource.RaiseTestEventAsync("myValue1");
            Assert.That(observedValue, Is.EqualTo("myValue1"));
            Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(1));
        }

        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));

        await testEventSource.RaiseTestEventAsync("myValue2");
        Assert.That(observedValue, Is.EqualTo("myValue1"));
    }

    [Test]
    public void TestDoubleDisposeDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        observer.Dispose();
        Assert.That(() => observer.Dispose(), Throws.Nothing);
    }

    [Test]
    public async Task TestDisposeAsyncRemovesObserver()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);
        await testEventSource.RaiseTestEventAsync("myValue1");
        Assert.That(observedValue, Is.EqualTo("myValue1"));

        await observer.DisposeAsync();
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));

        await testEventSource.RaiseTestEventAsync("myValue2");
        Assert.That(observedValue, Is.EqualTo("myValue1"));
    }

    [Test]
    public async Task TestAwaitUsingPatternRemovesObserver()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();

        await using (EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue))
        {
            await testEventSource.RaiseTestEventAsync("myValue1");
            Assert.That(observedValue, Is.EqualTo("myValue1"));
            Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(1));
        }

        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));

        await testEventSource.RaiseTestEventAsync("myValue2");
        Assert.That(observedValue, Is.EqualTo("myValue1"));
    }

    [Test]
    public async Task TestDoubleDisposeAsyncDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        await observer.DisposeAsync();
        Assert.That(async () => await observer.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestDisposeAsyncAfterDisposeDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        observer.Dispose();
        Assert.That(async () => await observer.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestDisposeAfterDisposeAsyncDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        await observer.DisposeAsync();
        Assert.That(() => observer.Dispose(), Throws.Nothing);
    }

    [Test]
    public void TestCannotAddMoreThanMaxObservers()
    {
        TestEventSource testEventSource = new(1);
        Assert.That(testEventSource.TestObservableEvent.MaxObserverCount, Is.EqualTo(1));
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(1));
        Assert.That(() => testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { }), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("This observable event only allows 1 handler."));

        testEventSource = new(2);
        Assert.That(testEventSource.TestObservableEvent.MaxObserverCount, Is.EqualTo(2));
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(1));
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(2));
        Assert.That(() => testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { }), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("This observable event only allows 2 handlers."));
    }

    [Test]
    public void TestToStringReturnsDescription()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { }, ObservableEventHandlerOptions.RunHandlerSynchronously, "My first handler");
        string eventSourceString = testEventSource.TestObservableEvent.ToString();
        Assert.That(eventSourceString, Is.EqualTo("ObservableEvent<TestObservableEventArgs> with observers:\n    My first handler"));
    }

    [Test]
    public void TestToStringReturnsDefaultDescription()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        string eventSourceString = testEventSource.TestObservableEvent.ToString();
        Assert.That(eventSourceString, Does.StartWith("ObservableEvent<TestObservableEventArgs> with observers:\n    EventObserver<TestObservableEventArgs> (id:"));
    }

    [Test]
    public void TestEventName()
    {
        TestEventSource testEventSource = new();
        Assert.That(testEventSource.TestObservableEvent.EventName, Is.EqualTo("testModule.testEvent"));
    }

    [Test]
    public async Task TestThrowingObserverDoesNotPreventSubsequentObserversFromBeingNotified()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => throw new InvalidOperationException("observer failure"));
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);

        Assert.That(async () => await testEventSource.RaiseTestEventAsync("myValue"), Throws.InstanceOf<InvalidOperationException>().With.Message.EqualTo("observer failure"));
        Assert.That(observedValue, Is.EqualTo("myValue"));
    }

    [Test]
    public async Task TestMultipleThrowingObserversProduceAggregateException()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => throw new InvalidOperationException("first failure"));
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => throw new ArgumentException("second failure"));

        AggregateException? caught = null;
        try
        {
            await testEventSource.RaiseTestEventAsync("myValue");
        }
        catch (AggregateException ex)
        {
            caught = ex;
        }

        Assert.That(caught, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(caught!.InnerExceptions, Has.Count.EqualTo(2));
            Assert.That(caught.InnerExceptions[0], Is.InstanceOf<InvalidOperationException>());
            Assert.That(caught.InnerExceptions[0].Message, Is.EqualTo("first failure"));
            Assert.That(caught.InnerExceptions[1], Is.InstanceOf<ArgumentException>());
            Assert.That(caught.InnerExceptions[1].Message, Is.EqualTo("second failure"));
        }
    }

    [Test]
    public async Task TestThrowingAsyncObserverDoesNotPreventSubsequentObserversFromBeingNotified()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) =>
        {
            return Task.FromException(new InvalidOperationException("async observer failure"));
        });
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);

        Assert.That(async () => await testEventSource.RaiseTestEventAsync("myValue"), Throws.InstanceOf<InvalidOperationException>().With.Message.EqualTo("async observer failure"));
        Assert.That(observedValue, Is.EqualTo("myValue"));
    }

    [Test]
    public async Task TestNoExceptionThrownWhenAllObserversSucceed()
    {
        string? firstValue = null;
        string? secondValue = null;
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => firstValue = e.EventValue);
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => secondValue = e.EventValue);

        Assert.That(async () => await testEventSource.RaiseTestEventAsync("myValue"), Throws.Nothing);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(firstValue, Is.EqualTo("myValue"));
            Assert.That(secondValue, Is.EqualTo("myValue"));
        }
    }

    [Test]
    public async Task TestHandlerRunAsynchronouslyWithSynchronousExceptionPropagates()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(
            (TestObservableEventArgs e) => Task.FromException(new InvalidOperationException("sync fire-and-forget failure")),
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        Assert.That(
            async () => await testEventSource.RaiseTestEventAsync("myValue"),
            Throws.InstanceOf<InvalidOperationException>().With.Message.EqualTo("sync fire-and-forget failure"));
    }

    [Test]
    public async Task TestHandlerRunAsynchronouslyWithSynchronousExceptionDoesNotPreventSubsequentObservers()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(
            (TestObservableEventArgs e) => Task.FromException(new InvalidOperationException("sync fire-and-forget failure")),
            ObservableEventHandlerOptions.RunHandlerAsynchronously);
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);

        Assert.That(
            async () => await testEventSource.RaiseTestEventAsync("myValue"),
            Throws.InstanceOf<InvalidOperationException>().With.Message.EqualTo("sync fire-and-forget failure"));
        Assert.That(observedValue, Is.EqualTo("myValue"));
    }

    [Test]
    public async Task TestHandlerRunAsynchronouslyWithAsyncExceptionDoesNotCauseUnobservedTaskException()
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
            using ManualResetEventSlim handlerFaulted = new(false);
            TestEventSource testEventSource = new();
            testEventSource.TestObservableEvent.AddObserver(
                async (TestObservableEventArgs e) =>
                {
                    await Task.Yield();
                    handlerFaulted.Set();
                    throw new InvalidOperationException("async fire-and-forget failure");
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously);

            await testEventSource.RaiseTestEventAsync("myValue");

            // Wait for the handler to fault before triggering GC.
            handlerFaulted.Wait(TimeSpan.FromSeconds(5));

            // Force garbage collection to trigger UnobservedTaskException
            // for any task whose exception was not observed.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Allow time for the unobserved exception event to fire.
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            Assert.That(unobservedExceptionRaised, Is.False);
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= handler;
        }
    }

    [Test]
    public async Task TestHandlerRunAsynchronouslyWithAsyncSuccessCompletesWithoutError()
    {
        bool handlerCompleted = false;
        using ManualResetEventSlim handlerReachedAsync = new(false);
        using ManualResetEventSlim handlerFinished = new(false);
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(
            async (TestObservableEventArgs e) =>
            {
                handlerReachedAsync.Set();
                await Task.Yield();
                handlerCompleted = true;
                handlerFinished.Set();
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await testEventSource.RaiseTestEventAsync("myValue");

        // handlerReachedAsync is set before the first real await, so the handler
        // has started but has not yet set handlerCompleted.
        handlerReachedAsync.Wait(TimeSpan.FromSeconds(5));
        Assert.That(handlerCompleted, Is.False, "Fire-and-forget handler should not have completed synchronously");

        handlerFinished.Wait(TimeSpan.FromSeconds(5));
        Assert.That(handlerCompleted, Is.True, "Fire-and-forget handler should have completed asynchronously");
    }

    [Test]
    public async Task TestAsyncHandlerRaisesIncrementThenDecrementEventSourceEvent()
    {
        using ManualResetEventSlim release = new(initialState: false);
        using TestEventListener listener = new();
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(
            async (TestObservableEventArgs e) =>
            {
                await Task.Run(() => release.Wait(TimeSpan.FromSeconds(5)));
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await testEventSource.RaiseTestEventAsync("myValue");

        // Wait for the increment event to land.
        for (int i = 0; i < 50 && listener.GetEventsForEventName("AsyncHandlerTaskCount").Count == 0; i++)
        {
            await Task.Delay(20);
        }

        List<System.Diagnostics.Tracing.EventWrittenEventArgs> incrementEvents = listener.GetEventsForEventName("AsyncHandlerTaskCount");
        Assert.That(incrementEvents, Is.Not.Empty, "increment should raise an AsyncHandlerTaskCount event");
        int? incrementPayload = incrementEvents[0].Payload is [int c] ? c : null;
        Assert.That(incrementPayload, Is.Not.Null);

        release.Set();

        // Wait for the decrement event to land.
        for (int i = 0; i < 50 && listener.GetEventsForEventName("AsyncHandlerTaskCount").Count < 2; i++)
        {
            await Task.Delay(20);
        }

        List<System.Diagnostics.Tracing.EventWrittenEventArgs> allEvents = listener.GetEventsForEventName("AsyncHandlerTaskCount");
        Assert.That(allEvents, Has.Count.GreaterThanOrEqualTo(2), "both increment and decrement should raise events");

        // The decrement value must be strictly less than the increment value, regardless
        // of any concurrent activity from other tests, because each increment is paired
        // with exactly one decrement.
        int firstValue = (int)allEvents[0].Payload![0]!;
        int lastValue = (int)allEvents[allEvents.Count - 1].Payload![0]!;
        Assert.That(lastValue, Is.LessThan(firstValue), "decrement value should be lower than preceding increment value");
    }

    [Test]
    public async Task TestAsyncHandlerFaultStillRaisesDecrementEventSourceEvent()
    {
        using TestEventListener listener = new();
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(
            async (TestObservableEventArgs e) =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(30));
                throw new InvalidOperationException("async fault");
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await testEventSource.RaiseTestEventAsync("myValue");

        for (int i = 0; i < 50 && listener.GetEventsForEventName("AsyncHandlerTaskCount").Count < 2; i++)
        {
            await Task.Delay(20);
        }

        List<System.Diagnostics.Tracing.EventWrittenEventArgs> allEvents = listener.GetEventsForEventName("AsyncHandlerTaskCount");
        Assert.That(allEvents, Has.Count.GreaterThanOrEqualTo(2), "increment and decrement should both fire even when handler faults");
    }

    [Test]
    public async Task TestSynchronouslyCompletedAsyncHandlerDoesNotRaiseCounterEvents()
    {
        using TestEventListener listener = new();
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(
            (TestObservableEventArgs e) => Task.CompletedTask,
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await testEventSource.RaiseTestEventAsync("myValue");

        // Give any would-be continuation a chance to fire.
        await Task.Delay(TimeSpan.FromMilliseconds(50));

        Assert.That(listener.GetEventsForEventName("AsyncHandlerTaskCount"), Is.Empty, "no counter events should fire when async handler completes synchronously");
    }

    [Test]
    public async Task TestSynchronousHandlerDoesNotRaiseCounterEvents()
    {
        using TestEventListener listener = new();
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(
            (TestObservableEventArgs e) => { },
            ObservableEventHandlerOptions.RunHandlerSynchronously);

        await testEventSource.RaiseTestEventAsync("myValue");

        await Task.Delay(TimeSpan.FromMilliseconds(50));

        Assert.That(listener.GetEventsForEventName("AsyncHandlerTaskCount"), Is.Empty, "no counter events should fire for synchronous handlers");
    }

    [Test]
    public void TestEventArgsCopySemantics()
    {
        TestObservableEventArgs testEventArgs = new("foo");
        TestObservableEventArgs copy = testEventArgs with { };
        Assert.That(copy, Is.EqualTo(testEventArgs));
    }

    [Test]
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
            workers.Add(Task.Run(() =>
            {
                for (int j = 0; j < addRemoveIterationsPerWorker; j++)
                {
                    EventObserver<TestObservableEventArgs> transient = observable.AddObserver(_ => { });
                    transient.Unobserve();
                }
            }));
        }

        for (int i = 0; i < notificationWorkers; i++)
        {
            workers.Add(Task.Run(async () =>
            {
                for (int j = 0; j < raisesPerWorker; j++)
                {
                    await testEventSource.RaiseTestEventAsync("stress");
                }
            }));
        }

        await Task.WhenAll(workers);

        // After all churn has ceased, only the two steady-state observers should remain.
        Assert.That(observable.CurrentObserverCount, Is.EqualTo(2), "only the steady-state observers should remain after concurrent add/remove churn");

        // A final raise after the churn must invoke exactly the two steady observers.
        int invocationsBeforeFinalRaise = steadyInvocations;
        await testEventSource.RaiseTestEventAsync("post-stress");
        Assert.That(steadyInvocations - invocationsBeforeFinalRaise, Is.EqualTo(2), "after churn, a single raise should invoke exactly the two remaining observers");

        steady1.Unobserve();
        steady2.Unobserve();
    }

    [Test]
    public void TestIsCapturingReturnsFalseWhenNotCapturing()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        Assert.That(observer.IsCapturing, Is.False);
    }

    [Test]
    public void TestIsCapturingReturnsTrueWhenCapturing()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        observer.StartCapturingTasks();
        Assert.That(observer.IsCapturing, Is.True);
        observer.StopCapturingTasks();
    }

    [Test]
    public void TestStartCapturingWithActiveCaptureThrows()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        observer.StartCapturingTasks();
        Assert.That(() => observer.StartCapturingTasks(), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("already has an active capture session"));
        observer.StopCapturingTasks();
    }

    [Test]
    public void TestStopCapturingWithoutStartCapturingIsNoOp()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        Assert.That(() => observer.StopCapturingTasks(), Throws.Nothing);
        Assert.That(observer.IsCapturing, Is.False);
    }

    [Test]
    public void TestCallingStopCapturingRepeatedlyDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        observer.StartCapturingTasks();
        observer.StopCapturingTasks();
        Assert.That(() => observer.StopCapturingTasks(), Throws.Nothing);
    }

    [Test]
    public async Task TestWaitForCapturedTasksAsyncCountZeroThrows()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        observer.StartCapturingTasks();
        Assert.That(async () => await observer.WaitForCapturedTasksAsync(0, TimeSpan.FromMilliseconds(100)), Throws.InstanceOf<ArgumentException>().With.Message.Contains("must be greater than 0"));
        observer.StopCapturingTasks();
    }

    [Test]
    public async Task TestWaitForCapturedTasksAsyncWithoutStartCapturingThrows()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        Assert.That(async () => await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromMilliseconds(100)), Throws.InstanceOf<InvalidOperationException>().With.Message.Contains("No capture session is active"));
    }

    [Test]
    public async Task TestWaitForCapturedTasksAsync()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");
        Task[] tasks = await observer.WaitForCapturedTasksAsync(2, TimeSpan.FromMilliseconds(100));
        Assert.That(tasks, Has.Length.EqualTo(2));
        observer.StopCapturingTasks();
    }

    [Test]
    public async Task TestWaitForCapturedTasksAsyncCanTimeout()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        Task[] tasks = await observer.WaitForCapturedTasksAsync(2, TimeSpan.FromMilliseconds(100));
        Assert.That(tasks, Has.Length.EqualTo(1));
        Assert.That(observer.IsCapturing, Is.True);
        observer.StopCapturingTasks();
    }

    [Test]
    public async Task TestWaitForCapturedTasksAsyncCanBeCancelled()
    {
        CancellationTokenSource cancellationTokenSource = new();
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        Task waitTask = observer.WaitForCapturedTasksAsync(2, TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        cancellationTokenSource.Cancel();
        Assert.That(async () => await waitTask, Throws.InstanceOf<OperationCanceledException>().With.Message.Contains("Wait cancelled waiting for captured event handler tasks"));
        Assert.That(observer.IsCapturing, Is.True);
        observer.StopCapturingTasks();
    }

    [Test]
    public async Task TestGetCapturedTasksWithWithoutStartCaptureReturnsEmptyArray()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        Task[] tasks = observer.GetCapturedTasks();
        Assert.That(tasks, Is.Empty);
    }

    [Test]
    public async Task TestGetCapturedTasksWithActiveCaptureAndNoTasksReturnsEmptyArray()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        observer.StartCapturingTasks();
        Task[] tasks = observer.GetCapturedTasks();
        Assert.That(tasks, Is.Empty);
        observer.StopCapturingTasks();
    }

    [Test]
    public async Task TestGetCapturedTasksWithActiveCaptureReturnsPendingTasks()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");
        Task[] tasks = observer.GetCapturedTasks();
        Assert.That(tasks, Has.Length.EqualTo(2));
        observer.StopCapturingTasks();
    }

    [Test]
    public async Task TestWaitForCapturedTasksCompleteAsync()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");
        bool fulfilled = await observer.WaitForCapturedTasksCompleteAsync(2, TimeSpan.FromMilliseconds(100));
        Assert.That(fulfilled, Is.True);
        observer.StopCapturingTasks();
    }

    [Test]
    public async Task TestWaitForCapturedTasksCompleteAsyncCanTimeout()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        bool fulfilled = await observer.WaitForCapturedTasksCompleteAsync(2, TimeSpan.FromMilliseconds(100));
        Assert.That(fulfilled, Is.False);
        Assert.That(observer.IsCapturing, Is.True);
        observer.StopCapturingTasks();
    }

    [Test]
    public async Task TestWaitForCapturedTasksCompleteAsyncCanBeCancelledWaitingForTaskCapture()
    {
        CancellationTokenSource cancellationTokenSource = new();
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        Task waitTask = observer.WaitForCapturedTasksCompleteAsync(2, TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        cancellationTokenSource.Cancel();
        Assert.That(async () => await waitTask, Throws.InstanceOf<OperationCanceledException>().With.Message.Contains("Wait cancelled waiting for captured event handler tasks"));
        Assert.That(observer.IsCapturing, Is.True);
        observer.StopCapturingTasks();
    }

    [Test]
    public async Task TestWaitForCapturedTasksCompleteAsyncCanBeCancelledWaitingForTaskCompletion()
    {
        CountdownEvent countdownEvent = new(2);
        CancellationTokenSource cancellationTokenSource = new();
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
            async (TestObservableEventArgs e) =>
            {
                countdownEvent.Signal();
                await Task.Delay(TimeSpan.FromSeconds(2));
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");

        // Ensure both handlers have started before cancelling.
        countdownEvent.Wait();

        Task waitTask = observer.WaitForCapturedTasksCompleteAsync(2, TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        cancellationTokenSource.Cancel();
        Assert.That(async () => await waitTask, Throws.InstanceOf<OperationCanceledException>());

        // WaitForAsync auto-closes the channel once count tasks are collected, so
        // IsCapturing is false by the time the task-completion wait is cancelled.
        Assert.That(observer.IsCapturing, Is.False);
    }

    [Test]
    public async Task TestWaitForCapturedTasksCompleteAsyncReturnsFalseWhenNoRemainingTimeAfterCapture()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });

        observer.StartCapturingTasks();

        // Raise only 1 event but request 2 tasks. WaitForCapturedTasksAsync will block
        // for the full timeout duration waiting for the second task that never arrives,
        // guaranteeing remainingTime <= TimeSpan.Zero when control returns to
        // WaitForCapturedTasksCompleteAsync regardless of machine speed.
        await testEventSource.RaiseTestEventAsync("myValue1");
        bool fulfilled = await observer.WaitForCapturedTasksCompleteAsync(2, TimeSpan.FromMilliseconds(50));
        Assert.That(fulfilled, Is.False);
        observer.StopCapturingTasks();
    }

    [Test]
    public async Task TestWaitForCapturedTasksCompleteAsyncReturnsFalseWhenExecutionTimesOut()
    {
        CountdownEvent countdownEvent = new(2);
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
            async (TestObservableEventArgs e) =>
            {
                countdownEvent.Signal();
                await Task.Delay(TimeSpan.FromSeconds(5));
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");

        // Ensure both handlers have started before waiting, so capture completes
        // immediately and the remaining timeout is spent waiting for slow execution.
        countdownEvent.Wait();

        bool fulfilled = await observer.WaitForCapturedTasksCompleteAsync(2, TimeSpan.FromMilliseconds(100));
        Assert.That(fulfilled, Is.False);

        // Capture session is auto-closed once count tasks are collected.
        Assert.That(observer.IsCapturing, Is.False);
    }

    [Test]
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

        Assert.That(async () => await observer.WaitForCapturedTasksCompleteAsync(1, TimeSpan.FromMilliseconds(100)), Throws.InstanceOf<InvalidOperationException>().With.Message.EqualTo("capture failure"));
        observer.StopCapturingTasks();
    }

    [Test]
    public void TestDisposeWithActiveCaptureDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        observer.StartCapturingTasks();
        Assert.That(observer.IsCapturing, Is.True);
        Assert.That(() => observer.Dispose(), Throws.Nothing);
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));
    }

    [Test]
    public async Task TestDisposeAsyncWithActiveCaptureDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((e) => { });
        observer.StartCapturingTasks();
        Assert.That(observer.IsCapturing, Is.True);
        Assert.That(async () => await observer.DisposeAsync(), Throws.Nothing);
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));
    }

    [Test]
    public async Task TestCapturingTasksFromSynchronousHandler()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
            (e) => { },
            ObservableEventHandlerOptions.RunHandlerSynchronously);
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue");
        Task[] tasks = observer.GetCapturedTasks();
        Assert.That(tasks, Has.Length.EqualTo(1));
        Assert.That(tasks[0].IsCompletedSuccessfully, Is.True);
        observer.StopCapturingTasks();
    }

    [Test]
    public async Task TestCapturingTasksFromAsynchronousHandler()
    {
        bool handlerCompleted = false;
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
            async (e) =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50));
                handlerCompleted = true;
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);
        observer.StartCapturingTasks();
        await testEventSource.RaiseTestEventAsync("myValue");
        Task[] tasks = observer.GetCapturedTasks();
        Assert.That(tasks, Has.Length.EqualTo(1));
        Assert.That(tasks[0].IsCompleted, Is.False, "Captured task should still be running");
        await tasks[0];
        Assert.That(handlerCompleted, Is.True);
        observer.StopCapturingTasks();
    }

    [Test]
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
            TestEventSource testEventSource = new();
            EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
                async (e) =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50));
                    throw new InvalidOperationException("async capture failure");
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously);

            observer.StartCapturingTasks();
            await testEventSource.RaiseTestEventAsync("myValue");

            Task[] tasks = observer.GetCapturedTasks();
            Assert.That(tasks, Has.Length.EqualTo(1));

            await Task.Delay(TimeSpan.FromMilliseconds(250));

            // Force garbage collection to trigger UnobservedTaskException
            // for any task whose exception was not observed.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            await Task.Delay(TimeSpan.FromMilliseconds(100));

            Assert.That(unobservedExceptionRaised, Is.False, "Captured task exception should be observed by caller, not by the error reporter");
            Assert.That(tasks[0].IsFaulted, Is.True);
            Assert.That(tasks[0].Exception!.InnerException, Is.InstanceOf<InvalidOperationException>().With.Message.EqualTo("async capture failure"));

            observer.StopCapturingTasks();
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= handler;
        }
    }

    [Test]
    public async Task TestConcurrentWaitForForCapturedTasksAsyncCallsGetUniqueBatches()
    {
        // Two concurrent callers each asking for 3 tasks should receive unique,
        // non-interleaved batches — one gets tasks 1-3, the other gets tasks 4-6.
        TestEventSource testEventSource = new();

        // Local function typed as Task so the Func<T,Task> overload is chosen rather than
        // Action<T>. An Action<T> wrapper always returns Task.CompletedTask (a singleton), which
        // would collapse all tasks in the HashSet to a count of 1. Task.FromResult(Guid.NewGuid())
        // creates a distinct Task<Guid> object on every invocation.
        static Task DistinctTaskHandler(TestObservableEventArgs _) => Task.FromResult(Guid.NewGuid());
        await using EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(DistinctTaskHandler);
        observer.StartCapturingTasks();

        // Start both waiters before any events are raised.
        Task<Task[]> waiter1 = observer.WaitForCapturedTasksAsync(3, TimeSpan.FromSeconds(5));
        Task<Task[]> waiter2 = observer.WaitForCapturedTasksAsync(3, TimeSpan.FromSeconds(5));

        // Raise 6 events so both waiters can be satisfied.
        for (int i = 0; i < 6; i++)
        {
            await testEventSource.RaiseTestEventAsync($"value{i}");
        }

        Task[][] results = await Task.WhenAll(waiter1, waiter2);
        Task[] batch1 = results[0];
        Task[] batch2 = results[1];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(batch1, Has.Length.EqualTo(3), "first waiter should receive exactly 3 tasks");
            Assert.That(batch2, Has.Length.EqualTo(3), "second waiter should receive exactly 3 tasks");

            // Every task should appear exactly once across both batches.
            HashSet<Task> allTasks = [.. batch1, .. batch2];
            Assert.That(allTasks, Has.Count.EqualTo(6), "no task should appear in both batches");
        }
    }

    [Test]
    public async Task TestWaitForCapturedTasksAsyncUnblocksOnStopCapturing()
    {
        // WaitForAsync blocked waiting for 3 tasks should return early (with however
        // many arrived so far) when StopCapturing is called from another thread.
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(_ => { });
        observer.StartCapturingTasks();

        // Raise only 1 event, then start a waiter that needs 3.
        await testEventSource.RaiseTestEventAsync("value1");
        Task<Task[]> waitTask = observer.WaitForCapturedTasksAsync(3, TimeSpan.FromSeconds(10));

        // Stop the capture session from another thread — waiter should unblock.
        observer.StopCapturingTasks();

        Task[] tasks = await waitTask;
        Assert.That(tasks, Has.Length.EqualTo(1), "should return the 1 task that arrived before StopCapturing");
    }

    [Test]
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
        Task<Task[]> waitTask = observer.WaitForCapturedTasksAsync(2, TimeSpan.FromSeconds(5));
        observer.StopCapturingTasks();

        Task[] tasks = await waitTask;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tasks, Has.Length.EqualTo(2), "both tasks must be returned regardless of race outcome");
            Assert.That(observer.IsCapturing, Is.False, "capture session must be ended");
        }
    }

    [Test]
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

            using ManualResetEventSlim handlerStarted = new(false);
            using ManualResetEventSlim allowFault = new(false);

            EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
                async (TestObservableEventArgs e) =>
                {
                    if (e.EventValue == "raced")
                    {
                        handlerStarted.Set();
                        await Task.Run(() => allowFault.Wait(TimeSpan.FromSeconds(5)));
                        throw new InvalidOperationException("raced task fault");
                    }
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously);

            observer.StartCapturingTasks();

            // Raise the event that WaitForAsync(1) will collect.
            await testEventSource.RaiseTestEventAsync("collected");

            // Raise the raced event — its task will be in-flight when the channel closes.
            await testEventSource.RaiseTestEventAsync("raced");
            handlerStarted.Wait(TimeSpan.FromSeconds(5));

            // WaitForAsync collects 1 task, auto-closes the channel, and drains the raced task.
            Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5));
            Assert.That(tasks, Has.Length.EqualTo(1));

            // Let the raced handler fault.
            allowFault.Set();
            await Task.Delay(250);

            // Force garbage collection to trigger UnobservedTaskException
            // for any task whose exception was not observed.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            await Task.Delay(100);

            Assert.That(unobservedExceptionRaised, Is.False, "raced task fault must be observed by the drain, not trigger UnobservedTaskException");
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= unobservedHandler;
        }
    }

    [Test]
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
            Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5));
            Assert.That(tasks, Has.Length.EqualTo(1));

            await Task.Delay(100);

            // Force garbage collection to trigger UnobservedTaskException
            // for any task whose exception was not observed.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            await Task.Delay(100);

            Assert.That(unobservedExceptionRaised, Is.False, "already-faulted raced task must be observed by the direct drain path");
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= UnobservedHandler;
        }
    }

    [Test]
    public async Task TestWaitForCapturedTasksAsyncDoesNotRemovePendingTasksWhenConcurrentWaiterIsActive()
    {
        // When two WaitForAsync callers are both active (waitingReaderCount == 2),
        // the first to finish must NOT drain the channel — those tasks belong to the
        // second waiter.
        //
        // Determinism guarantee: WaitForCapturedTasksAsync increments waitingReaderCount
        // synchronously under captureLock before its first await point
        // (captureReadSemaphore.WaitAsync). Awaiting Task.Delay(0) after starting each
        // waiter drives each task's synchronous preamble to completion on the thread pool,
        // ensuring both have incremented waitingReaderCount before any events are raised.
        // waiter1 then blocks inside WaitToReadAsync on the empty channel (it holds the
        // semaphore); waiter2 blocks on captureReadSemaphore.WaitAsync. Both are registered
        // as active readers when the events arrive.
        TestEventSource testEventSource = new();
        static Task DistinctTaskHandler(TestObservableEventArgs _) => Task.FromResult(Guid.NewGuid());
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(DistinctTaskHandler);
        observer.StartCapturingTasks();

        // Start each waiter and yield once to advance it to its first await point,
        // guaranteeing waitingReaderCount == 2 before any events enter the channel.
        Task<Task[]> waiter1 = observer.WaitForCapturedTasksAsync(2, TimeSpan.FromSeconds(5));
        await Task.Delay(0);
        Task<Task[]> waiter2 = observer.WaitForCapturedTasksAsync(2, TimeSpan.FromSeconds(5));
        await Task.Delay(0);

        // Raise 4 events — enough for both waiters.
        for (int i = 0; i < 4; i++)
        {
            await testEventSource.RaiseTestEventAsync($"value{i}");
        }

        Task[][] results = await Task.WhenAll(waiter1, waiter2);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[0], Has.Length.EqualTo(2), "first waiter should get its 2 tasks");
            Assert.That(results[1], Has.Length.EqualTo(2), "second waiter should get its 2 tasks — not drained");
            HashSet<Task> all = [.. results[0], .. results[1]];
            Assert.That(all, Has.Count.EqualTo(4), "all 4 tasks must be delivered with no overlap");
        }

        observer.StopCapturingTasks();
    }

    [Test]
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

        Task[] firstBatch = await observer.WaitForCapturedTasksAsync(2, TimeSpan.FromSeconds(5));

        // Raise further events — these must not be captured because the channel is closed.
        for (int i = 3; i < 6; i++)
        {
            await testEventSource.RaiseTestEventAsync($"value{i}");
        }

        // GetCapturedTasks should return nothing (no active capture) and not the extra events.
        Task[] remaining = observer.GetCapturedTasks();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(firstBatch, Has.Length.EqualTo(2), "WaitForAsync should return exactly 2 tasks");
            Assert.That(remaining, Has.Length.EqualTo(0), "no tasks should accumulate after channel auto-close");
            Assert.That(observer.IsCapturing, Is.False, "capture session should be ended after count reached");
        }
    }

    private class TestEventSource
    {
        private readonly ObservableEvent<TestObservableEventArgs> testObservableEvent = new("testModule.testEvent");

        public TestEventSource(uint maxObserverCount = 0)
        {
            this.testObservableEvent = new ObservableEvent<TestObservableEventArgs>("testModule.testEvent", maxObserverCount);
        }

        public ObservableEvent<TestObservableEventArgs> TestObservableEvent => this.testObservableEvent;

        public async Task RaiseTestEventAsync(string eventValue)
        {
            await this.testObservableEvent.NotifyObserversAsync(new TestObservableEventArgs(eventValue));
        }
    }

    private record TestObservableEventArgs : WebDriverBiDiEventArgs
    {
        private readonly string eventValue = string.Empty;

        public TestObservableEventArgs(string eventValue)
        {
            this.eventValue = eventValue;
        }

        public string EventValue => this.eventValue;
    }
}
