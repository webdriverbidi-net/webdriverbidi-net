// <copyright file="HarRouteTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Dramaturge.TestUtilities;
using WebDriverBiDi;
using WebDriverBiDi.Network;

public sealed class HarRouteTests : IDisposable
{
    private const string RequestUrl = "https://example.com/api/data.json";
    private static readonly TimeSpan EventWait = TimeSpan.FromSeconds(10);

    private readonly string directory = Path.Combine(Path.GetTempPath(), $"dramaturge-har-{Guid.NewGuid():N}");

    public HarRouteTests()
    {
        Directory.CreateDirectory(this.directory);
    }

    public void Dispose()
    {
        Directory.Delete(this.directory, true);
    }

    [Fact]
    public async Task RequestIsAnsweredWithItsEntrysResponseAndItsOwnLength()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        string har = this.WriteHar(Entry("GET", RequestUrl, 201, "Created", Content("hello"), responseHeaders:
        [
            ("Content-Type", "text/plain"), ("Set-Cookie", "a=1"), ("Set-Cookie", "b=2"), ("Content-Encoding", "gzip"), ("Content-Length", "999"), ("Transfer-Encoding", "chunked"), (":status", "201"),
        ]));

        RouteRegistration route = await page.RouteFromHarAsync(har, cancellationToken: TestContext.Current.CancellationToken);
        JsonObject response = await RaiseAndWaitForAsync(session, "network.provideResponse", Blocked(page.Id, session));

