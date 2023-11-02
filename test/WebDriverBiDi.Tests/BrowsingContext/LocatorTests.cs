namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class LocatorTests
{
    [Test]
    public void TestCanSerializeCssLocator()
    {
        CssLocator value = new(".selector");
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("css"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo(".selector"));
        });
    }

    [Test]
    public void TestCanSerializeXPathLocator()
    {
        XPathLocator value = new("//selector");
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("xpath"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("//selector"));
        });
    }

    [Test]
    public void TestCanSerializeInnerTextLocator()
    {
        InnerTextLocator value = new("text to locate");
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("innerText"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("text to locate"));
        });
    }

    [Test]
    public void TestCanSerializeInnerTextLocatorWithMaxDepthZero()
    {
        InnerTextLocator value = new("text to locate")
        {
            MaxDepth = 0
        };
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(3));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("innerText"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("text to locate"));
            Assert.That(parsed, Contains.Key("maxDepth"));
            Assert.That(parsed["maxDepth"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(parsed["maxDepth"]!.Value<ulong>(), Is.EqualTo(0));
        });
    }

    [Test]
    public void TestCanSerializeInnerTextLocatorWithMaxDepthNonZero()
    {
        InnerTextLocator value = new("text to locate")
        {
            MaxDepth = 10
        };
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(3));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("innerText"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("text to locate"));
            Assert.That(parsed, Contains.Key("maxDepth"));
            Assert.That(parsed["maxDepth"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(parsed["maxDepth"]!.Value<ulong>(), Is.EqualTo(10));
        });
    }

    [Test]
    public void TestCanSerializeInnerTextLocatorWithIgnoreCaseTrue()
    {
        InnerTextLocator value = new("text to locate")
        {
            IgnoreCase = true
        };
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(3));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("innerText"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("text to locate"));
            Assert.That(parsed, Contains.Key("ignoreCase"));
            Assert.That(parsed["ignoreCase"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(parsed["ignoreCase"]!.Value<bool>(), Is.True);
        });
    }

    [Test]
    public void TestCanSerializeInnerTextLocatorWithIgnoreCaseFalse()
    {
        InnerTextLocator value = new("text to locate")
        {
            IgnoreCase = false
        };
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(3));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("innerText"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("text to locate"));
            Assert.That(parsed, Contains.Key("ignoreCase"));
            Assert.That(parsed["ignoreCase"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(parsed["ignoreCase"]!.Value<bool>(), Is.False);
        });
    }

    [Test]
    public void TestCanSerializeInnerTextLocatorWithMatchTypeFull()
    {
        InnerTextLocator value = new("text to locate")
        {
            MatchType = InnerTextMatchType.Full
        };
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(3));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("innerText"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("text to locate"));
            Assert.That(parsed, Contains.Key("matchType"));
            Assert.That(parsed["matchType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["matchType"]!.Value<string>(), Is.EqualTo("full"));
        });
    }

    [Test]
    public void TestCanSerializeInnerTextLocatorWithMatchTypePartial()
    {
        InnerTextLocator value = new("text to locate")
        {
            MatchType = InnerTextMatchType.Partial
        };
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(3));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("innerText"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("text to locate"));
            Assert.That(parsed, Contains.Key("matchType"));
            Assert.That(parsed["matchType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["matchType"]!.Value<string>(), Is.EqualTo("partial"));
        });
    }
}
