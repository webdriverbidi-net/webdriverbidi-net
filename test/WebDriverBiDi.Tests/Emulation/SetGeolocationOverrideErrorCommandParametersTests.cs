namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetGeolocationOverrideErrorCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetGeolocationOverrideErrorCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("emulation.setGeolocationOverride"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetGeolocationOverrideErrorCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("error"));
            Assert.That(serialized["error"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? errorObject = serialized["error"]!.ToObject<JObject>();
            Assert.That(errorObject, Has.Count.EqualTo(1));
            Assert.That(errorObject, Contains.Key("type"));
            Assert.That(errorObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(errorObject!["type"]!.Value<string>(), Is.EqualTo("positionUnavailable"));
        }
    }


    [Test]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetGeolocationOverrideErrorCommandParameters properties = new()
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
            Assert.That(serialized, Contains.Key("error"));
            Assert.That(serialized["error"]!.Type, Is.EqualTo(JTokenType.Object));
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
        SetGeolocationOverrideErrorCommandParameters properties = new()
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
            Assert.That(serialized, Contains.Key("error"));
            Assert.That(serialized["error"]!.Type, Is.EqualTo(JTokenType.Object));
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
