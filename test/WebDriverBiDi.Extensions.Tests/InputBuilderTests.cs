// <copyright file="InputBuilderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Text.Json.Nodes;
using WebDriverBiDi.Input;
using WebDriverBiDi.Script;
using WebDriverBiDi.TestUtilities;

// The builder's output is checked as the JSON sent to the remote end.
public class InputBuilderTests
{
    private static readonly SharedReference Element = new("element-1");
    private static readonly SharedReference OtherElement = new("element-2");

    [Fact]
    public async Task DefaultSourcesAreCreatedOnFirstUse()
    {
        InputBuilder builder = new();
        Assert.Empty(builder.Build());

        builder.AddAction(builder.DefaultKeyInputSource.CreateKeyDown("a"));

        JsonArray sources = await SendAsync(builder);
        Assert.Equal("key", (string?)Assert.Single(sources)!["type"]);
    }

    [Fact]
    public async Task CreatedPointerDoesNotBecomeTheDefault()
    {
        InputBuilder builder = new();
        PointerInputSource pen = builder.CreatePointerInputSource(PointerType.Pen);

        PointerInputSource defaultPointer = builder.DefaultPointerInputSource;
        builder.AddAction(defaultPointer.CreatePointerDown());

        Assert.NotEqual(pen.SourceId, defaultPointer.SourceId);
        JsonArray sources = await SendAsync(builder);
        Assert.Equal(["pen", "mouse"], sources.Select(source => (string)source!["parameters"]!["pointerType"]!));
    }

    [Fact]
    public async Task ClearRemovesDefaultSourcesToo()
    {
        InputBuilder builder = new();
        string firstPointer = builder.DefaultPointerInputSource.SourceId;
        builder.AddAction(builder.DefaultKeyInputSource.CreateKeyDown("a"));

        builder.Clear();
        builder.AddAction(builder.DefaultPointerInputSource.CreatePointerDown());

        JsonArray sources = await SendAsync(builder);
        JsonNode pointer = Assert.Single(sources)!;
        Assert.NotEqual(firstPointer, (string?)pointer["id"]);
        Assert.Equal(["pointerDown"], pointer["actions"]!.AsArray().Select(action => (string)action!["type"]!));
    }

    [Fact]
    public async Task EachActionIsATickInWhichOtherSourcesPause()
    {
        InputBuilder builder = new();
        builder.AddAction(builder.DefaultKeyInputSource.CreateKeyDown("a"))
            .AddAction(builder.DefaultPointerInputSource.CreatePointerDown());

        JsonArray sources = await SendAsync(builder);

        Assert.Equal(["keyDown", "pause"], Types(sources[0]!));
        Assert.Equal(["pause", "pointerDown"], Types(sources[1]!));
    }

    [Fact]
    public async Task ActionsAddedTogetherShareATick()
    {
        InputBuilder builder = new();
        builder.AddActions(builder.DefaultKeyInputSource.CreateKeyDown(Keys.Shift), builder.DefaultPointerInputSource.CreatePointerDown());

        JsonArray sources = await SendAsync(builder);

        Assert.Equal(["keyDown"], Types(sources[0]!));
        Assert.Equal(["pointerDown"], Types(sources[1]!));
    }

    [Fact]
    public void TickWithInvalidActionAddsNothing()
    {
        InputBuilder builder = new();
        KeyInputSource keyboard = builder.DefaultKeyInputSource;
        InputAction foreign = new InputBuilder().DefaultKeyInputSource.CreateKeyDown("a");

        Assert.Throws<ArgumentException>(() => builder.AddActions(keyboard.CreateKeyDown("a"), foreign));
        Assert.Throws<ArgumentException>(() => builder.AddActions(keyboard.CreateKeyDown("a"), keyboard.CreateKeyUp("a")));

        Assert.Empty(((KeySourceActions)Assert.Single(builder.Build())).Actions);
    }

    [Fact]
    public void ActionOfWrongKindForItsSourceIsRejected()
    {
        InputBuilder builder = new();
        InputAction keyActionForPointer = new(builder.DefaultPointerInputSource.SourceId, new KeyDownAction("a"));

        Assert.Throws<WebDriverBiDiException>(() => builder.AddAction(keyActionForPointer));
    }

