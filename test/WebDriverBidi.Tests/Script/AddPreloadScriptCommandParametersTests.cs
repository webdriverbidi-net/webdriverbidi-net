namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class AddPreloadScriptCommandParametersTests
{
    [Test]
    public void TestCanSerializeProperties()
    {
        AddPreloadScriptCommandParameters properties = new("myExpression");
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("expression"));
            Assert.That(serialized["expression"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["expression"]!.Value<string>(), Is.EqualTo("myExpression"));
        });
    }

    [Test]
    public void TestCanSerializePropertiesWithSandbox()
    {
        AddPreloadScriptCommandParameters properties = new("myExpression")
        {
            Sandbox = "mySandbox"
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("expression"));
            Assert.That(serialized["expression"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["expression"]!.Value<string>(), Is.EqualTo("myExpression"));
            Assert.That(serialized, Contains.Key("sandbox"));
            Assert.That(serialized["sandbox"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["sandbox"]!.Value<string>(), Is.EqualTo("mySandbox"));
        });
    }
}