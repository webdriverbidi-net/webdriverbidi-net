namespace WebDriverBiDi;

using TestUtilities;

[TestFixture]
public class ObservableEventTests
{
    [Test]
    public void TestCanAddHandler()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver((e) => { });
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(1));
    }

    [Test]
    public void TestCanAddEventDataCollector()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddDataCollector();
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(1));
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
    public void TestCannotAddMoreThanMaxObserversUsingStandardObservers()
    {
        TestEventSource testEventSource = new(1);
        Assert.That(testEventSource.TestObservableEvent.MaxObserverCount, Is.EqualTo(1));
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(1));
        Assert.That(() => testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { }), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("This observable event only allows 1 observer."));

        testEventSource = new(2);
        Assert.That(testEventSource.TestObservableEvent.MaxObserverCount, Is.EqualTo(2));
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(1));
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(2));
        Assert.That(() => testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { }), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("This observable event only allows 2 observers."));
    }

    [Test]
    public void TestCannotAddMoreThanMaxObserversUsingEventDataCollectors()
    {
        TestEventSource testEventSource = new(1);
        Assert.That(testEventSource.TestObservableEvent.MaxObserverCount, Is.EqualTo(1));
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));
        testEventSource.TestObservableEvent.AddDataCollector();
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(1));
        Assert.That(() => testEventSource.TestObservableEvent.AddDataCollector(), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("This observable event only allows 1 observer."));

        testEventSource = new(2);
        Assert.That(testEventSource.TestObservableEvent.MaxObserverCount, Is.EqualTo(2));
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));
        testEventSource.TestObservableEvent.AddDataCollector();
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(1));
        testEventSource.TestObservableEvent.AddDataCollector();
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(2));
        Assert.That(() => testEventSource.TestObservableEvent.AddDataCollector(), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("This observable event only allows 2 observers."));
    }

    [Test]
    public void TestCannotAddMoreThanMaxObserversUsingMixedObservers()
    {
        TestEventSource testEventSource = new(2);
        Assert.That(testEventSource.TestObservableEvent.MaxObserverCount, Is.EqualTo(2));
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(0));
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(1));
        testEventSource.TestObservableEvent.AddDataCollector();
        Assert.That(testEventSource.TestObservableEvent.CurrentObserverCount, Is.EqualTo(2));
        Assert.That(() => testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { }), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("This observable event only allows 2 observers."));
        Assert.That(() => testEventSource.TestObservableEvent.AddDataCollector(), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("This observable event only allows 2 observers."));
    }

    [Test]
    public void TestToStringReturnsDescriptionForEventObserver()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { }, ObservableEventHandlerOptions.RunHandlerSynchronously, "My first handler");
        string eventSourceString = testEventSource.TestObservableEvent.ToString();
        Assert.That(eventSourceString, Is.EqualTo("ObservableEvent<TestObservableEventArgs> with observers:\n    My first handler"));
    }

    [Test]
    public void TestToStringReturnsDefaultDescriptionForEventObserver()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        string eventSourceString = testEventSource.TestObservableEvent.ToString();
        Assert.That(eventSourceString, Does.StartWith("ObservableEvent<TestObservableEventArgs> with observers:\n    EventObserver<TestObservableEventArgs> (id:"));
    }

    [Test]
    public void TestToStringReturnsDescriptionForEventDataCollector()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddDataCollector(description: "My first collector");
        string eventSourceString = testEventSource.TestObservableEvent.ToString();
        Assert.That(eventSourceString, Is.EqualTo("ObservableEvent<TestObservableEventArgs> with observers:\n    My first collector"));
    }

    [Test]
    public void TestToStringReturnsDefaultDescriptionForEventDataCollector()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddDataCollector();
        string eventSourceString = testEventSource.TestObservableEvent.ToString();
        Assert.That(eventSourceString, Does.StartWith("ObservableEvent<TestObservableEventArgs> with observers:\n    EventDataCollector<TestObservableEventArgs> (id:"));
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
    public async Task TestDataCollectorsExecuteBeforeHandlers()
    {
        int capturedEventDataCount = 0;
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => capturedEventDataCount = collector.GetCollectedEventData().Count);
        await testEventSource.RaiseTestEventAsync("myValue1");
        Assert.That(capturedEventDataCount, Is.EqualTo(1));
    }
}
