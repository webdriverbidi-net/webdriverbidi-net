namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class BluetoothManufacturerDataTests
{
    [Test]
    public void TestCanSerialize()
    {
        BluetoothManufacturerData properties = new(123, "myData");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("key"));
            Assert.That(serialized["key"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["key"]!.Value<uint>(), Is.EqualTo(123));
            Assert.That(serialized, Contains.Key("data"));
            Assert.That(serialized["data"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["data"]!.Value<string>(), Is.EqualTo("myData"));
        }
    }
    
    [Test]
    public void TestCanUpdatePropertiesAfterInstantiation()
    {
        BluetoothManufacturerData properties = new(123, "myData");
        properties.Key = 456;
        properties.Data = "myUpdatedData";
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("key"));
            Assert.That(serialized["key"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["key"]!.Value<uint>(), Is.EqualTo(456));
            Assert.That(serialized, Contains.Key("data"));
            Assert.That(serialized["data"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["data"]!.Value<string>(), Is.EqualTo("myUpdatedData"));
        }
    }
}
