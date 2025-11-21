namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class UnsubscribeCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        UnsubscribeByAttributesCommandParameters byAttributesProperties = new();
        Assert.That(byAttributesProperties.MethodName, Is.EqualTo("session.unsubscribe"));
        UnsubscribeByIdsCommandParameters byIdProperties = new();
        Assert.That(byIdProperties.MethodName, Is.EqualTo("session.unsubscribe"));
    }

    [Test]
    public void TestCanSerializeByAttributesParameters()
    {
        UnsubscribeByAttributesCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("events"));
            Assert.That(serialized["events"]!.Count, Is.EqualTo(0));
        }
    }

    [Test]
    public void TestCanSerializeByAttributesParametersWithEvents()
    {
        UnsubscribeByAttributesCommandParameters properties = new();
        properties.Events.Add("some.event");
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
    public void TestCanSerializeByIdsParameters()
    {
        UnsubscribeByIdsCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("subscriptions"));
            Assert.That(serialized["subscriptions"]!.Count, Is.EqualTo(0));
        }
    }

    [Test]
    public void TestCanSerializeByIdsParametersWithEvents()
    {
        UnsubscribeByIdsCommandParameters properties = new();
        properties.SubscriptionIds.Add("mySubscriptionId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("subscriptions"));
            Assert.That(serialized["subscriptions"]!.Count, Is.EqualTo(1));
            Assert.That(serialized["subscriptions"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["subscriptions"]![0]!.Value<string>(), Is.EqualTo("mySubscriptionId"));
        }
    }
}
