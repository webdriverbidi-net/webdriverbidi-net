namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetScriptingEnabledCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetScriptingEnabledCommandParameters properties = new();
        Assert.Equal("emulation.setScriptingEnabled", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetScriptingEnabledCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("enabled"));
        JToken? enabled = serialized["enabled"];
        Assert.NotNull(enabled);
        Assert.Equal(JTokenType.Null, enabled.Type);
        Assert.Null(enabled.Value<bool?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithEnabledTrue()
    {
        SetScriptingEnabledCommandParameters properties = new()
        {
            IsScriptingEnabled = true,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("enabled"));
        JToken? enabled = serialized["enabled"];
        Assert.NotNull(enabled);
        Assert.Equal(JTokenType.Boolean, enabled.Type);
        Assert.True(enabled.Value<bool?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithEnabledFalse()
    {
        SetScriptingEnabledCommandParameters properties = new()
        {
            IsScriptingEnabled = false,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("enabled"));
        JToken? enabled = serialized["enabled"];
        Assert.NotNull(enabled);
        Assert.Equal(JTokenType.Boolean, enabled.Type);
        Assert.False(enabled.Value<bool?>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetScriptingEnabledCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("enabled"));
        JToken? enabled = serialized["enabled"];
        Assert.NotNull(enabled);
        Assert.Equal(JTokenType.Null, enabled.Type);
        Assert.Null(enabled.Value<bool?>());

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
        SetScriptingEnabledCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("enabled"));
        JToken? enabled = serialized["enabled"];
        Assert.NotNull(enabled);
        Assert.Equal(JTokenType.Null, enabled.Type);
        Assert.Null(enabled.Value<bool?>());

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
        SetScriptingEnabledCommandParameters properties = SetScriptingEnabledCommandParameters.ResetScriptingEnabled;
        Assert.NotNull(properties);

        Assert.Null(properties.IsScriptingEnabled);
        Assert.Null(properties.Contexts);
        Assert.Null(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetScriptingEnabledCommandParameters firstInstance = SetScriptingEnabledCommandParameters.ResetScriptingEnabled;
        SetScriptingEnabledCommandParameters secondInstance = SetScriptingEnabledCommandParameters.ResetScriptingEnabled;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
