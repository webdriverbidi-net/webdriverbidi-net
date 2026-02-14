namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class GetDataCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "bytes": {
                          "type": "string",
                          "value": "myNetworkData"
                        }
                      }
                      """;
        GetDataCommandResult? result = JsonSerializer.Deserialize<GetDataCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Bytes.Type, Is.EqualTo(BytesValueType.String));
        Assert.That(result.Bytes.Value, Is.EqualTo("myNetworkData"));
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "bytes": {
                          "type": "string",
                          "value": "myNetworkData"
                        }
                      }
                      """;
        GetDataCommandResult? result = JsonSerializer.Deserialize<GetDataCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        GetDataCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }

    [Test]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<GetDataCommandResult>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "bytes": "invalidValue"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<GetDataCommandResult>(json), Throws.InstanceOf<JsonException>());
    }
}
