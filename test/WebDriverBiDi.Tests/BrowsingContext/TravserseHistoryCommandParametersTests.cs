namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class TraverseHistoryCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        TraverseHistoryCommandParameters properties = new("myContextId", 0);
        Assert.That(properties.MethodName, Is.EqualTo("browsingContext.traverseHistory"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        TraverseHistoryCommandParameters properties = new("myContextId", 0);
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("delta"));
            Assert.That(serialized["delta"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["delta"]!.Value<long>(), Is.EqualTo(0));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithPositiveDelta()
    {
        TraverseHistoryCommandParameters properties = new("myContextId", 1);
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("delta"));
            Assert.That(serialized["delta"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["delta"]!.Value<long>(), Is.EqualTo(1));
         });
    }

    [Test]
    public void TestCanSerializeParametersWithNegativeDelta()
    {
        TraverseHistoryCommandParameters properties = new("myContextId", -1);
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("delta"));
            Assert.That(serialized["delta"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["delta"]!.Value<long>(), Is.EqualTo(-1));
        });
    }
}
