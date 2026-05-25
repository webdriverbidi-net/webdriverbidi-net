namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetUserAgentOverrideCoordinatesCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetUserAgentOverrideCommandParameters properties = new();
        Assert.Equal("emulation.setUserAgentOverride", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetUserAgentOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("userAgent"));
        JToken? userAgent = serialized["userAgent"];
        Assert.NotNull(userAgent);
        Assert.Equal(JTokenType.Null, userAgent.Type);
        Assert.Null(userAgent.Value<JObject?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithLocale()
    {
        SetUserAgentOverrideCommandParameters properties = new()
        {
            UserAgent = "WebDriverBiDi.NET/1.0 (no platform)"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("userAgent"));
        JToken? userAgent = serialized["userAgent"];
        Assert.NotNull(userAgent);
        Assert.Equal(JTokenType.String, userAgent.Type);
        Assert.Equal("WebDriverBiDi.NET/1.0 (no platform)", userAgent.Value<string>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetUserAgentOverrideCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("userAgent"));
        JToken? userAgent = serialized["userAgent"];
        Assert.NotNull(userAgent);
        Assert.Equal(JTokenType.Null, userAgent.Type);
        Assert.Null(userAgent.Value<JObject?>());

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
        SetUserAgentOverrideCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("userAgent"));
        JToken? userAgent = serialized["userAgent"];
        Assert.NotNull(userAgent);
        Assert.Equal(JTokenType.Null, userAgent.Type);
        Assert.Null(userAgent.Value<JObject?>());

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
        SetUserAgentOverrideCommandParameters properties = SetUserAgentOverrideCommandParameters.ResetUserAgentOverride;
        Assert.NotNull(properties);

        Assert.Null(properties.UserAgent);
        Assert.Null(properties.Contexts);
        Assert.Null(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetUserAgentOverrideCommandParameters firstInstance = SetUserAgentOverrideCommandParameters.ResetUserAgentOverride;
        SetUserAgentOverrideCommandParameters secondInstance = SetUserAgentOverrideCommandParameters.ResetUserAgentOverride;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
