namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetScrollbarTypeOverrideCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetScrollbarTypeOverrideCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("emulation.setScrollbarTypeOverride"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetScrollbarTypeOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("scrollbarType"));
            Assert.That(serialized["scrollbarType"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["scrollbarType"]!.Value<JObject?>, Is.Null);
        }
    }

    [Test]
    public void TestCanSerializeParametersWithClassicScrollbarType()
    {
        SetScrollbarTypeOverrideCommandParameters properties = new()
        {
            ScrollbarType = ScrollbarType.Classic,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("scrollbarType"));
            Assert.That(serialized["scrollbarType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["scrollbarType"]!.Value<string>(), Is.EqualTo("classic"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithOverlayScrollbarType()
    {
        SetScrollbarTypeOverrideCommandParameters properties = new()
        {
            ScrollbarType = ScrollbarType.Overlay,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("scrollbarType"));
            Assert.That(serialized["scrollbarType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["scrollbarType"]!.Value<string>(), Is.EqualTo("overlay"));
        }
    }

    [Test]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetScrollbarTypeOverrideCommandParameters properties = new()
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
            Assert.That(serialized, Contains.Key("scrollbarType"));
            Assert.That(serialized["scrollbarType"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["scrollbarType"]!.Value<JObject?>(), Is.Null);
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
        SetScrollbarTypeOverrideCommandParameters properties = new()
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
            Assert.That(serialized, Contains.Key("scrollbarType"));
            Assert.That(serialized["scrollbarType"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["scrollbarType"]!.Value<JObject?>(), Is.Null);
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
