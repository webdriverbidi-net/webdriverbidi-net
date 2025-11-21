namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SimulateCharacteristicCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        SimulateCharacteristicCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicType.Add);
        Assert.That(properties.MethodName, Is.EqualTo("bluetooth.simulateCharacteristic"));
    }

    [Test]
    public void TestCanSerializeParametersForAddingCharacteristic()
    {
        SimulateCharacteristicCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicType.Add);
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
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("myAddress"));
            Assert.That(serialized, Contains.Key("serviceUuid"));
            Assert.That(serialized["serviceUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["serviceUuid"]!.Value<string>(), Is.EqualTo("myServiceUuid"));
            Assert.That(serialized, Contains.Key("characteristicUuid"));
            Assert.That(serialized["characteristicUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["characteristicUuid"]!.Value<string>(), Is.EqualTo("myCharacteristicUuid"));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("add"));
       }
    }

    [Test]
    public void TestCanSerializeParametersForRemovingCharacteristic()
    {
        SimulateCharacteristicCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicType.Remove);
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
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("myAddress"));
            Assert.That(serialized, Contains.Key("serviceUuid"));
            Assert.That(serialized["serviceUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["serviceUuid"]!.Value<string>(), Is.EqualTo("myServiceUuid"));
            Assert.That(serialized, Contains.Key("characteristicUuid"));
            Assert.That(serialized["characteristicUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["characteristicUuid"]!.Value<string>(), Is.EqualTo("myCharacteristicUuid"));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("remove"));
       }
    }

    [Test]
    public void TestCanSerializeParametersWithEmptyCharacteristicProperties()
    {
        SimulateCharacteristicCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicType.Add)
        {
            CharacteristicProperties = new()
        };
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
            Assert.That(serialized, Contains.Key("characteristicProperties"));
            Assert.That(serialized["characteristicProperties"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized["characteristicProperties"]!.Value<JObject>(), Is.Empty);
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("add"));
       }
    }

    [Test]
    public void TestCanSerializeParametersWithCharacteristicProperties()
    {
        SimulateCharacteristicCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicType.Add)
        {
            CharacteristicProperties = new()
            {
                IsRead = true,
            }
        };
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
            Assert.That(serialized, Contains.Key("characteristicProperties"));
            Assert.That(serialized["characteristicProperties"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? characteristicProperties = serialized["characteristicProperties"] as JObject;
            Assert.That(characteristicProperties, Has.Count.EqualTo(1));
            Assert.That(characteristicProperties, Contains.Key("read"));
            Assert.That(characteristicProperties!["read"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(characteristicProperties!["read"]!.Value<bool>(), Is.True);
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("add"));
       }
    }
}
