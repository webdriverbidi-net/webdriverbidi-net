namespace WebDriverBiDi.Storage;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetCookieCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "partitionKey": {
                          "userContext": "myUserContext",
                          "sourceOrigin": "mySourceOrigin",
                          "extraPropertyName": "extraPropertyValue"
                        }
                      }
                      """;
        SetCookieCommandResult? result = JsonSerializer.Deserialize<SetCookieCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.PartitionKey, Is.Not.Null);
            Assert.That(result.PartitionKey.UserContextId, Is.EqualTo("myUserContext"));
            Assert.That(result.PartitionKey.SourceOrigin, Is.EqualTo("mySourceOrigin"));
            Assert.That(result.PartitionKey.AdditionalData, Has.Count.EqualTo(1));
            Assert.That(result.PartitionKey.AdditionalData, Contains.Key("extraPropertyName"));
            Assert.That(result.PartitionKey.AdditionalData["extraPropertyName"], Is.EqualTo("extraPropertyValue"));
        });
    }

    [Test]
    public void TestCanDeserializeWithMissingData()
    {
        string json = """
                      {
                        "partitionKey": {}
                      }
                      """;
        SetCookieCommandResult? result = JsonSerializer.Deserialize<SetCookieCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.PartitionKey, Is.Not.Null);
            Assert.That(result.PartitionKey.UserContextId, Is.Null);
            Assert.That(result.PartitionKey.SourceOrigin, Is.Null);
            Assert.That(result.PartitionKey.AdditionalData, Is.Empty);
        });
    }

    [Test]
    public void TestCanDeserializingWithMissingPartition()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<SetCookieCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "partitionKey": {
                          "userContext": "myUserContext",
                          "sourceOrigin": "mySourceOrigin",
                          "extraPropertyName": "extraPropertyValue"
                        }
                      }
                      """;
        SetCookieCommandResult? result = JsonSerializer.Deserialize<SetCookieCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SetCookieCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }

    [Test]
    public void TestDeserializingWithInvalidPartitionDataTypeThrows()
    {
        string json = """
                      {
                        "partitionKey": "invalidPartitionType"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<SetCookieCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
