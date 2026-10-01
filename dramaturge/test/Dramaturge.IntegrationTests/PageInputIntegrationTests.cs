// <copyright file="PageInputIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;
using WebDriverBiDi.Script;

public class PageInputIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task MouseButtonsStayPressedBetweenCallsAndClicksLandAtViewportCoordinates(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "input.html");
        CancellationToken token = TestContext.Current.CancellationToken;

        await page.Mouse.MoveAsync(100, 100, steps: 4, cancellationToken: token);
        await page.Mouse.DownAsync(cancellationToken: token);
        await page.Mouse.UpAsync(cancellationToken: token);
        await page.Mouse.DownAsync(PointerButton.Right, token);
        await page.Mouse.UpAsync(PointerButton.Right, token);
        await page.Mouse.ClickAsync(60, 70, clickCount: 1, cancellationToken: token);
        await page.Mouse.DblClickAsync(160, 130, cancellationToken: token);

        string[] events = await EventsAsync(page);
        Assert.Equal(["mousedown:0", "mouseup:0", "click:1:false", "mousedown:2", "mouseup:2", "mousedown:0", "mouseup:0", "click:1:false", "mousedown:0", "mouseup:0", "click:1:false", "mousedown:0", "mouseup:0", "click:2:false", "dblclick"], events);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task WheelScrollsWhatIsUnderTheMouse(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "input.html");

        await page.Mouse.MoveAsync(300, 300, cancellationToken: TestContext.Current.CancellationToken);
        await page.Mouse.WheelAsync(0, 400, TestContext.Current.CancellationToken);

        await page.WaitForFunctionAsync("() => window.scrollY > 0", timeout: TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task KeysStayPressedBetweenCallsAndHoldForTheOtherInput(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "input.html");
        CancellationToken token = TestContext.Current.CancellationToken;
        await page.Locate(new CssLocator("#field")).FocusAsync(cancellationToken: token);

        await page.Keyboard.TypeAsync("hi", TimeSpan.FromMilliseconds(10), token);
        await page.Keyboard.DownAsync(Keys.Shift, token);
        await page.Keyboard.PressAsync("a", cancellationToken: token);
        string value = await page.EvaluateAsync<string>("() => document.getElementById('field').value", cancellationToken: token);
        await page.Locate(new CssLocator("#target")).ClickAsync(cancellationToken: token);
        await page.Keyboard.UpAsync(Keys.Shift, token);
        await page.Locate(new CssLocator("#field")).FocusAsync(cancellationToken: token);
        await page.Keyboard.PressAsync("b", KeyModifiers.Shift, token);

        string[] events = await EventsAsync(page);
        // Chrome reports the unshifted key in the event even for a key pressed with Shift, though it types the
        // shifted character; the key is compared without case.
        Assert.Equal(["keydown:h:false", "keydown:i:false", "keydown:shift:true", "keydown:a:true", "mousedown:0", "mouseup:0", "click:1:true", "keydown:shift:true", "keydown:b:true"], events);
        Assert.Equal("hiA", value);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task BoxesInNestedFramesConvertToPageCoordinatesTheMouseCanUse(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server, "frame-offset.html");
        CancellationToken token = TestContext.Current.CancellationToken;
        Frame outer = await page.Locate(new CssLocator("#outer")).ContentFrameAsync(cancellationToken: token);
        Frame inner = await outer.Locate(new CssLocator("#inner")).ContentFrameAsync(cancellationToken: token);
        await inner.WaitForLoadStateAsync(cancellationToken: token);

        BoundingBox inFrame = (await inner.Locate(new CssLocator("#inner-button")).BoundingBoxAsync(cancellationToken: token))!;
        BoundingBox onPage = await inFrame.ToTopLevelAsync(cancellationToken: token);
        BoundingBox again = await onPage.ToTopLevelAsync(cancellationToken: token);
        await page.Mouse.ClickAsync(onPage.X + (onPage.Width / 2), onPage.Y + (onPage.Height / 2), cancellationToken: token);
        // Measured from the document, the box is where it was before the page scrolled.
        await page.EvaluateAsync("() => window.scrollTo(0, 100)", cancellationToken: token);
        BoundingBox inDocument = await inFrame.ToTopLevelAsync(CoordinateOrigin.Document, cancellationToken: token);

        Assert.Same(inner, inFrame.Frame);
        Assert.Same(page.MainFrame, onPage.Frame);
        Assert.Same(onPage, again);
        Assert.Equal(inFrame.X + 30 + 5 + 7 + 20 + 3 + 4, onPage.X, 3);
        Assert.Equal(inFrame.Y + 40 + 5 + 7 + 25 + 3 + 4, onPage.Y, 3);
        Assert.Equal((inFrame.Width, inFrame.Height), (onPage.Width, onPage.Height));
        Assert.True(await inner.EvaluateAsync<bool>("() => window.clicked", cancellationToken: token));
        Assert.Equal(onPage.Y, inDocument.Y, 3);
    }

    private static async Task<Page> OpenAsync(BrowserGroup group, TestPageServer server, string pageName)
    {
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor(pageName), cancellationToken: TestContext.Current.CancellationToken);
        return page;
    }

    private static Task<string[]> EventsAsync(Page page)
    {
        return page.EvaluateAsync<string[]>("() => window.events", cancellationToken: TestContext.Current.CancellationToken);
    }
}
