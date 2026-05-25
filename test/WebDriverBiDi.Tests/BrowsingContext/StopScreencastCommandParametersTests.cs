namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class StopScreenshotCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        StopScreencastCommandParameters properties = new("myScreencastId");
        Assert.Equal("browsingContext.stopScreencast", properties.MethodName);
    }

    [Fact]
    public async Task TestCanSerializeParameters()
    {
        StopScreencastCommandParameters properties = new("myScreencastId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("screencast"));
        JToken? screencast = serialized["screencast"];
        Assert.NotNull(screencast);
        Assert.Equal(JTokenType.String, screencast.Type);
        Assert.Equal("myScreencastId", screencast.Value<string>());
    }
}