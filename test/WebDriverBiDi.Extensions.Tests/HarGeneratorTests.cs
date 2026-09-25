// <copyright file="HarGeneratorTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Text;
using System.Text.Json.Nodes;
using WebDriverBiDi.Network;
using WebDriverBiDi.TestUtilities;
using static WebDriverBiDi.TestUtilities.NetworkEvents;

// Requests are captured through the monitor from events a fake remote end raises, then written as HAR, which is
// checked against the HAR 1.2 schema and for the rules the schema does not enforce.
public class HarGeneratorTests
{
    [Fact]
    public void ValidatorRejectsDocumentsThatAreNotHar()
    {
        Assert.NotEmpty(HarSchemaValidator.Validate("{}"));
        Assert.NotEmpty(HarSchemaValidator.Validate("""{"log":{"version":"1.2","creator":{"name":"x","version":"1"},"entries":[{}]}}"""));
        Assert.Empty(HarSchemaValidator.Validate("""{"log":{"version":"1.2","creator":{"name":"x","version":"1"},"entries":[]}}"""));
    }

    [Fact]
    public async Task NavigationBecomesPageThatLaterRequestsBelongTo()
    {
        IReadOnlyList<NetworkRequest> requests = await CaptureAsync(async remoteEnd =>
        {
            await BeforeRequestSentAsync(remoteEnd, "document", url: "https://example.com/start", navigation: "navigation-1");
            await ResponseCompletedAsync(remoteEnd, "document", 302, headers: new() { ["location"] = "https://example.com/" });
            await BeforeRequestSentAsync(remoteEnd, "document", redirectCount: 1, url: "https://example.com/", navigation: "navigation-1");
            await ResponseCompletedAsync(remoteEnd, "document", 200, redirectCount: 1);
            await BeforeRequestSentAsync(remoteEnd, "image", url: "https://example.com/logo.png", startMilliseconds: 10);
            await ResponseCompletedAsync(remoteEnd, "image", mimeType: "image/png");
        });

        JsonNode log = GenerateValid(requests)["log"]!;

        JsonNode page = Assert.Single(log["pages"]!.AsArray())!;
        Assert.Equal("navigation-1", (string?)page["id"]);
        Assert.Equal("https://example.com/", (string?)page["title"]);
        Assert.Equal(-1, (double?)page["pageTimings"]!["onLoad"]);
        JsonArray entries = log["entries"]!.AsArray();
        Assert.Equal(["https://example.com/start", "https://example.com/", "https://example.com/logo.png"], entries.Select(entry => (string)entry!["request"]!["url"]!));
        Assert.All(entries, entry => Assert.Equal("navigation-1", (string?)entry!["pageref"]));
        Assert.Equal("https://example.com/", (string?)entries[0]!["response"]!["redirectURL"]);
        Assert.NotNull(entries[0]!["cache"]);
    }

    [Fact]
    public async Task RequestBeforeAnyNavigationBelongsToNoPage()
    {
        IReadOnlyList<NetworkRequest> requests = await CaptureAsync(async remoteEnd =>
        {
            await BeforeRequestSentAsync(remoteEnd, "fetch");
            await ResponseCompletedAsync(remoteEnd, "fetch");
        });

        JsonNode log = GenerateValid(requests)["log"]!;

        Assert.Empty(log["pages"]!.AsArray());
        Assert.Null(log["entries"]![0]!["pageref"]);
    }

    [Theory]
    [InlineData("string", "<html></html>", "<html></html>", null)]
    [InlineData("base64", "iVBORw0KGgo=", "iVBORw0KGgo=", "base64")]
    public async Task ResponseBodyIsWrittenAsTheBrowserSentIt(string bytesType, string body, string expectedText, string? expectedEncoding)
    {
        IReadOnlyList<NetworkRequest> requests = await CaptureAsync(
            async remoteEnd =>
            {
                await BeforeRequestSentAsync(remoteEnd, "request-1");
                await ResponseCompletedAsync(remoteEnd, "request-1");
            },
            _ => Bytes(bytesType, body));

        JsonNode content = GenerateValid(requests)["log"]!["entries"]![0]!["response"]!["content"]!;

        Assert.Equal(expectedText, (string?)content["text"]);
        Assert.Equal(expectedEncoding, (string?)content["encoding"]);
        Assert.Equal(200, (int?)content["size"]);
    }

