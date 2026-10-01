// <copyright file="ExpectIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Text.RegularExpressions;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.Browsers;
using WebDriverBiDi.BrowsingContext;
using static WebDriverBiDi.Automation.Assertions;

public class ExpectIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task StateExpectationsAreMet(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        CancellationToken token = TestContext.Current.CancellationToken;

        await Expect(Css(page, "#visible")).ToBeVisibleAsync(cancellationToken: token);
        await Expect(Css(page, "#invisible")).ToBeHiddenAsync(cancellationToken: token);
        await Expect(Css(page, "#missing")).ToBeHiddenAsync(cancellationToken: token);
        await Expect(Css(page, "#invisible")).ToBeAttachedAsync(cancellationToken: token);
        await Expect(Css(page, "#missing")).Not.ToBeAttachedAsync(cancellationToken: token);
        await Expect(Css(page, "#enabled")).ToBeEnabledAsync(cancellationToken: token);
        await Expect(Css(page, "#disabled")).ToBeDisabledAsync(cancellationToken: token);
        await Expect(Css(page, "#name")).ToBeEditableAsync(cancellationToken: token);
        await Expect(Css(page, "#readonly")).Not.ToBeEditableAsync(cancellationToken: token);
        await Expect(Css(page, "#check")).ToBeCheckedAsync(cancellationToken: token);
        await Expect(Css(page, "#mixed")).Not.ToBeCheckedAsync(cancellationToken: token);
        await Expect(Css(page, "#empty-input")).ToBeEmptyAsync(cancellationToken: token);
        await Expect(Css(page, "#empty-div")).ToBeEmptyAsync(cancellationToken: token);
        await Expect(Css(page, "#name")).Not.ToBeEmptyAsync(cancellationToken: token);
        await Expect(Css(page, "#with-child")).Not.ToBeEmptyAsync(cancellationToken: token);
        await Expect(Css(page, "#visible")).Not.ToBeEmptyAsync(cancellationToken: token);
        await Expect(Css(page, "#visible")).ToBeInViewportAsync(cancellationToken: token);
        await Expect(Css(page, "#far")).Not.ToBeInViewportAsync(cancellationToken: token);
        await Expect(Css(page, "li")).ToHaveCountAsync(3, cancellationToken: token);
        await Expect(Css(page, "#missing")).ToHaveCountAsync(0, cancellationToken: token);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Expect(Css(page, "#visible")).ToBeCheckedAsync(cancellationToken: token));
        await Assert.ThrowsAsync<AmbiguousElementException>(() => Expect(Css(page, "li")).ToBeVisibleAsync(cancellationToken: token));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task TextExpectationsAreMet(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        CancellationToken token = TestContext.Current.CancellationToken;

        await Expect(Css(page, "#spaced")).ToHaveTextAsync("Spaced out text", cancellationToken: token);
        await Expect(Css(page, "#spaced")).ToContainTextAsync("OUT", ignoreCase: true, cancellationToken: token);
        await Expect(Css(page, "#partly")).ToHaveTextAsync("Shown hidden text", cancellationToken: token);
        await Expect(Css(page, "#partly")).ToHaveTextAsync("Shown text", useInnerText: true, cancellationToken: token);
        await Expect(Css(page, "#partly")).ToHaveTextAsync(new Regex("^Shown"), cancellationToken: token);
        await Expect(Css(page, "li")).ToHaveTextAsync(["One", "Two", "Three"], cancellationToken: token);
        await Expect(Css(page, "li")).ToContainTextAsync(["One", "Three"], cancellationToken: token);
        await Expect(Css(page, "li")).Not.ToContainTextAsync(["Three", "One"], cancellationToken: token);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Expect(Css(page, "#svg-text")).ToHaveTextAsync("SVG", useInnerText: true, cancellationToken: token));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ValueAttributeAndCssExpectationsAreMet(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        CancellationToken token = TestContext.Current.CancellationToken;
        LocatorAssertions name = Expect(Css(page, "#name"));

        await name.ToHaveValueAsync("Ada", cancellationToken: token);
        await name.ToHaveAttributeAsync("data-role", "person", cancellationToken: token);
        await name.ToHaveAttributeAsync("DATA-ROLE", cancellationToken: token);
        await name.Not.ToHaveAttributeAsync("title", cancellationToken: token);
        await name.ToHaveIdAsync("name", cancellationToken: token);
        await name.ToHaveClassAsync("field wide", cancellationToken: token);
        await name.ToHaveClassAsync(new Regex(@"\bwide\b"), cancellationToken: token);
        await Expect(Css(page, "#styled")).ToHaveCssAsync("color", "rgb(255, 0, 0)", cancellationToken: token);
        await Css(page, "#name").FillAsync("Grace", cancellationToken: token);
        await name.ToHaveValueAsync("Grace", cancellationToken: token);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Expect(Css(page, "#visible")).ToHaveValueAsync("Visible", cancellationToken: token));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task FocusIsExpectedInTheDocumentOrAShadowRoot(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        CancellationToken token = TestContext.Current.CancellationToken;
        ElementLocator inner = Css(page, "#host").ShadowRoot().Locate(new CssLocator("#inner"));

        await Css(page, "#name").FocusAsync(cancellationToken: token);
        await Expect(Css(page, "#name")).ToBeFocusedAsync(cancellationToken: token);
        await Expect(Css(page, "#readonly")).Not.ToBeFocusedAsync(cancellationToken: token);
        await inner.FocusAsync(cancellationToken: token);
        await Expect(inner).ToBeFocusedAsync(cancellationToken: token);
        await Expect(Css(page, "#name")).Not.ToBeFocusedAsync(cancellationToken: token);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ExpectationWaitsForThePageToChange(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        CancellationToken token = TestContext.Current.CancellationToken;

        await Css(page, "#reveal").ClickAsync(cancellationToken: token);

        await Expect(Css(page, "#later")).ToBeVisibleAsync(cancellationToken: token);
        await Expect(Css(page, "li")).ToHaveCountAsync(4, cancellationToken: token);
        await Expect(Css(page, "li")).ToHaveTextAsync(["One", "Two", "Three", ""], cancellationToken: token);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task UnmetExpectationSaysWhatItSaw(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);

        ExpectationFailedException exception = await Assert.ThrowsAsync<ExpectationFailedException>(() => Expect(Css(page, "#visible")).ToBeHiddenAsync(TimeSpan.FromMilliseconds(500), TestContext.Current.CancellationToken));

        Assert.Equal("Expected css \"#visible\" to be hidden; received visible after 0.5 seconds.", exception.Message);
    }

    private static ElementLocator Css(Page page, string selector)
    {
        return page.Locate(new CssLocator(selector));
    }

    private static async Task<Page> OpenAsync(BrowserGroup group, TestPageServer server)
    {
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("expect.html"), cancellationToken: TestContext.Current.CancellationToken);
        return page;
    }
}
