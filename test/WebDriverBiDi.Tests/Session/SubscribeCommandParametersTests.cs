namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SubscribeCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SubscribeCommandParameters properties = new(["my.event"]);
        Assert.That(properties.MethodName, Is.EqualTo("session.subscribe"));
    }

    [Test]
    public void TestCannotInitializeWithEmptyEventList()
    {
        Assert.Throws<ArgumentException>(() => new SubscribeCommandParameters(Array.Empty<string>()));
    }

    [Test]
    public void TestCanSerializeParametersWithASingleEvent()
    {
        SubscribeCommandParameters properties = new("some.event");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("events"));
            Assert.That(serialized["events"]!.Count, Is.EqualTo(1));
            Assert.That(serialized["events"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["events"]![0]!.Value<string>(), Is.EqualTo("some.event"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithEvents()
    {
        SubscribeCommandParameters properties = new(["some.event"]);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("events"));
            Assert.That(serialized["events"]!.Count, Is.EqualTo(1));
            Assert.That(serialized["events"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["events"]![0]!.Value<string>(), Is.EqualTo("some.event"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithBrowsingContextData()
    {
        SubscribeCommandParameters properties = new(["some.event"]);
        properties.Contexts.Add("myContext");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("events"));
            Assert.That(serialized["events"]!.Count, Is.EqualTo(1));
            Assert.That(serialized["events"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["events"]![0]!.Value<string>(), Is.EqualTo("some.event"));
            Assert.That(serialized, Contains.Key("contexts"));
            Assert.That(serialized["contexts"]!.Count, Is.EqualTo(1));
            Assert.That(serialized["contexts"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["contexts"]![0]!.Value<string>(), Is.EqualTo("myContext"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithUserContextData()
    {
        SubscribeCommandParameters properties = new(["some.event"]);
        properties.UserContexts.Add("myUserContext");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("events"));
            Assert.That(serialized["events"]!.Count, Is.EqualTo(1));
            Assert.That(serialized["events"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["events"]![0]!.Value<string>(), Is.EqualTo("some.event"));
            Assert.That(serialized, Contains.Key("userContexts"));
            Assert.That(serialized["userContexts"]!.Count, Is.EqualTo(1));
            Assert.That(serialized["userContexts"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["userContexts"]![0]!.Value<string>(), Is.EqualTo("myUserContext"));
        }
    }

    [Test]
    public void TestInitializeUsingConstructor()
    {
        SubscribeCommandParameters properties = new(["someEvent"], ["someContext"], ["someUserContext"]);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(properties.Events, Has.Count.EqualTo(1));
            Assert.That(properties.Events, Contains.Item("someEvent"));
            Assert.That(properties.Contexts, Has.Count.EqualTo(1));
            Assert.That(properties.Contexts, Contains.Item("someContext"));
            Assert.That(properties.UserContexts, Has.Count.EqualTo(1));
            Assert.That(properties.UserContexts, Contains.Item("someUserContext"));
        }
    }

    [Test]
    public void TestInitializeUsingConstructorForBrowsingContexts()
    {
        SubscribeCommandParameters properties = new(["someEvent"], ["someContext"]);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(properties.Events, Has.Count.EqualTo(1));
            Assert.That(properties.Events, Contains.Item("someEvent"));
            Assert.That(properties.Contexts, Has.Count.EqualTo(1));
            Assert.That(properties.Contexts, Contains.Item("someContext"));
            Assert.That(properties.UserContexts, Is.Empty);
        }
    }

    [Test]
    public void TestInitializeUsingConstructorForBrowsingContextsWithEmptyUserContextList()
    {
        SubscribeCommandParameters properties = new(["someEvent"], ["someContext"], []);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(properties.Events, Has.Count.EqualTo(1));
            Assert.That(properties.Events, Contains.Item("someEvent"));
            Assert.That(properties.Contexts, Has.Count.EqualTo(1));
            Assert.That(properties.Contexts, Contains.Item("someContext"));
            Assert.That(properties.UserContexts, Is.Empty);
        }
    }

    [Test]
    public void TestInitializeUsingConstructorForUserContexts()
    {
        SubscribeCommandParameters properties = new(["someEvent"], null, ["someUserContext"]);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(properties.Events, Has.Count.EqualTo(1));
            Assert.That(properties.Events, Contains.Item("someEvent"));
            Assert.That(properties.Contexts, Is.Empty);
            Assert.That(properties.UserContexts, Has.Count.EqualTo(1));
            Assert.That(properties.UserContexts, Contains.Item("someUserContext"));
        }
    }

    [Test]
    public void TestInitializeUsingConstructorForUserContextsWithEmptyBrowsingContextList()
    {
        SubscribeCommandParameters properties = new(["someEvent"], [], ["someUserContext"]);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(properties.Events, Has.Count.EqualTo(1));
            Assert.That(properties.Events, Contains.Item("someEvent"));
            Assert.That(properties.Contexts, Is.Empty);
            Assert.That(properties.UserContexts, Has.Count.EqualTo(1));
            Assert.That(properties.UserContexts, Contains.Item("someUserContext"));
        }
    }
}
