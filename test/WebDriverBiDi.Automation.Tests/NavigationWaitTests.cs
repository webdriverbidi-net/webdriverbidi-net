// <copyright file="NavigationWaitTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.BrowsingContext;

public class NavigationWaitTests
{
    private const string PageUrl = "https://example.com/start";

    [Fact]
    public async Task LoadStateFollowsTheEventsForANewDocument()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerReadyState(session, "loading");

        Task parsed = page.WaitForLoadStateAsync(ReadinessState.Interactive, cancellationToken: TestContext.Current.CancellationToken);
        Task loaded = page.WaitForLoadStateAsync(cancellationToken: TestContext.Current.CancellationToken);
        await WaitForScriptCallsAsync(driver, session, 2);
        Assert.False(parsed.IsCompleted);
        await session.RaiseNavigationEventAsync("browsingContext.domContentLoaded", page.Id, PageUrl);
        await parsed;
        Assert.False(loaded.IsCompleted);
        await session.RaiseNavigationEventAsync("browsingContext.load", page.Id, PageUrl);
        await loaded;

        JsonObject call = session.RemoteEnd.CommandsFor("script.callFunction")[0]["params"]!.AsObject();
        Assert.Equal("() => document.readyState", (string?)call["functionDeclaration"]);
        Assert.Equal(page.Id, (string?)call["target"]!["context"]);
        Assert.Equal(page.Group().Options.SandboxName, (string?)call["target"]!["sandbox"]);
    }

    [Theory]
    [InlineData("interactive", ReadinessState.Interactive)]
    [InlineData("complete", ReadinessState.Complete)]
    public async Task AnUnreportedDocumentIsAskedForItsStateOnce(string readyState, ReadinessState awaited)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerReadyState(session, readyState);

        await page.WaitForLoadStateAsync(awaited, cancellationToken: TestContext.Current.CancellationToken);
        await page.WaitForLoadStateAsync(ReadinessState.Interactive, cancellationToken: TestContext.Current.CancellationToken);
        await page.WaitForLoadStateAsync(ReadinessState.None, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task AnAnswerOlderThanAnEventIsDiscarded()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", _ => new FakeResponse(ReadyState("loading"), [("browsingContext.load", NavigationEvent(page.Id))]));

        await page.WaitForLoadStateAsync(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AFailedReadAfterAnEventIsIgnoredButOtherwisePropagates()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", _ => FakeResponse.Failure("unknown error", "Inspected target navigated or closed", ("browsingContext.load", NavigationEvent(page.Id))));

        await page.WaitForLoadStateAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(PageUrl, cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("script.callFunction", _ => FakeResponse.Failure("unknown error", "Something else"));

        await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => page.WaitForLoadStateAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LoadStateThatNeverComesTimesOutSayingWhatWasSeen()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", _ => new FakeResponse(ReadyState("loading"), [("browsingContext.historyUpdated", HistoryEvent(page.Id, PageUrl))]));

        Task unknown = page.WaitForLoadStateAsync(timeout: TimeSpan.FromSeconds(1), cancellationToken: TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException neverReported = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, unknown));
        AnswerReadyState(session, "loading");
        await page.NavigateAsync(PageUrl, cancellationToken: TestContext.Current.CancellationToken);
        Task loading = page.WaitForLoadStateAsync(timeout: TimeSpan.FromSeconds(1), cancellationToken: TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException stillLoading = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, loading));

        Assert.Equal("Timed out after 1 seconds waiting for the frame's document to be complete; it was unknown.", neverReported.Message);
        Assert.Equal("Timed out after 1 seconds waiting for the frame's document to be complete; it was loading.", stillLoading.Message);
    }

    [Fact]
    public async Task UrlWaitsReturnAtOnceForAMatchAndOtherwiseWaitForOne()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerReadyState(session, "complete");

        string exact = await page.WaitForUrlAsync(PageUrl, cancellationToken: TestContext.Current.CancellationToken);
        Task<string> byPattern = page.WaitForUrlAsync(new Regex("#done$"), ReadinessState.None, cancellationToken: TestContext.Current.CancellationToken);
        Task<string> byCondition = page.WaitForUrlAsync(url => url.EndsWith("#done", StringComparison.Ordinal), cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(byPattern.IsCompleted);
        await session.RaiseNavigationEventAsync("browsingContext.fragmentNavigated", page.Id, PageUrl + "#done");

        Assert.Equal(PageUrl, exact);
        Assert.Equal(PageUrl + "#done", await byPattern);
        Assert.Equal(PageUrl + "#done", await byCondition);
    }

    [Fact]
    public async Task UrlThatNeverMatchesTimesOutWithTheUrlSeen()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Task<string> wait = page.MainFrame.WaitForUrlAsync("https://example.com/other", timeout: TimeSpan.FromSeconds(1), cancellationToken: TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, wait));

        Assert.Equal($"Timed out after 1 seconds waiting for the URL https://example.com/other; the frame's URL was {PageUrl}.", exception.Message);
        Assert.Empty(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task RunAndWaitForNavigationWaitsForTheNavigationTheActionCauses()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerReadyState(session, "complete");

        string url = await page.RunAndWaitForNavigationAsync(() => session.RaiseHistoryUpdatedAsync(page.Id, PageUrl + "/pushed"), cancellationToken: TestContext.Current.CancellationToken);
        Task<string> none = page.RunAndWaitForNavigationAsync(() => Task.CompletedTask, ReadinessState.None, TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, none));

        Assert.Equal(PageUrl + "/pushed", url);
        Assert.Equal("Timed out after 1 seconds waiting for a navigation of the frame; no navigation was reported.", exception.Message);
    }

    [Fact]
    public async Task HistoryTraversalWaitsForTheNavigationItCauses()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        int[] traversals = [0];
        session.RemoteEnd.AnswerWith("browsingContext.traverseHistory", parameters => new FakeResponse(new JsonObject(), [("browsingContext.historyUpdated", HistoryEvent((string)parameters["context"]!, $"{PageUrl}/{Interlocked.Increment(ref traversals[0])}"))]));

        string back = await page.GoBackAsync(ReadinessState.None, cancellationToken: TestContext.Current.CancellationToken);
        string forward = await page.GoForwardAsync(ReadinessState.None, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(PageUrl + "/1", back);
        Assert.Equal(PageUrl + "/2", forward);
        Assert.Equal(["-1", "1"], session.RemoteEnd.CommandsFor("browsingContext.traverseHistory").Select(command => command["params"]!["delta"]!.ToString()));
    }

    [Fact]
    public async Task WaitEndsWhenTheFrameIsDetached()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Task<string> wait = page.WaitForUrlAsync("https://example.com/never", cancellationToken: TestContext.Current.CancellationToken);
        await page.CloseAsync(TestContext.Current.CancellationToken);
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => wait);

        Assert.Equal("The frame was detached while waiting for the URL https://example.com/never.", exception.Message);
    }

    [Fact]
    public async Task WaitIsCancelledByItsToken()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        using CancellationTokenSource cancellation = new();

        Task<string> wait = page.WaitForUrlAsync("https://example.com/never", cancellationToken: cancellation.Token);
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => wait);
    }

    [Fact]
    public async Task FramesNavigateReloadAndTrackTheirOwnState()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext child = await session.CreateFrameAsync(page.Id);
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        Frame frame = page.Frames[1];

        string navigated = await frame.NavigateAsync("https://example.com/child", ReadinessState.Interactive, cancellationToken: TestContext.Current.CancellationToken);
        string reloaded = await frame.ReloadAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://example.com/child", navigated);
        Assert.Equal("https://example.com/child", reloaded);
        Assert.Equal("https://example.com/child", frame.Url);
        Assert.Equal(PageUrl, page.Url);
        JsonObject navigate = session.RemoteEnd.CommandsFor("browsingContext.navigate").Last()["params"]!.AsObject();
        Assert.Equal(child.Id, (string?)navigate["context"]);
        Assert.Equal("interactive", (string?)navigate["wait"]);
        Assert.Equal(child.Id, (string?)session.RemoteEnd.CommandsFor("browsingContext.reload").Single()["params"]!["context"]);
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page, FakeTimeProvider Time)> OpenPageAsync()
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new AutomationOptions() { TimeProvider = time }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(PageUrl, cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page, time);
    }

    private static void AnswerReadyState(FakeSession session, string readyState)
    {
        session.RemoteEnd.AnswerWith("script.callFunction", ReadyState(readyState));
    }

    private static JsonObject ReadyState(string readyState)
    {
        return ProtocolJson.Success(new JsonObject() { ["type"] = "string", ["value"] = readyState });
    }

    private static JsonObject NavigationEvent(string contextId)
    {
        return new JsonObject() { ["context"] = contextId, ["navigation"] = "navigation-late", ["timestamp"] = 1790000000000, ["url"] = PageUrl };
    }

    private static JsonObject HistoryEvent(string contextId, string url)
    {
        return new JsonObject() { ["context"] = contextId, ["timestamp"] = 1790000000000, ["url"] = url };
    }

    // Waits for the waits under test to have asked the document for its state.
    private static async Task WaitForScriptCallsAsync(BiDiDriver driver, FakeSession session, int count)
    {
        while (session.RemoteEnd.CommandsFor("script.callFunction").Count < count)
        {
            await Task.Delay(5, TestContext.Current.CancellationToken);
        }

        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
    }

    private static async Task DriveAsync(FakeTimeProvider time, Task operation)
    {
        while (!operation.IsCompleted)
        {
            time.Advance(TimeSpan.FromMilliseconds(100));
            await Task.WhenAny(operation, Task.Delay(5, TestContext.Current.CancellationToken));
        }

        await operation;
    }
}
