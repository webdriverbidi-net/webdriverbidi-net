namespace WebDriverBiDi.Script;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class WindowProxyPropertiesTests
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
        WindowProxyProperties? windowProxyProperties = JsonSerializer.Deserialize<WindowProxyProperties>(json, deserializationOptions);
        Assert.That(windowProxyProperties, Is.Not.Null);
        Assert.That(windowProxyProperties!.Context, Is.EqualTo("myContextId"));
    }

    [Test]
    public void TestDeserializeWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<WindowProxyProperties>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithMissingContextThrows()
    {
        string json = """
                      {
                        "nodeType": 1
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<WindowProxyProperties>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
