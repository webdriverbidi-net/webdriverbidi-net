namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SimulateServiceCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SimulateServiceCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", SimulateServiceType.Add);
        Assert.Equal("bluetooth.simulateService", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParametersForAddingService()
    {
        SimulateServiceCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", SimulateServiceType.Add);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

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

        Assert.True(serialized.ContainsKey("uuid"));
        JToken? uuid = serialized["uuid"];
        Assert.NotNull(uuid);
        Assert.Equal(JTokenType.String, uuid.Type);
        Assert.Equal("myServiceUuid", uuid.Value<string>());

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("add", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersForRemovingService()
    {
        SimulateServiceCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", SimulateServiceType.Remove);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

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

        Assert.True(serialized.ContainsKey("uuid"));
        JToken? uuid = serialized["uuid"];
        Assert.NotNull(uuid);
        Assert.Equal(JTokenType.String, uuid.Type);
        Assert.Equal("myServiceUuid", uuid.Value<string>());

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("remove", type.Value<string>());
    }
}
