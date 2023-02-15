namespace WebDriverBidi;

using WebDriverBidi.TestUtilities;

[TestFixture]
public class EventInfoTests
{
    [Test]
    public void TestCanCreateEventArgsFromEventInfo()
    {
        EventInfo<TestEventArgs> eventInfo = new(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);
        TestEventArgs eventArgs = eventInfo.ToEventArgs<TestEventArgs>();
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs.ParamName, Is.EqualTo("paramValue"));
            Assert.That(eventArgs.AdditionalData, Has.Count.EqualTo(0));
        });
    }

    [Test]
    public void TestCanCreateEventArgsUsingParameterizedConstructor()
    {
        EventInfo<TestValidEventData> eventInfo = new(new TestValidEventData("eventName"), ReceivedDataDictionary.EmptyDictionary);
        TestParameterizedEventArgs eventArgs = eventInfo.ToEventArgs<TestParameterizedEventArgs>();
        Assert.That(eventArgs.EventName, Is.EqualTo("eventName"));
    }

    [Test]
    public void TestConvertingToInvalidTypeThrows()
    {
        EventInfo<TestInvalidEventData> eventInfo = new(new TestInvalidEventData(), ReceivedDataDictionary.EmptyDictionary);
        Assert.That(() => eventInfo.ToEventArgs<TestParameterizedEventArgs>(), Throws.InstanceOf<WebDriverBidiException>().With.Message.Contains("Could not produce an EventArgs"));
    }
}
