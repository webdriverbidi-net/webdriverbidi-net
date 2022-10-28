namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class GetTreeCommandSettingsTests
{
    [Test]
    public void TestCanSerializeSettings()
    {
        var properties = new GetTreeCommandSettings();
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestCanSerializeSettingsWithMaxDepth()
    {
        var properties = new GetTreeCommandSettings();
        properties.MaxDepth = 2;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("maxDepth"));
        Assert.That(serialized["maxDepth"], Is.Not.Null);
        Assert.That(serialized["maxDepth"]!.Type, Is.EqualTo(JTokenType.Integer));
        Assert.That(serialized["maxDepth"]!.Value<long>(), Is.EqualTo(2));
    }

    [Test]
    public void TestCanSerializeSettingsWithRoot()
    {
        var properties = new GetTreeCommandSettings();
        properties.RootBrowsingContextId = "rootBrowsingContext";
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("root"));
        Assert.That(serialized["root"], Is.Not.Null);
        Assert.That(serialized["root"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["root"]!.Value<string>(), Is.EqualTo("rootBrowsingContext"));
    }
}