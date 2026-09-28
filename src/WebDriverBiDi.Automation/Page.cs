// <copyright file="Page.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.BrowsingContext;

/// <summary>
/// A tab or window of a <see cref="Automation.Browser"/>: a top-level browsing context, and the frames within it.
/// </summary>
public sealed class Page
{
    private readonly object lockObject = new();
    private readonly List<Frame> frames = [];
    private readonly ObservableEventInvocable<PageEventArgs> onClosed = new("automation.pageClosed");

    /// <summary>
    /// Initializes a new instance of the <see cref="Page"/> class.
    /// </summary>
    /// <param name="browser">The browser the page belongs to.</param>
    /// <param name="id">The ID of the page's browsing context.</param>
    /// <param name="url">The URL of the page's main frame.</param>
    internal Page(Browser browser, string id, string url)
    {
        this.Browser = browser;
        this.Id = id;
        this.MainFrame = new Frame(this, id, null, url);
        this.frames.Add(this.MainFrame);
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
    /// Gets the page's main frame, whose browsing context is the page's own.
    /// </summary>
    public Frame MainFrame { get; }

    /// <summary>
    /// Gets the URL of the page's main frame.
    /// </summary>
    public string Url => this.MainFrame.Url;

    /// <summary>
    /// Gets a value indicating whether the page has been closed.
    /// </summary>
    public bool IsClosed => this.MainFrame.IsDetached;

    /// <summary>
    /// Gets the page's frames: the main frame first, then the others in the order they were found.
    /// </summary>
    public IReadOnlyList<Frame> Frames
    {
        get
        {
            lock (this.lockObject)
            {
                return [.. this.frames];
            }
        }
    }

    /// <summary>
    /// Gets an observable event raised when the page closes.
    /// </summary>
    public ObservableEvent<PageEventArgs> OnClosed => this.onClosed;

    /// <summary>
    /// Creates a locator for elements in the page's main frame.
    /// </summary>
    /// <param name="locator">How the elements are found, such as a <see cref="CssLocator"/>.</param>
    /// <returns>The locator.</returns>
    public ElementLocator Locate(Locator locator)
    {
        return this.MainFrame.Locate(locator);
    }

    /// <summary>
    /// Creates a locator for elements, in the page's main frame, by their rendered text: by default, text containing
    /// <paramref name="text"/> ignoring case; with <paramref name="exact"/>, text matching it exactly, with case.
    /// The browser compares its own rendering of the text, without collapsing whitespace.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="exact">Whether the whole text must match, with case.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByText(string text, bool exact = false)
    {
        return this.MainFrame.GetByText(text, exact);
    }

    /// <summary>
    /// Creates a locator for input elements by their placeholder: by default, one containing <paramref name="text"/> ignoring case; with
    /// <paramref name="exact"/>, one matching it exactly, in the page's main frame.
    /// </summary>
    /// <param name="text">The placeholder text.</param>
    /// <param name="exact">Whether the whole placeholder must match, with case.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByPlaceholder(string text, bool exact = false)
    {
        return this.MainFrame.GetByPlaceholder(text, exact);
    }

    /// <summary>
    /// Creates a locator for elements, such as images, by their alternative text: by default, text containing <paramref name="text"/>
    /// ignoring case; with <paramref name="exact"/>, text matching it exactly, in the page's main frame.
    /// </summary>
    /// <param name="text">The alternative text.</param>
    /// <param name="exact">Whether the whole alternative text must match, with case.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByAltText(string text, bool exact = false)
    {
        return this.MainFrame.GetByAltText(text, exact);
    }

    /// <summary>
    /// Creates a locator for elements by their title attribute: by default, one containing <paramref name="text"/> ignoring case; with
    /// <paramref name="exact"/>, one matching it exactly, in the page's main frame.
    /// </summary>
    /// <param name="text">The title text.</param>
    /// <param name="exact">Whether the whole title must match, with case.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByTitle(string text, bool exact = false)
    {
        return this.MainFrame.GetByTitle(text, exact);
    }

    /// <summary>
    /// Creates a locator for elements by their test ID, the value of the <see cref="AutomationOptions.TestIdAttribute"/> attribute, in the page's main frame.
    /// </summary>
    /// <param name="testId">The test ID, matched exactly.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByTestId(string testId)
    {
        return this.MainFrame.GetByTestId(testId);
    }

    /// <summary>
    /// Creates a locator for elements by the accessibility role the browser computes for them, such as "button", and optionally the
    /// accessible name, which must match exactly, in the page's main frame.
    /// </summary>
    /// <param name="role">The role.</param>
    /// <param name="name">The exact accessible name, or <see langword="null"/> for any.</param>
    /// <param name="states">ARIA states the elements must have, such as checked, or <see langword="null"/> for any.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByRole(string role, string? name = null, RoleStates? states = null)
    {
        return this.MainFrame.GetByRole(role, name, states);
    }

    /// <summary>
    /// Creates a locator for elements, in the page's main frame, by their labels: the elements their aria-labelledby attribute refers
    /// to; failing that, their aria-label attribute; failing that, the label elements of a form control. By default a
    /// label must contain <paramref name="text"/> ignoring case; with <paramref name="exact"/>, it must match it
    /// exactly, with case. Labels are compared with runs of whitespace collapsed.
    /// </summary>
    /// <param name="text">The label text.</param>
    /// <param name="exact">Whether the whole label must match, with case.</param>
    /// <returns>The locator.</returns>
    public ElementLocator GetByLabel(string text, bool exact = false)
    {
        return this.MainFrame.GetByLabel(text, exact);
    }

    /// <summary>
    /// Navigates the page to a URL.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="wait">How far the new document must load before the navigation completes.</param>
    /// <param name="timeout">The time the navigation may take, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the navigation.</param>
    /// <returns>The URL navigated to, after any redirects.</returns>
    public async Task<string> NavigateAsync(string url, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        NavigateCommandParameters parameters = new(this.Id, url) { Wait = wait };
        NavigateCommandResult result = await this.Browser.Group.Driver.BrowsingContext.NavigateAsync(parameters, this.NavigationTimeout(timeout), cancellationToken).ConfigureAwait(false);
        return result.Url;
    }

    /// <summary>
    /// Reloads the page.
    /// </summary>
    /// <param name="wait">How far the reloaded document must load before the reload completes.</param>
    /// <param name="timeout">The time the reload may take, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the reload.</param>
    /// <returns>The URL reloaded.</returns>
    public async Task<string> ReloadAsync(ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ReloadCommandParameters parameters = new(this.Id) { Wait = wait };
        ReloadCommandResult result = await this.Browser.Group.Driver.BrowsingContext.ReloadAsync(parameters, this.NavigationTimeout(timeout), cancellationToken).ConfigureAwait(false);
        return result.Url;
    }

    /// <summary>
    /// Navigates back one step in the page's history. It completes once the history has moved, without waiting
    /// for a document to load.
    /// </summary>
    /// <param name="timeout">The time the command may take, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the history has moved.</returns>
    public Task GoBackAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.Browser.Group.Driver.BrowsingContext.TraverseHistoryAsync(new TraverseHistoryCommandParameters(this.Id, -1), this.NavigationTimeout(timeout), cancellationToken);
    }

