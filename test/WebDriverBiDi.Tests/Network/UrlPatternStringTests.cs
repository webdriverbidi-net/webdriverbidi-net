namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class UrlPatternStringTests
{
    [Test]
    public void TestCanSerializeValue()
    {
        UrlPattern value = new UrlPatternString("https://example.com/*");
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(serialized, Contains.Key("pattern"));
            Assert.That(serialized["pattern"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["pattern"]!.Value<string>(), Is.EqualTo("https://example.com/*"));
        }
    }

    [Test]
    public void TestCanSerializeValueWithDefaultConstructor()
    {
        UrlPattern value = new UrlPatternString();
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(serialized, Contains.Key("pattern"));
            Assert.That(serialized["pattern"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["pattern"]!.Value<string>(), Is.EqualTo(string.Empty));
        }
    }
}
