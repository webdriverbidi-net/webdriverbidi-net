using WebDriverBiDi.TestUtilities;

namespace WebDriverBiDi.Protocol;

[TestFixture]
public class EventReceivedEventArgsTests
{
    [Test]
    public void TestCanCreateEventReceivedEventArgs()
    {
        EventReceivedEventArgs eventArgs = new(new EventMessage<TestEventArgs>());
        Assert.That(eventArgs.EventName, Is.Empty);
        Assert.That(eventArgs.EventData, Is.Null);
        Assert.That(eventArgs.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        EventReceivedEventArgs eventArgs = new(new EventMessage<TestEventArgs>());
        EventReceivedEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));        
    }
}
