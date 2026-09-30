// <copyright file="FakeRemoteEndTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Text.Json.Nodes;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Session;

// The fake remote end stands in for a browser in every other test, so its behavior is pinned here.
public class FakeRemoteEndTests
{
    [Fact]
    public async Task CommandsAreRecordedAndAnsweredWithDefaultResults()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;

        SubscribeCommandResult result = await driver.Session.SubscribeAsync(new SubscribeCommandParameters(["log.entryAdded"]), cancellationToken: TestContext.Current.CancellationToken);

        Assert.StartsWith("subscription-", result.SubscriptionId);
        JsonObject sent = Assert.Single(remoteEnd.CommandsFor("session.subscribe"));
        Assert.Equal("log.entryAdded", (string?)sent["params"]!["events"]![0]);
    }

    [Fact]
    public async Task ConfiguredResultIsComputedFromParameters()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        remoteEnd.AnswerWith("browsingContext.navigate", parameters => new JsonObject() { ["navigation"] = "navigation-1", ["url"] = parameters["url"]!.DeepClone() });

        NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters("context-1", "https://example.com/"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("https://example.com/", result.Url);
    }

    [Fact]
    public async Task ConfiguredErrorFailsTheCommand()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        remoteEnd.FailWith("browsingContext.close", "no such frame", "No context with that ID");

        WebDriverBiDiCommandException exception = await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => driver.BrowsingContext.CloseAsync(new CloseCommandParameters("missing"), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("No context with that ID", exception.Message);
    }

    [Fact]
    public async Task UnansweredCommandTimesOut()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        remoteEnd.NeverAnswer("browsingContext.close");

        await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => driver.BrowsingContext.CloseAsync(new CloseCommandParameters("context-1"), TimeSpan.FromMilliseconds(200), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RaisedEventReachesObservers()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        TaskCompletionSource<NavigationEventArgs> received = new(TaskCreationOptions.RunContinuationsAsynchronously);
        driver.BrowsingContext.OnDomContentLoaded.AddObserver(e => received.TrySetResult(e));

        await remoteEnd.RaiseEventAsync("browsingContext.domContentLoaded", new JsonObject()
        {
            ["context"] = "context-1",
            ["navigation"] = "navigation-1",
            ["timestamp"] = 1790000000000,
            ["url"] = "https://example.com/",
        });

        NavigationEventArgs args = await received.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        Assert.Equal("context-1", args.BrowsingContextId);
    }

    [Fact]
    public async Task EventsOfAnErrorArriveBeforeIt()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        List<string> arrivals = [];
        driver.BrowsingContext.OnNavigationStarted.AddObserver(e => arrivals.Add($"event:{e.BrowsingContextId}"));
        JsonObject eventParameters = new()
        {
            ["context"] = "context-1",
            ["navigation"] = "navigation-1",
            ["timestamp"] = 1790000000000,
            ["url"] = "https://example.com/",
        };
        remoteEnd.AnswerWith("browsingContext.close", _ => FakeResponse.Failure("unknown error", "gone", ("browsingContext.navigationStarted", eventParameters)));

        await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => driver.BrowsingContext.CloseAsync(new CloseCommandParameters("context-1"), cancellationToken: TestContext.Current.CancellationToken));
        arrivals.Add("error");

        Assert.Equal(["event:context-1", "error"], arrivals);
    }

    [Fact]
    public async Task EventsOfAResponseArriveBeforeIt()
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        List<string> arrivals = [];
        driver.BrowsingContext.OnDomContentLoaded.AddObserver(e => arrivals.Add($"event:{e.BrowsingContextId}"));
        JsonObject eventParameters = new()
        {
            ["context"] = "context-1",
            ["navigation"] = "navigation-1",
            ["timestamp"] = 1790000000000,
            ["url"] = "https://example.com/",
        };
        remoteEnd.AnswerWith("browsingContext.close", _ => new FakeResponse(new JsonObject(), [("browsingContext.domContentLoaded", eventParameters)]));

        await driver.BrowsingContext.CloseAsync(new CloseCommandParameters("context-1"), cancellationToken: TestContext.Current.CancellationToken);
        arrivals.Add("response");

        Assert.Equal(["event:context-1", "response"], arrivals);
    }
}
