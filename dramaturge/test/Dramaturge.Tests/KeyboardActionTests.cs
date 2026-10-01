// <copyright file="KeyboardActionTests.cs" company="WebDriverBiDi.NET Committers">
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

public class KeyboardActionTests
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    [Fact]
    public async Task FocusAndBlurCallTheElementInTheSandbox()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("input-1"));
        AnswerScripts(session, [Readiness("ready")], [true]);

        await page.Locate(new CssLocator("input")).FocusAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("input")).BlurAsync(cancellationToken: TestContext.Current.CancellationToken);

        IReadOnlyList<JsonObject> calls = session.RemoteEnd.CommandsFor("script.callFunction");
        Assert.Equal(["(element) => element.focus()", "(element) => element.blur()"], calls.Select(call => (string?)call["params"]!["functionDeclaration"]));
        Assert.All(calls, call =>
        {
            Assert.Equal(page.Group().Options.SandboxName, (string?)call["params"]!["target"]!["sandbox"]);
            Assert.Equal("input-1", (string?)call["params"]!["arguments"]![0]!["sharedId"]);
        });
        Assert.Empty(session.RemoteEnd.CommandsFor("input.performActions"));
    }

    [Fact]
    public async Task FocusWaitsForAMatchAndRejectsSeveral()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(0));

        Task focus = page.Locate(new CssLocator("input")).FocusAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException timeout = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, focus));
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(2));

        Assert.Equal("Timed out after 1 seconds waiting for css \"input\" to be focused; no element matched.", timeout.Message);
        await Assert.ThrowsAsync<AmbiguousElementException>(() => page.Locate(new CssLocator("input")).BlurAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Empty(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task PressFocusesThenPressesTheKeyWithItsModifiers()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("input-1"));
        AnswerScripts(session, [Readiness("ready")], [true]);

        await page.Locate(new CssLocator("input")).PressAsync("a", new KeyActionOptions() { Modifiers = KeyModifiers.Control | KeyModifiers.Shift }, TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("input")).PressAsync(Keys.Enter, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, ScriptCalls(session, "element.focus()").Count);
        Assert.Equal(
            [$"keyDown:{Keys.Control}", $"keyDown:{Keys.Shift}", "keyDown:a", "keyUp:a", $"keyUp:{Keys.Shift}", $"keyUp:{Keys.Control}"],
            KeyActions(session, 0));
        Assert.Equal([$"keyDown:{Keys.Enter}", $"keyUp:{Keys.Enter}"], KeyActions(session, 1));
        Assert.Equal(page.Id, (string?)session.RemoteEnd.CommandsFor("input.performActions")[0]["params"]!["context"]);
    }

    [Fact]
    public async Task PressSequentiallyTypesEachCharacterWithTheDelayBetween()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("input-1"));
        AnswerScripts(session, [Readiness("ready")], [true]);

        await page.Locate(new CssLocator("input")).PressSequentiallyAsync("h😀", new PressSequentiallyOptions() { Delay = TimeSpan.FromMilliseconds(20) }, TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("input")).PressSequentiallyAsync("ok", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, ScriptCalls(session, "element.focus()").Count);
        Assert.Equal(["keyDown:h", "keyUp:h", "pause:20", "keyDown:😀", "keyUp:😀"], KeyActions(session, 0));
        Assert.Equal(["keyDown:o", "keyUp:o", "keyDown:k", "keyUp:k"], KeyActions(session, 1));
    }

    [Fact]
    public async Task FillWaitsForReadinessThenSelectsAndTypesOverTheText()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("input-1"));
        AnswerScripts(session, [NotReady("hidden"), Readiness("needsscroll"), Readiness("ready")], [false, true]);

        await DriveAsync(time, page.Locate(new CssLocator("input")).FillAsync("héllo", cancellationToken: TestContext.Current.CancellationToken));

        IReadOnlyList<JsonObject> checks = ScriptCalls(session, "isInteractionReady");
        Assert.Equal(4, checks.Count);
        JsonArray checkArguments = checks[0]["params"]!["arguments"]!.AsArray();
        Assert.Equal("input-1", (string?)checkArguments[0]!["sharedId"]);
        Assert.Equal("type", (string?)checkArguments[1]!["value"]);
        Assert.Equal(2, checkArguments.Count);
        Assert.Single(ScriptCalls(session, "scrollIntoView"));
        IReadOnlyList<JsonObject> selections = ScriptCalls(session, "selectText");
        Assert.Equal(2, selections.Count);
        Assert.Equal("input-1", (string?)selections[0]["params"]!["arguments"]![0]!["sharedId"]);
        Assert.Equal(["keyDown:h", "keyUp:h", "keyDown:é", "keyUp:é", "keyDown:l", "keyUp:l", "keyDown:l", "keyUp:l", "keyDown:o", "keyUp:o"], KeyActions(session, 0));
    }

    [Fact]
    public async Task ClearChecksForClearingThenDeletesTheSelectedText()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("input-1"));
        AnswerScripts(session, [Readiness("ready")], [true]);

        await page.Locate(new CssLocator("input")).ClearAsync(new ActionOptions() { Timeout = TimeSpan.FromSeconds(1) }, TestContext.Current.CancellationToken);

        Assert.Equal("clear", (string?)Assert.Single(ScriptCalls(session, "isInteractionReady"))["params"]!["arguments"]![1]!["value"]);
        Assert.Single(ScriptCalls(session, "selectText"));
        Assert.Equal([$"keyDown:{Keys.Delete}", $"keyUp:{Keys.Delete}"], KeyActions(session, 0));
    }

    [Fact]
    public async Task ForcedFillSkipsTheReadinessCheck()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("input-1"));
        AnswerScripts(session, [NotReady("hidden")], [true]);

        await page.Locate(new CssLocator("input")).FillAsync("x", new ActionOptions() { Force = true }, TestContext.Current.CancellationToken);

        Assert.Empty(ScriptCalls(session, "isInteractionReady"));
        Assert.Single(ScriptCalls(session, "selectText"));
        Assert.Equal(["keyDown:x", "keyUp:x"], KeyActions(session, 0));
    }

    [Theory]
    [InlineData("div", "date")]
    [InlineData("input", null)]
    [InlineData("input", "email")]
    [InlineData("input", "not-a-type")]
    public async Task FillTypesIntoElementsWhoseValueIsText(string localName, string? inputType)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", Element(localName, inputType));
        AnswerScripts(session, [Readiness("ready")], [true]);

        await page.Locate(new CssLocator("#field")).FillAsync("x", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["keyDown:x", "keyUp:x"], KeyActions(session, 0));
    }

    [Theory]
    [InlineData("date")]
    [InlineData("CheckBox")]
    [InlineData(" range ")]
    public async Task FillRejectsInputsWhoseValueIsNotTypedAtOnce(string inputType)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", Element("input", inputType));

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("#field")).FillAsync("2026-09-30", new ActionOptions() { Force = true }, TestContext.Current.CancellationToken));

        Assert.Equal($"css \"#field\" is an input of type \"{inputType}\", whose value cannot be typed.", exception.Message);
        Assert.Empty(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task FillRejectsAnElementThatCannotBeEditedAtOnce()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("div-1"));
        AnswerScripts(session, [NotReady("noteditable")], [true]);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("div")).ClearAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("css \"div\" is not an editable element.", exception.Message);
        Assert.Single(ScriptCalls(session, "isInteractionReady"));
        Assert.Empty(session.RemoteEnd.CommandsFor("input.performActions"));
    }

    [Theory]
    [InlineData(0, "readOnly", true, "no element matched")]
    [InlineData(1, "readOnly", true, "the element was read-only")]
    [InlineData(1, null, false, "the element was removed from the document")]
    public async Task FillThatTimesOutSaysWhy(int matches, string? reason, bool selected, string observed)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(matches));
        AnswerScripts(session, [reason is null ? Readiness("ready") : NotReady(reason)], [selected]);

        Task fill = page.Locate(new CssLocator("input")).FillAsync("x", new ActionOptions() { Timeout = TimeSpan.FromSeconds(1) }, TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, fill));

        Assert.Equal($"Timed out after 1 seconds waiting for css \"input\" to be ready to be filled; {observed}.", exception.Message);
        Assert.Empty(session.RemoteEnd.CommandsFor("input.performActions"));
    }

    [Fact]
    public async Task FillRejectsSeveralMatches()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(2));

        await Assert.ThrowsAsync<AmbiguousElementException>(() => page.Locate(new CssLocator("input")).FillAsync("x", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void OptionsDefaultToNoModifiersNoDelayAndNoForce()
    {
        Assert.Equal(KeyModifiers.None, new KeyActionOptions().Modifiers);
        Assert.Null(new KeyActionOptions().Timeout);
        Assert.Equal(TimeSpan.Zero, new PressSequentiallyOptions().Delay);
        Assert.Null(new PressSequentiallyOptions().Timeout);
        Assert.False(new ActionOptions().Force);
        Assert.Null(new ActionOptions().Timeout);
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page, FakeTimeProvider Time)> OpenPageAsync()
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new DramaturgeOptions() { PollInterval = PollInterval, TimeProvider = time }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page, time);
    }

    // Answers readiness checks and text selections each with the next result in turn (repeating the last), and
    // every other script, such as a scroll, with undefined.
    private static void AnswerScripts(FakeSession session, JsonObject[] readiness, bool[] selections)
    {
        int[] calls = [0, 0];
        session.RemoteEnd.AnswerWith("script.callFunction", parameters =>
        {
            string function = (string)parameters["functionDeclaration"]!;
            if (function.Contains("isInteractionReady"))
            {
                return ProtocolJson.Success((JsonObject)readiness[Math.Min(Interlocked.Increment(ref calls[0]), readiness.Length) - 1].DeepClone());
            }

            return function.Contains("selectText")
                ? ProtocolJson.Boolean(selections[Math.Min(Interlocked.Increment(ref calls[1]), selections.Length) - 1])
                : ProtocolJson.Success(new JsonObject() { ["type"] = "undefined" });
        });
    }

    private static JsonObject Element(string localName, string? inputType)
    {
        JsonObject value = new() { ["nodeType"] = 1, ["childNodeCount"] = 0, ["localName"] = localName, ["namespaceURI"] = "http://www.w3.org/1999/xhtml" };
        if (inputType is not null)
        {
            value["attributes"] = new JsonObject() { ["id"] = "field", ["type"] = inputType };
        }

        return new JsonObject() { ["nodes"] = new JsonArray(new JsonObject() { ["type"] = "node", ["sharedId"] = "field-1", ["value"] = value }) };
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

    private static JsonObject NotReady(string reason)
    {
        return Readiness("notready", ("reason", new JsonObject() { ["type"] = "string", ["value"] = reason }));
    }

    private static IReadOnlyList<JsonObject> ScriptCalls(FakeSession session, string functionFragment)
    {
        return [.. session.RemoteEnd.CommandsFor("script.callFunction").Where(command => ((string)command["params"]!["functionDeclaration"]!).Contains(functionFragment))];
    }

    private static IEnumerable<string> KeyActions(FakeSession session, int command)
    {
        JsonArray sources = session.RemoteEnd.CommandsFor("input.performActions")[command]["params"]!["actions"]!.AsArray();
        JsonArray actions = Assert.Single(sources, source => (string?)source!["type"] == "key")!["actions"]!.AsArray();
        return actions.Select(action => $"{action!["type"]}:{action["value"] ?? action["duration"]}");
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
