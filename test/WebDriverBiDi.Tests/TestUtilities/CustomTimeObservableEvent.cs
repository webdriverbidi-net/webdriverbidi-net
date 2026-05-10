namespace WebDriverBiDi.TestUtilities;

public class TestCustomTimeObservableEvent<T> : ObservableEventInvocable<T>
    where T : WebDriverBiDiEventArgs
{
    public TestCustomTimeObservableEvent(string eventName, uint maxObserverCount, TimeProvider timeProvider)
        : base(eventName, maxObserverCount)
    {
        this.TimeProvider = timeProvider;
    }
}
