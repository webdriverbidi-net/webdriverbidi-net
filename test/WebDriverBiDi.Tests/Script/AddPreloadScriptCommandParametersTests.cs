namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class AddPreloadScriptCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        AddPreloadScriptCommandParameters properties = new("myFunctionDeclaration");
        Assert.Equal("script.addPreloadScript", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeProperties()
    {
        AddPreloadScriptCommandParameters properties = new("myFunctionDeclaration");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("functionDeclaration"));
        JToken? functionDeclaration = serialized["functionDeclaration"];
        Assert.NotNull(functionDeclaration);
        Assert.Equal(JTokenType.String, functionDeclaration.Type);
        Assert.Equal("myFunctionDeclaration", functionDeclaration.Value<string>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithSandbox()
    {
        AddPreloadScriptCommandParameters properties = new("myFunctionDeclaration")
        {
            Sandbox = "mySandbox"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("functionDeclaration"));
        JToken? functionDeclaration = serialized["functionDeclaration"];
        Assert.NotNull(functionDeclaration);
        Assert.Equal(JTokenType.String, functionDeclaration.Type);
        Assert.Equal("myFunctionDeclaration", functionDeclaration.Value<string>());

        Assert.True(serialized.ContainsKey("sandbox"));
        JToken? sandbox = serialized["sandbox"];
        Assert.NotNull(sandbox);
        Assert.Equal(JTokenType.String, sandbox.Type);
        Assert.Equal("mySandbox", sandbox.Value<string>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithArguments()
    {
        AddPreloadScriptCommandParameters properties = new("myFunctionDeclaration")
        {
            Arguments =
            [
                new ChannelValue(new ChannelProperties("myChannel"))
            ]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("functionDeclaration"));
        JToken? functionDeclaration = serialized["functionDeclaration"];
        Assert.NotNull(functionDeclaration);
        Assert.Equal(JTokenType.String, functionDeclaration.Type);
        Assert.Equal("myFunctionDeclaration", functionDeclaration.Value<string>());

        Assert.True(serialized.ContainsKey("arguments"));
        JToken? argumentsToken = serialized["arguments"];
        Assert.NotNull(argumentsToken);
        Assert.Equal(JTokenType.Array, argumentsToken.Type);
        JArray? argsArray = argumentsToken as JArray;
        Assert.NotNull(argsArray);
        Assert.Single(argsArray);
        Assert.Equal(JTokenType.Object, argsArray[0].Type);
        JObject? argObject = argsArray[0] as JObject;
        Assert.NotNull(argObject);
        Assert.Equal(2, argObject.Count);
        Assert.True(argObject.ContainsKey("type"));
        JToken? argType = argObject["type"];
        Assert.NotNull(argType);
        Assert.Equal("channel", argType.Value<string>());
        Assert.True(argObject.ContainsKey("value"));
        JToken? argValueToken = argObject["value"];
        Assert.NotNull(argValueToken);
        Assert.Equal(JTokenType.Object, argValueToken.Type);
        JObject? argValue = argValueToken as JObject;
        Assert.NotNull(argValue);
        Assert.Single(argValue);
        Assert.True(argValue.ContainsKey("channel"));
        JToken? channel = argValue["channel"];
        Assert.NotNull(channel);
        Assert.Equal(JTokenType.String, channel.Type);
        Assert.Equal("myChannel", channel.Value<string>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithContexts()
    {
        AddPreloadScriptCommandParameters properties = new("myFunctionDeclaration")
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

        Assert.True(serialized.ContainsKey("functionDeclaration"));
        JToken? functionDeclaration = serialized["functionDeclaration"];
        Assert.NotNull(functionDeclaration);
        Assert.Equal(JTokenType.String, functionDeclaration.Type);
        Assert.Equal("myFunctionDeclaration", functionDeclaration.Value<string>());

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
        AddPreloadScriptCommandParameters properties = new("myFunctionDeclaration")
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

        Assert.True(serialized.ContainsKey("functionDeclaration"));
        JToken? functionDeclaration = serialized["functionDeclaration"];
        Assert.NotNull(functionDeclaration);
        Assert.Equal(JTokenType.String, functionDeclaration.Type);
        Assert.Equal("myFunctionDeclaration", functionDeclaration.Value<string>());

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
}
