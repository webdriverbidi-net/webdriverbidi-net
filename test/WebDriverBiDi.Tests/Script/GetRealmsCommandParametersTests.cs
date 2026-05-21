namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class GetRealmsCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        GetRealmsCommandParameters properties = new();
        Assert.Equal("script.getRealms", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        GetRealmsCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Empty(serialized);
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalWindowRealmTypeValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            RealmType = RealmType.Window
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("window", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalWorkerRealmTypeValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            RealmType = RealmType.Worker
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("worker", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalDedicatedWorkerRealmTypeValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            RealmType = RealmType.DedicatedWorker
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("dedicated-worker", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalServiceWorkerRealmTypeValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            RealmType = RealmType.ServiceWorker
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("service-worker", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalSharedWorkerRealmTypeValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            RealmType = RealmType.SharedWorker
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("shared-worker", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalWorkletRealmTypeValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            RealmType = RealmType.Worklet
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("worklet", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalPaintWorkletRealmTypeValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            RealmType = RealmType.PaintWorklet
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("paint-worklet", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalAudioWorkletRealmTypeValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            RealmType = RealmType.AudioWorklet
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("audio-worklet", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalBrowsingContextValue()
    {
        GetRealmsCommandParameters properties = new()
        {
            BrowsingContextId = "contextId"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("contextId", context.Value<string>());
    }
}
