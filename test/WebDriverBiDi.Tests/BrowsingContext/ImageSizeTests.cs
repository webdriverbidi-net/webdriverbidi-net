namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class ImageSizeTests
{
    [Fact]
    public void TestCanSerialize()
    {
        ImageSize size = new();
        string json = JsonSerializer.Serialize(size);
        JObject parsed = JObject.Parse(json);

        Assert.Empty(parsed);
    }

    [Fact]
    public void TestCanSerializeWithMaxHeight()
    {
        ImageSize size = new()
        {
            MaxHeight = 2,
        };
        string json = JsonSerializer.Serialize(size);
        JObject parsed = JObject.Parse(json);

        Assert.Single(parsed);
        Assert.True(parsed.ContainsKey("maxHeight"));
        JToken? token = parsed["maxHeight"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Integer, token.Type);
        Assert.Equal(2, token.Value<long>());
    }

    [Fact]
    public void TestCanSerializeWithMaxWidth()
    {
        ImageSize size = new()
        {
            MaxWidth = 2,
        };
        string json = JsonSerializer.Serialize(size);
        JObject parsed = JObject.Parse(json);

        Assert.Single(parsed);
        Assert.True(parsed.ContainsKey("maxWidth"));
        JToken? token = parsed["maxWidth"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Integer, token.Type);
        Assert.Equal(2, token.Value<long>());
    }
}
