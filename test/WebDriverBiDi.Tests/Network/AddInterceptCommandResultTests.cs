namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class AddInterceptCommandResultTests
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
                        "intercept": "myInterceptId"
                      }
                      """;
        AddInterceptCommandResult? result = JsonSerializer.Deserialize<AddInterceptCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.InterceptId, Is.EqualTo("myInterceptId"));
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "intercept": "myInterceptId"
                      }
                      """;
        AddInterceptCommandResult? result = JsonSerializer.Deserialize<AddInterceptCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        AddInterceptCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }

    [Test]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<AddInterceptCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "intercept": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<AddInterceptCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
