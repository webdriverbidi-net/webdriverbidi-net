namespace WebDriverBiDi.UserAgentClientHints;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetClientHintsOverrideCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetClientHintsOverrideCommandParameters properties = new();
        Assert.Equal("userAgentClientHints.setClientHintsOverride", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetClientHintsOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("clientHints"));
        JToken? clientHints = serialized["clientHints"];
        Assert.NotNull(clientHints);
        Assert.Equal(JTokenType.Null, clientHints.Type);
        Assert.Null(clientHints.Value<JObject?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithClientHints()
    {
        SetClientHintsOverrideCommandParameters properties = new()
        {
            ClientHints = new()
            {
                Platform = "myPlatform"
            },
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("clientHints"));
        JToken? clientHintsToken = serialized["clientHints"];
        Assert.NotNull(clientHintsToken);
        Assert.Equal(JTokenType.Object, clientHintsToken.Type);
        JObject? clientHintsObject = clientHintsToken as JObject;
        Assert.NotNull(clientHintsObject);
        Assert.Single(clientHintsObject);
        Assert.True(clientHintsObject.ContainsKey("platform"));
        JToken? platform = clientHintsObject["platform"];
        Assert.NotNull(platform);
        Assert.Equal("myPlatform", platform.Value<string>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetClientHintsOverrideCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("clientHints"));
        JToken? clientHints = serialized["clientHints"];
        Assert.NotNull(clientHints);
        Assert.Equal(JTokenType.Null, clientHints.Type);
        Assert.Null(clientHints.Value<JObject?>());

        Assert.True(serialized.ContainsKey("contexts"));
        JToken? contextsToken = serialized["contexts"];
        Assert.NotNull(contextsToken);
        Assert.Equal(JTokenType.Array, contextsToken.Type);
        JArray? contextsArray = contextsToken as JArray;
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
        SetClientHintsOverrideCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("clientHints"));
        JToken? clientHints = serialized["clientHints"];
        Assert.NotNull(clientHints);
        Assert.Equal(JTokenType.Null, clientHints.Type);
        Assert.Null(clientHints.Value<JObject?>());

        Assert.True(serialized.ContainsKey("userContexts"));
        JToken? userContextsToken = serialized["userContexts"];
        Assert.NotNull(userContextsToken);
        Assert.Equal(JTokenType.Array, userContextsToken.Type);
        JArray? userContextsArray = userContextsToken as JArray;
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
        SetClientHintsOverrideCommandParameters properties = SetClientHintsOverrideCommandParameters.ResetClientHintsOverride;
        Assert.NotNull(properties);

        Assert.Null(properties.ClientHints);
        Assert.Null(properties.Contexts);
        Assert.Null(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetClientHintsOverrideCommandParameters firstInstance = SetClientHintsOverrideCommandParameters.ResetClientHintsOverride;
        SetClientHintsOverrideCommandParameters secondInstance = SetClientHintsOverrideCommandParameters.ResetClientHintsOverride;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
