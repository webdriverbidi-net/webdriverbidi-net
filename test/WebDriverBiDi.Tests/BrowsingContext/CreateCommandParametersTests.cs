namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class CreateCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        CreateCommandParameters properties = new(CreateType.Tab);
        Assert.Equal("browsingContext.create", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParametersForTab()
    {
        CreateCommandParameters properties = new(CreateType.Tab);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("tab", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersForWindow()
    {
        CreateCommandParameters properties = new(CreateType.Window);
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
    public void TestCanSerializeParametersWithReferenceContext()
    {
        CreateCommandParameters properties = new(CreateType.Tab)
        {
            ReferenceContextId = "myReferenceContext"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("tab", type.Value<string>());

        Assert.True(serialized.ContainsKey("referenceContext"));
        JToken? referenceContext = serialized["referenceContext"];
        Assert.NotNull(referenceContext);
        Assert.Equal(JTokenType.String, referenceContext.Type);
        Assert.Equal("myReferenceContext", referenceContext.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithUserContext()
    {
        CreateCommandParameters properties = new(CreateType.Tab)
        {
            UserContextId = "myUserContext"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("tab", type.Value<string>());

        Assert.True(serialized.ContainsKey("userContext"));
        JToken? userContext = serialized["userContext"];
        Assert.NotNull(userContext);
        Assert.Equal(JTokenType.String, userContext.Type);
        Assert.Equal("myUserContext", userContext.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithBackgroundTrue()
    {
        CreateCommandParameters properties = new(CreateType.Tab)
        {
            IsCreatedInBackground = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("tab", type.Value<string>());

        Assert.True(serialized.ContainsKey("background"));
        JToken? background = serialized["background"];
        Assert.NotNull(background);
        Assert.Equal(JTokenType.Boolean, background.Type);
        Assert.True(background.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeParametersWithBackgroundFalse()
    {
        CreateCommandParameters properties = new(CreateType.Tab)
        {
            IsCreatedInBackground = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("tab", type.Value<string>());

        Assert.True(serialized.ContainsKey("background"));
        JToken? background = serialized["background"];
        Assert.NotNull(background);
        Assert.Equal(JTokenType.Boolean, background.Type);
        Assert.False(background.Value<bool>());
    }

    [Fact]
    public void TestCanSetCreateTypeProperty()
    {
        CreateCommandParameters properties = new(CreateType.Tab)
        {
            CreateType = CreateType.Window
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
}
