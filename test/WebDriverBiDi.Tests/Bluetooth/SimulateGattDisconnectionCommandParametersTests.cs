namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SimulateGattDisconnectionCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SimulateGattDisconnectionCommandParameters properties = new("myContext", "myAddress");
        Assert.Equal("bluetooth.simulateGattDisconnection", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SimulateGattDisconnectionCommandParameters properties = new("myContext", "myAddress");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContext", context.Value<string>());

        Assert.True(serialized.ContainsKey("address"));
        JToken? address = serialized["address"];
        Assert.NotNull(address);
        Assert.Equal(JTokenType.String, address.Type);
        Assert.Equal("myAddress", address.Value<string>());
    }
}
