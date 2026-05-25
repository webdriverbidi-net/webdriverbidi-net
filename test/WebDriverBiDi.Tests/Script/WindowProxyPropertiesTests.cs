namespace WebDriverBiDi.Script;

using System.Text.Json;

public class WindowProxyPropertiesTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "context": "myContextId"
                      }
                      """;
        WindowProxyProperties windowProxyProperties = JsonSerializer.Deserialize<WindowProxyProperties>(json);
        Assert.Equal("myContextId", windowProxyProperties.Context);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "context": "myContextId"
                      }
                      """;
        WindowProxyProperties windowProxyProperties = JsonSerializer.Deserialize<WindowProxyProperties>(json);
        WindowProxyProperties copy = windowProxyProperties;
        Assert.Equal(windowProxyProperties, copy);
    }

    [Fact]
    public void TestDeserializeWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<WindowProxyProperties>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingContextThrows()
    {
        string json = """
                      {
                        "nodeType": 1
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<WindowProxyProperties>(json));
    }
}
