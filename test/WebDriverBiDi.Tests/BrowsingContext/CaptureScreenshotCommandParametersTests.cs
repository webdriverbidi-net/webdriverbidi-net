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
            JObject? clipObject = serialized["clip"]!.Value<JObject>();
            Assert.That(clipObject, Has.Count.EqualTo(5));
            Assert.That(clipObject, Contains.Key("type"));
            Assert.That(clipObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(clipObject["type"]!.Value<string>(), Is.EqualTo("box"));
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
            JObject? clipObject = serialized["clip"]!.Value<JObject>();
            Assert.That(clipObject, Has.Count.EqualTo(5));
            Assert.That(clipObject, Contains.Key("type"));
            Assert.That(clipObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(clipObject["type"]!.Value<string>(), Is.EqualTo("box"));
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
            JObject? clipObject = serialized["clip"]!.Value<JObject>();
            Assert.That(clipObject, Has.Count.EqualTo(2));
            Assert.That(clipObject, Contains.Key("type"));
            Assert.That(clipObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(clipObject["type"]!.Value<string>(), Is.EqualTo("element"));
            Assert.That(clipObject, Contains.Key("element"));
            Assert.That(clipObject!["element"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? sharedReferenceObject = clipObject["element"]!.Value<JObject>();
            Assert.That(sharedReferenceObject, Contains.Key("sharedId"));
            Assert.That(sharedReferenceObject!["sharedId"]!.Value<string>(), Is.EqualTo("myElementSharedId"));
        });
    }

    [Test]
    public void TestCanSerializeWithDefaultFormatParameter()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId")
        {
            Format = new ImageFormat()
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("format"));
            Assert.That(serialized["format"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? formatObject = serialized["format"]!.Value<JObject>();
            Assert.That(formatObject, Has.Count.EqualTo(1));
            Assert.That(formatObject, Contains.Key("type"));
            Assert.That(formatObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(formatObject["type"]!.Value<string>(), Is.EqualTo("image/png"));
        });
    }

    [Test]
    public void TestCanSerializeWithNonDefaultFormatParameter()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId")
        {
            Format = new ImageFormat()
            {
                Type = "image/jpeg"
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
            Assert.That(serialized, Contains.Key("format"));
            Assert.That(serialized["format"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? formatObject = serialized["format"]!.Value<JObject>();
            Assert.That(formatObject, Has.Count.EqualTo(1));
            Assert.That(formatObject, Contains.Key("type"));
            Assert.That(formatObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(formatObject["type"]!.Value<string>(), Is.EqualTo("image/jpeg"));
        });
    }

    [Test]
    public void TestCanSerializeWithNonDefaultFormatParameterIncludingQuality()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId")
        {
            Format = new ImageFormat()
            {
                Type = "image/jpeg",
                Quality = 0.5
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
            Assert.That(serialized, Contains.Key("format"));
            Assert.That(serialized["format"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? formatObject = serialized["format"]!.Value<JObject>();
            Assert.That(formatObject, Has.Count.EqualTo(2));
            Assert.That(formatObject, Contains.Key("type"));
            Assert.That(formatObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(formatObject["type"]!.Value<string>(), Is.EqualTo("image/jpeg"));
            Assert.That(formatObject, Contains.Key("quality"));
            Assert.That(formatObject!["quality"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(formatObject["quality"]!.Value<double>(), Is.EqualTo(0.5));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithViewportOrigin()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId")
        {
            Origin = ScreenshotOrigin.Viewport
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("origin"));
            Assert.That(serialized["origin"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["origin"]!.Value<string>(), Is.EqualTo("viewport"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithDocumentOrigin()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId")
        {
            Origin = ScreenshotOrigin.Document
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("origin"));
            Assert.That(serialized["origin"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["origin"]!.Value<string>(), Is.EqualTo("document"));
        });
    }

    [Test]
    public void TestImageFormatQualityParameterOutsideValidRangeThrows()
    {
        ImageFormat format = new();

        // Also test that Quality property can be explicitly set to null.
        format.Quality = null;
        Assert.That(() => format.Quality = -.01, Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Quality must be between 0 and 1 inclusive."));
        Assert.That(() => format.Quality = 1.01, Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Quality must be between 0 and 1 inclusive."));
    }
}
