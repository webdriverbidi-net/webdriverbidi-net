namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetTouchOverrideCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetTouchOverrideCommandParameters properties = new();
        Assert.Equal("emulation.setTouchOverride", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetTouchOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("maxTouchPoints"));
        JToken? maxTouchPoints = serialized["maxTouchPoints"];
        Assert.NotNull(maxTouchPoints);
        Assert.Equal(JTokenType.Null, maxTouchPoints.Type);
        Assert.Null(maxTouchPoints.Value<JObject?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithScreenOrientation()
    {
        SetTouchOverrideCommandParameters properties = new()
        {
            MaxTouchPoints = 100,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("maxTouchPoints"));
        JToken? maxTouchPoints = serialized["maxTouchPoints"];
        Assert.NotNull(maxTouchPoints);
        Assert.Equal(JTokenType.Integer, maxTouchPoints.Type);
        Assert.Equal(100UL, maxTouchPoints.Value<ulong>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetTouchOverrideCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("maxTouchPoints"));
        JToken? maxTouchPoints = serialized["maxTouchPoints"];
        Assert.NotNull(maxTouchPoints);
        Assert.Equal(JTokenType.Null, maxTouchPoints.Type);
        Assert.Null(maxTouchPoints.Value<JObject?>());

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
        SetTouchOverrideCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("maxTouchPoints"));
        JToken? maxTouchPoints = serialized["maxTouchPoints"];
        Assert.NotNull(maxTouchPoints);
        Assert.Equal(JTokenType.Null, maxTouchPoints.Type);
        Assert.Null(maxTouchPoints.Value<JObject?>());

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
        SetTouchOverrideCommandParameters properties = SetTouchOverrideCommandParameters.ResetTouchOverride;
        Assert.NotNull(properties);

        Assert.Null(properties.MaxTouchPoints);
        Assert.Null(properties.Contexts);
        Assert.Null(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetTouchOverrideCommandParameters firstInstance = SetTouchOverrideCommandParameters.ResetTouchOverride;
        SetTouchOverrideCommandParameters secondInstance = SetTouchOverrideCommandParameters.ResetTouchOverride;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