    [Fact]
    public async Task NoneSourcePausesForADuration()
    {
        InputBuilder builder = new();
        builder.AddAction(builder.DefaultKeyInputSource.CreateKeyDown("a"));
        NoneInputSource timer = builder.CreateNoneInputSource();

        builder.AddAction(timer.CreatePause(TimeSpan.FromMilliseconds(250)));

        JsonArray sources = await SendAsync(builder);
        JsonNode none = sources[1]!;
        Assert.Equal("none", (string?)none["type"]);
        Assert.Equal(["pause", "pause"], Types(none));
        Assert.Equal(250, (int?)none["actions"]![1]!["duration"]);
        Assert.Equal(InputSourceKind.None, timer.DeviceKind);
        Assert.Contains(timer.SourceId, timer.ToString());
    }

    [Fact]
    public async Task SourceAddedLaterPausesThroughEarlierTicks()
    {
        InputBuilder builder = new();
        builder.AddAction(builder.DefaultWheelInputSource.CreateScroll(0, 0, 0, 10));
        NoneInputSource timer = builder.CreateNoneInputSource();
        builder.AddAction(timer.CreatePause());
        builder.AddAction(builder.DefaultKeyInputSource.CreateKeyDown("a"));

        JsonArray sources = await SendAsync(builder);

        Assert.Equal(["scroll", "pause", "pause"], Types(sources[0]!));
        Assert.Equal(["pause", "pause", "pause"], Types(sources[1]!));
        Assert.Null(sources[1]!["actions"]![1]!["duration"]);
        Assert.Equal(["pause", "pause", "keyDown"], Types(sources[2]!));
        Assert.Same(builder.DefaultKeyInputSource, builder.DefaultKeyInputSource);
    }

    [Fact]
    public void SourcesReportTheirKind()
    {
        InputBuilder builder = new();

        Assert.Equal(
            [InputSourceKind.Key, InputSourceKind.Pointer, InputSourceKind.Wheel],
            new InputSource[] { builder.DefaultKeyInputSource, builder.DefaultPointerInputSource, builder.DefaultWheelInputSource }.Select(source => source.DeviceKind));
    }

    [Fact]
    public async Task PointerActionsCarryFractionalCoordinatesAndProperties()
    {
        InputBuilder builder = new();
        PointerInputSource pen = builder.CreatePointerInputSource(PointerType.Pen);
        PointerActionProperties properties = new() { Width = 2, Height = 3, Pressure = 0.5, TangentialPressure = -0.25, Twist = 90, AltitudeAngle = 1.2, AzimuthAngle = 3.1 };

        builder.AddAction(pen.CreatePointerMove(10.5, 20.25, duration: TimeSpan.FromMilliseconds(100), additionalProperties: properties))
            .AddAction(pen.CreatePointerDown(PointerButton.Left, properties))
            .AddAction(pen.CreatePointerUp());

        JsonArray actions = (await SendAsync(builder))[0]!["actions"]!.AsArray();
        Assert.Equal(10.5, (double?)actions[0]!["x"]);
        Assert.Equal(20.25, (double?)actions[0]!["y"]);
        Assert.Equal(100, (int?)actions[0]!["duration"]);
        foreach (JsonNode? action in actions.Take(2))
        {
            Assert.Equal(0.5, (double?)action!["pressure"]);
            Assert.Equal(90, (int?)action["twist"]);
            Assert.Equal(3.1, (double?)action["azimuthAngle"]);
        }
    }

    [Fact]
    public void TouchPointerHasOnlyOneButton()
    {
        PointerInputSource touch = new InputBuilder().CreatePointerInputSource(PointerType.Touch);

        Assert.Throws<ArgumentException>(() => touch.CreatePointerDown(PointerButton.Right));
        Assert.Throws<ArgumentException>(() => touch.CreatePointerUp(PointerButton.Right));
    }

    [Fact]
    public async Task TypingSendsEachCharacterAsOneKey()
    {
        InputBuilder builder = new();

        builder.AddSendKeysToActiveElementAction("a😀é" + Keys.Enter);

        JsonArray actions = (await SendAsync(builder))[0]!["actions"]!.AsArray();
        Assert.Equal(["a", "a", "😀", "😀", "é", "é", Keys.Enter, Keys.Enter], actions.Select(action => (string)action!["value"]!));
    }

