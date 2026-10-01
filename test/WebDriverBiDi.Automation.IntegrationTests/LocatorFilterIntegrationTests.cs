// <copyright file="LocatorFilterIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using Dramaturge.Browsers;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.BrowsingContext;

public class LocatorFilterIntegrationTests
{
    // Text is compared ignoring case and whitespace runs, includes open shadow roots, and skips scripts.
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task TextFiltersReadTheRenderedText(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("filters.html"), cancellationToken: TestContext.Current.CancellationToken);
        ElementLocator items = page.Locate(new CssLocator("#fruit li"));

        Assert.Equal(2, await items.Filter(hasText: "apple").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await items.Filter(hasText: "green apple").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await items.Filter(hasText: "SHADOW FRUIT").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(3, await items.Filter(hasNotText: "apple").CountAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ElementFiltersAndCombinationsNarrowOrWidenTheMatches(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("filters.html"), cancellationToken: TestContext.Current.CancellationToken);
        ElementLocator items = page.Locate(new CssLocator("#fruit li"));
        ElementLocator badge = page.Locate(new CssLocator(".badge"));
        ElementLocator buttons = page.Locate(new CssLocator("button"));
        ElementLocator primary = page.Locate(new CssLocator(".primary"));

        Assert.Equal(1, await items.Filter(has: badge).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(4, await items.Filter(hasNot: badge).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await buttons.And(primary).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(3, await buttons.Or(primary).CountAsync(TestContext.Current.CancellationToken));
        Assert.True(await items.Filter(hasText: "banana").IsVisibleAsync(TestContext.Current.CancellationToken));
    }
}
