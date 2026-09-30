// <copyright file="Frame.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Text.RegularExpressions;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

/// <summary>
/// A document of a <see cref="Automation.Page"/>: its main frame, or an iframe within it, each a browsing context.
/// </summary>
public sealed class Frame
{
    private readonly object stateLock = new();
    private string url;
    private bool isDetached;
    private int navigationsStarted;
    private LoadState? loadState;
    private int navigations;
    private int stateVersion;
    private TaskCompletionSource<bool> stateChanged = new(TaskCreationOptions.RunContinuationsAsynchronously);

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
    public string Url
    {
        get
        {
            lock (this.stateLock)
            {
                return this.url;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the frame has been removed from its page, or its page closed.
    /// </summary>
    public bool IsDetached
    {
        get
        {
            lock (this.stateLock)
            {
                return this.isDetached;
            }
        }
    }

    /// <summary>
    /// Gets the frames directly within this one.
    /// </summary>
    public IReadOnlyList<Frame> ChildFrames => this.Page.GetChildFrames(this);

    /// <summary>
    /// Gets the number of navigations the browser has reported starting in the frame, so that an operation can tell
    /// whether its document may have been replaced while it ran.
    /// </summary>
    internal int NavigationsStarted => Volatile.Read(ref this.navigationsStarted);

    /// <summary>
    /// Gets the number of navigations the browser has reported in the frame, new documents and changes of URL
    /// within one, so that an operation can wait for the next.
    /// </summary>
    internal int Navigations
    {
        get
        {
            lock (this.stateLock)
            {
                return this.navigations;
            }
        }
    }

    private BrowserGroup Group => this.Page.Browser.Group;

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
    /// Navigates the frame to a URL.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="wait">How far the new document must load before the navigation completes.</param>
    /// <param name="timeout">The time the navigation may take, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the navigation.</param>
    /// <returns>The URL navigated to, after any redirects.</returns>
    public async Task<string> NavigateAsync(string url, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        NavigateCommandParameters parameters = new(this.Id, url) { Wait = wait };
        NavigateCommandResult result = await this.Group.Driver.BrowsingContext.NavigateAsync(parameters, this.NavigationTimeout(timeout), cancellationToken).ConfigureAwait(false);
        return result.Url;
    }

    /// <summary>
    /// Reloads the frame.
    /// </summary>
    /// <param name="wait">How far the reloaded document must load before the reload completes.</param>
    /// <param name="timeout">The time the reload may take, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the reload.</param>
    /// <returns>The URL reloaded.</returns>
    public async Task<string> ReloadAsync(ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ReloadCommandParameters parameters = new(this.Id) { Wait = wait };
        ReloadCommandResult result = await this.Group.Driver.BrowsingContext.ReloadAsync(parameters, this.NavigationTimeout(timeout), cancellationToken).ConfigureAwait(false);
        return result.Url;
    }

    /// <summary>
    /// Waits for the frame's document to load as far as a state. A document whose state no event has reported,
    /// such as one loaded before the group started tracking the frame, or restored from the back/forward cache,
    /// is asked for its state.
    /// </summary>
    /// <param name="state">The state: <see cref="ReadinessState.Interactive"/> once the document is parsed, <see cref="ReadinessState.Complete"/> once it and its resources have loaded, or <see cref="ReadinessState.None"/> for no wait.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>A task that completes when the document has loaded as far as the state.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the frame is detached while waiting.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the document does not load in time.</exception>
    public Task WaitForLoadStateAsync(ReadinessState state = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.WaitForLoadStateAsync(state, this.CreateBudget(timeout, cancellationToken));
    }

    /// <summary>
    /// Waits for the frame's URL to be exactly a URL, then for its document to load as far as a state. It returns
    /// at once if the URL already matches and the document has loaded.
    /// </summary>
    /// <param name="url">The full URL.</param>
    /// <param name="wait">How far the document must load once the URL matches.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The frame's URL.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the frame is detached while waiting.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the URL does not match, or the document does not load, in time.</exception>
    public Task<string> WaitForUrlAsync(string url, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.WaitForUrlAsync(candidate => candidate == url, $"the URL {url}", wait, timeout, cancellationToken);
    }

    /// <summary>
    /// Waits for the frame's URL to match a regular expression, then for its document to load as far as a state.
    /// It returns at once if the URL already matches and the document has loaded.
    /// </summary>
    /// <param name="url">The regular expression, matched against the full URL.</param>
    /// <param name="wait">How far the document must load once the URL matches.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The frame's URL.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the frame is detached while waiting.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the URL does not match, or the document does not load, in time.</exception>
    public Task<string> WaitForUrlAsync(Regex url, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.WaitForUrlAsync(url.IsMatch, $"a URL matching {url}", wait, timeout, cancellationToken);
    }

    /// <summary>
    /// Waits for the frame's URL to satisfy a condition, then for its document to load as far as a state. It
    /// returns at once if the URL already satisfies it and the document has loaded.
    /// </summary>
    /// <param name="url">The condition, given the full URL.</param>
    /// <param name="wait">How far the document must load once the URL matches.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The frame's URL.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the frame is detached while waiting.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the URL does not match, or the document does not load, in time.</exception>
    public Task<string> WaitForUrlAsync(Func<string, bool> url, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.WaitForUrlAsync(url, "a URL satisfying the condition", wait, timeout, cancellationToken);
    }

    /// <summary>
    /// Runs an action that navigates the frame, such as a click on a link, and waits for the navigation: a new
    /// document, loaded as far as a state, or a change of URL within the document.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="wait">How far a new document must load.</param>
    /// <param name="timeout">The time to wait, from the start of the action, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The frame's URL after the navigation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the frame is detached while waiting.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the frame does not navigate, or the document does not load, in time.</exception>
    public async Task<string> RunAndWaitForNavigationAsync(Func<Task> action, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        TimeBudget budget = this.CreateBudget(timeout, cancellationToken);
        int before = this.Navigations;
        await action().ConfigureAwait(false);
        await this.WaitForNavigationAsync(before, wait, budget).ConfigureAwait(false);
        return this.Url;
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
    /// Waits for a navigation after a count of them, then for its document to load as far as a state.
    /// </summary>
    /// <param name="before">The count of navigations before the one awaited.</param>
    /// <param name="wait">How far a new document must load.</param>
    /// <param name="budget">The time the wait may take.</param>
    /// <returns>A task that completes when the frame has navigated and loaded.</returns>
    internal async Task WaitForNavigationAsync(int before, ReadinessState wait, TimeBudget budget)
    {
        await this.WaitUntilAsync(() => this.navigations != before, "a navigation of the frame", () => "no navigation was reported", budget).ConfigureAwait(false);
        await this.WaitForLoadStateAsync(wait, budget).ConfigureAwait(false);
    }

    /// <summary>
    /// Records that a navigation started in the frame.
    /// </summary>
    internal void RecordNavigationStarted()
    {
        Interlocked.Increment(ref this.navigationsStarted);
    }

    /// <summary>
    /// Records that the frame has a new document. Its load state is unknown until an event reports it or the
    /// document is asked: a document restored from the back/forward cache is loaded already, and no load event
    /// follows.
    /// </summary>
    /// <param name="newUrl">The document's URL.</param>
    internal void RecordNewDocument(string newUrl)
    {
        lock (this.stateLock)
        {
            this.url = newUrl;
            this.loadState = null;
            this.navigations++;
            this.SignalStateChange();
        }
    }

    /// <summary>
    /// Records that the frame's URL changed within its document, by a fragment or the history API.
    /// </summary>
    /// <param name="newUrl">The URL.</param>
    internal void RecordSameDocumentNavigation(string newUrl)
    {
        lock (this.stateLock)
        {
            this.url = newUrl;
            this.navigations++;
            this.SignalStateChange();
        }
    }

    /// <summary>
    /// Records how far the frame's document has loaded.
    /// </summary>
    /// <param name="state">The state.</param>
    internal void RecordLoadState(LoadState state)
    {
        lock (this.stateLock)
        {
            this.loadState = state;
            this.SignalStateChange();
        }
    }

    /// <summary>
    /// Marks the frame as detached.
    /// </summary>
    internal void MarkDetached()
    {
        lock (this.stateLock)
        {
            this.isDetached = true;
            this.SignalStateChange();
        }
    }

    private static LoadState ParseReadyState(string readyState)
    {
        return readyState switch
        {
            "loading" => LoadState.Loading,
            "interactive" => LoadState.Interactive,
            _ => LoadState.Complete,
        };
    }

    private TimeBudget CreateBudget(TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return new TimeBudget(this.NavigationTimeout(timeout), this.Group.Options.TimeProvider, cancellationToken);
    }

    private TimeSpan NavigationTimeout(TimeSpan? timeout)
    {
        return timeout ?? this.Group.Options.NavigationTimeout;
    }

    private async Task<string> WaitForUrlAsync(Func<string, bool> matches, string awaited, ReadinessState wait, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        TimeBudget budget = this.CreateBudget(timeout, cancellationToken);
        await this.WaitUntilAsync(() => matches(this.url), awaited, () => $"the frame's URL was {this.url}", budget).ConfigureAwait(false);
        await this.WaitForLoadStateAsync(wait, budget).ConfigureAwait(false);
        return this.Url;
    }

    private async Task WaitForLoadStateAsync(ReadinessState state, TimeBudget budget)
    {
        if (state == ReadinessState.None)
        {
            return;
        }

        LoadState target = state == ReadinessState.Interactive ? LoadState.Interactive : LoadState.Complete;
        await this.ReadUnknownLoadStateAsync(budget).ConfigureAwait(false);
        await this.WaitUntilAsync(() => this.loadState >= target, $"the frame's document to be {target.ToString().ToLowerInvariant()}", () => $"it was {this.loadState?.ToString().ToLowerInvariant() ?? "unknown"}", budget).ConfigureAwait(false);
    }

    // A document whose state no event has reported is asked; its answer is kept only if no event reported a newer
    // state meanwhile.
    private async Task ReadUnknownLoadStateAsync(TimeBudget budget)
    {
        int version;
        lock (this.stateLock)
        {
            if (this.loadState is not null)
            {
                return;
            }

            version = this.stateVersion;
        }

        RemoteValue readyState;
        try
        {
            readyState = await this.Group.Driver.Script.CallFunctionAsync(this.Id, "() => document.readyState", [], this.Group.Options.SandboxName, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
        }
        catch (WebDriverBiDiCommandException) when (this.StateChangedSince(version))
        {
            return;
        }

        lock (this.stateLock)
        {
            if (this.stateVersion == version)
            {
                this.loadState = ParseReadyState(readyState.As<StringRemoteValue>().Value);
                this.SignalStateChange();
            }
        }
    }

    private bool StateChangedSince(int version)
    {
        lock (this.stateLock)
        {
            return this.stateVersion != version;
        }
    }

    // Waits until a condition on the frame's state holds, checking it each time the state changes. The condition
    // and the description run under the state lock.
    private async Task WaitUntilAsync(Func<bool> isDone, string awaited, Func<string> describe, TimeBudget budget)
    {
        Task timedOut = budget.DelayAsync(budget.Remaining);
        while (true)
        {
            Task changed;
            lock (this.stateLock)
            {
                if (isDone())
                {
                    return;
                }

                if (this.isDetached)
                {
                    throw new InvalidOperationException($"The frame was detached while waiting for {awaited}.");
                }

                changed = this.stateChanged.Task;
            }

            if (await Task.WhenAny(changed, timedOut).ConfigureAwait(false) == timedOut)
            {
                await timedOut.ConfigureAwait(false);
                lock (this.stateLock)
                {
                    throw new WebDriverBiDiTimeoutException($"Timed out after {budget.Duration.TotalSeconds} seconds waiting for {awaited}; {describe()}.");
                }
            }
        }
    }

    private void SignalStateChange()
    {
        this.stateVersion++;
        TaskCompletionSource<bool> previous = this.stateChanged;
        this.stateChanged = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        previous.TrySetResult(true);
    }
}
