namespace WebDriverBiDi.Session;

using System.Text.Json;

[TestFixture]
public class StatusCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "ready": true,
                        "message": "myMessage"
                      }
                      """;
        StatusCommandResult? result = JsonSerializer.Deserialize<StatusCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsReady, Is.EqualTo(true));
            Assert.That(result.Message, Is.EqualTo("myMessage"));
        }
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "ready": true,
                        "message": "myMessage"
                      }
                      """;
        StatusCommandResult? result = JsonSerializer.Deserialize<StatusCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        StatusCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }

    [Test]
    public void TestDeserializingWithMissingReadyThrows()
    {
        string json = """
                      {
                        "message": "myMessage"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<StatusCommandResult>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidReadyTypeThrows()
    {
        string json = """
                      {
                        "ready": "invalid value",
                        "message": "myMessage"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<StatusCommandResult>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingMessageThrows()
    {
        string json = """
                      {
                        "ready": true
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<StatusCommandResult>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidMessageTypeThrows()
    {
        string json = """
                      {
                        "ready": true,
                        "message": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<StatusCommandResult>(json), Throws.InstanceOf<JsonException>());
    }
}
