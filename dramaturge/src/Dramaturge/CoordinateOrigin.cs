// <copyright file="CoordinateOrigin.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// The point page coordinates are measured from.
/// </summary>
public enum CoordinateOrigin
{
    /// <summary>
    /// The top left corner of the page's viewport, as <see cref="Page.Mouse"/> coordinates are.
    /// </summary>
    Viewport,

    /// <summary>
    /// The top left corner of the page's document, whatever is scrolled into view, as a full-page screenshot's
    /// coordinates are.
    /// </summary>
    Document,
}
