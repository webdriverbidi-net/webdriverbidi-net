// <copyright file="TraceRecordingTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Dramaturge.TestUtilities;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using static Dramaturge.Assertions;
using static Dramaturge.TestUtilities.NetworkEvents;

public sealed class TraceRecordingTests : IDisposable
{
    private static readonly TimeSpan EventWait = TimeSpan.FromSeconds(10);
    private readonly string directory = Path.Combine(Path.GetTempPath(), $"dramaturge-trace-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(this.directory))
        {
            Directory.Delete(this.directory, true);
        }
    }

    [Fact]
    public async Task TraceStartsWithTheContextAndHasEachActionWithItsLogCallerAndResult()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        AnswerReadiness(session);
        ElementLocator button = page.Locate(new CssLocator("button"));
        string path = Path.Combine(this.directory, "nested", "trace.zip");

        TraceRecording recording = await page.Browser.RecordTraceAsync(path, new TraceRecordingOptions() { Title = "Checkout" }, TestContext.Current.CancellationToken);
        await button.ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        int count = await button.CountAsync(TestContext.Current.CancellationToken);
        await button.SetInputFilesAsync(["/tmp/a.txt", "/tmp/b.txt"], cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.FailWith("input.performActions", "unknown error", "The pointer is stuck.");
        await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => button.ClickAsync(cancellationToken: TestContext.Current.CancellationToken));
        string stopped = await recording.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, count);
        Assert.Equal(Path.GetFullPath(path), stopped);
        Assert.Equal(stopped, recording.Path);
        List<JsonObject> events = ReadLines(path, "trace.trace");
        JsonObject context = events[0];
        Assert.Equal("context-options", (string?)context["type"]);
        Assert.Equal(9, (int?)context["version"]);
        Assert.Equal("library", (string?)context["origin"]);
        Assert.Equal("Checkout", (string?)context["title"]);
        Assert.Equal("csharp", (string?)context["sdkLanguage"]);
        Assert.Equal("data-testid", (string?)context["testIdAttributeName"]);
        Assert.Contains((string?)context["platform"], new[] { "darwin", "linux", "win32" });
        Assert.Equal(page.Id, (string?)events.Single(e => (string?)e["method"] == "page")["params"]!["pageId"]);
        List<JsonObject> befores = [.. events.Where(e => (string?)e["type"] == "before")];
        Assert.Equal(["Click", "Count", "Set input files", "Click"], befores.Select(e => (string?)e["title"]));
        Assert.All(befores, before => Assert.Equal("{locator}", (string?)before["subtitle"]));
        Assert.All(befores, before => Assert.Equal("Locator", (string?)before["class"]));
        Assert.Equal(button.ToString(), (string?)befores[0]["params"]!["locator"]);
        JsonObject caller = befores[0]["stack"]!.AsArray()[0]!.AsObject();
        Assert.EndsWith("TraceRecordingTests.cs", (string?)caller["file"]);
        Assert.Equal("TraceRecordingTests.TraceStartsWithTheContextAndHasEachActionWithItsLogCallerAndResult", (string?)caller["function"]);
        Assert.True((int?)caller["line"] > 0);
        Assert.Equal("""["/tmp/a.txt","/tmp/b.txt"]""", befores[2]["params"]!["files"]!.ToJsonString());
        string firstCall = (string)befores[0]["callId"]!;
        Assert.Equal([$"waiting for {button} to be ready to be clicked", "the element was not visible"], events.Where(e => (string?)e["type"] == "log" && (string?)e["callId"] == firstCall).Select(e => (string?)e["message"]));
        List<JsonObject> afters = [.. events.Where(e => (string?)e["type"] == "after")];
        Assert.Equal(befores.Select(e => (string?)e["callId"]), afters.Select(e => (string?)e["callId"]));
        Assert.Null(afters[0]["error"]);
        Assert.Equal("WebDriverBiDiCommandException", (string?)afters[3]["error"]!["name"]);
        Assert.Contains("The pointer is stuck.", (string?)afters[3]["error"]!["message"]);
        Assert.True((double)afters[0]["endTime"]! >= (double)befores[0]["startTime"]!);
    }

    [Fact]
    public async Task CallersAreNamedForTheMethodsTheyWereWrittenIn()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        ElementLocator button = page.Locate(new CssLocator("button"));
        string path = Path.Combine(this.directory, "trace.zip");
        Func<ElementLocator, Task<int>> countInLambda = locator => locator.CountAsync(TestContext.Current.CancellationToken);

        await using (await page.Browser.RecordTraceAsync(path, cancellationToken: TestContext.Current.CancellationToken))
        {
            await CountFromHelper(button);
            await countInLambda(button);
            await Helpers.CountAsync(button);
        }

        List<JsonObject> befores = [.. ReadLines(path, "trace.trace").Where(e => (string?)e["type"] == "before")];
        Assert.Equal("TraceRecordingTests.CountFromHelper", (string?)befores[0]["stack"]![0]!["function"]);
        Assert.Equal("TraceRecordingTests.CallersAreNamedForTheMethodsTheyWereWrittenIn", (string?)befores[0]["stack"]![1]!["function"]);
        Assert.Equal("TraceRecordingTests.CallersAreNamedForTheMethodsTheyWereWrittenIn", (string?)befores[1]["stack"]![0]!["function"]);
        Assert.Equal("Helpers.CountAsync", (string?)befores[2]["stack"]![0]!["function"]);
    }

    [Fact]
    public async Task CallersEndWhereTheLibraryCalledThem()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        string path = Path.Combine(this.directory, "trace.zip");
        Task<int>? counted = null;
        page.Browser.OnPageCreated.AddObserver(e => counted = e.Page.Locate(new CssLocator("button")).CountAsync());

        await using (await page.Browser.RecordTraceAsync(path, cancellationToken: TestContext.Current.CancellationToken))
        {
            await page.Browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
            await counted!;
        }

        JsonArray stack = ReadLines(path, "trace.trace").Single(e => (string?)e["title"] == "Count")["stack"]!.AsArray();
        Assert.Equal(["TraceRecordingTests.CallersEndWhereTheLibraryCalledThem"], stack.Select(frame => (string?)frame!["function"]));
    }

    [Fact]
    public async Task EachTypesActionsAreRecordedUnderItsName()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        JsonObject snapshot = new()
        {
            ["type"] = "object",
            ["value"] = new JsonArray(
                new JsonArray("json", new JsonObject() { ["type"] = "string", ["value"] = """{"html":["HTML"],"viewport":{"width":800,"height":600},"url":"about:blank","wallTime":1,"collectionTime":1}""" }),
                new JsonArray("frames", new JsonObject() { ["type"] = "array", ["value"] = new JsonArray() }),
                new JsonArray("point", new JsonObject() { ["type"] = "null" })),
        };
        session.RemoteEnd.AnswerWith("script.callFunction", parameters => ProtocolJson.Success(((string)parameters["functionDeclaration"]!).Contains("snapshots.snapshot") ? (JsonObject)snapshot.DeepClone() : new JsonObject() { ["type"] = "string", ["value"] = "42" }));
        session.RemoteEnd.AnswerWith("storage.getCookies", new JsonObject() { ["cookies"] = new JsonArray(), ["partitionKey"] = new JsonObject() });
        string path = Path.Combine(this.directory, "trace.zip");

        await using (await page.Browser.RecordTraceAsync(path, new TraceRecordingOptions() { Snapshots = true }, TestContext.Current.CancellationToken))
        {
            Page opened = await page.Browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
            await opened.NavigateAsync("https://example.com/", cancellationToken: TestContext.Current.CancellationToken);
            await opened.EvaluateAsync<string>("() => '42'", cancellationToken: TestContext.Current.CancellationToken);
            await opened.MainFrame.EvaluateAsync("() => '42'", cancellationToken: TestContext.Current.CancellationToken);
            await opened.Keyboard.PressAsync("Enter", cancellationToken: TestContext.Current.CancellationToken);
            await opened.Mouse.ClickAsync(10, 20, cancellationToken: TestContext.Current.CancellationToken);
            await Expect(opened).ToHaveUrlAsync("https://example.com/", cancellationToken: TestContext.Current.CancellationToken);
            await page.Browser.GetCookiesAsync(cancellationToken: TestContext.Current.CancellationToken);
            await opened.CloseAsync(TestContext.Current.CancellationToken);
        }

        List<JsonObject> befores = [.. ReadLines(path, "trace.trace").Where(e => (string?)e["type"] == "before")];
        Assert.Equal(
            ["Browser:New page", "Page:Navigate", "Page:Evaluate", "Frame:Evaluate", "Keyboard:Press", "Mouse:Mouse click", "Expect:Expect to have URL \"https://example.com/\"", "Browser:Get cookies", "Page:Close"],
            befores.Select(e => $"{e["class"]}:{e["title"]}"));
        Assert.Equal("https://example.com/", (string?)befores[1]["params"]!["url"]);
        Assert.Equal("(10, 20)", (string?)befores[5]["params"]!["point"]);
        List<JsonObject> snapshots = [.. ReadLines(path, "trace.trace").Where(e => (string?)e["type"] == "frame-snapshot")];
        Assert.DoesNotContain(snapshots, e => (string?)e["snapshot"]!["callId"] == (string?)befores[0]["callId"]);
        Assert.Contains(snapshots, e => (string?)e["snapshot"]!["callId"] == (string?)befores[1]["callId"]);
    }

    [Fact]
    public async Task FrameActionsAreRecordedAsTheFrames()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(new JsonObject() { ["type"] = "string", ["value"] = "42" }));
        Frame frame = page.MainFrame;
        string path = Path.Combine(this.directory, "trace.zip");

        await using (await page.Browser.RecordTraceAsync(path, cancellationToken: TestContext.Current.CancellationToken))
        {
            await frame.WaitForLoadStateAsync(ReadinessState.None, cancellationToken: TestContext.Current.CancellationToken);
            await frame.WaitForUrlAsync(new Regex("^about:"), ReadinessState.None, cancellationToken: TestContext.Current.CancellationToken);
            await frame.WaitForUrlAsync(url => url.Length > 0, ReadinessState.None, cancellationToken: TestContext.Current.CancellationToken);
            await frame.RunAndWaitForNavigationAsync(() => frame.NavigateAsync("https://example.com/", ReadinessState.None, cancellationToken: TestContext.Current.CancellationToken), ReadinessState.None, cancellationToken: TestContext.Current.CancellationToken);
            await frame.WaitForFunctionAsync<string>("() => '42'", timeout: TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
            await frame.SetContentAsync("<p>Hello</p>", ReadinessState.None, cancellationToken: TestContext.Current.CancellationToken);
        }

        Assert.Equal(
            ["Wait for load state", "Wait for URL", "Wait for URL", "Run and wait for navigation", "Navigate", "Wait for function", "Set content"],
            ReadLines(path, "trace.trace").Where(e => (string?)e["type"] == "before" && (string?)e["class"] == "Frame").Select(e => (string?)e["title"]));
    }

    [Fact]
    public async Task PagesOpenedAndClosedAndTheirConsoleMessagesAndErrorsAreRecorded()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        string path = Path.Combine(this.directory, "trace.zip");

        TraceRecording recording = await page.Browser.RecordTraceAsync(path, cancellationToken: TestContext.Current.CancellationToken);
        TaskCompletionSource<Page> popped = new(TaskCreationOptions.RunContinuationsAsynchronously);
        page.OnPopup.AddObserver(e => popped.TrySetResult(e.Page));
        FakeContext popupContext = session.AddContext();
        await session.RemoteEnd.RaiseEventAsync("browsingContext.contextCreated", new JsonObject()
        {
            ["context"] = popupContext.Id,
            ["clientWindow"] = "window-2",
            ["originalOpener"] = page.Id,
            ["url"] = "about:blank",
            ["userContext"] = "default",
            ["children"] = null,
        });
        Page popup = await popped.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);
        await RaiseAndWaitAsync(session, popup, ConsoleEntry(popup.Id, "warn", "careful", Stack(("check", "https://example.com/app.js", 4, 2))));
        await RaiseAndWaitAsync(session, popup, ConsoleEntry(popup.Id, "log", "plain", null));
        await RaiseAndWaitAsync(session, popup, Entry("javascript", "error", popup.Id, "boom", Stack(("", "https://example.com/app.js", 9, 0), ("load", "https://example.com/main.js", 1, 5))));
        await RaiseAndWaitAsync(session, popup, Entry("javascript", "error", popup.Id, "bare", null));
        await popup.CloseAsync(TestContext.Current.CancellationToken);
        await recording.DisposeAsync();
        await recording.DisposeAsync();

        List<JsonObject> events = ReadLines(path, "trace.trace");
        Assert.Equal([page.Id, popup.Id], events.Where(e => (string?)e["method"] == "page").Select(e => (string?)e["params"]!["pageId"]));
        Assert.Equal(page.Id, (string?)events.Single(e => (string?)e["method"] == "page" && (string?)e["params"]!["pageId"] == popup.Id)["params"]!["openerPageId"]);
        Assert.Equal(popup.Id, (string?)events.Single(e => (string?)e["method"] == "pageClosed")["params"]!["pageId"]);
        List<JsonObject> console = [.. events.Where(e => (string?)e["type"] == "console")];
        Assert.Equal(["warning:careful", "log:plain"], console.Select(e => $"{e["messageType"]}:{e["text"]}"));
        Assert.Equal(popup.Id, (string?)console[0]["pageId"]);
        Assert.Equal("""{"url":"https://example.com/app.js","lineNumber":4,"columnNumber":2}""", console[0]["location"]!.ToJsonString());
        Assert.Equal("""{"url":"","lineNumber":0,"columnNumber":0}""", console[1]["location"]!.ToJsonString());
        List<JsonObject> errors = [.. events.Where(e => (string?)e["method"] == "pageError")];
        Assert.Equal(popup.Id, (string?)errors[0]["pageId"]);
        JsonNode error = errors[0]["params"]!["error"]!["error"]!;
        Assert.Equal("boom", (string?)error["message"]);
        Assert.Equal("Error", (string?)error["name"]);
        Assert.Equal("Error: boom\n    at <anonymous> (https://example.com/app.js:10:1)\n    at load (https://example.com/main.js:2:6)", (string?)error["stack"]);
        Assert.Equal("""{"url":"https://example.com/app.js","line":9,"column":0}""", errors[0]["params"]!["location"]!.ToJsonString());
        Assert.Null(errors[1]["params"]!["error"]!["error"]!["stack"]);
    }

    [Fact]
    public async Task NetworkTrafficIsRecordedWithBodiesInTheTrace()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("network.getData", parameters => (string)parameters["request"]! == "request-1" ? Bytes("string", "<p>Hello</p>") : Bytes("base64", Convert.ToBase64String([0xFF, 0x00, 0x7F])));
        string path = Path.Combine(this.directory, "trace.zip");

        await using (await page.Browser.RecordTraceAsync(path, cancellationToken: TestContext.Current.CancellationToken))
        {
            await BeforeRequestSentAsync(session.RemoteEnd, "request-1", url: "https://example.com/page", userContext: page.Browser.Id, context: page.Id);
            await ResponseCompletedAsync(session.RemoteEnd, "request-1");
            await BeforeRequestSentAsync(session.RemoteEnd, "request-2", url: "https://example.com/gone", startMilliseconds: 1, userContext: page.Browser.Id, context: "closed-frame");
            await ResponseCompletedAsync(session.RemoteEnd, "request-2", mimeType: "application/octet-stream");
            await BeforeRequestSentAsync(session.RemoteEnd, "request-3", url: "https://example.com/worker", startMilliseconds: 2, userContext: page.Browser.Id, context: null);
            await FetchErrorAsync(session.RemoteEnd, "request-3", "net::ERR_FAILED");
            await FlushAsync(driver);
        }

        List<JsonObject> network = ReadLines(path, "trace.network");
        Assert.All(network, line => Assert.Equal("resource-snapshot", (string?)line["type"]));
        List<JsonNode> entries = [.. network.Select(line => line["snapshot"]!)];
        Assert.Equal(["https://example.com/page", "https://example.com/gone", "https://example.com/worker"], entries.Select(entry => (string?)entry["request"]!["url"]));
        Assert.Equal([page.Id, "closed-frame", null], entries.Select(entry => (string?)entry["pageref"]));
        Assert.Equal([page.Id, "closed-frame", null], entries.Select(entry => (string?)entry["_frameref"]));
        Assert.All(entries, entry => Assert.True((double?)entry["_monotonicTime"] > 0));
        string file = (string)entries[0]["response"]!["content"]!["_file"]!;
        Assert.Matches("^resources/[0-9a-f]{40}$", file);
        Assert.Null(entries[0]["response"]!["content"]!["text"]);
        Assert.Equal("<p>Hello</p>", Encoding.UTF8.GetString(ReadEntry(path, file)));
        Assert.Equal([0xFF, 0x00, 0x7F], ReadEntry(path, (string)entries[1]["response"]!["content"]!["_file"]!));
        Assert.Null(entries[2]["response"]!["content"]!["_file"]);
    }

    [Fact]
    public async Task OneTraceAtATimeIsRecordedAndStoppingAgainDoesNothing()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        Browser other = await page.Browser.Group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page otherPage = await other.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        string path = Path.Combine(this.directory, "trace.zip");

        TraceRecording recording = await other.RecordTraceAsync(path, cancellationToken: TestContext.Current.CancellationToken);
        InvalidOperationException twice = await Assert.ThrowsAsync<InvalidOperationException>(() => other.RecordTraceAsync(Path.Combine(this.directory, "second.zip"), cancellationToken: TestContext.Current.CancellationToken));
        await otherPage.Locate(new CssLocator("button")).CountAsync(TestContext.Current.CancellationToken);
        await recording.StopAsync(TestContext.Current.CancellationToken);
        File.Delete(path);
        await otherPage.Locate(new CssLocator("button")).CountAsync(TestContext.Current.CancellationToken);
        string again = await recording.StopAsync(TestContext.Current.CancellationToken);
        await using TraceRecording next = await other.RecordTraceAsync(Path.Combine(this.directory, "next.zip"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("The browser is already recording a trace.", twice.Message);
        Assert.Equal(recording.Path, again);
        Assert.False(File.Exists(path));
        Assert.False(File.Exists(Path.Combine(this.directory, "second.zip")));
    }

    [Fact]
    public async Task EachBrowserRecordsItsOwnActionsUnderCallIdsNoOtherTraceUses()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        Browser other = await page.Browser.Group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page otherPage = await other.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        string path = Path.Combine(this.directory, "trace.zip");

        string defaultPath = Path.Combine(this.directory, "default.zip");

        await using (await other.RecordTraceAsync(path, cancellationToken: TestContext.Current.CancellationToken))
        {
            await using (await page.Browser.RecordTraceAsync(defaultPath, cancellationToken: TestContext.Current.CancellationToken))
            {
                await page.Locate(new CssLocator("button")).CountAsync(TestContext.Current.CancellationToken);
                await otherPage.Locate(new CssLocator("a")).CountAsync(TestContext.Current.CancellationToken);
            }
        }

        List<JsonObject> events = ReadLines(path, "trace.trace");
        Assert.Equal(["css \"a\""], events.Where(e => (string?)e["type"] == "before").Select(e => (string?)e["params"]!["locator"]));
        Assert.Equal([otherPage.Id], events.Where(e => (string?)e["method"] == "page").Select(e => (string?)e["params"]!["pageId"]));
        List<JsonObject> defaultEvents = ReadLines(defaultPath, "trace.trace");
        Assert.Equal(["css \"button\""], defaultEvents.Where(e => (string?)e["type"] == "before").Select(e => (string?)e["params"]!["locator"]));

        // Traces of different browsers can be merged into one, so they never share a call ID.
        Assert.Empty(events.Where(e => (string?)e["type"] == "before").Select(e => (string?)e["callId"]).Intersect(defaultEvents.Where(e => (string?)e["type"] == "before").Select(e => (string?)e["callId"])));
    }

    [Fact]
    public async Task PageCreatedAsTheRecordingStopsIsLeftOut()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        string path = Path.Combine(this.directory, "trace.zip");
        TraceRecording? recording = null;
        Task<string>? stopping = null;

        // This observer is added first, so it stops the recording before the recording's own observer runs.
        page.Browser.OnPageCreated.AddObserver(_ => stopping ??= recording!.StopAsync());
        recording = await page.Browser.RecordTraceAsync(path, cancellationToken: TestContext.Current.CancellationToken);
        Page late = await page.Browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await stopping!;

        Assert.DoesNotContain(ReadLines(path, "trace.trace"), e => (string?)e["method"] == "page" && (string?)e["params"]!["pageId"] == late.Id);
    }

    [Fact]
    public async Task FailureToStartLeavesTheBrowserFreeToRecord()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.FailWith("network.addDataCollector", "unknown error", "No collectors.");

        await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => page.Browser.RecordTraceAsync(Path.Combine(this.directory, "failed.zip"), cancellationToken: TestContext.Current.CancellationToken));
        session.RemoteEnd.AnswerWith("network.addDataCollector", new JsonObject() { ["collector"] = "collector-1" });
        await using TraceRecording recording = await page.Browser.RecordTraceAsync(Path.Combine(this.directory, "trace.zip"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(File.Exists(Path.Combine(this.directory, "failed.zip")));
    }

    private static Task<int> CountFromHelper(ElementLocator locator) => locator.CountAsync(TestContext.Current.CancellationToken);

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new DramaturgeOptions() { NavigationTimeout = TimeSpan.FromMilliseconds(200), PollInterval = TimeSpan.FromMilliseconds(10) }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page);
    }

    private static void AnswerReadiness(FakeSession session)
    {
        JsonObject offset = new()
        {
            ["type"] = "object",
            ["value"] = new JsonArray(new JsonArray("x", new JsonObject() { ["type"] = "number", ["value"] = 1 }), new JsonArray("y", new JsonObject() { ["type"] = "number", ["value"] = 1 })),
        };
        JsonObject ready = new()
        {
            ["type"] = "object",
            ["value"] = new JsonArray(new JsonArray("status", new JsonObject() { ["type"] = "string", ["value"] = "ready" }), new JsonArray("interactionOffset", offset)),
        };
        JsonObject hidden = new()
        {
            ["type"] = "object",
            ["value"] = new JsonArray(new JsonArray("status", new JsonObject() { ["type"] = "string", ["value"] = "notready" }), new JsonArray("reason", new JsonObject() { ["type"] = "string", ["value"] = "hidden" })),
        };
        int[] checks = [0];
        session.RemoteEnd.AnswerWith("script.callFunction", parameters => ProtocolJson.Success(!((string)parameters["functionDeclaration"]!).Contains("isInteractionReady") ? new JsonObject() { ["type"] = "undefined" }
            : Interlocked.Increment(ref checks[0]) <= 2 ? (JsonObject)hidden.DeepClone() : (JsonObject)ready.DeepClone()));
    }

    // The recording's observers were added before the test's, so they have run once the test's has.
    private static async Task RaiseAndWaitAsync(FakeSession session, Page page, JsonObject entry)
    {
        TaskCompletionSource received = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using IDisposable console = page.OnConsoleMessage.AddObserver(_ => received.TrySetResult());
        using IDisposable error = page.OnPageError.AddObserver(_ => received.TrySetResult());
        await session.RemoteEnd.RaiseEventAsync("log.entryAdded", entry);
        await received.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);
    }

    private static JsonObject Entry(string type, string level, string contextId, string text, JsonObject? stack)
    {
        JsonObject entry = new() { ["type"] = type, ["level"] = level, ["source"] = new JsonObject() { ["realm"] = "realm-1", ["context"] = contextId }, ["text"] = text, ["timestamp"] = 1790000000000 };
        if (stack is not null)
        {
            entry["stackTrace"] = stack;
        }

        return entry;
    }

    private static JsonObject ConsoleEntry(string contextId, string method, string text, JsonObject? stack)
    {
        JsonObject entry = Entry("console", "info", contextId, text, stack);
        entry["method"] = method;
        entry["args"] = new JsonArray(new JsonObject() { ["type"] = "string", ["value"] = text });
        return entry;
    }

    private static JsonObject Stack(params (string Function, string Url, int Line, int Column)[] frames)
    {
        return new JsonObject()
        {
            ["callFrames"] = new JsonArray([.. frames.Select(frame => (JsonNode?)new JsonObject() { ["functionName"] = frame.Function, ["url"] = frame.Url, ["lineNumber"] = frame.Line, ["columnNumber"] = frame.Column })]),
        };
    }

    private static List<JsonObject> ReadLines(string path, string entry)
    {
        return [.. Encoding.UTF8.GetString(ReadEntry(path, entry)).Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => JsonNode.Parse(line)!.AsObject())];
    }

    private static byte[] ReadEntry(string path, string entry)
    {
        using ZipArchive zip = ZipFile.OpenRead(path);
        using MemoryStream content = new();
        using (Stream stream = zip.GetEntry(entry)!.Open())
        {
            stream.CopyTo(content);
        }

        return content.ToArray();
    }

    private static class Helpers
    {
        public static Task<int> CountAsync(ElementLocator locator) => locator.CountAsync(TestContext.Current.CancellationToken);
    }
}
