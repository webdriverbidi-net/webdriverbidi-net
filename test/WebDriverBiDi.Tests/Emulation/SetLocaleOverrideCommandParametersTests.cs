namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetLocaleOverrideCoordinatesCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetLocaleOverrideCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("emulation.setLocaleOverride"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetLocaleOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("locale"));
            Assert.That(serialized["locale"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["locale"]!.Value<JObject?>, Is.Null);
        }
    }

    [Test]
    public void TestCanSerializeParametersWithLocale()
    {
        SetLocaleOverrideCommandParameters properties = new()
        {
            Locale = "en-US"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("locale"));
            Assert.That(serialized["locale"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["locale"]!.Value<string>(), Is.EqualTo("en-US"));
        }
    }

    [Test]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetLocaleOverrideCommandParameters properties = new()
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
            Assert.That(serialized, Contains.Key("locale"));
            Assert.That(serialized["locale"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["locale"]!.Value<JObject?>(), Is.Null);
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
        SetLocaleOverrideCommandParameters properties = new()
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
            Assert.That(serialized, Contains.Key("locale"));
            Assert.That(serialized["locale"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["locale"]!.Value<JObject?>(), Is.Null);
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
