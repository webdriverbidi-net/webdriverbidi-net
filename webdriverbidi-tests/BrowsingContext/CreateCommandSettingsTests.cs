namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CreateCommandSettingsTests
{
    [Test]
    public void TestCanSerializeSettingsForTab()
    {
        CreateCommandSettings properties = new(BrowsingContextCreateType.Tab);
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("tab"));
        });
    }

    [Test]
    public void TestCanSerializeSettingsForWindow()
    {
        CreateCommandSettings properties = new(BrowsingContextCreateType.Window);
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("window"));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithReferenceContext()
    {
        CreateCommandSettings properties = new(BrowsingContextCreateType.Tab)
        {
            ReferenceContextId = "myReferenceContext"
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("tab"));
            Assert.That(serialized.ContainsKey("referenceContext"));
            Assert.That(serialized["referenceContext"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["referenceContext"]!.Value<string>(), Is.EqualTo("myReferenceContext"));
        });
    }

    [Test]
    public void TestCanSetCreateTypeProperty()
    {
        CreateCommandSettings properties = new(BrowsingContextCreateType.Tab)
        {
            CreateType = BrowsingContextCreateType.Window
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("window"));
        });
    }
}