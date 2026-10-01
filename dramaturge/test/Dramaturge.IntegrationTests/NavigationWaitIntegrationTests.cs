// <copyright file="NavigationWaitIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.RegularExpressions;
using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;

public class NavigationWaitIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task NewAndNavigatedPagesAreLoaded(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);

        await page.WaitForLoadStateAsync(timeout: TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("index.html"), cancellationToken: TestContext.Current.CancellationToken);
        await page.WaitForLoadStateAsync(timeout: TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task LoadStateWaitsTellAParsedDocumentFromALoadedOne(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);

        await page.NavigateAsync(server.UrlFor("gated.html"), ReadinessState.Interactive, cancellationToken: TestContext.Current.CancellationToken);
        await page.WaitForLoadStateAsync(ReadinessState.Interactive, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => page.WaitForLoadStateAsync(timeout: TimeSpan.FromSeconds(1), cancellationToken: TestContext.Current.CancellationToken));
        server.ReleaseGatedResource();
        await page.WaitForLoadStateAsync(timeout: TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Timed out after 1 seconds waiting for the frame's document to be complete; it was interactive.", exception.Message);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task RunAndWaitForNavigationWaitsForNewDocumentsAndChangesWithinOne(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("navigation.html"), cancellationToken: TestContext.Current.CancellationToken);

        string fragment = await page.RunAndWaitForNavigationAsync(() => page.Locate(new CssLocator("#fragment")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken), cancellationToken: TestContext.Current.CancellationToken);
        string pushed = await page.RunAndWaitForNavigationAsync(() => page.Locate(new CssLocator("#push")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken), cancellationToken: TestContext.Current.CancellationToken);
        string next = await page.RunAndWaitForNavigationAsync(() => page.Locate(new CssLocator("#next")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(server.UrlFor("navigation.html#part"), fragment);
        Assert.Equal(server.UrlFor("pushed"), pushed);
        Assert.Equal(server.UrlFor("second.html"), next);
        Assert.Equal(next, page.Url);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task WaitForUrlWaitsForANavigationTheActionStartsLater(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("navigation.html"), cancellationToken: TestContext.Current.CancellationToken);

        await page.Locate(new CssLocator("#later")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        string url = await page.WaitForUrlAsync(new Regex("second\\.html$"), cancellationToken: TestContext.Current.CancellationToken);
        string again = await page.WaitForUrlAsync(server.UrlFor("second.html"), cancellationToken: TestContext.Current.CancellationToken);
        string matched = await page.WaitForUrlAsync(candidate => candidate.EndsWith("/second.html", StringComparison.Ordinal), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(server.UrlFor("second.html"), url);
        Assert.Equal(url, again);
        Assert.Equal(url, matched);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task BackAndForwardWaitForTheDocumentTheyReturnTo(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("navigation.html"), cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("second.html"), cancellationToken: TestContext.Current.CancellationToken);

        string back = await page.GoBackAsync(timeout: TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
        string forward = await page.GoForwardAsync(timeout: TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(server.UrlFor("navigation.html"), back);
        Assert.Equal(server.UrlFor("second.html"), forward);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task FramesNavigateAndWaitOnTheirOwn(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("frames.html"), cancellationToken: TestContext.Current.CancellationToken);
        Frame frame = await page.Locate(new CssLocator("#child")).ContentFrameAsync(cancellationToken: TestContext.Current.CancellationToken);

        string url = await frame.NavigateAsync(server.UrlFor("second.html"), cancellationToken: TestContext.Current.CancellationToken);
        await frame.WaitForUrlAsync(server.UrlFor("second.html"), timeout: TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(server.UrlFor("second.html"), url);
        Assert.Equal(server.UrlFor("frames.html"), page.Url);
    }
}
