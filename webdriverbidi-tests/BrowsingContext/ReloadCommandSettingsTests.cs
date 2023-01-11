namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class ReloadCommandSettingsTests
{
    [Test]
    public void TestCanSerializeSettings()
    {
        ReloadCommandSettings properties = new("myContextId");
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithIgnoreCacheTrue()
    {
        ReloadCommandSettings properties = new("myContextId")
        {
            IgnoreCache = true
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized.ContainsKey("ignoreCache"));
            Assert.That(serialized["ignoreCache"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["ignoreCache"]!.Value<bool>(), Is.EqualTo(true));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithIgnoreCacheFalse()
    {
        ReloadCommandSettings properties = new("myContextId")
        {
            IgnoreCache = false
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized.ContainsKey("ignoreCache"));
            Assert.That(serialized["ignoreCache"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["ignoreCache"]!.Value<bool>(), Is.EqualTo(false));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithAcceptWaitNone()
    {
        ReloadCommandSettings properties = new("myContextId")
        {
            Wait = ReadinessState.None
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized.ContainsKey("wait"));
            Assert.That(serialized["wait"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["wait"]!.Value<string>(), Is.EqualTo("none"));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithAcceptWaitInteractive()
    {
        ReloadCommandSettings properties = new("myContextId")
        {
            Wait = ReadinessState.Interactive
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized.ContainsKey("wait"));
            Assert.That(serialized["wait"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["wait"]!.Value<string>(), Is.EqualTo("interactive"));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithAcceptWaitComplete()
    {
        ReloadCommandSettings properties = new("myContextId")
        {
            Wait = ReadinessState.Complete
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized.ContainsKey("wait"));
            Assert.That(serialized["wait"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["wait"]!.Value<string>(), Is.EqualTo("complete"));
        });
    }
}