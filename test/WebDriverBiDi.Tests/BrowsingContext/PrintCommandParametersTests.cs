namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class PrintCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        PrintCommandParameters properties = new("myContextId");
        Assert.Equal("browsingContext.print", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        PrintCommandParameters properties = new("myContextId");
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
    public void TestCanSerializeParametersWithMargins()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Margins = new()
            {
                Right = 2.54,
                Left = 2.54,
                Top = 2.54,
                Bottom = 2.54
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

        Assert.True(serialized.ContainsKey("margin"));
        JToken? marginToken = serialized["margin"];
        Assert.NotNull(marginToken);
        Assert.Equal(JTokenType.Object, marginToken.Type);

        JObject? margins = marginToken.Value<JObject>();
        Assert.NotNull(margins);
        Assert.Equal(4, margins.Count);

        Assert.True(margins.ContainsKey("right"));
        JToken? right = margins["right"];
        Assert.NotNull(right);
        Assert.Equal(JTokenType.Float, right.Type);
        Assert.Equal(2.54, right.Value<double>());

        Assert.True(margins.ContainsKey("left"));
        JToken? left = margins["left"];
        Assert.NotNull(left);
        Assert.Equal(JTokenType.Float, left.Type);
        Assert.Equal(2.54, left.Value<double>());

        Assert.True(margins.ContainsKey("top"));
        JToken? top = margins["top"];
        Assert.NotNull(top);
        Assert.Equal(JTokenType.Float, top.Type);
        Assert.Equal(2.54, top.Value<double>());

        Assert.True(margins.ContainsKey("bottom"));
        JToken? bottom = margins["bottom"];
        Assert.NotNull(bottom);
        Assert.Equal(JTokenType.Float, bottom.Type);
        Assert.Equal(2.54, bottom.Value<double>());
    }

    [Fact]
    public void TestCanSerializeParametersWithNullMarginValues()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Margins = new()
            {
                Right = null,
                Left = null,
                Top = null,
                Bottom = null
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

        Assert.True(serialized.ContainsKey("margin"));
        JToken? marginToken = serialized["margin"];
        Assert.NotNull(marginToken);
        Assert.Equal(JTokenType.Object, marginToken.Type);

        JObject? margins = marginToken.Value<JObject>();
        Assert.NotNull(margins);
        Assert.Empty(margins);
    }

    [Fact]
    public void TestSettingMarginsToInvalidValuesThrows()
    {
        PrintMarginParameters properties = new();

        Assert.Contains("Value must be greater than or equal to zero", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.Top = -1).Message);
        Assert.Contains("Value must be greater than or equal to zero", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.Bottom = -1).Message);
        Assert.Contains("Value must be greater than or equal to zero", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.Left = -1).Message);
        Assert.Contains("Value must be greater than or equal to zero", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.Right = -1).Message);
    }

    [Fact]
    public void TestCanSerializeParametersWithNullPageSizeValues()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Page = new()
            {
                Width = null,
                Height = null
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

        Assert.True(serialized.ContainsKey("page"));
        JToken? pageToken = serialized["page"];
        Assert.NotNull(pageToken);
        Assert.Equal(JTokenType.Object, pageToken.Type);

        JObject? margins = pageToken.Value<JObject>();
        Assert.NotNull(margins);
        Assert.Empty(margins);
    }

    [Fact]
    public void TestSettingPageSizeToInvalidValuesThrows()
    {
        PrintPageParameters properties = new();

        Assert.Contains("Value must be greater than or equal to zero", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.Width = -1).Message);
        Assert.Contains("Value must be greater than or equal to zero", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.Height = -1).Message);
    }

    [Fact]
    public void TestCanSerializeParametersWithNullMargins()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Margins = null
        };
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
    public void TestCanSerializeParametersWithPageSize()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Page = new()
            {
                Width = 24,
                Height = 29.7
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

        Assert.True(serialized.ContainsKey("page"));
        JToken? pageToken = serialized["page"];
        Assert.NotNull(pageToken);
        Assert.Equal(JTokenType.Object, pageToken.Type);

        JObject? pageSize = pageToken.Value<JObject>();
        Assert.NotNull(pageSize);
        Assert.Equal(2, pageSize.Count);

        Assert.True(pageSize.ContainsKey("width"));
        JToken? width = pageSize["width"];
        Assert.NotNull(width);
        Assert.Equal(JTokenType.Float, width.Type);
        Assert.Equal(24, width.Value<double>());

        Assert.True(pageSize.ContainsKey("height"));
        JToken? height = pageSize["height"];
        Assert.NotNull(height);
        Assert.Equal(JTokenType.Float, height.Type);
        Assert.Equal(29.7, height.Value<double>());
    }

    [Fact]
    public void TestCanSerializeParametersWithNullPageSize()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Page = null
        };
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
    public void TestCanSerializeParametersWithOrientation()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Orientation = PrintOrientation.Landscape
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("orientation"));
        JToken? orientation = serialized["orientation"];
        Assert.NotNull(orientation);
        Assert.Equal(JTokenType.String, orientation.Type);
        Assert.Equal("landscape", orientation.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithNullOrientation()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Orientation = null
        };
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
    public void TestCanSerializeParametersWithBackground()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Background = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("background"));
        JToken? background = serialized["background"];
        Assert.NotNull(background);
        Assert.Equal(JTokenType.Boolean, background.Type);
        Assert.True(background.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeParametersWithNullBackground()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Background = null
        };
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
    public void TestCanSerializeParametersWithScale()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Scale = 1.5
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("scale"));
        JToken? scale = serialized["scale"];
        Assert.NotNull(scale);
        Assert.Equal(JTokenType.Float, scale.Type);
        Assert.Equal(1.5, scale.Value<double>());
    }

    [Fact]
    public void TestCanSerializeParametersWithNullScale()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Scale = null
        };
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
    public void TestSettingScaleToInvalidValueThrows()
    {
        PrintCommandParameters properties = new("myContextId");

        Assert.Contains("Value must be between 0.1 and 2.0", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.Scale = -1).Message);
        Assert.Contains("Value must be between 0.1 and 2.0", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.Scale = 0.01).Message);
        Assert.Contains("Value must be between 0.1 and 2.0", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.Scale = 2.01).Message);
        Assert.Contains("Value must be between 0.1 and 2.0", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.Scale = 0).Message);
    }

    [Fact]
    public void TestCanSerializeParametersWithShrinkToFit()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            ShrinkToFit = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("shrinkToFit"));
        JToken? shrinkToFit = serialized["shrinkToFit"];
        Assert.NotNull(shrinkToFit);
        Assert.Equal(JTokenType.Boolean, shrinkToFit.Type);
        Assert.False(shrinkToFit.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeParametersWithNullShrinkToFit()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            ShrinkToFit = null
        };
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
    public void TestCanSerializeParametersWithPageRanges()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            PageRanges =
            [
                1,
                "3-5",
                10L,
            ]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("pageRanges"));
        JToken? pageRangesToken = serialized["pageRanges"];
        Assert.NotNull(pageRangesToken);
        Assert.Equal(JTokenType.Array, pageRangesToken.Type);

        JArray? pageRanges = pageRangesToken as JArray;
        Assert.NotNull(pageRanges);
        Assert.Equal(3, pageRanges.Count);

        Assert.Equal(JTokenType.Integer, pageRanges[0].Type);
        Assert.Equal(1L, pageRanges[0].Value<long>());
        Assert.Equal(JTokenType.String, pageRanges[1].Type);
        Assert.Equal("3-5", pageRanges[1].Value<string>());
        Assert.Equal(JTokenType.Integer, pageRanges[2].Type);
        Assert.Equal(10L, pageRanges[2].Value<long>());
    }

    [Fact]
    public void TestCanSerializeParametersWithNoPageRanges()
    {
        PrintCommandParameters properties = new("myContextId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());
    }
}
