namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class ReloadCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        ReloadCommandParameters properties = new("myContextId");
        Assert.That(properties.MethodName, Is.EqualTo("browsingContext.reload"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        ReloadCommandParameters properties = new("myContextId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithIgnoreCacheTrue()
    {
        ReloadCommandParameters properties = new("myContextId")
        {
            IgnoreCache = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("ignoreCache"));
            Assert.That(serialized["ignoreCache"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["ignoreCache"]!.Value<bool>(), Is.EqualTo(true));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithIgnoreCacheFalse()
    {
        ReloadCommandParameters properties = new("myContextId")
        {
            IgnoreCache = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("ignoreCache"));
            Assert.That(serialized["ignoreCache"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["ignoreCache"]!.Value<bool>(), Is.EqualTo(false));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithAcceptWaitNone()
    {
        ReloadCommandParameters properties = new("myContextId")
        {
            Wait = ReadinessState.None
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("wait"));
            Assert.That(serialized["wait"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["wait"]!.Value<string>(), Is.EqualTo("none"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithAcceptWaitInteractive()
    {
        ReloadCommandParameters properties = new("myContextId")
        {
            Wait = ReadinessState.Interactive
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("wait"));
            Assert.That(serialized["wait"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["wait"]!.Value<string>(), Is.EqualTo("interactive"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithAcceptWaitComplete()
    {
        ReloadCommandParameters properties = new("myContextId")
        {
            Wait = ReadinessState.Complete
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("wait"));
            Assert.That(serialized["wait"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["wait"]!.Value<string>(), Is.EqualTo("complete"));
        });
    }
}
