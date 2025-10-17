namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SimulateGattDisconnectionCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        SimulateGattDisconnectionCommandParameters properties = new("myContext", "myAddress");
        Assert.That(properties.MethodName, Is.EqualTo("bluetooth.simulateGattDisconnection"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SimulateGattDisconnectionCommandParameters properties = new("myContext", "myAddress");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("address"));
            Assert.That(serialized["address"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("myAddress"));
       });
    }
}
