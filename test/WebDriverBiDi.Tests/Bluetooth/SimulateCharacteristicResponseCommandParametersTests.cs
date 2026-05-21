namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SimulateCharacteristicResponseCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SimulateCharacteristicResponseCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicResponseType.Read, 0);
        Assert.Equal("bluetooth.simulateCharacteristicResponse", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParametersForCharacteristicReadResponse()
    {
        SimulateCharacteristicResponseCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicResponseType.Read, 0);
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

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("read", type.Value<string>());

        Assert.True(serialized.ContainsKey("code"));
        JToken? code = serialized["code"];
        Assert.NotNull(code);
        Assert.Equal(JTokenType.Integer, code.Type);
        Assert.Equal(0U, code.Value<uint>());
    }

    [Fact]
    public void TestCanSerializeParametersForCharacteristicWriteResponse()
    {
        SimulateCharacteristicResponseCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicResponseType.Write, 0);
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

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("write", type.Value<string>());

        Assert.True(serialized.ContainsKey("code"));
        JToken? code = serialized["code"];
        Assert.NotNull(code);
        Assert.Equal(JTokenType.Integer, code.Type);
        Assert.Equal(0U, code.Value<uint>());
    }

    [Fact]
    public void TestCanSerializeParametersForCharacteristicSubscribeResponse()
    {
        SimulateCharacteristicResponseCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicResponseType.SubscribeToNotifications, 0);
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

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("subscribe-to-notifications", type.Value<string>());

        Assert.True(serialized.ContainsKey("code"));
        JToken? code = serialized["code"];
        Assert.NotNull(code);
        Assert.Equal(JTokenType.Integer, code.Type);
        Assert.Equal(0U, code.Value<uint>());
    }

    [Fact]
    public void TestCanSerializeParametersForCharacteristicUnsubscribeResponse()
    {
        SimulateCharacteristicResponseCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicResponseType.UnsubscribeFromNotifications, 0);
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

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("unsubscribe-from-notifications", type.Value<string>());

        Assert.True(serialized.ContainsKey("code"));
        JToken? code = serialized["code"];
        Assert.NotNull(code);
        Assert.Equal(JTokenType.Integer, code.Type);
        Assert.Equal(0U, code.Value<uint>());
    }

    [Fact]
    public void TestCanSerializeParametersForCharacteristicResponseWithEmptyData()
    {
        SimulateCharacteristicResponseCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicResponseType.Read, 0)
        {
            Data = [],
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(7, serialized.Count);

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
        Assert.Equal("read", type.Value<string>());

        Assert.True(serialized.ContainsKey("code"));
        JToken? code = serialized["code"];
        Assert.NotNull(code);
        Assert.Equal(JTokenType.Integer, code.Type);
        Assert.Equal(0u, code.Value<uint>());

        Assert.True(serialized.ContainsKey("data"));
        JToken? data = serialized["data"];
        Assert.NotNull(data);
        Assert.Equal(JTokenType.Array, data.Type);

        JArray? dataArray = serialized["data"] as JArray;
        Assert.NotNull(dataArray);
        Assert.Empty(dataArray);
    }

    [Fact]
    public void TestCanSerializeParametersForCharacteristicResponseWithData()
    {
        SimulateCharacteristicResponseCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicResponseType.Read, 0)
        {
            Data = [123, 456],
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(7, serialized.Count);

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
        Assert.Equal("read", type.Value<string>());

        Assert.True(serialized.ContainsKey("code"));
        JToken? code = serialized["code"];
        Assert.NotNull(code);
        Assert.Equal(JTokenType.Integer, code.Type);
        Assert.Equal(0U, code.Value<uint>());

        Assert.True(serialized.ContainsKey("data"));
        JToken? data = serialized["data"];
        Assert.NotNull(data);
        Assert.Equal(JTokenType.Array, data.Type);

        JArray? dataArray = serialized["data"] as JArray;
        Assert.NotNull(dataArray);
        Assert.Equal(2, dataArray.Count);
        Assert.Equal(JTokenType.Integer, dataArray[0].Type);
        Assert.Equal(123U, dataArray[0].Value<uint>());
        Assert.Equal(JTokenType.Integer, dataArray[1].Type);
        Assert.Equal(456U, dataArray[1].Value<uint>());
    }
}
