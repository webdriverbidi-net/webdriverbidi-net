// <copyright file="PageInputTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using WebDriverBiDi;
using WebDriverBiDi.Input;

public class PageInputTests
{
    [Fact]
    public async Task MouseMovesInStepsFromWhereItLastWas()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        await page.Mouse.MoveAsync(40, 20, steps: 2, cancellationToken: TestContext.Current.CancellationToken);
        await page.Mouse.MoveAsync(60, 30, cancellationToken: TestContext.Current.CancellationToken);

        JsonObject first = Source(session, 0);
        Assert.Equal("automation-mouse", (string?)first["id"]);
        Assert.Equal("pointer", (string?)first["type"]);
        Assert.Equal("mouse", (string?)first["parameters"]!["pointerType"]);
        Assert.Equal(["pointerMove:20:10", "pointerMove:40:20"], Actions(first));
        Assert.Equal(["pointerMove:60:30"], Actions(Source(session, 1)));
        Assert.All(session.RemoteEnd.CommandsFor("input.performActions"), command => Assert.Equal(page.Id, (string?)command["params"]!["context"]));
    }

    [Fact]
    public async Task MouseButtonsAreSentOnTheSameSourceAcrossCalls()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        await page.Mouse.DownAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Mouse.UpAsync(PointerButton.Right, TestContext.Current.CancellationToken);
        await page.Mouse.ClickAsync(5, 6, PointerButton.Middle, 2, TestContext.Current.CancellationToken);
        await page.Mouse.DblClickAsync(7, 8, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["pointerDown:0"], Actions(Source(session, 0)));
        Assert.Equal(["pointerUp:2"], Actions(Source(session, 1)));
        Assert.Equal(["pointerMove:5:6", "pointerDown:1", "pointerUp:1", "pointerDown:1", "pointerUp:1"], Actions(Source(session, 2)));
        Assert.Equal(["pointerMove:7:8", "pointerDown:0", "pointerUp:0", "pointerDown:0", "pointerUp:0"], Actions(Source(session, 3)));
        Assert.All(Enumerable.Range(0, 4), index => Assert.Equal("automation-mouse", (string?)Source(session, index)["id"]));
    }

    [Fact]
    public async Task WheelTurnsWhereTheMouseIs()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        await page.Mouse.ClickAsync(12.7, 30, cancellationToken: TestContext.Current.CancellationToken);
        await page.Mouse.WheelAsync(-5, 200, TestContext.Current.CancellationToken);

        JsonObject wheel = Source(session, 1);
        Assert.Equal("automation-wheel", (string?)wheel["id"]);
        Assert.Equal("wheel", (string?)wheel["type"]);
        JsonNode scroll = Assert.Single(wheel["actions"]!.AsArray())!;
        Assert.Equal("scroll:12:30:-5:200", $"{scroll["type"]}:{scroll["x"]}:{scroll["y"]}:{scroll["deltaX"]}:{scroll["deltaY"]}");
    }

    [Fact]
    public async Task MouseRejectsCountsBelowOne()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        ArgumentOutOfRangeException steps = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => page.Mouse.MoveAsync(1, 1, steps: 0, cancellationToken: TestContext.Current.CancellationToken));
        ArgumentOutOfRangeException clicks = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => page.Mouse.ClickAsync(1, 1, clickCount: 0, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("steps", steps.ParamName);
        Assert.Equal("clickCount", clicks.ParamName);
        Assert.Empty(session.RemoteEnd.CommandsFor("input.performActions"));
    }

    [Fact]
    public async Task KeyboardSendsEveryKeyOnTheSameSource()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        await page.Keyboard.DownAsync(Keys.Shift, TestContext.Current.CancellationToken);
        await page.Keyboard.UpAsync(Keys.Shift, TestContext.Current.CancellationToken);
        await page.Keyboard.PressAsync("a", KeyModifiers.Control, TestContext.Current.CancellationToken);
        await page.Keyboard.TypeAsync("h😀", TimeSpan.FromMilliseconds(15), TestContext.Current.CancellationToken);

        Assert.All(Enumerable.Range(0, 4), index => Assert.Equal("automation-keyboard", (string?)Source(session, index)["id"]));
        Assert.Equal([$"keyDown:{Keys.Shift}"], Actions(Source(session, 0)));
        Assert.Equal([$"keyUp:{Keys.Shift}"], Actions(Source(session, 1)));
        Assert.Equal([$"keyDown:{Keys.Control}", "keyDown:a", "keyUp:a", $"keyUp:{Keys.Control}"], Actions(Source(session, 2)));
        Assert.Equal(["keyDown:h", "keyUp:h", "pause:15", "keyDown:😀", "keyUp:😀"], Actions(Source(session, 3)));
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page);
    }

    private static JsonObject Source(FakeSession session, int command)
    {
        return Assert.Single(session.RemoteEnd.CommandsFor("input.performActions")[command]["params"]!["actions"]!.AsArray())!.AsObject();
    }

    private static IEnumerable<string?> Actions(JsonObject source)
    {
        return source["actions"]!.AsArray().Select(action => (string?)action!["type"] switch
        {
            "pointerMove" => $"pointerMove:{(double?)action["x"]}:{(double?)action["y"]}",
            "pointerDown" or "pointerUp" => $"{action["type"]}:{action["button"]}",
            "pause" => $"pause:{action["duration"]}",
            _ => $"{action["type"]}:{action["value"]}",
        });
    }
}
