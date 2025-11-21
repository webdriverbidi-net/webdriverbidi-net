namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class ScanRecordTests
{
    [Test]
    public void TestCanSerialize()
    {
        ScanRecord properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(0));
    }

    [Test]
    public void TestCanSerializeWithOptionalData()
    {
        ScanRecord properties = new()
        {
            Name = "myName",
            Appearance = 123,
            ManufacturerData = [new BluetoothManufacturerData(456, "myManufacturerData")],
            UUIDs = ["my-service-uuid"]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        using (Assert.EnterMultipleScope())
        {
            // BluetoothManufacturerData serialization is tested in its own set of tests,
            // so its serialized structure need not be fully verified here.
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("myName"));
            Assert.That(serialized, Contains.Key("uuids"));
            Assert.That(serialized["uuids"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["uuids"]!.Value<JArray>(), Has.Count.EqualTo(1));
            Assert.That(serialized["uuids"]![0]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["uuids"]![0]!.Value<string>(), Is.EqualTo("my-service-uuid"));
            Assert.That(serialized, Contains.Key("appearance"));
            Assert.That(serialized["appearance"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["appearance"]!.Value<uint>(), Is.EqualTo(123));
            Assert.That(serialized, Contains.Key("manufacturerData"));
            Assert.That(serialized["manufacturerData"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["manufacturerData"]!.Value<JArray>(), Has.Count.EqualTo(1));
            Assert.That(serialized["manufacturerData"]![0]!.Type, Is.EqualTo(JTokenType.Object));
        }
    }
}
