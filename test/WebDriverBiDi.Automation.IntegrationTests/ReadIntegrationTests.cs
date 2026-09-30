// <copyright file="ReadIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.Browsers;
using WebDriverBiDi.BrowsingContext;

public class ReadIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task TextHtmlValuesAndAttributesAreRead(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        CancellationToken token = TestContext.Current.CancellationToken;

        Assert.Equal("Visible hidden text", await page.Locate(new CssLocator("#text")).TextContentAsync(cancellationToken: token));
        Assert.Equal("Visible text", await page.Locate(new CssLocator("#text")).InnerTextAsync(cancellationToken: token));
        Assert.Equal("<b>bold</b> text", await page.Locate(new CssLocator("#html")).InnerHtmlAsync(cancellationToken: token));
        Assert.Equal("Ada", await page.Locate(new CssLocator("#name")).InputValueAsync(cancellationToken: token));
        Assert.Equal("Some notes", await page.Locate(new CssLocator("#notes")).InputValueAsync(cancellationToken: token));
        Assert.Equal("green", await page.Locate(new CssLocator("#color")).InputValueAsync(cancellationToken: token));
        Assert.Equal("person", await page.Locate(new CssLocator("#name")).GetAttributeAsync("data-role", cancellationToken: token));
        Assert.Equal("person", await page.Locate(new CssLocator("#name")).GetAttributeAsync("DATA-ROLE", cancellationToken: token));
        Assert.Null(await page.Locate(new CssLocator("#name")).GetAttributeAsync("title", cancellationToken: token));
        await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("#plain")).InputValueAsync(cancellationToken: token));
        await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("#svg-text")).InnerTextAsync(cancellationToken: token));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task StatesAreRead(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        CancellationToken token = TestContext.Current.CancellationToken;

        Assert.True(await page.Locate(new CssLocator("#invisible")).IsHiddenAsync(token));
        Assert.True(await page.Locate(new CssLocator("#missing")).IsHiddenAsync(token));
        Assert.False(await page.Locate(new CssLocator("#enabled")).IsHiddenAsync(token));
        Assert.True(await page.Locate(new CssLocator("#enabled")).IsEnabledAsync(cancellationToken: token));
        Assert.True(await page.Locate(new CssLocator("#disabled")).IsDisabledAsync(cancellationToken: token));
        Assert.True(await page.Locate(new CssLocator("#name")).IsEditableAsync(cancellationToken: token));
        Assert.False(await page.Locate(new CssLocator("#readonly")).IsEditableAsync(cancellationToken: token));
        Assert.True(await page.Locate(new CssLocator("#checked")).IsCheckedAsync(cancellationToken: token));
        Assert.False(await page.Locate(new CssLocator("#mixed")).IsCheckedAsync(cancellationToken: token));
        await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("#plain")).IsEditableAsync(cancellationToken: token));
        await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("#plain")).IsCheckedAsync(cancellationToken: token));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task BoundingBoxIsInTheViewportAndNullWhenNotVisible(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);

        Assert.Equal(new BoundingBox(page.MainFrame, 20, 30, 100, 50), await page.Locate(new CssLocator("#box")).BoundingBoxAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Null(await page.Locate(new CssLocator("#invisible")).BoundingBoxAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ScreenshotCapturesTheElementInOrOutOfView(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);

        (int Width, int Height) box = PngSize(await page.Locate(new CssLocator("#box")).ScreenshotAsync(cancellationToken: TestContext.Current.CancellationToken));
        (int Width, int Height) far = PngSize(await page.Locate(new CssLocator("#far")).ScreenshotAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(box.Width, box.Height * 2);
        Assert.Equal(far.Width * 2, far.Height * 3);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ScreenshotCapturesAnElementInAFrame(BrowserKind browserKind)
    {
        Assert.SkipWhen(browserKind == BrowserKind.Chrome, "Chrome does not capture screenshots of a frame that is not top-level (\"Non-top-level 'context' is currently not supported\"), which the protocol permits.");
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        Frame frame = await page.Locate(new CssLocator("#frame")).ContentFrameAsync(cancellationToken: TestContext.Current.CancellationToken);

        byte[] image = await frame.Locate(new CssLocator("#inner-button")).ScreenshotAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(PngSize(image).Width > 0);
    }

    private static async Task<Page> OpenAsync(BrowserGroup group, TestPageServer server)
    {
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("reads.html"), cancellationToken: TestContext.Current.CancellationToken);
        return page;
    }

    // A PNG's width and height are big-endian integers at bytes 16 and 20 of its header.
    private static (int Width, int Height) PngSize(byte[] image)
    {
        Assert.Equal([0x89, (byte)'P', (byte)'N', (byte)'G'], image.Take(4));
        return ((image[16] << 24) | (image[17] << 16) | (image[18] << 8) | image[19], (image[20] << 24) | (image[21] << 16) | (image[22] << 8) | image[23]);
    }
}
