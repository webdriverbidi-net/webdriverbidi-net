// <copyright file="Page.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

/// <summary>
/// A tab or window of a <see cref="Automation.Browser"/>: a top-level browsing context.
/// </summary>
public sealed class Page
{
    private volatile bool isClosed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Page"/> class.
    /// </summary>
    /// <param name="browser">The browser the page belongs to.</param>
    /// <param name="id">The ID of the page's browsing context.</param>
    internal Page(Browser browser, string id)
    {
        this.Browser = browser;
        this.Id = id;
    }

    /// <summary>
    /// Gets the ID of the page's browsing context.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the browser the page belongs to.
    /// </summary>
    public Browser Browser { get; }

    /// <summary>
    /// Gets a value indicating whether the page has been closed.
    /// </summary>
    public bool IsClosed => this.isClosed;

    /// <summary>
    /// Marks the page as closed.
    /// </summary>
    internal void MarkClosed()
    {
        this.isClosed = true;
    }
}
