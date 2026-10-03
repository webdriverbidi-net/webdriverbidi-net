// <copyright file="HarRecordingIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi.BrowsingContext;

public sealed class HarRecordingIntegrationTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), $"dramaturge-recording-it-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(this.directory))
        {
            Directory.Delete(this.directory, true);
        }
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task RecordedPageIsReplayedWithoutTheServer(BrowserKind browserKind)
    {
        string har = Path.Combine(this.directory, "page.har");
        string pageUrl;
        await using (TestPageServer server = await TestPageServer.StartAsync())
        {
            await using BrowserGroup recordingGroup = await TestBrowsers.LaunchAsync(browserKind);
            Page recorded = await recordingGroup.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
            pageUrl = server.UrlFor("network.html");
            await using (HarRecording recording = await recorded.RecordHarAsync(har, cancellationToken: TestContext.Current.CancellationToken))
            {
                await recorded.NavigateAsync(pageUrl, cancellationToken: TestContext.Current.CancellationToken);
                Assert.Equal("server data", await recorded.EvaluateAsync<string>("async () => (await fetch('data.txt')).text()", cancellationToken: TestContext.Current.CancellationToken));
            }
        }

        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.RouteFromHarAsync(har, cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(pageUrl, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Network", await page.EvaluateAsync<string>("() => document.title", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("server data", await page.EvaluateAsync<string>("async () => (await fetch('data.txt')).text()", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task RecordingKeepsARoutedRequestsBodyAndTheRoutesAnswer(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("network.html"), cancellationToken: TestContext.Current.CancellationToken);
        await page.RouteAsync(server.UrlFor("submit"), route => route.FulfillAsync(200, "routed answer", new Dictionary<string, string>() { ["Content-Type"] = "text/plain" }), cancellationToken: TestContext.Current.CancellationToken);
        string har = Path.Combine(this.directory, "routed.har");

        string answer;
        await using (HarRecording recording = await page.RecordHarAsync(har, new HarRecordingOptions() { Include = request => request.Url.EndsWith("/submit", StringComparison.Ordinal) }, TestContext.Current.CancellationToken))
        {
            answer = await page.EvaluateAsync<string>("async () => (await fetch('submit', { method: 'POST', body: 'name=Ada' })).text()", cancellationToken: TestContext.Current.CancellationToken);
        }

        JsonNode entry = Assert.Single(Entries(har))!;
        Assert.Equal("routed answer", answer);
        Assert.Equal("POST", (string?)entry["request"]!["method"]);
        Assert.Equal("name=Ada", (string?)entry["request"]!["postData"]!["text"]);
        Assert.Equal("routed answer", (string?)entry["response"]!["content"]!["text"]);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task BrowserRecordingHasItsWorkersRequestsAndNotAnotherBrowsers(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Browser browser = await group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page page = await browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        Page other = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        string har = Path.Combine(this.directory, "browser.har");

        await using (HarRecording recording = await browser.RecordHarAsync(har, cancellationToken: TestContext.Current.CancellationToken))
        {
            await page.NavigateAsync(server.UrlFor("network.html"), cancellationToken: TestContext.Current.CancellationToken);
            await page.Locate(new CssLocator("#fetch-worker")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal("server data", await page.WaitForFunctionAsync<string>("() => window.worker", timeout: TimeSpan.FromSeconds(10), cancellationToken: TestContext.Current.CancellationToken));
            await other.NavigateAsync(server.UrlFor("second.html"), cancellationToken: TestContext.Current.CancellationToken);
        }

        string[] urls = [.. Entries(har).Select(entry => (string)entry!["request"]!["url"]!)];
        Assert.Contains(server.UrlFor("network.html"), urls);
        Assert.Contains(server.UrlFor("worker.js"), urls);
        Assert.Contains(server.UrlFor("data.txt"), urls);
        Assert.DoesNotContain(server.UrlFor("second.html"), urls);
    }

    private static JsonArray Entries(string path) => JsonNode.Parse(File.ReadAllText(path))!["log"]!["entries"]!.AsArray();
}
