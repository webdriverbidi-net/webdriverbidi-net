namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using WebDriverBiDi.Script;

[TestFixture]
public class RemoteValueDictionaryJsonConverterTests
{
    [Test]
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
        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
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
        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public void TestDeserializingValidEmptyMapRepresentation()
    {
        string json = "[]";
        RemoteValueDictionary? result = JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } });
        Assert.That(result, Has.Count.EqualTo(0));
    }

    [Test]
    public void TestDeserializingNonArrayThrows()
    {
        string json = "\"not-an-array\"";
        Assert.That(() => JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } }), Throws.InstanceOf<JsonException>().With.Message.Contains($"JSON value could not be converted"));
    }

    [Test]
    public void TestDeserializingInvalidMapArrayElementThrows()
    {
        string json = "[\"not-a-remote-value\"]";
        Assert.That(() => JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } }), Throws.InstanceOf<JsonException>().With.Message.Contains($"RemoteValue array element for dictionary must be an array"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } }), Throws.InstanceOf<JsonException>().With.Message.Contains($"RemoteValue array element for dictionary must be an array with exactly two elements"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } }), Throws.InstanceOf<JsonException>().With.Message.Contains($"RemoteValue array element for dictionary must be an array with exactly two elements"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } }), Throws.InstanceOf<JsonException>().With.Message.Contains($"RemoteValue array element for dictionary must have a first element (key) that is either a string or an object"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } }), Throws.InstanceOf<JsonException>().With.Message.Contains($"RemoteValue array element for dictionary must have a second element (value) that is an object"));
    }

    [Test]
    public void TestSerializationThrows()
    {
        string json = "[]";
        RemoteValueDictionary? result = JsonSerializer.Deserialize<RemoteValueDictionary>(json, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } });
        Assert.That(() => JsonSerializer.Serialize(result, new JsonSerializerOptions { Converters = { new RemoteValueDictionaryJsonConverter() } }), Throws.InstanceOf<NotSupportedException>());
    }
}
