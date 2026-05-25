namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class MediaTrackConstraintsTests
{
    [Fact]
    public async Task TestCanSerialize()
    {
        MediaTrackConstraints value = new();
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Empty(parsed);
    }

    [Fact]
    public async Task TestCanSerializeWithOptionalParameters()
    {
        MediaTrackConstraints value = new()
        {
            Width = 640,
            Height = 480,
            FrameRate = 30,
        };
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(3, parsed.Count);

        Assert.True(parsed.ContainsKey("width"));
        JToken? width = parsed["width"];
        Assert.NotNull(width);
        Assert.Equal(JTokenType.Integer, width.Type);
        Assert.Equal(640ul, width.Value<ulong>());

        Assert.True(parsed.ContainsKey("height"));
        JToken? height = parsed["height"];
        Assert.NotNull(height);
        Assert.Equal(JTokenType.Integer, height.Type);
        Assert.Equal(480ul, height.Value<ulong>());

        Assert.True(parsed.ContainsKey("frameRate"));
        JToken? frameRate = parsed["frameRate"];
        Assert.NotNull(frameRate);
        Assert.Equal(JTokenType.Integer, frameRate.Type);
        Assert.Equal(30ul, frameRate.Value<ulong>());
    }
}
