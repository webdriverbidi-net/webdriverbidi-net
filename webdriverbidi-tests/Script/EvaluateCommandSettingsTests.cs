namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class EvaluateCommandSettingsTests
{
    [Test]
    public void TestCanSerializeSettings()
    {
        EvaluateCommandSettings properties = new("myExpression", new RealmTarget("myRealm"), true);
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("expression"));
            Assert.That(serialized["expression"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized.ContainsKey("target"));
            Assert.That(serialized["target"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized.ContainsKey("awaitPromise"));
            Assert.That(serialized["awaitPromise"]!.Type, Is.EqualTo(JTokenType.Boolean));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalValues()
    {
        EvaluateCommandSettings properties = new("myExpression", new RealmTarget("myRealm"), true)
        {
            OwnershipModel = OwnershipModel.None
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("expression"));
            Assert.That(serialized["expression"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized.ContainsKey("target"));
            Assert.That(serialized["target"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized.ContainsKey("awaitPromise"));
            Assert.That(serialized["awaitPromise"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized.ContainsKey("resultOwnership"));
            Assert.That(serialized["resultOwnership"]!.Type, Is.EqualTo(JTokenType.String));
        });
    }
}