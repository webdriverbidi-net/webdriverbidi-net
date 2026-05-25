namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class ChannelPropertiesTests
{
    [Fact]
    public void TestCanSerializeChannelProperties()
    {
        ChannelProperties properties = new("myChannel");
        string json = JsonSerializer.Serialize(properties);
        JObject parsed = JObject.Parse(json);

        Assert.Single(parsed);
        Assert.True(parsed.ContainsKey("channel"));
        JToken? channel = parsed["channel"];
        Assert.NotNull(channel);
        Assert.Equal("myChannel", channel.Value<string>());
    }

    [Fact]
    public void TestCanSerializeChannelPropertiesWithOptionalOwnership()
    {
        ChannelProperties properties = new("myChannel")
        {
            Ownership = ResultOwnership.Root
        };
        string json = JsonSerializer.Serialize(properties);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);
        Assert.True(parsed.ContainsKey("channel"));
        JToken? channel = parsed["channel"];
        Assert.NotNull(channel);
        Assert.Equal(JTokenType.String, channel.Type);
        Assert.Equal("myChannel", channel.Value<string>());

        Assert.True(parsed.ContainsKey("ownership"));
        JToken? ownership = parsed["ownership"];
        Assert.NotNull(ownership);
        Assert.Equal(JTokenType.String, ownership.Type);
        Assert.Equal("root", ownership.Value<string>());
    }

    [Fact]
    public void TestCanSerializeChannelPropertiesWithOptionalSerializationOptions()
    {
        // Note that SerializationOptions serialization is tested elsewhere.
        ChannelProperties properties = new("myChannel")
        {
            SerializationOptions = new()
        };
        string json = JsonSerializer.Serialize(properties);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);
        Assert.True(parsed.ContainsKey("channel"));
        JToken? channel = parsed["channel"];
        Assert.NotNull(channel);
        Assert.Equal(JTokenType.String, channel.Type);
        Assert.Equal("myChannel", channel.Value<string>());

        Assert.True(parsed.ContainsKey("serializationOptions"));
        JToken? serializationOptionsToken = parsed["serializationOptions"];
        Assert.NotNull(serializationOptionsToken);
        Assert.Equal(JTokenType.Object, serializationOptionsToken.Type);
        JObject? serializationOptions = serializationOptionsToken.Value<JObject>();
        Assert.NotNull(serializationOptions);
        Assert.Empty(serializationOptions);
    }
}
