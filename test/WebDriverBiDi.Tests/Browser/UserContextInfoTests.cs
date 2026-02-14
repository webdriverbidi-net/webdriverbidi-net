namespace WebDriverBiDi.Browser;

using System.Text.Json;

[TestFixture]
public class UserContextInfoTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "userContext": "default"
                      }
                      """;
        UserContextInfo? result = JsonSerializer.Deserialize<UserContextInfo>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserContextId, Is.EqualTo("default"));
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "userContext": "default"
                      }
                      """;
        UserContextInfo? result = JsonSerializer.Deserialize<UserContextInfo>(json);
        Assert.That(result, Is.Not.Null);
        UserContextInfo copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }

    [Test]
    public void TestDeserializingWithMissingUserContextThrows()
    {
        string json = """
                      {
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<UserContextInfo>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithIncorrectUserContextTypeThrows()
    {
        string json = """
                      {
                        "userContext": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<UserContextInfo>(json), Throws.InstanceOf<JsonException>());
    }
}
