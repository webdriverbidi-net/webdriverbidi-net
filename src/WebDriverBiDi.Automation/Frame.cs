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
    /// Creates a locator for elements, in this frame, by their rendered text: by default, text containing
    /// <paramref name="text"/> ignoring case; with <paramref name="exact"/>, text matching it exactly, with case.
    /// The browser compares its own rendering of the text, without collapsing whitespace.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="exact">Whether the whole text must match, with case.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByText(string text, bool exact = false)
    {
        return this.Query(ElementQuery.ByText(text, exact));
    }

    /// <summary>
    /// Creates a locator for input elements by their placeholder: by default, one containing <paramref name="text"/> ignoring case; with
    /// <paramref name="exact"/>, one matching it exactly, in this frame.
    /// </summary>
    /// <param name="text">The placeholder text.</param>
    /// <param name="exact">Whether the whole placeholder must match, with case.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByPlaceholder(string text, bool exact = false)
    {
        return this.Query(ElementQuery.ByAttribute("placeholder", "getByPlaceholder", text, exact));
    }

    /// <summary>
    /// Creates a locator for elements, such as images, by their alternative text: by default, text containing <paramref name="text"/>
    /// ignoring case; with <paramref name="exact"/>, text matching it exactly, in this frame.
    /// </summary>
    /// <param name="text">The alternative text.</param>
    /// <param name="exact">Whether the whole alternative text must match, with case.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByAltText(string text, bool exact = false)
    {
        return this.Query(ElementQuery.ByAttribute("alt", "getByAltText", text, exact));
    }

    /// <summary>
    /// Creates a locator for elements by their title attribute: by default, one containing <paramref name="text"/> ignoring case; with
    /// <paramref name="exact"/>, one matching it exactly, in this frame.
    /// </summary>
    /// <param name="text">The title text.</param>
    /// <param name="exact">Whether the whole title must match, with case.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByTitle(string text, bool exact = false)
    {
        return this.Query(ElementQuery.ByAttribute("title", "getByTitle", text, exact));
    }

    /// <summary>
    /// Creates a locator for elements by their test ID, the value of the <see cref="AutomationOptions.TestIdAttribute"/> attribute, in this frame.
    /// </summary>
    /// <param name="testId">The test ID, matched exactly.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByTestId(string testId)
    {
        return this.Query(ElementQuery.ByTestId(this.Page.Browser.Group.Options.TestIdAttribute, testId));
    }

    /// <summary>
    /// Creates a locator for elements by the accessibility role the browser computes for them, such as "button", and optionally the
    /// accessible name, which must match exactly, in this frame.
    /// </summary>
    /// <param name="role">The role.</param>
    /// <param name="name">The exact accessible name, or <see langword="null"/> for any.</param>
    /// <param name="states">ARIA states the elements must have, such as checked, or <see langword="null"/> for any.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByRole(string role, string? name = null, RoleStates? states = null)
    {
        return ElementLocator.ByRole(this, role, name, states);
    }

    /// <summary>
    /// Creates a locator for elements, in this frame, by their labels: the elements their aria-labelledby attribute refers
    /// to; failing that, their aria-label attribute; failing that, the label elements of a form control. By default a
    /// label must contain <paramref name="text"/> ignoring case; with <paramref name="exact"/>, it must match it
    /// exactly, with case. Labels are compared with runs of whitespace collapsed.
    /// </summary>
    /// <param name="text">The label text.</param>
    /// <param name="exact">Whether the whole label must match, with case.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByLabel(string text, bool exact = false)
    {
        return ElementLocator.ByLabel(this, text, exact);
    }

    /// <summary>
    /// Creates a locator for a GetBy helper's query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>The locator.</returns>
    internal ElementLocator Query(ElementQuery query)
    {
        return new ElementLocator(this, query);
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
