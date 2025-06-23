namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class RemoveDataCollectorCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        RemoveDataCollectorCommandParameters properties = new("myCollectorId");
        Assert.That(properties.MethodName, Is.EqualTo("network.removeDataCollector"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        RemoveDataCollectorCommandParameters properties = new("myCollectorId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("collector"));
            Assert.That(serialized["collector"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["collector"]!.Value<string>(), Is.EqualTo("myCollectorId"));
        });
    }
}
