namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class HandleUserPromptCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        HandleUserPromptCommandParameters properties = new("myContextId");
        Assert.That(properties.MethodName, Is.EqualTo("browsingContext.handleUserPrompt"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        HandleUserPromptCommandParameters properties = new("myContextId");
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
    public void TestCanSerializeParametersWithAcceptTrue()
    {
        HandleUserPromptCommandParameters properties = new("myContextId")
        {
            Accept = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("accept"));
            Assert.That(serialized["accept"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["accept"]!.Value<bool>(), Is.EqualTo(true));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithAcceptFalse()
    {
        HandleUserPromptCommandParameters properties = new("myContextId")
        {
            Accept = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("accept"));
            Assert.That(serialized["accept"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["accept"]!.Value<bool>(), Is.EqualTo(false));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithUserText()
    {
        HandleUserPromptCommandParameters properties = new("myContextId")
        {
            UserText = "myUserText"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("userText"));
            Assert.That(serialized["userText"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["userText"]!.Value<string>(), Is.EqualTo("myUserText"));
        });
    }
}