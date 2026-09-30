// <copyright file="KeyboardActionIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.Browsers;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;
using WebDriverBiDi.Script;

public class KeyboardActionIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task FillReplacesTheTextOfEachKindOfEditableElement(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "keyboard.html");

        await page.Locate(new CssLocator("#name")).FillAsync("new name", cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#email")).FillAsync("new@example.com", cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#number")).FillAsync("42", cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#notes")).FillAsync("new notes", cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#rich")).FillAsync("new rich text", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("new name", await ValueAsync(group, page.Id, "#name"));
        Assert.Equal("new@example.com", await ValueAsync(group, page.Id, "#email"));
        Assert.Equal("42", await ValueAsync(group, page.Id, "#number"));
        Assert.Equal("new notes", await ValueAsync(group, page.Id, "#notes"));
        Assert.Equal("new rich text", await EvaluateAsync(group, page.Id, "() => document.getElementById('rich').textContent"));
        string[] events = await EventsAsync(group, page);
        Assert.Equal(["focus", .. Enumerable.Repeat("input", "new name".Length), "blur"], events);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ClearEmptiesAnInputAndFillWaitsForItToBeEnabled(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "keyboard.html");

        await page.Locate(new CssLocator("#name")).ClearAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.Locate(new CssLocator("#later")).FillAsync("enabled", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(string.Empty, await ValueAsync(group, page.Id, "#name"));
        Assert.Equal("enabled", await ValueAsync(group, page.Id, "#later"));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task FillThatCannotHappenSaysWhy(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "keyboard.html");

        WebDriverBiDiTimeoutException readOnly = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => page.Locate(new CssLocator("#locked")).FillAsync("x", new ActionOptions() { Timeout = TimeSpan.FromSeconds(1) }, TestContext.Current.CancellationToken));
        InvalidOperationException notEditable = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("#label")).FillAsync("x", cancellationToken: TestContext.Current.CancellationToken));
        InvalidOperationException date = await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("#date")).FillAsync("2026-09-30", cancellationToken: TestContext.Current.CancellationToken));

        Assert.EndsWith("; the element was read-only.", readOnly.Message);
        Assert.Equal("css \"#label\" is not an editable element.", notEditable.Message);
        Assert.Equal("css \"#date\" is an input of type \"date\", whose value cannot be typed.", date.Message);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task PressAndPressSequentiallyTypeIntoTheFocusedElement(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "keyboard.html");
        ElementLocator keys = page.Locate(new CssLocator("#keys"));

        await keys.PressSequentiallyAsync("hi", new PressSequentiallyOptions() { Delay = TimeSpan.FromMilliseconds(10) }, TestContext.Current.CancellationToken);
        await keys.PressAsync(Keys.Backspace, cancellationToken: TestContext.Current.CancellationToken);
        string typed = await ValueAsync(group, page.Id, "#keys");
        await keys.PressAsync("x", new KeyActionOptions() { Modifiers = KeyModifiers.Shift }, TestContext.Current.CancellationToken);

        Assert.Equal("h", typed);
        Assert.Equal(["keydown:h:false", "keydown:i:false", "keydown:backspace:false", "keydown:shift:true", "keydown:x:true"], await EventsAsync(group, page));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task FocusAndBlurMoveTheFocus(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "keyboard.html");
        ElementLocator name = page.Locate(new CssLocator("#name"));

        await name.FocusAsync(cancellationToken: TestContext.Current.CancellationToken);
        string focused = await EvaluateAsync(group, page.Id, "() => document.activeElement.id");
        await name.BlurAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("name", focused);
        Assert.Equal("BODY", await EvaluateAsync(group, page.Id, "() => document.activeElement.tagName"));
        Assert.Equal(["focus", "blur"], await EventsAsync(group, page));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task FillTypesIntoAnInputInAFrame(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "action-frame.html");
        Frame frame = await page.Locate(new CssLocator("#inner")).ContentFrameAsync(cancellationToken: TestContext.Current.CancellationToken);

        await frame.Locate(new CssLocator("#inner-input")).FillAsync("framed", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("framed", await ValueAsync(group, frame.Id, "#inner-input"));
    }

    private static async Task<Page> OpenAsync(BrowserGroup group, TestPageServer server, string pageName)
    {
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor(pageName), cancellationToken: TestContext.Current.CancellationToken);
        return page;
    }

    private static Task<string> ValueAsync(BrowserGroup group, string contextId, string selector)
    {
        return EvaluateAsync(group, contextId, $"() => document.querySelector('{selector}').value");
    }

    private static async Task<string> EvaluateAsync(BrowserGroup group, string contextId, string function)
    {
        StringRemoteValue value = await group.Driver.Script.CallFunctionAsync<StringRemoteValue>(contextId, function, cancellationToken: TestContext.Current.CancellationToken);
        return value.Value;
    }

    private static async Task<string[]> EventsAsync(BrowserGroup group, Page page)
    {
        CollectionRemoteValue events = await group.Driver.Script.CallFunctionAsync<CollectionRemoteValue>(page.Id, "() => window.events", cancellationToken: TestContext.Current.CancellationToken);
        return [.. events.Value!.Select(value => value.As<StringRemoteValue>().Value)];
    }
}
