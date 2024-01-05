namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class UrlPatternPatternTests
{
    [Test]
    public void TestCanSerializeValue()
    {
        UrlPattern value = new UrlPatternPattern();
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pattern"));
        });
    }

    [Test]
    public void TestCanSerializeValueWithProtocol()
    {
        UrlPattern value = new UrlPatternPattern()
        {
            Protocol = "https"
        };
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pattern"));
            Assert.That(serialized, Contains.Key("protocol"));
            Assert.That(serialized["protocol"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["protocol"]!.Value<string>(), Is.EqualTo("https"));
        });
    }

    [Test]
    public void TestCanSerializeValueWithHostName()
    {
        UrlPattern value = new UrlPatternPattern()
        {
            HostName = "example.com"
        };
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pattern"));
            Assert.That(serialized, Contains.Key("hostname"));
            Assert.That(serialized["hostname"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["hostname"]!.Value<string>(), Is.EqualTo("example.com"));
        });
    }

    [Test]
    public void TestCanSerializeValueWithPort()
    {
        UrlPattern value = new UrlPatternPattern()
        {
            Port = "2222"
        };
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pattern"));
            Assert.That(serialized, Contains.Key("port"));
            Assert.That(serialized["port"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["port"]!.Value<string>(), Is.EqualTo("2222"));
        });
    }

    [Test]
    public void TestCanSerializeValueWithPathName()
    {
        UrlPattern value = new UrlPatternPattern()
        {
            PathName = "/data/*"
        };
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pattern"));
            Assert.That(serialized, Contains.Key("pathname"));
            Assert.That(serialized["pathname"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["pathname"]!.Value<string>(), Is.EqualTo("/data/*"));
        });
    }

    [Test]
    public void TestCanSerializeValueWithSearch()
    {
        UrlPattern value = new UrlPatternPattern()
        {
            Search = "?user=foo"
        };
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pattern"));
            Assert.That(serialized, Contains.Key("search"));
            Assert.That(serialized["search"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["search"]!.Value<string>(), Is.EqualTo("?user=foo"));
        });
    }
}
