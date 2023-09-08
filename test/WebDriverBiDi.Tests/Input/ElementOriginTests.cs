namespace WebDriverBiDi.Input;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Script;

[TestFixture]
public class ElementOriginTests
{
    [Test]
    public void TestCanSerializeOrigin()
    {
        string nodeJson = @"{ ""type"": ""node"", ""value"": { ""nodeType"": 1, ""childNodeCount"": 0 }, ""sharedId"": ""testSharedId"" }";
        SharedReference node = JsonConvert.DeserializeObject<RemoteValue>(nodeJson)!.ToSharedReference();
        ElementOrigin origin = new(node);
        string json = JsonConvert.SerializeObject(origin);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("element"));
            Assert.That(serialized, Contains.Key("element"));
            Assert.That(serialized["element"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject? serializedElementReference = serialized["element"]!.Value<JObject>();
        Assert.Multiple(() =>
        {
            Assert.That(serializedElementReference, Is.Not.Null);
            Assert.That(serializedElementReference, Has.Count.EqualTo(1));
            Assert.That(serializedElementReference, Contains.Key("sharedId"));
            Assert.That(serializedElementReference!["sharedId"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serializedElementReference["sharedId"]!.Value<string>(), Is.EqualTo("testSharedId"));
        });
    }
}