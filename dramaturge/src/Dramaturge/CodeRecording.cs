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
    private readonly ObservableEventInvocable<CodeStatementEventArgs> onStatement = new("automation.codeStatement");
    private readonly ObservableEventInvocable<LocatorPickedEventArgs> onLocatorPicked = new("automation.locatorPicked");
    private readonly SemaphoreSlim stopLock = new(1, 1);
    private Task processing = Task.CompletedTask;
    private RecordedStep? pending;
    private string? subscriptionId;
    private (Frame Frame, string? ElementId, string Code, ElementLocator Locator)? lastLocator;
    private string mode = "record";
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

    private static string[] Strings(JsonElement array) => [.. array.EnumerateArray().Select(item => item.GetString()!)];

    private static string Capitalize(string name) => char.ToUpperInvariant(name[0]) + name.Substring(1);

    // A step for an action, or null for an action the recording does not know.
    private static RecordedStep? CreateStep(JsonElement action, string locator)
    {
        string[] locatorNamespaces = locator.Contains("new CssLocator(") ? [CodeWriter.BrowsingContextNamespace] : [];
        string[] assertionNamespaces = [.. locatorNamespaces, CodeWriter.AssertionsNamespace];
        string Statement(string call) => $"await {locator}.{call};";
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
                return new(Statement($"{method}({options})"), button == "Left" ? locatorNamespaces : [.. locatorNamespaces, CodeWriter.InputNamespace], $"click {locator} {button} {modifiers}", clickCount);
            case "check":
                return new(Statement("CheckAsync()"), locatorNamespaces);
            case "uncheck":
                return new(Statement("UncheckAsync()"), locatorNamespaces);
            case "fill":
                return new(Statement($"FillAsync({CodeWriter.Literal(action.GetProperty("value").GetString()!)})"), locatorNamespaces, $"fill {locator}");
            case "press":
                string pressed = action.GetProperty("key").GetString()!;
                string? key = CodeWriter.Key(pressed);
                if (key is null)
                {
                    return new($"// The key {CodeWriter.Literal(pressed)} was pressed, which is neither a member of Keys nor a character.", []);
                }

                string pressOptions = CodeWriter.Options(Modifiers() is string held ? $"Modifiers = {held}" : null);
                return new(Statement($"PressAsync({key}{(pressOptions.Length == 0 ? string.Empty : ", " + pressOptions)})"), key.StartsWith("Keys.", StringComparison.Ordinal) ? [.. locatorNamespaces, CodeWriter.InputNamespace] : locatorNamespaces);
            case "select":
                return new(Statement($"SelectOptionAsync({CodeWriter.Collection(Strings(action.GetProperty("values")).Select(value => $"SelectOption.ByValue({CodeWriter.Literal(value)})"))})"), locatorNamespaces);
            case "setInputFiles":
                return new(Statement($"SetInputFilesAsync({CodeWriter.Collection(Strings(action.GetProperty("files")).Select(CodeWriter.Literal))})"), locatorNamespaces);
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

    // Handles messages one at a time, in the order they arrive.
    private void OnMessage(MessageEventArgs e)
    {
        if (e.ChannelId != this.channelId)
        {
            return;
        }

        lock (this.lockObject)
        {
            if (!this.isStopped)
            {
                this.processing = this.processing.ContinueWith(_ => this.HandleAsync(e), CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default).Unwrap();
            }
        }
    }

    private async Task HandleAsync(MessageEventArgs e)
    {
        Frame? frame = e.Source.BrowsingContextId is string contextId ? this.browser.Group.FindFrame(contextId) : null;

        // Actions in a page's child frames are not recorded.
        if (frame is null || frame.Page.Browser != this.browser || frame != frame.Page.MainFrame)
        {
            return;
        }

        RemoteValueList data = e.Data.As<CollectionRemoteValue>().Value!;
        using JsonDocument document = JsonDocument.Parse(data[0].As<StringRemoteValue>().Value);
        JsonElement message = document.RootElement;
        string kind = message.GetProperty("kind").GetString()!;
        if (kind == "mode")
        {
            await this.ChooseModeAsync(frame, message.GetProperty("mode").GetString()!).ConfigureAwait(false);
            return;
        }

        (string Code, ElementLocator Locator) found = await this.FindLocatorAsync(frame, message, [.. data.Skip(1).Select(element => element.As<NodeRemoteValue>())]).ConfigureAwait(false);
        List<RecordedStep> settled = [];
        LocatorPickedEventArgs? picked = null;
        lock (this.lockObject)
        {
            switch (kind)
            {
                case "pick":
                    settled.AddRange(this.TakePending());
                    picked = new LocatorPickedEventArgs(frame.Page, found.Locator, $"{this.GetPageVariable(frame.Page, settled)}.{found.Code}");
                    break;
                default:
                    if (CreateStep(message, $"{this.GetPageVariable(frame.Page, settled)}.{found.Code}") is RecordedStep step)
                    {
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
            variable = this.pageVariables.Count == 0 ? "page" : $"page{this.pageVariables.Count}";
            this.pageVariables[page] = variable;
            if (variable != "page")
            {
                settled.AddRange(this.TakePending());
                settled.Add(new($"Page {variable} = await page.Browser.NewPageAsync();", []));
            }
        }

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
            this.statements.Add(step.Statement);
            this.namespaces.UnionWith(step.Namespaces);
        }

        await this.onStatement.InvokeNotifyObserversAsync(new CodeStatementEventArgs(step.Statement)).ConfigureAwait(false);
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
    // fill of the same field, or the next click of the same series. An assertion settles at once.
    private sealed record RecordedStep(string Statement, IReadOnlyList<string> Namespaces, string? MergeKey = null, int ClickCount = 0, bool SettlesAtOnce = false)
    {
        public bool Replaces(RecordedStep earlier)
        {
            return this.MergeKey is not null && this.MergeKey == earlier.MergeKey && (this.ClickCount == 0 || this.ClickCount == earlier.ClickCount + 1);
        }
    }
}
