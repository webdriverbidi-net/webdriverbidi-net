namespace WebDriverBidi.Session;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class UnsubscribeCommandParametersTests
{
    [Test]
    public void TestCanSerializeParameters()
    {
        UnsubscribeCommandParameters properties = new();
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("events"));
            Assert.That(serialized["events"]!.Count, Is.EqualTo(0));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithEvents()
    {
        UnsubscribeCommandParameters properties = new();
        properties.Events.Add("some.event");
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("events"));
            Assert.That(serialized["events"]!.Count, Is.EqualTo(1));
            Assert.That(serialized["events"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["events"]![0]!.Value<string>(), Is.EqualTo("some.event"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithContexts()
    {
        UnsubscribeCommandParameters properties = new();
        properties.Contexts.Add("myContext");
        properties.Events.Add("some.event");
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("events"));
            Assert.That(serialized["events"]!.Count, Is.EqualTo(1));
            Assert.That(serialized["events"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["events"]![0]!.Value<string>(), Is.EqualTo("some.event"));
            Assert.That(serialized.ContainsKey("contexts"));
            Assert.That(serialized["contexts"]!.Count, Is.EqualTo(1));
            Assert.That(serialized["contexts"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["contexts"]![0]!.Value<string>(), Is.EqualTo("myContext"));
        });
    }
}