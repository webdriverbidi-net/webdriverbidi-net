namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetTouchOverrideCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetTouchOverrideCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("emulation.setTouchOverride"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetTouchOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("maxTouchPoints"));
            Assert.That(serialized["maxTouchPoints"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["maxTouchPoints"]!.Value<JObject?>, Is.Null);
        }
    }

    [Test]
    public void TestCanSerializeParametersWithScreenOrientation()
    {
        SetTouchOverrideCommandParameters properties = new()
        {
            MaxTouchPoints = 100,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("maxTouchPoints"));
            Assert.That(serialized["maxTouchPoints"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["maxTouchPoints"]!.Value<ulong>(), Is.EqualTo(100));
        }
    }

    [Test]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetTouchOverrideCommandParameters properties = new()
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
            Assert.That(serialized, Contains.Key("maxTouchPoints"));
            Assert.That(serialized["maxTouchPoints"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["maxTouchPoints"]!.Value<JObject?>(), Is.Null);
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
        SetTouchOverrideCommandParameters properties = new()
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
            Assert.That(serialized, Contains.Key("maxTouchPoints"));
            Assert.That(serialized["maxTouchPoints"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["maxTouchPoints"]!.Value<JObject?>(), Is.Null);
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
