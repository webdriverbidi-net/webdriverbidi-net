namespace WebDriverBiDi;

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
    public async Task TestDisposeAsyncWithActiveCheckpointDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        observer.SetCheckpoint();
        Assert.That(observer.IsCheckpointSet, Is.True);
        Assert.That(async () => await observer.DisposeAsync(), Throws.Nothing);
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));
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
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { }, ObservableEventHandlerOptions.None, "My first handler");
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
    public void TestSettingCheckpointCountZeroThrows()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> handler = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);
        Assert.That(() => handler.SetCheckpoint(0), Throws.InstanceOf<ArgumentException>().With.Message.Contains("must be greater than 0"));
    }

    [Test]
    public void TestSettingCheckpointWithActiveCheckpointThrows()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> handler = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);
        handler.SetCheckpoint();
        Assert.That(() => handler.SetCheckpoint(), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("already has a checkpoint set"));
    }


    [Test]
    public async Task TestCanUnsetCheckpoint()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);
        observer.SetCheckpoint();
        observer.UnsetCheckpoint();
        Assert.That(observer.IsCheckpointSet, Is.False);
        await testEventSource.RaiseTestEventAsync("myValue1");
        Task[] tasks = observer.GetCheckpointTasks();
        Assert.That(tasks, Is.Empty);
    }

    [Test]
    public void TestCanUnsetCheckpointWithoutSetting()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);
        observer.UnsetCheckpoint();
        Assert.That(observer.IsCheckpointSet, Is.False);
        Task[] tasks = observer.GetCheckpointTasks();
        Assert.That(tasks, Is.Empty);
    }

    [Test]
    public void UnsetCheckpointCalledTwiceShouldNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(async (TestObservableEventArgs e) => { });
        
        observer.SetCheckpoint(1);
        observer.UnsetCheckpoint();  // First call disposes counter
        
        // Second call would try to dispose already-disposed counter
        Assert.DoesNotThrow(() => observer.UnsetCheckpoint());
    }

    [Test]
    public async Task TestWaitForCheckpointAsyncWithoutSettingIsANoOp()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        bool checkpointFulfilled = await observer.WaitForCheckpointAsync(TimeSpan.FromMilliseconds(100));
        Assert.That(checkpointFulfilled, Is.True);
        Assert.That(observer.GetCheckpointTasks(), Is.Empty);
    }

    [Test]
    public async Task TestWaitForCheckpointAsyncSucceeds()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        observer.SetCheckpoint();
        Assert.That(observer.IsCheckpointSet, Is.True);
        await testEventSource.RaiseTestEventAsync("myValue");
        bool checkpointFulfilled = await observer.WaitForCheckpointAsync(TimeSpan.FromMilliseconds(100));
        Assert.That(checkpointFulfilled, Is.True);
        Assert.That(observer.IsCheckpointSet, Is.False);
    }

    [Test]
    public async Task TestWaitForCheckpointAsyncWithAsyncHandlerSucceeds()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> handler = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) =>
        {
            TaskCompletionSource taskCompletionSource = new();
            taskCompletionSource.SetResult();
            return taskCompletionSource.Task;
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        handler.SetCheckpoint();
        Assert.That(handler.IsCheckpointSet, Is.True);
        await testEventSource.RaiseTestEventAsync("myValue");
        bool eventSet = await handler.WaitForCheckpointAsync(TimeSpan.FromMilliseconds(100));
        Assert.That(eventSet, Is.True);
        Assert.That(handler.IsCheckpointSet, Is.False);

        Task[] eventTasks = handler.GetCheckpointTasks();
        Assert.That(eventTasks, Has.Length.EqualTo(1));
        await eventTasks[0];
        Assert.That(eventTasks[0].IsCompletedSuccessfully, Is.True);
    }

    [Test]
    public async Task TestWaitForCheckpointAndTasksWaitsForMultipleEventTasksToComplete()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        observer.SetCheckpoint(2);
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");
        bool checkpointFulfilled = await observer.WaitForCheckpointAndTasksAsync(TimeSpan.FromMilliseconds(100));
        Assert.That(checkpointFulfilled, Is.True);
        Assert.That(observer.IsCheckpointSet, Is.False);
        Task[] tasks = observer.GetCheckpointTasks();
        Assert.That(tasks, Has.Length.EqualTo(0));
    }

    [Test]
    public async Task TestWaitForCheckpointAndTasksAsyncCanReachTimeout()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        observer.SetCheckpoint(2);
        await testEventSource.RaiseTestEventAsync("myValue1");
        bool checkpointFulfilled = await observer.WaitForCheckpointAndTasksAsync(TimeSpan.FromMilliseconds(100));
        Assert.That(checkpointFulfilled, Is.False);
        Assert.That(observer.IsCheckpointSet, Is.True);
        Task[] tasks = observer.GetCheckpointTasks();
        Assert.That(tasks, Has.Length.EqualTo(1));
    }

    [Test]
    public void TestDisposeWithActiveCheckpointDoesNotThrow()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        observer.SetCheckpoint();
        Assert.That(observer.IsCheckpointSet, Is.True);
        Assert.That(() => observer.Dispose(), Throws.Nothing);
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));
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
    public async Task TestHandlerRunAsynchronouslyWithSynchronousExceptionCapturedByCheckpoint()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
            (TestObservableEventArgs e) => Task.FromException(new InvalidOperationException("checkpoint failure")),
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        observer.SetCheckpoint();
        try
        {
            await testEventSource.RaiseTestEventAsync("myValue");
        }
        catch (InvalidOperationException)
        {
        }

        Task[] tasks = observer.GetCheckpointTasks();
        Assert.That(tasks, Has.Length.EqualTo(1));
        Assert.That(tasks[0].IsFaulted, Is.True);
        Assert.That(tasks[0].Exception!.InnerException, Is.InstanceOf<InvalidOperationException>().With.Message.EqualTo("checkpoint failure"));
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
            TestEventSource testEventSource = new();
            testEventSource.TestObservableEvent.AddObserver(
                async (TestObservableEventArgs e) =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50));
                    throw new InvalidOperationException("async fire-and-forget failure");
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously);

            await testEventSource.RaiseTestEventAsync("myValue");

            // Allow time for the handler to complete and fault.
            await Task.Delay(TimeSpan.FromMilliseconds(250));

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
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver(
            async (TestObservableEventArgs e) =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50));
                handlerCompleted = true;
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await testEventSource.RaiseTestEventAsync("myValue");

        Assert.That(handlerCompleted, Is.False, "Fire-and-forget handler should not have completed synchronously");
        await Task.Delay(TimeSpan.FromMilliseconds(150));
        Assert.That(handlerCompleted, Is.True, "Fire-and-forget handler should have completed asynchronously");
    }

    [Test]
    public async Task TestHandlerRunAsynchronouslyWithAsyncSuccessCapturedByCheckpoint()
    {
        bool handlerCompleted = false;
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver(
            async (TestObservableEventArgs e) =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50));
                handlerCompleted = true;
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        observer.SetCheckpoint();
        await testEventSource.RaiseTestEventAsync("myValue");

        Task[] tasks = observer.GetCheckpointTasks();
        Assert.That(tasks, Has.Length.EqualTo(1));
        Assert.That(tasks[0].IsCompleted, Is.False, "Captured task should still be running");

        await Task.Delay(TimeSpan.FromMilliseconds(150));
        Assert.That(tasks[0].IsCompletedSuccessfully, Is.True, "Captured task should have completed successfully");
        Assert.That(handlerCompleted, Is.True);
    }

    [Test]
    public void TestEventArgsCopySemantics()
    {
        TestObservableEventArgs testEventArgs = new("foo");
        TestObservableEventArgs copy = testEventArgs with { };
        Assert.That(copy, Is.EqualTo(testEventArgs));
    }

    private class TestEventSource
    {
        private ObservableEvent<TestObservableEventArgs> testObservableEvent = new("testModule.testEvent");

        public TestEventSource(int maxObserverCount = 0)
        {
            this.testObservableEvent = new ObservableEvent<TestObservableEventArgs>("testModule.testEvent", maxObserverCount);
        }

        public ObservableEvent<TestObservableEventArgs> TestObservableEvent => testObservableEvent;

        public async Task RaiseTestEventAsync(string eventValue)
        {
            await this.testObservableEvent.NotifyObserversAsync(new TestObservableEventArgs(eventValue));
        }
    }

    private record TestObservableEventArgs: WebDriverBiDiEventArgs
    {
        private string eventValue = string.Empty;

        public TestObservableEventArgs(string eventValue)
        {
            this.eventValue = eventValue;
        }

        public string EventValue => this.eventValue;
    }
}
