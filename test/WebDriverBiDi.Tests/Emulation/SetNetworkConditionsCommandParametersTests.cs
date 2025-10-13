namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetNetworkConditionsCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetNetworkConditionsCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("emulation.setNetworkConditions"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetNetworkConditionsCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("networkConditions"));
            Assert.That(serialized["networkConditions"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["networkConditions"]!.Value<JObject?>, Is.Null);
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOffline()
    {
        SetNetworkConditionsCommandParameters properties = new()
        {
            NetworkConditions = new NetworkConditionsOffline()
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("networkConditions"));
            Assert.That(serialized["networkConditions"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? conditionsObject = serialized["networkConditions"] as JObject;
            Assert.That(conditionsObject, Has.Count.EqualTo(1));
            Assert.That(conditionsObject, Contains.Key("type"));
            Assert.That(conditionsObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(conditionsObject!["type"]!.Value<string>(), Is.EqualTo("offline"));
        });
    }

    [Test]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetNetworkConditionsCommandParameters properties = new()
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
            Assert.That(serialized, Contains.Key("networkConditions"));
            Assert.That(serialized["networkConditions"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["networkConditions"]!.Value<JObject?>(), Is.Null);
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
        SetNetworkConditionsCommandParameters properties = new()
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
            Assert.That(serialized, Contains.Key("networkConditions"));
            Assert.That(serialized["networkConditions"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["networkConditions"]!.Value<JObject?>(), Is.Null);
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
