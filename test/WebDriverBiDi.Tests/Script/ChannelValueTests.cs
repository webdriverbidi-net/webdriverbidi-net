namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class ChannelValueTests
{
    [Test]
    public void TestCanSerializeChannelValue()
    {
        // Note that serialization of ChannelProperties (value property) is tested elsewhere.
        ChannelValue value = new(new ChannelProperties("myChannel"));
        string json = JsonSerializer.Serialize(value);
        var parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("channel"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Object));
        });
    }
}
