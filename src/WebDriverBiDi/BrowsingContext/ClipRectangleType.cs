// <copyright file="ClipRectangleType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The enumerated value of types of clip rectangles.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ClipRectangleType>))]
public enum ClipRectangleType
{
    /// <summary>
    /// The clip rectangle is relative to the viewport of the browser.
    /// </summary>
    Viewport,

    /// <summary>
    /// The clip rectangle is relative to an element.
    /// </summary>
    Element,
}
