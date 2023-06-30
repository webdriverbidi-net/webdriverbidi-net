// <copyright file="BoxClipRectangle.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// The abstract base class for a clipping rectangle for a screenshot.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class BoxClipRectangle : ClipRectangle
{
    private double x = 0.0;
    private double y = 0.0;
    private double width = 0.0;
    private double height = 0.0;

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
    public override string Type => "viewport";

    /// <summary>
    /// Gets or sets the X coordinate of the clip rectangle relative to the left edge of the viewport.
    /// </summary>
    [JsonProperty("x")]
    public double X { get => this.x; set => this.x = value; }

    /// <summary>
    /// Gets or sets the Y coordinate of the clip rectangle relative to the left edge of the viewport.
    /// </summary>
    [JsonProperty("y")]
    public double Y { get => this.y; set => this.y = value; }

    /// <summary>
    /// Gets or sets the width of the clip rectangle.
    /// </summary>
    [JsonProperty("width")]
    public double Width { get => this.width; set => this.width = value; }

    /// <summary>
    /// Gets or sets the height of the clip rectangle.
    /// </summary>
    [JsonProperty("height")]
    public double Height { get => this.height; set => this.height = value; }
}
