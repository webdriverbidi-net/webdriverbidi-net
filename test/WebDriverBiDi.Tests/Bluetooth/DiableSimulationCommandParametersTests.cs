
namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class DisableSimulationCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        DisableSimulationCommandParameters properties = new("myContext");
        Assert.Equal("bluetooth.disableSimulation", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        DisableSimulationCommandParameters properties = new("myContext");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContext", context.Value<string>());
    }
}
