// <copyright file="GetByHelperIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi.BrowsingContext;

public class GetByHelperIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task AttributeHelpersMatchContainingTextIgnoringCaseOrExactly(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("getby.html"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, await page.GetByPlaceholder("email").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByPlaceholder("Email address", exact: true).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await page.GetByPlaceholder("email address", exact: true).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByAltText("LOGO").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByTitle("help").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByTestId("cancel-button").CountAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task RoleHelperUsesTheBrowsersOwnComputation(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("getby.html"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(3, await page.GetByRole("button").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByRole("button", "Save").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByRole("link", "Home").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByRole("heading", "Welcome").CountAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task TextHelperUsesTheBrowsersOwnComputation(BrowserKind browserKind)
    {
        Assert.SkipWhen(browserKind == BrowserKind.Firefox, "Firefox does not yet support the innerText locator (https://bugzilla.mozilla.org/show_bug.cgi?id=1869538).");
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("getby.html"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, await page.GetByText("Sign in to continue", exact: true).CountAsync(TestContext.Current.CancellationToken));
        Assert.True(await page.GetByText("SIGN IN").IsVisibleAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task HelpersOnALocatorSearchWithinIt(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("getby.html"), cancellationToken: TestContext.Current.CancellationToken);
        ElementLocator form = page.Locate(new CssLocator("#signin"));

        Assert.Equal(2, await form.GetByRole("button").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await form.GetByRole("link").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await form.GetByPlaceholder("password").CountAsync(TestContext.Current.CancellationToken));
    }
}
