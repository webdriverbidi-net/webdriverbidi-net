// <copyright file="CaptureIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Text;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.Browsers;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

public class CaptureIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ScreenshotsCaptureTheViewportTheDocumentOrAClip(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        await page.SetViewportSizeAsync(400, 300, TestContext.Current.CancellationToken);

        (int Width, int Height) viewport = PngSize(await page.ScreenshotAsync(cancellationToken: TestContext.Current.CancellationToken));
        (int Width, int Height) document = PngSize(await page.ScreenshotAsync(new PageScreenshotOptions() { FullPage = true }, TestContext.Current.CancellationToken));
        (int Width, int Height) clip = PngSize(await page.ScreenshotAsync(new PageScreenshotOptions() { Clip = new BoxClipRectangle() { X = 10, Y = 20, Width = 50, Height = 40 } }, TestContext.Current.CancellationToken));
        byte[] jpeg = await page.ScreenshotAsync(new PageScreenshotOptions() { Format = new ImageFormat() { Type = "image/jpeg" } }, TestContext.Current.CancellationToken);
        int documentHeight = await page.EvaluateAsync<int>("() => document.documentElement.scrollHeight", cancellationToken: TestContext.Current.CancellationToken);

        int scale = viewport.Width / 400;
        Assert.True(scale >= 1);
        Assert.Equal((400 * scale, 300 * scale), viewport);
        Assert.True(documentHeight >= 2000);
        Assert.Equal((400 * scale, documentHeight * scale), document);
        Assert.Equal((50 * scale, 40 * scale), clip);
        Assert.Equal([0xFF, 0xD8], jpeg.Take(2));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ViewportSizeIsSetAndReset(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        int original = await page.EvaluateAsync<int>("() => window.innerWidth", cancellationToken: TestContext.Current.CancellationToken);

        await page.SetViewportSizeAsync(321, 234, TestContext.Current.CancellationToken);
        int[] set = await page.EvaluateAsync<int[]>("() => [window.innerWidth, window.innerHeight]", cancellationToken: TestContext.Current.CancellationToken);
        await page.ResetViewportSizeAsync(TestContext.Current.CancellationToken);

        // Chrome lays the page out again only after the reset command returns.
        await page.WaitForFunctionAsync("(original) => window.innerWidth === original", [LocalValue.Number(original)], TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal([321, 234], set);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task PdfIsPrinted(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        PdfOptions options = new() { Background = true, Orientation = PrintOrientation.Landscape, PageSize = new PrintPageParameters() { Width = 21, Height = 29.7 }, Margins = new PrintMarginParameters() { Top = 1 }, Scale = 0.5, ShrinkToFit = false };
        options.PageRanges.Add(1);

        byte[] plain = await page.PdfAsync(cancellationToken: TestContext.Current.CancellationToken);
        byte[] configured = await page.PdfAsync(options, TestContext.Current.CancellationToken);

        Assert.Equal("%PDF", Encoding.ASCII.GetString(plain, 0, 4));
        Assert.Equal("%PDF", Encoding.ASCII.GetString(configured, 0, 4));
    }

    private static async Task<Page> OpenAsync(BrowserGroup group, TestPageServer server)
    {
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("capture.html"), cancellationToken: TestContext.Current.CancellationToken);
        return page;
    }

    // A PNG's width and height are big-endian integers at bytes 16 and 20 of its header.
    private static (int Width, int Height) PngSize(byte[] image)
    {
        Assert.Equal([0x89, (byte)'P', (byte)'N', (byte)'G'], image.Take(4));
        return ((image[16] << 24) | (image[17] << 16) | (image[18] << 8) | image[19], (image[20] << 24) | (image[21] << 16) | (image[22] << 8) | image[23]);
    }
}
