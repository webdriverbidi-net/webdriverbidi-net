// <copyright file="ClipRectangleType.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

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