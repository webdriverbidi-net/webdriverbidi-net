namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class GetTreeCommandSettingsTests
{
    [Test]
    public void TestCanSerializeSettings()
    {
        GetTreeCommandSettings properties = new();
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Is.Empty);
    }

    [Test]
    public void TestCanSerializeSettingsWithMaxDepth()
    {
        GetTreeCommandSettings properties = new()
        {
            MaxDepth = 2
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("maxDepth"));
            Assert.That(serialized["maxDepth"], Is.Not.Null);
            Assert.That(serialized["maxDepth"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["maxDepth"]!.Value<long>(), Is.EqualTo(2));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithRoot()
    {
        GetTreeCommandSettings properties = new()
        {
            RootBrowsingContextId = "rootBrowsingContext"
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("root"));
            Assert.That(serialized["root"], Is.Not.Null);
            Assert.That(serialized["root"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["root"]!.Value<string>(), Is.EqualTo("rootBrowsingContext"));
        });
    }
}