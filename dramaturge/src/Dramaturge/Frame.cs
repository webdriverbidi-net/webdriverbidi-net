// <copyright file="Frame.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

/// <summary>
/// A document of a <see cref="Dramaturge.Page"/>: its main frame, or an iframe within it, each a browsing context.
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
    /// Creates a locator for elements by their test ID, the value of the <see cref="DramaturgeOptions.TestIdAttribute"/> attribute, in this frame.
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
    /// <param name="role">The role. img is asked for as image, the name ARIA 1.3 gives it and browsers report.</param>
    /// <param name="name">The exact accessible name, or <see langword="null"/> for any.</param>
    /// <param name="states">ARIA states the elements must have, such as checked, or <see langword="null"/> for any.</param>
    /// <returns>The locator.</returns>
    /// <remarks>
    /// The browser computes the roles and names this matches. An accessibility snapshot, such as one from
    /// <see cref="Page.AriaSnapshotAsync"/>, computes names with the library's script in the page, so a name read from
    /// a snapshot can differ from the browser's, and find nothing here; to act on an element from a snapshot, use
    /// <see cref="AriaSnapshot.Locator"/> with its ref.
    /// </remarks>
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
    /// <param name="timeout">The time the navigation may take, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the navigation.</param>
    /// <returns>The URL navigated to, after any redirects.</returns>
    public Task<string> NavigateAsync(string url, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(Call("Navigate", "{url}", "goto", ("url", url)), this.CreateBudget(timeout, cancellationToken), budget => this.NavigateCoreAsync(url, wait, budget));
    }

    /// <summary>
    /// Reloads the frame.
    /// </summary>
    /// <param name="wait">How far the reloaded document must load before the reload completes.</param>
    /// <param name="timeout">The time the reload may take, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the reload.</param>
    /// <returns>The URL reloaded.</returns>
    public Task<string> ReloadAsync(ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(Call("Reload", null, "reload"), this.CreateBudget(timeout, cancellationToken), budget => this.ReloadCoreAsync(wait, budget));
    }

    /// <summary>
    /// Waits for the frame's document to load as far as a state. A document whose state no event has reported,
    /// such as one loaded before the group started tracking the frame, or restored from the back/forward cache,
    /// is asked for its state.
    /// </summary>
    /// <param name="state">The state: <see cref="ReadinessState.Interactive"/> once the document is parsed, <see cref="ReadinessState.Complete"/> once it and its resources have loaded, or <see cref="ReadinessState.None"/> for no wait.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>A task that completes when the document has loaded as far as the state.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the frame is detached while waiting.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the document does not load in time.</exception>
    public Task WaitForLoadStateAsync(ReadinessState state = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(Call("Wait for load state", "{state}", "waitForLoadState", ("state", state.ToString())), this.CreateBudget(timeout, cancellationToken), budget => this.WaitForLoadStateAsync(state, budget));
    }

    /// <summary>
    /// Waits for the frame's URL to be exactly a URL, then for its document to load as far as a state. It returns
    /// at once if the URL already matches and the document has loaded.
    /// </summary>
    /// <param name="url">The full URL.</param>
    /// <param name="wait">How far the document must load once the URL matches.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
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
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
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
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
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
    /// <param name="timeout">The time to wait, from the start of the action, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The frame's URL after the navigation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the frame is detached while waiting.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the frame does not navigate, or the document does not load, in time.</exception>
    public Task<string> RunAndWaitForNavigationAsync(Func<Task> action, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(Call("Run and wait for navigation", null, "waitForNavigation"), this.CreateBudget(timeout, cancellationToken), budget => this.RunAndWaitForNavigationCoreAsync(action, wait, budget));
    }

    /// <summary>
    /// Calls a JavaScript function in the frame's document, in the page's own script realm, awaiting a promise it
    /// returns.
    /// </summary>
    /// <param name="function">The function's declaration, such as <c>(a, b) =&gt; a + b</c>; an expression is written as a function of no arguments, such as <c>() =&gt; document.title</c>.</param>
    /// <param name="arguments">The arguments, or <see langword="null"/> for none.</param>
    /// <param name="timeout">The time the call may take, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the call.</param>
    /// <returns>The function's result.</returns>
    /// <exception cref="ScriptException">Thrown when the function throws.</exception>
    public Task<RemoteValue> EvaluateAsync(string function, IEnumerable<LocalValue>? arguments = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(Call("Evaluate", null, "evaluate", ("function", function)), this.CreateActionBudget(timeout, cancellationToken), budget => this.EvaluateCoreAsync(function, arguments, budget));
    }

    /// <summary>
    /// Calls a JavaScript function in the frame's document, as <see cref="EvaluateAsync(string, IEnumerable{LocalValue}?, TimeSpan?, CancellationToken)"/>
    /// does, and converts its result.
    /// </summary>
    /// <typeparam name="T">The type to convert the result to: a string, a Boolean, a number type, <see cref="System.Numerics.BigInteger"/>, <see cref="DateTime"/>, or a nullable one of those; <see cref="object"/>, for an untyped tree of those with lists and string-keyed dictionaries; a <see cref="RemoteValue"/> type; an array of any of these, at any depth; or a <see cref="List{T}"/> or <see cref="Dictionary{TKey, TValue}"/> with string keys of any of these, but not within another value.</typeparam>
    /// <param name="function">The function's declaration.</param>
    /// <param name="arguments">The arguments, or <see langword="null"/> for none.</param>
    /// <param name="timeout">The time the call may take, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the call.</param>
    /// <returns>The function's result, converted.</returns>
    /// <exception cref="ScriptException">Thrown when the function throws.</exception>
    /// <exception cref="InvalidCastException">Thrown when the result cannot be converted to the type.</exception>
    /// <exception cref="NotSupportedException">Thrown when the type is not one a result converts to.</exception>
    /// <exception cref="OverflowException">Thrown when a number is outside the range of the type.</exception>
    public Task<T> EvaluateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string function, IEnumerable<LocalValue>? arguments = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(Call("Evaluate", null, "evaluate", ("function", function)), this.CreateActionBudget(timeout, cancellationToken), async budget => RemoteValueConverter.Convert<T>(await this.EvaluateCoreAsync(function, arguments, budget).ConfigureAwait(false)));
    }

    /// <summary>
    /// Calls a JavaScript function in the frame's document until it returns a truthy value, calling it again after
    /// <see cref="DramaturgeOptions.PollInterval"/> each time it does not, and through a navigation of the frame.
    /// </summary>
    /// <param name="function">The function's declaration.</param>
    /// <param name="arguments">The arguments, or <see langword="null"/> for none.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The truthy value.</returns>
    /// <exception cref="ScriptException">Thrown when the function throws.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the function does not return a truthy value in time.</exception>
    public Task<RemoteValue> WaitForFunctionAsync(string function, IEnumerable<LocalValue>? arguments = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(Call("Wait for function", null, "waitForFunction", ("function", function)), this.CreateActionBudget(timeout, cancellationToken), budget => this.WaitForFunctionCoreAsync(function, arguments, budget));
    }

    /// <summary>
    /// Calls a JavaScript function in the frame's document until it returns a truthy value, as
    /// <see cref="WaitForFunctionAsync(string, IEnumerable{LocalValue}?, TimeSpan?, CancellationToken)"/> does, and
    /// converts the value.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to, as for <see cref="EvaluateAsync{T}"/>.</typeparam>
    /// <param name="function">The function's declaration.</param>
    /// <param name="arguments">The arguments, or <see langword="null"/> for none.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The truthy value, converted.</returns>
    /// <exception cref="ScriptException">Thrown when the function throws.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the function does not return a truthy value in time.</exception>
    public Task<T> WaitForFunctionAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(string function, IEnumerable<LocalValue>? arguments = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(Call("Wait for function", null, "waitForFunction", ("function", function)), this.CreateActionBudget(timeout, cancellationToken), async budget => RemoteValueConverter.Convert<T>(await this.WaitForFunctionCoreAsync(function, arguments, budget).ConfigureAwait(false)));
    }

    /// <summary>
    /// Replaces the frame's document's contents with HTML, then waits for it to load as far as a state.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <param name="wait">How far the document must load.</param>
    /// <param name="timeout">The time it may take, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>A task that completes when the document has loaded as far as the state.</returns>
    public Task SetContentAsync(string html, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.TraceAsync(Call("Set content", null, "setContent"), this.CreateBudget(timeout, cancellationToken), budget => this.SetContentCoreAsync(html, wait, budget));
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
    /// Gets where the frame's content starts within its parent frame's viewport: the content box of its frame
    /// element, found as the element whose window is this frame's.
    /// </summary>
    /// <param name="budget">The time the lookup may take.</param>
    /// <returns>The distances from the parent's viewport's left and top edges.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the frame's element is not found.</exception>
    internal async Task<(double X, double Y)> GetOffsetInParentAsync(TimeBudget budget)
    {
        Frame parent = this.ParentFrame!;
        LocateNodesCommandResult located = await this.Group.Driver.BrowsingContext.LocateNodesAsync(new LocateNodesCommandParameters(parent.Id, new CssLocator("iframe, frame")), budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
        if (located.Nodes.Count > 0)
        {
            RemoteValue offsets = await this.Group.Driver.Script.CallFunctionAsync(parent.Id, "(...elements) => elements.map((element) => { const rect = element.getBoundingClientRect(); const style = getComputedStyle(element); return [element.contentWindow, rect.left + element.clientLeft + parseFloat(style.paddingLeft), rect.top + element.clientTop + parseFloat(style.paddingTop)]; })", [.. located.Nodes.Select(node => node.ToSharedReference())], this.Group.Options.SandboxName, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
            foreach (RemoteValue entry in offsets.As<CollectionRemoteValue>().Value!)
            {
                RemoteValueList values = entry.As<CollectionRemoteValue>().Value!;
                if (values[0] is WindowProxyRemoteValue window && window.Value.BrowsingContextId == this.Id)
                {
                    return (values[1].As<NumberRemoteValue>().Value, values[2].As<NumberRemoteValue>().Value);
                }
            }
        }

        throw new InvalidOperationException($"The element of frame {this.Id} was not found in its parent frame; a frame element within a shadow root is not searched.");
    }

    /// <summary>
    /// Gets how far the frame's document is scrolled.
    /// </summary>
    /// <param name="budget">The time the read may take.</param>
    /// <returns>The distances scrolled right and down.</returns>
    internal async Task<(double X, double Y)> GetScrollPositionAsync(TimeBudget budget)
    {
        RemoteValue scroll = await this.Group.Driver.Script.CallFunctionAsync(this.Id, "() => [window.scrollX, window.scrollY]", [], this.Group.Options.SandboxName, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
        RemoteValueList values = scroll.As<CollectionRemoteValue>().Value!;
        return (values[0].As<NumberRemoteValue>().Value, values[1].As<NumberRemoteValue>().Value);
    }

    /// <summary>
    /// Waits until the frame's URL matches, or, if negated, does not, checking it each time it changes.
    /// </summary>
    /// <param name="expected">What the expectation requires, negation included, such as <c>to have URL "https://example.com/"</c>.</param>
    /// <param name="isNot">A value indicating whether the expectation is negated.</param>
    /// <param name="pattern">The expected URL.</param>
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the frame is detached while waiting.</exception>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    internal Task ExpectUrlAsync(string expected, bool isNot, TextPattern pattern, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return this.TraceAsync(ExpectCall(expected), this.CreateExpectBudget(timeout, cancellationToken), budget => this.ExpectUrlCoreAsync(expected, isNot, pattern, budget));
    }

    /// <summary>
    /// Reads the document's title, with white space normalized, until it matches, or, if negated, does not, reading
    /// it again after a navigation.
    /// </summary>
    /// <param name="expected">What the expectation requires, negation included, such as <c>to have title "Home"</c>.</param>
    /// <param name="isNot">A value indicating whether the expectation is negated.</param>
    /// <param name="pattern">The expected title.</param>
    /// <param name="timeout">The time to retry, or <see langword="null"/> for <see cref="DramaturgeOptions.ExpectTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the expectation.</param>
    /// <returns>A task that completes when the expectation is met.</returns>
    /// <exception cref="ExpectationFailedException">Thrown when the expectation is not met in time.</exception>
    internal Task ExpectTitleAsync(string expected, bool isNot, TextPattern pattern, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return this.TraceAsync(ExpectCall(expected), this.CreateExpectBudget(timeout, cancellationToken), budget => this.ExpectTitleCoreAsync(expected, isNot, pattern, budget));
    }

    /// <summary>
    /// Navigates the frame, waiting for the new document to load as far as a state.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="wait">How far the new document must load.</param>
    /// <param name="budget">The time the navigation may take.</param>
    /// <returns>The URL navigated to, after any redirects.</returns>
    internal async Task<string> NavigateCoreAsync(string url, ReadinessState wait, TimeBudget budget)
    {
        NavigateCommandParameters parameters = new(this.Id, url) { Wait = wait };
        NavigateCommandResult result = await this.Group.Driver.BrowsingContext.NavigateAsync(parameters, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
        return result.Url;
    }

    /// <summary>
    /// Reloads the frame, waiting for the reloaded document to load as far as a state.
    /// </summary>
    /// <param name="wait">How far the reloaded document must load.</param>
    /// <param name="budget">The time the reload may take.</param>
    /// <returns>The URL reloaded.</returns>
    internal async Task<string> ReloadCoreAsync(ReadinessState wait, TimeBudget budget)
    {
        ReloadCommandParameters parameters = new(this.Id) { Wait = wait };
        ReloadCommandResult result = await this.Group.Driver.BrowsingContext.ReloadAsync(parameters, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
        return result.Url;
    }

    /// <summary>
    /// Runs an action, then waits for the frame to navigate and the new document to load as far as a state.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="wait">How far the new document must load.</param>
    /// <param name="budget">The time the navigation may take.</param>
    /// <returns>The frame's URL after the navigation.</returns>
    internal async Task<string> RunAndWaitForNavigationCoreAsync(Func<Task> action, ReadinessState wait, TimeBudget budget)
    {
        int before = this.Navigations;
        await action().ConfigureAwait(false);
        await this.WaitForNavigationAsync(before, wait, budget).ConfigureAwait(false);
        return this.Url;
    }

    /// <summary>
    /// Calls a function in the frame's own realm.
    /// </summary>
    /// <param name="function">The function.</param>
    /// <param name="arguments">Its arguments, or <see langword="null"/> for none.</param>
    /// <param name="budget">The time the call may take.</param>
    /// <returns>The function's result.</returns>
    internal Task<RemoteValue> EvaluateCoreAsync(string function, IEnumerable<LocalValue>? arguments, TimeBudget budget)
    {
        return this.Group.Driver.Script.CallFunctionAsync(this.Id, function, arguments, null, budget.Remaining, budget.CancellationToken);
    }

    /// <summary>
    /// Waits for the frame's URL to satisfy a condition, then for its document to load as far as a state.
    /// </summary>
    /// <param name="matches">The condition.</param>
    /// <param name="awaited">The URL awaited, for a timeout's message.</param>
    /// <param name="wait">How far the document must load.</param>
    /// <param name="budget">The time the wait may take.</param>
    /// <returns>The frame's URL.</returns>
    internal async Task<string> WaitForUrlCoreAsync(Func<string, bool> matches, string awaited, ReadinessState wait, TimeBudget budget)
    {
        await this.WaitUntilAsync(() => matches(this.url), awaited, () => $"the frame's URL was {this.url}", budget).ConfigureAwait(false);
        await this.WaitForLoadStateAsync(wait, budget).ConfigureAwait(false);
        return this.Url;
    }

    /// <summary>
    /// Waits for the frame's document to load as far as a state.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="budget">The time the wait may take.</param>
    /// <returns>A task that completes when the document has loaded as far as the state.</returns>
    internal async Task WaitForLoadStateAsync(ReadinessState state, TimeBudget budget)
    {
        if (state == ReadinessState.None)
        {
            return;
        }

        LoadState target = state == ReadinessState.Interactive ? LoadState.Interactive : LoadState.Complete;
        await this.ReadUnknownLoadStateAsync(budget).ConfigureAwait(false);
        await this.WaitUntilAsync(() => this.loadState >= target, $"the frame's document to be {target.ToString().ToLowerInvariant()}", () => $"it was {this.loadState?.ToString().ToLowerInvariant() ?? "unknown"}", budget).ConfigureAwait(false);
    }

    /// <summary>
    /// Calls a function in the frame's own realm until it returns a truthy value.
    /// </summary>
    /// <param name="function">The function.</param>
    /// <param name="arguments">Its arguments, or <see langword="null"/> for none.</param>
    /// <param name="budget">The time the wait may take.</param>
    /// <returns>The truthy value.</returns>
    internal async Task<RemoteValue> WaitForFunctionCoreAsync(string function, IEnumerable<LocalValue>? arguments, TimeBudget budget)
    {
        List<LocalValue> argumentList = [.. arguments ?? []];
        string observed = "it was still running";
        while (true)
        {
            int navigationsStarted = this.NavigationsStarted;
            try
            {
                RemoteValue result = await this.Group.Driver.Script.CallFunctionAsync(this.Id, function, argumentList, null, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
                if (IsTruthy(result))
                {
                    return result;
                }

                observed = $"it last returned {DescribeFalsy(result)}";
            }
            catch (WebDriverBiDiCommandException) when (this.NavigationsStarted != navigationsStarted)
            {
                observed = "the frame navigated while it ran";
            }
            catch (WebDriverBiDiTimeoutException)
            {
                // The call was given the rest of the budget, so timing out means the budget is spent.
                break;
            }

            // The delay ends with the budget, at once if none is left.
            await budget.DelayAsync(this.Group.Options.PollInterval).ConfigureAwait(false);
            if (budget.IsExhausted)
            {
                break;
            }
        }

        throw new WebDriverBiDiTimeoutException($"Timed out after {budget.Duration.TotalSeconds} seconds waiting for the function to return a truthy value; {observed}.");
    }

    /// <summary>
    /// Replaces the frame's document's content, waiting for it to load as far as a state.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <param name="wait">How far the content must load.</param>
    /// <param name="budget">The time it may take.</param>
    /// <returns>A task that completes when the content is set.</returns>
    internal async Task SetContentCoreAsync(string html, ReadinessState wait, TimeBudget budget)
    {
        string state = wait switch
        {
            ReadinessState.Complete => "complete",
            ReadinessState.Interactive => "interactive",
            _ => "none",
        };
        await this.Group.ScriptHost.CallActionsAsync(this.Id, "(actions, html, state) => actions.setContent(html, state)", [LocalValue.String(html), LocalValue.String(state)], budget).ConfigureAwait(false);
        this.RecordLoadState(wait == ReadinessState.Complete ? LoadState.Complete : null);
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
    /// <param name="state">The state, or <see langword="null"/> when it is unknown.</param>
    internal void RecordLoadState(LoadState? state)
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

    private static TracedCall Call(string title, string? subtitle, string method, params (string Name, object Value)[] parameters)
    {
        return TraceRecording.Call("Frame", title, subtitle, method, parameters);
    }

    private static TracedCall ExpectCall(string expected)
    {
        return TraceRecording.Call("Expect", $"Expect {expected}", null, "expect");
    }

    private static bool IsTruthy(RemoteValue value)
    {
        return value switch
        {
            NullRemoteValue or UndefinedRemoteValue => false,
            BooleanRemoteValue boolean => boolean.Value,
            NumberRemoteValue number => number.Value != 0 && !double.IsNaN(number.Value),
            StringRemoteValue text => text.Value.Length > 0,
            BigIntegerRemoteValue bigInteger => !bigInteger.Value.IsZero,
            _ => true,
        };
    }

    private static string DescribeFalsy(RemoteValue value)
    {
        return value switch
        {
            NullRemoteValue => "null",
            BooleanRemoteValue => "false",
            NumberRemoteValue number => number.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
            StringRemoteValue => "an empty string",
            BigIntegerRemoteValue => "0n",
            _ => "undefined",
        };
    }

    // Waits for a command no longer needed to finish, however it finishes.
    private static async Task IgnoreFailureAsync(Task command)
    {
        try
        {
            await command.ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is OperationCanceledException or WebDriverBiDiException)
        {
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

    private Task<T> TraceAsync<T>(TracedCall call, TimeBudget budget, Func<TimeBudget, Task<T>> action)
    {
        return TraceRecording.RunAsync(this.Page.Browser, this.Page, budget, call, action);
    }

    private Task TraceAsync(TracedCall call, TimeBudget budget, Func<TimeBudget, Task> action)
    {
        return TraceRecording.RunAsync(this.Page.Browser, this.Page, budget, call, action);
    }

    private TimeBudget CreateActionBudget(TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return new TimeBudget(timeout ?? this.Group.Options.ActionTimeout, this.Group.Options.TimeProvider, cancellationToken);
    }

    private async Task ExpectUrlCoreAsync(string expected, bool isNot, TextPattern pattern, TimeBudget budget)
    {
        if (!await this.TryWaitUntilAsync(() => pattern.Matches(this.url) != isNot, $"the page {expected}", budget).ConfigureAwait(false))
        {
            string url = TextPattern.Quote(this.Url);
            throw new ExpectationFailedException($"Expected the page {expected}; received {url} after {budget.Duration.TotalSeconds} seconds.", expected, url, budget.Duration);
        }
    }

    private async Task ExpectTitleCoreAsync(string expected, bool isNot, TextPattern pattern, TimeBudget budget)
    {
        string? actual = null;
        string? observed = null;
        while (true)
        {
            int navigationsStarted = this.NavigationsStarted;
            try
            {
                RemoteValue title = await this.Group.Driver.Script.CallFunctionAsync(this.Id, "() => document.title", [], this.Group.Options.SandboxName, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
                string text = TextPattern.Normalize(title.As<StringRemoteValue>().Value);
                actual = TextPattern.Quote(text);
                if (pattern.Matches(text) != isNot)
                {
                    return;
                }

                observed = $"received {actual}";
            }
            catch (WebDriverBiDiCommandException) when (this.NavigationsStarted != navigationsStarted)
            {
                observed = "the frame navigated while the title was read";
            }
            catch (WebDriverBiDiTimeoutException)
            {
                // The call was given the rest of the budget, so timing out means the budget is spent.
                observed ??= "a command was still running";
                break;
            }

            // The delay ends with the budget, at once if none is left.
            await budget.DelayAsync(this.Group.Options.PollInterval).ConfigureAwait(false);
            if (budget.IsExhausted)
            {
                break;
            }
        }

        throw new ExpectationFailedException($"Expected the page {expected}; {observed} after {budget.Duration.TotalSeconds} seconds.", expected, actual, budget.Duration);
    }

    private TimeBudget CreateBudget(TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return new TimeBudget(this.NavigationTimeout(timeout), this.Group.Options.TimeProvider, cancellationToken);
    }

    private TimeBudget CreateExpectBudget(TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return new TimeBudget(timeout ?? this.Group.Options.ExpectTimeout, this.Group.Options.TimeProvider, cancellationToken);
    }

    private TimeSpan NavigationTimeout(TimeSpan? timeout)
    {
        return timeout ?? this.Group.Options.NavigationTimeout;
    }

    private Task<string> WaitForUrlAsync(Func<string, bool> matches, string awaited, ReadinessState wait, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        return this.TraceAsync(Call("Wait for URL", "{url}", "waitForURL", ("url", awaited)), this.CreateBudget(timeout, cancellationToken), budget => this.WaitForUrlCoreAsync(matches, awaited, wait, budget));
    }

    // A document whose state no event has reported is asked. An event that changes the frame's state while it is
    // asked makes the answer stale, so the question is dropped, and asked again only if the state is still unknown;
    // a browser may not answer at all while a new document settles, when the events tell the state anyway.
    private async Task ReadUnknownLoadStateAsync(TimeBudget budget)
    {
        while (true)
        {
            int version;
            Task changed;
            lock (this.stateLock)
            {
                if (this.loadState is not null)
                {
                    return;
                }

                version = this.stateVersion;
                changed = this.stateChanged.Task;
            }

            using CancellationTokenSource readCancellation = CancellationTokenSource.CreateLinkedTokenSource(budget.CancellationToken);
            Task<RemoteValue> read = this.Group.Driver.Script.CallFunctionAsync(this.Id, "() => document.readyState", [], this.Group.Options.SandboxName, budget.Remaining, readCancellation.Token);
            await Task.WhenAny(read, changed).ConfigureAwait(false);
            if (this.StateChangedSince(version))
            {
                readCancellation.Cancel();
                await IgnoreFailureAsync(read).ConfigureAwait(false);
                continue;
            }

            RemoteValue readyState;
            try
            {
                readyState = await read.ConfigureAwait(false);
            }
            catch (WebDriverBiDiTimeoutException)
            {
                // The question was given the rest of the budget, so the wait that asked it has timed out.
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
        if (!await this.TryWaitUntilAsync(isDone, awaited, budget).ConfigureAwait(false))
        {
            lock (this.stateLock)
            {
                throw new WebDriverBiDiTimeoutException($"Timed out after {budget.Duration.TotalSeconds} seconds waiting for {awaited}; {describe()}.");
            }
        }
    }

    // False when the budget runs out first.
    private async Task<bool> TryWaitUntilAsync(Func<bool> isDone, string awaited, TimeBudget budget)
    {
        Task timedOut = budget.DelayAsync(budget.Remaining);
        while (true)
        {
            Task changed;
            lock (this.stateLock)
            {
                if (isDone())
                {
                    return true;
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
                return false;
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
