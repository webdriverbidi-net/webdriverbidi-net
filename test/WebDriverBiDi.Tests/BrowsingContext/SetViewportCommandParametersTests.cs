namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetViewportCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetViewportCommandParameters properties = new("myContextId");
        Assert.That(properties.MethodName, Is.EqualTo("browsingContext.setViewport"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetViewportCommandParameters properties = new("myContextId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("viewport"));
            Assert.That(serialized["viewport"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["viewport"]!.Value<JObject>(), Is.Null);
        });
    }

    [Test]
    public void TestCanSerializeParametersWithViewport()
    {
        SetViewportCommandParameters properties = new("myContextId")
        {
            Viewport = new Viewport()
            {
                Height = 1024,
                Width = 1280,
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("viewport"));
            Assert.That(serialized["viewport"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? viewportObject = serialized["viewport"]!.Value<JObject>();
            Assert.That(viewportObject, Is.Not.Null);
            Assert.That(viewportObject, Has.Count.EqualTo(2));
            Assert.That(viewportObject, Contains.Key("width"));
            Assert.That(viewportObject!["width"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(viewportObject!["width"]!.Value<ulong>(), Is.EqualTo(1280));
            Assert.That(viewportObject, Contains.Key("height"));
            Assert.That(viewportObject!["height"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(viewportObject!["height"]!.Value<ulong>(), Is.EqualTo(1024));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithDefaultViewport()
    {
        SetViewportCommandParameters properties = new("myContextId")
        {
            Viewport = new Viewport()
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("viewport"));
            Assert.That(serialized["viewport"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? viewportObject = serialized["viewport"]!.Value<JObject>();
            Assert.That(viewportObject, Is.Not.Null);
            Assert.That(viewportObject, Has.Count.EqualTo(2));
            Assert.That(viewportObject, Contains.Key("width"));
            Assert.That(viewportObject!["width"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(viewportObject!["width"]!.Value<ulong>(), Is.EqualTo(0));
            Assert.That(viewportObject, Contains.Key("height"));
            Assert.That(viewportObject!["height"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(viewportObject!["height"]!.Value<ulong>(), Is.EqualTo(0));
        });
    }
}