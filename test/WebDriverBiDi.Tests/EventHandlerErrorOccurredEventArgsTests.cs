namespace WebDriverBiDi;

[TestFixture]
public class EventHandlerErrorOccurredEventArgsTests
{
    [Test]
    public void TestCanCreateEventArgs()
    {
        EventObserverErrorInfo errorInfo = new EventObserverErrorInfo
        {
            ObservableEventName = "test.event",
            ObserverId = "observer1",
            ObserverDescription = "Test observer",
            Exception = new InvalidOperationException("Test exception"),
            IsAsynchronousHandler = true,
            FaultOccurredAfterHandlerReturned = false
        };

        EventHandlerErrorOccurredEventArgs eventArgs = new EventHandlerErrorOccurredEventArgs(errorInfo);

        Assert.That(eventArgs.ErrorInfo, Is.EqualTo(errorInfo));
    }

    [Test]
    public void TestCopySemantics()
    {
        EventObserverErrorInfo errorInfo = new EventObserverErrorInfo
        {
            ObservableEventName = "test.event",
            ObserverId = "observer1",
            ObserverDescription = "Test observer",
            Exception = new InvalidOperationException("Test exception"),
            IsAsynchronousHandler = true,
            FaultOccurredAfterHandlerReturned = false
        };

        EventHandlerErrorOccurredEventArgs eventArgs = new EventHandlerErrorOccurredEventArgs(errorInfo);
        EventHandlerErrorOccurredEventArgs copy = eventArgs with { };

        Assert.That(eventArgs, Is.EqualTo(copy));
        Assert.That(copy, Is.Not.SameAs(eventArgs));

        EventObserverErrorInfo copyErrorInfo = copy.ErrorInfo with { };
        Assert.That(copyErrorInfo, Is.EqualTo(copy.ErrorInfo));
        Assert.That(copyErrorInfo, Is.Not.SameAs(copy.ErrorInfo));
    }
}
