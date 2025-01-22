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
        Task? eventTask = null;
        ManualResetEvent syncEvent = new(false);
        TestEventSource testEventSource = new();
        EventObserver<TestObservableEventArgs> handler = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) =>
        {
            TaskCompletionSource taskCompletionSource = new();
            eventTask = taskCompletionSource.Task;
            taskCompletionSource.SetResult();
            syncEvent.Set();
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await testEventSource.RaiseTestEventAsync("myValue");
        bool eventSet = syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.That(eventSet, Is.True);
        await eventTask!;
        Assert.That(eventTask!.IsCompletedSuccessfully, Is.True);
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

    private class TestEventSource
    {
        private ObservableEvent<TestObservableEventArgs> testObservableEvent = new();

        public TestEventSource(int maxObserverCount = 0)
        {
            this.testObservableEvent = new ObservableEvent<TestObservableEventArgs>(maxObserverCount);
        }

        public ObservableEvent<TestObservableEventArgs> TestObservableEvent => testObservableEvent;

        public async Task RaiseTestEventAsync(string eventValue)
        {
            await this.testObservableEvent.NotifyObserversAsync(new TestObservableEventArgs(eventValue));
        }
    }

    private class TestObservableEventArgs: EventArgs
    {
        private string eventValue = string.Empty;

        public TestObservableEventArgs(string eventValue)
        {
            this.eventValue = eventValue;
        }

        public string EventValue => this.eventValue;
    }
}
