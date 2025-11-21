namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class KeySourceActionsTests
{
    [Test]
    public void TestCanSerializeParameters()
    {
        KeySourceActions properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("id"));
            Assert.That(serialized["id"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("key"));
            Assert.That(serialized, Contains.Key("actions"));
            Assert.That(serialized["actions"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["actions"]!.Value<JArray>(), Is.Empty);
        }
    }

    [Test]
    public void TestCanSerializeParametersWithActions()
    {
        KeySourceActions properties = new();
        properties.Actions.Add(new KeyDownAction("a"));
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("id"));
            Assert.That(serialized["id"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"], Is.Not.Null);
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("key"));
            Assert.That(serialized["actions"], Is.Not.Null);
            Assert.That(serialized, Contains.Key("actions"));
            Assert.That(serialized["actions"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["actions"]!.Value<JArray>(), Has.Count.EqualTo(1));
            Assert.That(serialized["actions"]![0], Is.Not.Null);
            Assert.That(serialized["actions"]![0]!.Type, Is.EqualTo(JTokenType.Object));
        }
        JObject? action = serialized["actions"]![0]!.Value<JObject>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action, Is.Not.Null);
            Assert.That(action, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("id"));
            Assert.That(serialized["id"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(action, Contains.Key("type"));
            Assert.That(action!["type"], Is.Not.Null);
            Assert.That(action!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(action!["type"]!.Value<string>(), Is.EqualTo("keyDown"));
            Assert.That(action, Contains.Key("value"));
            Assert.That(action!["value"], Is.Not.Null);
            Assert.That(action!["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(action!["value"]!.Value<string>(), Is.EqualTo("a"));
        }
    }
}
