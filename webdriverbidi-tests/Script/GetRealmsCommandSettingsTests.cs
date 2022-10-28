namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class GetRealmsCommandSettingsTests
{
    [Test]
    public void TestCanSerializeSettings()
    {
        var properties = new GetRealmsCommandSettings();
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalWindowRealmTypeValue()
    {
        var properties = new GetRealmsCommandSettings();
        properties.RealmType = RealmType.Window;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("type"));
        Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("window"));
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalWorkerRealmTypeValue()
    {
        var properties = new GetRealmsCommandSettings();
        properties.RealmType = RealmType.Worker;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("type"));
        Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("worker"));
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalDedicatedWorkerRealmTypeValue()
    {
        var properties = new GetRealmsCommandSettings();
        properties.RealmType = RealmType.DedicatedWorker;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("type"));
        Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("dedicated-worker"));
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalServiceWorkerRealmTypeValue()
    {
        var properties = new GetRealmsCommandSettings();
        properties.RealmType = RealmType.ServiceWorker;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("type"));
        Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("service-worker"));
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalSharedWorkerRealmTypeValue()
    {
        var properties = new GetRealmsCommandSettings();
        properties.RealmType = RealmType.SharedWorker;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("type"));
        Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("shared-worker"));
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalWorkletRealmTypeValue()
    {
        var properties = new GetRealmsCommandSettings();
        properties.RealmType = RealmType.Worklet;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("type"));
        Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("worklet"));
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalPaintWorkletRealmTypeValue()
    {
        var properties = new GetRealmsCommandSettings();
        properties.RealmType = RealmType.PaintWorklet;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("type"));
        Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("paint-worklet"));
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalAudioWorkletRealmTypeValue()
    {
        var properties = new GetRealmsCommandSettings();
        properties.RealmType = RealmType.AudioWorklet;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("type"));
        Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("audio-worklet"));
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalBrowsingContextValue()
    {
        var properties = new GetRealmsCommandSettings();
        properties.BrowsingContextId = "contextId";
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("context"));
        Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("contextId"));
    }
}