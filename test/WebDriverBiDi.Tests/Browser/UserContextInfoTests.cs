namespace WebDriverBiDi.Browser;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class UserContextInfoTests
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
                        "userContext": "default"
                      }
                      """;
        UserContextInfo? result = JsonSerializer.Deserialize<UserContextInfo>(json, deserializationOptions);
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
        UserContextInfo? result = JsonSerializer.Deserialize<UserContextInfo>(json, deserializationOptions);
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
        Assert.That(() => JsonSerializer.Deserialize<UserContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithIncorrectUserContextTypeThrows()
    {
        string json = """
                      {
                        "userContext": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<UserContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
