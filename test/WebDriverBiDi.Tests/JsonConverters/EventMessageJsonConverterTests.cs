namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.TestUtilities;

/// <summary>
/// Tests for <see cref="EventMessageJsonConverter{T}"/>, the library-owned envelope converter that
/// <see cref="Transport.RegisterEventMessage{T}"/> pairs with <see cref="EventMessage{T}"/> so that a consumer only
/// has to register their event args type.
/// </summary>
public class EventMessageJsonConverterTests
{
    private static readonly JsonSerializerOptions Options = new() { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

    private static JsonTypeInfo<EventMessage<TestEventArgs>> EventTypeInfo =>
        JsonMetadataServices.CreateValueInfo<EventMessage<TestEventArgs>>(Options, new EventMessageJsonConverter<TestEventArgs>());

    [Fact]
    public void TestReadsEnvelopeAndParams()
    {
        string json = """{"type":"event","method":"protocol.event","params":{"paramName":"paramValue"},"goog:channel":"c1"}""";
        EventMessage<TestEventArgs>? message = JsonSerializer.Deserialize(json, EventTypeInfo);

        Assert.NotNull(message);
        Assert.Equal("event", message.Type);
        Assert.Equal("protocol.event", message.EventName);
        TestEventArgs eventData = Assert.IsType<TestEventArgs>(message.EventData);
        Assert.Equal("paramValue", eventData.ParamName);
        Assert.True(message.AdditionalData.ContainsKey("goog:channel"));
    }

    [Fact]
    public void TestAllowsMissingMethodAndNullParams()
    {
        // Mirrors the attribute-based shape: 'method' was not required, and 'params' could be null.
        string json = """{"type":"event","params":null}""";
        EventMessage<TestEventArgs>? message = JsonSerializer.Deserialize(json, EventTypeInfo);
        Assert.NotNull(message);
        Assert.Equal(string.Empty, message.EventName);
        Assert.Null(message.EventData);
    }

    [Theory]
    [InlineData("""42""", "must be a JSON object")]
    [InlineData("""{"method":"protocol.event","params":{"paramName":"v"}}""", "missing the required 'type'")]
    [InlineData("""{"type":null,"method":"protocol.event","params":{"paramName":"v"}}""", "'type' property must be a string")]
    [InlineData("""{"type":"event","method":5,"params":{"paramName":"v"}}""", "'method' property must be a string")]
    [InlineData("""{"type":"event","method":"protocol.event"}""", "missing the required 'params'")]
    public void TestRejectsMalformedEnvelopes(string json, string expectedMessage)
    {
        JsonException exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(json, EventTypeInfo));
        Assert.Contains(expectedMessage, exception.Message);
    }

    [Fact]
    public void TestDoesNotSerialize()
    {
        EventMessage<TestEventArgs> message = new();
        Assert.Throws<NotSupportedException>(() => JsonSerializer.Serialize(message, EventTypeInfo));
    }
}
