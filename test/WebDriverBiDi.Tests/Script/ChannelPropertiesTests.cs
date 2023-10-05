namespace WebDriverBiDi.Script;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class ChannelPropertiesTests
{
    [Test]
    public void TestCanSerializeChannelProperties()
    {
        ChannelProperties properties = new("myChannel");
        string json = JsonConvert.SerializeObject(properties);
        var parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(1));
            Assert.That(parsed, Contains.Key("channel"));
            Assert.That(parsed["channel"]!.Value<string>(), Is.EqualTo("myChannel"));
        });
    }

    [Test]
    public void TestCanSerializeChannelPropertiesWithOptionalResultOwnership()
    {
        ChannelProperties properties = new("myChannel")
        {
            ResultOwnership = ResultOwnership.Root
        };
        string json = JsonConvert.SerializeObject(properties);
        var parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("channel"));
            Assert.That(parsed["channel"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["channel"]!.Value<string>(), Is.EqualTo("myChannel"));
            Assert.That(parsed, Contains.Key("resultOwnership"));
            Assert.That(parsed["resultOwnership"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["resultOwnership"]!.Value<string>(), Is.EqualTo("root"));
        });
    }

    [Test]
    public void TestCanSerializeChannelPropertiesWithOptionalSerializationOptions()
    {
        // Note that SerializationOptions serialization is tested elsewhere.
        ChannelProperties properties = new("myChannel")
        {
            SerializationOptions = new()
        };
        string json = JsonConvert.SerializeObject(properties);
        var parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        { 
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("channel"));
            Assert.That(parsed["channel"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["channel"]!.Value<string>(), Is.EqualTo("myChannel"));
            Assert.That(parsed, Contains.Key("serializationOptions"));
            Assert.That(parsed["serializationOptions"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(parsed["serializationOptions"]!.Value<JObject>(), Is.Empty);
        });
    }
}
