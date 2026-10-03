// <copyright file="NetworkTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Dramaturge.TestUtilities;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi;
using WebDriverBiDi.Network;

public class NetworkTests
{
    private const string RequestUrl = "https://example.com/api/data.json";
    private static readonly TimeSpan EventWait = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task RoutesAddAnInterceptEachAfterSubscribingOnce()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        RouteRegistration exact = await page.RouteAsync(RequestUrl, _ => Task.CompletedTask, cancellationToken: TestContext.Current.CancellationToken);
        await page.RouteAsync(new Regex("/api/"), _ => Task.CompletedTask, new UrlPatternPattern() { HostName = "example.com" }, TestContext.Current.CancellationToken);

        Assert.Equal(RequestUrl, exact.Description);
        JsonObject subscription = session.RemoteEnd.CommandsFor("session.subscribe").Last()["params"]!.AsObject();
        Assert.Equal(["network.beforeRequestSent", "network.responseCompleted"], subscription["events"]!.AsArray().Select(name => (string?)name));
        Assert.Equal(2, session.RemoteEnd.CommandsFor("session.subscribe").Count);
        IReadOnlyList<JsonObject> intercepts = session.RemoteEnd.CommandsFor("network.addIntercept");
        Assert.All(intercepts, intercept =>
        {
            Assert.Equal(["beforeRequestSent"], intercept["params"]!["phases"]!.AsArray().Select(phase => (string?)phase));
            Assert.Equal([page.Id], intercept["params"]!["contexts"]!.AsArray().Select(context => (string?)context));
        });
        Assert.False(intercepts[0]["params"]!.AsObject().ContainsKey("urlPatterns"));
        Assert.Equal("example.com", (string?)intercepts[1]["params"]!["urlPatterns"]![0]!["hostname"]);
    }

    [Fact]
    public async Task RoutesAreTriedNewestFirstSkippingThoseThatDoNotMatchOrDecide()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        List<string> tried = [];
        TaskCompletionSource<Route> fulfilled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await page.RouteAsync(new Regex("/api/"), async route =>
        {
            tried.Add("oldest");
            await route.FulfillAsync(201, "{}", new Dictionary<string, string>() { ["Content-Type"] = "application/json" });
            fulfilled.TrySetResult(route);
        }, cancellationToken: TestContext.Current.CancellationToken);
        await page.RouteAsync(RequestUrl, _ => { tried.Add("undecided"); return Task.CompletedTask; }, cancellationToken: TestContext.Current.CancellationToken);
        await page.RouteAsync("https://example.com/other", _ => { tried.Add("other"); return Task.CompletedTask; }, cancellationToken: TestContext.Current.CancellationToken);

        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", BlockedRequest(page.Id, InterceptIds(session)));
        Route route = await fulfilled.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["undecided", "oldest"], tried);
        Assert.Same(page, route.Page);
        Assert.Same(page.MainFrame, route.Frame);
        Assert.Equal(RequestUrl, route.Request.Url);
        JsonObject response = Assert.Single(session.RemoteEnd.CommandsFor("network.provideResponse"))["params"]!.AsObject();
        Assert.Equal("request-1", (string?)response["request"]);
        Assert.Equal(201, (int?)response["statusCode"]);
        Assert.Equal("{}", (string?)response["body"]!["value"]);
        Assert.Equal("Content-Type", (string?)response["headers"]![0]!["name"]);
        Assert.Empty(session.RemoteEnd.CommandsFor("network.continueRequest"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => route.AbortAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HandlersContinueAbortOrAnswerWithBinaryBodies()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        int[] requests = [0];
        TaskCompletionSource<bool> done = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await page.RouteAsync(RequestUrl, async route =>
        {
            switch (Interlocked.Increment(ref requests[0]))
            {
                case 1:
                    await route.ContinueAsync(new RouteOverrides() { Url = "https://example.com/elsewhere", Method = "POST", Headers = new Dictionary<string, string>() { ["X-Test"] = "1" }, Body = "payload" });
                    break;
                case 2:
                    await route.ContinueAsync();
                    break;
                case 3:
                    await route.FulfillAsync(200, [1, 2, 3]);
                    break;
                case 4:
                    await route.FulfillAsync(204);
                    break;
                default:
                    await route.AbortAsync();
                    done.TrySetResult(true);
                    break;
            }
        }, cancellationToken: TestContext.Current.CancellationToken);

        for (int i = 1; i <= 5; i++)
        {
            await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", BlockedRequest(page.Id, InterceptIds(session), $"request-{i}"));
        }

        await done.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);

        IReadOnlyList<JsonObject> continued = session.RemoteEnd.CommandsFor("network.continueRequest");
        JsonObject changed = continued[0]["params"]!.AsObject();
        Assert.Equal("https://example.com/elsewhere", (string?)changed["url"]);
        Assert.Equal("POST", (string?)changed["method"]);
        Assert.Equal("X-Test", (string?)changed["headers"]![0]!["name"]);
        Assert.Equal("payload", (string?)changed["body"]!["value"]);
        Assert.Equal(["request"], continued[1]["params"]!.AsObject().Select(property => property.Key));
        IReadOnlyList<JsonObject> responses = session.RemoteEnd.CommandsFor("network.provideResponse");
        Assert.Equal("base64", (string?)responses[0]["params"]!["body"]!["type"]);
        Assert.Equal(204, (int?)responses[1]["params"]!["statusCode"]);
        Assert.False(responses[1]["params"]!.AsObject().ContainsKey("body"));
        Assert.Equal("request-5", (string?)Assert.Single(session.RemoteEnd.CommandsFor("network.failRequest"))["params"]!["request"]);
    }

    [Fact]
    public async Task UndecidedOrFailedRequestsContinueAndFailuresAreReported()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        List<string> messages = [];
        TaskCompletionSource<bool> reported = new(TaskCreationOptions.RunContinuationsAsynchronously);
        group.OnLogMessage.AddObserver(e =>
        {
            messages.Add(e.Message);
            if (messages.Count == 2)
            {
                reported.TrySetResult(true);
            }
        });
        await page.RouteAsync(_ => throw new InvalidOperationException("matcher broke"), _ => Task.CompletedTask, cancellationToken: TestContext.Current.CancellationToken);
        session.RemoteEnd.AnswerWith("network.continueRequest", _ => FakeResponse.Failure("no such request", "gone"));

        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", BlockedRequest(page.Id, InterceptIds(session)));
        await reported.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);

        Assert.Equal($"The route for requests satisfying the condition failed handling a request to {RequestUrl}: matcher broke", messages[0]);
        Assert.StartsWith($"A request to {RequestUrl} could not be continued: ", messages[1]);
        Assert.Single(session.RemoteEnd.CommandsFor("network.continueRequest"));
    }

    [Fact]
    public async Task RequestsStoppedByOthersAreLeftAloneAndThoseOfUntrackedContextsContinue()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        List<string> tried = [];
        await page.RouteAsync(RequestUrl, _ => { tried.Add("route"); return Task.CompletedTask; }, cancellationToken: TestContext.Current.CancellationToken);
        TaskCompletionSource<string> continued = new(TaskCreationOptions.RunContinuationsAsynchronously);
        group.OnLogMessage.AddObserver(e => continued.TrySetResult(e.Message));

        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", BlockedRequest(page.Id, ["someone-elses-intercept"], "request-other"));
        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", NotBlockedRequest(page.Id));
        JsonObject blockedWithoutIntercepts = BlockedRequest(page.Id, [], "request-no-intercepts");
        blockedWithoutIntercepts.Remove("intercepts");
        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", blockedWithoutIntercepts);
        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", BlockedRequest(null, InterceptIds(session), "request-worker"));
        string message = await continued.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);

        Assert.Equal($"A request to {RequestUrl} that no route decided was continued as it was.", message);
        Assert.Empty(tried);
        Assert.Equal("request-worker", (string?)Assert.Single(session.RemoteEnd.CommandsFor("network.continueRequest"))["params"]!["request"]);
    }

    [Fact]
    public async Task RemovedRoutesStopHandlingAndTheirInterceptsAreRemoved()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        RouteRegistration first = await page.RouteAsync(RequestUrl, _ => Task.CompletedTask, cancellationToken: TestContext.Current.CancellationToken);
        await page.RouteAsync(RequestUrl, _ => Task.CompletedTask, cancellationToken: TestContext.Current.CancellationToken);
        await page.RouteAsync(RequestUrl, _ => Task.CompletedTask, cancellationToken: TestContext.Current.CancellationToken);
        string[] interceptIds = InterceptIds(session);

        await first.RemoveAsync(TestContext.Current.CancellationToken);
        await page.UnrouteAllAsync(TestContext.Current.CancellationToken);
        await group.DisposeAsync();

        Assert.Equal(interceptIds.OrderBy(id => id), session.RemoteEnd.CommandsFor("network.removeIntercept").Select(command => (string)command["params"]!["intercept"]!).OrderBy(id => id));
    }

    [Fact]
    public async Task DisposalRemovesInterceptsAndTheNetworkSubscriptionOfASessionItDoesNotOwn()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        await page.RouteAsync(RequestUrl, _ => Task.CompletedTask, cancellationToken: TestContext.Current.CancellationToken);
        string networkSubscription = (string)session.RemoteEnd.ResultsFor("session.subscribe").Last()["subscription"]!;

        await group.DisposeAsync();

        Assert.Equal(InterceptIds(session), session.RemoteEnd.CommandsFor("network.removeIntercept").Select(command => (string)command["params"]!["intercept"]!));
        Assert.Contains(session.RemoteEnd.CommandsFor("session.unsubscribe"), command => command["params"]!["subscriptions"]!.AsArray().Any(id => (string?)id == networkSubscription));
    }

    [Fact]
    public async Task RunAndWaitReturnsTheFirstMatchingRequestOrResponseOfThePage()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        BeforeRequestSentEventArgs byUrl = await page.RunAndWaitForRequestAsync(() => RaiseRequestsAsync(session, page.Id), RequestUrl, cancellationToken: TestContext.Current.CancellationToken);
        BeforeRequestSentEventArgs byPattern = await page.RunAndWaitForRequestAsync(() => RaiseRequestsAsync(session, page.Id), new Regex("data\\.json$"), cancellationToken: TestContext.Current.CancellationToken);
        BeforeRequestSentEventArgs byCondition = await page.RunAndWaitForRequestAsync(() => RaiseRequestsAsync(session, page.Id), request => request.Method == "post", cancellationToken: TestContext.Current.CancellationToken);
        ResponseCompletedEventArgs response = await page.RunAndWaitForResponseAsync(() => RaiseResponseAsync(session, page.Id), RequestUrl, cancellationToken: TestContext.Current.CancellationToken);
        ResponseCompletedEventArgs responseByPattern = await page.RunAndWaitForResponseAsync(() => RaiseResponseAsync(session, page.Id), new Regex("api"), cancellationToken: TestContext.Current.CancellationToken);
        ResponseCompletedEventArgs responseByCondition = await page.RunAndWaitForResponseAsync(() => RaiseResponseAsync(session, page.Id), request => request.RequestId == "request-1", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("request-1", byUrl.Request.RequestId);
        Assert.Equal("request-1", byPattern.Request.RequestId);
        Assert.Equal("request-2", byCondition.Request.RequestId);
        Assert.Equal(200UL, response.Response.Status);
        Assert.Equal("request-1", responseByPattern.Request.RequestId);
        Assert.Equal("request-1", responseByCondition.Request.RequestId);
    }

    [Fact]
    public async Task RunAndWaitTimesOutOrFailsWithTheCondition()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Task<BeforeRequestSentEventArgs> never = page.RunAndWaitForRequestAsync(() => Task.CompletedTask, RequestUrl, TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException timeout = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, never));
        InvalidOperationException broken = await Assert.ThrowsAsync<InvalidOperationException>(() => page.RunAndWaitForResponseAsync(() => RaiseResponseAsync(session, page.Id), _ => throw new InvalidOperationException("condition broke"), cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal($"Timed out after 1 seconds waiting for a request to {RequestUrl}.", timeout.Message);
        Assert.Equal("condition broke", broken.Message);
    }

    [Fact]
    public async Task EventsOfUntrackedContextsAreNotOffered()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Task<ResponseCompletedEventArgs> wait = page.RunAndWaitForResponseAsync(() => RaiseResponseAsync(session, "untracked-context"), RequestUrl, TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, wait));
    }

    [Fact]
    public async Task BrowserRoutesAddAnInterceptForEveryPage()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        RouteRegistration exact = await page.Browser.RouteAsync(RequestUrl, _ => Task.CompletedTask, cancellationToken: TestContext.Current.CancellationToken);
        RouteRegistration matching = await page.Browser.RouteAsync(new Regex("/api/"), _ => Task.CompletedTask, new UrlPatternPattern() { HostName = "example.com" }, TestContext.Current.CancellationToken);
        RouteRegistration satisfying = await page.Browser.RouteAsync(_ => true, _ => Task.CompletedTask, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([RequestUrl, "URLs matching /api/", "requests satisfying the condition"], new[] { exact, matching, satisfying }.Select(route => route.Description));
        IReadOnlyList<JsonObject> intercepts = session.RemoteEnd.CommandsFor("network.addIntercept");
        Assert.All(intercepts, intercept => Assert.False(intercept["params"]!.AsObject().ContainsKey("contexts")));
        Assert.Equal("example.com", (string?)intercepts[1]["params"]!["urlPatterns"]![0]!["hostname"]);
    }

    [Fact]
    public async Task PageRoutesAreTriedBeforeBrowserRoutesEachNewestFirst()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        List<string> tried = [];
        TaskCompletionSource<Route> fulfilled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await page.Browser.RouteAsync(RequestUrl, async route =>
        {
            tried.Add("older browser route");
            await route.FulfillAsync(200, "{}");
            fulfilled.TrySetResult(route);
        }, cancellationToken: TestContext.Current.CancellationToken);
        await page.Browser.RouteAsync(RequestUrl, _ => { tried.Add("newer browser route"); return Task.CompletedTask; }, cancellationToken: TestContext.Current.CancellationToken);
        await page.RouteAsync(RequestUrl, _ => { tried.Add("page route"); return Task.CompletedTask; }, cancellationToken: TestContext.Current.CancellationToken);

        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", BlockedRequest(page.Id, InterceptIds(session)));
        Route route = await fulfilled.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);

        Assert.Equal(["page route", "newer browser route", "older browser route"], tried);
        Assert.Same(page.Browser, route.Browser);
        Assert.Same(page, route.Page);
        Assert.Same(page.MainFrame, route.Frame);
    }

    [Fact]
    public async Task RequestsOfAnotherBrowserStoppedByABrowserRouteContinueAtOnce()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        Browser other = await group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page otherPage = await other.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        List<string> tried = [];
        await page.Browser.RouteAsync(RequestUrl, _ => { tried.Add("route"); return Task.CompletedTask; }, cancellationToken: TestContext.Current.CancellationToken);

        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", BlockedRequest(otherPage.Id, InterceptIds(session)));
        JsonObject continued = await session.RemoteEnd.WaitForCommandAsync("network.continueRequest");

        Assert.Equal("request-1", (string?)continued["params"]!["request"]);
        Assert.Empty(tried);
    }

    [Fact]
    public async Task RequestsOfNoPageGoToTheBrowserOfTheirUserContext()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        TaskCompletionSource<Route> routed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await page.Browser.RouteAsync(RequestUrl, async route =>
        {
            await route.AbortAsync();
            routed.TrySetResult(route);
        }, cancellationToken: TestContext.Current.CancellationToken);
        JsonObject worker = BlockedRequest(null, InterceptIds(session), "request-worker");
        worker["userContext"] = page.Browser.Id;
        JsonObject unknown = BlockedRequest(null, InterceptIds(session), "request-unknown-browser");
        unknown["userContext"] = "no-such-user-context";

        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", unknown);
        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", worker);
        Route route = await routed.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);

        Assert.Same(page.Browser, route.Browser);
        Assert.Null(route.Page);
        Assert.Null(route.Frame);
        Assert.Equal("request-worker", (string?)Assert.Single(session.RemoteEnd.CommandsFor("network.failRequest"))["params"]!["request"]);
        Assert.Equal("request-unknown-browser", (string?)Assert.Single(session.RemoteEnd.CommandsFor("network.continueRequest"))["params"]!["request"]);
    }

    [Fact]
    public async Task HandlerThatDecidesThenThrowsIsReportedWithoutContinuing()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        TaskCompletionSource<string> reported = new(TaskCreationOptions.RunContinuationsAsynchronously);
        group.OnLogMessage.AddObserver(e => reported.TrySetResult(e.Message));
        await page.Browser.RouteAsync(RequestUrl, async route =>
        {
            await route.AbortAsync();
            throw new InvalidOperationException("handler broke");
        }, cancellationToken: TestContext.Current.CancellationToken);

        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", BlockedRequest(page.Id, InterceptIds(session)));
        string message = await reported.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);
        await driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal($"The route for {RequestUrl} failed handling a request to {RequestUrl}: handler broke", message);
        Assert.Single(session.RemoteEnd.CommandsFor("network.failRequest"));
        Assert.Empty(session.RemoteEnd.CommandsFor("network.continueRequest"));
    }

    [Fact]
    public async Task BrowserRoutesAreRemovedByRemovalUnroutingOrClosingTheBrowser()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        Browser created = await group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);
        RouteRegistration removed = await page.Browser.RouteAsync(RequestUrl, _ => Task.CompletedTask, cancellationToken: TestContext.Current.CancellationToken);
        await page.Browser.RouteAsync(RequestUrl, _ => Task.CompletedTask, cancellationToken: TestContext.Current.CancellationToken);
        await page.RouteAsync(RequestUrl, _ => Task.CompletedTask, cancellationToken: TestContext.Current.CancellationToken);
        await created.RouteAsync(RequestUrl, _ => Task.CompletedTask, cancellationToken: TestContext.Current.CancellationToken);
        string[] interceptIds = InterceptIds(session);

        await removed.RemoveAsync(TestContext.Current.CancellationToken);
        await page.Browser.UnrouteAllAsync(TestContext.Current.CancellationToken);
        await created.CloseAsync(TestContext.Current.CancellationToken);

        // The page's own route is not the browser's to remove.
        Assert.Equal([interceptIds[0], interceptIds[1], interceptIds[3]], session.RemoteEnd.CommandsFor("network.removeIntercept").Select(command => (string)command["params"]!["intercept"]!));
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, BrowserGroup Group, Page Page, FakeTimeProvider Time)> OpenPageAsync()
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new DramaturgeOptions() { TimeProvider = time }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, group, page, time);
    }

    private static string[] InterceptIds(FakeSession session)
    {
        return [.. session.RemoteEnd.ResultsFor("network.addIntercept").Select(result => (string)result["intercept"]!)];
    }

    private static JsonObject Request(string requestId, string url, string method)
    {
        return new JsonObject()
        {
            ["request"] = requestId,
            ["url"] = url,
            ["method"] = method,
            ["headers"] = new JsonArray(),
            ["cookies"] = new JsonArray(),
            ["destination"] = "",
            ["initiatorType"] = "fetch",
            ["headersSize"] = 0,
            ["bodySize"] = 0,
            ["timings"] = new JsonObject()
            {
                ["timeOrigin"] = 0, ["requestTime"] = 0, ["redirectStart"] = 0, ["redirectEnd"] = 0, ["fetchStart"] = 0, ["dnsStart"] = 0, ["dnsEnd"] = 0,
                ["connectStart"] = 0, ["connectEnd"] = 0, ["tlsStart"] = 0, ["requestStart"] = 0, ["responseStart"] = 0, ["responseEnd"] = 0,
            },
        };
    }

    private static JsonObject Event(string? contextId, bool isBlocked, string[]? intercepts, JsonObject request)
    {
        JsonObject parameters = new() { ["context"] = contextId, ["navigation"] = null, ["isBlocked"] = isBlocked, ["redirectCount"] = 0, ["timestamp"] = 1790000000000, ["request"] = request };
        if (intercepts is not null)
        {
            parameters["intercepts"] = new JsonArray([.. intercepts.Select(id => (JsonNode)id)]);
        }

        return parameters;
    }

    private static JsonObject BlockedRequest(string? contextId, string[] intercepts, string requestId = "request-1")
    {
        JsonObject parameters = Event(contextId, true, intercepts, Request(requestId, RequestUrl, "get"));
        parameters["initiator"] = new JsonObject() { ["type"] = "script" };
        return parameters;
    }

    private static JsonObject NotBlockedRequest(string contextId)
    {
        JsonObject parameters = Event(contextId, false, null, Request("request-plain", RequestUrl, "get"));
        parameters["initiator"] = new JsonObject() { ["type"] = "script" };
        return parameters;
    }

    private static async Task RaiseRequestsAsync(FakeSession session, string contextId)
    {
        JsonObject first = Event(contextId, false, null, Request("request-1", RequestUrl, "get"));
        first["initiator"] = new JsonObject() { ["type"] = "script" };
        JsonObject second = Event(contextId, false, null, Request("request-2", "https://example.com/submit", "post"));
        second["initiator"] = new JsonObject() { ["type"] = "script" };
        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", first);
        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", second);
    }

    private static Task RaiseResponseAsync(FakeSession session, string contextId)
    {
        JsonObject parameters = Event(contextId, false, null, Request("request-1", RequestUrl, "get"));
        parameters["response"] = new JsonObject()
        {
            ["url"] = RequestUrl, ["protocol"] = "http/1.1", ["status"] = 200, ["statusText"] = "OK", ["fromCache"] = false, ["headers"] = new JsonArray(),
            ["mimeType"] = "application/json", ["bytesReceived"] = 2, ["headersSize"] = 0, ["bodySize"] = 2, ["content"] = new JsonObject() { ["size"] = 2 },
        };
        return session.RemoteEnd.RaiseEventAsync("network.responseCompleted", parameters);
    }

    private static async Task<T> DriveAsync<T>(FakeTimeProvider time, Task<T> operation)
    {
        while (!operation.IsCompleted)
        {
            time.Advance(TimeSpan.FromMilliseconds(100));
            await Task.WhenAny(operation, Task.Delay(5, TestContext.Current.CancellationToken));
        }

        return await operation;
    }
}
