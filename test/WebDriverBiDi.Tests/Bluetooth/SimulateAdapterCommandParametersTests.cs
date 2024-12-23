namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SimulateAdapterCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        SimulateAdapterCommandParameters properties = new("myContext", AdapterState.Absent);
        Assert.That(properties.MethodName, Is.EqualTo("bluetooth.simulateAdapter"));
    }

    [Test]
    public void TestCanSerializeParametersWithAbsentState()
    {
        SimulateAdapterCommandParameters properties = new("myContext", AdapterState.Absent);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("state"));
            Assert.That(serialized["state"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["state"]!.Value<string>(), Is.EqualTo("absent"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithPoweredOffState()
    {
        SimulateAdapterCommandParameters properties = new("myContext", AdapterState.PoweredOff);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("state"));
            Assert.That(serialized["state"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["state"]!.Value<string>(), Is.EqualTo("powered-off"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithPoweredOnState()
    {
        SimulateAdapterCommandParameters properties = new("myContext", AdapterState.PoweredOn);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("state"));
            Assert.That(serialized["state"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["state"]!.Value<string>(), Is.EqualTo("powered-on"));
        });
    }
}
