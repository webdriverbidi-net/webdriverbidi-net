namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.TestUtilities;

/// <summary>
/// Tests for <see cref="CommandResponseMessageJsonConverter{T}"/>, the library-owned envelope converter that
/// <see cref="CommandParameters{T}"/> pairs with <see cref="CommandResponseMessage{T}"/> so that a consumer only
/// has to register their result type.
/// </summary>
public class CommandResponseMessageJsonConverterTests
{
    private static readonly JsonSerializerOptions Options = new() { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

    private static JsonTypeInfo<CommandResponseMessage<TestCommandResult>> ResponseTypeInfo =>
        JsonMetadataServices.CreateValueInfo<CommandResponseMessage<TestCommandResult>>(Options, new CommandResponseMessageJsonConverter<TestCommandResult>());

    [Fact]
    public void TestReadsEnvelopeAndResult()
    {
        string json = """{"type":"success","id":42,"result":{"value":"hello","elapsed":1.5},"goog:extra":{"nested":true}}""";
        CommandResponseMessage<TestCommandResult>? response = JsonSerializer.Deserialize(json, ResponseTypeInfo);

        Assert.NotNull(response);
        Assert.Equal("success", response.Type);
        Assert.Equal(42, response.Id);
        TestCommandResult result = Assert.IsType<TestCommandResult>(response.Result);
        Assert.Equal("hello", result.Value);
        Assert.Equal(1.5, result.ElapsedMilliseconds);
        Assert.True(response.AdditionalData.ContainsKey("goog:extra"));
    }

    [Fact]
    public void TestAcceptsPropertiesInAnyOrder()
    {
        string json = """{"result":{"value":"hello"},"id":1,"type":"success"}""";
        CommandResponseMessage<TestCommandResult>? response = JsonSerializer.Deserialize(json, ResponseTypeInfo);
        Assert.NotNull(response);
        Assert.Equal(1, response.Id);
        Assert.Equal("hello", ((TestCommandResult)response.Result).Value);
        Assert.Empty(response.AdditionalData);
    }

    [Theory]
    [InlineData("""[]""", "must be a JSON object")]
    [InlineData("""{"id":1,"result":{"value":"v"}}""", "missing the required 'type'")]
    [InlineData("""{"type":7,"id":1,"result":{"value":"v"}}""", "'type' property must be a string")]
    [InlineData("""{"type":"success","result":{"value":"v"}}""", "missing the required 'id'")]
    [InlineData("""{"type":"success","id":"1","result":{"value":"v"}}""", "'id' property must be a number")]
    [InlineData("""{"type":"success","id":1}""", "missing the required 'result'")]
    [InlineData("""{"type":"success","id":1,"result":null}""", "'result' property must not be null")]
    public void TestRejectsMalformedEnvelopes(string json, string expectedMessage)
    {
        JsonException exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(json, ResponseTypeInfo));
        Assert.Contains(expectedMessage, exception.Message);
    }

    [Fact]
    public void TestDoesNotSerialize()
    {
        CommandResponseMessage<TestCommandResult> response = new();
        Assert.Throws<NotSupportedException>(() => JsonSerializer.Serialize(response, ResponseTypeInfo));
    }
}
