namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class EvaluateCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        EvaluateCommandParameters properties = new("myExpression", new RealmTarget("myRealm"), true);
        Assert.Equal("script.evaluate", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        EvaluateCommandParameters properties = new("myExpression", new RealmTarget("myRealm"), true);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("expression"));
        JToken? expression = serialized["expression"];
        Assert.NotNull(expression);
        Assert.Equal(JTokenType.String, expression.Type);

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
        EvaluateCommandParameters properties = new("myExpression", new RealmTarget("myRealm"), true)
        {
            ResultOwnership = ResultOwnership.None,
            SerializationOptions = new()
            {
                MaxDomDepth = 1,
            },
            UserActivation = true,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(6, serialized.Count);

        Assert.True(serialized.ContainsKey("expression"));
        JToken? expression = serialized["expression"];
        Assert.NotNull(expression);
        Assert.Equal(JTokenType.String, expression.Type);

        Assert.True(serialized.ContainsKey("target"));
        JToken? target = serialized["target"];
        Assert.NotNull(target);
        Assert.Equal(JTokenType.Object, target.Type);

        Assert.True(serialized.ContainsKey("awaitPromise"));
        JToken? awaitPromise = serialized["awaitPromise"];
        Assert.NotNull(awaitPromise);
        Assert.Equal(JTokenType.Boolean, awaitPromise.Type);

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
}
