namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetBypassCSPCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetBypassCSPCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("browsingContext.setBypassCSP"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetBypassCSPCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("bypass"));
            Assert.That(serialized["bypass"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["bypass"]!.Value<bool?>(), Is.Null);
        }
    }

    [Test]
    public void TestCanSerializeParametersWithEnabledTrue()
    {
        SetBypassCSPCommandParameters properties = new()
        {
            Bypass = true,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("bypass"));
            Assert.That(serialized["bypass"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["bypass"]!.Value<bool?>(), Is.True);
        }
    }

    [Test]
    public void TestCanSerializeParametersWithEnabledFalse()
    {
        SetBypassCSPCommandParameters properties = new()
        {
            Bypass = false,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("bypass"));
            Assert.That(serialized["bypass"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["bypass"]!.Value<bool?>(), Is.Null);
        }
    }

    [Test]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetBypassCSPCommandParameters properties = new()
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
            Assert.That(serialized, Contains.Key("bypass"));
            Assert.That(serialized["bypass"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["bypass"]!.Value<bool?>(), Is.Null);
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
        SetBypassCSPCommandParameters properties = new()
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
            Assert.That(serialized, Contains.Key("bypass"));
            Assert.That(serialized["bypass"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["bypass"]!.Value<bool?>(), Is.Null);
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

    [Test]
    public void TestCanGetResetParameters()
    {
        SetBypassCSPCommandParameters properties = SetBypassCSPCommandParameters.ResetBypassCSP;
        Assert.That(properties, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(properties.Bypass, Is.Null);
            Assert.That(properties.Contexts, Is.Null);
            Assert.That(properties.UserContexts, Is.Null);
        }
    }

    [Test]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetBypassCSPCommandParameters firstInstance = SetBypassCSPCommandParameters.ResetBypassCSP;
        SetBypassCSPCommandParameters secondInstance = SetBypassCSPCommandParameters.ResetBypassCSP;
        Assert.That(firstInstance, Is.Not.SameAs(secondInstance));
    }
}
