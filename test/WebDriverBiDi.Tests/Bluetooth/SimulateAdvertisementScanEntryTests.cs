namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SimulateAdvertisementScanEntryTests
{
    [Test]
    public void TestCanSerialize()
    {
        SimulateAdvertisementScanEntry properties = new("08:08:08:08:08", -10.1, new ScanRecord());
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            // ScanRecord serialization is tested in its own set of tests,
            // so its serialized structure need not be fully verified here.
            Assert.That(serialized, Contains.Key("address"));
            Assert.That(serialized["address"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("08:08:08:08:08"));
            Assert.That(serialized, Contains.Key("rssi"));
            Assert.That(serialized["rssi"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["rssi"]!.Value<double>(), Is.EqualTo(-10.1));
            Assert.That(serialized, Contains.Key("scanRecord"));
            Assert.That(serialized["scanRecord"]!.Type, Is.EqualTo(JTokenType.Object));
        });
    }
}
