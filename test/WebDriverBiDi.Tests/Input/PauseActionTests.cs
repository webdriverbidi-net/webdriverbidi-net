namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class PauseActionTests
{
    [Fact]
    public void TestCanSerializeParameters()
    {
        PauseAction properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pause", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalDuration()
    {
        PauseAction properties = new()
        {
            Duration = TimeSpan.FromMilliseconds(1),
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pause", type.Value<string>());

        Assert.True(serialized.ContainsKey("duration"));
        JToken? duration = serialized["duration"];
        Assert.NotNull(duration);
        Assert.Equal(JTokenType.Integer, duration.Type);
        Assert.Equal(1L, duration.Value<long>());
    }
}
