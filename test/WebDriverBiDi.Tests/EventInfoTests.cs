namespace WebDriverBiDi;

using WebDriverBiDi.TestUtilities;

[TestFixture]
public class EventInfoTests
{
    [Test]
    public void TestCanCreateEventArgsFromEventInfo()
    {
        EventInfo<TestEventArgs> eventInfo = new(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);
        TestEventArgs eventArgs = eventInfo.ToEventArgs<TestEventArgs>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.ParamName, Is.EqualTo("paramValue"));
            Assert.That(eventArgs.AdditionalData, Has.Count.EqualTo(0));
            Assert.That(eventArgs with { }, Is.EqualTo(eventArgs));
        }
    }

    [Test]
    public void TestConvertingToInvalidTypeThrows()
    {
        EventInfo<TestInvalidEventData> eventInfo = new(new TestInvalidEventData(), ReceivedDataDictionary.EmptyDictionary);
        Assert.That(() => eventInfo.ToEventArgs<TestParameterizedEventArgs>(), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("Could not produce an EventArgs of type"));
    }

    [Test]
    public void TestCanCreateEventArgsFromFactoryWithSameType()
    {
        EventInfo<TestEventArgs> eventInfo = new(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);
        TestEventArgs eventArgs = eventInfo.ToEventArgs(data => data);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.ParamName, Is.EqualTo("paramValue"));
            Assert.That(eventArgs.AdditionalData, Has.Count.EqualTo(0));
        }
    }

    [Test]
    public void TestCanCreateEventArgsFromFactoryWithDifferentType()
    {
        EventInfo<TestValidEventData> eventInfo = new(new TestValidEventData("eventName"), ReceivedDataDictionary.EmptyDictionary);
        TestParameterizedEventArgs eventArgs = eventInfo.ToEventArgs(data => new TestParameterizedEventArgs(data));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.EventName, Is.EqualTo("eventName"));
            Assert.That(eventArgs.AdditionalData, Has.Count.EqualTo(0));
        }
    }

    [Test]
    public void TestFactoryOverloadSetsAdditionalData()
    {
        Dictionary<string, object?> additionalDataValues = new() { ["extra"] = "value" };
        ReceivedDataDictionary additionalData = new(additionalDataValues);
        EventInfo<TestEventArgs> eventInfo = new(new TestEventArgs(), additionalData);
        TestEventArgs eventArgs = eventInfo.ToEventArgs(data => data);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.AdditionalData, Has.Count.EqualTo(1));
            Assert.That(eventArgs.AdditionalData["extra"], Is.EqualTo("value"));
        }
    }
}
