// <copyright file="PageScreenshotOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.BrowsingContext;

/// <summary>
/// Options for capturing an image of a page.
/// </summary>
public sealed class PageScreenshotOptions
{
    /// <summary>
    /// Gets a value indicating whether to capture the whole document rather than what the viewport shows.
    /// </summary>
    public bool FullPage { get; init; }

    /// <summary>
    /// Gets the part to capture, in CSS pixels from the top left corner of the viewport, or of the document when
    /// <see cref="FullPage"/> is <see langword="true"/>; or <see langword="null"/> for all of it.
    /// </summary>
    public BoxClipRectangle? Clip { get; init; }

    /// <summary>
    /// Gets the image format, or <see langword="null"/> for PNG.
    /// </summary>
    public ImageFormat? Format { get; init; }
}