    public static TheoryData<string?, string, string, string, string?, string?> RequestBodies => new()
    {
        { null, "string", "hello", "hello", null, null },
        { "text/plain", "string", "hello", "hello", null, null },
        { "application/json", "base64", Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"a\":1}")), "{\"a\":1}", null, null },
        { "application/octet-stream", "base64", Convert.ToBase64String([0xFF, 0xFE, 0x00]), Convert.ToBase64String([0xFF, 0xFE, 0x00]), "base64", null },
        { "application/x-www-form-urlencoded", "string", "name=Jane+Doe&city=S%C3%A3o+Paulo&flag", "name=Jane+Doe&city=S%C3%A3o+Paulo&flag", null, "name=Jane Doe;city=São Paulo;flag=" },
    };

    [Theory]
    [MemberData(nameof(RequestBodies))]
    public async Task RequestBodyIsWrittenAsTextWhereItIsText(string? contentType, string bytesType, string body, string expectedText, string? expectedEncoding, string? expectedParameters)
    {
        IReadOnlyList<NetworkRequest> requests = await CaptureAsync(
            async remoteEnd =>
            {
                await BeforeRequestSentAsync(remoteEnd, "request-1", "POST", bodySize: 20, headers: contentType is null ? null : new() { ["content-type"] = contentType });
                await ResponseCompletedAsync(remoteEnd, "request-1");
            },
            parameters => (string?)parameters["dataType"] == "request" ? Bytes(bytesType, body) : Bytes("string", string.Empty));

        JsonNode request = GenerateValid(requests)["log"]!["entries"]![0]!["request"]!;

        JsonNode postData = request["postData"]!;
        Assert.Equal(contentType ?? "application/octet-stream", (string?)postData["mimeType"]);
        Assert.Equal(expectedText, (string?)postData["text"]);
        Assert.Equal(expectedEncoding, (string?)postData["_encoding"]);
        Assert.Equal(expectedParameters, postData["params"] is JsonArray parameters ? string.Join(";", parameters.Select(p => $"{p!["name"]}={p["value"]}")) : null);
        Assert.Equal(20, (int?)request["bodySize"]);
    }

    [Fact]
    public async Task TimingPhasesComeFromTheirMarksAndSumToTheTime()
    {
        JsonObject marks = new()
        {
            ["timeOrigin"] = 0, ["requestTime"] = 0, ["redirectStart"] = 0, ["redirectEnd"] = 0, ["fetchStart"] = 10, ["dnsStart"] = 12, ["dnsEnd"] = 20,
            ["connectStart"] = 20, ["connectEnd"] = 50, ["tlsStart"] = 30, ["requestStart"] = 50, ["responseStart"] = 80, ["responseEnd"] = 95,
        };
        IReadOnlyList<NetworkRequest> requests = await CaptureAsync(async remoteEnd =>
        {
            await BeforeRequestSentAsync(remoteEnd, "request-1");
            await ResponseCompletedAsync(remoteEnd, "request-1", timings: marks);
        });

        JsonNode entry = GenerateValid(requests)["log"]!["entries"]![0]!;

        JsonNode timings = entry["timings"]!;
        Assert.Equal([2.0, 8, 30, 20, 0, 30, 15], new[] { "blocked", "dns", "connect", "ssl", "send", "wait", "receive" }.Select(phase => (double)timings[phase]!));
        Assert.Equal(85.0, (double?)entry["time"]);
    }

    [Fact]
    public async Task PhasesThatDidNotHappenAreUnknownAndTheRequiredOnesZero()
    {
        JsonObject marks = new()
        {
            ["timeOrigin"] = 0, ["requestTime"] = 0, ["redirectStart"] = 0, ["redirectEnd"] = 0, ["fetchStart"] = 0, ["dnsStart"] = 0, ["dnsEnd"] = 0,
            ["connectStart"] = 0, ["connectEnd"] = 0, ["tlsStart"] = 0, ["requestStart"] = 0, ["responseStart"] = 0, ["responseEnd"] = 0,
        };
        IReadOnlyList<NetworkRequest> requests = await CaptureAsync(async remoteEnd =>
        {
            await BeforeRequestSentAsync(remoteEnd, "request-1");
            await ResponseCompletedAsync(remoteEnd, "request-1", timings: marks);
        });

        JsonNode entry = GenerateValid(requests)["log"]!["entries"]![0]!;

        JsonNode timings = entry["timings"]!;
        Assert.Equal([-1.0, -1, -1, -1, 0, 0, 0], new[] { "blocked", "dns", "connect", "ssl", "send", "wait", "receive" }.Select(phase => (double)timings[phase]!));
        Assert.Equal(0.0, (double?)entry["time"]);
    }

    [Fact]
    public async Task FailedRequestHasStatusZeroAndItsError()
    {
        IReadOnlyList<NetworkRequest> requests = await CaptureAsync(async remoteEnd =>
        {
            await BeforeRequestSentAsync(remoteEnd, "request-1");
            await FetchErrorAsync(remoteEnd, "request-1", "net::ERR_CONNECTION_REFUSED");
        });

        JsonNode entry = GenerateValid(requests)["log"]!["entries"]![0]!;

        Assert.Equal(0, (int?)entry["response"]!["status"]);
        Assert.Equal("net::ERR_CONNECTION_REFUSED", (string?)entry["_error"]);
    }

    [Fact]
    public async Task UnavailableBodiesAreExplainedInComments()
    {
        IReadOnlyList<NetworkRequest> requests = await CaptureAsync(async remoteEnd =>
        {
            remoteEnd.FailWith("network.getData", "no such network data", "Evicted");
            await BeforeRequestSentAsync(remoteEnd, "request-1", "POST", bodySize: 10);
            await ResponseCompletedAsync(remoteEnd, "request-1");
        });

        JsonNode entry = GenerateValid(requests)["log"]!["entries"]![0]!;

        Assert.Contains("Evicted", (string?)entry["request"]!["comment"]);
        Assert.Contains("Evicted", (string?)entry["response"]!["content"]!["comment"]);
        Assert.Null(entry["response"]!["content"]!["text"]);
    }

    [Theory]
    [InlineData("h2", "HTTP/2")]
    [InlineData("http/1.0", "HTTP/1.0")]
    [InlineData("http/1.1", "HTTP/1.1")]
    [InlineData("HTTP/2.0", "HTTP/2")]
    [InlineData("http/2", "HTTP/2")]
    [InlineData("h3", "HTTP/3")]
    [InlineData("http/3", "HTTP/3")]
    [InlineData("spdy/3", "spdy/3")]
    public async Task ProtocolIsWrittenAsHttpVersion(string protocol, string expected)
    {
        IReadOnlyList<NetworkRequest> requests = await CaptureAsync(async remoteEnd =>
        {
            await BeforeRequestSentAsync(remoteEnd, "request-1");
            await ResponseCompletedAsync(remoteEnd, "request-1", protocol: protocol);
        });

        JsonNode entry = GenerateValid(requests)["log"]!["entries"]![0]!;

        Assert.Equal(expected, (string?)entry["request"]!["httpVersion"]);
        Assert.Equal(expected, (string?)entry["response"]!["httpVersion"]);
    }

    [Theory]
    [InlineData("https://example.com/search?q=a+b&lang=pt%2DBR#results")]
    [InlineData("https://example.com/search?q=a+b&lang=pt%2DBR")]
    public async Task CookiesAndQueryStringAreParsed(string url)
    {
        IReadOnlyList<NetworkRequest> requests = await CaptureAsync(async remoteEnd =>
        {
            await BeforeRequestSentAsync(remoteEnd, "request-1", url: url);
            await ResponseCompletedAsync(remoteEnd, "request-1", headers: new() { ["Set-Cookie"] = "session=abc; Path=/; Domain=example.com; Expires=Wed, 21 Oct 2026 07:28:00 GMT; Secure; HttpOnly; SameSite=Lax", ["set-cookie"] = "malformed" });
        });

        JsonNode entry = GenerateValid(requests)["log"]!["entries"]![0]!;

        Assert.Equal("q=a b;lang=pt-BR", string.Join(";", entry["request"]!["queryString"]!.AsArray().Select(p => $"{p!["name"]}={p["value"]}")));
        JsonNode cookie = Assert.Single(entry["response"]!["cookies"]!.AsArray())!;
        Assert.Equal("session", (string?)cookie["name"]);
        Assert.Equal("abc", (string?)cookie["value"]);
        Assert.Equal("/", (string?)cookie["path"]);
        Assert.Equal("example.com", (string?)cookie["domain"]);
        Assert.Equal("2026-10-21T07:28:00.000Z", (string?)cookie["expires"]);
        Assert.True((bool?)cookie["secure"]);
        Assert.True((bool?)cookie["httpOnly"]);
    }

    [Fact]
    public async Task UnreportedSizesAndTypeAreUnknown()
    {
        IReadOnlyList<NetworkRequest> requests = await CaptureAsync(async remoteEnd =>
        {
            await BeforeRequestSentAsync(remoteEnd, "request-1", sizesKnown: false);
            await ResponseCompletedAsync(remoteEnd, "request-1", mimeType: string.Empty, sizesKnown: false);
        });

        JsonNode entry = GenerateValid(requests)["log"]!["entries"]![0]!;

        Assert.Equal([-1, -1, -1], new[] { entry["request"]!["bodySize"]!, entry["response"]!["headersSize"]!, entry["response"]!["bodySize"]! }.Select(size => (int)size));
        Assert.Equal("x-unknown", (string?)entry["response"]!["content"]!["mimeType"]);
    }

    [Fact]
    public async Task CreatorDefaultsToThisLibrary()
    {
        IReadOnlyList<NetworkRequest> requests = await CaptureAsync(_ => Task.CompletedTask);

        JsonNode defaultCreator = GenerateValid(requests)["log"]!["creator"]!;
        JsonNode customCreator = JsonNode.Parse(HarGenerator.Generate(requests, "My Tool", "2.1"))!["log"]!["creator"]!;

        Assert.Equal("WebDriverBiDi.Extensions", (string?)defaultCreator["name"]);
        Assert.False(string.IsNullOrEmpty((string?)defaultCreator["version"]));
        Assert.Equal("My Tool 2.1", $"{customCreator["name"]} {customCreator["version"]}");
    }

    private static JsonNode GenerateValid(IReadOnlyList<NetworkRequest> requests)
    {
        string har = HarGenerator.Generate(requests);
        Assert.Empty(HarSchemaValidator.Validate(har));
        JsonNode document = JsonNode.Parse(har)!;
        foreach (JsonNode? entry in document["log"]!["entries"]!.AsArray())
        {
            JsonNode timings = entry!["timings"]!;
            double[] phases = [.. new[] { "blocked", "dns", "connect", "send", "wait", "receive" }.Select(phase => (double)timings[phase]!)];
            Assert.All(phases, phase => Assert.True(phase >= -1));
            Assert.All(phases.Skip(3), phase => Assert.True(phase >= 0));
            Assert.Equal(phases.Where(phase => phase > 0).Sum(), (double)entry["time"]!, 3);
        }

        return document;
    }
}
