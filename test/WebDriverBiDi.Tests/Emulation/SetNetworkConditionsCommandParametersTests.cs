namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetNetworkConditionsCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetNetworkConditionsCommandParameters properties = new();
        Assert.Equal("emulation.setNetworkConditions", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetNetworkConditionsCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("networkConditions"));
        JToken? networkConditions = serialized["networkConditions"];
        Assert.NotNull(networkConditions);
        Assert.Equal(JTokenType.Null, networkConditions.Type);
        Assert.Null(networkConditions.Value<JObject?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOffline()
    {
        SetNetworkConditionsCommandParameters properties = new()
        {
            NetworkConditions = new NetworkConditionsOffline()
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("networkConditions"));
        JToken? networkConditionsToken = serialized["networkConditions"];
        Assert.NotNull(networkConditionsToken);
        Assert.Equal(JTokenType.Object, networkConditionsToken.Type);
        JObject? conditionsObject = networkConditionsToken as JObject;
        Assert.NotNull(conditionsObject);
        Assert.Single(conditionsObject);

        Assert.True(conditionsObject.ContainsKey("type"));
        JToken? type = conditionsObject["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("offline", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetNetworkConditionsCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("networkConditions"));
        JToken? networkConditions = serialized["networkConditions"];
        Assert.NotNull(networkConditions);
        Assert.Equal(JTokenType.Null, networkConditions.Type);
        Assert.Null(networkConditions.Value<JObject?>());

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
        SetNetworkConditionsCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("networkConditions"));
        JToken? networkConditions = serialized["networkConditions"];
        Assert.NotNull(networkConditions);
        Assert.Equal(JTokenType.Null, networkConditions.Type);
        Assert.Null(networkConditions.Value<JObject?>());

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
        SetNetworkConditionsCommandParameters properties = SetNetworkConditionsCommandParameters.ResetNetworkConditions;
        Assert.NotNull(properties);

        Assert.Null(properties.NetworkConditions);
        Assert.Null(properties.Contexts);
        Assert.Null(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetNetworkConditionsCommandParameters firstInstance = SetNetworkConditionsCommandParameters.ResetNetworkConditions;
        SetNetworkConditionsCommandParameters secondInstance = SetNetworkConditionsCommandParameters.ResetNetworkConditions;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
