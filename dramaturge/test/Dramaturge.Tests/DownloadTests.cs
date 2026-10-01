// <copyright file="DownloadTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using Microsoft.Extensions.Time.Testing;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;

public class DownloadTests
{
    private static readonly TimeSpan EventWait = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task DownloadIsReportedAndItsEndMatchedByTheDownloadId()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        TaskCompletionSource<Download> reported = new(TaskCreationOptions.RunContinuationsAsynchronously);
        page.OnDownload.AddObserver(e => reported.TrySetResult(e.Download));

        Download download = await page.RunAndWaitForDownloadAsync(() => session.RemoteEnd.RaiseEventAsync("browsingContext.downloadWillBegin", WillBegin(page.Id, "navigation-1", "download-1")), cancellationToken: TestContext.Current.CancellationToken);
        await session.RemoteEnd.RaiseEventAsync("browsingContext.downloadEnd", Ended(page.Id, "navigation-other", "download-1", "complete", "/tmp/report.txt"));
        DownloadOutcome outcome = await download.WaitForEndAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Same(download, await reported.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken));
        Assert.Equal("https://example.com/report.txt", download.Url);
        Assert.Equal("report.txt", download.SuggestedFileName);
        Assert.Same(page, download.Page);
        Assert.Same(page.MainFrame, download.Frame);
        Assert.Equal(new DownloadOutcome(DownloadEndStatus.Complete, "/tmp/report.txt"), outcome);
    }

    [Fact]
    public async Task EndsOfOtherDownloadsAreIgnoredAndACanceledOneHasNoFile()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Download download = await page.RunAndWaitForDownloadAsync(() => session.RemoteEnd.RaiseEventAsync("browsingContext.downloadWillBegin", WillBegin(page.Id, "navigation-1", "download-1")), cancellationToken: TestContext.Current.CancellationToken);
        await session.RemoteEnd.RaiseEventAsync("browsingContext.downloadEnd", Ended(page.Id, "navigation-1", "download-unrelated", "complete", "/tmp/other.txt"));
        await session.RemoteEnd.RaiseEventAsync("browsingContext.downloadEnd", Ended(page.Id, "navigation-1", "download-1", "canceled", null));
        DownloadOutcome outcome = await download.WaitForEndAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(new DownloadOutcome(DownloadEndStatus.Canceled, null), outcome);
    }

    [Fact]
    public async Task DownloadsFromUntrackedContextsAreNotReported()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        List<string> urls = [];
        TaskCompletionSource<bool> tracked = new(TaskCreationOptions.RunContinuationsAsynchronously);
        page.OnDownload.AddObserver(e =>
        {
            urls.Add(e.Download.Url);
            tracked.TrySetResult(true);
        });

        JsonObject elsewhere = WillBegin("untracked-context", "navigation-1", "download-1");
        elsewhere["url"] = "https://example.com/elsewhere";
        await session.RemoteEnd.RaiseEventAsync("browsingContext.downloadWillBegin", elsewhere);
        await session.RemoteEnd.RaiseEventAsync("browsingContext.downloadWillBegin", WillBegin(page.Id, "navigation-2", "download-2"));
        await tracked.Task.WaitAsync(EventWait, TestContext.Current.CancellationToken);

        Assert.Equal(["https://example.com/report.txt"], urls);
    }

    [Fact]
    public async Task WaitsThatSeeNothingTimeOut()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider time) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Task<Download> begin = page.RunAndWaitForDownloadAsync(() => Task.CompletedTask, TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException notBegun = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, begin));
        Download unmatched = await page.RunAndWaitForDownloadAsync(() => session.RemoteEnd.RaiseEventAsync("browsingContext.downloadWillBegin", WillBegin(page.Id, null, "download-never-ends")), cancellationToken: TestContext.Current.CancellationToken);
        WebDriverBiDiTimeoutException notEnded = await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(() => DriveAsync(time, unmatched.WaitForEndAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken)));

        Assert.Equal("Timed out after 1 seconds waiting for a download to begin.", notBegun.Message);
        Assert.Equal("Timed out after 1 seconds waiting for the download of https://example.com/report.txt to end.", notEnded.Message);
    }

    [Fact]
    public async Task WaitIsCancelledByItsToken()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        using CancellationTokenSource cancellation = new();

        Task<Download> begin = page.RunAndWaitForDownloadAsync(() => Task.CompletedTask, cancellationToken: cancellation.Token);
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => begin);
    }

    [Fact]
    public async Task DownloadBehaviorIsSetForTheBrowsersUserContext()
    {
        (BiDiDriver driver, FakeSession session, Page page, FakeTimeProvider _) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        Browser browser = page.Browser;

        await browser.AllowDownloadsAsync("/tmp/downloads", TestContext.Current.CancellationToken);
        await browser.DenyDownloadsAsync(TestContext.Current.CancellationToken);
        await browser.ResetDownloadBehaviorAsync(TestContext.Current.CancellationToken);

        IReadOnlyList<JsonObject> commands = session.RemoteEnd.CommandsFor("browser.setDownloadBehavior");
        Assert.Equal(3, commands.Count);
        Assert.All(commands, command => Assert.Equal([browser.Id], command["params"]!["userContexts"]!.AsArray().Select(userContext => (string?)userContext)));
        Assert.Equal("allowed", (string?)commands[0]["params"]!["downloadBehavior"]!["type"]);
        Assert.Equal("/tmp/downloads", (string?)commands[0]["params"]!["downloadBehavior"]!["destinationFolder"]);
        Assert.Equal("denied", (string?)commands[1]["params"]!["downloadBehavior"]!["type"]);
        Assert.Null(commands[2]["params"]!["downloadBehavior"]);
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page, FakeTimeProvider Time)> OpenPageAsync()
    {
        FakeTimeProvider time = new();
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new AutomationOptions() { TimeProvider = time }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page, time);
    }

    private static JsonObject WillBegin(string contextId, string? navigationId, string downloadId)
    {
        return new JsonObject() { ["context"] = contextId, ["navigation"] = navigationId, ["timestamp"] = 1790000000000, ["url"] = "https://example.com/report.txt", ["suggestedFilename"] = "report.txt", ["download"] = downloadId };
    }

    private static JsonObject Ended(string contextId, string? navigationId, string downloadId, string status, string? filePath)
    {
        JsonObject parameters = new() { ["context"] = contextId, ["navigation"] = navigationId, ["timestamp"] = 1790000000000, ["url"] = "https://example.com/report.txt", ["status"] = status, ["download"] = downloadId };
        if (status == "complete")
        {
            parameters["filepath"] = filePath;
        }

        return parameters;
    }

    private static async Task<T> DriveAsync<T>(FakeTimeProvider time, Task<T> operation)
    {
        while (!operation.IsCompleted)
        {
            time.Advance(TimeSpan.FromMilliseconds(100));
            await Task.WhenAny(operation, Task.Delay(5, TestContext.Current.CancellationToken));
        }

        return await operation;
    }
}
