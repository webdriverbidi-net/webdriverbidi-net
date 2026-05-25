namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SimulateCharacteristicCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SimulateCharacteristicCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicType.Add);
        Assert.Equal("bluetooth.simulateCharacteristic", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParametersForAddingCharacteristic()
    {
        SimulateCharacteristicCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicType.Add);
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

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("add", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersForRemovingCharacteristic()
    {
        SimulateCharacteristicCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicType.Remove);
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

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("remove", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithEmptyCharacteristicProperties()
    {
        SimulateCharacteristicCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicType.Add)
        {
            CharacteristicProperties = new()
        };
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

        Assert.True(serialized.ContainsKey("characteristicProperties"));
        JToken? characteristicProperties = serialized["characteristicProperties"];
        Assert.NotNull(characteristicProperties);
        Assert.Equal(JTokenType.Object, characteristicProperties.Type);

        JObject? characteristicPropertiesObject = characteristicProperties.Value<JObject>();
        Assert.NotNull(characteristicPropertiesObject);
        Assert.Empty(characteristicPropertiesObject);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("add", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithCharacteristicProperties()
    {
        SimulateCharacteristicCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicType.Add)
        {
            CharacteristicProperties = new()
            {
                IsRead = true,
            }
        };
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

        Assert.True(serialized.ContainsKey("characteristicProperties"));
        JToken? characteristicProperties = serialized["characteristicProperties"];
        Assert.NotNull(characteristicProperties);
        Assert.Equal(JTokenType.Object, characteristicProperties.Type);

        JObject? characteristicPropertiesObject = serialized["characteristicProperties"] as JObject;
        Assert.NotNull(characteristicPropertiesObject);
        Assert.True(characteristicPropertiesObject.ContainsKey("read"));
        JToken? read = characteristicPropertiesObject["read"];
        Assert.NotNull(read);
        Assert.Equal(JTokenType.Boolean, read.Type);
        Assert.True(read.Value<bool>());

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("add", type.Value<string>());
    }
}
