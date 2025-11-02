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
    public async Task TestCanExecuteEventHandlersAsynchronously()
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
        bool eventSet = handler.WaitForCheckpoint(TimeSpan.FromMilliseconds(100));
        Assert.That(eventSet, Is.True);
        Assert.That(handler.IsCheckpointSet, Is.False);

        Task[] eventTasks = handler.GetCheckpointTasks();
        Assert.That(eventTasks, Has.Length.EqualTo(1));
        await eventTasks[0];
        Assert.That(eventTasks[0].IsCompletedSuccessfully, Is.True);
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
    public void TestCannotAddMoreThanMaxObservers()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new(1);
        Assert.That(testEventSource.TestObservableEvent.MaxObserverCount, Is.EqualTo(1));
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(1));
        Assert.That(() => testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => _ = e.EventValue), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("This observable event only allows 1 handler."));

        testEventSource = new(2);
        Assert.That(testEventSource.TestObservableEvent.MaxObserverCount, Is.EqualTo(2));
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(1));
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(2));
        Assert.That(() => testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => _ = e.EventValue), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("This observable event only allows 2 handlers."));
    }

    [Test]
    public void TestToStringReturnsDescription()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue, ObservableEventHandlerOptions.None, "My first handler");
        string eventSourceString = testEventSource.TestObservableEvent.ToString();
        Assert.That(eventSourceString, Is.EqualTo("ObservableEvent<TestObservableEventArgs> with observers:\n    My first handler"));
    }

    [Test]
    public void TestToStringReturnsDefaultDescription()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);
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
        Assert.That(() => handler.SetCheckpoint(0), Throws.InstanceOf<ArgumentException>().With.Message.Contains("must be greater than 1"));
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
    public async Task TestObserverCheckpointCanCaptureMultipleEventTasks()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        observer.SetCheckpoint(2);
        await testEventSource.RaiseTestEventAsync("myValue1");
        await testEventSource.RaiseTestEventAsync("myValue2");
        bool checkpointFulfilled = observer.WaitForCheckpoint(TimeSpan.FromMilliseconds(100));
        Assert.That(checkpointFulfilled, Is.True);
        Assert.That(observer.IsCheckpointSet, Is.False);
        Task[] tasks = observer.GetCheckpointTasks();
        Assert.That(tasks, Has.Length.EqualTo(2));
    }

    [Test]
    public async Task TestObserverCheckpointBeUnfulfilledIfTimeoutExpires()
    {
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        observer.SetCheckpoint(2);
        await testEventSource.RaiseTestEventAsync("myValue1");
        bool checkpointFulfilled = observer.WaitForCheckpoint(TimeSpan.FromMilliseconds(100));
        Assert.That(checkpointFulfilled, Is.False);
        Assert.That(observer.IsCheckpointSet, Is.True);
        Task[] tasks = observer.GetCheckpointTasks();
        Assert.That(tasks, Has.Length.EqualTo(1));
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
    public void TestWaitForCheckpointWithoutSettingIsANoOp()
    {
        string? observedValue = null;
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);
        bool checkpointFulfilled = observer.WaitForCheckpoint(TimeSpan.FromMilliseconds(100));
        Assert.That(checkpointFulfilled, Is.True);
        Assert.That(observer.GetCheckpointTasks(), Is.Empty);
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
