
namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class DisableSimulationCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        DisableSimulationCommandParameters properties = new("myContext");
        Assert.That(properties.MethodName, Is.EqualTo("bluetooth.disableSimulation"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        DisableSimulationCommandParameters properties = new("myContext");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
        }
    }
}
