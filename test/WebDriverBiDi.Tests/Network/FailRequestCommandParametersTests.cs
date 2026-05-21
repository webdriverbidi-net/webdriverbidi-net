namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class FailRequestCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        FailRequestCommandParameters properties = new("requestId");
        Assert.Equal("network.failRequest", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        FailRequestCommandParameters properties = new("requestId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("requestId", request.Value<string>());
    }
}
