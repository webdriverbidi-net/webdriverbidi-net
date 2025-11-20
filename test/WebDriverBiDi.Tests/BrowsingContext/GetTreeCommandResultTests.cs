namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class GetTreeCommandResultTests
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
                        "contexts": [
                          {
                            "context": "myContextId",
                            "clientWindow": "myClientWindow",
                            "url": "http://example.com",
                            "originalOpener": "openerContext",
                            "userContext": "default",
                            "children": []
                          }
                        ]
                      }
                      """;
        GetTreeCommandResult? result = JsonSerializer.Deserialize<GetTreeCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ContextTree, Has.Count.EqualTo(1));
    }

    [Test]
    public void TestCanDeserializeWithNoContexts()
    {
        string json = """
                      {
                        "contexts": []
                      }
                      """;
        GetTreeCommandResult? result = JsonSerializer.Deserialize<GetTreeCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ContextTree, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "contexts": [
                          {
                            "context": "myContextId",
                            "clientWindow": "myClientWindow",
                            "url": "http://example.com",
                            "originalOpener": "openerContext",
                            "userContext": "default",
                            "children": []
                          }
                        ]
                      }
                      """;
        GetTreeCommandResult? result = JsonSerializer.Deserialize<GetTreeCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        GetTreeCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }

    [Test]
    public void TestDeserializingWithMissingContextsThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<GetTreeCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidContextsTypeThrows()
    {
        string json = """
                      {
                        "contexts": "invalid"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<GetTreeCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidContextValueTypeThrows()
    {
        string json = """
                      {
                        "contexts": [ "invalid" ]
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<GetTreeCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
