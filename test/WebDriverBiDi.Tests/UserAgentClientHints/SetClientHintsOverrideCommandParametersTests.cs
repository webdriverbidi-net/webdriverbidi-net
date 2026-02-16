namespace WebDriverBiDi.UserAgentClientHints;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetClientHintsOverrideCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetClientHintsOverrideCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("userAgentClientHints.setClientHintsOverride"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetClientHintsOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("clientHints"));
            Assert.That(serialized["clientHints"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["clientHints"]!.Value<JObject?>, Is.Null);
        }
    }

    [Test]
    public void TestCanSerializeParametersWithClientHints()
    {
        SetClientHintsOverrideCommandParameters properties = new()
        {
            ClientHints = new()
            {
                Platform = "myPlatform"
            },
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("clientHints"));
            Assert.That(serialized["clientHints"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? clientHintsObject = serialized["clientHints"]!.Value<JObject>();
            Assert.That(clientHintsObject, Is.Not.Null);
            Assert.That(clientHintsObject, Has.Count.EqualTo(1));
            Assert.That(clientHintsObject, Contains.Key("platform"));
            Assert.That(clientHintsObject!["platform"]!.Value<string>(), Is.EqualTo("myPlatform"));
        }
    }

    [Test]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetClientHintsOverrideCommandParameters properties = new()
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
            Assert.That(serialized, Contains.Key("clientHints"));
            Assert.That(serialized["clientHints"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["clientHints"]!.Value<JObject?>(), Is.Null);
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
        SetClientHintsOverrideCommandParameters properties = new()
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
            Assert.That(serialized, Contains.Key("clientHints"));
            Assert.That(serialized["clientHints"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["clientHints"]!.Value<JObject?>(), Is.Null);
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
