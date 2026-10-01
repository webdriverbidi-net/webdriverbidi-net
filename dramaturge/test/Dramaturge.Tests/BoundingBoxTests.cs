// <copyright file="BoundingBoxTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using WebDriverBiDi;

public class BoundingBoxTests
{
    [Fact]
    public async Task MainFrameBoxInTheViewportIsAlreadyTopLevel()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        BoundingBox box = new(page.MainFrame, 1, 2, 3, 4);

        BoundingBox converted = await box.ToTopLevelAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Same(box, converted);
        Assert.Empty(session.RemoteEnd.CommandsFor("script.callFunction"));
    }

    [Fact]
    public async Task DocumentOriginAddsTheMainFramesScroll()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(Array(Number(10), Number(200))));

        BoundingBox converted = await new BoundingBox(page.MainFrame, 1, 2, 3, 4).ToTopLevelAsync(CoordinateOrigin.Document, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(new BoundingBox(page.MainFrame, 11, 202, 3, 4), converted);
        JsonObject call = Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"))["params"]!.AsObject();
        Assert.Equal("() => [window.scrollX, window.scrollY]", (string?)call["functionDeclaration"]);
        Assert.Equal(page.Group().Options.SandboxName, (string?)call["target"]!["sandbox"]);
    }

    [Fact]
    public async Task NestedBoxAddsTheOffsetOfEachFrameElementItIsIn()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext outer = await session.CreateFrameAsync(page.Id);
        FakeContext inner = await session.CreateFrameAsync(outer.Id);
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", parameters => (string?)parameters["context"] == page.Id ? ProtocolJson.Nodes("other-frame", "outer-frame") : ProtocolJson.Nodes("inner-frame"));
        session.RemoteEnd.AnswerWith("script.callFunction", parameters => (string?)parameters["target"]!["context"] == page.Id
            ? ProtocolJson.Success(Array(Array(Window("unrelated"), Number(0), Number(0)), Array(Window(outer.Id), Number(30), Number(40))))
            : ProtocolJson.Success(Array(Array(Window(inner.Id), Number(5), Number(6)))));
        Frame innerFrame = page.Frames.Single(frame => frame.Id == inner.Id);

        BoundingBox converted = await new BoundingBox(innerFrame, 1, 2, 3, 4).ToTopLevelAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(new BoundingBox(page.MainFrame, 36, 48, 3, 4), converted);
        IReadOnlyList<JsonObject> lookups = session.RemoteEnd.CommandsFor("browsingContext.locateNodes");
        Assert.Equal([outer.Id, page.Id], lookups.Select(lookup => (string?)lookup["params"]!["context"]));
        Assert.All(lookups, lookup => Assert.Equal("iframe, frame", (string?)lookup["params"]!["locator"]!["value"]));
        Assert.Equal(["other-frame", "outer-frame"], session.RemoteEnd.CommandsFor("script.callFunction")[1]["params"]!["arguments"]!.AsArray().Select(argument => (string?)argument!["sharedId"]));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task FrameElementThatCannotBeFoundIsReported(int frameElements)
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext child = await session.CreateFrameAsync(page.Id);
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(frameElements));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(Array(Array(new JsonObject() { ["type"] = "null" }, Number(0), Number(0)), Array(Window("unrelated"), Number(0), Number(0)))));
        Frame frame = page.Frames.Single(candidate => candidate.Id == child.Id);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => new BoundingBox(frame, 1, 2, 3, 4).ToTopLevelAsync(timeout: TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal($"The element of frame {child.Id} was not found in its parent frame; a frame element within a shadow root is not searched.", exception.Message);
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page);
    }

    private static JsonObject Number(double value) => new() { ["type"] = "number", ["value"] = value };

    private static JsonObject Window(string contextId) => new() { ["type"] = "window", ["value"] = new JsonObject() { ["context"] = contextId } };

    private static JsonObject Array(params JsonObject[] items) => new() { ["type"] = "array", ["value"] = new JsonArray([.. items]) };
}
