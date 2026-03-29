namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using WebDriverBiDi.Script;

[TestFixture]
public class RemoteValueListJsonConverterTests
{
    [Test]
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
        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public void TestDeserializingValidEmptyArray()
    {
        string json = "[]";
        RemoteValueList? result = JsonSerializer.Deserialize<RemoteValueList>(json, new JsonSerializerOptions { Converters = { new RemoteValueListJsonConverter() } });
        Assert.That(result, Has.Count.EqualTo(0));
    }

    [Test]
    public void TestDeserializingInvalidArrayThrows()
    {
        string json = "\"not-an-array\"";
        Assert.That(() => JsonSerializer.Deserialize<RemoteValueList>(json, new JsonSerializerOptions { Converters = { new RemoteValueListJsonConverter() } }), Throws.InstanceOf<JsonException>().With.Message.Contains($"JSON value could not be converted"));
    }

    [Test]
    public void TestDeserializingInvalidArrayElementThrows()
    {
        string json = "[\"not-a-remote-value\"]";
        Assert.That(() => JsonSerializer.Deserialize<RemoteValueList>(json, new JsonSerializerOptions { Converters = { new RemoteValueListJsonConverter() } }), Throws.InstanceOf<JsonException>().With.Message.Contains($"JSON for 'RemoteValue' must be an object"));
    }

    [Test]
    public void TestSerializationThrows()
    {
        string json = "[]";
        RemoteValueList? result = JsonSerializer.Deserialize<RemoteValueList>(json, new JsonSerializerOptions { Converters = { new RemoteValueListJsonConverter() } });
        Assert.That(() => JsonSerializer.Serialize(result, new JsonSerializerOptions { Converters = { new RemoteValueListJsonConverter() } }), Throws.InstanceOf<NotSupportedException>());
    }
}
