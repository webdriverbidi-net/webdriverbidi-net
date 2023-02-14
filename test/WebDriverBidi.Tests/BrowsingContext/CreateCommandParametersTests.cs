namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CreateCommandParametersTests
{
    [Test]
    public void TestCanSerializeParametersForTab()
    {
        CreateCommandParameters properties = new(CreateType.Tab);
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("tab"));
        });
    }

    [Test]
    public void TestCanSerializeParametersForWindow()
    {
        CreateCommandParameters properties = new(CreateType.Window);
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("type"));
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
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("tab"));
            Assert.That(serialized.ContainsKey("referenceContext"));
            Assert.That(serialized["referenceContext"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["referenceContext"]!.Value<string>(), Is.EqualTo("myReferenceContext"));
        });
    }

    [Test]
    public void TestCanSetCreateTypeProperty()
    {
        CreateCommandParameters properties = new(CreateType.Tab)
        {
            CreateType = CreateType.Window
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("window"));
        });
    }
}