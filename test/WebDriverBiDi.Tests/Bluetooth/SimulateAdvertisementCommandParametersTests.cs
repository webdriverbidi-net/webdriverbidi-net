namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SimulateAdvertisementCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        SimulateAdvertisementCommandParameters properties = new("myContext", new SimulateAdvertisementScanEntry("08:08:08:08:08", -10.1, new ScanRecord()));
        Assert.That(properties.MethodName, Is.EqualTo("bluetooth.simulateAdvertisement"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SimulateAdvertisementCommandParameters properties = new("myContext", new SimulateAdvertisementScanEntry("08:08:08:08:08", -10.1, new ScanRecord()));
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            // SimulateAdvertisementScanEntry serialization is tested in its own set of tests,
            // so its serialized structure need not be fully verified here.
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("scanEntry"));
            Assert.That(serialized["scanEntry"]!.Type, Is.EqualTo(JTokenType.Object));
        });
    }
}
