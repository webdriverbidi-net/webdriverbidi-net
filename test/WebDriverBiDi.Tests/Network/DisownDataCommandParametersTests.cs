namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class DisownDataCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        DisownDataCommandParameters properties = new("myCollectorId", "myRequestId");
        Assert.Equal("network.disownData", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        DisownDataCommandParameters properties = new("myCollectorId", "myRequestId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);
        Assert.True(serialized.ContainsKey("collector"));
        JToken? collector = serialized["collector"];
        Assert.NotNull(collector);
        Assert.Equal(JTokenType.String, collector.Type);
        Assert.Equal("myCollectorId", collector.Value<string>());

        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("myRequestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("dataType"));
        JToken? dataType = serialized["dataType"];
        Assert.NotNull(dataType);
        Assert.Equal(JTokenType.String, dataType.Type);
        Assert.Equal("response", dataType.Value<string>());
    }
}
