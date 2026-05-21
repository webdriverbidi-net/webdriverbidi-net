namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class RemoveDataCollectorCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        RemoveDataCollectorCommandParameters properties = new("myCollectorId");
        Assert.Equal("network.removeDataCollector", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        RemoveDataCollectorCommandParameters properties = new("myCollectorId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("collector"));
        JToken? collector = serialized["collector"];
        Assert.NotNull(collector);
        Assert.Equal(JTokenType.String, collector.Type);
        Assert.Equal("myCollectorId", collector.Value<string>());
    }
}
