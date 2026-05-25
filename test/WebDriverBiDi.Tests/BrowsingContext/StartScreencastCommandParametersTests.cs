namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class StartScreencastCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        StartScreencastCommandParameters properties = new("myBrowsingContext");
        Assert.Equal("browsingContext.startScreencast", properties.MethodName);
    }

    [Fact]
    public async Task TestCanSerializeParameters()
    {
        StartScreencastCommandParameters properties = new("myBrowsingContext");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myBrowsingContext", context.Value<string>());
    }

    [Fact]
    public async Task TestCanSerializeParametersWithOptionalValues()
    {
        StartScreencastCommandParameters properties = new("myBrowsingContext")
        {
            MimeType = "video/mpeg4",
            StreamOptions = new MediaStreamOptions()
            {
                Audio = true,
                Video = new MediaTrackConstraints()
                {
                    Height = 480,
                    Width = 640,
                    FrameRate = 30,
                }
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myBrowsingContext", context.Value<string>());

        Assert.True(serialized.ContainsKey("mimeType"));
        JToken? mimeType = serialized["mimeType"];
        Assert.NotNull(mimeType);
        Assert.Equal(JTokenType.String, mimeType.Type);
        Assert.Equal("video/mpeg4", mimeType.Value<string>());

        // MediaStreamOptions has its own serialization tests, so no need to
        // test all permutations here.
        Assert.True(serialized.ContainsKey("streamOptions"));
        JToken? streamOptions = serialized["streamOptions"];
        Assert.NotNull(streamOptions);
        Assert.Equal(JTokenType.Object, streamOptions.Type);
        Assert.Equal(2, streamOptions.Count());
    }
}
