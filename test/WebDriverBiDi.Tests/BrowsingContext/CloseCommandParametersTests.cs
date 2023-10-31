namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CloseCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        CloseCommandParameters properties = new("myContextId");
        Assert.That(properties.MethodName, Is.EqualTo("browsingContext.close"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        CloseCommandParameters properties = new("myContextId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithPromptUnloadTrue()
    {
        CloseCommandParameters properties = new("myContextId")
        {
            PromptUnload = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("promptUnload"));
            Assert.That(serialized["promptUnload"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["promptUnload"]!.Value<bool>(), Is.True);
        });
    }

    [Test]
    public void TestCanSerializeParametersWithPromptUnloadFalse()
    {
        CloseCommandParameters properties = new("myContextId")
        {
            PromptUnload = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("promptUnload"));
            Assert.That(serialized["promptUnload"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["promptUnload"]!.Value<bool>(), Is.False);
        });
    }
}
