namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class CreateCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "context": "myContextId"
                      }
                      """;
        CreateCommandResult? result = JsonSerializer.Deserialize<CreateCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.BrowsingContextId, Is.EqualTo("myContextId"));
    }

    [Test]
    public void TestCanDeserializeWithUserContext()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "userContext": "myUserContextId"
                      }
                      """;
        CreateCommandResult? result = JsonSerializer.Deserialize<CreateCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.BrowsingContextId, Is.EqualTo("myContextId"));
        Assert.That(result.UserContextId, Is.EqualTo("myUserContextId"));
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "context": "myContextId"
                      }
                      """;
        CreateCommandResult? result = JsonSerializer.Deserialize<CreateCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        CreateCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }

    [Test]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<CreateCommandResult>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<CreateCommandResult>(json), Throws.InstanceOf<JsonException>());
    }
}
