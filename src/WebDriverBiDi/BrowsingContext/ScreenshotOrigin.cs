// <copyright file="ScreenshotOrigin.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The enumerated values for the area a screenshot captures.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ScreenshotOrigin>))]
public enum ScreenshotOrigin
{
    /// <summary>
    /// The screenshot captures the visual viewport. A box clip rectangle is positioned relative to it; an
    /// element clip rectangle is positioned at its element whatever the origin.
    /// </summary>
    Viewport,

    /// <summary>
    /// The screenshot captures the whole document, producing a full-page image. A box clip rectangle is
    /// positioned relative to the document origin; an element clip rectangle is positioned at its element.
    /// </summary>
    Document,
}
