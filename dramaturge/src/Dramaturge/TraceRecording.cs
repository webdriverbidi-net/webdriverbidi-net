// <copyright file="TraceRecording.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Dramaturge.Network;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;
using StackFrame = System.Diagnostics.StackFrame;
using StackTrace = System.Diagnostics.StackTrace;

/// <summary>
/// The recording of a browser's trace: the actions taken on its pages, with their timings, logs, errors, and the
/// code that called them, and the pages' console messages, errors, and network traffic. Disposing it, or
/// <see cref="StopAsync"/>, ends it and writes the trace, a zip file that Playwright's trace viewer opens, such as
/// at https://trace.playwright.dev or with <c>npx playwright show-trace</c>.
/// </summary>
public sealed class TraceRecording : IAsyncDisposable
{
    private static readonly HashSet<Assembly> LibraryAssemblies = [typeof(TraceRecording).Assembly, typeof(BiDiDriver).Assembly, typeof(ScriptException).Assembly];

    private readonly Browser browser;
    private readonly TraceRecordingOptions options;
    private readonly NetworkTrafficMonitor monitor;
    private readonly TraceWriter writer = new();
    private readonly double startTime = TraceWriter.Now;
    private readonly DateTime startWallTime = DateTime.UtcNow;
    private readonly object lockObject = new();
    private readonly List<IDisposable> observers = [];
    private readonly List<Task> loadCaptures = [];
    private readonly HashSet<string> sourceFiles = [];
    private readonly SemaphoreSlim stopLock = new(1, 1);
    private int lastCallId;
    private bool isStopped;

    private TraceRecording(Browser browser, string path, TraceRecordingOptions options, NetworkTrafficMonitor monitor)
    {
        this.browser = browser;
        this.options = options;
        this.Path = System.IO.Path.GetFullPath(path);
        this.monitor = monitor;
    }

