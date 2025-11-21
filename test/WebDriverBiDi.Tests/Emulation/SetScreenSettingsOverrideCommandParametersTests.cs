namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetScreenSettingsOverrideCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetScreenSettingsOverrideCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("emulation.setScreenSettingsOverride"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetScreenSettingsOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("screenArea"));
            Assert.That(serialized["screenArea"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["screenArea"]!.Value<JObject?>, Is.Null);
        }
    }

    [Test]
    public void TestCanSerializeParametersWithDefaultScreenArea()
    {
        SetScreenSettingsOverrideCommandParameters properties = new()
        {
            ScreenArea = new(),
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("screenArea"));
            Assert.That(serialized["screenArea"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? screenArea = serialized["screenArea"] as JObject;
            Assert.That(screenArea, Has.Count.EqualTo(2));
            Assert.That(screenArea, Contains.Key("width"));
            Assert.That(screenArea!["width"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(screenArea!["width"]!.Value<ulong>, Is.EqualTo(0));
            Assert.That(screenArea, Contains.Key("height"));
            Assert.That(screenArea!["height"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(screenArea!["height"]!.Value<ulong>, Is.EqualTo(0));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithScreenArea()
    {
        SetScreenSettingsOverrideCommandParameters properties = new()
        {
            ScreenArea = new()
            {
                Width = 1280,
                Height = 1024,
            },
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("screenArea"));
            Assert.That(serialized["screenArea"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? screenArea = serialized["screenArea"] as JObject;
            Assert.That(screenArea, Has.Count.EqualTo(2));
            Assert.That(screenArea, Contains.Key("width"));
            Assert.That(screenArea!["width"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(screenArea!["width"]!.Value<ulong>, Is.EqualTo(1280));
            Assert.That(screenArea, Contains.Key("height"));
            Assert.That(screenArea!["height"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(screenArea!["height"]!.Value<ulong>, Is.EqualTo(1024));
        }
    }

    [Test]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetScreenSettingsOverrideCommandParameters properties = new()
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
            Assert.That(serialized, Contains.Key("screenArea"));
            Assert.That(serialized["screenArea"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["screenArea"]!.Value<JObject?>(), Is.Null);
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
        SetScreenSettingsOverrideCommandParameters properties = new()
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
            Assert.That(serialized, Contains.Key("screenArea"));
            Assert.That(serialized["screenArea"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["screenArea"]!.Value<JObject?>(), Is.Null);
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
