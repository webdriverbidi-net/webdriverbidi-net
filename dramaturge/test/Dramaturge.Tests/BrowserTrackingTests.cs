// <copyright file="BrowserTrackingTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi;
using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;

public class BrowserTrackingTests
{
    [Fact]
    public async Task StartFindsBrowsersAndTheirPages()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        string userContextId = session.AddUserContext();
        FakeContext defaultTab = session.AddContext();
        FakeContext otherTab = session.AddContext(userContextId);
        session.AddContext(parentId: defaultTab.Id);

        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([Browser.DefaultBrowserId, userContextId], group.Browsers.Select(browser => browser.Id));
        Assert.True(group.DefaultBrowser.IsDefault);
        Assert.Same(group, group.DefaultBrowser.Group);
        Page page = Assert.Single(group.DefaultBrowser.Pages);
        Assert.Equal(defaultTab.Id, page.Id);
        Assert.Same(group.DefaultBrowser, page.Browser);
        Assert.False(page.IsClosed);
        Assert.Equal([otherTab.Id], group.Browsers[1].Pages.Select(p => p.Id));
        Assert.False(group.Browsers[1].IsDefault);
    }

    // The tree is read after subscribing, so a page closing in between is reported closed before the tree reports it open.
    [Fact]
    public async Task PageClosedWhileStartingIsNotTracked()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.getTree", _ => new FakeResponse(
            new JsonObject() { ["contexts"] = new JsonArray(ContextJson("closing-tab", Browser.DefaultBrowserId, null)) },
            [("browsingContext.contextDestroyed", ContextJson("closing-tab", Browser.DefaultBrowserId, null))]));

        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(group.DefaultBrowser.Pages);
    }

    [Fact]
    public async Task NewPageIsTrackedAndAnnouncedOnce()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        List<Page> announced = [];
        group.DefaultBrowser.OnPageCreated.AddObserver(e => announced.Add(e.Page));

        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([page], announced);
        Assert.Equal([page], group.DefaultBrowser.Pages);
        Assert.Equal("tab", (string?)Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.create"))["params"]!["type"]);
    }

    [Fact]
    public async Task NewPageReturnsOnceTheObserversOfItsCreationHaveRun()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        TaskCompletionSource<bool> entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<bool> release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        group.DefaultBrowser.OnPageCreated.AddObserver(async _ =>
        {
            entered.TrySetResult(true);
            await release.Task;
        });

        // The browser announces the page before answering the command, so the observer runs for the event.
        Task<Page> opening = group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        await session.RemoteEnd.WaitForCommandAsync("browsingContext.create");
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(opening.IsCompleted);
        release.TrySetResult(true);
        Assert.Same(Assert.Single(group.DefaultBrowser.Pages), await opening);
    }

    [Fact]
    public async Task PageCreatedWithoutAnEventIsStillTracked()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("browsingContext.create", _ => new JsonObject() { ["context"] = "quiet-tab", ["userContext"] = Browser.DefaultBrowserId });
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        List<Page> announced = [];
        group.DefaultBrowser.OnPageCreated.AddObserver(e => announced.Add(e.Page));

        Page page = await group.DefaultBrowser.NewPageAsync(CreateType.Window, TestContext.Current.CancellationToken);

        Assert.Equal("quiet-tab", page.Id);
        Assert.Equal([page], announced);
        Assert.Equal("window", (string?)Assert.Single(session.RemoteEnd.CommandsFor("browsingContext.create"))["params"]!["type"]);
    }

    [Fact]
    public async Task PageClosedElsewhereIsRemovedAndAnnounced()
    {
        (BiDiDriver driver, FakeSession _) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        List<Page> announced = [];
        group.DefaultBrowser.OnPageClosed.AddObserver(e => announced.Add(e.Page));

        await driver.BrowsingContext.CloseAsync(new WebDriverBiDi.BrowsingContext.CloseCommandParameters(page.Id), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(group.DefaultBrowser.Pages);
        Assert.True(page.IsClosed);
        Assert.Equal([page], announced);
    }

    [Fact]
    public async Task CreatedBrowserIsTrackedAndRemovedWhenTheGroupIsDisposed()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);

        Browser browser = await group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page page = await browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        bool wasTracked = group.Browsers.Contains(browser);
        await group.DisposeAsync();

        Assert.True(wasTracked);
        Assert.Equal(browser.Id, page.Browser.Id);
        Assert.Equal([Browser.DefaultBrowserId], session.UserContextIds);
        Assert.Empty(session.Contexts);
    }

    [Fact]
    public async Task BrowserCreatedElsewhereAppearsWithItsFirstPageAndIsLeftWhenTheGroupIsDisposed()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);

        CreateUserContextCommandResult userContext = await driver.Browser.CreateUserContextAsync(new CreateUserContextCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        bool trackedBeforePage = group.Browsers.Any(browser => browser.Id == userContext.UserContextId);
        CreateCommandResult tab = await driver.BrowsingContext.CreateAsync(new CreateCommandParameters(CreateType.Tab) { UserContextId = userContext.UserContextId }, cancellationToken: TestContext.Current.CancellationToken);
        Browser browser = Assert.Single(group.Browsers, browser => browser.Id == userContext.UserContextId);
        await group.DisposeAsync();

        Assert.False(trackedBeforePage);
        Assert.Equal([tab.BrowsingContextId], browser.Pages.Select(page => page.Id));
        Assert.Contains(userContext.UserContextId, session.UserContextIds);
        Assert.Empty(session.RemoteEnd.CommandsFor("browser.removeUserContext"));
    }

    [Fact]
    public async Task ClosedBrowserIsRemovedWithItsPages()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Browser browser = await group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page page = await browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);

        await browser.CloseAsync(TestContext.Current.CancellationToken);
        await group.DisposeAsync();

        Assert.DoesNotContain(browser, group.Browsers);
        Assert.True(page.IsClosed);
        Assert.Single(session.RemoteEnd.CommandsFor("browser.removeUserContext"));
    }

    [Fact]
    public async Task ClosedDefaultBrowserClosesItsPagesAndStays()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        session.AddContext();
        session.AddContext();
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        IReadOnlyList<Page> pages = group.DefaultBrowser.Pages;

        await group.DefaultBrowser.CloseAsync(TestContext.Current.CancellationToken);

        Assert.All(pages, page => Assert.True(page.IsClosed));
        Assert.Empty(group.DefaultBrowser.Pages);
        Assert.Contains(group.DefaultBrowser, group.Browsers);
        Assert.Empty(session.RemoteEnd.CommandsFor("browser.removeUserContext"));
    }

    [Fact]
    public async Task FramesAreNotPages()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext tab = session.AddContext();
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);

        await session.CreateFrameAsync(tab.Id);
        await WaitForEventProcessingAsync(driver);

        Assert.Equal([tab.Id], group.DefaultBrowser.Pages.Select(page => page.Id));
    }

    [Fact]
    public async Task ClosingOfUntrackedContextsIsIgnored()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext tab = session.AddContext();
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);

        await session.RemoteEnd.RaiseEventAsync("browsingContext.contextDestroyed", ContextJson("unknown-tab", Browser.DefaultBrowserId, null));
        await session.RemoteEnd.RaiseEventAsync("browsingContext.contextDestroyed", ContextJson("other-tab", "unknown-user-context", null));
        await WaitForEventProcessingAsync(driver);

        Assert.Equal([tab.Id], group.DefaultBrowser.Pages.Select(page => page.Id));
        Assert.Equal([Browser.DefaultBrowserId], group.Browsers.Select(browser => browser.Id));
    }

    [Fact]
    public async Task DisposalFailuresAreReportedAndDisposalContinues()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Browser browser = await group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.FailWith("browser.removeUserContext", "unknown error", "cannot remove");
        session.RemoteEnd.FailWith("session.unsubscribe", "unknown error", "cannot unsubscribe");
        List<string> messages = [];
        group.OnLogMessage.AddObserver(e => messages.Add(e.Message));

        await group.DisposeAsync();

        Assert.Equal(2, messages.Count);
        Assert.Contains($"Closing browser {browser.Id} failed", messages[0]);
        Assert.Contains("cannot remove", messages[0]);
        Assert.Contains("Removing the event subscription failed", messages[1]);
        Assert.Equal(0, driver.BrowsingContext.OnContextCreated.CurrentObserverCount);
        Assert.Equal(0, driver.BrowsingContext.OnContextDestroyed.CurrentObserverCount);
    }

    [Fact]
    public async Task ConnectionThatCannotReadTheContextsRemovesWhatItAdded()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.FailWith("browsingContext.getTree", "unknown error", "no tree");

        await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Single(session.RemoteEnd.CommandsFor("session.unsubscribe"));
        Assert.Equal(0, driver.BrowsingContext.OnContextCreated.CurrentObserverCount);
        Assert.True(driver.IsStarted);
    }

    [Fact]
    public async Task ConnectionThatCannotSubscribeHasNothingToUnsubscribe()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.FailWith("session.subscribe", "unknown error", "no subscriptions");

        await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Empty(session.RemoteEnd.CommandsFor("session.unsubscribe"));
        Assert.Equal(0, driver.BrowsingContext.OnContextDestroyed.CurrentObserverCount);
    }

    [Fact]
    public async Task LaunchThatCannotReadTheContextsEndsTheSession()
    {
        await using ScriptedBiDiServer server = await ScriptedBiDiServer.StartAsync();
        server.FailWith("browsingContext.getTree", "unknown error", "no tree");

        await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => BrowserGroup.LaunchAsync(BrowserLauncher.Configure(BrowserKind.Firefox).ConnectToExisting(server.Url), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("session.end", server.ReceivedMethods[^1]);
        Assert.DoesNotContain("session.unsubscribe", server.ReceivedMethods);
    }

    private static JsonObject ContextJson(string id, string userContextId, string? parentId)
    {
        JsonObject context = new()
        {
            ["context"] = id,
            ["clientWindow"] = "window-1",
            ["originalOpener"] = null,
            ["url"] = "about:blank",
            ["userContext"] = userContextId,
            ["children"] = null,
        };
        if (parentId is not null)
        {
            context["parent"] = parentId;
        }

        return context;
    }

    private static JsonObject CreatedJson(string id, string userContextId, string? parentId)
    {
        JsonObject context = ContextJson(id, userContextId, parentId);
        context["hasPlannedNavigation"] = false;
        return context;
    }

    // Events and responses are processed in order, so a command's completion means earlier events have been handled.
    private static Task WaitForEventProcessingAsync(BiDiDriver driver)
    {
        return driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
    }
}
