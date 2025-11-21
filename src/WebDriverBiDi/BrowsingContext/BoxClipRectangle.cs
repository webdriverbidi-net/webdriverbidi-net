// <copyright file="BoxClipRectangle.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The abstract base class for a clipping rectangle for a screenshot.
/// </summary>
public class BoxClipRectangle : ClipRectangle
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BoxClipRectangle"/> class.
    /// </summary>
    public BoxClipRectangle()
        : base()
    {
    }

    /// <summary>
    /// Gets the type of clip rectangle.
    /// </summary>
    [JsonPropertyName("type")]
    public override string Type => "box";

    /// <summary>
    /// Gets or sets the X coordinate of the clip rectangle relative to the left edge of the viewport.
    /// </summary>
    [JsonPropertyName("x")]
    [JsonInclude]
    [JsonConverter(typeof(FixedDoubleJsonConverter))]
    public double X { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the Y coordinate of the clip rectangle relative to the left edge of the viewport.
    /// </summary>
    [JsonPropertyName("y")]
    [JsonInclude]
    [JsonConverter(typeof(FixedDoubleJsonConverter))]
    public double Y { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the width of the clip rectangle.
    /// </summary>
    [JsonPropertyName("width")]
    [JsonInclude]
    [JsonConverter(typeof(FixedDoubleJsonConverter))]
    public double Width { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the height of the clip rectangle.
    /// </summary>
    [JsonPropertyName("height")]
    [JsonInclude]
    [JsonConverter(typeof(FixedDoubleJsonConverter))]
    public double Height { get; set; } = 0.0;
}
