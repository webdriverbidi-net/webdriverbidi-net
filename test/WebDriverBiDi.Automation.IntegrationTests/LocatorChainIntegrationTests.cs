// <copyright file="LocatorChainIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.Browsers;
using WebDriverBiDi.BrowsingContext;

public class LocatorChainIntegrationTests
{
    // Leaf A is inside both #outer and #inner, so the browser finds it from each; it is counted once.
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ChainFindsEachElementOnceAcrossNestedMatches(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("nested.html"), cancellationToken: TestContext.Current.CancellationToken);

        int leaves = await page.Locate(new CssLocator(".section")).Locate(new CssLocator(".leaf")).CountAsync(TestContext.Current.CancellationToken);
        int innerLeaves = await page.Locate(new CssLocator("#inner")).Locate(new XPathLocator(".//span")).CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(3, leaves);
        Assert.Equal(1, innerLeaves);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task PositionsPickFromTheMatches(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("nested.html"), cancellationToken: TestContext.Current.CancellationToken);
        ElementLocator items = page.Locate(new CssLocator("#list")).Locate(new CssLocator("li"));

        Assert.True(await items.First().IsVisibleAsync(TestContext.Current.CancellationToken));
        Assert.True(await items.Nth(1).IsVisibleAsync(TestContext.Current.CancellationToken));
        Assert.False(await items.Last().IsVisibleAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await items.Nth(3).CountAsync(TestContext.Current.CancellationToken));
        await items.Last().WaitForAsync(ElementState.Hidden, cancellationToken: TestContext.Current.CancellationToken);
    }
}
