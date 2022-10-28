namespace WebDriverBidi.Session;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class NewCommandSettingsTests
{
    [Test]
    public void TestCanSerializeSettings()
    {
        var properties = new NewCommandSettings();
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(0));
    }

    public void TestCanSerializeWithAlwaysMatch()
    {
        var properties = new NewCommandSettings();
        properties.AlwaysMatch = new CapabilitiesRequest() { BrowserName = "greatBrowser" };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("alwaysMatch"));
        Assert.That(serialized["alwaysMatch"]!.Type, Is.EqualTo(JTokenType.Object));
        JObject? alwaysMatch = serialized["alwaysMatch"] as JObject;
        Assert.That(alwaysMatch!.Count, Is.EqualTo(1));
        Assert.That(alwaysMatch.ContainsKey("browserName"));
        Assert.That(alwaysMatch["browserName"]!.Value<string>(), Is.EqualTo("greatBrowser"));
    }

    public void TestCanSerializeWithFirstMatch()
    {
        var properties = new NewCommandSettings();
        properties.FirstMatch.Add(new CapabilitiesRequest() { BrowserName = "greatBrowser" });
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("firstMatch"));
        Assert.That(serialized["firstMatch"]!.Type, Is.EqualTo(JTokenType.Array));
        JArray? firstMatch = serialized["firstMatch"] as JArray;
        Assert.That(firstMatch!.Count, Is.EqualTo(1));
        Assert.That(firstMatch[0].Type, Is.EqualTo(JTokenType.Object));
        JObject? firstMatchElement = firstMatch[0] as JObject;
        Assert.That(firstMatchElement!.Count, Is.EqualTo(1));
        Assert.That(firstMatchElement!.ContainsKey("browserName"));
        Assert.That(firstMatchElement["browserName"]!.Value<string>(), Is.EqualTo("greatBrowser"));
    }
}