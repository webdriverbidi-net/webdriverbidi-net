// <copyright file="FakeSessionTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Session;

// The fake session stands in for a browser's contexts in the Automation tests, so its behavior is pinned here.
public class FakeSessionTests
{
    [Fact]
    public async Task NewSessionIsAnswered()
    {
        (BiDiDriver driver, FakeSession _) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;

        NewCommandResult result = await driver.Session.NewSessionAsync(new NewCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("fake-session", result.SessionId);
        Assert.Equal("fake", result.Capabilities.BrowserName);
    }

    [Fact]
    public async Task TreeReportsContextsWithTheirChildren()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext tab = session.AddContext(url: "https://example.com/");
        FakeContext frame = session.AddContext(parentId: tab.Id);
        FakeContext otherTab = session.AddContext();

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([tab.Id, otherTab.Id], tree.ContextTree.Select(context => context.BrowsingContextId));
        Assert.Equal("https://example.com/", tree.ContextTree[0].Url);
        BrowsingContextInfo child = Assert.Single(tree.ContextTree[0].Children!);
        Assert.Equal(frame.Id, child.BrowsingContextId);
        Assert.Equal(tab.Id, child.Parent);
    }

    [Fact]
    public async Task TreeHonorsRootAndMaximumDepth()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        session.AddContext();
        FakeContext tab = session.AddContext();
        session.AddContext(parentId: tab.Id);

        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters() { RootBrowsingContextId = tab.Id, MaxDepth = 0 }, cancellationToken: TestContext.Current.CancellationToken);

        BrowsingContextInfo root = Assert.Single(tree.ContextTree);
        Assert.Equal(tab.Id, root.BrowsingContextId);
        Assert.Null(root.Children);
    }

    [Fact]
    public async Task CreatedContextIsAnnouncedBeforeTheResponse()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        List<string> arrivals = [];
        driver.BrowsingContext.OnContextCreated.AddObserver(e => arrivals.Add($"created:{e.BrowsingContextId}:{e.UserContextId}"));

        CreateCommandResult result = await driver.BrowsingContext.CreateAsync(new CreateCommandParameters(CreateType.Tab), cancellationToken: TestContext.Current.CancellationToken);
        arrivals.Add("response");

        Assert.Equal([$"created:{result.BrowsingContextId}:{FakeSession.DefaultUserContextId}", "response"], arrivals);
        Assert.Equal(result.BrowsingContextId, Assert.Single(session.Contexts).Id);
    }

    [Fact]
    public async Task ClosedContextIsAnnouncedAndRemovedWithItsChildren()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        FakeContext tab = session.AddContext();
        session.AddContext(parentId: tab.Id);
        FakeContext otherTab = session.AddContext();
        List<string> destroyed = [];
        driver.BrowsingContext.OnContextDestroyed.AddObserver(e => destroyed.Add(e.BrowsingContextId));

        await driver.BrowsingContext.CloseAsync(new WebDriverBiDi.BrowsingContext.CloseCommandParameters(tab.Id), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([tab.Id], destroyed);
        Assert.Equal(otherTab, Assert.Single(session.Contexts));
    }

    [Fact]
    public async Task UserContextsAreCreatedListedAndRemovedWithTheirContexts()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        CreateUserContextCommandResult created = await driver.Browser.CreateUserContextAsync(new CreateUserContextCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        CreateCommandResult tab = await driver.BrowsingContext.CreateAsync(new CreateCommandParameters(CreateType.Tab) { UserContextId = created.UserContextId }, cancellationToken: TestContext.Current.CancellationToken);
        FakeContext defaultTab = session.AddContext();
        List<string> destroyed = [];
        driver.BrowsingContext.OnContextDestroyed.AddObserver(e => destroyed.Add(e.BrowsingContextId));

        GetUserContextsCommandResult listed = await driver.Browser.GetUserContextsAsync(new GetUserContextsCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        await driver.Browser.RemoveUserContextAsync(new RemoveUserContextCommandParameters(created.UserContextId), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([FakeSession.DefaultUserContextId, created.UserContextId], listed.UserContexts.Select(userContext => userContext.UserContextId));
        Assert.Equal(created.UserContextId, tab.UserContextId);
        Assert.Equal([tab.BrowsingContextId], destroyed);
        Assert.Equal([FakeSession.DefaultUserContextId], session.UserContextIds);
        Assert.Equal(defaultTab, Assert.Single(session.Contexts));
    }

    [Fact]
    public async Task NavigationEventIsRaised()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        TaskCompletionSource<NavigationEventArgs> received = new(TaskCreationOptions.RunContinuationsAsynchronously);
        driver.BrowsingContext.OnLoad.AddObserver(e => received.TrySetResult(e));

        await session.RaiseNavigationEventAsync("browsingContext.load", "context-1", "https://example.com/");

        NavigationEventArgs args = await received.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        Assert.Equal("context-1", args.BrowsingContextId);
        Assert.Equal("https://example.com/", args.Url);
    }
}
