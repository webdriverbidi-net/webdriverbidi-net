namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class RemoveInterceptCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        RemoveInterceptCommandParameters properties = new("interceptId");
        Assert.Equal("network.removeIntercept", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        RemoveInterceptCommandParameters properties = new("interceptId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("intercept"));
        JToken? intercept = serialized["intercept"];
        Assert.NotNull(intercept);
        Assert.Equal(JTokenType.String, intercept.Type);
        Assert.Equal("interceptId", intercept.Value<string>());
    }
}
