namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class NavigateCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        NavigateCommandParameters properties = new("myContextId", "http://example.com");
        Assert.Equal("browsingContext.navigate", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        NavigateCommandParameters properties = new("myContextId", "http://example.com");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("url"));
        JToken? url = serialized["url"];
        Assert.NotNull(url);
        Assert.Equal(JTokenType.String, url.Type);
        Assert.Equal("http://example.com", url.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithAcceptWaitNone()
    {
        NavigateCommandParameters properties = new("myContextId", "http://example.com")
        {
            Wait = ReadinessState.None
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("url"));
        JToken? url = serialized["url"];
        Assert.NotNull(url);
        Assert.Equal(JTokenType.String, url.Type);
        Assert.Equal("http://example.com", url.Value<string>());

        Assert.True(serialized.ContainsKey("wait"));
        JToken? wait = serialized["wait"];
        Assert.NotNull(wait);
        Assert.Equal(JTokenType.String, wait.Type);
        Assert.Equal("none", wait.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithAcceptWaitInteractive()
    {
        NavigateCommandParameters properties = new("myContextId", "http://example.com")
        {
            Wait = ReadinessState.Interactive
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("url"));
        JToken? url = serialized["url"];
        Assert.NotNull(url);
        Assert.Equal(JTokenType.String, url.Type);
        Assert.Equal("http://example.com", url.Value<string>());

        Assert.True(serialized.ContainsKey("wait"));
        JToken? wait = serialized["wait"];
        Assert.NotNull(wait);
        Assert.Equal(JTokenType.String, wait.Type);
        Assert.Equal("interactive", wait.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithAcceptWaitComplete()
    {
        NavigateCommandParameters properties = new("myContextId", "http://example.com")
        {
            Wait = ReadinessState.Complete
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("url"));
        JToken? url = serialized["url"];
        Assert.NotNull(url);
        Assert.Equal(JTokenType.String, url.Type);
        Assert.Equal("http://example.com", url.Value<string>());

        Assert.True(serialized.ContainsKey("wait"));
        JToken? wait = serialized["wait"];
        Assert.NotNull(wait);
        Assert.Equal(JTokenType.String, wait.Type);
        Assert.Equal("complete", wait.Value<string>());
    }
}
