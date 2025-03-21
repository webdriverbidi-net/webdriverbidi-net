namespace WebDriverBiDi.Protocol;

using System.Reflection.Metadata.Ecma335;
using TestUtilities;

[TestFixture]
public class EventInvokerTests
{
    [Test]
    public async Task TestCanInvokeEvent()
    {
        bool eventInvoked = false;
        Task action(EventInfo<TestEventArgs> info) {
            eventInvoked = true;
            return Task.CompletedTask;
        }
        EventInvoker<TestEventArgs> invoker = new(action);
        await invoker.InvokeEventAsync(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);
        Assert.That(eventInvoked, Is.True);
    }

    [Test]
    public void TestInvokeEventWithInvalidObjectTypeThrows()
    {
        EventInvoker<TestEventArgs> invoker = new((info) => Task.CompletedTask);
        Assert.That(async () => await invoker.InvokeEventAsync("this is an invalid object", ReceivedDataDictionary.EmptyDictionary), Throws.InstanceOf<WebDriverBiDiException>());
    }
}
