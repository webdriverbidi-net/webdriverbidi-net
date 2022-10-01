namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class ReloadCommandPropertiesTests
{
    [Test]
    public void TestCanSerializeProperties()
    {
        var properties = new ReloadCommandProperties("myContextId");
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("context"));
        Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
    }

    [Test]
    public void TestCanSerializePropertiesWithIgnoreCacheTrue()
    {
        var properties = new ReloadCommandProperties("myContextId");
        properties.IgnoreCache = true;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(2));
        Assert.That(serialized.ContainsKey("context"));
        Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        Assert.That(serialized.ContainsKey("ignoreCache"));
        Assert.That(serialized["ignoreCache"]!.Type, Is.EqualTo(JTokenType.Boolean));
        Assert.That(serialized["ignoreCache"]!.Value<bool>(), Is.EqualTo(true));
    }

    [Test]
    public void TestCanSerializePropertiesWithIgnoreCacheFalse()
    {
        var properties = new ReloadCommandProperties("myContextId");
        properties.IgnoreCache = false;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(2));
        Assert.That(serialized.ContainsKey("context"));
        Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        Assert.That(serialized.ContainsKey("ignoreCache"));
        Assert.That(serialized["ignoreCache"]!.Type, Is.EqualTo(JTokenType.Boolean));
        Assert.That(serialized["ignoreCache"]!.Value<bool>(), Is.EqualTo(false));
    }

    [Test]
    public void TestCanSerializePropertiesWithAcceptWaitNone()
    {
        var properties = new ReloadCommandProperties("myContextId");
        properties.Wait = ReadinessState.None;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(2));
        Assert.That(serialized.ContainsKey("context"));
        Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        Assert.That(serialized.ContainsKey("wait"));
        Assert.That(serialized["wait"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["wait"]!.Value<string>(), Is.EqualTo("none"));
    }

    [Test]
    public void TestCanSerializePropertiesWithAcceptWaitInteractive()
    {
        var properties = new ReloadCommandProperties("myContextId");
        properties.Wait = ReadinessState.Interactive;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(2));
        Assert.That(serialized.ContainsKey("context"));
        Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        Assert.That(serialized.ContainsKey("wait"));
        Assert.That(serialized["wait"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["wait"]!.Value<string>(), Is.EqualTo("interactive"));
    }

    [Test]
    public void TestCanSerializePropertiesWithAcceptWaitComplete()
    {
        var properties = new ReloadCommandProperties("myContextId");
        properties.Wait = ReadinessState.Complete;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(2));
        Assert.That(serialized.ContainsKey("context"));
        Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        Assert.That(serialized.ContainsKey("wait"));
        Assert.That(serialized["wait"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["wait"]!.Value<string>(), Is.EqualTo("complete"));
    }
}