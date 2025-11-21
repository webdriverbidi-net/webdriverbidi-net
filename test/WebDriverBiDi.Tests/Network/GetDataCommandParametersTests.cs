namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class GetDataCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        GetDataCommandParameters properties = new("myRequestId");
        Assert.That(properties.MethodName, Is.EqualTo("network.getData"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        GetDataCommandParameters properties = new("myRequestId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("request"));
            Assert.That(serialized["request"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["request"]!.Value<string>(), Is.EqualTo("myRequestId"));
            Assert.That(serialized, Contains.Key("dataType"));
            Assert.That(serialized["dataType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["dataType"]!.Value<string>(), Is.EqualTo("response"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithCollectorId()
    {
        GetDataCommandParameters properties = new("myRequestId")
        {
            CollectorId = "myCollectorId"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
            Assert.That(serialized, Contains.Key("request"));
            Assert.That(serialized["request"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["request"]!.Value<string>(), Is.EqualTo("myRequestId"));
            Assert.That(serialized, Contains.Key("dataType"));
            Assert.That(serialized["dataType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["dataType"]!.Value<string>(), Is.EqualTo("response"));
            Assert.That(serialized, Contains.Key("collector"));
            Assert.That(serialized["collector"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["collector"]!.Value<string>(), Is.EqualTo("myCollectorId"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithDisownDataTrue()
    {
        GetDataCommandParameters properties = new("myRequestId")
        {
            CollectorId = "myCollectorId",
            DisownCollectedData = true,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(4));
            Assert.That(serialized, Contains.Key("request"));
            Assert.That(serialized["request"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["request"]!.Value<string>(), Is.EqualTo("myRequestId"));
            Assert.That(serialized, Contains.Key("dataType"));
            Assert.That(serialized["dataType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["dataType"]!.Value<string>(), Is.EqualTo("response"));
            Assert.That(serialized, Contains.Key("collector"));
            Assert.That(serialized["collector"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["collector"]!.Value<string>(), Is.EqualTo("myCollectorId"));
            Assert.That(serialized, Contains.Key("disown"));
            Assert.That(serialized["disown"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["disown"]!.Value<bool>(), Is.True);
        }
    }

    [Test]
    public void TestCanSerializeParametersWithDisownDataFalse()
    {
        GetDataCommandParameters properties = new("myRequestId")
        {
            CollectorId = "myCollectorId",
            DisownCollectedData = false,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(4));
            Assert.That(serialized, Contains.Key("request"));
            Assert.That(serialized["request"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["request"]!.Value<string>(), Is.EqualTo("myRequestId"));
            Assert.That(serialized, Contains.Key("dataType"));
            Assert.That(serialized["dataType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["dataType"]!.Value<string>(), Is.EqualTo("response"));
            Assert.That(serialized, Contains.Key("collector"));
            Assert.That(serialized["collector"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["collector"]!.Value<string>(), Is.EqualTo("myCollectorId"));
            Assert.That(serialized, Contains.Key("disown"));
            Assert.That(serialized["disown"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["disown"]!.Value<bool>(), Is.False);
        }
    }
}
