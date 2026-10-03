// <copyright file="HarRecordingTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.TestUtilities;
using WebDriverBiDi;
using static Dramaturge.TestUtilities.NetworkEvents;

public sealed class HarRecordingTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), $"dramaturge-recording-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(this.directory))
        {
            Directory.Delete(this.directory, true);
        }
    }

    [Fact]
    public async Task PageRecordingWritesTheRequestsItIncludesWhenSavedAndWhenDisposed()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        session.RemoteEnd.AnswerWith("network.getData", parameters => Bytes("string", $"{parameters["dataType"]} body"));
        string path = Path.Combine(this.directory, "nested", "page.har");

        HarRecording recording = await page.RecordHarAsync(path, new HarRecordingOptions() { Include = request => !request.Url.EndsWith("/skipped", StringComparison.Ordinal) }, TestContext.Current.CancellationToken);
        await BeforeRequestSentAsync(session.RemoteEnd, "request-1", url: "https://example.com/first");
        await ResponseCompletedAsync(session.RemoteEnd, "request-1");
        await BeforeRequestSentAsync(session.RemoteEnd, "request-2", url: "https://example.com/skipped");
        await ResponseCompletedAsync(session.RemoteEnd, "request-2");
        await FlushAsync(driver);
        await recording.SaveAsync(cancellationToken: TestContext.Current.CancellationToken);
        string[] saved = Urls(path);
        await BeforeRequestSentAsync(session.RemoteEnd, "request-3", url: "https://example.com/in-flight");
        await FlushAsync(driver);
        await recording.DisposeAsync();
        await recording.DisposeAsync();

        Assert.Equal(Path.GetFullPath(path), recording.Path);
        Assert.Equal([page.Id], session.RemoteEnd.CommandsFor("network.addDataCollector")[0]["params"]!["contexts"]!.AsArray().Select(context => (string?)context));
        Assert.Equal(["https://example.com/first"], saved);
        JsonArray entries = Entries(path);
        Assert.Equal(["https://example.com/first", "https://example.com/in-flight"], entries.Select(entry => (string?)entry!["request"]!["url"]));
        Assert.Equal("Monitoring stopped before the request completed.", (string?)entries[1]!["_error"]);
        Assert.Single(session.RemoteEnd.CommandsFor("network.removeDataCollector"));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => recording.SaveAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task BrowserRecordingFollowsItsUserContextAndCanLeaveOutBodies()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        string path = Path.Combine(this.directory, "browser.har");

        await using (HarRecording recording = await page.Browser.RecordHarAsync(path, new HarRecordingOptions() { CaptureBodies = false }, TestContext.Current.CancellationToken))
        {
            await BeforeRequestSentAsync(session.RemoteEnd, "request-1", url: "https://example.com/own", userContext: page.Browser.Id);
            await ResponseCompletedAsync(session.RemoteEnd, "request-1");
            await BeforeRequestSentAsync(session.RemoteEnd, "request-2", url: "https://example.com/another-browsers", userContext: "another-user-context");
            await ResponseCompletedAsync(session.RemoteEnd, "request-2");
            await FlushAsync(driver);
            await recording.SaveAsync(cancellationToken: TestContext.Current.CancellationToken);
        }

        Assert.Empty(session.RemoteEnd.CommandsFor("network.addDataCollector"));
        Assert.Equal(["https://example.com/own"], Urls(path));
    }

    [Fact]
    public async Task RecordingWithoutOptionsCapturesBodiesAndSavesWithinTheTimeGiven()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        string path = Path.Combine(this.directory, "defaults.har");

        await using HarRecording recording = await page.RecordHarAsync(path, cancellationToken: TestContext.Current.CancellationToken);
        await recording.SaveAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Single(session.RemoteEnd.CommandsFor("network.addDataCollector"));
        Assert.Empty(Urls(path));
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, new DramaturgeOptions() { NavigationTimeout = TimeSpan.FromMilliseconds(200) }, TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page);
    }

    private static JsonArray Entries(string path) => JsonNode.Parse(File.ReadAllText(path))!["log"]!["entries"]!.AsArray();

    private static string[] Urls(string path) => [.. Entries(path).Select(entry => (string)entry!["request"]!["url"]!)];
}
