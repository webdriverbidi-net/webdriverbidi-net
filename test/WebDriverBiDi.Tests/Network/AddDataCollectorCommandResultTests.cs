namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class AddDataCollectorCommandResultTests
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
                        "collector": "myCollectorId"
                      }
                      """;
        AddDataCollectorCommandResult? result = JsonSerializer.Deserialize<AddDataCollectorCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CollectorId, Is.EqualTo("myCollectorId"));
    }

    [Test]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<AddDataCollectorCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "collector": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<AddDataCollectorCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
