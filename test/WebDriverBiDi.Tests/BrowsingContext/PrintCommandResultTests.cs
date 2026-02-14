namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class PrintCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "data": "some print data"
                      }
                      """;
        PrintCommandResult? result = JsonSerializer.Deserialize<PrintCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data, Is.EqualTo("some print data"));
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "data": "some print data"
                      }
                      """;
        PrintCommandResult? result = JsonSerializer.Deserialize<PrintCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        PrintCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }

    [Test]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<PrintCommandResult>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "data": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<PrintCommandResult>(json), Throws.InstanceOf<JsonException>());
    }
}
