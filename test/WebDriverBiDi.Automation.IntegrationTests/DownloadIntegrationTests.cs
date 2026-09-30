// <copyright file="DownloadIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.Browsers;
using WebDriverBiDi.BrowsingContext;

public class DownloadIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task AllowedDownloadsAreSavedInTheFolderAndReported(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        string folder = Directory.CreateTempSubdirectory("automation-downloads-").FullName;
        try
        {
            await group.DefaultBrowser.AllowDownloadsAsync(folder, TestContext.Current.CancellationToken);
            Page page = await OpenAsync(group, server);
            TaskCompletionSource<Download> reported = new(TaskCreationOptions.RunContinuationsAsynchronously);
            page.OnDownload.AddObserver(e => reported.TrySetResult(e.Download));

            Download download = await page.RunAndWaitForDownloadAsync(() => page.Locate(new CssLocator("#save")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken), cancellationToken: TestContext.Current.CancellationToken);
            DownloadOutcome outcome = await download.WaitForEndAsync(cancellationToken: TestContext.Current.CancellationToken);

            Assert.Same(download, await reported.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken));
            Assert.Equal("report.txt", download.SuggestedFileName);
            Assert.Equal(server.UrlFor("report.txt"), download.Url);
            Assert.Same(page, download.Page);
            Assert.Equal(DownloadEndStatus.Complete, outcome.Status);
            Assert.NotNull(outcome.FilePath);
            Assert.Equal("Report contents\n", await File.ReadAllTextAsync(outcome.FilePath, TestContext.Current.CancellationToken));
            Assert.Equal(folder, Path.GetDirectoryName(Path.GetFullPath(outcome.FilePath)));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task DeniedDownloadsAreCanceled(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        await group.DefaultBrowser.DenyDownloadsAsync(TestContext.Current.CancellationToken);
        Page page = await OpenAsync(group, server);

        Download download = await page.RunAndWaitForDownloadAsync(() => page.Locate(new CssLocator("#save")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken), cancellationToken: TestContext.Current.CancellationToken);
        DownloadOutcome outcome = await download.WaitForEndAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(DownloadEndStatus.Canceled, outcome.Status);
        Assert.Null(outcome.FilePath);
    }

    private static async Task<Page> OpenAsync(BrowserGroup group, TestPageServer server)
    {
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("downloads.html"), cancellationToken: TestContext.Current.CancellationToken);
        return page;
    }
}
