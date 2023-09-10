namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class RemovePreloadScriptCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        RemovePreloadScriptCommandParameters properties = new("myLoadScriptId");
        Assert.That(properties.MethodName, Is.EqualTo("script.removePreloadScript"));
    }

    [Test]
    public void TestCanSerializeProperties()
    {
        RemovePreloadScriptCommandParameters properties = new("myLoadScriptId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("script"));
            Assert.That(serialized["script"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["script"]!.Value<string>(), Is.EqualTo("myLoadScriptId"));
        });
    }
}