// <copyright file="CookieIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.Browsers;

public class CookieIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task CookiesAreAddedReadAndCleared(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Browser browser = group.DefaultBrowser;
        CancellationToken token = TestContext.Current.CancellationToken;

        await browser.AddCookiesAsync([new BrowserCookie("flavor", "oatmeal", "localhost") { Path = "/" }], token);
        Page page = await browser.NewPageAsync(cancellationToken: token);
        await page.NavigateAsync(server.UrlFor("index.html"), cancellationToken: token);
        string seenByPage = await page.EvaluateAsync<string>("() => document.cookie", cancellationToken: token);
        await page.EvaluateAsync("() => { document.cookie = 'fromPage=yes; path=/'; }", cancellationToken: token);
        IReadOnlyList<BrowserCookie> all = await browser.GetCookiesAsync(cancellationToken: token);
        IReadOnlyList<BrowserCookie> named = await browser.GetCookiesAsync(name: "fromPage", cancellationToken: token);
        await browser.ClearCookiesAsync(name: "flavor", cancellationToken: token);
        IReadOnlyList<BrowserCookie> afterNamedClear = await browser.GetCookiesAsync(cancellationToken: token);
        await browser.ClearCookiesAsync(cancellationToken: token);
        IReadOnlyList<BrowserCookie> afterClear = await browser.GetCookiesAsync(cancellationToken: token);

        Assert.Contains("flavor=oatmeal", seenByPage);
        Assert.Contains(all, cookie => cookie is { Name: "flavor", Value: "oatmeal", Path: "/" });
        Assert.Contains(all, cookie => cookie is { Name: "fromPage", Value: "yes" });
        BrowserCookie fromPage = Assert.Single(named);
        Assert.Equal("localhost", fromPage.Domain.TrimStart('.'));
        Assert.Equal(["fromPage"], afterNamedClear.Select(cookie => cookie.Name));
        Assert.Empty(afterClear);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task BrowsersKeepTheirOwnCookies(BrowserKind browserKind)
    {
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Browser other = await group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);

        await other.AddCookiesAsync([new BrowserCookie("private", "1", "localhost") { Path = "/", Expires = DateTime.UtcNow.AddDays(1) }], TestContext.Current.CancellationToken);
        IReadOnlyList<BrowserCookie> own = await other.GetCookiesAsync(cancellationToken: TestContext.Current.CancellationToken);
        IReadOnlyList<BrowserCookie> defaults = await group.DefaultBrowser.GetCookiesAsync(cancellationToken: TestContext.Current.CancellationToken);

        BrowserCookie cookie = Assert.Single(own);
        Assert.NotNull(cookie.Expires);
        Assert.DoesNotContain(defaults, candidate => candidate.Name == "private");
    }
}