        Assert.Equal("requests from the HAR har.har", route.Description);
        Assert.Equal(201, (int?)response["statusCode"]);
        Assert.Equal("Created", (string?)response["reasonPhrase"]);
        Assert.Equal("hello", Encoding.UTF8.GetString(Convert.FromBase64String((string)response["body"]!["value"]!)));
        Assert.Equal(["Content-Type: text/plain", "Set-Cookie: a=1", "Set-Cookie: b=2", "Content-Length: 5"], Headers(response));
        Assert.Empty(session.RemoteEnd.CommandsFor("network.addDataCollector"));
    }

    [Fact]
    public async Task EntryOfTheMethodAndUrlWithTheMostOfTheRequestsHeadersIsChosenThenTheFirst()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        string har = this.WriteHar(
            Entry("POST", RequestUrl, 200, "OK", Content("posted")),
            Entry("GET", RequestUrl + "#recorded-fragment", 200, "OK", Content("first"), requestHeaders: [("Accept", "text/html")]),
            Entry("GET", RequestUrl, 200, "OK", Content("most headers"), requestHeaders: [("accept", "application/json"), ("X-Client", "test")]),
            Entry("GET", RequestUrl, 200, "OK", Content("fewer headers"), requestHeaders: [("Accept", "application/json")]));
        await page.RouteFromHarAsync(har, cancellationToken: TestContext.Current.CancellationToken);

        JsonObject withHeaders = Blocked(page.Id, session, "request-1", RequestUrl + "#page-fragment", headers: [("Accept", "application/json"), ("X-Client", "test")]);
        JsonObject withBase64Header = Blocked(page.Id, session, "request-2");
        withBase64Header["request"]!["headers"] = new JsonArray(new JsonObject() { ["name"] = "Accept", ["value"] = new JsonObject() { ["type"] = "base64", ["value"] = Convert.ToBase64String(Encoding.UTF8.GetBytes("application/json")) } });
        JsonObject withoutHeaders = Blocked(page.Id, session, "request-3");
        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", withHeaders);
        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", withBase64Header);
        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", withoutHeaders);
        await session.RemoteEnd.WaitForCommandAsync("network.provideResponse", 3);

        Assert.Equal(["most headers", "most headers", "first"], session.RemoteEnd.CommandsFor("network.provideResponse").OrderBy(command => (string)command["params"]!["request"]!).Select(command => Body(command["params"]!.AsObject())));
    }

    [Fact]
    public async Task RequestWithoutAnEntryIsAbortedOrPassedOn()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        string har = this.WriteHar(Entry("GET", "https://example.com/other", 200, "OK", Content("other")));
        await page.RouteAsync(RequestUrl, route => route.FulfillAsync(200, "from the older route"), cancellationToken: TestContext.Current.CancellationToken);
        RouteRegistration aborting = await page.RouteFromHarAsync(har, RequestUrl, cancellationToken: TestContext.Current.CancellationToken);

        JsonObject aborted = await RaiseAndWaitForAsync(session, "network.failRequest", Blocked(page.Id, session, "request-1"));
        await aborting.RemoveAsync(TestContext.Current.CancellationToken);
        await page.RouteFromHarAsync(har, new Regex("/api/"), new HarRouteOptions() { NotFound = HarNotFound.Fallback }, TestContext.Current.CancellationToken);
        JsonObject passedOn = await RaiseAndWaitForAsync(session, "network.provideResponse", Blocked(page.Id, session, "request-2"));

        Assert.Equal("request-1", (string?)aborted["request"]);
        Assert.Equal("from the older route", (string?)passedOn["body"]!["value"]);
    }

    [Fact]
    public async Task RedirectIsAnsweredAsRecordedWithItsLocation()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        JsonObject redirect = Entry("GET", RequestUrl, 302, string.Empty, Content(string.Empty));
        redirect["response"]!["redirectURL"] = "https://example.com/moved";
        JsonObject notRedirect = Entry("GET", "https://example.com/created", 201, "Created", Content(string.Empty));
        notRedirect["response"]!["redirectURL"] = "https://example.com/ignored";
        JsonObject recordedLocation = Entry("GET", "https://example.com/recorded", 301, "Moved", Content(string.Empty), responseHeaders: [("location", "https://example.com/kept")]);
        recordedLocation["response"]!["redirectURL"] = "https://example.com/not-added";
        string har = this.WriteHar(redirect, notRedirect, recordedLocation);
        await page.RouteFromHarAsync(har, _ => true, cancellationToken: TestContext.Current.CancellationToken);

        JsonObject moved = await RaiseAndWaitForAsync(session, "network.provideResponse", Blocked(page.Id, session, "request-1"));
        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", Blocked(page.Id, session, "request-2", "https://example.com/created"));
        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", Blocked(page.Id, session, "request-3", "https://example.com/recorded"));
        await session.RemoteEnd.WaitForCommandAsync("network.provideResponse", 3);
        IReadOnlyList<JsonObject> responses = session.RemoteEnd.CommandsFor("network.provideResponse");

        Assert.Equal(302, (int?)moved["statusCode"]);
        Assert.False(moved.ContainsKey("reasonPhrase"));
        Assert.Equal(["Location: https://example.com/moved", "Content-Length: 0"], Headers(moved));
        Assert.Equal(["Content-Length: 0"], Headers(responses.Single(command => (string?)command["params"]!["request"] == "request-2")["params"]!.AsObject()));
        Assert.Equal(["location: https://example.com/kept", "Content-Length: 0"], Headers(responses.Single(command => (string?)command["params"]!["request"] == "request-3")["params"]!.AsObject()));
    }

    [Fact]
    public async Task RequestBodiesAreReadThroughACollectorAndCompared()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        const string RecordedForm = "--recorded\r\nContent-Disposition: form-data; name=\"name\"\r\n\r\nAda\r\n--recorded--\r\n";
        string har = this.WriteHar(
            Entry("POST", RequestUrl, 200, "OK", Content("no recorded body")),
            Entry("POST", RequestUrl, 200, "OK", Content("text body"), requestBody: Content("name=Ada")),
            Entry("POST", RequestUrl, 200, "OK", Content("form body"), requestBody: Content(RecordedForm), requestHeaders: [("Content-Type", "multipart/form-data; boundary=recorded")]),
            Entry("POST", RequestUrl, 200, "OK", Content("other form"), requestBody: Content("--x\r\nOther\r\n--x--"), requestHeaders: [("Content-Type", "multipart/form-data")]));
        Dictionary<string, string> bodies = new()
        {
            ["request-text"] = "name=Ada",
            ["request-form"] = RecordedForm.Replace("recorded", "browser-made-a-longer-one"),
            ["request-unrecorded"] = "name=Grace",
        };
        session.RemoteEnd.AnswerWith("network.getData", parameters => bodies.TryGetValue((string)parameters["request"]!, out string? body)
            ? new FakeResponse(new JsonObject() { ["bytes"] = new JsonObject() { ["type"] = "string", ["value"] = body } })
            : FakeResponse.Failure("no such network data", "not collected"));
        await page.RouteFromHarAsync(har, cancellationToken: TestContext.Current.CancellationToken);

        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", Blocked(page.Id, session, "request-text", method: "POST", bodySize: 8));
        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", Blocked(page.Id, session, "request-form", method: "POST", bodySize: 80, headers: [("Content-Type", "multipart/form-data; boundary=\"browser-made-a-longer-one\"")]));
        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", Blocked(page.Id, session, "request-unrecorded", method: "POST", bodySize: 10, headers: [("Content-Type", "text/plain")]));
        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", Blocked(page.Id, session, "request-lost", method: "POST", bodySize: null));
        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", Blocked(page.Id, session, "request-empty", method: "POST", bodySize: 0));
        await session.RemoteEnd.WaitForCommandAsync("network.provideResponse", 5);

        JsonObject collector = Assert.Single(session.RemoteEnd.CommandsFor("network.addDataCollector"))["params"]!.AsObject();
        Assert.Equal(["request"], collector["dataTypes"]!.AsArray().Select(type => (string?)type));
        Assert.Equal([page.Id], collector["contexts"]!.AsArray().Select(context => (string?)context));
        Assert.Equal(RecordedForm.Length + 4096, (int?)collector["maxEncodedDataSize"]);
        IReadOnlyList<JsonObject> reads = session.RemoteEnd.CommandsFor("network.getData");
        Assert.Equal(["request-text", "request-form", "request-unrecorded", "request-lost"], reads.Select(command => (string?)command["params"]!["request"]));
        Assert.All(reads, read => Assert.False(read["params"]!.AsObject().ContainsKey("disown")));
        Dictionary<string, string> answers = session.RemoteEnd.CommandsFor("network.provideResponse").ToDictionary(command => (string)command["params"]!["request"]!, command => Body(command["params"]!.AsObject()));
        Assert.Equal("text body", answers["request-text"]);
        Assert.Equal("form body", answers["request-form"]);
        Assert.Equal("no recorded body", answers["request-unrecorded"]);
        Assert.Equal("no recorded body", answers["request-lost"]);
        Assert.Equal("no recorded body", answers["request-empty"]);
    }

    [Fact]
    public async Task RemovingTheRouteOrDisposingTheGroupRemovesItsCollector()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        string har = this.WriteHar(Entry("POST", RequestUrl, 200, "OK", Content("posted"), requestBody: Content("body")));

        RouteRegistration removed = await page.RouteFromHarAsync(har, cancellationToken: TestContext.Current.CancellationToken);
        await page.Browser.RouteFromHarAsync(har, cancellationToken: TestContext.Current.CancellationToken);
        await removed.RemoveAsync(TestContext.Current.CancellationToken);
        await group.DisposeAsync();

        IReadOnlyList<JsonObject> added = session.RemoteEnd.CommandsFor("network.addDataCollector");
        Assert.Equal([page.Browser.Id], added[1]["params"]!["userContexts"]!.AsArray().Select(userContext => (string?)userContext));
        Assert.Equal(session.RemoteEnd.ResultsFor("network.addDataCollector").Select(result => (string?)result["collector"]), session.RemoteEnd.CommandsFor("network.removeDataCollector").Select(command => (string?)command["params"]!["collector"]));
    }

    [Fact]
    public async Task CollectorIsRemovedWhenTheRouteCannotBeAdded()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        string har = this.WriteHar(Entry("POST", RequestUrl, 200, "OK", Content("posted"), requestBody: Content("body")));
        session.RemoteEnd.FailWith("network.addIntercept", "unknown error", "no intercepts today");

        await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => page.Browser.RouteFromHarAsync(har, RequestUrl, cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<WebDriverBiDiCommandException>(() => page.RouteFromHarAsync(har, RequestUrl, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(2, session.RemoteEnd.CommandsFor("network.removeDataCollector").Count);
    }

    [Fact]
    public async Task BrowserRoutesFromHarAnswerItsRequests()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        string har = this.WriteHar(Entry("GET", RequestUrl, 200, "OK", Content("replayed")));

        RouteRegistration byUrl = await page.Browser.RouteFromHarAsync(har, RequestUrl, cancellationToken: TestContext.Current.CancellationToken);
        RouteRegistration byPattern = await page.Browser.RouteFromHarAsync(har, new Regex("/api/"), cancellationToken: TestContext.Current.CancellationToken);
        RouteRegistration byCondition = await page.Browser.RouteFromHarAsync(har, _ => true, new HarRouteOptions() { Filter = new UrlPatternPattern() { HostName = "example.com" } }, TestContext.Current.CancellationToken);
        JsonObject response = await RaiseAndWaitForAsync(session, "network.provideResponse", Blocked(page.Id, session));

        Assert.Equal(["https://example.com/api/data.json from the HAR har.har", "URLs matching /api/ from the HAR har.har", "requests satisfying the condition from the HAR har.har"], new[] { byUrl, byPattern, byCondition }.Select(route => route.Description));
        Assert.Equal("example.com", (string?)session.RemoteEnd.CommandsFor("network.addIntercept")[2]["params"]!["urlPatterns"]![0]!["hostname"]);
        Assert.Equal("replayed", Body(response));
    }

    [Fact]
    public async Task BodiesAreReadFromTheHarFromFilesBesideItOrFromAZip()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        JsonObject inline = Entry("GET", "https://example.com/inline", 200, "OK", new JsonObject() { ["text"] = Convert.ToBase64String([1, 2, 3]), ["encoding"] = "base64" });
        JsonObject beside = Entry("GET", "https://example.com/beside", 200, "OK", new JsonObject() { ["_file"] = "body.bin" });
        JsonObject bare = Entry("GET", "https://example.com/bare", 204, "No Content", new JsonObject());
        bare["response"]!.AsObject().Remove("content");
        bare["response"]!.AsObject().Remove("statusText");
        bare["response"]!.AsObject().Remove("headers");
        bare["response"]!.AsObject().Remove("redirectURL");
        bare["request"]!.AsObject().Remove("headers");
        bare["request"]!["postData"] = null;
        JsonObject nullText = Entry("GET", "https://example.com/null-text", 200, "OK", new JsonObject() { ["text"] = null, ["encoding"] = "identity" });
        nullText["response"]!["redirectURL"] = null;
        nullText["response"]!["statusText"] = null;
        JsonObject noText = Entry("GET", "https://example.com/no-text", 200, "OK", new JsonObject() { ["mimeType"] = "text/plain" });
        File.WriteAllBytes(Path.Combine(this.directory, "body.bin"), [4, 5, 6]);
        string har = this.WriteHar(inline, beside, bare, nullText, noText);
        string zip = Path.Combine(this.directory, "recording.zip");
        using (ZipArchive archive = ZipFile.Open(zip, ZipArchiveMode.Create))
        {
            archive.CreateEntryFromFile(Path.Combine(this.directory, "body.bin"), "zipped.bin");
            JsonObject zipped = Entry("GET", "https://example.com/zipped", 200, "OK", new JsonObject() { ["_file"] = "zipped.bin" });
            using StreamWriter writer = new(archive.CreateEntry("har.har").Open());
            writer.Write(Har(zipped).ToJsonString());
        }

        await page.RouteFromHarAsync(har, cancellationToken: TestContext.Current.CancellationToken);
        await page.RouteFromHarAsync(zip, "https://example.com/zipped", cancellationToken: TestContext.Current.CancellationToken);
        string[] urls = ["https://example.com/inline", "https://example.com/beside", "https://example.com/bare", "https://example.com/null-text", "https://example.com/zipped", "https://example.com/no-text"];
        for (int index = 0; index < urls.Length; index++)
        {
            await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", Blocked(page.Id, session, $"request-{index}", urls[index]));
        }

        await session.RemoteEnd.WaitForCommandAsync("network.provideResponse", urls.Length);
        Dictionary<string, byte[]> bodies = session.RemoteEnd.CommandsFor("network.provideResponse").ToDictionary(command => (string)command["params"]!["request"]!, command => Convert.FromBase64String((string)command["params"]!["body"]!["value"]!));
        Assert.Equal([1, 2, 3], bodies["request-0"]);
        Assert.Equal([4, 5, 6], bodies["request-1"]);
        Assert.Empty(bodies["request-2"]);
        Assert.Empty(bodies["request-3"]);
        Assert.Equal([4, 5, 6], bodies["request-4"]);
        Assert.Empty(bodies["request-5"]);
    }

    [Fact]
    public async Task FilesThatAreNotArchivesItCanReadAreRejected()
    {
        (BiDiDriver driver, FakeSession session, BrowserGroup group, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;
        string notJson = this.WriteFile("not-json.har", "not json");
        string noEntries = this.WriteFile("no-entries.har", "{\"log\":{}}");
        string entriesNotArray = this.WriteFile("entries-object.har", "{\"log\":{\"entries\":{}}}");
        string noLog = this.WriteFile("no-log.har", "{}");
        string noUrl = this.WriteFile("no-url.har", Har(new JsonObject() { ["request"] = new JsonObject() { ["method"] = "GET" }, ["response"] = new JsonObject() { ["status"] = 200 } }).ToJsonString());
        string escaping = this.WriteFile("escaping.har", Har(Entry("GET", RequestUrl, 200, "OK", new JsonObject() { ["_file"] = "../outside.bin" })).ToJsonString());
        string missingZip = Path.Combine(this.directory, "no-har.zip");
        using (ZipArchive archive = ZipFile.Open(missingZip, ZipArchiveMode.Create))
        {
            archive.CreateEntry("readme.txt");
        }

        string missingEntry = Path.Combine(this.directory, "missing-entry.zip");
        using (ZipArchive archive = ZipFile.Open(missingEntry, ZipArchiveMode.Create))
        {
            using StreamWriter writer = new(archive.CreateEntry("har.har").Open());
            writer.Write(Har(Entry("GET", RequestUrl, 200, "OK", new JsonObject() { ["_file"] = "absent.bin" })).ToJsonString());
        }

        async Task<string> RejectedAsync(string path) => (await Assert.ThrowsAsync<InvalidDataException>(() => page.RouteFromHarAsync(path, cancellationToken: TestContext.Current.CancellationToken))).Message;

        Assert.StartsWith($"The file {notJson} is not a HAR: ", await RejectedAsync(notJson));
        Assert.Equal($"The file {noEntries} is not a HAR: it has no log.entries.", await RejectedAsync(noEntries));
        Assert.Equal($"The file {entriesNotArray} is not a HAR: it has no log.entries.", await RejectedAsync(entriesNotArray));
        Assert.Equal($"The file {noLog} is not a HAR: it has no log.entries.", await RejectedAsync(noLog));
        Assert.StartsWith($"The file {noUrl} is not a HAR: ", await RejectedAsync(noUrl));
        Assert.Equal($"The HAR {escaping} names a body file, ../outside.bin, outside its directory.", await RejectedAsync(escaping));
        Assert.Equal($"The archive {missingZip} holds no .har file.", await RejectedAsync(missingZip));
        Assert.Equal($"The archive {missingEntry} has no file absent.bin, which an entry's _file names.", await RejectedAsync(missingEntry));
        Assert.Empty(session.RemoteEnd.CommandsFor("network.addIntercept"));
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, BrowserGroup Group, Page Page)> OpenPageAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, group, page);
    }

    private static async Task<JsonObject> RaiseAndWaitForAsync(FakeSession session, string method, JsonObject blocked)
    {
        await session.RemoteEnd.RaiseEventAsync("network.beforeRequestSent", blocked);
        return (await session.RemoteEnd.WaitForCommandAsync(method).WaitAsync(EventWait, TestContext.Current.CancellationToken))["params"]!.AsObject();
    }

    private static JsonObject Blocked(string contextId, FakeSession session, string requestId = "request-1", string url = RequestUrl, string method = "GET", ulong? bodySize = 0, (string Name, string Value)[]? headers = null)
    {
        JsonObject request = new()
        {
            ["request"] = requestId,
            ["url"] = url,
            ["method"] = method,
            ["headers"] = new JsonArray([.. (headers ?? []).Select(header => (JsonNode)new JsonObject() { ["name"] = header.Name, ["value"] = new JsonObject() { ["type"] = "string", ["value"] = header.Value } })]),
            ["cookies"] = new JsonArray(),
            ["destination"] = string.Empty,
            ["initiatorType"] = "fetch",
            ["headersSize"] = 0,
            ["bodySize"] = bodySize,
            ["timings"] = new JsonObject()
            {
                ["timeOrigin"] = 0, ["requestTime"] = 0, ["redirectStart"] = 0, ["redirectEnd"] = 0, ["fetchStart"] = 0, ["dnsStart"] = 0, ["dnsEnd"] = 0,
                ["connectStart"] = 0, ["connectEnd"] = 0, ["tlsStart"] = 0, ["requestStart"] = 0, ["responseStart"] = 0, ["responseEnd"] = 0,
            },
        };
        return new JsonObject()
        {
            ["context"] = contextId,
            ["navigation"] = null,
            ["isBlocked"] = true,
            ["redirectCount"] = 0,
            ["timestamp"] = 1790000000000,
            ["request"] = request,
            ["initiator"] = new JsonObject() { ["type"] = "script" },
            ["intercepts"] = new JsonArray([.. session.RemoteEnd.ResultsFor("network.addIntercept").Select(result => (JsonNode)(string)result["intercept"]!)]),
        };
    }

    private static JsonObject Content(string text) => new() { ["mimeType"] = "text/plain", ["text"] = text };

    private static JsonObject Entry(string method, string url, int status, string statusText, JsonObject content, (string Name, string Value)[]? requestHeaders = null, (string Name, string Value)[]? responseHeaders = null, JsonObject? requestBody = null)
    {
        static JsonArray HeaderArray((string Name, string Value)[]? headers) => new([.. (headers ?? []).Select(header => (JsonNode)new JsonObject() { ["name"] = header.Name, ["value"] = header.Value })]);
        JsonObject request = new() { ["method"] = method, ["url"] = url, ["headers"] = HeaderArray(requestHeaders) };
        if (requestBody is not null)
        {
            request["postData"] = requestBody;
        }

        return new JsonObject()
        {
            ["request"] = request,
            ["response"] = new JsonObject() { ["status"] = status, ["statusText"] = statusText, ["headers"] = HeaderArray(responseHeaders), ["content"] = content, ["redirectURL"] = string.Empty },
        };
    }

    private static JsonObject Har(params JsonObject[] entries) => new() { ["log"] = new JsonObject() { ["version"] = "1.2", ["entries"] = new JsonArray([.. entries]) } };

    private static string Body(JsonObject response) => Encoding.UTF8.GetString(Convert.FromBase64String((string)response["body"]!["value"]!));

    private static IEnumerable<string> Headers(JsonObject response) => response["headers"]!.AsArray().Select(header => $"{header!["name"]}: {header["value"]!["value"]}");

    private string WriteHar(params JsonObject[] entries) => this.WriteFile("har.har", Har(entries).ToJsonString());

    private string WriteFile(string name, string contents)
    {
        string path = Path.Combine(this.directory, name);
        File.WriteAllText(path, contents);
        return path;
    }
}
