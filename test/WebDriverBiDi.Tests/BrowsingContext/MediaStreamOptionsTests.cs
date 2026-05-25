namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class MediaStreamOptionsTests
{
    [Fact]
    public async Task TestCanSerialze()
    {
        MediaStreamOptions value = new();
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Empty(parsed);
    }

    [Fact]
    public async Task TestCanSerializeWithOptionalParametersAndAudioTrue()
    {
        MediaStreamOptions value = new()
        {
            Audio = true,
            Video = new MediaTrackConstraints()
            {
                Width = 640,
                Height = 480,
                FrameRate = 30,
            }
        };
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("audio"));
        JToken? audio = parsed["audio"];
        Assert.NotNull(audio);
        Assert.Equal(JTokenType.Boolean, audio.Type);
        Assert.True(audio.Value<bool>());

        // Notg that MediaTrackConstraints has its own serialization tests,
        // so no need to fully test serialization of members here.
        Assert.True(parsed.ContainsKey("video"));
        JToken? video = parsed["video"];
        Assert.NotNull(video);
        Assert.Equal(JTokenType.Object, video.Type);
    }

    [Fact]
    public async Task TestCanSerializeWithOptionalParametersAndAudioFalse()
    {
        MediaStreamOptions value = new()
        {
            Audio = false,
            Video = new MediaTrackConstraints()
            {
                Width = 640,
                Height = 480,
                FrameRate = 30,
            }
        };
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("audio"));
        JToken? audio = parsed["audio"];
        Assert.NotNull(audio);
        Assert.Equal(JTokenType.Boolean, audio.Type);
        Assert.False(audio.Value<bool>());

        // Notg that MediaTrackConstraints has its own serialization tests,
        // so no need to fully test serialization of members here.
        Assert.True(parsed.ContainsKey("video"));
        JToken? video = parsed["video"];
        Assert.NotNull(video);
    } 
}
