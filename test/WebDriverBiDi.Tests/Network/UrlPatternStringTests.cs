namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class UrlPatternStringTests
{
    [Fact]
    public void TestCanSerializeValue()
    {
        UrlPattern value = new UrlPatternString("https://example.com/*");
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("string", type.Value<string>());

        Assert.True(serialized.ContainsKey("pattern"));
        JToken? pattern = serialized["pattern"];
        Assert.NotNull(pattern);
        Assert.Equal(JTokenType.String, pattern.Type);
        Assert.Equal("https://example.com/*", pattern.Value<string>());
    }

    [Fact]
    public void TestCanSerializeValueWithDefaultConstructor()
    {
        UrlPattern value = new UrlPatternString();
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("string", type.Value<string>());

        Assert.True(serialized.ContainsKey("pattern"));
        JToken? pattern = serialized["pattern"];
        Assert.NotNull(pattern);
        Assert.Equal(JTokenType.String, pattern.Type);
        Assert.Equal(string.Empty, pattern.Value<string>());
    }
}
