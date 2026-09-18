namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;

public class NestedValueJsonConverterTests
{
    [Fact]
    public void TestDeserializingReadsTheValueThroughItsOwnConverter()
    {
        string json = """{ "child": { "name": "inner", "child": { "name": "innermost" } }, "name": "outer" }""";
        RecursiveType? result = JsonSerializer.Deserialize<RecursiveType>(json);
        Assert.NotNull(result);
        Assert.Equal("outer", result.Name);
        Assert.NotNull(result.Child);
        Assert.Equal("inner", result.Child.Name);
        Assert.NotNull(result.Child.Child);
        Assert.Equal("innermost", result.Child.Child.Name);
        Assert.Null(result.Child.Child.Child);
    }

    [Fact]
    public void TestDeserializingNullIsHandledByTheSerializer()
    {
        // The serializer assigns null for a JSON null token without calling the converter, exactly as it
        // would for the member without one.
        RecursiveType? result = JsonSerializer.Deserialize<RecursiveType>("""{ "name": "outer", "child": null }""");
        Assert.NotNull(result);
        Assert.Null(result.Child);
    }

    [Fact]
    public void TestSerializationThrows()
    {
        // The converter is applied only to members of payloads the library receives, so it is inbound-only,
        // as the library's other read-only converters are.
        RecursiveType value = new() { Name = "outer", Child = new RecursiveType { Name = "inner" } };
        Assert.ThrowsAny<NotSupportedException>(() => JsonSerializer.Serialize(value));
    }

    private record RecursiveType
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("child")]
        [JsonConverter(typeof(NestedValueJsonConverter<RecursiveType>))]
        public RecursiveType? Child { get; init; }
    }
}
