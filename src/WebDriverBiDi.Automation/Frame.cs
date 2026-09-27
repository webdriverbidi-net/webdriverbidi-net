// <copyright file="Frame.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.BrowsingContext;

/// <summary>
/// A document of a <see cref="Automation.Page"/>: its main frame, or an iframe within it, each a browsing context.
/// </summary>
public sealed class Frame
{
    private volatile string url;
    private volatile bool isDetached;

    /// <summary>
    /// Initializes a new instance of the <see cref="Frame"/> class.
    /// </summary>
    /// <param name="page">The page the frame belongs to.</param>
    /// <param name="id">The ID of the frame's browsing context.</param>
    /// <param name="parentFrame">The frame containing this one, or <see langword="null"/> for the main frame.</param>
    /// <param name="url">The frame's URL.</param>
    internal Frame(Page page, string id, Frame? parentFrame, string url)
    {
        this.Page = page;
        this.Id = id;
        this.ParentFrame = parentFrame;
        this.url = url;
    }

    /// <summary>
    /// Gets the ID of the frame's browsing context.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the page the frame belongs to.
    /// </summary>
    public Page Page { get; }

    /// <summary>
    /// Gets the frame containing this one, or <see langword="null"/> for the page's main frame.
    /// </summary>
    public Frame? ParentFrame { get; }

    /// <summary>
    /// Gets a value indicating whether this is the page's main frame.
    /// </summary>
    public bool IsMainFrame => this.ParentFrame is null;

    /// <summary>
    /// Gets the frame's URL, as of the latest navigation or history change the browser reported.
    /// </summary>
    public string Url => this.url;

    /// <summary>
    /// Gets a value indicating whether the frame has been removed from its page, or its page closed.
    /// </summary>
    public bool IsDetached => this.isDetached;

    /// <summary>
    /// Gets the frames directly within this one.
    /// </summary>
    public IReadOnlyList<Frame> ChildFrames => this.Page.GetChildFrames(this);

    /// <summary>
    /// Creates a locator for elements in this frame.
    /// </summary>
    /// <param name="locator">How the elements are found, such as a <see cref="CssLocator"/>.</param>
    /// <returns>The locator.</returns>
    public ElementLocator Locate(Locator locator)
    {
        return new ElementLocator(this, locator);
    }

    /// <summary>
    /// Records the frame's new URL.
    /// </summary>
    /// <param name="newUrl">The URL.</param>
    internal void SetUrl(string newUrl)
    {
        this.url = newUrl;
    }

    /// <summary>
    /// Marks the frame as detached.
    /// </summary>
    internal void MarkDetached()
    {
        this.isDetached = true;
    }
}
