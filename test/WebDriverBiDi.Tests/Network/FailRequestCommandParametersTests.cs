namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class FailRequestCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        FailRequestCommandParameters properties = new("requestId");
        Assert.That(properties.MethodName, Is.EqualTo("network.failRequest"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        FailRequestCommandParameters properties = new("requestId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("request"));
            Assert.That(serialized["request"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["request"]!.Value<string>(), Is.EqualTo("requestId"));
        });
    }
}
