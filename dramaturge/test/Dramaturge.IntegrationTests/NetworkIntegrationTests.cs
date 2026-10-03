// <copyright file="NetworkIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Network;

public class NetworkIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task RoutesAnswerMatchingRequestsNewestFirstAndFallThrough(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        CancellationToken token = TestContext.Current.CancellationToken;

        await page.RouteAsync(new Regex("/api/"), route => route.FulfillAsync(200, "{\"from\":\"older\"}", new Dictionary<string, string>() { ["Content-Type"] = "application/json" }), cancellationToken: token);
        await page.RouteAsync(server.UrlFor("api/data.json"), _ => Task.CompletedTask, cancellationToken: token);
        string fulfilled = await ClickAndReadAsync(page, "#fetch-api", "api");
        RouteRegistration abort = await page.RouteAsync(request => request.Url.EndsWith("/data.json", StringComparison.Ordinal), route => route.AbortAsync(), cancellationToken: token);
        string aborted = await ClickAndReadAsync(page, "#fetch-api", "api");
        string untouched = await ClickAndReadAsync(page, "#fetch-file", "file");
        await abort.RemoveAsync(token);
        string afterRemoval = await ClickAndReadAsync(page, "#fetch-api", "api");

        Assert.Equal("{\"from\":\"older\"}", fulfilled);
        Assert.Equal("failed", aborted);
        Assert.Equal("server data", untouched);
        Assert.Equal("{\"from\":\"older\"}", afterRemoval);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task FiltersLimitWhatTheBrowserStops(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        CancellationToken token = TestContext.Current.CancellationToken;
        ConcurrentQueue<string> stopped = new();

        await page.RouteAsync(_ => true, route => { stopped.Enqueue(route.Request.Url); return route.ContinueAsync(); }, new UrlPatternPattern() { PathName = "/data.txt" }, token);
        string routed = await ClickAndReadAsync(page, "#fetch-file", "file");
        string unrouted = await ClickAndReadAsync(page, "#fetch-api", "api");
        await page.UnrouteAllAsync(token);
        string restored = await ClickAndReadAsync(page, "#fetch-file", "file");

        Assert.Equal("server data", routed);
        Assert.NotEqual("failed", unrouted);
        Assert.Equal("server data", restored);
        Assert.Equal([server.UrlFor("data.txt")], stopped);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ContinuedRequestCanGoToAnotherUrl(BrowserKind browserKind)
    {
        Assert.SkipWhen(browserKind == BrowserKind.Firefox, "Firefox sends a request continued with another URL and receives its response, but the page's fetch rejects it.");
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);

        await page.RouteAsync(server.UrlFor("data.txt"), route => route.ContinueAsync(new RouteOverrides() { Url = server.UrlFor("second.html") }), cancellationToken: TestContext.Current.CancellationToken);
        string redirected = await ClickAndReadAsync(page, "#fetch-file", "file");

        Assert.Contains("Second page", redirected);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task RequestWhoseHandlerThrowsContinuesAndIsReported(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        ConcurrentQueue<string> messages = new();
        group.OnLogMessage.AddObserver(e => messages.Enqueue(e.Message));

        await page.RouteAsync(server.UrlFor("data.txt"), _ => throw new InvalidOperationException("handler broke"), cancellationToken: TestContext.Current.CancellationToken);
        string file = await ClickAndReadAsync(page, "#fetch-file", "file");

        Assert.Equal("server data", file);
        Assert.Contains(messages, message => message.Contains("handler broke"));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task RunAndWaitReturnsTheRequestAndItsResponse(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        CancellationToken token = TestContext.Current.CancellationToken;

        BeforeRequestSentEventArgs request = await page.RunAndWaitForRequestAsync(() => page.Locate(new CssLocator("#fetch-file")).ClickAsync(cancellationToken: token), server.UrlFor("data.txt"), cancellationToken: token);
        ResponseCompletedEventArgs response = await page.RunAndWaitForResponseAsync(() => page.Locate(new CssLocator("#fetch-file")).ClickAsync(cancellationToken: token), new Regex("data\\.txt$"), cancellationToken: token);

        Assert.Equal("GET", request.Request.Method);
        Assert.Equal(page.Id, request.BrowsingContextId);
        Assert.Equal(200UL, response.Response.Status);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task BrowserRouteAnswersAPopupsFirstRequest(BrowserKind browserKind)
    {
        Assert.SkipWhen(browserKind == BrowserKind.Chrome, "Chrome neither stops nor reports a popup's first request, so no route can answer it.");
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        TaskCompletionSource<Page> opened = new(TaskCreationOptions.RunContinuationsAsynchronously);
        page.OnPopup.AddObserver(e => opened.TrySetResult(e.Page));
        ConcurrentQueue<Route> answered = new();
        await page.Browser.RouteAsync(server.UrlFor("routed.html"), route =>
        {
            answered.Enqueue(route);
            return route.FulfillAsync(200, "<!DOCTYPE html><title>Routed popup</title>", new Dictionary<string, string>() { ["Content-Type"] = "text/html" });
        }, cancellationToken: TestContext.Current.CancellationToken);

        await page.Locate(new CssLocator("#popup-routed")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page popup = await opened.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        await popup.WaitForUrlAsync(server.UrlFor("routed.html"), cancellationToken: TestContext.Current.CancellationToken);

        // The server has no routed.html, so the popup loaded only because the route answered its first request.
        Route route = Assert.Single(answered);
        Assert.Same(popup, route.Page);
        Assert.Same(page.Browser, route.Browser);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task BrowserRouteAnswersItsOwnPagesAndWorkersAndLeavesOtherBrowsersAlone(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        Browser other = await group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page otherPage = await other.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await otherPage.NavigateAsync(server.UrlFor("network.html"), cancellationToken: TestContext.Current.CancellationToken);
        await page.Browser.RouteAsync(server.UrlFor("data.txt"), route => route.FulfillAsync(200, "routed data", new Dictionary<string, string>() { ["Content-Type"] = "text/plain" }), cancellationToken: TestContext.Current.CancellationToken);

        string fromPage = await ClickAndReadAsync(page, "#fetch-file", "file");
        string fromWorker = await ClickAndReadAsync(page, "#fetch-worker", "worker");
        string fromOtherBrowser = await ClickAndReadAsync(otherPage, "#fetch-file", "file");

        Assert.Equal("routed data", fromPage);
        Assert.Equal("routed data", fromWorker);
        Assert.Equal("server data", fromOtherBrowser);
    }

    private static async Task<Page> OpenAsync(BrowserGroup group, TestPageServer server)
    {
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("network.html"), cancellationToken: TestContext.Current.CancellationToken);
        return page;
    }

    private static async Task<string> ClickAndReadAsync(Page page, string button, string result)
    {
        await page.Locate(new CssLocator(button)).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        return await page.WaitForFunctionAsync<string>($"() => window.{result}", timeout: TimeSpan.FromSeconds(10), cancellationToken: TestContext.Current.CancellationToken);
    }
}
