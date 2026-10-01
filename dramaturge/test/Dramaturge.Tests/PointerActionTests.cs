// <copyright file="PointerActionTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;

public class PointerActionTests
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    [Fact]
    public async Task ClickWaitsForReadinessScrollingAsNeededThenClicksAtTheChosenPoint()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        AnswerScripts(session, NotReady("hidden"), Readiness("needsscroll"), Ready(2.5, -1));

        await DriveAsync(time, page.Locate(new CssLocator("button")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken));

        IReadOnlyList<JsonObject> checks = ScriptCalls(session, "isInteractionReady");
        Assert.Equal(3, checks.Count);
        JsonArray checkArguments = checks[0]["params"]!["arguments"]!.AsArray();
        Assert.Equal("button-1", (string?)checkArguments[0]!["sharedId"]);
        Assert.Equal("click", (string?)checkArguments[1]!["value"]);
        Assert.Equal(0, (double?)ObjectEntry(checkArguments[2]!, "x"));
        JsonObject scroll = Assert.Single(ScriptCalls(session, "scrollIntoView"));
        Assert.False((bool?)scroll["params"]!["arguments"]![1]!["value"]);
        JsonArray pointerActions = SourceActions(session, "pointer");
        Assert.Equal("pointerMove", (string?)pointerActions[0]!["type"]);
        Assert.Equal(2.5, (double?)pointerActions[0]!["x"]);
        Assert.Equal(-1, (double?)pointerActions[0]!["y"]);
        Assert.Equal("button-1", (string?)pointerActions[0]!["origin"]!["element"]!["sharedId"]);
        Assert.Equal(["pointerDown:0", "pointerUp:0"], pointerActions.Skip(1).Select(action => $"{action!["type"]}:{action["button"]}"));
        Assert.Equal(page.Id, (string?)Assert.Single(session.RemoteEnd.CommandsFor("input.performActions"))["params"]!["context"]);
    }

    [Fact]
    public async Task ClickOptionsChooseTheButtonCountPointAndModifiers()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        AnswerScripts(session, Ready(5, 6));
        ClickOptions options = new() { Button = PointerButton.Right, ClickCount = 3, Offset = new PointerOffset(5, 6), Modifiers = KeyModifiers.Shift | KeyModifiers.Control };

        await page.Locate(new CssLocator("button")).ClickAsync(options, TestContext.Current.CancellationToken);

        JsonArray checkArguments = Assert.Single(ScriptCalls(session, "isInteractionReady"))["params"]!["arguments"]!.AsArray();
        Assert.Equal("doubleclick", (string?)checkArguments[1]!["value"]);
        Assert.Equal(5, (double?)ObjectEntry(checkArguments[2]!, "x"));
        Assert.Equal(6, (double?)ObjectEntry(checkArguments[2]!, "y"));
        Assert.Equal([$"keyDown:{Keys.Control}", $"keyDown:{Keys.Shift}", $"keyUp:{Keys.Shift}", $"keyUp:{Keys.Control}"], SourceActions(session, "key").Where(action => (string?)action!["type"] != "pause").Select(action => $"{action!["type"]}:{action["value"]}"));
        Assert.Equal(6, SourceActions(session, "pointer").Count(action => (string?)action!["type"] is "pointerDown" or "pointerUp" && (int?)action["button"] == 2));
    }

    [Fact]
    public async Task EveryModifierKeyCanBeHeld()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        AnswerScripts(session, Ready(0, 0));

        await page.Locate(new CssLocator("button")).HoverAsync(new PointerActionOptions() { Modifiers = KeyModifiers.Alt | KeyModifiers.Meta }, TestContext.Current.CancellationToken);

        Assert.Equal([$"keyDown:{Keys.Alt}", $"keyDown:{Keys.Meta}", $"keyUp:{Keys.Meta}", $"keyUp:{Keys.Alt}"], SourceActions(session, "key").Where(action => (string?)action!["type"] != "pause").Select(action => $"{action!["type"]}:{action["value"]}"));
    }

    [Fact]
    public async Task DoubleClickKeepsTheOptionsButClicksTwice()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        AnswerScripts(session, Ready(0, 0));

        await page.Locate(new CssLocator("button")).DblClickAsync(new ClickOptions() { Button = PointerButton.Middle, ClickCount = 5 }, TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("button")).DblClickAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("doubleclick", (string?)ScriptCalls(session, "isInteractionReady")[0]["params"]!["arguments"]![1]!["value"]);
        Assert.Equal(["pointerDown:1", "pointerUp:1", "pointerDown:1", "pointerUp:1"], SourceActions(session, "pointer", 0).Where(action => (string?)action!["type"] is "pointerDown" or "pointerUp").Select(action => $"{action!["type"]}:{action["button"]}"));
        Assert.Equal(4, SourceActions(session, "pointer", 1).Count(action => (string?)action!["type"] is "pointerDown" or "pointerUp" && (int?)action["button"] == 0));
    }

    [Fact]
    public async Task HoverOnlyMovesThePointer()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("link-1"));
        AnswerScripts(session, Ready(1, 1));

        await page.Locate(new CssLocator("a")).HoverAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("hover", (string?)Assert.Single(ScriptCalls(session, "isInteractionReady"))["params"]!["arguments"]![1]!["value"]);
        Assert.Equal(["pointerMove"], SourceActions(session, "pointer").Select(action => (string?)action!["type"]));
    }

    [Fact]
    public async Task ForcedClickSkipsTheChecksButScrollsTheElementIntoView()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        AnswerScripts(session);

        await page.Locate(new CssLocator("button")).ClickAsync(new ClickOptions() { Force = true, Offset = new PointerOffset(3, 4) }, TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("button")).HoverAsync(new PointerActionOptions() { Force = true }, TestContext.Current.CancellationToken);

        Assert.Empty(ScriptCalls(session, "isInteractionReady"));
        Assert.All(ScriptCalls(session, "scrollIntoView"), call => Assert.True((bool?)call["params"]!["arguments"]![1]!["value"]));
        Assert.Equal(3, (double?)SourceActions(session, "pointer", 0)[0]!["x"]);
        Assert.Equal(4, (double?)SourceActions(session, "pointer", 0)[0]!["y"]);
        Assert.Equal(0, (double?)SourceActions(session, "pointer", 1)[0]!["x"]);
    }

    [Theory]
    [InlineData("hidden", "the element was not visible")]
    [InlineData("disabled", "the element was disabled")]
    [InlineData("readOnly", "the element was read-only")]
    [InlineData("stable", "the element was still moving")]
    [InlineData("unviewable", "the element could not be scrolled into view")]
    [InlineData("notconnected", "the element was removed from the document")]
    [InlineData("obscured by <div class=\"overlay\"></div>", "the element was obscured by <div class=\"overlay\"></div>")]
    public async Task ActionThatTimesOutSaysWhichCheckFailed(string reason, string observed)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        AnswerScripts(session, NotReady(reason));

        Task click = page.Locate(new CssLocator("button")).ClickAsync(new ClickOptions() { Timeout = TimeSpan.FromSeconds(1) }, TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, click));

        Assert.Equal($"Timed out after 1 seconds waiting for css \"button\" to be ready to be clicked; {observed}.", exception.Message);
        Assert.Empty(session.RemoteEnd.CommandsFor("input.performActions"));
    }

    [Fact]
    public async Task ActionWaitsForAMatchAndRejectsSeveral()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(0));

        Task hover = page.Locate(new CssLocator("a")).HoverAsync(new PointerActionOptions() { Timeout = TimeSpan.FromSeconds(1) }, TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException timeout = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, hover));
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(2));

        Assert.EndsWith("to be ready to be hovered; no element matched.", timeout.Message);
        await Assert.ThrowsAsync<AmbiguousElementException>(() => page.Locate(new CssLocator("a")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CommandThatFailsWhileTheFrameNavigatesIsRetried()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        int[] lookups = [0];
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", _ => Interlocked.Increment(ref lookups[0]) == 1
            ? FakeResponse.Failure("unknown error", "Inspected target navigated or closed", NavigationStarted(page.Id))
            : new FakeResponse(ProtocolJson.Nodes("button-1")));
        AnswerScripts(session, Ready(0, 0));

        await DriveAsync(time, page.Locate(new CssLocator("button")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(2, session.RemoteEnd.CommandsFor("browsingContext.locateNodes").Count);
        Assert.Single(session.RemoteEnd.CommandsFor("input.performActions"));
    }

    [Fact]
    public async Task CommandThatFailsWithoutANavigationFailsTheAction()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext childFrame = await session.CreateFrameAsync(page.Id);
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", _ => FakeResponse.Failure("unknown error", "Cannot find context with specified id", NavigationStarted(childFrame.Id)));

        WebDriverBiDiCommandException exception = await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => page.Locate(new CssLocator("button")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCode.UnknownError, exception.ErrorCode);
        Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.locateNodes"));
    }

    [Fact]
    public async Task ActionThatKeepsFailingWhileTheFrameNavigatesTimesOut()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", _ => FakeResponse.Failure("unknown error", "Inspected target navigated or closed", NavigationStarted(page.Id)));

        Task click = page.Locate(new CssLocator("button")).ClickAsync(new ClickOptions() { Timeout = TimeSpan.FromSeconds(1) }, TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, click));

        Assert.Equal("Timed out after 1 seconds waiting for css \"button\" to be ready to be clicked; the frame navigated while the element was checked.", exception.Message);
    }

    [Fact]
    public void ClickCountMustBePositive()
    {
        Assert.Equal("ClickCount", Assert.Throws<ArgumentOutOfRangeException>(() => new ClickOptions() { ClickCount = 0 }).ParamName);
        Assert.Equal(1, new ClickOptions().ClickCount);
        Assert.Equal(PointerButton.Left, new ClickOptions().Button);
    }

    [Fact]
    public async Task ScrollIntoViewWaitsForAVisibleElementAndScrollsItOnce()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("section-1"));
        AnswerScripts(session, States("failure", "missingState", "hidden"), States("failure", "missingState", "notinview"), States("success", null, null));

        await DriveAsync(time, page.Locate(new CssLocator("section")).ScrollIntoViewIfNeededAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(3, ScriptCalls(session, "queryElementStates").Count);
        Assert.Contains("['stable', 'visible', 'inview']", (string?)ScriptCalls(session, "queryElementStates")[0]["params"]!["functionDeclaration"]);
        Assert.Single(ScriptCalls(session, "scrollIntoView"));
    }

    [Theory]
    [InlineData(0, null, null, "no element matched")]
    [InlineData(1, "failure", "unviewable", "the element could not be scrolled into view")]
    [InlineData(1, "error", "notconnected", "the element was removed from the document")]
    public async Task ScrollIntoViewThatTimesOutSaysWhy(int matches, string? status, string? detail, string observed)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(matches));
        AnswerScripts(session, States(status ?? "success", status == "error" ? "message" : "missingState", detail));

        Task scroll = page.Locate(new CssLocator("section")).ScrollIntoViewIfNeededAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, scroll));

        Assert.Equal($"Timed out after 1 seconds waiting for css \"section\" to be in view; {observed}.", exception.Message);
    }

    [Fact]
    public async Task ScrollIntoViewRejectsSeveralMatches()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(2));

        await Assert.ThrowsAsync<AmbiguousElementException>(() => page.Locate(new CssLocator("section")).ScrollIntoViewIfNeededAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page, FakeTimeProvider Time)> OpenPageAsync()
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new DramaturgeOptions() { PollInterval = PollInterval, TimeProvider = time }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page, time);
    }

    // Answers readiness and state queries with the next result in turn (repeating the last), and every other
    // script, such as a scroll, with undefined.
    private static void AnswerScripts(FakeSession session, params JsonObject[] results)
    {
        int[] calls = [0];
        session.RemoteEnd.AnswerWith("script.callFunction", parameters =>
        {
            string function = (string)parameters["functionDeclaration"]!;
            return (function.Contains("isInteractionReady") || function.Contains("queryElementStates")) && results.Length > 0
                ? ProtocolJson.Success((JsonObject)results[Math.Min(Interlocked.Increment(ref calls[0]), results.Length) - 1].DeepClone())
                : ProtocolJson.Success(new JsonObject() { ["type"] = "undefined" });
        });
    }

    private static JsonObject Readiness(string status, params (string Name, JsonNode Value)[] extra)
    {
        JsonArray entries = [new JsonArray("status", new JsonObject() { ["type"] = "string", ["value"] = status })];
        foreach ((string name, JsonNode value) in extra)
        {
            entries.Add(new JsonArray(name, value));
        }

        return new JsonObject() { ["type"] = "object", ["value"] = entries };
    }

    private static (string Method, JsonObject Parameters) NavigationStarted(string contextId)
    {
        return ("browsingContext.navigationStarted", new JsonObject()
        {
            ["context"] = contextId,
            ["navigation"] = "navigation-1",
            ["timestamp"] = 1790000000000,
            ["url"] = "https://example.com/next",
        });
    }

    private static JsonObject Ready(double x, double y)
    {
        JsonObject offset = new()
        {
            ["type"] = "object",
            ["value"] = new JsonArray(new JsonArray("x", new JsonObject() { ["type"] = "number", ["value"] = x }), new JsonArray("y", new JsonObject() { ["type"] = "number", ["value"] = y })),
        };
        return Readiness("ready", ("interactionOffset", offset));
    }

    private static JsonObject NotReady(string reason)
    {
        return Readiness("notready", ("reason", new JsonObject() { ["type"] = "string", ["value"] = reason }));
    }

    private static JsonObject States(string status, string? detailName, string? detail)
    {
        return detailName is null || detail is null
            ? Readiness(status)
            : Readiness(status, (detailName, new JsonObject() { ["type"] = "string", ["value"] = detail }));
    }

    private static IReadOnlyList<JsonObject> ScriptCalls(FakeSession session, string functionFragment)
    {
        return [.. session.RemoteEnd.CommandsFor("script.callFunction").Where(command => ((string)command["params"]!["functionDeclaration"]!).Contains(functionFragment))];
    }

    private static JsonArray SourceActions(FakeSession session, string sourceType, int command = 0)
    {
        JsonArray sources = session.RemoteEnd.CommandsFor("input.performActions")[command]["params"]!["actions"]!.AsArray();
        return sources.Single(source => (string?)source!["type"] == sourceType)!["actions"]!.AsArray();
    }

    private static JsonNode? ObjectEntry(JsonNode serializedObject, string name)
    {
        return serializedObject["value"]!.AsArray().Single(entry => (string?)entry![0] == name)![1]!["value"];
    }

    private static async Task DriveAsync(FakeTimeProvider time, Task operation)
    {
        while (!operation.IsCompleted)
        {
            time.Advance(PollInterval);
            await Task.WhenAny(operation, Task.Delay(5, TestContext.Current.CancellationToken));
        }

        await operation;
    }
}
