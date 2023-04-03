namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class EvaluateCommandParametersTests
{
    [Test]
    public void TestCanSerializeParameters()
    {
        EvaluateCommandParameters properties = new("myExpression", new RealmTarget("myRealm"), true);
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("expression"));
            Assert.That(serialized["expression"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized, Contains.Key("target"));
            Assert.That(serialized["target"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized, Contains.Key("awaitPromise"));
            Assert.That(serialized["awaitPromise"]!.Type, Is.EqualTo(JTokenType.Boolean));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalValues()
    {
        EvaluateCommandParameters properties = new("myExpression", new RealmTarget("myRealm"), true)
        {
            OwnershipModel = OwnershipModel.None
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("expression"));
            Assert.That(serialized["expression"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized, Contains.Key("target"));
            Assert.That(serialized["target"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized, Contains.Key("awaitPromise"));
            Assert.That(serialized["awaitPromise"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized, Contains.Key("resultOwnership"));
            Assert.That(serialized["resultOwnership"]!.Type, Is.EqualTo(JTokenType.String));
        });
    }
}