namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SimulateAdapterCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SimulateAdapterCommandParameters properties = new("myContext", AdapterState.Absent);
        Assert.Equal("bluetooth.simulateAdapter", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParametersWithAbsentState()
    {
        SimulateAdapterCommandParameters properties = new("myContext", AdapterState.Absent);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContext", context.Value<string>());

        Assert.True(serialized.ContainsKey("state"));
        JToken? state = serialized["state"];
        Assert.NotNull(state);
        Assert.Equal(JTokenType.String, state.Type);
        Assert.Equal("absent", state.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithPoweredOffState()
    {
        SimulateAdapterCommandParameters properties = new("myContext", AdapterState.PoweredOff);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContext", context.Value<string>());

        Assert.True(serialized.ContainsKey("state"));
        JToken? state = serialized["state"];
        Assert.NotNull(state);
        Assert.Equal(JTokenType.String, state.Type);
        Assert.Equal("powered-off", state.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithPoweredOnState()
    {
        SimulateAdapterCommandParameters properties = new("myContext", AdapterState.PoweredOn);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContext", context.Value<string>());

        Assert.True(serialized.ContainsKey("state"));
        JToken? state = serialized["state"];
        Assert.NotNull(state);
        Assert.Equal(JTokenType.String, state.Type);
        Assert.Equal("powered-on", state.Value<string>());
    }
}
