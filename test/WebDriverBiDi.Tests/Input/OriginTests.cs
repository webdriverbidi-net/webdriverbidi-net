namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

using WebDriverBiDi.Script;

[TestFixture]
public class OriginTests
{
    [Test]
    public void TestCanSerializeViewportOrigin()
    {
        Origin origin = Origin.Viewport;
        string json = JsonSerializer.Serialize(origin.Value);
        string? parsed = JsonSerializer.Deserialize<string>(json);
        Assert.That(parsed, Is.InstanceOf<string>());
        Assert.That(parsed, Is.EqualTo("viewport"));
    }

    [Test]
    public void TestCanSerializePointerOrigin()
    {
        Origin origin = Origin.Pointer;
        string json = JsonSerializer.Serialize(origin.Value);
        string? parsed = JsonSerializer.Deserialize<string>(json);
        Assert.That(parsed, Is.InstanceOf<string>());
        Assert.That(parsed, Is.EqualTo("pointer"));
    }

    [Test]
    public void TestCanSerializeElementOrigin()
    {
        string nodeJson = """
                          {
                            "type": "node",
                            "value": {
                              "nodeType": 1,
                              "childNodeCount": 0 
                            },
                            "sharedId": "testSharedId"
                          }
                          """;
        SharedReference node = JsonSerializer.Deserialize<RemoteValue>(nodeJson)!.ToSharedReference();
        
        ElementOrigin elementOrigin = new(node);
        Origin origin = Origin.Element(elementOrigin);
        string json = JsonSerializer.Serialize(origin.Value);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("element"));
            Assert.That(serialized, Contains.Key("element"));
            Assert.That(serialized["element"]!.Type, Is.EqualTo(JTokenType.Object));
        }
        JObject? serializedElementReference = serialized["element"]!.Value<JObject>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serializedElementReference, Is.Not.Null);
            Assert.That(serializedElementReference, Has.Count.EqualTo(1));
            Assert.That(serializedElementReference, Contains.Key("sharedId"));
            Assert.That(serializedElementReference!["sharedId"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serializedElementReference["sharedId"]!.Value<string>(), Is.EqualTo("testSharedId"));
        }
    }

    [Test]
    public void TestCanSerializeElementOriginFromSharedReference()
    {
        string nodeJson = """
                          {
                            "type": "node",
                            "value": {
                              "nodeType": 1,
                              "childNodeCount": 0 
                            },
                            "sharedId": "testSharedId"
                          }
                          """;
        SharedReference node = JsonSerializer.Deserialize<RemoteValue>(nodeJson)!.ToSharedReference();
        
        Origin origin = Origin.Element(node);
        string json = JsonSerializer.Serialize(origin.Value);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("element"));
            Assert.That(serialized, Contains.Key("element"));
            Assert.That(serialized["element"]!.Type, Is.EqualTo(JTokenType.Object));
        }
        JObject? serializedElementReference = serialized["element"]!.Value<JObject>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serializedElementReference, Is.Not.Null);
            Assert.That(serializedElementReference, Has.Count.EqualTo(1));
            Assert.That(serializedElementReference, Contains.Key("sharedId"));
            Assert.That(serializedElementReference!["sharedId"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serializedElementReference["sharedId"]!.Value<string>(), Is.EqualTo("testSharedId"));
        }
    }
}
