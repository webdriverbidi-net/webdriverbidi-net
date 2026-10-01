// <copyright file="PointerExtrasTests.cs" company="WebDriverBiDi.NET Committers">
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
using WebDriverBiDi.Script;

public class PointerExtrasTests
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    [Fact]
    public async Task TapUsesATouchPointerAtTheChosenPoint()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        AnswerScripts(session, _ => Ready(3, 4));

        await page.Locate(new CssLocator("button")).TapAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("click", (string?)Assert.Single(ScriptCalls(session, "isInteractionReady"))["params"]!["arguments"]![1]!["value"]);
        JsonObject pointer = PointerSource(session);
        Assert.Equal("touch", (string?)pointer["parameters"]!["pointerType"]);
        JsonArray actions = pointer["actions"]!.AsArray();
        Assert.Equal(["pointerMove", "pointerDown", "pointerUp"], actions.Select(action => (string?)action!["type"]));
        Assert.Equal(3, (double?)actions[0]!["x"]);
        Assert.Equal(4, (double?)actions[0]!["y"]);
        Assert.Equal("button-1", (string?)actions[0]!["origin"]!["element"]!["sharedId"]);
    }

    [Fact]
    public async Task TapThatTimesOutSaysSo()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("button-1"));
        AnswerScripts(session, _ => NotReady("hidden"));

        Task tap = page.Locate(new CssLocator("button")).TapAsync(new PointerActionOptions() { Timeout = TimeSpan.FromSeconds(1) }, TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, tap));

        Assert.Equal("Timed out after 1 seconds waiting for css \"button\" to be ready to be tapped; the element was not visible.", exception.Message);
    }

    [Fact]
    public async Task DragToHoldsTheElementMovesToTheTargetAndReleases()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerLookups(session);
        AnswerScripts(session, arguments => (string?)arguments[0]!["sharedId"] == "source-1" ? Ready(1, 2) : Ready(5, 6));
        DragOptions options = new() { Offset = new PointerOffset(1, 2), TargetOffset = new PointerOffset(5, 6), Modifiers = KeyModifiers.Alt };

        await page.Locate(new CssLocator("#source")).DragToAsync(page.Locate(new CssLocator("#target")), options, TestContext.Current.CancellationToken);

        IReadOnlyList<JsonObject> checks = ScriptCalls(session, "isInteractionReady");
        Assert.Equal(["source-1:drag:1", "target-1:drop:5"], checks.Select(check => $"{check["params"]!["arguments"]![0]!["sharedId"]}:{check["params"]!["arguments"]![1]!["value"]}:{check["params"]!["arguments"]![2]!["value"]![0]![1]!["value"]}"));
        JsonObject pointer = PointerSource(session);
        Assert.Equal("mouse", (string?)pointer["parameters"]!["pointerType"]);
        JsonArray actions = pointer["actions"]!.AsArray();
        Assert.Equal(
            ["pointerMove:source-1:1:2", "pointerDown", "pointerMove:target-1:5:6", "pointerUp"],
            actions.Where(action => (string?)action!["type"] != "pause").Select(action => (string?)action!["type"] == "pointerMove" ? $"pointerMove:{action!["origin"]!["element"]!["sharedId"]}:{(double?)action["x"]}:{(double?)action["y"]}" : (string?)action!["type"]));
        JsonArray keys = Sources(session).Single(source => (string?)source!["type"] == "key")!["actions"]!.AsArray();
        Assert.Equal([$"keyDown:{Keys.Alt}", $"keyUp:{Keys.Alt}"], keys.Where(action => (string?)action!["type"] != "pause").Select(action => $"{action!["type"]}:{action["value"]}"));
    }

    [Fact]
    public async Task DragWithoutOptionsHoldsAndDropsAtTheChosenPoints()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerLookups(session);
        AnswerScripts(session, arguments => (string?)arguments[0]!["sharedId"] == "source-1" ? Ready(-2, 0) : Ready(0, 3));

        await page.Locate(new CssLocator("#source")).DragToAsync(page.Locate(new CssLocator("#target")), cancellationToken: TestContext.Current.CancellationToken);

        JsonArray actions = PointerSource(session)["actions"]!.AsArray();
        Assert.Equal(-2, (double?)actions[0]!["x"]);
        Assert.Equal(3, (double?)actions[2]!["y"]);
        Assert.DoesNotContain(Sources(session), source => (string?)source!["type"] == "key");
    }

    [Fact]
    public async Task ForcedDragSkipsBothChecks()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerLookups(session);
        AnswerScripts(session, _ => NotReady("hidden"));

        await page.Locate(new CssLocator("#source")).DragToAsync(page.Locate(new CssLocator("#target")), new DragOptions() { Force = true, TargetOffset = new PointerOffset(7, 8) }, TestContext.Current.CancellationToken);

        Assert.Empty(ScriptCalls(session, "isInteractionReady"));
        JsonArray actions = PointerSource(session)["actions"]!.AsArray();
        Assert.Equal(7, (double?)actions[2]!["x"]);
        Assert.Equal(8, (double?)actions[2]!["y"]);
    }

    [Theory]
    [InlineData("hidden", "css \"#source\" to be ready to be dragged; the element was not visible")]
    [InlineData("none", "css \"#target\" to be ready to be dropped on; no element matched")]
    public async Task DragThatTimesOutNamesTheElementItWaitedFor(string problem, string expected)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerLookups(session, missingTarget: problem == "none");
        AnswerScripts(session, _ => problem == "hidden" ? NotReady("hidden") : Ready(0, 0));

        Task drag = page.Locate(new CssLocator("#source")).DragToAsync(page.Locate(new CssLocator("#target")), new DragOptions() { Timeout = TimeSpan.FromSeconds(1) }, TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, drag));

        Assert.Equal($"Timed out after 1 seconds waiting for {expected}.", exception.Message);
        Assert.Empty(session.RemoteEnd.CommandsFor("input.performActions"));
    }

    [Fact]
    public async Task DragToATargetInAnotherFrameIsRejected()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        await session.CreateFrameAsync(page.Id);
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => page.Locate(new CssLocator("#source")).DragToAsync(page.Frames[1].Locate(new CssLocator("#target")), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("target", exception.ParamName);
        Assert.Empty(session.RemoteEnd.CommandsFor("browsingContext.locateNodes"));
    }

    [Fact]
    public async Task DispatchEventSendsTheTypeAndInitAndReportsWhetherItWasCanceled()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("div-1"));
        bool[] results = [false, true];
        int[] calls = [0];
        session.RemoteEnd.AnswerWith("script.callFunction", _ => ProtocolJson.Boolean(results[Interlocked.Increment(ref calls[0]) - 1]));

        bool canceled = await page.Locate(new CssLocator("div")).DispatchEventAsync("click", new Dictionary<string, LocalValue>() { ["clientX"] = LocalValue.Number(12) }, cancellationToken: TestContext.Current.CancellationToken);
        bool plain = await page.Locate(new CssLocator("div")).DispatchEventAsync("my-event", cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(canceled);
        Assert.True(plain);
        IReadOnlyList<JsonObject> dispatches = ScriptCalls(session, "actions.dispatchEvent(element, type, init)");
        Assert.Equal(2, dispatches.Count);
        JsonArray arguments = dispatches[0]["params"]!["arguments"]!.AsArray();
        Assert.Equal("div-1", (string?)arguments[0]!["sharedId"]);
        Assert.Equal("click", (string?)arguments[1]!["value"]);
        Assert.Equal("clientX", (string?)arguments[2]!["value"]![0]![0]);
        Assert.Equal(12, (int?)arguments[2]!["value"]![0]![1]!["value"]);
        Assert.Empty(dispatches[1]["params"]!["arguments"]![2]!["value"]!.AsArray());
    }

    [Fact]
    public async Task DispatchEventWaitsForAMatch()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Task<bool> dispatch = page.Locate(new CssLocator("div")).DispatchEventAsync("click", timeout: TimeSpan.FromSeconds(1), cancellationToken: TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, dispatch));

        Assert.Equal("Timed out after 1 seconds waiting for css \"div\" to be attached; no element matched.", exception.Message);
        Assert.Empty(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page, FakeTimeProvider Time)> OpenPageAsync()
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new DramaturgeOptions() { PollInterval = PollInterval, TimeProvider = time }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page, time);
    }

    // Finds "#source" as source-1 and "#target" as target-1, unless the target is missing.
    private static void AnswerLookups(FakeSession session, bool missingTarget = false)
    {
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", parameters => (string?)parameters["locator"]!["value"] switch
        {
            "#source" => ProtocolJson.Nodes("source-1"),
            _ => missingTarget ? ProtocolJson.Nodes(0) : ProtocolJson.Nodes("target-1"),
        });
    }

    // Answers readiness checks from their arguments, and every other script, such as a scroll, with undefined.
    private static void AnswerScripts(FakeSession session, Func<JsonArray, JsonObject> readiness)
    {
        session.RemoteEnd.AnswerWith("script.callFunction", parameters => ((string)parameters["functionDeclaration"]!).Contains("isInteractionReady")
            ? ProtocolJson.Success(readiness(parameters["arguments"]!.AsArray()))
            : ProtocolJson.Success(new JsonObject() { ["type"] = "undefined" }));
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

    private static IReadOnlyList<JsonObject> ScriptCalls(FakeSession session, string functionFragment)
    {
        return [.. session.RemoteEnd.CommandsFor("script.callFunction").Where(command => ((string)command["params"]!["functionDeclaration"]!).Contains(functionFragment))];
    }

    private static JsonArray Sources(FakeSession session)
    {
        return Assert.Single(session.RemoteEnd.CommandsFor("input.performActions"))["params"]!["actions"]!.AsArray();
    }

    private static JsonObject PointerSource(FakeSession session)
    {
        return Sources(session).Single(source => (string?)source!["type"] == "pointer")!.AsObject();
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
