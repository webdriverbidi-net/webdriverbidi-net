// <copyright file="ElementLocatorIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using Dramaturge.Browsers;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

public class ElementLocatorIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task LocatorsCountAndReadVisibility(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("elements.html"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(3, await page.Locate(new CssLocator("li")).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.Locate(new XPathLocator("//button")).CountAsync(TestContext.Current.CancellationToken));
        Assert.True(await page.Locate(new CssLocator("#shown")).IsVisibleAsync(TestContext.Current.CancellationToken));
        Assert.False(await page.Locate(new CssLocator("#hidden")).IsVisibleAsync(TestContext.Current.CancellationToken));
        Assert.False(await page.Locate(new CssLocator("#missing")).IsVisibleAsync(TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<AmbiguousElementException>(() => page.Locate(new CssLocator("li")).IsVisibleAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task WaitFindsAnElementAddedLater(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("elements.html"), ReadinessState.Interactive, cancellationToken: TestContext.Current.CancellationToken);
        ElementLocator late = page.Locate(new CssLocator("#late"));

        await late.WaitForAsync(timeout: TimeSpan.FromSeconds(10), cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(await late.IsVisibleAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task WaitFollowsVisibilityChanges(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("elements.html"), cancellationToken: TestContext.Current.CancellationToken);
        ElementLocator hidden = page.Locate(new CssLocator("#hidden"));

        await hidden.WaitForAsync(ElementState.Hidden, cancellationToken: TestContext.Current.CancellationToken);
        await hidden.WaitForAsync(ElementState.Attached, cancellationToken: TestContext.Current.CancellationToken);
        await group.Driver.Script.CallFunctionAsync(page.Id, "() => document.getElementById('reveal').click()", cancellationToken: TestContext.Current.CancellationToken);
        await hidden.WaitForAsync(ElementState.Visible, TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        await group.Driver.Script.CallFunctionAsync(page.Id, "() => document.getElementById('shown').remove()", cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#shown")).WaitForAsync(ElementState.Detached, cancellationToken: TestContext.Current.CancellationToken);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task WaitThatTimesOutSaysWhy(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("elements.html"), cancellationToken: TestContext.Current.CancellationToken);

        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => page.Locate(new CssLocator("#hidden")).WaitForAsync(timeout: TimeSpan.FromSeconds(1), cancellationToken: TestContext.Current.CancellationToken));

        Assert.EndsWith("css \"#hidden\" to be visible; the element was hidden.", exception.Message);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task WaitContinuesAcrossANavigation(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("index.html"), cancellationToken: TestContext.Current.CancellationToken);

        Task wait = page.Locate(new CssLocator("#late")).WaitForAsync(timeout: TimeSpan.FromSeconds(10), cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("elements.html"), ReadinessState.None, cancellationToken: TestContext.Current.CancellationToken);
        await wait;
    }

    // The group's preload script reaches documents loaded after the group started; the page open before it
    // started has the library installed on first use.
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task LibraryWorksInDocumentsLoadedBeforeAndAfterTheGroupStarted(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page initialPage = group.DefaultBrowser.Pages[0];

        Exception? before = await Record.ExceptionAsync(() => initialPage.Locate(new CssLocator("body")).IsVisibleAsync(TestContext.Current.CancellationToken));
        await initialPage.NavigateAsync(server.UrlFor("elements.html"), cancellationToken: TestContext.Current.CancellationToken);
        bool shownVisibleAfter = await initialPage.Locate(new CssLocator("#shown")).IsVisibleAsync(TestContext.Current.CancellationToken);

        Assert.Null(before);
        Assert.True(shownVisibleAfter);
    }
}
