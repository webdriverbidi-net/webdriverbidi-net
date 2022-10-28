namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class HandleUserPromptCommandSettingsTests
{
    [Test]
    public void TestCanSerializeSettings()
    {
        var properties = new HandleUserPromptCommandSettings("myContextId");
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("context"));
        Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
    }

    [Test]
    public void TestCanSerializeSettingsWithAcceptTrue()
    {
        var properties = new HandleUserPromptCommandSettings("myContextId");
        properties.Accept = true;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(2));
        Assert.That(serialized.ContainsKey("context"));
        Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        Assert.That(serialized.ContainsKey("accept"));
        Assert.That(serialized["accept"]!.Type, Is.EqualTo(JTokenType.Boolean));
        Assert.That(serialized["accept"]!.Value<bool>(), Is.EqualTo(true));
    }

    [Test]
    public void TestCanSerializeSettingsWithAcceptFalse()
    {
        var properties = new HandleUserPromptCommandSettings("myContextId");
        properties.Accept = false;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(2));
        Assert.That(serialized.ContainsKey("context"));
        Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        Assert.That(serialized.ContainsKey("accept"));
        Assert.That(serialized["accept"]!.Type, Is.EqualTo(JTokenType.Boolean));
        Assert.That(serialized["accept"]!.Value<bool>(), Is.EqualTo(false));
    }

    [Test]
    public void TestCanSerializeSettingsWithUserText()
    {
        var properties = new HandleUserPromptCommandSettings("myContextId");
        properties.UserText = "myUserText";
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(2));
        Assert.That(serialized.ContainsKey("context"));
        Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        Assert.That(serialized.ContainsKey("userText"));
        Assert.That(serialized["userText"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["userText"]!.Value<string>(), Is.EqualTo("myUserText"));
    }
}