    [Fact]
    public async Task ChordPressesKeysInOrderAndReleasesInReverse()
    {
        InputBuilder builder = new();

        builder.AddKeyChordAction(Keys.Control, Keys.Shift, "a");

        JsonArray actions = (await SendAsync(builder))[0]!["actions"]!.AsArray();
        Assert.Equal(
            [$"keyDown:{Keys.Control}", $"keyDown:{Keys.Shift}", "keyDown:a", "keyUp:a", $"keyUp:{Keys.Shift}", $"keyUp:{Keys.Control}"],
            actions.Select(action => $"{action!["type"]}:{action["value"]}"));
    }

    [Fact]
    public async Task ClickHelpersPressTheRequestedButton()
    {
        InputBuilder builder = new();

        builder.AddClickOnElementAction(Element, PointerButton.Right).AddDoubleClickOnElementAction(OtherElement);

        JsonArray actions = (await SendAsync(builder))[0]!["actions"]!.AsArray();
        Assert.Equal(["pointerMove", "pointerDown", "pointerUp", "pointerMove", "pointerDown", "pointerUp", "pointerDown", "pointerUp"], actions.Select(action => (string)action!["type"]!));
        Assert.Equal(2, (int?)actions[1]!["button"]);
        Assert.Equal(0, (int?)actions[4]!["button"]);
        Assert.Equal("element-2", (string?)actions[3]!["origin"]!["element"]!["sharedId"]);
    }

    [Fact]
    public async Task DragMovesFromSourceToTargetWithButtonHeld()
    {
        InputBuilder builder = new();

        builder.AddDragAndDropAction(Element, OtherElement);

        JsonArray actions = (await SendAsync(builder))[0]!["actions"]!.AsArray();
        Assert.Equal(["pointerMove", "pointerDown", "pointerMove", "pointerUp"], actions.Select(action => (string)action!["type"]!));
        Assert.Equal("element-1", (string?)actions[0]!["origin"]!["element"]!["sharedId"]);
        Assert.Equal("element-2", (string?)actions[2]!["origin"]!["element"]!["sharedId"]);
    }

    [Fact]
    public async Task ScrollIsOverElementOrViewport()
    {
        InputBuilder builder = new();

        builder.AddScrollAction(0, 300, Element).AddScrollAction(-50, 0);

        JsonNode wheel = (await SendAsync(builder))[0]!;
        Assert.Equal("wheel", (string?)wheel["type"]);
        JsonArray actions = wheel["actions"]!.AsArray();
        Assert.Equal(300, (int?)actions[0]!["deltaY"]);
        Assert.Equal("element-1", (string?)actions[0]!["origin"]!["element"]!["sharedId"]);
        Assert.Equal(-50, (int?)actions[1]!["deltaX"]);
        Assert.Null(actions[1]!["origin"]);
    }

    [Fact]
    public async Task ReleaseActionsTargetsTheBrowsingContext()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;

        await driver.Input.ReleaseActionsAsync("context-1", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("context-1", (string?)Assert.Single(remoteEnd.CommandsFor("input.releaseActions"))["params"]!["context"]);
    }

    [Theory]
    [InlineData(nameof(Keys.RightShift), "")]
    [InlineData(nameof(Keys.RightMeta), "")]
    [InlineData(nameof(Keys.NumberPadPageUp), "")]
    [InlineData(nameof(Keys.NumberPadDelete), "")]
    [InlineData(nameof(Keys.ZenkakuHankaku), "")]
    public void KeysHaveTheirSpecifiedCodePoints(string name, string expected)
    {
        Assert.Equal(expected, typeof(Keys).GetField(name)!.GetValue(null));
    }

    private static List<string> Types(JsonNode source) => [.. source["actions"]!.AsArray().Select(action => (string)action!["type"]!)];

    private static async Task<JsonArray> SendAsync(InputBuilder builder)
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await driver.Input.PerformActionsAsync("context-1", builder, cancellationToken: TestContext.Current.CancellationToken);
        return Assert.Single(remoteEnd.CommandsFor("input.performActions"))["params"]!["actions"]!.AsArray();
    }
}
