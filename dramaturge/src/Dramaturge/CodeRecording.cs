// <copyright file="CodeRecording.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using WebDriverBiDi;
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
    private readonly List<Task> installs = [];
    private readonly List<string> statements = [];
    private readonly HashSet<string> namespaces = [];
    private readonly Dictionary<Page, string> pageVariables = [];
    private readonly ObservableEventInvocable<CodeStatementEventArgs> onStatement = new("automation.codeStatement");
    private readonly SemaphoreSlim stopLock = new(1, 1);
    private Task processing = Task.CompletedTask;
    private RecordedStep? pending;
    private string? subscriptionId;
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
            Task[] installing;
            lock (this.lockObject)
            {
                this.isStopped = true;
                foreach (IDisposable observer in this.observers)
                {
                    observer.Dispose();
                }

                processed = this.processing;
                installing = [.. this.installs];
            }

            await processed.ConfigureAwait(false);
            await Task.WhenAll(installing).ConfigureAwait(false);
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
    private static RecordedStep? CreateStep(JsonElement action, string pageVariable)
    {
        string locator = $"{pageVariable}.Locate(new CssLocator({CodeWriter.Literal(action.GetProperty("target").GetProperty("cssPath").GetString()!)}))";
        string[] locatorNamespaces = [CodeWriter.BrowsingContextNamespace];
        string Statement(string call) => $"await {locator}.{call};";
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
                this.installs.Add(this.Install(contextId));
            }
        }
    }

    // Starts recording in a document, once its scripts are installed; a document that closes first is not recorded.
    private Task Install(string contextId)
    {
        ChannelValue channel = new(new ChannelProperties(this.channelId) { SerializationOptions = new SerializationOptions() { MaxDomDepth = 0 }, Ownership = ResultOwnership.None });
        return this.RunStepAsync(
            $"Starting the code recording in browsing context {contextId}",
            () => this.browser.Group.ScriptHost.CallRecorderAsync(contextId, "(recorder, send, testIdAttribute) => recorder.start(send, testIdAttribute)", [channel, LocalValue.String(this.browser.Group.Options.TestIdAttribute)], this.CreateBudget(CancellationToken.None)));
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

        List<RecordedStep> settled = [];
        lock (this.lockObject)
        {
            if (!this.pageVariables.TryGetValue(frame.Page, out string? variable))
            {
                variable = this.pageVariables.Count == 0 ? "page" : $"page{this.pageVariables.Count}";
                this.pageVariables[frame.Page] = variable;
                if (variable != "page")
                {
                    settled.AddRange(this.TakePending());
                    settled.Add(new($"Page {variable} = await page.Browser.NewPageAsync();", []));
                }
            }

            using JsonDocument document = JsonDocument.Parse(e.Data.As<CollectionRemoteValue>().Value![0].As<StringRemoteValue>().Value);
            if (CreateStep(document.RootElement, variable) is RecordedStep step)
            {
                if (this.pending is null || !step.Replaces(this.pending))
                {
                    settled.AddRange(this.TakePending());
                }

                this.pending = step;
            }
        }

        foreach (RecordedStep step in settled)
        {
            await this.SettleAsync(step).ConfigureAwait(false);
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

    // An action's statement; a step with a merge key replaces the pending step with the same key: a fill of the same
    // field, or the next click of the same series.
    private sealed record RecordedStep(string Statement, IReadOnlyList<string> Namespaces, string? MergeKey = null, int ClickCount = 0)
    {
        public bool Replaces(RecordedStep earlier)
        {
            return this.MergeKey is not null && this.MergeKey == earlier.MergeKey && (this.ClickCount == 0 || this.ClickCount == earlier.ClickCount + 1);
        }
    }
}
