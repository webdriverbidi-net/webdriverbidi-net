namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetViewportMetaOverrideCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetViewportMetaOverrideCommandParameters properties = new();
        Assert.Equal("emulation.setViewportMetaOverride", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetViewportMetaOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("viewportMeta"));
        JToken? viewportMeta = serialized["viewportMeta"];
        Assert.NotNull(viewportMeta);
        Assert.Equal(JTokenType.Null, viewportMeta.Type);
        Assert.Null(viewportMeta.Value<bool?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOverrideTrue()
    {
        SetViewportMetaOverrideCommandParameters properties = new()
        {
            IsViewportMetaOverridden = true,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("viewportMeta"));
        JToken? viewportMeta = serialized["viewportMeta"];
        Assert.NotNull(viewportMeta);
        Assert.Equal(JTokenType.Boolean, viewportMeta.Type);
        Assert.True(viewportMeta.Value<bool?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOverrideFalse()
    {
        SetViewportMetaOverrideCommandParameters properties = new()
        {
            IsViewportMetaOverridden = false,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("viewportMeta"));
        JToken? viewportMeta = serialized["viewportMeta"];
        Assert.NotNull(viewportMeta);
        Assert.Equal(JTokenType.Boolean, viewportMeta.Type);
        Assert.False(viewportMeta.Value<bool?>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetViewportMetaOverrideCommandParameters properties = new()
        {
            Contexts =
            {
                "context1",
                "context2",
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("viewportMeta"));
        JToken? viewportMeta = serialized["viewportMeta"];
        Assert.NotNull(viewportMeta);
        Assert.Equal(JTokenType.Null, viewportMeta.Type);
        Assert.Null(viewportMeta.Value<bool?>());

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
        SetViewportMetaOverrideCommandParameters properties = new()
        {
            UserContexts =
            {
                "userContext1",
                "userContext2",
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("viewportMeta"));
        JToken? viewportMeta = serialized["viewportMeta"];
        Assert.NotNull(viewportMeta);
        Assert.Equal(JTokenType.Null, viewportMeta.Type);
        Assert.Null(viewportMeta.Value<bool?>());

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
        SetViewportMetaOverrideCommandParameters properties = SetViewportMetaOverrideCommandParameters.ResetViewportMetaOverride;
        Assert.NotNull(properties);

        Assert.Null(properties.IsViewportMetaOverridden);
        Assert.Empty(properties.Contexts);
        Assert.Empty(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetViewportMetaOverrideCommandParameters firstInstance = SetViewportMetaOverrideCommandParameters.ResetViewportMetaOverride;
        SetViewportMetaOverrideCommandParameters secondInstance = SetViewportMetaOverrideCommandParameters.ResetViewportMetaOverride;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
