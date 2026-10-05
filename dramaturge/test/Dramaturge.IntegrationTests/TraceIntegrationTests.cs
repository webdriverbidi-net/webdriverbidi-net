// <copyright file="TraceIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;
using Dramaturge.Browsers;
using Dramaturge.TestUtilities;
using WebDriverBiDi.BrowsingContext;

public sealed class TraceIntegrationTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), $"dramaturge-trace-it-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(this.directory))
        {
            Directory.Delete(this.directory, true);
        }
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task TraceHasTheActionsConsoleMessagesAndNetworkTrafficOfTheBrowser(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Browser browser = await group.CreateBrowserAsync(cancellationToken: TestContext.Current.CancellationToken);
        string path = Path.Combine(this.directory, "trace.zip");

        await using (TraceRecording recording = await browser.RecordTraceAsync(path, cancellationToken: TestContext.Current.CancellationToken))
        {
            Page page = await browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
            await page.NavigateAsync(server.UrlFor("events.html"), cancellationToken: TestContext.Current.CancellationToken);
            TaskCompletionSource warned = new(TaskCreationOptions.RunContinuationsAsynchronously);
            page.OnConsoleMessage.AddObserver(e => _ = e.Method == "warn" && warned.TrySetResult());
            await page.Locate(new CssLocator("#log")).ClickAsync(cancellationToken: TestContext.Current.CancellationToken);
            await warned.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        }

        List<JsonObject> events = Lines(path, "trace.trace");
        Assert.Equal("context-options", (string?)events[0]["type"]);
        Assert.False(string.IsNullOrEmpty((string?)events[0]["browserName"]));
        Assert.Single(events, e => (string?)e["method"] == "page");
        JsonObject click = Assert.Single(events, e => (string?)e["type"] == "before");
        Assert.Equal("Click", (string?)click["title"]);
        Assert.Contains(events, e => (string?)e["type"] == "after" && (string?)e["callId"] == (string?)click["callId"] && e["error"] is null);
        Assert.Equal(["log", "warning"], events.Where(e => (string?)e["type"] == "console").Select(e => (string?)e["messageType"]));
        JsonNode document = Assert.Single(Lines(path, "trace.network"), line => ((string)line["snapshot"]!["request"]!["url"]!).EndsWith("/events.html", StringComparison.Ordinal))["snapshot"]!;
        Assert.Contains("id=\"log\"", Encoding.UTF8.GetString(Entry(path, (string)document["response"]!["content"]!["_file"]!)));
    }

    private static List<JsonObject> Lines(string path, string name)
    {
        return [.. Encoding.UTF8.GetString(Entry(path, name)).Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => JsonNode.Parse(line)!.AsObject())];
    }

    private static byte[] Entry(string path, string name)
    {
        using ZipArchive zip = ZipFile.OpenRead(path);
        using MemoryStream content = new();
        using (Stream stream = zip.GetEntry(name)!.Open())
        {
            stream.CopyTo(content);
        }

        return content.ToArray();
    }
}
