namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetGeolocationOverrideErrorCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetGeolocationOverrideErrorCommandParameters properties = new();
        Assert.Equal("emulation.setGeolocationOverride", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetGeolocationOverrideErrorCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("error"));
        JToken? errorToken = serialized["error"];
        Assert.NotNull(errorToken);
        Assert.Equal(JTokenType.Object, errorToken.Type);
        JObject? errorObject = errorToken.ToObject<JObject>();
        Assert.NotNull(errorObject);
        Assert.Single(errorObject);

        Assert.True(errorObject.ContainsKey("type"));
        JToken? type = errorObject["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("positionUnavailable", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetGeolocationOverrideErrorCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("error"));
        JToken? error = serialized["error"];
        Assert.NotNull(error);
        Assert.Equal(JTokenType.Object, error.Type);

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
        SetGeolocationOverrideErrorCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("error"));
        JToken? error = serialized["error"];
        Assert.NotNull(error);
        Assert.Equal(JTokenType.Object, error.Type);

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
}
