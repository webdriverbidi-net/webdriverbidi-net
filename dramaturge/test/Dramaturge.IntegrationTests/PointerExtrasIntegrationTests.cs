// <copyright file="PointerExtrasIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

public class PointerExtrasIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task TapUsesATouchPointer(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);

        await page.Locate(new CssLocator("#tap-target")).TapAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["pointerdown:touch", "click"], await EventsAsync(group, page));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task DragToDropsWithMouseEventsHoldingTheModifiers(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);

        await page.Locate(new CssLocator("#pointer-source")).DragToAsync(page.Locate(new CssLocator("#pointer-target")), new DragOptions() { Modifiers = KeyModifiers.Shift }, TestContext.Current.CancellationToken);

        Assert.Equal(["pointer-drop:true"], await EventsAsync(group, page));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task DragToDropsDraggableElements(BrowserKind browserKind)
    {
        Assert.SkipWhen(browserKind == BrowserKind.Firefox, "Firefox dispatches only dragstart for a native drag from WebDriver actions, never dragover or drop (https://bugzilla.mozilla.org/show_bug.cgi?id=1515879).");
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);

        await page.Locate(new CssLocator("#html-source")).DragToAsync(page.Locate(new CssLocator("#html-target")), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["dragstart", "drop:dragged"], await EventsAsync(group, page));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task DispatchEventCreatesTheEventInterfaceForItsType(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        ElementLocator target = page.Locate(new CssLocator("#dispatch-target"));

        bool click = await target.DispatchEventAsync("click", new Dictionary<string, LocalValue>() { ["clientX"] = LocalValue.Number(12) }, cancellationToken: TestContext.Current.CancellationToken);
        bool custom = await target.DispatchEventAsync("my-event", new Dictionary<string, LocalValue>() { ["bubbles"] = LocalValue.Boolean(false) }, cancellationToken: TestContext.Current.CancellationToken);
        bool canceled = await target.DispatchEventAsync("keydown", new Dictionary<string, LocalValue>() { ["key"] = LocalValue.String("Enter") }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(click);
        Assert.True(custom);
        Assert.False(canceled);
        Assert.Equal(["click:MouseEvent:12:true", "bubbled", "my-event:Event:false", "keydown:Enter"], await EventsAsync(group, page));
    }

    private static async Task<Page> OpenAsync(BrowserGroup group, TestPageServer server)
    {
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("pointer-extras.html"), cancellationToken: TestContext.Current.CancellationToken);
        return page;
    }

    private static async Task<string[]> EventsAsync(BrowserGroup group, Page page)
    {
        CollectionRemoteValue events = await group.Driver.Script.CallFunctionAsync<CollectionRemoteValue>(page.Id, "() => window.events", cancellationToken: TestContext.Current.CancellationToken);
        return [.. events.Value!.Select(value => value.As<StringRemoteValue>().Value)];
    }
}
