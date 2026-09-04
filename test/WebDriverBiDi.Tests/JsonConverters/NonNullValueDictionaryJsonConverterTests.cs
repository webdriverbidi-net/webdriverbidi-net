namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;

public class NonNullValueDictionaryJsonConverterTests
{
    private static JsonSerializerOptions Options()
    {
        return new JsonSerializerOptions { Converters = { new NonNullValueDictionaryJsonConverter<string>() } };
    }

    [Fact]
    public void TestDeserializingValidObject()
    {
        string json = """{ "id": "main", "class": "container" }""";
        Dictionary<string, string>? result = JsonSerializer.Deserialize<Dictionary<string, string>>(json, Options());
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("main", result["id"]);
        Assert.Equal("container", result["class"]);
    }

    [Fact]
    public void TestDeserializingValidEmptyObject()
    {
        Dictionary<string, string>? result = JsonSerializer.Deserialize<Dictionary<string, string>>("{}", Options());
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void TestDeserializingObjectWithNullValueThrows()
    {
        string json = """{ "id": "main", "class": null }""";
        JsonException exception = Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Dictionary<string, string>>(json, Options()));
        Assert.Contains("may not be null", exception.Message);
        Assert.Contains("class", exception.Message);
    }

    [Fact]
    public void TestDeserializingNonObjectThrows()
    {
        JsonException exception = Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Dictionary<string, string>>("[]", Options()));
        Assert.Contains("must be an object", exception.Message);
        Assert.Contains("String", exception.Message);
    }

    [Fact]
    public void TestSerializationThrows()
    {
        // Every map the specification gives a non-nullable value type belongs to a payload the library
        // only receives, so this converter is inbound-only, as the library's other read-only converters
        // are. Nothing on the outbound graph carries it.
        Dictionary<string, string> attributes = new() { ["id"] = "main" };
        Assert.ThrowsAny<NotSupportedException>(() => JsonSerializer.Serialize(attributes, Options()));
    }

    [Fact]
    public void TestDeserializingNullDictionaryIsHandledByTheSerializer()
    {
        Assert.Null(JsonSerializer.Deserialize<Dictionary<string, string>>("null", Options()));
    }
}
