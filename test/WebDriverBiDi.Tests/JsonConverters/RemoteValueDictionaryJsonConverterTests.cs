namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using WebDriverBiDi.Script;

public class RemoteValueDictionaryJsonConverterTests
{
    [Fact]
    public void TestDeserializingValidMapRepresentation()
    {
        string json = """
                      [
                        [
                          "stringProperty",
                          {
                            "type": "string",
                            "value": "stringValue"
                          }
                        ],
                        [
                          "numberProperty",
                          {
                            "type": "number",
                            "value": 123
                          }
                        ],
                        [
                          "booleanProperty",
                          {
                            "type": "boolean",
                            "value": true
                          }
                        ]
                      ]
                      """;
        RemoteValueDictionary? result = JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } });

        // Assertions that the elements of the list are deserialized correctly
        // are performed elsewhere.
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void TestDeserializingValidMapWithNonStringKeys()
    {
        string json = """
                      [
                        [
                          {
                            "type": "number",
                            "value": 123
                          },
                          {
                            "type": "string",
                            "value": "stringValue"
                          }
                        ],
                        [
                          {
                            "type": "boolean",
                            "value": true
                          },
                          {
                            "type": "number",
                            "value": 123
                          }
                        ],
                        [
                          {
                            "type": "string",
                            "value": "booleanProperty"
                          },
                          {
                            "type": "boolean",
                            "value": true
                          }
                        ]
                      ]
                      """;
        RemoteValueDictionary? result = JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } });

        // Assertions that the elements of the list are deserialized correctly
        // are performed elsewhere.
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void TestDeserializingValidEmptyMapRepresentation()
    {
        string json = "[]";
        RemoteValueDictionary? result = JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } });
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void TestDeserializingNonArrayThrows()
    {
        string json = "\"not-an-array\"";
        Assert.Contains($"JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } })).Message);
    }

    [Fact]
    public void TestDeserializingInvalidMapArrayElementThrows()
    {
        string json = "[\"not-a-remote-value\"]";
        Assert.Contains($"RemoteValue array element for dictionary must be an array", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } })).Message);
    }

    [Fact]
    public void TestDeserializingMapArrayWithTooLongElementLengthThrows()
    {
        string json = """
                      [
                        [
                          "stringProperty",
                          {
                            "type": "string",
                            "value": "stringValue"
                          },
                          {
                            "type": "number",
                            "value": 123
                          }
                        ]
                      ]
                      """;
        Assert.Contains($"RemoteValue array element for dictionary must be an array with exactly two elements", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } })).Message);
    }

    [Fact]
    public void TestDeserializingMapArrayWithTooShortElementLengthThrows()
    {
        string json = """
                      [
                        [
                          {
                            "type": "string",
                            "value": "stringValue"
                          }
                        ]
                      ]
                      """;
        Assert.Contains($"RemoteValue array element for dictionary must be an array with exactly two elements", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } })).Message);
    }

    [Fact]
    public void TestDeserializingMapArrayWithInvalidKeyElementTypeThrows()
    {
        string json = """
                      [
                        [
                          ["stringProperty"],
                          {
                            "type": "string",
                            "value": "stringValue"
                          }
                        ]
                      ]
                      """;
        Assert.Contains($"RemoteValue array element for dictionary must have a first element (key) that is either a string or an object", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } })).Message);
    }

    [Fact]
    public void TestDeserializingMapArrayWithInvalidValueElementTypeThrows()
    {
        string json = """
                      [
                        [
                          "stringProperty",
                          "stringValue"
                        ]
                      ]
                      """;
        Assert.Contains($"RemoteValue array element for dictionary must have a second element (value) that is an object", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } })).Message);
    }

    [Fact]
    public void TestSerializationThrows()
    {
        string json = "[]";
        RemoteValueDictionary? result = JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } });
        Assert.ThrowsAny<NotSupportedException>(() => JsonSerializer.Serialize(result, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } }));
    }
}
