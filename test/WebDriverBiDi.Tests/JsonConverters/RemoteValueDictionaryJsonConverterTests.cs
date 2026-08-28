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
        Assert.Contains("RemoteValue for dictionary must be an array", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } })).Message);
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

    [Fact]
    public void TestStructurallyEqualObjectKeysRemainDistinctEntries()
    {
        // Two function keys with no handle and no internal ID serialize identically, but each
        // denotes a different JavaScript object; the dictionary must keep both entries.
        string json = """
                      [
                        [
                          { "type": "function" },
                          { "type": "number", "value": 1 }
                        ],
                        [
                          { "type": "function" },
                          { "type": "number", "value": 2 }
                        ]
                      ]
                      """;
        RemoteValueDictionary? result = JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } });
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        List<long> values = [];
        foreach (KeyValuePair<object, RemoteValue> entry in result)
        {
            Assert.IsType<ObjectReferenceRemoteValue>(entry.Key);
            values.Add((long)entry.Value.ConvertTo<NumberRemoteValue>().Value!);
        }

        Assert.Equal([1L, 2L], values.Order());
    }

    [Fact]
    public void TestObjectKeysAreLookedUpByReference()
    {
        string json = """
                      [
                        [
                          { "type": "number", "value": 123 },
                          { "type": "string", "value": "stringValue" }
                        ]
                      ]
                      """;
        JsonSerializerOptions options = new() { Converters = { new RemoteValueDictionaryJsonConverter() } };
        RemoteValueDictionary? result = JsonSerializer.Deserialize<RemoteValueDictionary>(json, options);
        Assert.NotNull(result);

        // The key instance obtained from the dictionary itself is found.
        object key = Assert.Single(result.Keys);
        Assert.True(result.ContainsKey(key));
        Assert.Equal("stringValue", result[key].ConvertTo<StringRemoteValue>().Value);

        // A separately deserialized, structurally equal key is a different remote object and is not found.
        RemoteValueDictionary? other = JsonSerializer.Deserialize<RemoteValueDictionary>(json, options);
        Assert.NotNull(other);
        object equalButDistinctKey = Assert.Single(other.Keys);
        Assert.Equal(key, equalButDistinctKey);
        Assert.False(result.ContainsKey(equalButDistinctKey));
    }

    [Fact]
    public void TestStringKeysAreLookedUpByValue()
    {
        string json = """
                      [
                        [
                          "name",
                          { "type": "string", "value": "stringValue" }
                        ]
                      ]
                      """;
        RemoteValueDictionary? result = JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } });
        Assert.NotNull(result);
        Assert.True(result.ContainsKey(string.Concat("na", "me")));
        Assert.False(result.ContainsKey(new object()));
        Assert.Equal("stringValue", result["name"].ConvertTo<StringRemoteValue>().Value);
    }
}
