namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class PointerSourceActionsTests
{
    [Test]
    public void TestCanSerializeParameters()
    {
        PointerSourceActions properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("id"));
            Assert.That(serialized["id"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointer"));
            Assert.That(serialized, Contains.Key("actions"));
            Assert.That(serialized["actions"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["actions"]!.Value<JArray>(), Is.Empty);
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalMousePointerType()
    {
        PointerSourceActions properties = new()
        {
            Parameters = new PointerParameters()
            {
                PointerType = PointerType.Mouse,
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("id"));
            Assert.That(serialized["id"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointer"));
            Assert.That(serialized, Contains.Key("actions"));
            Assert.That(serialized["actions"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["actions"]!.Value<JArray>(), Is.Empty);
            Assert.That(serialized, Contains.Key("parameters"));
            Assert.That(serialized["parameters"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject parametersObject = (JObject)serialized["parameters"]!;
        Assert.Multiple(() =>
        {
            Assert.That(parametersObject, Has.Count.EqualTo(1));
            Assert.That(parametersObject, Contains.Key("pointerType"));
            Assert.That(parametersObject["pointerType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parametersObject["pointerType"]!.Value<string>(), Is.EqualTo("mouse"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalPenPointerType()
    {
        PointerSourceActions properties = new()
        {
            Parameters = new PointerParameters()
            {
                PointerType = PointerType.Pen,
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("id"));
            Assert.That(serialized["id"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointer"));
            Assert.That(serialized, Contains.Key("actions"));
            Assert.That(serialized["actions"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["actions"]!.Value<JArray>(), Is.Empty);
            Assert.That(serialized, Contains.Key("parameters"));
            Assert.That(serialized["parameters"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject parametersObject = (JObject)serialized["parameters"]!;
        Assert.Multiple(() =>
        {
            Assert.That(parametersObject, Has.Count.EqualTo(1));
            Assert.That(parametersObject, Contains.Key("pointerType"));
            Assert.That(parametersObject["pointerType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parametersObject["pointerType"]!.Value<string>(), Is.EqualTo("pen"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalTouchPointerType()
    {
        PointerSourceActions properties = new()
        {
            Parameters = new PointerParameters()
            {
                PointerType = PointerType.Touch,
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("id"));
            Assert.That(serialized["id"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointer"));
            Assert.That(serialized, Contains.Key("actions"));
            Assert.That(serialized["actions"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["actions"]!.Value<JArray>(), Is.Empty);
            Assert.That(serialized, Contains.Key("parameters"));
            Assert.That(serialized["parameters"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject parametersObject = (JObject)serialized["parameters"]!;
        Assert.Multiple(() =>
        {
            Assert.That(parametersObject, Has.Count.EqualTo(1));
            Assert.That(parametersObject, Contains.Key("pointerType"));
            Assert.That(parametersObject["pointerType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parametersObject["pointerType"]!.Value<string>(), Is.EqualTo("touch"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalUnspecifiedPointerType()
    {
        PointerSourceActions properties = new()
        {
            Parameters = new PointerParameters()
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("id"));
            Assert.That(serialized["id"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointer"));
            Assert.That(serialized, Contains.Key("actions"));
            Assert.That(serialized["actions"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["actions"]!.Value<JArray>(), Is.Empty);
            Assert.That(serialized, Contains.Key("parameters"));
            Assert.That(serialized["parameters"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject parametersObject = (JObject)serialized["parameters"]!;
        Assert.That(parametersObject, Is.Empty);
    }

    [Test]
    public void TestCanSerializeParametersWithActions()
    {
        PointerSourceActions properties = new();
        properties.Actions.Add(new PointerDownAction(0));
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("id"));
            Assert.That(serialized["id"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"], Is.Not.Null);
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointer"));
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
            Assert.That(action, Has.Count.EqualTo(2));
            Assert.That(action, Contains.Key("type"));
            Assert.That(action!["type"], Is.Not.Null);
            Assert.That(action!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(action!["type"]!.Value<string>(), Is.EqualTo("pointerDown"));
            Assert.That(action, Contains.Key("button"));
            Assert.That(action!["button"], Is.Not.Null);
            Assert.That(action!["button"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(action!["button"]!.Value<long>(), Is.EqualTo(0));
        });
    }
}
