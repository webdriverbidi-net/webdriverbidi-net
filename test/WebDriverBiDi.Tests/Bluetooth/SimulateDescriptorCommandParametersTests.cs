namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SimulateDescriptorCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        SimulateDescriptorCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", "myDescriptorUuid", SimulateDescriptorType.Add);
        Assert.That(properties.MethodName, Is.EqualTo("bluetooth.simulateDescriptor"));
    }

    [Test]
    public void TestCanSerializeParametersForAddingDescriptor()
    {
        SimulateDescriptorCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", "myDescriptorUuid", SimulateDescriptorType.Add);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(6));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("address"));
            Assert.That(serialized["address"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("myAddress"));
            Assert.That(serialized, Contains.Key("serviceUuid"));
            Assert.That(serialized["serviceUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["serviceUuid"]!.Value<string>(), Is.EqualTo("myServiceUuid"));
            Assert.That(serialized, Contains.Key("characteristicUuid"));
            Assert.That(serialized["characteristicUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["characteristicUuid"]!.Value<string>(), Is.EqualTo("myCharacteristicUuid"));
            Assert.That(serialized, Contains.Key("descriptorUuid"));
            Assert.That(serialized["descriptorUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["descriptorUuid"]!.Value<string>(), Is.EqualTo("myDescriptorUuid"));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("add"));
       }
    }

    [Test]
    public void TestCanSerializeParametersForRemovingDescriptor()
    {
        SimulateDescriptorCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", "myDescriptorUuid", SimulateDescriptorType.Remove);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(6));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("address"));
            Assert.That(serialized["address"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("myAddress"));
            Assert.That(serialized, Contains.Key("serviceUuid"));
            Assert.That(serialized["serviceUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["serviceUuid"]!.Value<string>(), Is.EqualTo("myServiceUuid"));
            Assert.That(serialized, Contains.Key("characteristicUuid"));
            Assert.That(serialized["characteristicUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["characteristicUuid"]!.Value<string>(), Is.EqualTo("myCharacteristicUuid"));
            Assert.That(serialized, Contains.Key("descriptorUuid"));
            Assert.That(serialized["descriptorUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["descriptorUuid"]!.Value<string>(), Is.EqualTo("myDescriptorUuid"));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("remove"));
       }
    }
}
