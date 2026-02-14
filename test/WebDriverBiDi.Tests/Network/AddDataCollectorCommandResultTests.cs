namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class AddDataCollectorCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "collector": "myCollectorId"
                      }
                      """;
        AddDataCollectorCommandResult? result = JsonSerializer.Deserialize<AddDataCollectorCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CollectorId, Is.EqualTo("myCollectorId"));
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "collector": "myCollectorId"
                      }
                      """;
        AddDataCollectorCommandResult? result = JsonSerializer.Deserialize<AddDataCollectorCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        AddDataCollectorCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }

    [Test]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<AddDataCollectorCommandResult>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "collector": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<AddDataCollectorCommandResult>(json), Throws.InstanceOf<JsonException>());
    }
}
