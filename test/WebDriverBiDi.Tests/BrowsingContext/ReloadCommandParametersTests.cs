namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class ReloadCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        ReloadCommandParameters properties = new("myContextId");
        Assert.Equal("browsingContext.reload", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        ReloadCommandParameters properties = new("myContextId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithIgnoreCacheTrue()
    {
        ReloadCommandParameters properties = new("myContextId")
        {
            IgnoreCache = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("ignoreCache"));
        JToken? ignoreCache = serialized["ignoreCache"];
        Assert.NotNull(ignoreCache);
        Assert.Equal(JTokenType.Boolean, ignoreCache.Type);
        Assert.True(ignoreCache.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeParametersWithIgnoreCacheFalse()
    {
        ReloadCommandParameters properties = new("myContextId")
        {
            IgnoreCache = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("ignoreCache"));
        JToken? ignoreCache = serialized["ignoreCache"];
        Assert.NotNull(ignoreCache);
        Assert.Equal(JTokenType.Boolean, ignoreCache.Type);
        Assert.False(ignoreCache.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeParametersWithAcceptWaitNone()
    {
        ReloadCommandParameters properties = new("myContextId")
        {
            Wait = ReadinessState.None
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("wait"));
        JToken? wait = serialized["wait"];
        Assert.NotNull(wait);
        Assert.Equal(JTokenType.String, wait.Type);
        Assert.Equal("none", wait.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithAcceptWaitInteractive()
    {
        ReloadCommandParameters properties = new("myContextId")
        {
            Wait = ReadinessState.Interactive
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("wait"));
        JToken? wait = serialized["wait"];
        Assert.NotNull(wait);
        Assert.Equal(JTokenType.String, wait.Type);
        Assert.Equal("interactive", wait.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithAcceptWaitComplete()
    {
        ReloadCommandParameters properties = new("myContextId")
        {
            Wait = ReadinessState.Complete
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("wait"));
        JToken? wait = serialized["wait"];
        Assert.NotNull(wait);
        Assert.Equal(JTokenType.String, wait.Type);
        Assert.Equal("complete", wait.Value<string>());
    }
}
