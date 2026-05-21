namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetCacheBehaviorCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetCacheBehaviorCommandParameters properties = new(CacheBehavior.Default);
        Assert.Equal("network.setCacheBehavior", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetCacheBehaviorCommandParameters properties = new(CacheBehavior.Default);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("cacheBehavior"));
        JToken? cacheBehavior = serialized["cacheBehavior"];
        Assert.NotNull(cacheBehavior);
        Assert.Equal(JTokenType.String, cacheBehavior.Type);
        Assert.Equal("default", cacheBehavior.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithBypass()
    {
        SetCacheBehaviorCommandParameters properties = new(CacheBehavior.Bypass);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("cacheBehavior"));
        JToken? cacheBehavior = serialized["cacheBehavior"];
        Assert.NotNull(cacheBehavior);
        Assert.Equal(JTokenType.String, cacheBehavior.Type);
        Assert.Equal("bypass", cacheBehavior.Value<string>());
    }

    [Fact]
    public void TestCanSetCacheBehaviorViaProperty()
    {
        SetCacheBehaviorCommandParameters properties = new(CacheBehavior.Default);
        properties.CacheBehavior = CacheBehavior.Bypass;
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("cacheBehavior"));
        JToken? cacheBehavior = serialized["cacheBehavior"];
        Assert.NotNull(cacheBehavior);
        Assert.Equal(JTokenType.String, cacheBehavior.Type);
        Assert.Equal("bypass", cacheBehavior.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithContexts()
    {
        SetCacheBehaviorCommandParameters properties = new(CacheBehavior.Default)
        {
            Contexts = ["myContext"]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("cacheBehavior"));
        JToken? cacheBehavior = serialized["cacheBehavior"];
        Assert.NotNull(cacheBehavior);
        Assert.Equal(JTokenType.String, cacheBehavior.Type);
        Assert.Equal("default", cacheBehavior.Value<string>());

        Assert.True(serialized.ContainsKey("contexts"));
        JToken? contextsToken = serialized["contexts"];
        Assert.NotNull(contextsToken);
        Assert.Equal(JTokenType.Array, contextsToken.Type);
        JArray? contextsObject = contextsToken as JArray;
        Assert.NotNull(contextsObject);
        Assert.Single(contextsObject);
        Assert.Equal(JTokenType.String, contextsObject[0].Type);
        Assert.Equal("myContext", contextsObject[0].Value<string>());
    }
}
