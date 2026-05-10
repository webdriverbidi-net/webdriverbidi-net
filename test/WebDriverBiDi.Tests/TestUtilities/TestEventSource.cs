namespace WebDriverBiDi.TestUtilities;

public class TestEventSource
{
    public TestEventSource()
        : this(TimeProvider.System)
    {
    }

    public TestEventSource(TimeProvider timeProvider)
        : this(timeProvider, 0)
    {
    }

    public TestEventSource(uint maxObserverCount)
        : this(TimeProvider.System, maxObserverCount)
    {
    }

    public TestEventSource(TimeProvider timeProvider, uint maxObserverCount)
    {
        this.TestObservableEvent = new TestCustomTimeObservableEvent<TestObservableEventArgs>("testModule.testEvent", maxObserverCount, timeProvider);
    }

    public ObservableEventInvocable<TestObservableEventArgs> TestObservableEvent { get; }

    public async Task RaiseTestEventAsync(string eventValue)
    {
        await this.TestObservableEvent.NotifyObserversAsync(new TestObservableEventArgs(eventValue));
    }
}
