namespace WebDriverBiDi.Emulation;

using System.Drawing;
using System.Text.Json;
using Newtonsoft.Json.Linq;

public class MediaFeaturesTests
{
    [Fact]
    public async Task CanSerialize()
    {
        // This tests that unset, null values do not serialize null in the JSON.
        MediaFeatures features = new();
        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Empty(serialized);
    }

    [Fact]
    public async Task CanSerializeAnyHoverValue()
    {
        MediaFeatures features = new()
        {
            AnyHover = AnyHoverMediaFeatureValue.None,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("any-hover"));
        JToken? token = serialized["any-hover"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("none", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeAnyHoverSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            AnyHover = AnyHoverMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("any-hover"));
        JToken? token = serialized["any-hover"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeAnyPointerValue()
    {
        MediaFeatures features = new()
        {
            AnyPointer = AnyPointerMediaFeatureValue.Fine,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("any-pointer"));
        JToken? token = serialized["any-pointer"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("fine", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeAnyPointerSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            AnyPointer = AnyPointerMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("any-pointer"));
        JToken? token = serialized["any-pointer"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeColorValue()
    {
        MediaFeatures features = new()
        {
            Color = 1,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("color"));
        JToken? token = serialized["color"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Integer, token.Type);
        Assert.Equal(1, token.Value<long>());
    }

    [Fact]
    public async Task CanSerializeColorSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            Color = MediaFeatures.ResetColorValue,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("color"));
        JToken? token = serialized["color"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeColorGamutValue()
    {
        MediaFeatures features = new()
        {
            ColorGamut = ColorGamutMediaFeatureValue.Srgb,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("color-gamut"));
        JToken? token = serialized["color-gamut"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("srgb", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeColorGamutSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            ColorGamut = ColorGamutMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("color-gamut"));
        JToken? token = serialized["color-gamut"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeColorIndexValue()
    {
        MediaFeatures features = new()
        {
            ColorIndex = 1,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("color-index"));
        JToken? token = serialized["color-index"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Integer, token.Type);
        Assert.Equal(1, token.Value<long>());
    }

    [Fact]
    public async Task CanSerializeColorIndexSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            ColorIndex = MediaFeatures.ResetColorIndexValue,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("color-index"));
        JToken? token = serialized["color-index"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeDisplayModeValue()
    {
        MediaFeatures features = new()
        {
            DisplayMode = DisplayModeMediaFeatureValue.PictureInPicture,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("display-mode"));
        JToken? token = serialized["display-mode"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("picture-in-picture", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeDisplayModeNullValue()
    {
        MediaFeatures features = new()
        {
            DisplayMode = DisplayModeMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("display-mode"));
        JToken? token = serialized["display-mode"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeDynamicRangeValue()
    {
        MediaFeatures features = new()
        {
            DynamicRange = DynamicRangeMediaFeatureValue.Standard,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("dynamic-range"));
        JToken? token = serialized["dynamic-range"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("standard", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeDynamicRangeNullValue()
    {
        MediaFeatures features = new()
        {
            DynamicRange = DynamicRangeMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("dynamic-range"));
        JToken? token = serialized["dynamic-range"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeEnvironmentBlendingValue()
    {
        MediaFeatures features = new()
        {
            EnvironmentBlending = EnvironmentBlendingMediaFeatureValue.Subtractive,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("environment-blending"));
        JToken? token = serialized["environment-blending"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("subtractive", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeEnvironmentBlendingNullValue()
    {
        MediaFeatures features = new()
        {
            EnvironmentBlending = EnvironmentBlendingMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("environment-blending"));
        JToken? token = serialized["environment-blending"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeForcedColorsValue()
    {
        MediaFeatures features = new()
        {
            ForcedColors = ForcedColorsMediaFeatureValue.Active,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("forced-colors"));
        JToken? token = serialized["forced-colors"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("active", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeForcedColorsNullValue()
    {
        MediaFeatures features = new()
        {
            ForcedColors = ForcedColorsMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("forced-colors"));
        JToken? token = serialized["forced-colors"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeGridValue()
    {
        MediaFeatures features = new()
        {
            Grid = 1,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("grid"));
        JToken? token = serialized["grid"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Integer, token.Type);
        Assert.Equal(1, token.Value<long>());
    }

    [Fact]
    public async Task CanSerializeGridSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            Grid = MediaFeatures.ResetGridValue,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("grid"));
        JToken? token = serialized["grid"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeHorizontalViewportSegmentsValue()
    {
        MediaFeatures features = new()
        {
            HorizontalViewportSegments = 1,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("horizontal-viewport-segments"));
        JToken? token = serialized["horizontal-viewport-segments"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Integer, token.Type);
        Assert.Equal(1, token.Value<long>());
    }

    [Fact]
    public async Task CanSerializeHorizontalViewportSegmentsSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            HorizontalViewportSegments = MediaFeatures.ResetHorizonalViewportSegmentsValue,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("horizontal-viewport-segments"));
        JToken? token = serialized["horizontal-viewport-segments"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeHoverValue()
    {
        MediaFeatures features = new()
        {
            Hover = HoverMediaFeatureValue.None,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("hover"));
        JToken? token = serialized["hover"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("none", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeHoverSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            Hover = HoverMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("hover"));
        JToken? token = serialized["hover"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeInvertedColorsValue()
    {
        MediaFeatures features = new()
        {
            InvertedColors = InvertedColorsMediaFeatureValue.None,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("inverted-colors"));
        JToken? token = serialized["inverted-colors"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("none", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeInvertedColorsSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            InvertedColors = InvertedColorsMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("inverted-colors"));
        JToken? token = serialized["inverted-colors"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeMonochromeValue()
    {
        MediaFeatures features = new()
        {
            Monochrome = 1,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("monochrome"));
        JToken? token = serialized["monochrome"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Integer, token.Type);
        Assert.Equal(1, token.Value<long>());
    }

    [Fact]
    public async Task CanSerializeMonochromeSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            Monochrome = MediaFeatures.ResetMonochromeValue,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("monochrome"));
        JToken? token = serialized["monochrome"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeNavControlsValue()
    {
        MediaFeatures features = new()
        {
            NavControls = NavControlsMediaFeatureValue.None,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("nav-controls"));
        JToken? token = serialized["nav-controls"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("none", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeNavControlsSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            NavControls = NavControlsMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("nav-controls"));
        JToken? token = serialized["nav-controls"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeOverflowBlockValue()
    {
        MediaFeatures features = new()
        {
            OverflowBlock = OverflowBlockMediaFeatureValue.None,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("overflow-block"));
        JToken? token = serialized["overflow-block"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("none", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeOverflowBlockSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            OverflowBlock = OverflowBlockMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("overflow-block"));
        JToken? token = serialized["overflow-block"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeOverflowInlineValue()
    {
        MediaFeatures features = new()
        {
            OverflowInline = OverflowInlineMediaFeatureValue.None,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("overflow-inline"));
        JToken? token = serialized["overflow-inline"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("none", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeOverflowInlineSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            OverflowInline = OverflowInlineMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("overflow-inline"));
        JToken? token = serialized["overflow-inline"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializePointerValue()
    {
        MediaFeatures features = new()
        {
            Pointer = PointerMediaFeatureValue.None,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("pointer"));
        JToken? token = serialized["pointer"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("none", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializePointerSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            Pointer = PointerMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("pointer"));
        JToken? token = serialized["pointer"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializePrefersColorSchemeValue()
    {
        MediaFeatures features = new()
        {
            PrefersColorScheme = PrefersColorSchemeFeatureValue.Light,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("prefers-color-scheme"));
        JToken? token = serialized["prefers-color-scheme"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("light", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializePrefersColorSchemeSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            PrefersColorScheme = PrefersColorSchemeFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("prefers-color-scheme"));
        JToken? token = serialized["prefers-color-scheme"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializePrefersContrastValue()
    {
        MediaFeatures features = new()
        {
            PrefersContrast = PrefersContrastMediaFeatureValue.NoPreference,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("prefers-contrast"));
        JToken? token = serialized["prefers-contrast"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("no-preference", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializePrefersContrastSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            PrefersContrast = PrefersContrastMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("prefers-contrast"));
        JToken? token = serialized["prefers-contrast"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializePrefersReducedDataValue()
    {
        MediaFeatures features = new()
        {
            PrefersReducedData = PrefersReducedDataMediaFeatureValue.NoPreference,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("prefers-reduced-data"));
        JToken? token = serialized["prefers-reduced-data"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("no-preference", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializePrefersReducedDataSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            PrefersReducedData = PrefersReducedDataMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("prefers-reduced-data"));
        JToken? token = serialized["prefers-reduced-data"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializePrefersReducedMotionValue()
    {
        MediaFeatures features = new()
        {
            PrefersReducedData = PrefersReducedDataMediaFeatureValue.NoPreference,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("prefers-reduced-data"));
        JToken? token = serialized["prefers-reduced-data"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("no-preference", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializePrefersReducedMotionSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            PrefersReducedData = PrefersReducedDataMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("prefers-reduced-data"));
        JToken? token = serialized["prefers-reduced-data"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializePrefersReducedTransparencyValue()
    {
        MediaFeatures features = new()
        {
            PrefersReducedMotion = PrefersReducedMotionMediaFeatureValue.NoPreference,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("prefers-reduced-motion"));
        JToken? token = serialized["prefers-reduced-motion"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("no-preference", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializePrefersReducedTransparencySentinelNullValue()
    {
        MediaFeatures features = new()
        {
            PrefersReducedTransparency = PrefersReducedTransparencyMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("prefers-reduced-transparency"));
        JToken? token = serialized["prefers-reduced-transparency"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeScanValue()
    {
        MediaFeatures features = new()
        {
            Scan = ScanMediaFeatureValue.Interlace,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("scan"));
        JToken? token = serialized["scan"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("interlace", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeScanSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            Scan = ScanMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("scan"));
        JToken? token = serialized["scan"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeScriptingValue()
    {
        MediaFeatures features = new()
        {
            Scripting = ScriptingMediaFeatureValue.None,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("scripting"));
        JToken? token = serialized["scripting"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("none", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeScriptingSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            Scripting = ScriptingMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("scripting"));
        JToken? token = serialized["scripting"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeUpdateValue()
    {
        MediaFeatures features = new()
        {
            Update = UpdateMediaFeatureValue.None,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("update"));
        JToken? token = serialized["update"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("none", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeUpdateSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            Update = UpdateMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("update"));
        JToken? token = serialized["update"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeVerticalViewportSegmentsValue()
    {
        MediaFeatures features = new()
        {
            VerticalViewportSegments = 1,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("vertical-viewport-segments"));
        JToken? token = serialized["vertical-viewport-segments"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Integer, token.Type);
        Assert.Equal(1, token.Value<long>());
    }

    [Fact]
    public async Task CanSerializeVerticalViewportSegmentsSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            VerticalViewportSegments = MediaFeatures.ResetHorizonalViewportSegmentsValue,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("vertical-viewport-segments"));
        JToken? token = serialized["vertical-viewport-segments"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeVideoColorGamutValue()
    {
        MediaFeatures features = new()
        {
            VideoColorGamut = VideoColorGamutMediaFeatureValue.Srgb,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("video-color-gamut"));
        JToken? token = serialized["video-color-gamut"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("srgb", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeVideoColorGamutSentinelNullValue()
    {
        MediaFeatures features = new()
        {
            VideoColorGamut = VideoColorGamutMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("video-color-gamut"));
        JToken? token = serialized["video-color-gamut"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }

    [Fact]
    public async Task CanSerializeVideoDynamicRangeValue()
    {
        MediaFeatures features = new()
        {
            VideoDynamicRange = VideoDynamicRangeMediaFeatureValue.Standard,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("video-dynamic-range"));
        JToken? token = serialized["video-dynamic-range"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.String, token.Type);
        Assert.Equal("standard", token.Value<string>());
    }

    [Fact]
    public async Task CanSerializeVideoDynamicRangeNullValue()
    {
        MediaFeatures features = new()
        {
            VideoDynamicRange = VideoDynamicRangeMediaFeatureValue.Reset,
        };

        string json = JsonSerializer.Serialize(features);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("video-dynamic-range"));
        JToken? token = serialized["video-dynamic-range"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Null, token.Type);
    }
}
