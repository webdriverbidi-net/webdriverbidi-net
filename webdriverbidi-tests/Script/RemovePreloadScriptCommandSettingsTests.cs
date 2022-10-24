namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class RemovePreloadScriptCommandSettingsTests
{
    [Test]
    public void TestCanSerializeProperties()
    {
        var properties = new RemovePreloadScriptCommandSettings("myLoadScriptId");
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("script"));
        Assert.That(serialized["script"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["script"]!.Value<string>(), Is.EqualTo("myLoadScriptId"));
    }
}