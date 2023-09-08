namespace WebDriverBiDi.Input;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class WheelSourceActionsTests
{
    [Test]
    public void TestCanSerializeParameters()
    {
        WheelSourceActions properties = new();
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("id"));
            Assert.That(serialized["id"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("wheel"));
            Assert.That(serialized, Contains.Key("actions"));
            Assert.That(serialized["actions"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["actions"]!.Value<JArray>(), Is.Empty);
        });
    }

    [Test]
    public void TestCanSerializeParametersWithActions()
    {
        WheelSourceActions properties = new();
        properties.Actions.Add(new WheelScrollAction());
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("id"));
            Assert.That(serialized["id"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"], Is.Not.Null);
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("wheel"));
            Assert.That(serialized["actions"], Is.Not.Null);
            Assert.That(serialized, Contains.Key("actions"));
            Assert.That(serialized["actions"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["actions"]!.Value<JArray>(), Has.Count.EqualTo(1));
            Assert.That(serialized["actions"]![0], Is.Not.Null);
            Assert.That(serialized["actions"]![0]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject? action = serialized["actions"]![0]!.Value<JObject>();
        Assert.Multiple(() =>
        {
            Assert.That(action, Is.Not.Null);
            Assert.That(action, Has.Count.EqualTo(5));
            Assert.That(action, Contains.Key("type"));
            Assert.That(action!["type"], Is.Not.Null);
            Assert.That(action!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(action!["type"]!.Value<string>(), Is.EqualTo("scroll"));
            Assert.That(action, Contains.Key("x"));
            Assert.That(action!["x"], Is.Not.Null);
            Assert.That(action!["x"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(action!["x"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(action, Contains.Key("y"));
            Assert.That(action!["y"], Is.Not.Null);
            Assert.That(action!["y"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(action!["y"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(action, Contains.Key("deltaX"));
            Assert.That(action!["deltaX"], Is.Not.Null);
            Assert.That(action!["deltaX"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(action!["deltaX"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(action, Contains.Key("deltaY"));
            Assert.That(action!["deltaY"], Is.Not.Null);
            Assert.That(action!["deltaY"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(action!["deltaY"]!.Value<long>(), Is.EqualTo(0));
        });
    }
}