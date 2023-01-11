namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class DisownCommandSettingsTests
{
    [Test]
    public void TestCanSerializeSettings()
    {
        DisownCommandSettings properties = new(new RealmTarget("myRealm"));
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("target"));
            Assert.That(serialized["target"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized.ContainsKey("handles"));
            Assert.That(serialized["handles"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["handles"]!.Count, Is.EqualTo(0));
        });
    }

    [Test]
    public void TestCanSerializeSettingsWithHandles()
    {
        DisownCommandSettings properties = new(new RealmTarget("myRealm"), "myHandle");
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("target"));
            Assert.That(serialized["target"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized.ContainsKey("handles"));
            Assert.That(serialized["handles"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["handles"]!.Count, Is.EqualTo(1));
        });
    }
}