// <copyright file="PdfOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi.BrowsingContext;

/// <summary>
/// Options for printing a page as a PDF. An option left <see langword="null"/> takes the browser's default.
/// </summary>
public sealed class PdfOptions
{
    /// <summary>
    /// Gets a value indicating whether to print background colors and images.
    /// </summary>
    public bool? Background { get; init; }

    /// <summary>
    /// Gets the page margins, in centimeters.
    /// </summary>
    public PrintMarginParameters? Margins { get; init; }

    /// <summary>
    /// Gets the page orientation.
    /// </summary>
    public PrintOrientation? Orientation { get; init; }

    /// <summary>
    /// Gets the page size, in centimeters.
    /// </summary>
    public PrintPageParameters? PageSize { get; init; }

    /// <summary>
    /// Gets the scale of the content.
    /// </summary>
    public double? Scale { get; init; }

    /// <summary>
    /// Gets a value indicating whether to shrink the content to fit the page width.
    /// </summary>
    public bool? ShrinkToFit { get; init; }

    /// <summary>
    /// Gets the pages to print, such as <c>1</c> or <c>"2-4"</c>; empty for all of them.
    /// </summary>
    public IList<PageRange> PageRanges { get; } = [];
}
