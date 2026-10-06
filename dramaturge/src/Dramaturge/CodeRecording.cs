// <copyright file="CodeRecording.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// The recording of the actions a user takes in a browser's pages as C# code. Each action's statement settles when
/// the next action is recorded, or when the recording stops, so that typing into a field is one fill and the
/// clicks of a double click are one. Disposing it, or <see cref="StopAsync"/>, ends it.
/// </summary>
public sealed class CodeRecording : IAsyncDisposable
{
    private readonly Browser browser;
    private readonly CodeTarget target;
    private readonly string channelId = $"dramaturge-code-{Guid.NewGuid():N}";
    private readonly object lockObject = new();
    private readonly List<IDisposable> observers = [];
    private readonly List<Task> documentCalls = [];
    private readonly List<string> statements = [];
    private readonly HashSet<string> namespaces = [];
    private readonly Dictionary<Page, string> pageVariables = [];
    private readonly Dictionary<Frame, string> frameVariables = [];
    private readonly HashSet<Page> actedPages = [];
    private readonly ObservableEventInvocable<CodeStatementEventArgs> onStatement = new("automation.codeStatement");
    private readonly ObservableEventInvocable<LocatorPickedEventArgs> onLocatorPicked = new("automation.locatorPicked");
    private readonly SemaphoreSlim stopLock = new(1, 1);
    private Task processing = Task.CompletedTask;
    private RecordedStep? pending;
    private string? subscriptionId;
    private (Frame Frame, string? ElementId, string Code, ElementLocator Locator)? lastLocator;
    private string mode = "record";
    private int downloadCount;
    private bool isStopped;

    private CodeRecording(Browser browser, CodeTarget target)
    {
        this.browser = browser;
        this.target = target;
    }

    /// <summary>
    /// Gets the whole file for the recording's target, built from the statements settled so far.
    /// </summary>
    public string Code
    {
        get
        {
            lock (this.lockObject)
            {
                BrowserGroup group = this.browser.Group;
                return CodeWriter.File(this.target, group.BrowserName, group.Options.TestIdAttribute, this.statements, this.namespaces);
            }
        }
    }

    /// <summary>
    /// Gets an event raised with each statement once it settles, in order.
    /// </summary>
    public ObservableEvent<CodeStatementEventArgs> OnStatement => this.onStatement;

    /// <summary>
    /// Gets an event raised with each element the user picks with the toolbar's Pick locator.
    /// </summary>
    public ObservableEvent<LocatorPickedEventArgs> OnLocatorPicked => this.onLocatorPicked;

