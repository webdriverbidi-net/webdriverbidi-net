// <copyright file="MediaFeatures.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// A data class representing the set of allowable CSS media feature override values.
/// </summary>
public class MediaFeatures
{
    /// <summary>
    /// Value to indicate resetting the emulation of the "color" CSS media feature.
    /// </summary>
    public const long ResetColorValue = -1;

    /// <summary>
    /// Value to indicate resetting the emulation of the "color-index" CSS media feature.
    /// </summary>
    public const long ResetColorIndexValue = -1;

    /// <summary>
    /// Value to indicate resetting the emulation of the "grid" CSS media feature.
    /// </summary>
    public const long ResetGridValue = -1;

    /// <summary>
    /// Value to indicate resetting the emulation of the "horizontal-viewport-segments" CSS media feature.
    /// </summary>
    public const long ResetHorizonalViewportSegmentsValue = -1;

    /// <summary>
    /// Value to indicate resetting the emulation of the "monochrome" CSS media feature.
    /// </summary>
    public const long ResetMonochromeValue = -1;

    /// <summary>
    /// Value to indicate resetting the emulation of the "vertical-viewport-segments" CSS media feature.
    /// </summary>
    public const long ResetVerticalViewportSegmentsValue = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaFeatures"/> class.
    /// </summary>
    public MediaFeatures()
    {
    }

    /// <summary>
    /// Gets or sets the value to emulate for the "any-hover" CSS media feature.
    /// Use <see cref="AnyHoverMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("any-hover")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AnyHoverMediaFeatureValue? AnyHover { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "any-pointer" CSS media feature.
    /// Use <see cref="AnyPointerMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("any-pointer")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AnyPointerMediaFeatureValue? AnyPointer { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "color" CSS media feature.
    /// Use <see cref="ResetColorValue"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("color")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(SentinelNullJsonConverter<long, NegativeLongSentinelChecker>))]
    public long? Color { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "color-gamut" CSS media feature.
    /// Use <see cref="ColorGamutMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("color-gamut")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ColorGamutMediaFeatureValue? ColorGamut { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "color" CSS media feature.
    /// Use <see cref="ResetColorIndexValue"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("color-index")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(SentinelNullJsonConverter<long, NegativeLongSentinelChecker>))]
    public long? ColorIndex { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "display-mode" CSS media feature.
    /// Use <see cref="DisplayModeMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("display-mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DisplayModeMediaFeatureValue? DisplayMode { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "dynamic-range" CSS media feature.
    /// Use <see cref="DynamicRangeMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("dynamic-range")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DynamicRangeMediaFeatureValue? DynamicRange { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "environment-blending" CSS media feature.
    /// Use <see cref="EnvironmentBlendingMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("environment-blending")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EnvironmentBlendingMediaFeatureValue? EnvironmentBlending { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "forced-colors" CSS media feature.
    /// Use <see cref="ForcedColorsMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("forced-colors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ForcedColorsMediaFeatureValue? ForcedColors { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "grid" CSS media feature.
    /// Use <see cref="ResetGridValue"/> to reset the emulation.
    /// </summary>
    /// <remarks>
    /// Note carefully that the only valid values of this property are zero (0)
    /// and one (1). Values other than those will result in an error at runtime.
    /// </remarks>
    [JsonPropertyName("grid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(SentinelNullJsonConverter<long, NegativeLongSentinelChecker>))]
    public long? Grid { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "horizontal-viewport-segments" CSS media feature.
    /// Use <see cref="ResetHorizonalViewportSegmentsValue"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("horizontal-viewport-segments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(SentinelNullJsonConverter<long, NegativeLongSentinelChecker>))]
    public long? HorizontalViewportSegments { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "hover" CSS media feature.
    /// Use <see cref="HoverMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("hover")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HoverMediaFeatureValue? Hover { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "inverted-colors" CSS media feature.
    /// Use <see cref="InvertedColorsMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("inverted-colors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InvertedColorsMediaFeatureValue? InvertedColors { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "monochrome" CSS media feature.
    /// Use <see cref="ResetMonochromeValue"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("monochrome")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(SentinelNullJsonConverter<long, NegativeLongSentinelChecker>))]
    public long? Monochrome { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "nav-controls" CSS media feature.
    /// Use <see cref="NavControlsMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("nav-controls")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NavControlsMediaFeatureValue? NavControls { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "overflow-block" CSS media feature.
    /// Use <see cref="OverflowBlockMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("overflow-block")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OverflowBlockMediaFeatureValue? OverflowBlock { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "overflow-inline" CSS media feature.
    /// Use <see cref="OverflowInlineMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("overflow-inline")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OverflowInlineMediaFeatureValue? OverflowInline { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "pointer" CSS media feature.
    /// Use <see cref="PointerMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("pointer")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PointerMediaFeatureValue? Pointer { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "prefers-color-scheme" CSS media feature.
    /// Use <see cref="PrefersColorSchemeFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("prefers-color-scheme")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PrefersColorSchemeFeatureValue? PrefersColorScheme { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "prefers-contrast" CSS media feature.
    /// Use <see cref="PrefersContrastMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("prefers-contrast")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PrefersContrastMediaFeatureValue? PrefersContrast { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "prefers-reduced-data" CSS media feature.
    /// Use <see cref="PrefersReducedDataMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("prefers-reduced-data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PrefersReducedDataMediaFeatureValue? PrefersReducedData { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "prefers-reduced-motion" CSS media feature.
    /// Use <see cref="PrefersReducedMotionMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("prefers-reduced-motion")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PrefersReducedMotionMediaFeatureValue? PrefersReducedMotion { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "prefers-reduced-transparency" CSS media feature.
    /// Use <see cref="PrefersReducedTransparencyMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("prefers-reduced-transparency")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PrefersReducedTransparencyMediaFeatureValue? PrefersReducedTransparency { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "scan" CSS media feature.
    /// Use <see cref="ScanMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("scan")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ScanMediaFeatureValue? Scan { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "scripting" CSS media feature.
    /// Use <see cref="ScriptingMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("scripting")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ScriptingMediaFeatureValue? Scripting { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "update" CSS media feature.
    /// Use <see cref="UpdateMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("update")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UpdateMediaFeatureValue? Update { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "vertical-viewport-segments" CSS media feature.
    /// Use <see cref="ResetVerticalViewportSegmentsValue"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("vertical-viewport-segments")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(SentinelNullJsonConverter<long, NegativeLongSentinelChecker>))]
    public long? VerticalViewportSegments { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "video-color-gamut" CSS media feature.
    /// Use <see cref="VideoColorGamutMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("video-color-gamut")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VideoColorGamutMediaFeatureValue? VideoColorGamut { get; set; }

    /// <summary>
    /// Gets or sets the value to emulate for the "video-dynamic-range" CSS media feature.
    /// Use <see cref="VideoDynamicRangeMediaFeatureValue.Reset"/> to reset the emulation.
    /// </summary>
    [JsonPropertyName("video-dynamic-range")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VideoDynamicRangeMediaFeatureValue? VideoDynamicRange { get; set; }
}
