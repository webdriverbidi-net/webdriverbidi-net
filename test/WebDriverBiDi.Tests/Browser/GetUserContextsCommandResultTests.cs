namespace WebDriverBiDi.Browser;

using System.Text.Json;

[TestFixture]
public class GetUserContextsCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "userContexts": [
                          {
                            "userContext": "default"
                          },
                          {
                            "userContext": "myUserContext"
                          }
                        ]
                      }
                      """;
        GetUserContextsCommandResult? result = JsonSerializer.Deserialize<GetUserContextsCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.UserContexts, Has.Count.EqualTo(2));
            Assert.That(result.UserContexts[0].UserContextId, Is.EqualTo("default"));
            Assert.That(result.UserContexts[1].UserContextId, Is.EqualTo("myUserContext"));
        }
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "userContexts": [
                          {
                            "userContext": "default"
                          },
                          {
                            "userContext": "myUserContext"
                          }
                        ]
                      }
                      """;
        GetUserContextsCommandResult? result = JsonSerializer.Deserialize<GetUserContextsCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        GetUserContextsCommandResult copy = result with { };
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
                        "userContexts": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<CreateUserContextCommandResult>(json), Throws.InstanceOf<JsonException>());
    }
}
