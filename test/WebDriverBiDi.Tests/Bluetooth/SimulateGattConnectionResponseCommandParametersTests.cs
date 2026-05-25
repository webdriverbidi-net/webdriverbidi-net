namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SimulateGattConnectionResponseCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SimulateGattConnectionResponseCommandParameters properties = new("myContext", "myAddress", 0);
        Assert.Equal("bluetooth.simulateGattConnectionResponse", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SimulateGattConnectionResponseCommandParameters properties = new("myContext", "myAddress", 0);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

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

        Assert.True(serialized.ContainsKey("code"));
        JToken? code = serialized["code"];
        Assert.NotNull(code);
        Assert.Equal(JTokenType.Integer, code.Type);
        Assert.Equal(0U, code.Value<uint>());
    }
}