    /// <summary>
    /// Navigates forward one step in the page's history. It completes once the history has moved, without waiting
    /// for a document to load.
    /// </summary>
    /// <param name="timeout">The time the command may take, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the history has moved.</returns>
    public Task GoForwardAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.Browser.Group.Driver.BrowsingContext.TraverseHistoryAsync(new TraverseHistoryCommandParameters(this.Id, 1), this.NavigationTimeout(timeout), cancellationToken);
    }

    /// <summary>
    /// Brings the page to the front of its window, making it the active tab.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the page is in front.</returns>
    public Task BringToFrontAsync(CancellationToken cancellationToken = default)
    {
        return this.Browser.Group.Driver.BrowsingContext.ActivateAsync(new ActivateCommandParameters(this.Id), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Closes the page.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the page is closed.</returns>
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        await this.Browser.Group.Driver.BrowsingContext.CloseAsync(new WebDriverBiDi.BrowsingContext.CloseCommandParameters(this.Id), cancellationToken: cancellationToken).ConfigureAwait(false);
        await this.Browser.Group.RemoveContextAsync(this.Id).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds a frame.
    /// </summary>
    /// <param name="frame">The frame.</param>
    internal void AddFrame(Frame frame)
    {
        lock (this.lockObject)
        {
            this.frames.Add(frame);
        }
    }

    /// <summary>
    /// Removes a frame and the frames within it, marking each detached.
    /// </summary>
    /// <param name="frame">The frame.</param>
    /// <returns>The frames removed.</returns>
    internal IReadOnlyList<Frame> RemoveFrame(Frame frame)
    {
        lock (this.lockObject)
        {
            List<Frame> removed = [frame];
            for (int i = 0; i < removed.Count; i++)
            {
                Frame parent = removed[i];
                removed.AddRange(this.frames.Where(candidate => candidate.ParentFrame == parent));
            }

            foreach (Frame detached in removed)
            {
                this.frames.Remove(detached);
                detached.MarkDetached();
            }

            return removed;
        }
    }

    /// <summary>
    /// Gets the frames directly within a frame.
    /// </summary>
    /// <param name="frame">The frame.</param>
    /// <returns>The frames.</returns>
    internal IReadOnlyList<Frame> GetChildFrames(Frame frame)
    {
        lock (this.lockObject)
        {
            return [.. this.frames.Where(candidate => candidate.ParentFrame == frame)];
        }
    }

    /// <summary>
    /// Raises <see cref="OnClosed"/>.
    /// </summary>
    /// <returns>A task that completes when observers are notified.</returns>
    internal Task NotifyClosedAsync()
    {
        return this.onClosed.InvokeNotifyObserversAsync(new PageEventArgs(this));
    }

    private TimeSpan NavigationTimeout(TimeSpan? timeout)
    {
        return timeout ?? this.Browser.Group.Options.NavigationTimeout;
    }
}
