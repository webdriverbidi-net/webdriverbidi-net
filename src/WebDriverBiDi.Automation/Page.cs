// <copyright file="Page.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

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
    public Task<string> NavigateAsync(string url, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.MainFrame.NavigateAsync(url, wait, timeout, cancellationToken);
    }

    /// <summary>
    /// Reloads the page.
    /// </summary>
    /// <param name="wait">How far the reloaded document must load before the reload completes.</param>
    /// <param name="timeout">The time the reload may take, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the reload.</param>
    /// <returns>The URL reloaded.</returns>
    public Task<string> ReloadAsync(ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.MainFrame.ReloadAsync(wait, timeout, cancellationToken);
    }

    /// <summary>
    /// Navigates back one step in the page's history, then waits for the navigation: a document, loaded as far as
    /// a state, or a change of URL within the document.
    /// </summary>
    /// <param name="wait">How far a new document must load.</param>
    /// <param name="timeout">The time the navigation may take, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the navigation.</param>
    /// <returns>The page's URL after the navigation.</returns>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the page does not navigate, or the document does not load, in time.</exception>
    public Task<string> GoBackAsync(ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraverseHistoryAsync(-1, wait, timeout, cancellationToken);
    }

    /// <summary>
    /// Navigates forward one step in the page's history, then waits for the navigation: a document, loaded as far
    /// as a state, or a change of URL within the document.
    /// </summary>
    /// <param name="wait">How far a new document must load.</param>
    /// <param name="timeout">The time the navigation may take, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the navigation.</param>
    /// <returns>The page's URL after the navigation.</returns>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the page does not navigate, or the document does not load, in time.</exception>
    public Task<string> GoForwardAsync(ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraverseHistoryAsync(1, wait, timeout, cancellationToken);
    }

    /// <summary>
    /// Waits for the page's document to load as far as a state, as <see cref="Frame.WaitForLoadStateAsync(ReadinessState, TimeSpan?, CancellationToken)"/> does
    /// for the main frame.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>A task that completes when the document has loaded as far as the state.</returns>
    public Task WaitForLoadStateAsync(ReadinessState state = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.MainFrame.WaitForLoadStateAsync(state, timeout, cancellationToken);
    }

    /// <summary>
    /// Waits for the page's URL to be exactly a URL, as <see cref="Frame.WaitForUrlAsync(string, ReadinessState, TimeSpan?, CancellationToken)"/> does for the main frame.
    /// </summary>
    /// <param name="url">The full URL.</param>
    /// <param name="wait">How far the document must load once the URL matches.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The page's URL.</returns>
    public Task<string> WaitForUrlAsync(string url, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.MainFrame.WaitForUrlAsync(url, wait, timeout, cancellationToken);
    }

    /// <summary>
    /// Waits for the page's URL to match a regular expression, as <see cref="Frame.WaitForUrlAsync(Regex, ReadinessState, TimeSpan?, CancellationToken)"/> does for the main frame.
    /// </summary>
    /// <param name="url">The regular expression, matched against the full URL.</param>
    /// <param name="wait">How far the document must load once the URL matches.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The page's URL.</returns>
    public Task<string> WaitForUrlAsync(Regex url, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.MainFrame.WaitForUrlAsync(url, wait, timeout, cancellationToken);
    }

    /// <summary>
    /// Waits for the page's URL to satisfy a condition, as <see cref="Frame.WaitForUrlAsync(Func{string, bool}, ReadinessState, TimeSpan?, CancellationToken)"/> does for the main frame.
    /// </summary>
    /// <param name="url">The condition, given the full URL.</param>
    /// <param name="wait">How far the document must load once the URL matches.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The page's URL.</returns>
    public Task<string> WaitForUrlAsync(Func<string, bool> url, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.MainFrame.WaitForUrlAsync(url, wait, timeout, cancellationToken);
    }

    /// <summary>
    /// Runs an action that navigates the page, and waits for the navigation, as
    /// <see cref="Frame.RunAndWaitForNavigationAsync"/> does for the main frame.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="wait">How far a new document must load.</param>
    /// <param name="timeout">The time to wait, from the start of the action, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The page's URL after the navigation.</returns>
    public Task<string> RunAndWaitForNavigationAsync(Func<Task> action, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.MainFrame.RunAndWaitForNavigationAsync(action, wait, timeout, cancellationToken);
    }

    /// <summary>
    /// Calls a JavaScript function in the page's main frame, as <see cref="Frame.EvaluateAsync(string, IEnumerable{LocalValue}?, TimeSpan?, CancellationToken)"/> does.
    /// </summary>
    /// <param name="function">The function's declaration.</param>
    /// <param name="arguments">The arguments, or <see langword="null"/> for none.</param>
    /// <param name="timeout">The time the call may take, or <see langword="null"/> for <see cref="AutomationOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the call.</param>
    /// <returns>The function's result.</returns>
    public Task<RemoteValue> EvaluateAsync(string function, IEnumerable<LocalValue>? arguments = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.MainFrame.EvaluateAsync(function, arguments, timeout, cancellationToken);
    }

    /// <summary>
    /// Calls a JavaScript function in the page's main frame and converts its result, as
    /// <see cref="Frame.EvaluateAsync{T}"/> does.
    /// </summary>
    /// <typeparam name="T">The type to convert the result to.</typeparam>
    /// <param name="function">The function's declaration.</param>
    /// <param name="arguments">The arguments, or <see langword="null"/> for none.</param>
    /// <param name="timeout">The time the call may take, or <see langword="null"/> for <see cref="AutomationOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the call.</param>
    /// <returns>The function's result, converted.</returns>
    public Task<T> EvaluateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string function, IEnumerable<LocalValue>? arguments = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.MainFrame.EvaluateAsync<T>(function, arguments, timeout, cancellationToken);
    }

    /// <summary>
    /// Calls a JavaScript function in the page's main frame until it returns a truthy value, as
    /// <see cref="Frame.WaitForFunctionAsync(string, IEnumerable{LocalValue}?, TimeSpan?, CancellationToken)"/> does.
    /// </summary>
    /// <param name="function">The function's declaration.</param>
    /// <param name="arguments">The arguments, or <see langword="null"/> for none.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="AutomationOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The truthy value.</returns>
    public Task<RemoteValue> WaitForFunctionAsync(string function, IEnumerable<LocalValue>? arguments = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.MainFrame.WaitForFunctionAsync(function, arguments, timeout, cancellationToken);
    }

    /// <summary>
    /// Calls a JavaScript function in the page's main frame until it returns a truthy value, and converts the
    /// value, as <see cref="Frame.WaitForFunctionAsync{T}"/> does.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="function">The function's declaration.</param>
    /// <param name="arguments">The arguments, or <see langword="null"/> for none.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="AutomationOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The truthy value, converted.</returns>
    public Task<T> WaitForFunctionAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string function, IEnumerable<LocalValue>? arguments = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.MainFrame.WaitForFunctionAsync<T>(function, arguments, timeout, cancellationToken);
    }

    /// <summary>
    /// Replaces the page's document's contents with HTML, as <see cref="Frame.SetContentAsync"/> does for the main
    /// frame.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <param name="wait">How far the document must load.</param>
    /// <param name="timeout">The time it may take, or <see langword="null"/> for <see cref="AutomationOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>A task that completes when the document has loaded as far as the state.</returns>
    public Task SetContentAsync(string html, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.MainFrame.SetContentAsync(html, wait, timeout, cancellationToken);
    }

    /// <summary>
    /// Adds a script that runs in each document the page or its frames load from now on, in the page's own script
    /// realm, before the document's own scripts. It does not run in documents already loaded.
    /// </summary>
    /// <param name="function">The script, as a function of no arguments, such as <c>() =&gt; { window.seed = 42; }</c>.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>The added script, which can be removed.</returns>
    public async Task<InitScript> AddInitScriptAsync(string function, CancellationToken cancellationToken = default)
    {
        AddPreloadScriptCommandParameters parameters = new(function);
        parameters.Contexts.Add(this.Id);
        AddPreloadScriptCommandResult result = await this.Browser.Group.Driver.Script.AddPreloadScriptAsync(parameters, cancellationToken: cancellationToken).ConfigureAwait(false);
        return new InitScript(this.Browser.Group.Driver, result.PreloadScriptId);
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

    // History traversal has no wait of its own, so the navigation it causes is awaited from the frame's events.
    private async Task<string> TraverseHistoryAsync(long delta, ReadinessState wait, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        TimeBudget budget = new(timeout ?? this.Browser.Group.Options.NavigationTimeout, this.Browser.Group.Options.TimeProvider, cancellationToken);
        int before = this.MainFrame.Navigations;
        await this.Browser.Group.Driver.BrowsingContext.TraverseHistoryAsync(new TraverseHistoryCommandParameters(this.Id, delta), budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
        await this.MainFrame.WaitForNavigationAsync(before, wait, budget).ConfigureAwait(false);
        return this.Url;
    }
}
