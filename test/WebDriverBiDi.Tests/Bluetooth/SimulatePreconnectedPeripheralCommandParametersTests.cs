namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SimulatePreconnectedPeripheralCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SimulatePreconnectedPeripheralCommandParameters properties = new("myContext", "AD:D2:E5:55", "myPeripheral");
        Assert.Equal("bluetooth.simulatePreconnectedPeripheral", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SimulatePreconnectedPeripheralCommandParameters properties = new("myContext", "AD:D2:E5:55", "myPeripheral");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(5, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContext", context.Value<string>());

        Assert.True(serialized.ContainsKey("address"));
        JToken? address = serialized["address"];
        Assert.NotNull(address);
        Assert.Equal(JTokenType.String, address.Type);
        Assert.Equal("AD:D2:E5:55", address.Value<string>());

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("myPeripheral", name.Value<string>());

        Assert.True(serialized.ContainsKey("manufacturerData"));
        JToken? manufacturerDataToken = serialized["manufacturerData"];
        Assert.NotNull(manufacturerDataToken);
        Assert.Equal(JTokenType.Array, manufacturerDataToken.Type);
        JArray? manufacturerData = manufacturerDataToken.Value<JArray>();
        Assert.NotNull(manufacturerData);
        Assert.Empty(manufacturerData);

        Assert.True(serialized.ContainsKey("knownServiceUuids"));
        JToken? knownServiceUuidsToken = serialized["knownServiceUuids"];
        Assert.NotNull(knownServiceUuidsToken);
        Assert.Equal(JTokenType.Array, knownServiceUuidsToken.Type);
        JArray? knownServiceUuids = knownServiceUuidsToken.Value<JArray>();
        Assert.NotNull(knownServiceUuids);
        Assert.Empty(knownServiceUuids);
    }

    [Fact]
    public void TestCanSerializeParametersWithListData()
    {
        SimulatePreconnectedPeripheralCommandParameters properties = new("myContext", "AD:D2:E5:55", "myPeripheral");
        properties.ManufacturerData.Add(new BluetoothManufacturerData(123, "myData"));
        properties.KnownServiceUUIDs.Add("my-known-uuid");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(5, serialized.Count);

        // BluetoothManufacturerData serialization is tested in its own set of tests,
        // so its serialized structure need not be fully verified here.
        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContext", context.Value<string>());

        Assert.True(serialized.ContainsKey("address"));
        JToken? address = serialized["address"];
        Assert.NotNull(address);
        Assert.Equal(JTokenType.String, address.Type);
        Assert.Equal("AD:D2:E5:55", address.Value<string>());

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("myPeripheral", name.Value<string>());

        Assert.True(serialized.ContainsKey("manufacturerData"));
        JToken? manufacturerDataToken = serialized["manufacturerData"];
        Assert.NotNull(manufacturerDataToken);
        Assert.Equal(JTokenType.Array, manufacturerDataToken.Type);
        JArray? manufacturerData = manufacturerDataToken.Value<JArray>();
        Assert.NotNull(manufacturerData);
        Assert.Single(manufacturerData);
        Assert.Equal(JTokenType.Object, manufacturerData[0].Type);

        Assert.True(serialized.ContainsKey("knownServiceUuids"));
        JToken? knownServiceUuidsToken = serialized["knownServiceUuids"];
        Assert.NotNull(knownServiceUuidsToken);
        Assert.Equal(JTokenType.Array, knownServiceUuidsToken.Type);
        JArray? knownServiceUuids = knownServiceUuidsToken.Value<JArray>();
        Assert.NotNull(knownServiceUuids);
        Assert.Single(knownServiceUuids);
        Assert.Equal(JTokenType.String, knownServiceUuids[0].Type);
        Assert.Equal("my-known-uuid", knownServiceUuids[0].Value<string>());
    }
}
