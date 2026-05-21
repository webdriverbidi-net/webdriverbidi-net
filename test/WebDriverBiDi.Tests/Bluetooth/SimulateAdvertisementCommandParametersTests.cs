namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SimulateAdvertisementCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SimulateAdvertisementCommandParameters properties = new("myContext", new SimulateAdvertisementScanEntry("08:08:08:08:08", -10.1, new ScanRecord()));
        Assert.Equal("bluetooth.simulateAdvertisement", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SimulateAdvertisementCommandParameters properties = new("myContext", new SimulateAdvertisementScanEntry("08:08:08:08:08", -10.1, new ScanRecord()));
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        // SimulateAdvertisementScanEntry serialization is tested in its own set of tests,
        // so its serialized structure need not be fully verified here.
        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContext", context.Value<string>());

        Assert.True(serialized.ContainsKey("scanEntry"));
        JToken? scanEntry = serialized["scanEntry"];
        Assert.NotNull(scanEntry);
        Assert.Equal(JTokenType.Object, scanEntry.Type);
    }
}
