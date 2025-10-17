namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CharacteristicPropertiesTests
{
    [Test]
    public void TestCanSerialize()
    {
        CharacteristicProperties properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(0));
    }
    
    [Test]
    public void TestCanSerializeWithAllValuesTrue()
    {
        CharacteristicProperties properties = new()
        {
            IsBroadcast = true,
            IsRead = true,
            IsWriteWithoutResponse = true,
            IsWrite = true,
            IsNotify = true,
            IsIndicate = true,
            IsAuthenticatedSignedWrites = true,
            IsExtendedProperties = true,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(8));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("broadcast"));
            Assert.That(serialized["broadcast"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["broadcast"]!.Value<bool>(), Is.True);
            Assert.That(serialized, Contains.Key("read"));
            Assert.That(serialized["read"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["read"]!.Value<bool>(), Is.True);
            Assert.That(serialized, Contains.Key("writeWithoutResponse"));
            Assert.That(serialized["writeWithoutResponse"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["writeWithoutResponse"]!.Value<bool>(), Is.True);
            Assert.That(serialized, Contains.Key("write"));
            Assert.That(serialized["write"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["write"]!.Value<bool>(), Is.True);
            Assert.That(serialized, Contains.Key("notify"));
            Assert.That(serialized["notify"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["notify"]!.Value<bool>(), Is.True);
            Assert.That(serialized, Contains.Key("indicate"));
            Assert.That(serialized["indicate"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["indicate"]!.Value<bool>(), Is.True);
            Assert.That(serialized, Contains.Key("authenticatedSignedWrites"));
            Assert.That(serialized["authenticatedSignedWrites"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["authenticatedSignedWrites"]!.Value<bool>(), Is.True);
            Assert.That(serialized, Contains.Key("extendedProperties"));
            Assert.That(serialized["extendedProperties"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["extendedProperties"]!.Value<bool>(), Is.True);
        });
    }
    
    [Test]
    public void TestCanSerializeWithAllValuesFalse()
    {
        CharacteristicProperties properties = new()
        {
            IsBroadcast = false,
            IsRead = false,
            IsWriteWithoutResponse = false,
            IsWrite = false,
            IsNotify = false,
            IsIndicate = false,
            IsAuthenticatedSignedWrites = false,
            IsExtendedProperties = false,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(8));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("broadcast"));
            Assert.That(serialized["broadcast"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["broadcast"]!.Value<bool>(), Is.False);
            Assert.That(serialized, Contains.Key("read"));
            Assert.That(serialized["read"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["read"]!.Value<bool>(), Is.False);
            Assert.That(serialized, Contains.Key("writeWithoutResponse"));
            Assert.That(serialized["writeWithoutResponse"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["writeWithoutResponse"]!.Value<bool>(), Is.False);
            Assert.That(serialized, Contains.Key("write"));
            Assert.That(serialized["write"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["write"]!.Value<bool>(), Is.False);
            Assert.That(serialized, Contains.Key("notify"));
            Assert.That(serialized["notify"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["notify"]!.Value<bool>(), Is.False);
            Assert.That(serialized, Contains.Key("indicate"));
            Assert.That(serialized["indicate"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["indicate"]!.Value<bool>(), Is.False);
            Assert.That(serialized, Contains.Key("authenticatedSignedWrites"));
            Assert.That(serialized["authenticatedSignedWrites"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["authenticatedSignedWrites"]!.Value<bool>(), Is.False);
            Assert.That(serialized, Contains.Key("extendedProperties"));
            Assert.That(serialized["extendedProperties"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["extendedProperties"]!.Value<bool>(), Is.False);
        });
    }
}
