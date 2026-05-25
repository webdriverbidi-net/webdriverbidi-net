namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Script;

public class CaptureScreenshotCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId");
        Assert.Equal("browsingContext.captureScreenshot", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithDefaultBoxClipRectangle()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId")
        {
            Clip = new BoxClipRectangle()
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("clip"));
        JToken? clipToken = serialized["clip"];
        Assert.NotNull(clipToken);
        Assert.Equal(JTokenType.Object, clipToken.Type);
        JObject? clipObject = clipToken.Value<JObject>();
        Assert.NotNull(clipObject);
        Assert.Equal(5, clipObject.Count);

        Assert.True(clipObject.ContainsKey("type"));
        JToken? clipType = clipObject["type"];
        Assert.NotNull(clipType);
        Assert.Equal(JTokenType.String, clipType.Type);
        Assert.Equal("box", clipType.Value<string>());

        Assert.True(clipObject.ContainsKey("x"));
        JToken? clipX = clipObject["x"];
        Assert.NotNull(clipX);
        Assert.Equal(JTokenType.Float, clipX.Type);
        Assert.Equal(0.0, clipX.Value<double>());

        Assert.True(clipObject.ContainsKey("y"));
        JToken? clipY = clipObject["y"];
        Assert.NotNull(clipY);
        Assert.Equal(JTokenType.Float, clipY.Type);
        Assert.Equal(0.0, clipY.Value<double>());

        Assert.True(clipObject.ContainsKey("width"));
        JToken? clipWidth = clipObject["width"];
        Assert.NotNull(clipWidth);
        Assert.Equal(JTokenType.Float, clipWidth.Type);
        Assert.Equal(0.0, clipWidth.Value<double>());

        Assert.True(clipObject.ContainsKey("height"));
        JToken? clipHeight = clipObject["height"];
        Assert.NotNull(clipHeight);
        Assert.Equal(JTokenType.Float, clipHeight.Type);
        Assert.Equal(0.0, clipHeight.Value<double>());
    }

    [Fact]
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
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("clip"));
        JToken? clipToken = serialized["clip"];
        Assert.NotNull(clipToken);
        Assert.Equal(JTokenType.Object, clipToken.Type);
        JObject? clipObject = clipToken.Value<JObject>();
        Assert.NotNull(clipObject);
        Assert.Equal(5, clipObject.Count);

        Assert.True(clipObject.ContainsKey("type"));
        JToken? clipType = clipObject["type"];
        Assert.NotNull(clipType);
        Assert.Equal(JTokenType.String, clipType.Type);
        Assert.Equal("box", clipType.Value<string>());

        Assert.True(clipObject.ContainsKey("x"));
        JToken? clipX = clipObject["x"];
        Assert.NotNull(clipX);
        Assert.Equal(JTokenType.Float, clipX.Type);
        Assert.Equal(10.0, clipX.Value<double>());

        Assert.True(clipObject.ContainsKey("y"));
        JToken? clipY = clipObject["y"];
        Assert.NotNull(clipY);
        Assert.Equal(JTokenType.Float, clipY.Type);
        Assert.Equal(20.0, clipY.Value<double>());

        Assert.True(clipObject.ContainsKey("width"));
        JToken? clipWidth = clipObject["width"];
        Assert.NotNull(clipWidth);
        Assert.Equal(JTokenType.Float, clipWidth.Type);
        Assert.Equal(200.0, clipWidth.Value<double>());

        Assert.True(clipObject.ContainsKey("height"));
        JToken? clipHeight = clipObject["height"];
        Assert.NotNull(clipHeight);
        Assert.Equal(JTokenType.Float, clipHeight.Type);
        Assert.Equal(100.0, clipHeight.Value<double>());
    }

    [Fact]
    public void TestCanSerializeParametersWithDefaultElementClipRectangle()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId")
        {
            Clip = new ElementClipRectangle(new SharedReference("myElementSharedId"))
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("clip"));
        JToken? clipToken = serialized["clip"];
        Assert.NotNull(clipToken);
        Assert.Equal(JTokenType.Object, clipToken.Type);
        JObject? clipObject = clipToken.Value<JObject>();
        Assert.NotNull(clipObject);
        Assert.Equal(2, clipObject.Count);

        Assert.True(clipObject.ContainsKey("type"));
        JToken? clipType = clipObject["type"];
        Assert.NotNull(clipType);
        Assert.Equal(JTokenType.String, clipType.Type);
        Assert.Equal("element", clipType.Value<string>());

        Assert.True(clipObject.ContainsKey("element"));
        JToken? clipElement = clipObject["element"];
        Assert.NotNull(clipElement);
        Assert.Equal(JTokenType.Object, clipElement.Type);
        JObject? sharedReferenceObject = clipElement.Value<JObject>();
        Assert.NotNull(sharedReferenceObject);
        Assert.True(sharedReferenceObject.ContainsKey("sharedId"));
        JToken? sharedId = sharedReferenceObject["sharedId"];
        Assert.NotNull(sharedId);
        Assert.Equal("myElementSharedId", sharedId.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithDefaultFormatParameter()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId")
        {
            Format = new ImageFormat()
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("format"));
        JToken? formatToken = serialized["format"];
        Assert.NotNull(formatToken);
        Assert.Equal(JTokenType.Object, formatToken.Type);
        JObject? formatObject = formatToken.Value<JObject>();
        Assert.NotNull(formatObject);
        Assert.Single(formatObject);

        Assert.True(formatObject.ContainsKey("type"));
        JToken? formatType = formatObject["type"];
        Assert.NotNull(formatType);
        Assert.Equal(JTokenType.String, formatType.Type);
        Assert.Equal("image/png", formatType.Value<string>());
    }

    [Fact]
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
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("format"));
        JToken? formatToken = serialized["format"];
        Assert.NotNull(formatToken);
        Assert.Equal(JTokenType.Object, formatToken.Type);
        JObject? formatObject = formatToken.Value<JObject>();
        Assert.NotNull(formatObject);
        Assert.Single(formatObject);

        Assert.True(formatObject.ContainsKey("type"));
        JToken? formatType = formatObject["type"];
        Assert.NotNull(formatType);
        Assert.Equal(JTokenType.String, formatType.Type);
        Assert.Equal("image/jpeg", formatType.Value<string>());
    }

    [Fact]
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
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("format"));
        JToken? formatToken = serialized["format"];
        Assert.NotNull(formatToken);
        Assert.Equal(JTokenType.Object, formatToken.Type);
        JObject? formatObject = formatToken.Value<JObject>();
        Assert.NotNull(formatObject);
        Assert.Equal(2, formatObject.Count);

        Assert.True(formatObject.ContainsKey("type"));
        JToken? formatType = formatObject["type"];
        Assert.NotNull(formatType);
        Assert.Equal(JTokenType.String, formatType.Type);
        Assert.Equal("image/jpeg", formatType.Value<string>());

        Assert.True(formatObject.ContainsKey("quality"));
        JToken? formatQuality = formatObject["quality"];
        Assert.NotNull(formatQuality);
        Assert.Equal(JTokenType.Float, formatQuality.Type);
        Assert.Equal(0.5, formatQuality.Value<double>());
    }

    [Fact]
    public void TestCanSerializeParametersWithViewportOrigin()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId")
        {
            Origin = ScreenshotOrigin.Viewport
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("origin"));
        JToken? origin = serialized["origin"];
        Assert.NotNull(origin);
        Assert.Equal(JTokenType.String, origin.Type);
        Assert.Equal("viewport", origin.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithDocumentOrigin()
    {
        CaptureScreenshotCommandParameters properties = new("myContextId")
        {
            Origin = ScreenshotOrigin.Document
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("origin"));
        JToken? origin = serialized["origin"];
        Assert.NotNull(origin);
        Assert.Equal(JTokenType.String, origin.Type);
        Assert.Equal("document", origin.Value<string>());
    }

    [Fact]
    public void TestImageFormatQualityParameterOutsideValidRangeThrows()
    {
        ImageFormat format = new();

        // Also test that Quality property can be explicitly set to null.
        format.Quality = null;
        Assert.Contains("Quality must be between 0 and 1 inclusive.", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => format.Quality = -.01).Message);
        Assert.Contains("Quality must be between 0 and 1 inclusive.", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => format.Quality = 1.01).Message);
    }
}
