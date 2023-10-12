// <copyright file="ScreenshotOrigin.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The enumerated value of origins for screenshot.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ScreenshotOrigin>))]
public enum ScreenshotOrigin
{
    /// <summary>
    /// The origin of the clip rectangle is relative to the viewport of the browser.
    /// </summary>
    Viewport,

    /// <summary>
    /// The origin of the clip rectangle is relative to document origin.
    /// </summary>
    Document,
}
