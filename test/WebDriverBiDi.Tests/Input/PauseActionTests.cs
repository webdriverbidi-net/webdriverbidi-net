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

    [Fact]
    public void TestSettingNegativeDurationThrows()
    {
        // The protocol transmits duration as an unsigned integer, so a negative TimeSpan cannot be
        // represented on the wire. The setter rejects it with a message naming the property rather
        // than letting an OverflowException surface later from inside serialization.
        PauseAction properties = new();
        Assert.Contains("Duration must not be negative", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.Duration = TimeSpan.FromMilliseconds(-1)).Message);

        // A null and a non-negative value are both accepted, exercising the setter's other paths.
        properties.Duration = null;
        Assert.Null(properties.Duration);
        properties.Duration = TimeSpan.FromMilliseconds(1);
        Assert.Equal(TimeSpan.FromMilliseconds(1), properties.Duration);
    }
}
