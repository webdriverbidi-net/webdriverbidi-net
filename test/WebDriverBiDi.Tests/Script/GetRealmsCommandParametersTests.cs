namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class GetRealmsCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        GetRealmsCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("script.getRealms"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        GetRealmsCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Is.Empty);
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalWindowRealmTypeValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            RealmType = RealmType.Window
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("window"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalWorkerRealmTypeValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            RealmType = RealmType.Worker
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("worker"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalDedicatedWorkerRealmTypeValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            RealmType = RealmType.DedicatedWorker
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("dedicated-worker"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalServiceWorkerRealmTypeValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            RealmType = RealmType.ServiceWorker
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("service-worker"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalSharedWorkerRealmTypeValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            RealmType = RealmType.SharedWorker
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("shared-worker"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalWorkletRealmTypeValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            RealmType = RealmType.Worklet
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("worklet"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalPaintWorkletRealmTypeValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            RealmType = RealmType.PaintWorklet
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("paint-worklet"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalAudioWorkletRealmTypeValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            RealmType = RealmType.AudioWorklet
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("audio-worklet"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalBrowsingContextValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            BrowsingContextId = "contextId"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("contextId"));
        }
    }
}