    /// <summary>
    /// Gets the full path of the file the trace is written to.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Ends the recording and writes the trace, after waiting, for at most <see cref="DramaturgeOptions.NavigationTimeout"/>,
    /// for requests in flight; a request still in flight then is written as failed. Stopping again does nothing.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the wait for requests in flight.</param>
    /// <returns>The trace's path.</returns>
    public async Task<string> StopAsync(CancellationToken cancellationToken = default)
    {
        await this.stopLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (this.isStopped)
            {
                return this.Path;
            }

            lock (this.lockObject)
            {
                this.isStopped = true;
                foreach (IDisposable observer in this.observers)
                {
                    observer.Dispose();
                }
            }

            this.browser.ReleaseTrace(this);
            await Task.WhenAll(this.loadCaptures).ConfigureAwait(false);
            List<NetworkRequest> requests = [.. await this.monitor.GetCapturedTrafficAsync(this.browser.Group.Options.NavigationTimeout, cancellationToken).ConfigureAwait(false)];
            await this.monitor.DisposeAsync().ConfigureAwait(false);
            requests.AddRange(await this.monitor.GetCapturedTrafficAsync(TimeSpan.Zero, CancellationToken.None).ConfigureAwait(false));
            IReadOnlyList<string> network = HarGenerator.GenerateTraceEntries(requests, this.Place, this.writer.AddResource);
            foreach (string file in this.sourceFiles.Where(File.Exists))
            {
                this.writer.AddSource(file, File.ReadAllBytes(file));
            }

            this.writer.Save(this.Path, network);
            return this.Path;
        }
        finally
        {
            this.stopLock.Release();
        }
    }

    /// <summary>
    /// Ends the recording and writes the trace, as <see cref="StopAsync"/> does.
    /// </summary>
    /// <returns>A task that completes when the trace is written.</returns>
    public async ValueTask DisposeAsync()
    {
        await this.StopAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Starts recording a browser's trace.
    /// </summary>
    /// <param name="browser">The browser.</param>
    /// <param name="path">The path of the trace to write.</param>
    /// <param name="options">The recording's settings, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels starting.</param>
    /// <returns>The recording.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the browser is already recording a trace.</exception>
    internal static async Task<TraceRecording> StartAsync(Browser browser, string path, TraceRecordingOptions? options, CancellationToken cancellationToken)
    {
        NetworkTrafficMonitorOptions monitorOptions = new();
        monitorOptions.UserContextIds.Add(browser.Id);
        TraceRecording recording = new(browser, path, options ?? new TraceRecordingOptions(), new NetworkTrafficMonitor(browser.Group.Driver, monitorOptions));

        // The trace starts with its context's options, before any action the browser records once it is claimed.
        recording.writer.WriteContextOptions(browser.Group.BrowserName, recording.options.Title, browser.Group.Options.TestIdAttribute);
        if (!browser.ClaimTrace(recording))
        {
            throw new InvalidOperationException("The browser is already recording a trace.");
        }

        try
        {
            await recording.monitor.StartMonitoringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            browser.ReleaseTrace(recording);
            throw;
        }

        lock (recording.lockObject)
        {
            recording.observers.Add(browser.OnPageCreated.AddObserver(e => recording.Follow(e.Page)));
            recording.observers.Add(browser.OnPageClosed.AddObserver(e => recording.writer.WritePageClosed(e.Page.Id)));
            if (recording.options.Screenshots)
            {
                recording.observers.Add(browser.Group.Driver.BrowsingContext.OnLoad.AddObserver(
                    e =>
                    {
                        recording.CaptureLoad(e.BrowsingContextId);
                        return Task.CompletedTask;
                    },
                    ObservableEventHandlerOptions.RunHandlerAsynchronously));
            }

            foreach (Page page in browser.Pages)
            {
                recording.Follow(page);
            }
        }

        return recording;
    }

    /// <summary>
    /// Runs an action, recorded in the trace of its page's browser if the browser is recording one.
    /// </summary>
    /// <typeparam name="T">The action's result type.</typeparam>
    /// <param name="page">The page the action is taken on.</param>
    /// <param name="budget">The action's budget.</param>
    /// <param name="call">What the action is.</param>
    /// <param name="action">The action, given its budget, which carries its trace entry when it is recorded.</param>
    /// <returns>The action's result.</returns>
    internal static Task<T> RunAsync<T>(Page page, TimeBudget budget, TracedCall call, Func<TimeBudget, Task<T>> action)
    {
        TraceRecording? recording = page.Browser.ActiveTrace;
        return recording is null ? action(budget) : recording.RecordAsync(page, budget, call, action);
    }

    /// <summary>
    /// Runs an action without a result, recorded in the trace of its page's browser if the browser is recording one.
    /// </summary>
    /// <param name="page">The page the action is taken on.</param>
    /// <param name="budget">The action's budget.</param>
    /// <param name="call">What the action is.</param>
    /// <param name="action">The action, given its budget, which carries its trace entry when it is recorded.</param>
    /// <returns>A task that completes when the action has.</returns>
    internal static Task RunAsync(Page page, TimeBudget budget, TracedCall call, Func<TimeBudget, Task> action)
    {
        return RunAsync(page, budget, call, async actionBudget =>
        {
            await action(actionBudget).ConfigureAwait(false);
            return true;
        });
    }

    /// <summary>
    /// Writes a line of an action's log.
    /// </summary>
    /// <param name="callId">The action's ID.</param>
    /// <param name="message">The line.</param>
    internal void WriteLog(string callId, string message)
    {
        this.writer.WriteLog(callId, message);
    }

    /// <summary>
    /// Records the element an action acts on, as it acts.
    /// </summary>
    /// <param name="trace">The action's entry.</param>
    /// <param name="frame">The element's frame.</param>
    /// <param name="target">The element.</param>
    /// <param name="offset">The point's offset from the element's center, or <see langword="null"/>.</param>
    /// <returns>A task that completes when the element is recorded.</returns>
    internal async Task RecordTargetAsync(ActionTrace trace, Frame frame, NodeRemoteValue target, PointerOffset? offset)
    {
        if (!this.options.Snapshots)
        {
            return;
        }

        (double X, double Y)? point = await this.SnapshotAsync(trace.Page, trace.CallId, "action", frame, target, offset).ConfigureAwait(false);
        if (point is (double x, double y))
        {
            this.writer.WriteInput(trace.CallId, x, y);
        }
    }

    // The frames of the code that called the library: those with source files after the library's own, up to the
    // library's next frame, which, for code an event or a continuation runs, is what ran it. Frames without source
    // files, such as the runtime's, are passed over.
    private static List<TraceStackFrame> CaptureStack()
    {
        List<TraceStackFrame> frames = [];
        foreach (StackFrame frame in new StackTrace(1, true).GetFrames())
        {
            (Type type, string method) = MethodOf(frame);
            string? file = frame.GetFileName();
            if (LibraryAssemblies.Contains(type.Assembly))
            {
                if (frames.Count > 0)
                {
                    break;
                }
            }
            else if (file is not null)
            {
                frames.Add(new TraceStackFrame(file, frame.GetFileLineNumber(), frame.GetFileColumnNumber(), FunctionName(type, method)));
            }
        }

        return frames;
    }

    // A frame whose method was trimmed, or is not in a type, is taken for one of the library's own, and left out.
    [ExcludeFromCodeCoverage] // Only trimming, and languages other than C#, leave a frame without a method's type.
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Only the frame's method's name and declaring type are read; a frame whose method was trimmed is left out.")]
    private static (Type Type, string Method) MethodOf(StackFrame frame)
    {
        MethodBase? method = frame.GetMethod();
        return method?.DeclaringType is Type type ? (type, method.Name) : (typeof(TraceRecording), string.Empty);
    }

    // Async methods and lambdas run in types or methods the compiler generates, nested in the method's type and named
    // for the method, such as <PlacesAnOrder>d__3, or, in a lambda class, <PlacesAnOrder>b__3_0.
    private static string FunctionName(Type type, string method)
    {
        if (type.IsNested && type.Name.StartsWith("<", StringComparison.Ordinal))
        {
            method = type.Name.StartsWith("<>", StringComparison.Ordinal) ? method : type.Name;
            type = type.DeclaringType!;
        }

        if (method.StartsWith("<", StringComparison.Ordinal))
        {
            method = method.Substring(1, method.IndexOf('>') - 1);
        }

        return $"{type.Name}.{method}";
    }

    // The frames' placeholders in the snapshot become their browsing contexts' IDs, which name their own snapshots.
    private static TraceFrameSnapshot ReadSnapshot(RemoteValue result, string phase, string callId, Page page, Frame frame)
    {
        using JsonDocument document = JsonDocument.Parse(Property(result, "json").As<StringRemoteValue>().Value);
        JsonElement snapshot = document.RootElement;
        string html = snapshot.GetProperty("html").GetRawText();
        IList<RemoteValue> windows = Property(result, "frames").As<CollectionRemoteValue>().Value!;
        for (int index = 0; index < windows.Count; index++)
        {
            string source = windows[index] is WindowProxyRemoteValue window ? $"/snapshot/{window.Value.BrowsingContextId}" : string.Empty;
            html = html.Replace($"\"/snapshot/@{index}\"", $"\"{source}\"");
        }

        JsonElement viewport = snapshot.GetProperty("viewport");
        string? doctype = snapshot.TryGetProperty("doctype", out JsonElement type) ? type.GetString() : null;
        return new TraceFrameSnapshot(phase, callId, page.Id, frame.Id, frame.Url, frame == page.MainFrame, doctype, html, viewport.GetProperty("width").GetDouble(), viewport.GetProperty("height").GetDouble(), snapshot.GetProperty("wallTime").GetDouble(), snapshot.GetProperty("collectionTime").GetDouble());
    }

    private static RemoteValue Property(RemoteValue value, string name)
    {
        return value.As<KeyValuePairCollectionRemoteValue>().Value!.First(property => property.Key is string key && key == name).Value;
    }

    private static double Number(RemoteValue value, string name) => Property(value, name).As<NumberRemoteValue>().Value;

    private static TraceSourceLocation Locate(WebDriverBiDi.Script.StackTrace? stack)
    {
        WebDriverBiDi.Script.StackFrame? top = stack?.CallFrames.FirstOrDefault();
        return top is null ? new TraceSourceLocation(string.Empty, 0, 0) : new TraceSourceLocation(top.Url, (long)top.LineNumber, (long)top.ColumnNumber);
    }

    private static string? DescribeStack(string message, WebDriverBiDi.Script.StackTrace? stack)
    {
        if (stack is null)
        {
            return null;
        }

        StringBuilder text = new($"Error: {message}");
        foreach (WebDriverBiDi.Script.StackFrame frame in stack.CallFrames)
        {
            text.Append($"\n    at {(frame.FunctionName.Length == 0 ? "<anonymous>" : frame.FunctionName)} ({frame.Url}:{frame.LineNumber + 1}:{frame.ColumnNumber + 1})");
        }

        return text.ToString();
    }

    private async Task<T> RecordAsync<T>(Page page, TimeBudget budget, TracedCall call, Func<TimeBudget, Task<T>> action)
    {
        ActionTrace trace = new(this, $"call@{Interlocked.Increment(ref this.lastCallId)}", page);
        List<TraceStackFrame> stack = CaptureStack();
        if (this.options.Sources)
        {
            lock (this.lockObject)
            {
                this.sourceFiles.UnionWith(stack.Select(frame => frame.File));
            }
        }

        this.writer.WriteBefore(trace.CallId, call, stack);
        if (this.options.Snapshots)
        {
            // The snapshot is not the action's own work, so it does not spend the action's time.
            await this.SnapshotAsync(page, trace.CallId, "before", null, null, null).ConfigureAwait(false);
            budget = budget.Restart();
        }

        Exception? error = null;
        try
        {
            return await action(budget.WithTrace(trace)).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            error = exception;
            throw;
        }
        finally
        {
            double endTime = TraceWriter.Now;
            if (this.options.Snapshots)
            {
                await this.SnapshotAsync(page, trace.CallId, "after", null, null, null).ConfigureAwait(false);
            }

            if (this.options.Screenshots)
            {
                await this.ScreenshotAsync(page).ConfigureAwait(false);
            }

            this.writer.WriteAfter(trace.CallId, endTime, error);
        }
    }

    // Each frame of the page is taken in its own document; a frame that cannot be, such as one navigating away, is
    // left out. The point is the target's, when the target is in the page's main frame.
    private async Task<(double X, double Y)?> SnapshotAsync(Page page, string callId, string phase, Frame? targetFrame, NodeRemoteValue? target, PointerOffset? offset)
    {
        (double X, double Y)? point = null;
        TimeBudget budget = new(this.browser.Group.Options.ActionTimeout, this.browser.Group.Options.TimeProvider, CancellationToken.None);
        foreach (Frame frame in page.Frames)
        {
            bool isTargetFrame = frame == targetFrame;
            LocalValue targetArgument = isTargetFrame ? target!.ToSharedReference() : LocalValue.Null;
            LocalValue offsetArgument = isTargetFrame && offset is PointerOffset at
                ? LocalValue.Object(new Dictionary<string, LocalValue>() { ["x"] = LocalValue.Number(at.X), ["y"] = LocalValue.Number(at.Y) })
                : LocalValue.Null;
            try
            {
                RemoteValue result = await this.browser.Group.ScriptHost.CallDomSnapshotAsync(frame.Id, "(snapshots, target, offset) => snapshots.snapshot(target, offset)", [targetArgument, offsetArgument], budget).ConfigureAwait(false);
                this.writer.WriteFrameSnapshot(ReadSnapshot(result, phase, callId, page, frame));
                if (Property(result, "point") is KeyValuePairCollectionRemoteValue acted)
                {
                    point = (Number(acted, "x"), Number(acted, "y"));
                }
            }
            catch (WebDriverBiDiException)
            {
            }
        }

        return point;
    }

    private async Task ScreenshotAsync(Page page)
    {
        try
        {
            CaptureScreenshotCommandParameters parameters = new(page.Id) { Format = new ImageFormat() { Type = "image/jpeg", Quality = 0.5 } };
            TimeBudget budget = new(this.browser.Group.Options.ActionTimeout, this.browser.Group.Options.TimeProvider, CancellationToken.None);
            CaptureScreenshotCommandResult result = await this.browser.Group.Driver.BrowsingContext.CaptureScreenshotAsync(parameters, budget.Remaining, budget.CancellationToken).ConfigureAwait(false);
            this.writer.WriteScreencastFrame(page.Id, Convert.FromBase64String(result.Data));
        }
        catch (WebDriverBiDiException)
        {
            // A page that cannot be captured, such as one that has closed, has no frame.
        }
    }

    // A page of the browser that loads a document gets a frame of the filmstrip; the capture is kept, so that
    // stopping waits for it.
    private void CaptureLoad(string contextId)
    {
        if (this.browser.Group.FindFrame(contextId) is not Frame frame || frame != frame.Page.MainFrame || frame.Page.Browser != this.browser)
        {
            return;
        }

        this.AddLoadCapture(frame.Page);
    }

    [ExcludeFromCodeCoverage] // Only a load dispatched as the recording stops finds it stopped.
    private void AddLoadCapture(Page page)
    {
        lock (this.lockObject)
        {
            if (!this.isStopped)
            {
                this.loadCaptures.Add(this.ScreenshotAsync(page));
            }
        }
    }

    private void Follow(Page page)
    {
        lock (this.lockObject)
        {
            if (this.isStopped)
            {
                return;
            }

            this.writer.WritePageOpened(page.Id, page.Opener?.Id);
            this.observers.Add(page.OnConsoleMessage.AddObserver(e => this.writer.WriteConsoleMessage(page.Id, e.Method == "warn" ? "warning" : e.Method, e.Text, Locate(e.StackTrace))));
            this.observers.Add(page.OnPageError.AddObserver(e => this.writer.WritePageError(page.Id, e.Message, DescribeStack(e.Message, e.StackTrace), Locate(e.StackTrace))));
        }
    }

    // A request's page is the page of the frame that made it, while the frame is known, and its start is the time
    // since the recording started on the trace's clock.
    private (string? PageId, double MonotonicTime) Place(NetworkRequest request)
    {
        string? contextId = request.BrowsingContextId;
        string? pageId = contextId is null ? null : this.browser.Group.FindFrame(contextId)?.Page.Id ?? contextId;
        return (pageId, this.startTime + (request.StartedDateTime.ToUniversalTime() - this.startWallTime).TotalMilliseconds);
    }
}
