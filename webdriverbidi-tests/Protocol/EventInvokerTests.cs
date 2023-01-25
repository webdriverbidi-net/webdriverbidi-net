namespace WebDriverBidi.Protocol;

using TestUtilities;

[TestFixture]
public class EventInvokerTests
{
    [Test]
    public void TestCanInvokeEvent()
    {
        bool eventInvoked = false;
        Action<EventInfo<TestEventArgs>> action = (EventInfo<TestEventArgs> info) => { eventInvoked = true; };
        EventInvoker<TestEventArgs> invoker = new(action);
        invoker.InvokeEvent(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);
        Assert.That(eventInvoked, Is.True);
    }

    [Test]
    public void TestInvokeEventWithInvalidObjectTypeThrows()
    {
        Action<EventInfo<TestEventArgs>> action = (EventInfo<TestEventArgs> info) => { };
        EventInvoker<TestEventArgs> invoker = new(action);
        Assert.That(() => invoker.InvokeEvent("this is an invalid object", ReceivedDataDictionary.EmptyDictionary), Throws.InstanceOf<WebDriverBidiException>());
    }
}