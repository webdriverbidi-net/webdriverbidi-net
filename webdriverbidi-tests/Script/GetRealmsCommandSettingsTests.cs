namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class GetRealmsCommandSettingsTests
{
    [Test]
    public void TestCanSerializeSettings()
    {
        GetRealmsCommandSettings properties = new();
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Is.Empty);
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalWindowRealmTypeValue()
    {
        GetRealmsCommandSettings properties = new()
        {
            RealmType = RealmType.Window
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("window"));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalWorkerRealmTypeValue()
    {
        GetRealmsCommandSettings properties = new()
        {
            RealmType = RealmType.Worker
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("worker"));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalDedicatedWorkerRealmTypeValue()
    {
        GetRealmsCommandSettings properties = new()
        {
            RealmType = RealmType.DedicatedWorker
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("dedicated-worker"));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalServiceWorkerRealmTypeValue()
    {
        GetRealmsCommandSettings properties = new()
        {
            RealmType = RealmType.ServiceWorker
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("service-worker"));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalSharedWorkerRealmTypeValue()
    {
        GetRealmsCommandSettings properties = new()
        {
            RealmType = RealmType.SharedWorker
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("shared-worker"));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalWorkletRealmTypeValue()
    {
        GetRealmsCommandSettings properties = new()
        {
            RealmType = RealmType.Worklet
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("worklet"));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalPaintWorkletRealmTypeValue()
    {
        GetRealmsCommandSettings properties = new()
        {
            RealmType = RealmType.PaintWorklet
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("paint-worklet"));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalAudioWorkletRealmTypeValue()
    {
        GetRealmsCommandSettings properties = new()
        {
            RealmType = RealmType.AudioWorklet
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("audio-worklet"));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalBrowsingContextValue()
    {
        GetRealmsCommandSettings properties = new()
        {
            BrowsingContextId = "contextId"
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("contextId"));
        });
    }
}