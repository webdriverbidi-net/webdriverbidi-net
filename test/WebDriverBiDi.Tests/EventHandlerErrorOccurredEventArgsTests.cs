namespace WebDriverBiDi;

public class EventHandlerErrorOccurredEventArgsTests
{
    [Fact]
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

        Assert.Equal(errorInfo, eventArgs.ErrorInfo);
    }

    [Fact]
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

        Assert.Equal(copy, eventArgs);
        Assert.NotSame(eventArgs, copy);

        EventObserverErrorInfo copyErrorInfo = copy.ErrorInfo with { };
        Assert.Equal(copy.ErrorInfo, copyErrorInfo);
        Assert.NotSame(copy.ErrorInfo, copyErrorInfo);
    }
}
