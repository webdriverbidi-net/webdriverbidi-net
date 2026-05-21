namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using WebDriverBiDi.Script;

public class RemoteValueListJsonConverterTests
{
    [Fact]
    public void TestDeserializingValidArray()
    {
        string json = """
                      [
                        {
                        "type": "string",
                        "value": "stringValue"
                        },
                        {
                        "type": "number",
                        "value": 123
                        },
                        {
                        "type": "boolean",
                        "value": true
                        }
                      ]
                      """;
        RemoteValueList? result = JsonSerializer.Deserialize<RemoteValueList>(json, new JsonSerializerOptions { Converters = { new RemoteValueListJsonConverter() } });

        // Assertions that the elements of the list are deserialized correctly
        // are performed elsewhere.
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void TestDeserializingValidEmptyArray()
    {
        string json = "[]";
        RemoteValueList? result = JsonSerializer.Deserialize<RemoteValueList>(json, new JsonSerializerOptions { Converters = { new RemoteValueListJsonConverter() } });
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void TestDeserializingInvalidArrayThrows()
    {
        string json = "\"not-an-array\"";
        Assert.Contains($"JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValueList>(json, new JsonSerializerOptions { Converters = { new RemoteValueListJsonConverter() } })).Message);
    }

    [Fact]
    public void TestDeserializingInvalidArrayElementThrows()
    {
        string json = "[\"not-a-remote-value\"]";
        Assert.Contains($"JSON for 'RemoteValue' must be an object", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValueList>(json, new JsonSerializerOptions { Converters = { new RemoteValueListJsonConverter() } })).Message);
    }

    [Fact]
    public void TestSerializationThrows()
    {
        string json = "[]";
        RemoteValueList? result = JsonSerializer.Deserialize<RemoteValueList>(json, new JsonSerializerOptions { Converters = { new RemoteValueListJsonConverter() } });
        Assert.ThrowsAny<NotSupportedException>(() => JsonSerializer.Serialize(result, new JsonSerializerOptions { Converters = { new RemoteValueListJsonConverter() } }));
    }
}
