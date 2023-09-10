namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Script;

[TestFixture]
public class CaptureScreenshotCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId");
        Assert.That(properties.MethodName, Is.EqualTo("browsingContext.captureScreenshot"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId");
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
    public void TestCanSerializeParametersWithDefaultBoxClipRectangle()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId")
        {
            Clip = new BoxClipRectangle()
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("clip"));
            Assert.That(serialized["clip"]!.Type, Is.EqualTo(JTokenType.Object));
            var clipObject = serialized["clip"]!.Value<JObject>();
            Assert.That(clipObject, Has.Count.EqualTo(5));
            Assert.That(clipObject, Contains.Key("type"));
            Assert.That(clipObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(clipObject["type"]!.Value<string>(), Is.EqualTo("viewport"));
            Assert.That(clipObject, Contains.Key("x"));
            Assert.That(clipObject!["x"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(clipObject["x"]!.Value<double>(), Is.EqualTo(0.0));
            Assert.That(clipObject, Contains.Key("y"));
            Assert.That(clipObject!["y"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(clipObject["y"]!.Value<double>(), Is.EqualTo(0.0));
            Assert.That(clipObject, Contains.Key("width"));
            Assert.That(clipObject!["width"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(clipObject["width"]!.Value<double>(), Is.EqualTo(0.0));
            Assert.That(clipObject, Contains.Key("height"));
            Assert.That(clipObject!["height"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(clipObject["height"]!.Value<double>(), Is.EqualTo(0.0));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithNonDefaultBoxClipRectangle()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId")
        {
            Clip = new BoxClipRectangle()
            {
                X = 10,
                Y = 20,
                Width = 200,
                Height = 100
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
            Assert.That(serialized, Contains.Key("clip"));
            Assert.That(serialized["clip"]!.Type, Is.EqualTo(JTokenType.Object));
            var clipObject = serialized["clip"]!.Value<JObject>();
            Assert.That(clipObject, Has.Count.EqualTo(5));
            Assert.That(clipObject, Contains.Key("type"));
            Assert.That(clipObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(clipObject["type"]!.Value<string>(), Is.EqualTo("viewport"));
            Assert.That(clipObject, Contains.Key("x"));
            Assert.That(clipObject!["x"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(clipObject["x"]!.Value<double>(), Is.EqualTo(10.0));
            Assert.That(clipObject, Contains.Key("y"));
            Assert.That(clipObject!["y"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(clipObject["y"]!.Value<double>(), Is.EqualTo(20.0));
            Assert.That(clipObject, Contains.Key("width"));
            Assert.That(clipObject!["width"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(clipObject["width"]!.Value<double>(), Is.EqualTo(200.0));
            Assert.That(clipObject, Contains.Key("height"));
            Assert.That(clipObject!["height"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(clipObject["height"]!.Value<double>(), Is.EqualTo(100.0));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithDefaultElementClipRectangle()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId")
        {
            Clip = new ElementClipRectangle(new SharedReference("myElementSharedId"))
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("clip"));
            Assert.That(serialized["clip"]!.Type, Is.EqualTo(JTokenType.Object));
            var clipObject = serialized["clip"]!.Value<JObject>();
            Assert.That(clipObject, Has.Count.EqualTo(2));
            Assert.That(clipObject, Contains.Key("type"));
            Assert.That(clipObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(clipObject["type"]!.Value<string>(), Is.EqualTo("element"));
            Assert.That(clipObject, Contains.Key("element"));
            Assert.That(clipObject!["element"]!.Type, Is.EqualTo(JTokenType.Object));
            var sharedReferenceObject = clipObject["element"]!.Value<JObject>();
            Assert.That(sharedReferenceObject, Contains.Key("sharedId"));
            Assert.That(sharedReferenceObject!["sharedId"]!.Value<string>(), Is.EqualTo("myElementSharedId"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithElementClipRectangleWithScrollIntoViewTrue()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId")
        {
            Clip = new ElementClipRectangle(new SharedReference("myElementSharedId"))
            {
                ScrollIntoView = true
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
            Assert.That(serialized, Contains.Key("clip"));
            Assert.That(serialized["clip"]!.Type, Is.EqualTo(JTokenType.Object));
            var clipObject = serialized["clip"]!.Value<JObject>();
            Assert.That(clipObject, Has.Count.EqualTo(3));
            Assert.That(clipObject, Contains.Key("type"));
            Assert.That(clipObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(clipObject["type"]!.Value<string>(), Is.EqualTo("element"));
            Assert.That(clipObject, Contains.Key("scrollIntoView"));
            Assert.That(clipObject["scrollIntoView"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(clipObject["scrollIntoView"]!.Value<bool>(), Is.True);
            Assert.That(clipObject, Contains.Key("element"));
            Assert.That(clipObject!["element"]!.Type, Is.EqualTo(JTokenType.Object));
            var sharedReferenceObject = clipObject["element"]!.Value<JObject>();
            Assert.That(sharedReferenceObject, Contains.Key("sharedId"));
            Assert.That(sharedReferenceObject!["sharedId"]!.Value<string>(), Is.EqualTo("myElementSharedId"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithElementClipRectangleWithScrollIntoViewFalse()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId")
        {
            Clip = new ElementClipRectangle(new SharedReference("myElementSharedId"))
            {
                ScrollIntoView = false
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
            Assert.That(serialized, Contains.Key("clip"));
            Assert.That(serialized["clip"]!.Type, Is.EqualTo(JTokenType.Object));
            var clipObject = serialized["clip"]!.Value<JObject>();
            Assert.That(clipObject, Has.Count.EqualTo(3));
            Assert.That(clipObject, Contains.Key("type"));
            Assert.That(clipObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(clipObject["type"]!.Value<string>(), Is.EqualTo("element"));
            Assert.That(clipObject, Contains.Key("scrollIntoView"));
            Assert.That(clipObject["scrollIntoView"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(clipObject["scrollIntoView"]!.Value<bool>(), Is.False);
            Assert.That(clipObject, Contains.Key("element"));
            Assert.That(clipObject!["element"]!.Type, Is.EqualTo(JTokenType.Object));
            var sharedReferenceObject = clipObject["element"]!.Value<JObject>();
            Assert.That(sharedReferenceObject, Contains.Key("sharedId"));
            Assert.That(sharedReferenceObject!["sharedId"]!.Value<string>(), Is.EqualTo("myElementSharedId"));
        });
    }
}