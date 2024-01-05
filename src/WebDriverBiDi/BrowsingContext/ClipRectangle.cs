// <copyright file="ClipRectangle.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// The abstract base class for a clipping rectangle for a screenshot.
/// </summary>
[JsonDerivedType(typeof(BoxClipRectangle))]
[JsonDerivedType(typeof(ElementClipRectangle))]
public abstract class ClipRectangle
{
    /// <summary>
    /// Gets the type of clip rectangle.
    /// </summary>
    [JsonPropertyName("type")]
    public abstract string Type { get; }
}
