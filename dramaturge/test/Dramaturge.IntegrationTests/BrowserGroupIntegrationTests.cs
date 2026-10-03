// <copyright file="BrowserGroupIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

public class BrowserGroupIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task LaunchedGroupFindsTheDefaultBrowserAndItsPage(BrowserKind browserKind)
    {
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);

        // Firefox also reports its container tabs (Personal, Work, and so on) as user contexts.
        Assert.Equal(Browser.DefaultBrowserId, group.Browsers[0].Id);
        Assert.Same(group.Browsers[0], group.DefaultBrowser);
        Assert.Single(group.DefaultBrowser.Pages);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task HeadlessChromeStartsWithoutAPage(BrowserKind browserKind)
    {
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind, configure: launcher => launcher.WithHeadlessOption());
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page another = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Firefox always keeps one tab open; headless Chrome needs none, and keeps running with none.
        Assert.Equal(browserKind == BrowserKind.Firefox ? 2 : 1, group.DefaultBrowser.Pages.Count);
        Assert.Contains(another, group.DefaultBrowser.Pages);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task CreatedBrowserAppliesItsViewportToNewPages(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);

        Browser browser = await group.CreateBrowserAsync(new BrowserOptions() { Viewport = new Viewport() { Width = 640, Height = 480 } }, TestContext.Current.CancellationToken);
        Page page = await browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("index.html"), cancellationToken: TestContext.Current.CancellationToken);
        NumberRemoteValue width = await group.Driver.Script.CallFunctionAsync<NumberRemoteValue>(page.Id, "() => window.innerWidth", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(640, width.Value);
        Assert.Contains(browser, group.Browsers);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task NavigationTracksTheUrlAndFrames(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);

        string url = await page.NavigateAsync(server.UrlFor("frames.html"), cancellationToken: TestContext.Current.CancellationToken);
        await TestBrowsers.WaitUntilAsync(() => page.Frames.Count == 2 && page.Frames[1].Url == server.UrlFor("child.html"), "the frame to load", () => $"Frames: {string.Join(", ", page.Frames.Select(f => $"{f.Id}={f.Url}"))}; pages: {string.Join(", ", group.Browsers.SelectMany(b => b.Pages).Select(p => $"{p.Id}={p.Url}"))}");

        Assert.Equal(server.UrlFor("frames.html"), url);
        Assert.Equal(url, page.Url);
        Assert.Same(page.MainFrame, page.Frames[1].ParentFrame);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task HistoryChangesTrackTheUrl(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("index.html"), cancellationToken: TestContext.Current.CancellationToken);

        await group.Driver.Script.CallFunctionAsync(page.Id, "() => history.pushState({}, '', 'pushed.html')", cancellationToken: TestContext.Current.CancellationToken);
        await TestBrowsers.WaitUntilAsync(() => page.Url == server.UrlFor("pushed.html"), "the pushed URL");
        await group.Driver.Script.CallFunctionAsync(page.Id, "() => { location.hash = 'section'; }", cancellationToken: TestContext.Current.CancellationToken);
        await TestBrowsers.WaitUntilAsync(() => page.Url == server.UrlFor("pushed.html#section"), "the fragment URL");
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task BackAndForwardMoveThroughHistory(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("index.html"), cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("second.html"), cancellationToken: TestContext.Current.CancellationToken);

        await page.GoBackAsync(cancellationToken: TestContext.Current.CancellationToken);
        await TestBrowsers.WaitUntilAsync(() => page.Url == server.UrlFor("index.html"), "the previous page");
        await page.GoForwardAsync(cancellationToken: TestContext.Current.CancellationToken);
        await TestBrowsers.WaitUntilAsync(() => page.Url == server.UrlFor("second.html"), "the next page");
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ClosedPagesAndBrowsersAreRemoved(BrowserKind browserKind)
    {
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Browser browser = await group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page page = await browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page otherPage = await browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);

        await page.CloseAsync(TestContext.Current.CancellationToken);
        bool otherPageOpenAfterFirstClose = !otherPage.IsClosed;
        await browser.CloseAsync(TestContext.Current.CancellationToken);
        await TestBrowsers.WaitUntilAsync(() => otherPage.IsClosed, "the browser's pages to close");

        Assert.True(page.IsClosed);
        Assert.True(otherPageOpenAfterFirstClose);
        Assert.DoesNotContain(browser, group.Browsers);
    }
}
