// <copyright file="PageTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;

public class PageTests
{
    [Fact]
    public async Task StartFindsFramesWithTheirUrls()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext tab = session.AddContext(url: "https://example.com/");
        FakeContext frame = session.AddContext(parentId: tab.Id, url: "https://example.com/frame");
        FakeContext innerFrame = session.AddContext(parentId: frame.Id, url: "https://example.com/inner");

        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);

        Page page = Assert.Single(group.DefaultBrowser.Pages);
        Assert.Equal("https://example.com/", page.Url);
        Assert.Equal([tab.Id, frame.Id, innerFrame.Id], page.Frames.Select(f => f.Id));
        Assert.Same(page.MainFrame, page.Frames[0]);
        Assert.True(page.MainFrame.IsMainFrame);
        Assert.Null(page.MainFrame.ParentFrame);
        Assert.Equal([frame.Id], page.MainFrame.ChildFrames.Select(f => f.Id));
        Frame child = page.Frames[1];
        Assert.False(child.IsMainFrame);
        Assert.Same(page.MainFrame, child.ParentFrame);
        Assert.Same(page, child.Page);
        Assert.Equal("https://example.com/frame", child.Url);
        Assert.Same(child, page.Frames[2].ParentFrame);
        Assert.Equal("https://example.com/inner", page.Frames[2].Url);
    }

    [Fact]
    public async Task FrameIsAddedAndDetachedWithTheFramesWithinIt()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        FakeContext frame = await session.CreateFrameAsync(page.Id, "https://example.com/frame");
        FakeContext innerFrame = await session.CreateFrameAsync(frame.Id);
        FakeContext otherFrame = await session.CreateFrameAsync(page.Id);
        await WaitForEventProcessingAsync(driver);
        IReadOnlyList<Frame> framesBefore = page.Frames;

        await session.RemoteEnd.RaiseEventAsync("browsingContext.contextDestroyed", ContextJson(frame.Id, page.Id));
        await WaitForEventProcessingAsync(driver);

        Assert.Equal([page.Id, frame.Id, innerFrame.Id, otherFrame.Id], framesBefore.Select(f => f.Id));
        Assert.Equal([page.Id, otherFrame.Id], page.Frames.Select(f => f.Id));
        Assert.True(framesBefore[1].IsDetached);
        Assert.True(framesBefore[2].IsDetached);
        Assert.False(framesBefore[3].IsDetached);
        Assert.False(page.IsClosed);
    }

    [Fact]
    public async Task FrameOfUntrackedContextOrAlreadyTrackedIsIgnored()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext tab = session.AddContext();
        FakeContext frame = session.AddContext(parentId: tab.Id, url: "https://example.com/first");
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);

        await session.RemoteEnd.RaiseEventAsync("browsingContext.contextCreated", CreatedJson("orphan-frame", "unknown-parent"));
        await session.RemoteEnd.RaiseEventAsync("browsingContext.contextCreated", CreatedJson(frame.Id, tab.Id));
        await WaitForEventProcessingAsync(driver);

        Page page = Assert.Single(group.DefaultBrowser.Pages);
        Assert.Equal([tab.Id, frame.Id], page.Frames.Select(f => f.Id));
        Assert.Equal("https://example.com/first", page.Frames[1].Url);
    }

    [Fact]
    public async Task UrlFollowsNavigationFragmentsAndHistory()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        FakeContext frame = await session.CreateFrameAsync(page.Id);

        string navigatedUrl = await page.NavigateAsync("https://example.com/", cancellationToken: TestContext.Current.CancellationToken);
        string urlAfterNavigation = page.Url;
        await session.RaiseNavigationEventAsync("browsingContext.fragmentNavigated", page.Id, "https://example.com/#section");
        await WaitForEventProcessingAsync(driver);
        string urlAfterFragment = page.Url;
        await session.RaiseHistoryUpdatedAsync(page.Id, "https://example.com/pushed");
        await session.RaiseNavigationEventAsync("browsingContext.navigationCommitted", frame.Id, "https://example.com/frame");
        await session.RaiseNavigationEventAsync("browsingContext.navigationCommitted", "untracked-context", "https://example.com/elsewhere");
        await WaitForEventProcessingAsync(driver);

        Assert.Equal("https://example.com/", navigatedUrl);
        Assert.Equal("https://example.com/", urlAfterNavigation);
        Assert.Equal("https://example.com/#section", urlAfterFragment);
        Assert.Equal("https://example.com/pushed", page.Url);
        Assert.Equal("https://example.com/frame", page.Frames[1].Url);
    }

    [Fact]
    public async Task NavigationWaitsAsRequested()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);

        await page.NavigateAsync("https://example.com/", ReadinessState.Interactive, cancellationToken: TestContext.Current.CancellationToken);
        string reloadedUrl = await page.ReloadAsync(ReadinessState.None, cancellationToken: TestContext.Current.CancellationToken);

        JsonObject navigate = Parameters(session, "browsingContext.navigate");
        Assert.Equal(page.Id, (string?)navigate["context"]);
        Assert.Equal("interactive", (string?)navigate["wait"]);
        JsonObject reload = Parameters(session, "browsingContext.reload");
        Assert.Equal(page.Id, (string?)reload["context"]);
        Assert.Equal("none", (string?)reload["wait"]);
        Assert.Equal("https://example.com/", reloadedUrl);
    }

    [Fact]
    public async Task NavigationTimesOutAfterTheGivenOrConfiguredTime()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new AutomationOptions() { NavigationTimeout = TimeSpan.FromMilliseconds(200) }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.NeverAnswer("browsingContext.navigate");
        session.RemoteEnd.NeverAnswer("browsingContext.reload");
        session.RemoteEnd.NeverAnswer("browsingContext.traverseHistory");

        await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => page.NavigateAsync("https://example.com/", cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => page.ReloadAsync(timeout: TimeSpan.FromMilliseconds(100), cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => page.GoBackAsync(cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => page.GoForwardAsync(timeout: TimeSpan.FromMilliseconds(100), cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HistoryAndActivationAreSentForThePage()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("browsingContext.traverseHistory", parameters => new FakeResponse(new JsonObject(), [("browsingContext.historyUpdated", new JsonObject() { ["context"] = (string)parameters["context"]!, ["timestamp"] = 1790000000000, ["url"] = "https://example.com/other" })]));
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(new JsonObject() { ["type"] = "string", ["value"] = "complete" }));

        await page.GoBackAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.GoForwardAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.BringToFrontAsync(TestContext.Current.CancellationToken);

        Assert.Equal([$"{page.Id}:-1", $"{page.Id}:1"], session.RemoteEnd.CommandsFor("browsingContext.traverseHistory").Select(command => $"{command["params"]!["context"]}:{command["params"]!["delta"]}"));
        Assert.Equal(page.Id, (string?)Parameters(session, "browsingContext.activate")["context"]);
    }

    [Fact]
    public async Task ClosedPageIsDetachedAndAnnouncedOnce()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await session.CreateFrameAsync(page.Id);
        await WaitForEventProcessingAsync(driver);
        Frame frame = page.Frames[1];
        List<Page> closedByPage = [];
        List<Page> closedByBrowser = [];
        page.OnClosed.AddObserver(e => closedByPage.Add(e.Page));
        group.DefaultBrowser.OnPageClosed.AddObserver(e => closedByBrowser.Add(e.Page));

        await page.CloseAsync(TestContext.Current.CancellationToken);

        Assert.True(page.IsClosed);
        Assert.True(frame.IsDetached);
        Assert.Equal([page], closedByPage);
        Assert.Equal([page], closedByBrowser);
        Assert.Empty(group.DefaultBrowser.Pages);
    }

    [Fact]
    public async Task PageClosedWithoutAnEventIsStillClosed()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("browsingContext.close", new JsonObject());
        List<Page> closed = [];
        page.OnClosed.AddObserver(e => closed.Add(e.Page));

        await page.CloseAsync(TestContext.Current.CancellationToken);

        Assert.True(page.IsClosed);
        Assert.Equal([page], closed);
        Assert.Empty(group.DefaultBrowser.Pages);
    }

    private static JsonObject Parameters(FakeSession session, string method)
    {
        return Assert.Single(session.RemoteEnd.CommandsFor(method))["params"]!.AsObject();
    }

    private static JsonObject ContextJson(string id, string parentId)
    {
        return new JsonObject()
        {
            ["context"] = id,
            ["clientWindow"] = "window-1",
            ["originalOpener"] = null,
            ["url"] = "about:blank",
            ["userContext"] = Browser.DefaultBrowserId,
            ["children"] = null,
            ["parent"] = parentId,
        };
    }

    private static JsonObject CreatedJson(string id, string parentId)
    {
        JsonObject context = ContextJson(id, parentId);
        context["url"] = "https://example.com/second";
        context["hasPlannedNavigation"] = false;
        return context;
    }

    // Events and responses are processed in order, so a command's completion means earlier events have been handled.
    private static Task WaitForEventProcessingAsync(BiDiDriver driver)
    {
        return driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
    }
}
