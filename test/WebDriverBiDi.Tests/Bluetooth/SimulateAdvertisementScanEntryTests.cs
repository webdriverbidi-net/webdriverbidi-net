namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SimulateAdvertisementScanEntryTests
{
    [Fact]
    public void TestCanSerialize()
    {
        SimulateAdvertisementScanEntry properties = new("08:08:08:08:08", -10.1, new ScanRecord());
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        // ScanRecord serialization is tested in its own set of tests,
        // so its serialized structure need not be fully verified here.
        Assert.True(serialized.ContainsKey("address"));
        JToken? address = serialized["address"];
        Assert.NotNull(address);
        Assert.Equal(JTokenType.String, address.Type);
        Assert.Equal("08:08:08:08:08", address.Value<string>());

        Assert.True(serialized.ContainsKey("rssi"));
        JToken? rssi = serialized["rssi"];
        Assert.NotNull(rssi);
        Assert.Equal(JTokenType.Float, rssi.Type);
        Assert.Equal(-10.1, rssi.Value<double>());

        Assert.True(serialized.ContainsKey("scanRecord"));
        JToken? scanRecord = serialized["scanRecord"];
        Assert.NotNull(scanRecord);
        Assert.Equal(JTokenType.Object, scanRecord.Type);
    }
}
