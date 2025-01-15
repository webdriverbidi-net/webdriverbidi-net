namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SubscribeCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        SubscribeCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("session.subscribe"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SubscribeCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("events"));
            Assert.That(serialized["events"]!.Count, Is.EqualTo(0));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithEvents()
    {
        SubscribeCommandParameters properties = new();
        properties.Events.Add("some.event");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("events"));
            Assert.That(serialized["events"]!.Count, Is.EqualTo(1));
            Assert.That(serialized["events"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["events"]![0]!.Value<string>(), Is.EqualTo("some.event"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithBrowsingContextData()
    {
        SubscribeCommandParameters properties = new();
        properties.Contexts.Add("myContext");
        properties.Events.Add("some.event");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("events"));
            Assert.That(serialized["events"]!.Count, Is.EqualTo(1));
            Assert.That(serialized["events"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["events"]![0]!.Value<string>(), Is.EqualTo("some.event"));
            Assert.That(serialized, Contains.Key("contexts"));
            Assert.That(serialized["contexts"]!.Count, Is.EqualTo(1));
            Assert.That(serialized["contexts"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["contexts"]![0]!.Value<string>(), Is.EqualTo("myContext"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithUserContextData()
    {
        SubscribeCommandParameters properties = new();
        properties.UserContexts.Add("myUserContext");
        properties.Events.Add("some.event");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("events"));
            Assert.That(serialized["events"]!.Count, Is.EqualTo(1));
            Assert.That(serialized["events"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["events"]![0]!.Value<string>(), Is.EqualTo("some.event"));
            Assert.That(serialized, Contains.Key("userContexts"));
            Assert.That(serialized["userContexts"]!.Count, Is.EqualTo(1));
            Assert.That(serialized["userContexts"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["userContexts"]![0]!.Value<string>(), Is.EqualTo("myUserContext"));
        });
    }
    
    [Test]
    public void TestInitializeUsingConstructor()
    {
        SubscribeCommandParameters properties = new(["someEvent"], ["someContext"], ["someUserContext"]);
        Assert.Multiple(() =>
        {
            Assert.That(properties.Events, Has.Count.EqualTo(1));
            Assert.That(properties.Events, Contains.Item("someEvent"));
            Assert.That(properties.Contexts, Has.Count.EqualTo(1));
            Assert.That(properties.Contexts, Contains.Item("someContext"));
            Assert.That(properties.UserContexts, Has.Count.EqualTo(1));
            Assert.That(properties.UserContexts, Contains.Item("someUserContext"));
        });
    }
}
