namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class HandleUserPromptCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        HandleUserPromptCommandParameters properties = new("myContextId");
        Assert.Equal("browsingContext.handleUserPrompt", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        HandleUserPromptCommandParameters properties = new("myContextId");
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
    public void TestCanSerializeParametersWithAcceptTrue()
    {
        HandleUserPromptCommandParameters properties = new("myContextId")
        {
            Accept = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("accept"));
        JToken? accept = serialized["accept"];
        Assert.NotNull(accept);
        Assert.Equal(JTokenType.Boolean, accept.Type);
        Assert.True(accept.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeParametersWithAcceptFalse()
    {
        HandleUserPromptCommandParameters properties = new("myContextId")
        {
            Accept = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("accept"));
        JToken? accept = serialized["accept"];
        Assert.NotNull(accept);
        Assert.Equal(JTokenType.Boolean, accept.Type);
        Assert.False(accept.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeParametersWithUserText()
    {
        HandleUserPromptCommandParameters properties = new("myContextId")
        {
            UserText = "myUserText"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("userText"));
        JToken? userText = serialized["userText"];
        Assert.NotNull(userText);
        Assert.Equal(JTokenType.String, userText.Type);
        Assert.Equal("myUserText", userText.Value<string>());
    }
}
