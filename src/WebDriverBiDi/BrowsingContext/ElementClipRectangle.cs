// <copyright file="ElementClipRectangle.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;
using WebDriverBiDi.Script;

/// <summary>
/// The abstract base class for a clipping rectangle for a screenshot.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class ElementClipRectangle : ClipRectangle
{
    private SharedReference element;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementClipRectangle"/> class.
    /// </summary>
    /// <param name="element">The reference to the element to use to clip the screenshot.</param>
    public ElementClipRectangle(SharedReference element)
        : base()
    {
        this.element = element;
    }

    /// <summary>
    /// Gets the type of clip rectangle.
    /// </summary>
    public override string Type => "element";

    /// <summary>
    /// Gets or sets the element to use to clip the screenshot.
    /// </summary>
    [JsonProperty("element")]
    public SharedReference Element { get => this.element; set => this.element = value; }
}
