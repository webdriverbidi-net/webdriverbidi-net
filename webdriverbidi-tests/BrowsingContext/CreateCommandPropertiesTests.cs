namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CreateCommandPropertiesTests
{
    [Test]
    public void TestCanSerializePropertiesForTab()
    {
        var properties = new CreateCommandSettings(BrowsingContextCreateType.Tab);
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("type"));
        Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("tab"));
    }

   [Test]
    public void TestCanSerializePropertiesForWindow()
    {
        var properties = new CreateCommandSettings(BrowsingContextCreateType.Window);
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("type"));
        Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("window"));
    }
}