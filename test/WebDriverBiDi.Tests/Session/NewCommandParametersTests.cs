namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class NewCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        NewCommandParameters properties = new();
        Assert.Equal("session.new", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        NewCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.True(serialized.ContainsKey("capabilities"));
        JObject? capabilities = serialized["capabilities"] as JObject;
        Assert.NotNull(capabilities);
        Assert.Empty(capabilities);
    }

    [Fact]
    public void TestCanSerializeWithAlwaysMatch()
    {
        NewCommandParameters properties = new()
        {
            Capabilities =
            {
                AlwaysMatch = new CapabilityRequest() { BrowserName = "greatBrowser" }
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("capabilities"));
        JObject? capabilities = serialized["capabilities"] as JObject;
        Assert.NotNull(capabilities);
        Assert.True(capabilities.ContainsKey("alwaysMatch"));
        JToken? alwaysMatchToken = capabilities["alwaysMatch"];
        Assert.NotNull(alwaysMatchToken);
        Assert.Equal(JTokenType.Object, alwaysMatchToken.Type);

        JObject? alwaysMatch = alwaysMatchToken as JObject;
        Assert.NotNull(alwaysMatch);
        Assert.Single(alwaysMatch);

        Assert.True(alwaysMatch.ContainsKey("browserName"));
        JToken? browserName = alwaysMatch["browserName"];
        Assert.NotNull(browserName);
        Assert.Equal("greatBrowser", browserName.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithFirstMatch()
    {
        NewCommandParameters properties = new()
        {
            Capabilities =
            {
                FirstMatch = [new CapabilityRequest() { BrowserName = "greatBrowser" }]
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("capabilities"));
        JObject? capabilities = serialized["capabilities"] as JObject;
        Assert.NotNull(capabilities);
        Assert.True(capabilities.ContainsKey("firstMatch"));
        JToken? firstMatchToken = capabilities["firstMatch"];
        Assert.NotNull(firstMatchToken);
        Assert.Equal(JTokenType.Array, firstMatchToken.Type);

        JArray? firstMatch = firstMatchToken as JArray;
        Assert.NotNull(firstMatch);
        Assert.Single(firstMatch);
        Assert.Equal(JTokenType.Object, firstMatch[0].Type);

        JObject? firstMatchElement = firstMatch[0] as JObject;
        Assert.NotNull(firstMatchElement);
        Assert.Single(firstMatchElement);
        Assert.True(firstMatchElement.ContainsKey("browserName"));
        JToken? browserName = firstMatchElement["browserName"];
        Assert.NotNull(browserName);
        Assert.Equal("greatBrowser", browserName.Value<string>());
    }
}
