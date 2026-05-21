using WebDriverBiDi.TestUtilities;

namespace WebDriverBiDi.Protocol;

public class EventReceivedEventArgsTests
{
    [Fact]
    public void TestCanCreateEventReceivedEventArgs()
    {
        EventReceivedEventArgs eventArgs = new(new EventMessage<TestEventArgs>());
        Assert.Empty(eventArgs.EventName);
        Assert.Null(eventArgs.EventData);
        Assert.Empty(eventArgs.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        EventReceivedEventArgs eventArgs = new(new EventMessage<TestEventArgs>());
        EventReceivedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }
}
