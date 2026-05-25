namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class ChannelValueTests
{
    [Fact]
    public void TestCanSerializeChannelValue()
    {
        // Note that serialization of ChannelProperties (value property) is tested elsewhere.
        ChannelValue value = new(new ChannelProperties("myChannel"));
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("channel", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Object, parsedValue.Type);
    }

    [Fact]
    public void TestCopySemantics()
    {
        // Note that serialization of ChannelProperties (value property) is tested elsewhere.
        ChannelValue value = new(new ChannelProperties("myChannel"));
        ChannelValue copy = value with { };
        Assert.Equal(value, copy);
    }
}
