namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class CloseCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        CloseCommandParameters properties = new("myContextId");
        Assert.Equal("browsingContext.close", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        CloseCommandParameters properties = new("myContextId");
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
    public void TestCanSerializeParametersWithPromptUnloadTrue()
    {
        CloseCommandParameters properties = new("myContextId")
        {
            PromptUnload = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("promptUnload"));
        JToken? promptUnload = serialized["promptUnload"];
        Assert.NotNull(promptUnload);
        Assert.Equal(JTokenType.Boolean, promptUnload.Type);
        Assert.True(promptUnload.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeParametersWithPromptUnloadFalse()
    {
        CloseCommandParameters properties = new("myContextId")
        {
            PromptUnload = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("promptUnload"));
        JToken? promptUnload = serialized["promptUnload"];
        Assert.NotNull(promptUnload);
        Assert.Equal(JTokenType.Boolean, promptUnload.Type);
        Assert.False(promptUnload.Value<bool>());
    }
}
