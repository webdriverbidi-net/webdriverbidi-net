namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class CreateCommandResultTests
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
                        "context": "myContextId"
                      }
                      """;
        CreateCommandResult? result = JsonSerializer.Deserialize<CreateCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.BrowsingContextId, Is.EqualTo("myContextId"));
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "context": "myContextId"
                      }
                      """;
        CreateCommandResult? result = JsonSerializer.Deserialize<CreateCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        CreateCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }

    [Test]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<CreateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<CreateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
