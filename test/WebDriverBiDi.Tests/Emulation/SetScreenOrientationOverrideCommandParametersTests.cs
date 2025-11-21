namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetScreenOrientationOverrideCoordinatesCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetScreenOrientationOverrideCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("emulation.setScreenOrientationOverride"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetScreenOrientationOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("screenOrientation"));
            Assert.That(serialized["screenOrientation"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["screenOrientation"]!.Value<JObject?>, Is.Null);
        }
    }

    [Test]
    public void TestCanSerializeParametersWithScreenOrientation()
    {
        SetScreenOrientationOverrideCommandParameters properties = new()
        {
            ScreenOrientation = new(ScreenOrientationNatural.Landscape, ScreenOrientationType.PortraitSecondary),
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("screenOrientation"));
            Assert.That(serialized["screenOrientation"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? coordinatesObject = serialized["screenOrientation"] as JObject;
            Assert.That(coordinatesObject, Has.Count.EqualTo(2));
            Assert.That(coordinatesObject, Contains.Key("natural"));
            Assert.That(coordinatesObject!["natural"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(coordinatesObject!["natural"]!.Value<string>, Is.EqualTo("landscape"));
            Assert.That(coordinatesObject, Contains.Key("type"));
            Assert.That(coordinatesObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(coordinatesObject!["type"]!.Value<string>, Is.EqualTo("portrait-secondary"));
        }
    }

    [Test]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetScreenOrientationOverrideCommandParameters properties = new()
        {
            Contexts =
            [
                "context1",
                "context2",
            ]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("screenOrientation"));
            Assert.That(serialized["screenOrientation"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["screenOrientation"]!.Value<JObject?>(), Is.Null);
            Assert.That(serialized, Contains.Key("contexts"));
            Assert.That(serialized["contexts"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? contextsArray = serialized["contexts"]!.Value<JArray>();
            Assert.That(contextsArray, Has.Count.EqualTo(2));
            Assert.That(contextsArray![0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(contextsArray[0].Value<string>(), Is.EqualTo("context1"));
            Assert.That(contextsArray[1].Type, Is.EqualTo(JTokenType.String));
            Assert.That(contextsArray[1].Value<string>(), Is.EqualTo("context2"));
        }
    }

    [Test]
    public void TestCanSerializePropertiesWithUserContexts()
    {
        SetScreenOrientationOverrideCommandParameters properties = new()
        {
            UserContexts =
            [
                "userContext1",
                "userContext2",
            ]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("screenOrientation"));
            Assert.That(serialized["screenOrientation"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["screenOrientation"]!.Value<JObject?>(), Is.Null);
            Assert.That(serialized, Contains.Key("userContexts"));
            Assert.That(serialized["userContexts"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? userContextsArray = serialized["userContexts"]!.Value<JArray>();
            Assert.That(userContextsArray, Has.Count.EqualTo(2));
            Assert.That(userContextsArray![0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(userContextsArray[0].Value<string>(), Is.EqualTo("userContext1"));
            Assert.That(userContextsArray[1].Type, Is.EqualTo(JTokenType.String));
            Assert.That(userContextsArray[1].Value<string>(), Is.EqualTo("userContext2"));
        }
    }
}
