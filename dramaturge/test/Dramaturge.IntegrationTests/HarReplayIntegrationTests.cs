// <copyright file="HarReplayIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.Json.Nodes;
using Dramaturge.Browsers;
using Dramaturge.TestUtilities;

// The archives answer URLs the test server does not serve, so a page shows their content only if the replay answered.
public sealed class HarReplayIntegrationTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), $"dramaturge-har-it-{Guid.NewGuid():N}");

    public HarReplayIntegrationTests()
    {
        Directory.CreateDirectory(this.directory);
    }

    public void Dispose()
    {
        Directory.Delete(this.directory, true);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task NavigationAndRedirectedFetchAreReplayed(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        string har = this.WriteHar(
            Entry("GET", server.UrlFor("replayed.html"), 200, "text/html", "<!DOCTYPE html><title>Replayed page</title>"),
            Entry("GET", server.UrlFor("old-data"), 302, "text/plain", string.Empty, responseHeaders: [("Location", server.UrlFor("new-data"))]),
            Entry("GET", server.UrlFor("new-data"), 200, "text/plain", "replayed data"));
        await page.RouteFromHarAsync(har, cancellationToken: TestContext.Current.CancellationToken);

        await page.NavigateAsync(server.UrlFor("replayed.html"), cancellationToken: TestContext.Current.CancellationToken);
        string title = await page.EvaluateAsync<string>("() => document.title", cancellationToken: TestContext.Current.CancellationToken);
        string[] fetched = await page.EvaluateAsync<string[]>("async () => { const response = await fetch('old-data'); return [await response.text(), response.url, String(response.redirected)]; }", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("Replayed page", title);
        Assert.Equal(["replayed data", server.UrlFor("new-data"), "true"], fetched);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task RequestBodiesChooseTheirEntries(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        const string RecordedForm = "--recorded-boundary\r\nContent-Disposition: form-data; name=\"name\"\r\n\r\nAda\r\n--recorded-boundary--\r\n";
        string har = this.WriteHar(
            Entry("POST", server.UrlFor("submit"), 200, "text/plain", "Grace's answer", requestBody: "name=Grace"),
            Entry("POST", server.UrlFor("submit"), 200, "text/plain", "Ada's answer", requestBody: "name=Ada"),
            Entry("POST", server.UrlFor("upload"), 200, "text/plain", "form answer", requestBody: RecordedForm, requestHeaders: [("Content-Type", "multipart/form-data; boundary=recorded-boundary")]));
        await page.RouteFromHarAsync(har, cancellationToken: TestContext.Current.CancellationToken);

        string[] answers = await page.EvaluateAsync<string[]>(
            """
            async () => {
              const text = async (response) => (await response).text();
              const form = new FormData();
              form.append('name', 'Ada');
              return [
                await text(fetch('submit', { method: 'POST', body: 'name=Ada' })),
                await text(fetch('submit', { method: 'POST', body: 'name=Grace' })),
                await text(fetch('upload', { method: 'POST', body: form })),
              ];
            }
            """,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(["Ada's answer", "Grace's answer", "form answer"], answers);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task RequestsWithoutEntriesAreAbortedOrPassedOn(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        string har = this.WriteHar(Entry("GET", server.UrlFor("recorded"), 200, "text/plain", "recorded"));
        const string Fetch = "async () => { try { return await (await fetch('data.txt')).text(); } catch { return 'failed'; } }";

        RouteRegistration aborting = await page.RouteFromHarAsync(har, cancellationToken: TestContext.Current.CancellationToken);
        string aborted = await page.EvaluateAsync<string>(Fetch, cancellationToken: TestContext.Current.CancellationToken);
        await aborting.RemoveAsync(TestContext.Current.CancellationToken);
        await page.RouteFromHarAsync(har, new HarRouteOptions() { NotFound = HarNotFound.Fallback }, TestContext.Current.CancellationToken);
        string passedOn = await page.EvaluateAsync<string>(Fetch, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("failed", aborted);
        Assert.Equal("server data", passedOn);
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task BinaryBodiesAndRepeatedHeadersAreReplayed(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await OpenAsync(group, server);
        JsonObject binary = Entry("GET", server.UrlFor("binary"), 200, "application/octet-stream", Convert.ToBase64String([0, 1, 254, 255]));
        binary["response"]!["content"]!["encoding"] = "base64";
        string har = this.WriteHar(
            binary,
            Entry("GET", server.UrlFor("cookies"), 200, "text/plain", "cookies set", responseHeaders: [("Set-Cookie", "first=1; Path=/"), ("Set-Cookie", "second=2; Path=/")]));
        await page.Browser.RouteFromHarAsync(har, cancellationToken: TestContext.Current.CancellationToken);

        int[] bytes = await page.EvaluateAsync<int[]>("async () => [...new Uint8Array(await (await fetch('binary')).arrayBuffer())]", cancellationToken: TestContext.Current.CancellationToken);
        await page.EvaluateAsync<string>("async () => (await fetch('cookies')).text()", cancellationToken: TestContext.Current.CancellationToken);
        IReadOnlyList<BrowserCookie> cookies = await page.Browser.GetCookiesAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([0, 1, 254, 255], bytes);
        Assert.Equal(["first=1", "second=2"], cookies.Select(cookie => $"{cookie.Name}={cookie.Value}").Order());
    }

    private static async Task<Page> OpenAsync(BrowserGroup group, TestPageServer server)
    {
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("network.html"), cancellationToken: TestContext.Current.CancellationToken);
        return page;
    }

    private static JsonObject Entry(string method, string url, int status, string mimeType, string body, (string Name, string Value)[]? requestHeaders = null, (string Name, string Value)[]? responseHeaders = null, string? requestBody = null)
    {
        static JsonArray Headers((string Name, string Value)[] headers) => new([.. headers.Select(header => (JsonNode)new JsonObject() { ["name"] = header.Name, ["value"] = header.Value })]);
        JsonObject request = new() { ["method"] = method, ["url"] = url, ["headers"] = Headers(requestHeaders ?? []) };
        if (requestBody is not null)
        {
            request["postData"] = new JsonObject() { ["mimeType"] = "text/plain", ["text"] = requestBody };
        }

        return new JsonObject()
        {
            ["request"] = request,
            ["response"] = new JsonObject()
            {
                ["status"] = status,
                ["statusText"] = string.Empty,
                ["headers"] = Headers([("Content-Type", mimeType), .. responseHeaders ?? []]),
                ["content"] = new JsonObject() { ["mimeType"] = mimeType, ["text"] = body },
                ["redirectURL"] = string.Empty,
            },
        };
    }

    private string WriteHar(params JsonObject[] entries)
    {
        string path = Path.Combine(this.directory, "replay.har");
        File.WriteAllText(path, new JsonObject() { ["log"] = new JsonObject() { ["version"] = "1.2", ["entries"] = new JsonArray([.. entries]) } }.ToJsonString());
        return path;
    }
}
