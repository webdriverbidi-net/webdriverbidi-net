namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SimulateGattConnectionResponseCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        SimulateGattConnectionResponseCommandParameters properties = new("myContext", "myAddress", 0);
        Assert.That(properties.MethodName, Is.EqualTo("bluetooth.simulateGattConnectionResponse"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SimulateGattConnectionResponseCommandParameters properties = new("myContext", "myAddress", 0);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("address"));
            Assert.That(serialized["address"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("myAddress"));
            Assert.That(serialized, Contains.Key("code"));
            Assert.That(serialized["code"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["code"]!.Value<uint>(), Is.Zero);
        });
    }
}
