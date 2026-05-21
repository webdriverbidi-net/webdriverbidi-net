namespace WebDriverBiDi;

using TestUtilities;

public class ObservableEventTests
{
    [Fact]
    public async Task TestCanAddHandler()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver((e) => { });
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
        EventObserver<TestObservableEventArgs> observer = testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);
        await testEventSource.RaiseTestEventAsync("myValue1");
        Assert.NotNull(observedValue);
        Assert.Equal("myValue1", observedValue);

        observer.Unobserve();
        await testEventSource.RaiseTestEventAsync("myValue2");
        Assert.Equal("myValue1", observedValue);
    }

    [Fact]
    public async Task TestCannotAddMoreThanMaxObserversUsingStandardObservers()
    {
        TestEventSource testEventSource = new(1);
        Assert.Equal(1u, testEventSource.TestObservableEvent.MaxObserverCount);
        Assert.Equal(0, testEventSource.TestObservableEvent.CurrentObserverCount);
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        Assert.Equal(1, testEventSource.TestObservableEvent.CurrentObserverCount);
        WebDriverBiDiException exception = Assert.ThrowsAny<WebDriverBiDiException>(() => testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { }));
        Assert.Equal("This observable event only allows 1 observer.", exception.Message);

        testEventSource = new(2);
        Assert.Equal(2u, testEventSource.TestObservableEvent.MaxObserverCount);
        Assert.Equal(0, testEventSource.TestObservableEvent.CurrentObserverCount);
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        Assert.Equal(1, testEventSource.TestObservableEvent.CurrentObserverCount);
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        Assert.Equal(2, testEventSource.TestObservableEvent.CurrentObserverCount);
        exception = Assert.ThrowsAny<WebDriverBiDiException>(() => testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { }));
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
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
        Assert.Equal(1, testEventSource.TestObservableEvent.CurrentObserverCount);
        testEventSource.TestObservableEvent.AddDataCollector();
        Assert.Equal(2, testEventSource.TestObservableEvent.CurrentObserverCount);
        WebDriverBiDiException exception = Assert.ThrowsAny<WebDriverBiDiException>(() => testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { }));
        Assert.Equal("This observable event only allows 2 observers.", exception.Message);
        Assert.Equal("This observable event only allows 2 observers.", Assert.ThrowsAny<WebDriverBiDiException>(() => testEventSource.TestObservableEvent.AddDataCollector()).Message);
    }

    [Fact]
    public async Task TestToStringReturnsDescriptionForEventObserver()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { }, ObservableEventHandlerOptions.RunHandlerSynchronously, "My first handler");
        string eventSourceString = testEventSource.TestObservableEvent.ToString();
        Assert.Equal("ObservableEvent<TestObservableEventArgs> with observers:\n    My first handler", eventSourceString);
    }

    [Fact]
    public async Task TestToStringReturnsDefaultDescriptionForEventObserver()
    {
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => { });
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
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => throw new InvalidOperationException("observer failure"));
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);

        Assert.Equal("observer failure", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await testEventSource.RaiseTestEventAsync("myValue"))).Message);
        Assert.Equal("myValue", observedValue);
    }

    [Fact]
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
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) =>
        {
            return Task.FromException(new InvalidOperationException("async observer failure"));
        });
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => observedValue = e.EventValue);

        Assert.Equal("async observer failure", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await testEventSource.RaiseTestEventAsync("myValue"))).Message);
        Assert.Equal("myValue", observedValue);
    }

    [Fact]
    public async Task TestNoExceptionThrownWhenAllObserversSucceed()
    {
        string? firstValue = null;
        string? secondValue = null;
        TestEventSource testEventSource = new();
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => firstValue = e.EventValue);
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => secondValue = e.EventValue);

        await testEventSource.RaiseTestEventAsync("myValue");

        Assert.Equal("myValue", firstValue);
        Assert.Equal("myValue", secondValue);
    }

    [Fact]
    public async Task TestDataCollectorsExecuteBeforeHandlers()
    {
        int capturedEventDataCount = 0;
        TestEventSource testEventSource = new();
        EventDataCollector<TestObservableEventArgs> collector = testEventSource.TestObservableEvent.AddDataCollector();
        testEventSource.TestObservableEvent.AddObserver((TestObservableEventArgs e) => capturedEventDataCount = collector.GetCollectedEventData().Count);
        await testEventSource.RaiseTestEventAsync("myValue1");
        Assert.Equal(1, capturedEventDataCount);
    }
}
