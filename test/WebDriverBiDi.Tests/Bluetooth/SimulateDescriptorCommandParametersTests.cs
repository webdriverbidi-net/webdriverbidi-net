namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SimulateDescriptorCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SimulateDescriptorCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", "myDescriptorUuid", SimulateDescriptorType.Add);
        Assert.Equal("bluetooth.simulateDescriptor", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParametersForAddingDescriptor()
    {
        SimulateDescriptorCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", "myDescriptorUuid", SimulateDescriptorType.Add);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(6, serialized.Count);

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

        Assert.True(serialized.ContainsKey("serviceUuid"));
        JToken? serviceUuid = serialized["serviceUuid"];
        Assert.NotNull(serviceUuid);
        Assert.Equal(JTokenType.String, serviceUuid.Type);
        Assert.Equal("myServiceUuid", serviceUuid.Value<string>());

        Assert.True(serialized.ContainsKey("characteristicUuid"));
        JToken? characteristicUuid = serialized["characteristicUuid"];
        Assert.NotNull(characteristicUuid);
        Assert.Equal(JTokenType.String, characteristicUuid.Type);
        Assert.Equal("myCharacteristicUuid", characteristicUuid.Value<string>());

        Assert.True(serialized.ContainsKey("descriptorUuid"));
        JToken? descriptorUuid = serialized["descriptorUuid"];
        Assert.NotNull(descriptorUuid);
        Assert.Equal(JTokenType.String, descriptorUuid.Type);
        Assert.Equal("myDescriptorUuid", descriptorUuid.Value<string>());

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("add", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersForRemovingDescriptor()
    {
        SimulateDescriptorCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", "myDescriptorUuid", SimulateDescriptorType.Remove);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(6, serialized.Count);

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

        Assert.True(serialized.ContainsKey("serviceUuid"));
        JToken? serviceUuid = serialized["serviceUuid"];
        Assert.NotNull(serviceUuid);
        Assert.Equal(JTokenType.String, serviceUuid.Type);
        Assert.Equal("myServiceUuid", serviceUuid.Value<string>());

        Assert.True(serialized.ContainsKey("characteristicUuid"));
        JToken? characteristicUuid = serialized["characteristicUuid"];
        Assert.NotNull(characteristicUuid);
        Assert.Equal(JTokenType.String, characteristicUuid.Type);
        Assert.Equal("myCharacteristicUuid", characteristicUuid.Value<string>());

        Assert.True(serialized.ContainsKey("descriptorUuid"));
        JToken? descriptorUuid = serialized["descriptorUuid"];
        Assert.NotNull(descriptorUuid);
        Assert.Equal(JTokenType.String, descriptorUuid.Type);
        Assert.Equal("myDescriptorUuid", descriptorUuid.Value<string>());

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("remove", type.Value<string>());
    }
}
