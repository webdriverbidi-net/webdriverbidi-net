namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SimulatePreconnectedPeripheralCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        SimulatePreconnectedPeripheralCommandParameters properties = new("myContext", "AD:D2:E5:55", "myPeripheral");
        Assert.That(properties.MethodName, Is.EqualTo("bluetooth.simulatePreconnectedPeripheral"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SimulatePreconnectedPeripheralCommandParameters properties = new("myContext", "AD:D2:E5:55", "myPeripheral");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(5));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("address"));
            Assert.That(serialized["address"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("AD:D2:E5:55"));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("myPeripheral"));
            Assert.That(serialized, Contains.Key("manufacturerData"));
            Assert.That(serialized["manufacturerData"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["manufacturerData"]!.Value<JArray>(), Is.Empty);
            Assert.That(serialized, Contains.Key("knownServiceUuids"));
            Assert.That(serialized["knownServiceUuids"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["knownServiceUuids"]!.Value<JArray>(), Is.Empty);
        }
    }

    [Test]
    public void TestCanSerializeParametersWithListData()
    {
        SimulatePreconnectedPeripheralCommandParameters properties = new("myContext", "AD:D2:E5:55", "myPeripheral");
        properties.ManufacturerData.Add(new BluetoothManufacturerData(123, "myData"));
        properties.KnownServiceUUIDs.Add("my-known-uuid");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(5));
        using (Assert.EnterMultipleScope())
        {
            // BluetoothManufacturerData serialization is tested in its own set of tests,
            // so its serialized structure need not be fully verified here.
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("address"));
            Assert.That(serialized["address"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("AD:D2:E5:55"));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("myPeripheral"));
            Assert.That(serialized, Contains.Key("manufacturerData"));
            Assert.That(serialized["manufacturerData"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["manufacturerData"]!.Value<JArray>(), Has.Count.EqualTo(1));
            Assert.That(serialized["manufacturerData"]![0]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized, Contains.Key("knownServiceUuids"));
            Assert.That(serialized["knownServiceUuids"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["knownServiceUuids"]!.Value<JArray>(), Has.Count.EqualTo(1));
            Assert.That(serialized["knownServiceUuids"]![0]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["knownServiceUuids"]![0]!.Value<string>(), Is.EqualTo("my-known-uuid"));
        }
    }
}
