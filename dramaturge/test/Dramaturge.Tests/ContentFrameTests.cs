// <copyright file="ContentFrameTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;

public class ContentFrameTests
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    [Fact]
    public async Task ContentWindowNamesTheTrackedFrame()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageWithFrameAsync();
        await using BiDiDriver ownedDriver = driver;
        Frame expected = page.Frames[1];
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("iframe-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", Window(expected.Id));

        Frame frame = await page.Locate(new CssLocator("iframe")).ContentFrameAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Same(expected, frame);
        JsonObject call = Assert.Single(session.RemoteEnd.CommandsFor("script.callFunction"))["params"]!.AsObject();
        Assert.Equal("(element) => element.contentWindow", (string?)call["functionDeclaration"]);
        Assert.Equal(page.Browser.Group.Options.SandboxName, (string?)call["target"]!["sandbox"]);
        Assert.Equal(page.Id, (string?)call["target"]!["context"]);
        Assert.Equal("iframe-1", (string?)call["arguments"]![0]!["sharedId"]);
    }

    [Fact]
    public async Task ElementAndFrameAreWaitedFor()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageWithFrameAsync();
        await using BiDiDriver ownedDriver = driver;
        AnswerInTurn(session, "browsingContext.locateNodes", ProtocolJson.Nodes(0), ProtocolJson.Nodes("iframe-1"));
        AnswerInTurn(session, "script.callFunction", ProtocolJson.Success(new JsonObject() { ["type"] = "null" }), Window("not-yet-tracked"), Window(page.Frames[1].Id));

        Frame frame = await DriveAsync(time, page.Locate(new CssLocator("iframe")).ContentFrameAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Same(page.Frames[1], frame);
        Assert.Equal(4, session.RemoteEnd.CommandsFor("browsingContext.locateNodes").Count);
        Assert.Equal(3, session.RemoteEnd.CommandsFor("script.callFunction").Count);
    }

    [Theory]
    [InlineData(0, null, "no element matched")]
    [InlineData(1, "null", "the frame had no document")]
    [InlineData(1, "untracked", "the frame was not tracked yet")]
    public async Task FrameNotFoundInTimeSaysWhy(int matches, string? window, string observed)
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageWithFrameAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(matches));
        session.RemoteEnd.AnswerWith("script.callFunction", window == "null" ? ProtocolJson.Success(new JsonObject() { ["type"] = "null" }) : Window("untracked"));

        Task<Frame> find = page.Locate(new CssLocator("iframe")).ContentFrameAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, find));

        Assert.Equal($"Timed out after 1 seconds waiting for the frame of css \"iframe\"; {observed}.", exception.Message);
    }

    [Fact]
    public async Task ElementThatIsNotAFrameFailsAtOnce()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageWithFrameAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes("div-1"));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(new JsonObject() { ["type"] = "undefined" }));

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("div")).ContentFrameAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("css \"div\" is not a frame element.", exception.Message);
        Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.locateNodes"));
    }

    [Fact]
    public async Task SeveralFrameElementsAreAmbiguous()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageWithFrameAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.locateNodes", ProtocolJson.Nodes(2));

        await Assert.ThrowsAsync<AmbiguousElementException>(() => page.Locate(new CssLocator("iframe")).ContentFrameAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page, FakeTimeProvider Time)> OpenPageWithFrameAsync()
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new AutomationOptions() { PollInterval = PollInterval, TimeProvider = time }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await session.CreateFrameAsync(page.Id);
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page, time);
    }

    private static JsonObject Window(string contextId)
    {
        return ProtocolJson.Success(new JsonObject() { ["type"] = "window", ["value"] = new JsonObject() { ["context"] = contextId } });
    }

    // Answers each sending of a command with the next result, repeating the last.
    private static void AnswerInTurn(FakeSession session, string method, params JsonObject[] results)
    {
        int[] calls = [0];
        session.RemoteEnd.AnswerWith(method, _ => results[Math.Min(Interlocked.Increment(ref calls[0]), results.Length) - 1].DeepClone());
    }

    // Moves fake time on by one poll interval at a time until the operation completes.
    private static async Task<T> DriveAsync<T>(FakeTimeProvider time, Task<T> operation)
    {
        while (!operation.IsCompleted)
        {
            time.Advance(PollInterval);
            await Task.WhenAny(operation, Task.Delay(5, TestContext.Current.CancellationToken));
        }

        return await operation;
    }
}
