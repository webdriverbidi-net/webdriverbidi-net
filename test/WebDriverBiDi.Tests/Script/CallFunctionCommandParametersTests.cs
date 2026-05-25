namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class CallFunctionCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        CallFunctionCommandParameters properties = new("myFunction", new RealmTarget("myRealm"), true);
        Assert.Equal("script.callFunction", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        CallFunctionCommandParameters properties = new("myFunction", new RealmTarget("myRealm"), true);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);
        Assert.True(serialized.ContainsKey("functionDeclaration"));
        JToken? functionDeclaration = serialized["functionDeclaration"];
        Assert.NotNull(functionDeclaration);
        Assert.Equal(JTokenType.String, functionDeclaration.Type);

        Assert.True(serialized.ContainsKey("target"));
        JToken? target = serialized["target"];
        Assert.NotNull(target);
        Assert.Equal(JTokenType.Object, target.Type);

        Assert.True(serialized.ContainsKey("awaitPromise"));
        JToken? awaitPromise = serialized["awaitPromise"];
        Assert.NotNull(awaitPromise);
        Assert.Equal(JTokenType.Boolean, awaitPromise.Type);
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalValues()
    {
        CallFunctionCommandParameters properties = new("myFunction", new RealmTarget("myRealm"), true);
        properties.Arguments.Add(LocalValue.String("myArgument"));
        properties.ThisObject = LocalValue.String("thisObject");
        properties.ResultOwnership = ResultOwnership.None;
        properties.SerializationOptions = new()
        {
            MaxDomDepth = 1,
        };
        properties.UserActivation = true;
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(8, serialized.Count);

        Assert.True(serialized.ContainsKey("functionDeclaration"));
        JToken? functionDeclaration = serialized["functionDeclaration"];
        Assert.NotNull(functionDeclaration);
        Assert.Equal(JTokenType.String, functionDeclaration.Type);

        Assert.True(serialized.ContainsKey("target"));
        JToken? target = serialized["target"];
        Assert.NotNull(target);
        Assert.Equal(JTokenType.Object, target.Type);

        Assert.True(serialized.ContainsKey("awaitPromise"));
        JToken? awaitPromise = serialized["awaitPromise"];
        Assert.NotNull(awaitPromise);
        Assert.Equal(JTokenType.Boolean, awaitPromise.Type);

        Assert.True(serialized.ContainsKey("arguments"));
        JToken? arguments = serialized["arguments"];
        Assert.NotNull(arguments);
        Assert.Equal(JTokenType.Array, arguments.Type);

        Assert.True(serialized.ContainsKey("this"));
        JToken? thisToken = serialized["this"];
        Assert.NotNull(thisToken);
        Assert.Equal(JTokenType.Object, thisToken.Type);

        Assert.True(serialized.ContainsKey("resultOwnership"));
        JToken? resultOwnership = serialized["resultOwnership"];
        Assert.NotNull(resultOwnership);
        Assert.Equal(JTokenType.String, resultOwnership.Type);

        Assert.True(serialized.ContainsKey("serializationOptions"));
        JToken? serializationOptions = serialized["serializationOptions"];
        Assert.NotNull(serializationOptions);
        Assert.Equal(JTokenType.Object, serializationOptions.Type);

        Assert.True(serialized.ContainsKey("userActivation"));
        JToken? userActivation = serialized["userActivation"];
        Assert.NotNull(userActivation);
        Assert.Equal(JTokenType.Boolean, userActivation.Type);
    }

    [Fact]
    public void TestCanSerializeParametersWithChannelValueArgument()
    {
        // This test verifies that ChannelValue can be serialized as an ArgumentValue
        // in the Arguments list. This requires ChannelValue to be registered as a
        // JsonDerivedType on ArgumentValue for proper polymorphic serialization.
        CallFunctionCommandParameters properties = new("myFunction", new RealmTarget("myRealm"), false);
        ChannelProperties channelProperties = new("myChannel")
        {
            Ownership = ResultOwnership.Root,
        };
        properties.Arguments.Add(new ChannelValue(channelProperties));
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.True(serialized.ContainsKey("arguments"));
            JToken? argumentsToken = serialized["arguments"];
            Assert.NotNull(argumentsToken);
            Assert.Equal(JTokenType.Array, argumentsToken.Type);
            JArray? args = argumentsToken as JArray;
            Assert.NotNull(args);
            Assert.Single(args);
            JObject? channelArg = args[0] as JObject;
            Assert.NotNull(channelArg);
            Assert.True(channelArg.ContainsKey("type"));
            JToken? channelArgType = channelArg["type"];
            Assert.NotNull(channelArgType);
            Assert.Equal("channel", channelArgType.Value<string>());
            Assert.True(channelArg.ContainsKey("value"));
            JToken? channelArgValueToken = channelArg["value"];
            Assert.NotNull(channelArgValueToken);
            Assert.Equal(JTokenType.Object, channelArgValueToken.Type);
            JObject? channelValue = channelArgValueToken as JObject;
            Assert.NotNull(channelValue);
            Assert.True(channelValue.ContainsKey("channel"));
            JToken? channelName = channelValue["channel"];
            Assert.NotNull(channelName);
            Assert.Equal("myChannel", channelName.Value<string>());
            Assert.True(channelValue.ContainsKey("ownership"));
            JToken? ownership = channelValue["ownership"];
            Assert.NotNull(ownership);
            Assert.Equal("root", ownership.Value<string>());
        });
    }
}
