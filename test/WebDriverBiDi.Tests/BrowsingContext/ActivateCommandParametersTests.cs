namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class ActivateCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        ActivateCommandParameters properties = new("myContextId");
        Assert.That(properties.MethodName, Is.EqualTo("browsingContext.activate"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        ActivateCommandParameters properties = new("myContextId");
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        });
    }
}
