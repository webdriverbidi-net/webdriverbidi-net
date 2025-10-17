namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SimulateServiceCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        SimulateServiceCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", SimulateServiceType.Add);
        Assert.That(properties.MethodName, Is.EqualTo("bluetooth.simulateService"));
    }

    [Test]
    public void TestCanSerializeParametersForAddingService()
    {
        SimulateServiceCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", SimulateServiceType.Add);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("address"));
            Assert.That(serialized["address"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("myAddress"));
            Assert.That(serialized, Contains.Key("uuid"));
            Assert.That(serialized["uuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["uuid"]!.Value<string>(), Is.EqualTo("myServiceUuid"));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("add"));
       });
    }

    [Test]
    public void TestCanSerializeParametersForRemovingService()
    {
        SimulateServiceCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", SimulateServiceType.Remove);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("address"));
            Assert.That(serialized["address"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("myAddress"));
            Assert.That(serialized, Contains.Key("uuid"));
            Assert.That(serialized["uuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["uuid"]!.Value<string>(), Is.EqualTo("myServiceUuid"));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("remove"));
       });
    }
}
