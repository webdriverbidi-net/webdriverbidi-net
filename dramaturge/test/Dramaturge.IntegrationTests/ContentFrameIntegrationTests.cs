// <copyright file="ContentFrameIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi.BrowsingContext;

public class ContentFrameIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ContentFrameIsTheTrackedFrameAndCanBeSearched(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("frames.html"), cancellationToken: TestContext.Current.CancellationToken);

        Frame frame = await page.Locate(new CssLocator("#child")).ContentFrameAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Same(page.MainFrame, frame.ParentFrame);
        Assert.Contains(frame, page.Frames);
        Assert.Equal(1, await frame.Locate(new CssLocator("p")).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await page.Locate(new CssLocator("p")).CountAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task CrossOriginFrameAddedLaterIsFound(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("cross-origin-frame.html"), cancellationToken: TestContext.Current.CancellationToken);

        Frame frame = await page.Locate(new CssLocator("#other-origin")).ContentFrameAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        await frame.Locate(new CssLocator("p")).WaitForAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.StartsWith("http://127.0.0.1:", frame.Url);
        Assert.True(await frame.Locate(new CssLocator("p")).IsVisibleAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task ElementThatIsNotAFrameIsReported(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("frames.html"), cancellationToken: TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() => page.Locate(new CssLocator("h1")).ContentFrameAsync(cancellationToken: TestContext.Current.CancellationToken));
    }
}
