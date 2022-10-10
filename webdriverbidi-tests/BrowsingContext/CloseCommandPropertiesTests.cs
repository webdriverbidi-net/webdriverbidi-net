namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CloseCommandPropertiesTests
{
    [Test]
    public void TestCanSerializeProperties()
    {
        var properties = new CloseCommandSettings("myContextId");
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("context"));
        Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
    }
}