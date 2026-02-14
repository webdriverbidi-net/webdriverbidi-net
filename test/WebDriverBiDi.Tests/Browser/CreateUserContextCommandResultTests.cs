namespace WebDriverBiDi.Browser;

using System.Text.Json;

[TestFixture]
public class CreateUserContextCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "userContext": "myUserContext"
                      }
                      """;
        CreateUserContextCommandResult? result = JsonSerializer.Deserialize<CreateUserContextCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserContextId, Is.EqualTo("myUserContext"));
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "userContext": "myUserContext"
                      }
                      """;
        CreateUserContextCommandResult? result = JsonSerializer.Deserialize<CreateUserContextCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        CreateUserContextCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }

    [Test]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<CreateUserContextCommandResult>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "userContext": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<CreateUserContextCommandResult>(json), Throws.InstanceOf<JsonException>());
    }
}
