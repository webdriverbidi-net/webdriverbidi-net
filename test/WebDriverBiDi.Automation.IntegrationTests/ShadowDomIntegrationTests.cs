// <copyright file="ShadowDomIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.Browsers;
using WebDriverBiDi.BrowsingContext;

public class ShadowDomIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ShadowRootStepReachesOpenAndClosedRoots(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("shadow-dom.html"), cancellationToken: TestContext.Current.CancellationToken);
        ElementLocator openRoot = page.Locate(new CssLocator("#open-host")).ShadowRoot();
        ElementLocator closedRoot = page.Locate(new CssLocator("#closed-host")).ShadowRoot();

        Assert.Equal(1, await page.Locate(new CssLocator("button")).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await openRoot.Locate(new CssLocator("button")).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await openRoot.Locate(new CssLocator("#nested-host")).ShadowRoot().Locate(new CssLocator("button")).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await closedRoot.Locate(new CssLocator("button")).CountAsync(TestContext.Current.CancellationToken));
        Assert.True(await closedRoot.Locate(new CssLocator("button")).IsVisibleAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await page.Locate(new CssLocator("body")).ShadowRoot().CountAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task PiercingSearchesOpenRootsButNotClosedOnes(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind, new AutomationOptions() { PierceShadowRoots = true });
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("shadow-dom.html"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(3, await page.Locate(new CssLocator("button")).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByLabel("shadow email").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.Locate(new CssLocator("#open-host")).Locate(new CssLocator("#nested-host")).Locate(new CssLocator("button")).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.Locate(new CssLocator("#closed-host")).ShadowRoot().Locate(new CssLocator("button")).CountAsync(TestContext.Current.CancellationToken));
    }

    // The protocol's accessibility locator walks each start node's children, so without piercing it stays outside
    // shadow roots, and with piercing it reaches the open ones.
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task RoleLookupsFollowPiercing(BrowserKind browserKind)
    {
        Assert.SkipWhen(browserKind == BrowserKind.Chrome, "Chrome's accessibility locator walks the accessibility tree, so it finds elements in every shadow root, closed ones included, whether or not lookups pierce.");
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup plain = await TestBrowsers.LaunchAsync(browserKind);
        Page plainPage = await plain.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await plainPage.NavigateAsync(server.UrlFor("shadow-dom.html"), cancellationToken: TestContext.Current.CancellationToken);
        await using BrowserGroup piercing = await TestBrowsers.LaunchAsync(browserKind, new AutomationOptions() { PierceShadowRoots = true });
        Page piercingPage = await piercing.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await piercingPage.NavigateAsync(server.UrlFor("shadow-dom.html"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, await plainPage.GetByRole("button").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(3, await piercingPage.GetByRole("button").CountAsync(TestContext.Current.CancellationToken));
    }
}
