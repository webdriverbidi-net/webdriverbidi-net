namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class NavigateCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        NavigateCommandParameters properties = new("myContextId", "http://example.com");
        Assert.That(properties.MethodName, Is.EqualTo("browsingContext.navigate"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        NavigateCommandParameters properties = new("myContextId", "http://example.com");
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("url"));
            Assert.That(serialized["url"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["url"]!.Value<string>(), Is.EqualTo("http://example.com"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithAcceptWaitNone()
    {
        NavigateCommandParameters properties = new("myContextId", "http://example.com")
        {
            Wait = ReadinessState.None
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("url"));
            Assert.That(serialized["url"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["url"]!.Value<string>(), Is.EqualTo("http://example.com"));
            Assert.That(serialized, Contains.Key("wait"));
            Assert.That(serialized["wait"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["wait"]!.Value<string>(), Is.EqualTo("none"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithAcceptWaitInteractive()
    {
        NavigateCommandParameters properties = new("myContextId", "http://example.com")
        {
            Wait = ReadinessState.Interactive
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("url"));
            Assert.That(serialized["url"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["url"]!.Value<string>(), Is.EqualTo("http://example.com"));
            Assert.That(serialized, Contains.Key("wait"));
            Assert.That(serialized["wait"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["wait"]!.Value<string>(), Is.EqualTo("interactive"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithAcceptWaitComplete()
    {
        NavigateCommandParameters properties = new("myContextId", "http://example.com")
        {
            Wait = ReadinessState.Complete
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("url"));
            Assert.That(serialized["url"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["url"]!.Value<string>(), Is.EqualTo("http://example.com"));
            Assert.That(serialized, Contains.Key("wait"));
            Assert.That(serialized["wait"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["wait"]!.Value<string>(), Is.EqualTo("complete"));
        });
    }
}