    /// <summary>
    /// Ends the recording: settles the last statement, and stops recording in the browser's documents. Stopping
    /// again does nothing.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the commands that stop recording in the documents.</param>
    /// <returns>The whole file, as <see cref="Code"/> gives it.</returns>
    public async Task<string> StopAsync(CancellationToken cancellationToken = default)
    {
        await this.stopLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (this.isStopped)
            {
                return this.Code;
            }

            Task processed;
            lock (this.lockObject)
            {
                this.isStopped = true;
                foreach (IDisposable observer in this.observers)
                {
                    observer.Dispose();
                }

                processed = this.processing;
            }

            await processed.ConfigureAwait(false);
            Task[] calls;
            lock (this.lockObject)
            {
                calls = [.. this.documentCalls];
            }

            await Task.WhenAll(calls).ConfigureAwait(false);
            await this.RunStepAsync("Removing the code recording's event subscription", () => this.browser.Group.Driver.Session.UnsubscribeAsync(new UnsubscribeByIdsCommandParameters(this.subscriptionId!), cancellationToken: cancellationToken)).ConfigureAwait(false);
            foreach (Frame frame in this.browser.Pages.SelectMany(page => page.Frames))
            {
                await this.RunStepAsync($"Stopping the code recording in browsing context {frame.Id}", () => this.browser.Group.ScriptHost.CallRecorderAsync(frame.Id, "(recorder) => recorder.stop()", [], this.CreateBudget(cancellationToken))).ConfigureAwait(false);
            }

            RecordedStep? last;
            lock (this.lockObject)
            {
                last = this.pending;
                this.pending = null;
            }

            if (last is not null)
            {
                await this.SettleAsync(last).ConfigureAwait(false);
            }

            this.browser.ReleaseCodeRecording(this);
            return this.Code;
        }
        finally
        {
            this.stopLock.Release();
        }
    }

    /// <summary>
    /// Ends the recording, as <see cref="StopAsync"/> does.
    /// </summary>
    /// <returns>A task that completes when the recording has ended.</returns>
    public async ValueTask DisposeAsync()
    {
        await this.StopAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Starts recording a browser's actions as code, in the documents already loaded before returning.
    /// </summary>
    /// <param name="browser">The browser.</param>
    /// <param name="options">The recording's settings, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels starting.</param>
    /// <returns>The recording.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the browser is already recording code.</exception>
    internal static async Task<CodeRecording> StartAsync(Browser browser, CodeRecordingOptions? options, CancellationToken cancellationToken)
    {
        CodeRecording recording = new(browser, (options ?? new CodeRecordingOptions()).Target);
        if (!browser.ClaimCodeRecording(recording))
        {
            throw new InvalidOperationException("The browser is already recording code.");
        }

        BiDiDriver driver = browser.Group.Driver;
        lock (recording.lockObject)
        {
            recording.observers.Add(driver.Script.OnMessage.AddObserver(recording.OnMessage));
            recording.observers.Add(driver.BrowsingContext.OnDomContentLoaded.AddObserver(e => recording.OnDocumentLoaded(e.BrowsingContextId)));
            recording.observers.Add(driver.BrowsingContext.OnNavigationStarted.AddObserver(e => recording.OnPageEvent(e.BrowsingContextId, frame => frame == frame.Page.MainFrame && e.Url != "about:blank", page => recording.OnNavigationAsync(page, e.Url))));
            recording.observers.Add(driver.BrowsingContext.OnDownloadWillBegin.AddObserver(e => recording.OnPageEvent(e.BrowsingContextId, _ => true, recording.OnDownloadAsync)));
            recording.observers.Add(driver.BrowsingContext.OnUserPromptOpened.AddObserver(e => recording.OnPageEvent(e.BrowsingContextId, _ => true, page => recording.OnDialogAsync(page, e.PromptType, e.Message))));
            recording.observers.Add(browser.OnPageCreated.AddObserver(e => recording.Enqueue(() => recording.OnPopupAsync(e.Page))));
            recording.observers.Add(browser.OnPageClosed.AddObserver(e => recording.Enqueue(() => recording.OnPageClosedAsync(e.Page))));
        }

        try
        {
            // Subscribing before installing in the documents already loaded, so that none of their messages is missed.
            SubscribeCommandParameters subscription = new([driver.Script.OnMessage.EventName], userContexts: [browser.Id]);
            recording.subscriptionId = (await driver.Session.SubscribeAsync(subscription, cancellationToken: cancellationToken).ConfigureAwait(false)).SubscriptionId;
        }
        catch
        {
            lock (recording.lockObject)
            {
                recording.isStopped = true;
                recording.observers.ForEach(observer => observer.Dispose());
            }

            browser.ReleaseCodeRecording(recording);
            throw;
        }

        await Task.WhenAll(browser.Pages.SelectMany(page => page.Frames).Select(frame => recording.Install(frame.Id))).ConfigureAwait(false);
        return recording;
    }

    private static string[] LocatorNamespaces(string locator) => locator.Contains("new CssLocator(") ? [CodeWriter.BrowsingContextNamespace] : [];

    private static string[] Strings(JsonElement array) => [.. array.EnumerateArray().Select(item => item.GetString()!)];

    private static string Capitalize(string name) => char.ToUpperInvariant(name[0]) + name.Substring(1);

    // A step for an action, or null for an action the recording does not know.
    private static RecordedStep? CreateStep(JsonElement action, string locator)
    {
        string[] locatorNamespaces = LocatorNamespaces(locator);
        string[] assertionNamespaces = [.. locatorNamespaces, CodeWriter.AssertionsNamespace];
        RecordedStep Action(string call, IReadOnlyList<string> namespaces, string? mergeKey = null, int clickCount = 0) => new($"await {locator}.{call};", namespaces, mergeKey, clickCount) { Call = $"{locator}.{call}" };
        RecordedStep Assertion(string call) => new($"await Expect({locator}).{call};", assertionNamespaces, SettlesAtOnce: true);
        string? Modifiers() => CodeWriter.Modifiers(Strings(action.GetProperty("modifiers")));

        switch (action.GetProperty("kind").GetString())
        {
            case "click":
                string button = Capitalize(action.GetProperty("button").GetString()!);
                int clickCount = action.GetProperty("clickCount").GetInt32();
                string? modifiers = Modifiers();
                string options = CodeWriter.Options(
                    button == "Left" ? null : $"Button = PointerButton.{button}",
                    clickCount > 2 ? $"ClickCount = {clickCount}" : null,
                    modifiers is null ? null : $"Modifiers = {modifiers}");
                string method = clickCount == 2 ? "DblClickAsync" : "ClickAsync";
                return Action($"{method}({options})", button == "Left" ? locatorNamespaces : [.. locatorNamespaces, CodeWriter.InputNamespace], $"click {locator} {button} {modifiers}", clickCount);
            case "check":
                return Action("CheckAsync()", locatorNamespaces);
            case "uncheck":
                return Action("UncheckAsync()", locatorNamespaces);
            case "fill":
                return Action($"FillAsync({CodeWriter.Literal(action.GetProperty("value").GetString()!)})", locatorNamespaces, $"fill {locator}");
            case "press":
                string pressed = action.GetProperty("key").GetString()!;
                string? key = CodeWriter.Key(pressed);
                if (key is null)
                {
                    return new($"// The key {CodeWriter.Literal(pressed)} was pressed, which is neither a member of Keys nor a character.", []);
                }

                string pressOptions = CodeWriter.Options(Modifiers() is string held ? $"Modifiers = {held}" : null);
                return Action($"PressAsync({key}{(pressOptions.Length == 0 ? string.Empty : ", " + pressOptions)})", key.StartsWith("Keys.", StringComparison.Ordinal) ? [.. locatorNamespaces, CodeWriter.InputNamespace] : locatorNamespaces);
            case "select":
                return Action($"SelectOptionAsync({CodeWriter.Collection(Strings(action.GetProperty("values")).Select(value => $"SelectOption.ByValue({CodeWriter.Literal(value)})"))})", locatorNamespaces);
            case "setInputFiles":
                return Action($"SetInputFilesAsync({CodeWriter.Collection(Strings(action.GetProperty("files")).Select(CodeWriter.Literal))})", locatorNamespaces);
            case "assertVisible":
                return Assertion("ToBeVisibleAsync()");
            case "assertText":
                return Assertion($"ToContainTextAsync({CodeWriter.Literal(action.GetProperty("text").GetString()!)})");
            case "assertValue":
                return Assertion($"ToHaveValueAsync({CodeWriter.Literal(action.GetProperty("value").GetString()!)})");
            case "assertChecked":
                return Assertion(action.GetProperty("checked").GetBoolean() ? "ToBeCheckedAsync()" : "Not.ToBeCheckedAsync()");
            case "assertSnapshot":
                return Assertion($"ToMatchAriaSnapshotAsync({CodeWriter.RawLiteral(action.GetProperty("snapshot").GetString()!)})");
            default:
                return null;
        }
    }

    private void OnDocumentLoaded(string contextId)
    {
        if (this.browser.Group.FindFrame(contextId)?.Page.Browser == this.browser)
        {
            this.InstallUnlessStopped(contextId);
        }
    }

    [ExcludeFromCodeCoverage] // Only a document loaded as the recording stops finds it stopped.
    private void InstallUnlessStopped(string contextId)
    {
        lock (this.lockObject)
        {
            if (!this.isStopped)
            {
                this.documentCalls.Add(this.Install(contextId));
            }
        }
    }

    // Starts recording in a document, once its scripts are installed; a document that closes first is not recorded.
    private Task Install(string contextId)
    {
        ChannelValue channel = new(new ChannelProperties(this.channelId) { SerializationOptions = new SerializationOptions() { MaxDomDepth = 0 }, Ownership = ResultOwnership.None });
        string current;
        lock (this.lockObject)
        {
            current = this.mode;
        }

        return this.RunStepAsync(
            $"Starting the code recording in browsing context {contextId}",
            () => this.browser.Group.ScriptHost.CallRecorderAsync(contextId, "(recorder, send, testIdAttribute, mode) => recorder.start(send, testIdAttribute, mode)", [channel, LocalValue.String(this.browser.Group.Options.TestIdAttribute), LocalValue.String(current)], this.CreateBudget(CancellationToken.None)));
    }

    // The sender's frame is found as the message arrives: its page may close before the message is handled.
    private void OnMessage(MessageEventArgs e)
    {
        Frame? frame = e.Source.BrowsingContextId is string contextId ? this.browser.Group.FindFrame(contextId) : null;
        if (e.ChannelId == this.channelId && frame?.Page.Browser == this.browser)
        {
            this.Enqueue(() => this.HandleAsync(frame, e.Data.As<CollectionRemoteValue>().Value!));
        }
    }

    // An event in one of the browser's frames that satisfies a condition is handled for the frame's page.
    private void OnPageEvent(string contextId, Func<Frame, bool> condition, Func<Page, Task> handle)
    {
        if (this.browser.Group.FindFrame(contextId) is Frame frame && frame.Page.Browser == this.browser && condition(frame))
        {
            this.Enqueue(() => handle(frame.Page));
        }
    }

    // Handles messages and events one at a time, in the order they arrive.
    private void Enqueue(Func<Task> handle)
    {
        lock (this.lockObject)
        {
            if (!this.isStopped)
            {
                this.processing = this.processing.ContinueWith(_ => this.RunStepAsync("Recording a message or an event", handle), CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default).Unwrap();
            }
        }
    }

    private async Task HandleAsync(Frame frame, RemoteValueList data)
    {
        using JsonDocument document = JsonDocument.Parse(data[0].As<StringRemoteValue>().Value);
        JsonElement message = document.RootElement;
        string kind = message.GetProperty("kind").GetString()!;
        if (kind == "mode")
        {
            await this.ChooseModeAsync(frame, message.GetProperty("mode").GetString()!).ConfigureAwait(false);
            return;
        }

        if (await this.DescribeFramesAsync(frame).ConfigureAwait(false) is not List<(Frame Frame, string Code)> frames)
        {
            return;
        }

        (string Code, ElementLocator Locator) found = await this.FindLocatorAsync(frame, message, [.. data.Skip(1).Select(element => element.As<NodeRemoteValue>())]).ConfigureAwait(false);
        List<RecordedStep> settled = [];
        LocatorPickedEventArgs? picked = null;
        lock (this.lockObject)
        {
            this.actedPages.Add(frame.Page);
            string pageVariable = this.GetPageVariable(frame.Page, settled);
            string locator = $"{this.DeclareFrames(frame, frames, pageVariable, settled)}.{found.Code}";
            switch (kind)
            {
                case "pick":
                    settled.AddRange(this.TakePending());
                    picked = new LocatorPickedEventArgs(frame.Page, found.Locator, locator);
                    break;
                default:
                    if (CreateStep(message, locator) is RecordedStep created)
                    {
                        RecordedStep step = created with { Page = frame.Page, PageVariable = pageVariable };
                        if (step.SettlesAtOnce || this.pending is null || !step.Replaces(this.pending))
                        {
                            settled.AddRange(this.TakePending());
                        }

                        if (step.SettlesAtOnce)
                        {
                            settled.Add(step);
                        }
                        else
                        {
                            this.pending = step;
                        }
                    }

                    break;
            }
        }

        foreach (RecordedStep step in settled)
        {
            await this.SettleAsync(step).ConfigureAwait(false);
        }

        if (picked is not null)
        {
            await this.onLocatorPicked.InvokeNotifyObserversAsync(picked).ConfigureAwait(false);
        }
    }

    // A navigation the pending action on the page caused waits for it; one in a popup the action opened is the popup's
    // own; any other is the user's, as when typing an address.
    private Task OnNavigationAsync(Page page, string url)
    {
        return this.ChangeAsync(settled =>
        {
            if (this.pending is { Call: not null } action && action.Page == page)
            {
                this.pending = action.WithEffect(null, "RunAndWaitForNavigationAsync");
            }
            else if (page.Opener is null || this.actedPages.Contains(page))
            {
                settled.AddRange(this.TakePending());
                settled.Add(new($"await {this.GetPageVariable(page, settled)}.NavigateAsync({CodeWriter.Literal(url)});", []));
            }
        });
    }

    private Task OnPopupAsync(Page popup)
    {
        return this.ChangeAsync(_ =>
        {
            if (this.pending is { Call: not null } action && action.Page == popup.Opener && !action.HasEffect)
            {
                this.pending = action.WithEffect($"Page {this.NameVariable(popup)} = ", "RunAndWaitForPopupAsync");
            }
        });
    }

    private Task OnDownloadAsync(Page page)
    {
        return this.ChangeAsync(_ =>
        {
            if (this.pending is { Call: not null } action && action.Page == page && !action.HasEffect)
            {
                string variable = this.downloadCount++ == 0 ? "download" : $"download{this.downloadCount - 1}";
                this.pending = action.WithEffect($"Download {variable} = ", "RunAndWaitForDownloadAsync");
            }
        });
    }

    // A dialog is noted; the browser handles it as the session's prompt behaviour says, on replay as when recording.
    private Task OnDialogAsync(Page page, UserPromptType type, string message)
    {
        string kind = type.ToString().ToLowerInvariant();
        return this.ChangeAsync(settled =>
        {
            if (this.pending is { Call: not null } action && action.Page == page)
            {
                this.pending = action with { Comments = [.. action.Comments, $"// Opens a dialog ({kind}): {CodeWriter.Literal(message)}"] };
            }
            else
            {
                settled.AddRange(this.TakePending());
                settled.Add(new($"// A dialog opened ({kind}): {CodeWriter.Literal(message)}", []));
            }
        });
    }

    // Closing the first page ends the session, rather than being a step of it.
    private Task OnPageClosedAsync(Page page)
    {
        return this.ChangeAsync(settled =>
        {
            if (this.pageVariables.TryGetValue(page, out string? variable) && variable != "page")
            {
                settled.AddRange(this.TakePending());
                settled.Add(new($"await {variable}.CloseAsync();", []));
            }
        });
    }

    // Changes the pending and settled statements under the lock, then settles those it settled.
    private async Task ChangeAsync(Action<List<RecordedStep>> change)
    {
        List<RecordedStep> settled = [];
        lock (this.lockObject)
        {
            change(settled);
        }

        foreach (RecordedStep step in settled)
        {
            await this.SettleAsync(step).ConfigureAwait(false);
        }
    }

    // The frames from the page's main frame down to a frame that have no variable yet, each with its element's
    // locator in its parent, or null when a frame's element cannot be found, as in a shadow root.
    private async Task<List<(Frame Frame, string Code)>?> DescribeFramesAsync(Frame frame)
    {
        List<(Frame Frame, string Code)> frames = [];
        for (Frame current = frame; current.ParentFrame is Frame parent; current = parent)
        {
            lock (this.lockObject)
            {
                if (this.frameVariables.ContainsKey(current))
                {
                    break;
                }
            }

            if (await this.FindFrameElementAsync(parent, current).ConfigureAwait(false) is not string code)
            {
                await this.browser.Group.LogAsync($"An action in browsing context {frame.Id} was not recorded: the element of frame {current.Id} was not found in its parent.", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
                return null;
            }

            frames.Insert(0, (current, code));
        }

        return frames;
    }

    // The locator, in its parent, of a frame's element, or null when it cannot be found.
    private async Task<string?> FindFrameElementAsync(Frame parent, Frame child)
    {
        TimeBudget budget = this.CreateBudget(CancellationToken.None);
        RemoteValueList? values;
        try
        {
            RemoteValue described = await this.browser.Group.ScriptHost.CallRecorderAsync(parent.Id, "(recorder, testIdAttribute) => recorder.describeFrames(testIdAttribute)", [LocalValue.String(this.browser.Group.Options.TestIdAttribute)], budget).ConfigureAwait(false);
            values = described.As<CollectionRemoteValue>().Value!.Select(entry => entry.As<CollectionRemoteValue>().Value!).FirstOrDefault(entry => entry[0].As<WindowProxyRemoteValue>().Value.BrowsingContextId == child.Id);
        }
        catch (Exception)
        {
            // A parent that cannot describe its frames, as one that has closed, finds none.
            return null;
        }

        if (values is null)
        {
            return null;
        }

        JsonElement description;
        using (JsonDocument document = JsonDocument.Parse(values[1].As<StringRemoteValue>().Value))
        {
            description = document.RootElement.Clone();
        }

        NodeRemoteValue[] elements = [.. values.Skip(2).Select(element => element.As<NodeRemoteValue>())];
        return (await LocatorGenerator.GenerateAsync(parent, description.GetProperty("target"), [.. description.GetProperty("ancestors").EnumerateArray()], elements, budget).ConfigureAwait(false)).Code;
    }

    // Declares the frames without variables, top down, each after settling the pending statement, and gives the
    // variable of the frame acted in: its page's for a main frame.
    private string DeclareFrames(Frame frame, List<(Frame Frame, string Code)> frames, string pageVariable, List<RecordedStep> settled)
    {
        string Variable(Frame target) => target.ParentFrame is null ? pageVariable : this.frameVariables[target];
        foreach ((Frame declared, string code) in frames)
        {
            settled.AddRange(this.TakePending());
            string variable = this.frameVariables.Count == 0 ? "frame" : $"frame{this.frameVariables.Count}";
            settled.Add(new($"Frame {variable} = await {Variable(declared.ParentFrame!)}.{code}.ContentFrameAsync();", LocatorNamespaces(code)));
            this.frameVariables[declared] = variable;
        }

        return Variable(frame);
    }

    // Using the toolbar settles the pending statement.
    private async Task ChooseModeAsync(Frame source, string chosenMode)
    {
        List<RecordedStep> settled;
        lock (this.lockObject)
        {
            settled = [.. this.TakePending()];
            this.mode = chosenMode;
        }

        this.SetModeInOtherDocuments(source, chosenMode);
        foreach (RecordedStep step in settled)
        {
            await this.SettleAsync(step).ConfigureAwait(false);
        }
    }

    // The element's locator; consecutive actions on one element, such as the fills of typing, share the first's.
    private async Task<(string Code, ElementLocator Locator)> FindLocatorAsync(Frame frame, JsonElement message, IReadOnlyList<NodeRemoteValue> elements)
    {
        if (this.lastLocator is { } last && last.Frame == frame && last.ElementId == elements[0].SharedId)
        {
            return (last.Code, last.Locator);
        }

        (string code, ElementLocator locator) = await LocatorGenerator.GenerateAsync(frame, message.GetProperty("target"), [.. message.GetProperty("ancestors").EnumerateArray()], elements, this.CreateBudget(CancellationToken.None)).ConfigureAwait(false);
        this.lastLocator = (frame, elements[0].SharedId, code, locator);
        return (code, locator);
    }

    // The page's variable, declaring a page after the first, after settling the pending statement, when it has none.
    private string GetPageVariable(Page page, List<RecordedStep> settled)
    {
        if (!this.pageVariables.TryGetValue(page, out string? variable))
        {
            variable = this.NameVariable(page);
            if (variable != "page")
            {
                settled.AddRange(this.TakePending());
                settled.Add(new($"Page {variable} = await page.Browser.NewPageAsync();", []));
            }
        }

        return variable;
    }

    private string NameVariable(Page page)
    {
        string variable = this.pageVariables.Count == 0 ? "page" : $"page{this.pageVariables.Count}";
        this.pageVariables[page] = variable;
        return variable;
    }

    // The document that chose the mode has applied it already.
    private void SetModeInOtherDocuments(Frame source, string chosenMode)
    {
        lock (this.lockObject)
        {
            foreach (Frame frame in this.browser.Pages.SelectMany(page => page.Frames).Where(frame => frame != source))
            {
                this.documentCalls.Add(this.RunStepAsync(
                    $"Setting the code recording's mode in browsing context {frame.Id}",
                    () => this.browser.Group.ScriptHost.CallRecorderAsync(frame.Id, "(recorder, mode) => recorder.setMode(mode)", [LocalValue.String(chosenMode)], this.CreateBudget(CancellationToken.None))));
            }
        }
    }

    private IEnumerable<RecordedStep> TakePending()
    {
        RecordedStep? taken = this.pending;
        this.pending = null;
        return taken is null ? [] : [taken];
    }

    private async Task SettleAsync(RecordedStep step)
    {
        lock (this.lockObject)
        {
            this.statements.Add(step.Text);
            this.namespaces.UnionWith(step.Namespaces);
        }

        await this.onStatement.InvokeNotifyObserversAsync(new CodeStatementEventArgs(step.Text)).ConfigureAwait(false);
    }

    private TimeBudget CreateBudget(CancellationToken cancellationToken)
    {
        return new TimeBudget(this.browser.Group.Options.NavigationTimeout, this.browser.Group.Options.TimeProvider, cancellationToken);
    }

    // Runs a step that may fail, as when its document has closed, logging the failure.
    private async Task RunStepAsync(string description, Func<Task> step)
    {
        try
        {
            await step().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await this.browser.Group.LogAsync($"{description} failed: {ex.Message}", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
        }
    }

    // An action's or an assertion's statement; a step with a merge key replaces the pending step with the same key: a
    // fill of the same field, or the next click of the same series, unless an effect or a dialog was attached to it.
    // An assertion settles at once. An action's call, on its page, can be wrapped in a wait for what it causes.
    private sealed record RecordedStep(string Statement, IReadOnlyList<string> Namespaces, string? MergeKey = null, int ClickCount = 0, bool SettlesAtOnce = false)
    {
        public string? Call { get; init; }

        public Page? Page { get; init; }

        public string? PageVariable { get; init; }

        public bool HasEffect { get; init; }

        public IReadOnlyList<string> Comments { get; init; } = [];

        public string Text => string.Join("\n", [.. this.Comments, this.Statement]);

        public bool Replaces(RecordedStep earlier)
        {
            return this.MergeKey is not null && this.MergeKey == earlier.MergeKey && (this.ClickCount == 0 || this.ClickCount == earlier.ClickCount + 1) && !earlier.HasEffect && earlier.Comments.Count == 0;
        }

        // The first effect wraps the call; a later one, such as a navigation after a popup, is left out.
        public RecordedStep WithEffect(string? declaration, string wait)
        {
            return this.HasEffect ? this : this with { Statement = $"{declaration}await {this.PageVariable}.{wait}(() => {this.Call});", HasEffect = true };
        }
    }
}
