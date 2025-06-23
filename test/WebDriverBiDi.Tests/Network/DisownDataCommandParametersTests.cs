namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class DisownDataCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        DisownDataCommandParameters properties = new("myCollectorId", "myRequestId");
        Assert.That(properties.MethodName, Is.EqualTo("network.disownData"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        DisownDataCommandParameters properties = new("myCollectorId", "myRequestId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
            Assert.That(serialized, Contains.Key("collector"));
            Assert.That(serialized["collector"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["collector"]!.Value<string>(), Is.EqualTo("myCollectorId"));
            Assert.That(serialized, Contains.Key("request"));
            Assert.That(serialized["request"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["request"]!.Value<string>(), Is.EqualTo("myRequestId"));
            Assert.That(serialized, Contains.Key("dataType"));
            Assert.That(serialized["dataType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["dataType"]!.Value<string>(), Is.EqualTo("response"));
        });
    }
}
