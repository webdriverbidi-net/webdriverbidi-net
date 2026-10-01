// <copyright file="PointerActionIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using Dramaturge.Browsers;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;
using WebDriverBiDi.Script;

public class PointerActionIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ClicksCarryTheirButtonCountAndModifiers(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "actions.html");
        ElementLocator record = page.Locate(new CssLocator("#record"));

        await record.ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await record.ClickAsync(new ClickOptions() { Modifiers = KeyModifiers.Shift | KeyModifiers.Alt }, TestContext.Current.CancellationToken);
        await record.ClickAsync(new ClickOptions() { Button = PointerButton.Middle }, TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#double")).DblClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#hover-target")).HoverAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["click:0:1:false:false", "click:0:1:true:true", "auxclick:1", "dblclick", "hover"], await EventsAsync(group, page));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ClickWaitsForCoveredAndDisabledElementsAndScrollsToFarOnes(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "actions.html");

        await page.Locate(new CssLocator("#covered")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#later")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#far")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["covered", "later", "far"], await EventsAsync(group, page));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ClickThatCannotHappenSaysWhy(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "actions.html");
        await group.Driver.Script.CallFunctionAsync(page.Id, "() => { const overlay = document.createElement('div'); overlay.id = 'blocker'; overlay.style.cssText = 'position: fixed; inset: 0'; document.body.appendChild(overlay); }", cancellationToken: TestContext.Current.CancellationToken);

        WebDriverBiDiTimeoutException exception = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => page.Locate(new CssLocator("#record")).ClickAsync(new ClickOptions() { Timeout = TimeSpan.FromSeconds(1) }, TestContext.Current.CancellationToken));

        Assert.Contains("the element was obscured by <div id=\"blocker\"", exception.Message);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ClickLandsOnElementsInFrames(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "action-frame.html");
        Frame frame = await page.Locate(new CssLocator("#inner")).ContentFrameAsync(cancellationToken: TestContext.Current.CancellationToken);

        await frame.Locate(new CssLocator("#inner-button")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);

        BooleanRemoteValue clicked = await group.Driver.Script.CallFunctionAsync<BooleanRemoteValue>(frame.Id, "() => window.clicked", cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(clicked.Value);
    }

    // A click that starts waiting in one document and finds its element in the next.
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ClickContinuesAcrossANavigation(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "index.html");

        Task click = page.Locate(new CssLocator("#late-button")).ClickAsync(new ClickOptions() { Timeout = TimeSpan.FromSeconds(10) }, TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("late-button.html"), ReadinessState.None, cancellationToken: TestContext.Current.CancellationToken);
        await click;

        BooleanRemoteValue clicked = await group.Driver.Script.CallFunctionAsync<BooleanRemoteValue>(page.Id, "() => window.clicked", cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(clicked.Value);
    }

    private static async Task<Page> OpenAsync(BrowserGroup group, TestPageServer server, string pageName)
    {
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor(pageName), cancellationToken: TestContext.Current.CancellationToken);
        return page;
    }

    private static async Task<string[]> EventsAsync(BrowserGroup group, Page page)
    {
        CollectionRemoteValue events = await group.Driver.Script.CallFunctionAsync<CollectionRemoteValue>(page.Id, "() => window.events", cancellationToken: TestContext.Current.CancellationToken);
        return [.. events.Value!.Select(value => value.As<StringRemoteValue>().Value)];
    }
}
