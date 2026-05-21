namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetBypassCSPCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetBypassCSPCommandParameters properties = new();
        Assert.Equal("browsingContext.setBypassCSP", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetBypassCSPCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("bypass"));
        JToken? bypass = serialized["bypass"];
        Assert.NotNull(bypass);
        Assert.Equal(JTokenType.Null, bypass.Type);
        Assert.Null(bypass.Value<bool?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithEnabledTrue()
    {
        SetBypassCSPCommandParameters properties = new()
        {
            Bypass = true,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("bypass"));
        JToken? bypass = serialized["bypass"];
        Assert.NotNull(bypass);
        Assert.Equal(JTokenType.Boolean, bypass.Type);
        Assert.True(bypass.Value<bool?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithEnabledFalse()
    {
        SetBypassCSPCommandParameters properties = new()
        {
            Bypass = false,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("bypass"));
        JToken? bypass = serialized["bypass"];
        Assert.NotNull(bypass);
        Assert.Equal(JTokenType.Null, bypass.Type);
        Assert.Null(bypass.Value<bool?>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetBypassCSPCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("bypass"));
        JToken? bypass = serialized["bypass"];
        Assert.NotNull(bypass);
        Assert.Equal(JTokenType.Null, bypass.Type);
        Assert.Null(bypass.Value<bool?>());

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
        SetBypassCSPCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("bypass"));
        JToken? bypass = serialized["bypass"];
        Assert.NotNull(bypass);
        Assert.Equal(JTokenType.Null, bypass.Type);
        Assert.Null(bypass.Value<bool?>());

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
        SetBypassCSPCommandParameters properties = SetBypassCSPCommandParameters.ResetBypassCSP;
        Assert.NotNull(properties);

        Assert.Null(properties.Bypass);
        Assert.Null(properties.Contexts);
        Assert.Null(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetBypassCSPCommandParameters firstInstance = SetBypassCSPCommandParameters.ResetBypassCSP;
        SetBypassCSPCommandParameters secondInstance = SetBypassCSPCommandParameters.ResetBypassCSP;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
