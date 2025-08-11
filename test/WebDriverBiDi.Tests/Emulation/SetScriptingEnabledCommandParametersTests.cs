namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetScriptingEnabledCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetScriptingEnabledCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("emulation.setScriptingEnabled"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetScriptingEnabledCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("enabled"));
            Assert.That(serialized["enabled"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["enabled"]!.Value<bool>, Is.False);
        });
    }

    [Test]
    public void TestCanSerializeParametersWithEnabledTrue()
    {
        SetScriptingEnabledCommandParameters properties = new(true);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("enabled"));
            Assert.That(serialized["enabled"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["enabled"]!.Value<bool>, Is.True);
        });
    }

    [Test]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetScriptingEnabledCommandParameters properties = new()
        {
            Contexts = new()
            {
                "context1",
                "context2",
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("enabled"));
            Assert.That(serialized["enabled"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["enabled"]!.Value<bool>(), Is.False);
            Assert.That(serialized, Contains.Key("contexts"));
            Assert.That(serialized["contexts"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? contextsArray = serialized["contexts"]!.Value<JArray>();
            Assert.That(contextsArray, Has.Count.EqualTo(2));
            Assert.That(contextsArray![0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(contextsArray[0].Value<string>(), Is.EqualTo("context1"));
            Assert.That(contextsArray[1].Type, Is.EqualTo(JTokenType.String));
            Assert.That(contextsArray[1].Value<string>(), Is.EqualTo("context2"));
        });
    }

    [Test]
    public void TestCanSerializePropertiesWithUserContexts()
    {
        SetScriptingEnabledCommandParameters properties = new()
        {
            UserContexts = new()
            {
                "userContext1",
                "userContext2",
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("enabled"));
            Assert.That(serialized["enabled"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["enabled"]!.Value<bool>(), Is.False);
            Assert.That(serialized, Contains.Key("userContexts"));
            Assert.That(serialized["userContexts"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? userContextsArray = serialized["userContexts"]!.Value<JArray>();
            Assert.That(userContextsArray, Has.Count.EqualTo(2));
            Assert.That(userContextsArray![0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(userContextsArray[0].Value<string>(), Is.EqualTo("userContext1"));
            Assert.That(userContextsArray[1].Type, Is.EqualTo(JTokenType.String));
            Assert.That(userContextsArray[1].Value<string>(), Is.EqualTo("userContext2"));
        });
    }
}
