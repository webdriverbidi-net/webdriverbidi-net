namespace WebDriverBiDi;

using WebDriverBiDi.TestUtilities;

public class EventInfoTests
{
    [Fact]
    public void TestCanCreateEventArgsFromEventInfo()
    {
        EventInfo<TestEventArgs> eventInfo = new(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);
        TestEventArgs eventArgs = eventInfo.ToEventArgs<TestEventArgs>();

        Assert.Equal("paramValue", eventArgs.ParamName);
        Assert.Empty(eventArgs.AdditionalData);
        Assert.Equal(eventArgs, eventArgs with { });
    }

    [Fact]
    public void TestConvertingToInvalidTypeThrows()
    {
        EventInfo<TestInvalidEventData> eventInfo = new(new TestInvalidEventData(), ReceivedDataDictionary.EmptyDictionary);
        Assert.Contains("Could not produce an EventArgs of type", Assert.ThrowsAny<WebDriverBiDiException>(() => eventInfo.ToEventArgs<TestParameterizedEventArgs>()).Message);
    }

    [Fact]
    public void TestCanCreateEventArgsFromFactoryWithSameType()
    {
        EventInfo<TestEventArgs> eventInfo = new(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);
        TestEventArgs eventArgs = eventInfo.ToEventArgs(data => data);

        Assert.Equal("paramValue", eventArgs.ParamName);
        Assert.Empty(eventArgs.AdditionalData);
    }

    [Fact]
    public void TestCanCreateEventArgsFromFactoryWithDifferentType()
    {
        EventInfo<TestValidEventData> eventInfo = new(new TestValidEventData("eventName"), ReceivedDataDictionary.EmptyDictionary);
        TestParameterizedEventArgs eventArgs = eventInfo.ToEventArgs(data => new TestParameterizedEventArgs(data));

        Assert.Equal("eventName", eventArgs.EventName);
        Assert.Empty(eventArgs.AdditionalData);
    }

    [Fact]
    public void TestFactoryOverloadSetsAdditionalData()
    {
        Dictionary<string, object?> additionalDataValues = new() { ["extra"] = "value" };
        ReceivedDataDictionary additionalData = new(additionalDataValues);
        EventInfo<TestEventArgs> eventInfo = new(new TestEventArgs(), additionalData);
        TestEventArgs eventArgs = eventInfo.ToEventArgs(data => data);

        Assert.Single(eventArgs.AdditionalData);
        Assert.Equal("value", eventArgs.AdditionalData["extra"]);
    }

    [Fact]
    public void TestEnvelopePropertiesFlowToEventArgs()
    {
        ReceivedDataDictionary payload = new(new Dictionary<string, object?> { ["goog:extra"] = "payload" });
        ReceivedDataDictionary envelope = new(new Dictionary<string, object?> { ["goog:channel"] = "channel" });
        EventInfo<TestEventArgs> eventInfo = new(new TestEventArgs(), payload, envelope);
        Assert.Same(envelope, eventInfo.AdditionalEventProperties);

        TestEventArgs direct = eventInfo.ToEventArgs<TestEventArgs>();
        Assert.Equal("payload", direct.AdditionalData["goog:extra"]);
        Assert.Equal("channel", direct.AdditionalEventProperties["goog:channel"]);

        TestEventArgs viaFactory = eventInfo.ToEventArgs(data => new TestEventArgs());
        Assert.Equal("payload", viaFactory.AdditionalData["goog:extra"]);
        Assert.Equal("channel", viaFactory.AdditionalEventProperties["goog:channel"]);
    }

    [Fact]
    public void TestTwoArgumentConstructorHasNoEnvelopeProperties()
    {
        EventInfo<TestEventArgs> eventInfo = new(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);
        Assert.Empty(eventInfo.AdditionalEventProperties);
    }
}
