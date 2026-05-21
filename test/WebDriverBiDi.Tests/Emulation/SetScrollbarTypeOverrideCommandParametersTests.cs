namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetScrollbarTypeOverrideCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetScrollbarTypeOverrideCommandParameters properties = new();
        Assert.Equal("emulation.setScrollbarTypeOverride", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetScrollbarTypeOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("scrollbarType"));
        JToken? scrollbarType = serialized["scrollbarType"];
        Assert.NotNull(scrollbarType);
        Assert.Equal(JTokenType.Null, scrollbarType.Type);
        Assert.Null(scrollbarType.Value<JObject?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithClassicScrollbarType()
    {
        SetScrollbarTypeOverrideCommandParameters properties = new()
        {
            ScrollbarType = ScrollbarType.Classic,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("scrollbarType"));
        JToken? scrollbarType = serialized["scrollbarType"];
        Assert.NotNull(scrollbarType);
        Assert.Equal(JTokenType.String, scrollbarType.Type);
        Assert.Equal("classic", scrollbarType.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOverlayScrollbarType()
    {
        SetScrollbarTypeOverrideCommandParameters properties = new()
        {
            ScrollbarType = ScrollbarType.Overlay,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("scrollbarType"));
        JToken? scrollbarType = serialized["scrollbarType"];
        Assert.NotNull(scrollbarType);
        Assert.Equal(JTokenType.String, scrollbarType.Type);
        Assert.Equal("overlay", scrollbarType.Value<string>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetScrollbarTypeOverrideCommandParameters properties = new()
        {
            Contexts =
            [
                "context1",
                "context2",
            ]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("scrollbarType"));
        JToken? scrollbarType = serialized["scrollbarType"];
        Assert.NotNull(scrollbarType);
        Assert.Equal(JTokenType.Null, scrollbarType.Type);
        Assert.Null(scrollbarType.Value<JObject?>());

        Assert.True(serialized.ContainsKey("contexts"));
        JToken? contextsToken = serialized["contexts"];
        Assert.NotNull(contextsToken);
        Assert.Equal(JTokenType.Array, contextsToken.Type);
        JArray? contextsArray = contextsToken.Value<JArray>();
        Assert.NotNull(contextsArray);
        Assert.Equal(2, contextsArray.Count);
        Assert.Equal(JTokenType.String, contextsArray[0].Type);
        Assert.Equal("context1", contextsArray[0].Value<string>());
        Assert.Equal(JTokenType.String, contextsArray[1].Type);
        Assert.Equal("context2", contextsArray[1].Value<string>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithUserContexts()
    {
        SetScrollbarTypeOverrideCommandParameters properties = new()
        {
            UserContexts =
            [
                "userContext1",
                "userContext2",
            ]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("scrollbarType"));
        JToken? scrollbarType = serialized["scrollbarType"];
        Assert.NotNull(scrollbarType);
        Assert.Equal(JTokenType.Null, scrollbarType.Type);
        Assert.Null(scrollbarType.Value<JObject?>());

        Assert.True(serialized.ContainsKey("userContexts"));
        JToken? userContextsToken = serialized["userContexts"];
        Assert.NotNull(userContextsToken);
        Assert.Equal(JTokenType.Array, userContextsToken.Type);
        JArray? userContextsArray = userContextsToken.Value<JArray>();
        Assert.NotNull(userContextsArray);
        Assert.Equal(2, userContextsArray.Count);
        Assert.Equal(JTokenType.String, userContextsArray[0].Type);
        Assert.Equal("userContext1", userContextsArray[0].Value<string>());
        Assert.Equal(JTokenType.String, userContextsArray[1].Type);
        Assert.Equal("userContext2", userContextsArray[1].Value<string>());
    }

    [Fact]
    public void TestCanGetResetParameters()
    {
        SetScrollbarTypeOverrideCommandParameters properties = SetScrollbarTypeOverrideCommandParameters.ResetScrollbarTypeOverride;
        Assert.NotNull(properties);

        Assert.Null(properties.ScrollbarType);
        Assert.Null(properties.Contexts);
        Assert.Null(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetScrollbarTypeOverrideCommandParameters firstInstance = SetScrollbarTypeOverrideCommandParameters.ResetScrollbarTypeOverride;
        SetScrollbarTypeOverrideCommandParameters secondInstance = SetScrollbarTypeOverrideCommandParameters.ResetScrollbarTypeOverride;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
