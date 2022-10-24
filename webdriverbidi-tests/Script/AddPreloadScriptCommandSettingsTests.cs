namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class AddPreloadScriptCommandSettingsTests
{
    [Test]
    public void TestCanSerializeProperties()
    {
        var properties = new AddPreloadScriptCommandSettings("myExpression");
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("expression"));
        Assert.That(serialized["expression"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["expression"]!.Value<string>(), Is.EqualTo("myExpression"));
    }

    [Test]
    public void TestCanSerializePropertiesWithSandbox()
    {
        var properties = new AddPreloadScriptCommandSettings("myExpression");
        properties.Sandbox = "mySandbox";
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(2));
        Assert.That(serialized.ContainsKey("expression"));
        Assert.That(serialized["expression"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["expression"]!.Value<string>(), Is.EqualTo("myExpression"));
        Assert.That(serialized.ContainsKey("sandbox"));
        Assert.That(serialized["sandbox"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["sandbox"]!.Value<string>(), Is.EqualTo("mySandbox"));
    }
}