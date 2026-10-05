// <copyright file="Page.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Dramaturge.Network;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Network;
using WebDriverBiDi.Script;

/// <summary>
/// A tab or window of a <see cref="Dramaturge.Browser"/>: a top-level browsing context, and the frames within it.
/// </summary>
public sealed class Page
{
    private readonly object lockObject = new();
    private readonly List<Frame> frames = [];
    private readonly Dictionary<string, int> snapshotFrameNumbers = [];
    private readonly ObservableEventInvocable<PageEventArgs> onClosed = new("automation.pageClosed");
    private readonly ObservableEventInvocable<ConsoleMessageEventArgs> onConsoleMessage = new("automation.consoleMessage");
    private readonly ObservableEventInvocable<PageErrorEventArgs> onPageError = new("automation.pageError");
    private readonly ObservableEventInvocable<DialogEventArgs> onDialog = new("automation.dialog");
    private readonly ObservableEventInvocable<PageEventArgs> onPopup = new("automation.popup");
    private readonly TaskCompletionSource<bool> created = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly ObservableEventInvocable<DownloadEventArgs> onDownload = new("automation.download");
    private readonly List<TaskCompletionSource<Download>> downloadWaiters = [];
    private readonly List<RouteRegistration> routes = [];
    private readonly List<NetworkWaiter<BeforeRequestSentEventArgs>> requestWaiters = [];
    private readonly List<NetworkWaiter<ResponseCompletedEventArgs>> responseWaiters = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Page"/> class.
    /// </summary>
    /// <param name="browser">The browser the page belongs to.</param>
    /// <param name="id">The ID of the page's browsing context.</param>
    /// <param name="url">The URL of the page's main frame.</param>
    /// <param name="opener">The page that opened this one, or <see langword="null"/> if none did or it is not tracked.</param>
    internal Page(Browser browser, string id, string url, Page? opener)
    {
        this.Browser = browser;
        this.Id = id;
        this.Opener = opener;
        this.Mouse = new Mouse(this);
        this.Keyboard = new Keyboard(this);
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
    /// Gets the page's mouse, driven by viewport coordinates.
    /// </summary>
    public Mouse Mouse { get; }

    /// <summary>
    /// Gets the page's keyboard.
    /// </summary>
    public Keyboard Keyboard { get; }

    /// <summary>
    /// Gets the page that opened this one, such as with <c>window.open</c> or a link with a target, or
    /// <see langword="null"/> if none did or the group was not tracking it.
    /// </summary>
    public Page? Opener { get; }

    /// <summary>
    /// Gets an observable event raised when a document of the page calls a <c>console</c> method.
    /// </summary>
    public ObservableEvent<ConsoleMessageEventArgs> OnConsoleMessage => this.onConsoleMessage;

    /// <summary>
    /// Gets an observable event raised when a script of the page throws an error it does not catch.
    /// </summary>
    public ObservableEvent<PageErrorEventArgs> OnPageError => this.onPageError;

    /// <summary>
    /// Gets an observable event raised when a document of the page opens a dialog. See <see cref="Dialog"/> for
    /// how dialogs are handled.
    /// </summary>
    public ObservableEvent<DialogEventArgs> OnDialog => this.onDialog;

    /// <summary>
    /// Gets an observable event raised when the page opens another page, once that page is tracked.
    /// </summary>
    public ObservableEvent<PageEventArgs> OnPopup => this.onPopup;

    /// <summary>
    /// Gets an observable event raised when a document of the page begins a download.
    /// </summary>
    public ObservableEvent<DownloadEventArgs> OnDownload => this.onDownload;

    /// <summary>
    /// Gets a task that completes once the observers of the page's creation have been notified.
    /// </summary>
    internal Task Created => this.created.Task;

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
    /// Creates a locator for elements by their test ID, the value of the <see cref="DramaturgeOptions.TestIdAttribute"/> attribute, in the page's main frame.
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
    /// <param name="timeout">The time the navigation may take, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
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
    /// <param name="timeout">The time the reload may take, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
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
    /// <param name="timeout">The time the navigation may take, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
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
    /// <param name="timeout">The time the navigation may take, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
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
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
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
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
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
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
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
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
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
    /// <param name="timeout">The time to wait, from the start of the action, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
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
    /// <param name="timeout">The time the call may take, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
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
    /// <param name="timeout">The time the call may take, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
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
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
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
    /// <param name="timeout">The time to wait, or <see langword="null"/> for <see cref="DramaturgeOptions.ActionTimeout"/>.</param>
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
    /// <param name="timeout">The time it may take, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
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
    /// Runs an action that makes the page begin a download, such as a click on a link, and waits for the download
    /// to begin.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="timeout">The time to wait, from the start of the action, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The download.</returns>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no download begins in time.</exception>
    public async Task<Download> RunAndWaitForDownloadAsync(Func<Task> action, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        TimeBudget budget = new(timeout ?? this.Browser.Group.Options.NavigationTimeout, this.Browser.Group.Options.TimeProvider, cancellationToken);
        TaskCompletionSource<Download> next = new(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (this.lockObject)
        {
            this.downloadWaiters.Add(next);
        }

        try
        {
            await action().ConfigureAwait(false);
            return await budget.WaitAsync(next.Task, "a download to begin").ConfigureAwait(false);
        }
        finally
        {
            lock (this.lockObject)
            {
                this.downloadWaiters.Remove(next);
            }
        }
    }

    /// <summary>
    /// Captures an image of what the page's viewport shows, or of its whole document.
    /// </summary>
    /// <param name="options">The part to capture and the format, or <see langword="null"/> for the viewport as PNG.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>The image.</returns>
    public async Task<byte[]> ScreenshotAsync(PageScreenshotOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new PageScreenshotOptions();
        CaptureScreenshotCommandParameters parameters = new(this.Id)
        {
            Origin = options.FullPage ? ScreenshotOrigin.Document : ScreenshotOrigin.Viewport,
            Clip = options.Clip,
            Format = options.Format,
        };
        CaptureScreenshotCommandResult result = await this.Browser.Group.Driver.BrowsingContext.CaptureScreenshotAsync(parameters, cancellationToken: cancellationToken).ConfigureAwait(false);
        return Convert.FromBase64String(result.Data);
    }

    /// <summary>
    /// Takes an accessibility snapshot of the page: each element with a role, with its accessible name and states,
    /// and the text between them, including the content of its frames.
    /// </summary>
    /// <param name="options">Whether the snapshot has refs and includes frames, or <see langword="null"/> for both.</param>
    /// <param name="cancellationToken">A token that cancels the snapshot.</param>
    /// <returns>The snapshot.</returns>
    /// <remarks>
    /// The snapshot's names are computed by the library's script in the page, and can differ from those the browser
    /// uses for <see cref="GetByRole"/>; act on an element from a snapshot through its ref, with
    /// <see cref="AriaSnapshot.Locator"/>.
    /// </remarks>
    public Task<AriaSnapshot> AriaSnapshotAsync(AriaSnapshotOptions? options = null, CancellationToken cancellationToken = default)
    {
        TimeBudget budget = new(this.Browser.Group.Options.ActionTimeout, this.Browser.Group.Options.TimeProvider, cancellationToken);
        return AriaSnapshotBuilder.TakeAsync(this.MainFrame, null, options ?? new AriaSnapshotOptions(), budget);
    }

    /// <summary>
    /// Prints the page as a PDF.
    /// </summary>
    /// <param name="options">The page setup, or <see langword="null"/> for the browser's defaults.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>The PDF document.</returns>
    public async Task<byte[]> PdfAsync(PdfOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new PdfOptions();
        PrintCommandParameters parameters = new(this.Id)
        {
            Background = options.Background,
            Margins = options.Margins,
            Orientation = options.Orientation,
            Page = options.PageSize,
            Scale = options.Scale,
            ShrinkToFit = options.ShrinkToFit,
        };
        parameters.PageRanges.AddRange(options.PageRanges);
        PrintCommandResult result = await this.Browser.Group.Driver.BrowsingContext.PrintAsync(parameters, cancellationToken: cancellationToken).ConfigureAwait(false);
        return Convert.FromBase64String(result.Data);
    }

    /// <summary>
    /// Sets the size of the page's viewport, in CSS pixels, overriding the size of its window or the browser's
    /// setting.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the viewport has the size.</returns>
    public Task SetViewportSizeAsync(ulong width, ulong height, CancellationToken cancellationToken = default)
    {
        return this.SetViewportAsync(new Viewport() { Width = width, Height = height }, cancellationToken);
    }

    /// <summary>
    /// Returns the page's viewport to its default size.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the viewport has its default size.</returns>
    public Task ResetViewportSizeAsync(CancellationToken cancellationToken = default)
    {
        return this.SetViewportAsync(SetViewportCommandParameters.ResetToDefaultViewport, cancellationToken);
    }

    /// <summary>
    /// Starts recording a video of the page, written to a file when the recording is disposed or stopped. The browser
    /// chooses the format, such as WebM.
    /// </summary>
    /// <param name="path">The path of the video file; its folder is created if needed.</param>
    /// <param name="options">The recording's settings, or <see langword="null"/> for the browser's own.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>The recording.</returns>
    /// <exception cref="NotSupportedException">Thrown when the browser cannot record video.</exception>
    public async Task<VideoRecording> RecordVideoAsync(string path, VideoRecordingOptions? options = null, CancellationToken cancellationToken = default)
    {
        string fullPath = Path.GetFullPath(path);
        string folder = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(folder);
        StartScreencastCommandParameters parameters = new(this.Id)
        {
            DestinationFolder = folder,
            Video = options is null ? null : new MediaTrackConstraints() { Width = options.Width, Height = options.Height, FrameRate = options.FrameRate },
        };
        try
        {
            StartScreencastCommandResult result = await this.Browser.Group.Driver.BrowsingContext.StartScreencastAsync(parameters, cancellationToken: cancellationToken).ConfigureAwait(false);
            return new VideoRecording(this, result.ScreencastId, fullPath);
        }
        catch (WebDriverBiDiCommandException exception) when (exception.ErrorCode == ErrorCode.UnsupportedOperation)
        {
            throw new NotSupportedException($"The browser cannot record video: {exception.Message}", exception);
        }
    }

    /// <summary>
    /// Adds a route for requests the page and its frames make to a URL.
    /// </summary>
    /// <param name="url">The request's full URL.</param>
    /// <param name="handler">The handler, which answers, continues, or aborts each request.</param>
    /// <param name="filter">A pattern the browser matches first, so that only requests it matches are stopped, or <see langword="null"/> to stop every request of the page while the route exists.</param>
    /// <param name="cancellationToken">A token that cancels the commands.</param>
    /// <returns>The route, which can be removed.</returns>
    public Task<RouteRegistration> RouteAsync(string url, Func<Route, Task> handler, UrlPattern? filter = null, CancellationToken cancellationToken = default)
    {
        return this.AddRouteAsync(request => request.Url == url, url, handler, filter, cancellationToken);
    }

    /// <summary>
    /// Adds a route for requests the page and its frames make to URLs matching a regular expression.
    /// </summary>
    /// <param name="url">The regular expression, matched against the request's full URL.</param>
    /// <param name="handler">The handler, which answers, continues, or aborts each request.</param>
    /// <param name="filter">A pattern the browser matches first, so that only requests it matches are stopped, or <see langword="null"/> to stop every request of the page while the route exists.</param>
    /// <param name="cancellationToken">A token that cancels the commands.</param>
    /// <returns>The route, which can be removed.</returns>
    public Task<RouteRegistration> RouteAsync(Regex url, Func<Route, Task> handler, UrlPattern? filter = null, CancellationToken cancellationToken = default)
    {
        return this.AddRouteAsync(request => url.IsMatch(request.Url), $"URLs matching {url}", handler, filter, cancellationToken);
    }

    /// <summary>
    /// Adds a route for requests the page and its frames make that satisfy a condition. Requests are stopped
    /// before they are sent and handed to the route's handler, which answers, continues, or aborts each. Routes are
    /// tried newest first; a handler that does none of these passes the request to the next route that matches it,
    /// and a request no route decides, or whose handler throws, continues as it was, the exception reported on
    /// <see cref="BrowserGroup.OnLogMessage"/>.
    /// </summary>
    /// <param name="request">The condition, given the request.</param>
    /// <param name="handler">The handler, which answers, continues, or aborts each request.</param>
    /// <param name="filter">A pattern the browser matches first, so that only requests it matches are stopped, or <see langword="null"/> to stop every request of the page while the route exists. A pattern's parts are matched exactly; a part left out matches anything.</param>
    /// <param name="cancellationToken">A token that cancels the commands.</param>
    /// <returns>The route, which can be removed.</returns>
    public Task<RouteRegistration> RouteAsync(Func<RequestData, bool> request, Func<Route, Task> handler, UrlPattern? filter = null, CancellationToken cancellationToken = default)
    {
        return this.AddRouteAsync(request, "requests satisfying the condition", handler, filter, cancellationToken);
    }

    /// <summary>
    /// Adds a route that answers requests the page and its frames make from an HTTP Archive: a .har file, or a .zip holding one
    /// and its body files, as Playwright writes them. Each request is answered with the response of the entry of its
    /// method and URL, ignoring a fragment, whose recorded body is the request's, if both have one, comparing a
    /// multipart form's body without its boundary; of several, the one recorded with the most of the request's
    /// headers, then the first. A redirect is answered as recorded, and the browser follows it to the next entry. A
    /// request with no entry is aborted, unless <see cref="HarRouteOptions.NotFound"/> passes it on.
    /// </summary>
    /// <param name="harPath">The path of the archive, which is read now.</param>
    /// <param name="options">The route's settings, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels reading the archive and the commands.</param>
    /// <returns>The route, which can be removed.</returns>
    /// <exception cref="InvalidDataException">Thrown when the file is not an archive that can be read.</exception>
    public Task<RouteRegistration> RouteFromHarAsync(string harPath, HarRouteOptions? options = null, CancellationToken cancellationToken = default)
    {
        return this.AddHarRouteAsync(harPath, _ => true, "requests", options, cancellationToken);
    }

    /// <summary>
    /// Adds a route that answers requests the page and its frames make to a URL from an HTTP Archive, as
    /// <see cref="RouteFromHarAsync(string, HarRouteOptions?, CancellationToken)"/> does.
    /// </summary>
    /// <param name="harPath">The path of the archive, which is read now.</param>
    /// <param name="url">The request's full URL.</param>
    /// <param name="options">The route's settings, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels reading the archive and the commands.</param>
    /// <returns>The route, which can be removed.</returns>
    /// <exception cref="InvalidDataException">Thrown when the file is not an archive that can be read.</exception>
    public Task<RouteRegistration> RouteFromHarAsync(string harPath, string url, HarRouteOptions? options = null, CancellationToken cancellationToken = default)
    {
        return this.AddHarRouteAsync(harPath, request => request.Url == url, url, options, cancellationToken);
    }

    /// <summary>
    /// Adds a route that answers requests the page and its frames make to URLs matching a regular expression from an HTTP
    /// Archive, as <see cref="RouteFromHarAsync(string, HarRouteOptions?, CancellationToken)"/> does.
    /// </summary>
    /// <param name="harPath">The path of the archive, which is read now.</param>
    /// <param name="url">The regular expression, matched against the request's full URL.</param>
    /// <param name="options">The route's settings, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels reading the archive and the commands.</param>
    /// <returns>The route, which can be removed.</returns>
    /// <exception cref="InvalidDataException">Thrown when the file is not an archive that can be read.</exception>
    public Task<RouteRegistration> RouteFromHarAsync(string harPath, Regex url, HarRouteOptions? options = null, CancellationToken cancellationToken = default)
    {
        return this.AddHarRouteAsync(harPath, request => url.IsMatch(request.Url), $"URLs matching {url}", options, cancellationToken);
    }

    /// <summary>
    /// Adds a route that answers requests the page and its frames make that satisfy a condition from an HTTP Archive, as
    /// <see cref="RouteFromHarAsync(string, HarRouteOptions?, CancellationToken)"/> does.
    /// </summary>
    /// <param name="harPath">The path of the archive, which is read now.</param>
    /// <param name="request">The condition, given the request.</param>
    /// <param name="options">The route's settings, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels reading the archive and the commands.</param>
    /// <returns>The route, which can be removed.</returns>
    /// <exception cref="InvalidDataException">Thrown when the file is not an archive that can be read.</exception>
    public Task<RouteRegistration> RouteFromHarAsync(string harPath, Func<RequestData, bool> request, HarRouteOptions? options = null, CancellationToken cancellationToken = default)
    {
        return this.AddHarRouteAsync(harPath, request, "requests satisfying the condition", options, cancellationToken);
    }

    /// <summary>
    /// Starts recording the requests the page and its frames make to an HTTP Archive file, with their responses and bodies. Disposing the
    /// recording writes the file; <see cref="HarRecording.SaveAsync"/> writes it sooner. The file can be replayed with
    /// <see cref="RouteFromHarAsync(string, HarRouteOptions?, CancellationToken)"/>.
    /// </summary>
    /// <param name="harPath">The path of the .har file to write.</param>
    /// <param name="options">The recording's settings, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels starting.</param>
    /// <returns>The recording.</returns>
    public Task<HarRecording> RecordHarAsync(string harPath, HarRecordingOptions? options = null, CancellationToken cancellationToken = default)
    {
        return HarRecording.StartAsync(this.Browser.Group, harPath, options, monitorOptions => monitorOptions.BrowsingContextIds.Add(this.Id), cancellationToken);
    }

    /// <summary>
    /// Removes every route of the page. A request already stopped is still handled.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the commands.</param>
    /// <returns>A task that completes when the routes are removed.</returns>
    public async Task UnrouteAllAsync(CancellationToken cancellationToken = default)
    {
        List<RouteRegistration> removed;
        lock (this.lockObject)
        {
            removed = [.. this.routes];
        }

        foreach (RouteRegistration route in removed)
        {
            await route.RemoveAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Runs an action and waits for the page or its frames to send a request to a URL.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="url">The request's full URL.</param>
    /// <param name="timeout">The time to wait, from the start of the action, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The request's event.</returns>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no such request is sent in time.</exception>
    public Task<BeforeRequestSentEventArgs> RunAndWaitForRequestAsync(Func<Task> action, string url, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.RunAndWaitForNetworkEventAsync(this.requestWaiters, action, request => request.Url == url, $"a request to {url}", timeout, cancellationToken);
    }

    /// <summary>
    /// Runs an action and waits for the page or its frames to send a request to a URL matching a regular expression.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="url">The regular expression, matched against the request's full URL.</param>
    /// <param name="timeout">The time to wait, from the start of the action, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The request's event.</returns>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no such request is sent in time.</exception>
    public Task<BeforeRequestSentEventArgs> RunAndWaitForRequestAsync(Func<Task> action, Regex url, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.RunAndWaitForNetworkEventAsync(this.requestWaiters, action, request => url.IsMatch(request.Url), $"a request to a URL matching {url}", timeout, cancellationToken);
    }

    /// <summary>
    /// Runs an action and waits for the page or its frames to send a request that satisfies a condition.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="request">The condition, given the request.</param>
    /// <param name="timeout">The time to wait, from the start of the action, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The request's event.</returns>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no such request is sent in time.</exception>
    public Task<BeforeRequestSentEventArgs> RunAndWaitForRequestAsync(Func<Task> action, Func<RequestData, bool> request, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.RunAndWaitForNetworkEventAsync(this.requestWaiters, action, request, "a request satisfying the condition", timeout, cancellationToken);
    }

    /// <summary>
    /// Runs an action and waits for the response to a request the page or its frames send to a URL.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="url">The request's full URL.</param>
    /// <param name="timeout">The time to wait, from the start of the action, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The event of the completed response.</returns>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no such response completes in time.</exception>
    public Task<ResponseCompletedEventArgs> RunAndWaitForResponseAsync(Func<Task> action, string url, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.RunAndWaitForNetworkEventAsync(this.responseWaiters, action, request => request.Url == url, $"a response to a request to {url}", timeout, cancellationToken);
    }

    /// <summary>
    /// Runs an action and waits for the response to a request the page or its frames send to a URL matching a
    /// regular expression.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="url">The regular expression, matched against the request's full URL.</param>
    /// <param name="timeout">The time to wait, from the start of the action, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The event of the completed response.</returns>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no such response completes in time.</exception>
    public Task<ResponseCompletedEventArgs> RunAndWaitForResponseAsync(Func<Task> action, Regex url, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.RunAndWaitForNetworkEventAsync(this.responseWaiters, action, request => url.IsMatch(request.Url), $"a response to a request to a URL matching {url}", timeout, cancellationToken);
    }

    /// <summary>
    /// Runs an action and waits for the response to a request the page or its frames send that satisfies a condition.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="request">The condition, given the request.</param>
    /// <param name="timeout">The time to wait, from the start of the action, or <see langword="null"/> for <see cref="DramaturgeOptions.NavigationTimeout"/>.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The event of the completed response.</returns>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when no such response completes in time.</exception>
    public Task<ResponseCompletedEventArgs> RunAndWaitForResponseAsync(Func<Task> action, Func<RequestData, bool> request, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return this.RunAndWaitForNetworkEventAsync(this.responseWaiters, action, request, "a response to a request satisfying the condition", timeout, cancellationToken);
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
    /// Gets the text put before the refs of a frame's accessibility snapshots: none for the main frame, and for
    /// another frame, f followed by a number it is given the first time it is snapshotted, so its refs stay the same
    /// from one snapshot to the next.
    /// </summary>
    /// <param name="frame">The frame.</param>
    /// <returns>The prefix.</returns>
    internal string GetSnapshotRefPrefix(Frame frame)
    {
        if (frame == this.MainFrame)
        {
            return string.Empty;
        }

        lock (this.lockObject)
        {
            if (!this.snapshotFrameNumbers.TryGetValue(frame.Id, out int number))
            {
                number = this.snapshotFrameNumbers.Count + 1;
                this.snapshotFrameNumbers[frame.Id] = number;
            }

            return $"f{number}";
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

    /// <summary>
    /// Raises <see cref="OnConsoleMessage"/>.
    /// </summary>
    /// <param name="args">The message.</param>
    /// <returns>A task that completes when observers are notified.</returns>
    internal Task NotifyConsoleMessageAsync(ConsoleMessageEventArgs args)
    {
        return this.onConsoleMessage.InvokeNotifyObserversAsync(args);
    }

    /// <summary>
    /// Raises <see cref="OnPageError"/>.
    /// </summary>
    /// <param name="args">The error.</param>
    /// <returns>A task that completes when observers are notified.</returns>
    internal Task NotifyPageErrorAsync(PageErrorEventArgs args)
    {
        return this.onPageError.InvokeNotifyObserversAsync(args);
    }

    /// <summary>
    /// Raises <see cref="OnDialog"/>.
    /// </summary>
    /// <param name="dialog">The dialog.</param>
    /// <returns>A task that completes when observers are notified.</returns>
    internal Task NotifyDialogAsync(Dialog dialog)
    {
        return this.onDialog.InvokeNotifyObserversAsync(new DialogEventArgs(dialog));
    }

    /// <summary>
    /// Raises <see cref="OnPopup"/>.
    /// </summary>
    /// <param name="popup">The page this one opened.</param>
    /// <returns>A task that completes when observers are notified.</returns>
    internal Task NotifyPopupAsync(Page popup)
    {
        return this.onPopup.InvokeNotifyObserversAsync(new PageEventArgs(popup));
    }

    /// <summary>
    /// Hands a download to those waiting for one, then raises <see cref="OnDownload"/>.
    /// </summary>
    /// <param name="download">The download.</param>
    /// <returns>A task that completes when observers are notified.</returns>
    internal Task NotifyDownloadAsync(Download download)
    {
        lock (this.lockObject)
        {
            foreach (TaskCompletionSource<Download> waiter in this.downloadWaiters)
            {
                waiter.TrySetResult(download);
            }
        }

        return this.onDownload.InvokeNotifyObserversAsync(new DownloadEventArgs(download));
    }

    /// <summary>
    /// Records that the observers of the page's creation have been notified.
    /// </summary>
    internal void MarkCreated()
    {
        this.created.TrySetResult(true);
    }

    /// <summary>
    /// Gets the page's routes that stopped a request, newest first.
    /// </summary>
    /// <param name="intercepts">The IDs of the intercepts that stopped the request.</param>
    /// <returns>The routes.</returns>
    internal List<RouteRegistration> FindStoppingRoutes(IList<string> intercepts)
    {
        lock (this.lockObject)
        {
            return this.routes.FindAll(route => intercepts.Contains(route.InterceptId));
        }
    }

    /// <summary>
    /// Forgets a route that was removed.
    /// </summary>
    /// <param name="route">The route.</param>
    internal void RemoveRoute(RouteRegistration route)
    {
        lock (this.lockObject)
        {
            this.routes.Remove(route);
        }
    }

    /// <summary>
    /// Offers a request's event to those waiting for one.
    /// </summary>
    /// <param name="e">The event.</param>
    internal void NotifyRequest(BeforeRequestSentEventArgs e)
    {
        lock (this.lockObject)
        {
            foreach (NetworkWaiter<BeforeRequestSentEventArgs> waiter in this.requestWaiters)
            {
                waiter.Offer(e, e.Request);
            }
        }
    }

    /// <summary>
    /// Offers a completed response's event to those waiting for one.
    /// </summary>
    /// <param name="e">The event.</param>
    internal void NotifyResponse(ResponseCompletedEventArgs e)
    {
        lock (this.lockObject)
        {
            foreach (NetworkWaiter<ResponseCompletedEventArgs> waiter in this.responseWaiters)
            {
                waiter.Offer(e, e.Request);
            }
        }
    }

    private Task SetViewportAsync(Viewport viewport, CancellationToken cancellationToken)
    {
        return this.Browser.Group.Driver.BrowsingContext.SetViewportAsync(new SetViewportCommandParameters() { BrowsingContextId = this.Id, Viewport = viewport }, cancellationToken: cancellationToken);
    }

    private async Task<RouteRegistration> AddHarRouteAsync(string harPath, Func<RequestData, bool> matches, string description, HarRouteOptions? options, CancellationToken cancellationToken)
    {
        HarRouter router = await HarRouter.CreateAsync(this.Browser.Group, harPath, options, parameters => parameters.Contexts.Add(this.Id), cancellationToken).ConfigureAwait(false);
        try
        {
            return await this.AddRouteAsync(matches, $"{description} from the HAR {Path.GetFileName(harPath)}", router.HandleAsync, options?.Filter, cancellationToken, router.RemoveAsync).ConfigureAwait(false);
        }
        catch
        {
            await router.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    // Each route has its own intercept, with its filter, limited to the page; the browser marks a request with every
    // intercept that stopped it.
    private async Task<RouteRegistration> AddRouteAsync(Func<RequestData, bool> matches, string description, Func<Route, Task> handler, UrlPattern? filter, CancellationToken cancellationToken, Func<CancellationToken, Task>? removing = null)
    {
        BrowserGroup group = this.Browser.Group;
        await group.EnsureNetworkEventsAsync(cancellationToken).ConfigureAwait(false);
        AddInterceptCommandParameters parameters = new(InterceptPhase.BeforeRequestSent);
        parameters.Contexts.Add(this.Id);
        if (filter is not null)
        {
            parameters.UrlPatterns.Add(filter);
        }

        string interceptId = (await group.Driver.Network.AddInterceptAsync(parameters, cancellationToken: cancellationToken).ConfigureAwait(false)).InterceptId;
        group.TrackIntercept(interceptId);
        RouteRegistration route = new(group, this.RemoveRoute, matches, description, handler, interceptId, removing);
        lock (this.lockObject)
        {
            this.routes.Insert(0, route);
        }

        return route;
    }

    private async Task<T> RunAndWaitForNetworkEventAsync<T>(List<NetworkWaiter<T>> waiters, Func<Task> action, Func<RequestData, bool> matches, string awaited, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        TimeBudget budget = new(timeout ?? this.Browser.Group.Options.NavigationTimeout, this.Browser.Group.Options.TimeProvider, cancellationToken);
        await this.Browser.Group.EnsureNetworkEventsAsync(cancellationToken).ConfigureAwait(false);
        NetworkWaiter<T> waiter = new(matches);
        lock (this.lockObject)
        {
            waiters.Add(waiter);
        }

        try
        {
            await action().ConfigureAwait(false);
            return await budget.WaitAsync(waiter.Found.Task, awaited).ConfigureAwait(false);
        }
        finally
        {
            lock (this.lockObject)
            {
                waiters.Remove(waiter);
            }
        }
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
