namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.JsonConverters;
using WebDriverBiDi.Script;

[TestFixture]
public class WheelScrollActionTests
{
    [Test]
    public void TestCanSerializeParameters()
    {
        WheelScrollAction properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(5));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("scroll"));
            Assert.That(serialized, Contains.Key("x"));
            Assert.That(serialized["x"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["x"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("y"));
            Assert.That(serialized["y"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["y"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("deltaX"));
            Assert.That(serialized["deltaX"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["deltaX"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("deltaY"));
            Assert.That(serialized["deltaY"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["deltaY"]!.Value<long>(), Is.EqualTo(0));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalDuration()
    {
        WheelScrollAction properties = new()
        {
            Duration = TimeSpan.FromMilliseconds(1),
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(6));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("scroll"));
            Assert.That(serialized, Contains.Key("x"));
            Assert.That(serialized["x"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["x"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("y"));
            Assert.That(serialized["y"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["y"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("deltaX"));
            Assert.That(serialized["deltaX"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["deltaX"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("deltaY"));
            Assert.That(serialized["deltaY"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["deltaY"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("duration"));
            Assert.That(serialized["duration"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["duration"]!.Value<long>(), Is.EqualTo(1));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalViewportOrigin()
    {
        WheelScrollAction properties = new()
        {
            Origin = Origin.Viewport
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(6));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("scroll"));
            Assert.That(serialized, Contains.Key("x"));
            Assert.That(serialized["x"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["x"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("y"));
            Assert.That(serialized["y"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["y"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("deltaX"));
            Assert.That(serialized["deltaX"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["deltaX"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("deltaY"));
            Assert.That(serialized["deltaY"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["deltaY"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("origin"));
            Assert.That(serialized["origin"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["origin"]!.Value<string>(), Is.EqualTo("viewport"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalPointerOrigin()
    {
        WheelScrollAction properties = new()
        {
            Origin = Origin.Pointer
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(6));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("scroll"));
            Assert.That(serialized, Contains.Key("x"));
            Assert.That(serialized["x"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["x"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("y"));
            Assert.That(serialized["y"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["y"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("deltaX"));
            Assert.That(serialized["deltaX"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["deltaX"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("deltaY"));
            Assert.That(serialized["deltaY"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["deltaY"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("origin"));
            Assert.That(serialized["origin"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["origin"]!.Value<string>(), Is.EqualTo("pointer"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalElementOrigin()
    {
        JsonSerializerOptions deserializationOptions = new()
        {
            TypeInfoResolver = new PrivateConstructorContractResolver(),
        };
        string nodeJson = @"{ ""type"": ""node"", ""value"": { ""nodeType"": 1, ""childNodeCount"": 0 }, ""sharedId"": ""testSharedId"" }";
        SharedReference node = JsonSerializer.Deserialize<RemoteValue>(nodeJson, deserializationOptions)!.ToSharedReference();
        WheelScrollAction properties = new()
        {
            Origin = Origin.Element(new ElementOrigin(node))
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(6));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("scroll"));
            Assert.That(serialized, Contains.Key("x"));
            Assert.That(serialized["x"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["x"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("y"));
            Assert.That(serialized["y"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["y"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("deltaX"));
            Assert.That(serialized["deltaX"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["deltaX"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("deltaY"));
            Assert.That(serialized["deltaY"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["deltaY"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("origin"));
            Assert.That(serialized["origin"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject originObject = (JObject)serialized["origin"]!;
        Assert.That(originObject, Has.Count.EqualTo(2));
        Assert.That(originObject, Contains.Key("type"));
        Assert.Multiple(() =>
        {
            Assert.That(originObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(originObject["type"]!.Value<string>(), Is.EqualTo("element"));
            Assert.That(originObject, Contains.Key("element"));
            Assert.That(originObject["element"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject elementObject = (JObject)originObject["element"]!;
            Assert.That(elementObject, Has.Count.EqualTo(1));
            Assert.That(elementObject, Contains.Key("sharedId"));
            Assert.That(elementObject["sharedId"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(elementObject["sharedId"]!.Value<string>(), Is.EqualTo("testSharedId"));
        });
    }
}
