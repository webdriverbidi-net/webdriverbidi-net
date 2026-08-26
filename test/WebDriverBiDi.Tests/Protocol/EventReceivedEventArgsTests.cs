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

    [Fact]
    public void TestPayloadAndEnvelopeExtensionDataAreExposedSeparately()
    {
        System.Text.Json.JsonSerializerOptions options = new() { TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver() };
        EventMessage<TestEventArgs>? message = System.Text.Json.JsonSerializer.Deserialize(
            """{"type":"event","method":"protocol.event","goog:channel":"channel","params":{"paramName":"v"}}""",
            System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateValueInfo<EventMessage<TestEventArgs>>(options, new JsonConverters.EventMessageJsonConverter<TestEventArgs>()));
        Assert.NotNull(message);
        ReceivedDataDictionary payload = new(new Dictionary<string, object?> { ["goog:extra"] = "payload" });

        EventReceivedEventArgs eventArgs = new(message, payload);
        Assert.Same(payload, eventArgs.AdditionalData);
        Assert.Equal("channel", eventArgs.AdditionalEventProperties["goog:channel"]);

        EventReceivedEventArgs withoutPayload = new(message);
        Assert.Empty(withoutPayload.AdditionalData);
        Assert.Equal("channel", withoutPayload.AdditionalEventProperties["goog:channel"]);
    }
}
