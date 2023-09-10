namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CreateCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        CreateCommandParameters properties = new(CreateType.Tab);
        Assert.That(properties.MethodName, Is.EqualTo("browsingContext.create"));
    }

    [Test]
    public void TestCanSerializeParametersForTab()
    {
        CreateCommandParameters properties = new(CreateType.Tab);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("tab"));
        });
    }

    [Test]
    public void TestCanSerializeParametersForWindow()
    {
        CreateCommandParameters properties = new(CreateType.Window);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("window"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithReferenceContext()
    {
        CreateCommandParameters properties = new(CreateType.Tab)
        {
            ReferenceContextId = "myReferenceContext"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("tab"));
            Assert.That(serialized, Contains.Key("referenceContext"));
            Assert.That(serialized["referenceContext"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["referenceContext"]!.Value<string>(), Is.EqualTo("myReferenceContext"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithBackgroundTrue()
    {
        CreateCommandParameters properties = new(CreateType.Tab)
        {
            IsCreatedInBackground = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("tab"));
            Assert.That(serialized, Contains.Key("background"));
            Assert.That(serialized["background"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["background"]!.Value<bool>(), Is.EqualTo(true));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithBackgroundFalse()
    {
        CreateCommandParameters properties = new(CreateType.Tab)
        {
            IsCreatedInBackground = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("tab"));
            Assert.That(serialized, Contains.Key("background"));
            Assert.That(serialized["background"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["background"]!.Value<bool>(), Is.EqualTo(false));
        });
    }

    [Test]
    public void TestCanSetCreateTypeProperty()
    {
        CreateCommandParameters properties = new(CreateType.Tab)
        {
            CreateType = CreateType.Window
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("window"));
        });
    }
}