namespace WebDriverBiDi.Browser;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class GetUserContextsCommandResultTests
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
        GetUserContextsCommandResult? result = JsonSerializer.Deserialize<GetUserContextsCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.UserContexts, Has.Count.EqualTo(2));
            Assert.That(result.UserContexts[0].UserContextId, Is.EqualTo("default"));
            Assert.That(result.UserContexts[1].UserContextId, Is.EqualTo("myUserContext"));
        });

    }

    [Test]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<CreateUserContextCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "userContexts": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<CreateUserContextCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
