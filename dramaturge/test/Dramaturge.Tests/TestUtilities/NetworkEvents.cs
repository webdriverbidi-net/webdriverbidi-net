// <copyright file="NetworkEvents.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.TestUtilities;

using System.Text.Json.Nodes;
using Dramaturge.Network;
using WebDriverBiDi;
using WebDriverBiDi.Network;

/// <summary>
/// Raises network events from a <see cref="FakeRemoteEnd"/>, shaped as the protocol requires.
/// </summary>
public static class NetworkEvents
{
    /// <summary>
    /// The browsing context from which events are raised.
    /// </summary>
    public const string ContextId = "context-1";

    /// <summary>
    /// Waits until every event raised so far has been dispatched to its handlers: the transport processes
    /// messages in order, so a command's response arrives after them.
    /// </summary>
    /// <param name="driver">The driver.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task FlushAsync(BiDiDriver driver) => driver.Session.StatusAsync(new WebDriverBiDi.Session.StatusCommandParameters());

    /// <summary>
    /// Captures, with a monitor using the default options, the requests that the given events describe.
    /// </summary>
    /// <param name="raiseEvents">Raises the events.</param>
    /// <param name="getData">Answers network.getData, or <see langword="null"/> to answer every request with a text body.</param>
    /// <returns>The captured requests.</returns>
    public static async Task<IReadOnlyList<NetworkRequest>> CaptureAsync(Func<FakeRemoteEnd, Task> raiseEvents, Func<JsonObject, JsonNode>? getData = null)
    {
        (BiDiDriver driver, FakeRemoteEnd remoteEnd) = await FakeRemoteEnd.ConnectAsync();
        await using BiDiDriver ownedDriver = driver;
        remoteEnd.AnswerWith("network.getData", getData ?? (_ => Bytes("string", "body")));
        await using NetworkTrafficMonitor monitor = new(driver);
        await monitor.StartMonitoringAsync(TestContext.Current.CancellationToken);
        await raiseEvents(remoteEnd);
        await FlushAsync(driver);
        return await monitor.GetCapturedTrafficAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Creates a network.getData result.
    /// </summary>
    /// <param name="type">The type of the bytes value, "string" or "base64".</param>
    /// <param name="value">The value.</param>
    /// <returns>The result.</returns>
    public static JsonNode Bytes(string type, string value) => new JsonObject() { ["bytes"] = new JsonObject() { ["type"] = type, ["value"] = value } };

    /// <summary>
    /// Raises network.beforeRequestSent.
    /// </summary>
    /// <param name="remoteEnd">The remote end.</param>
    /// <param name="requestId">The request ID.</param>
    /// <param name="method">The HTTP method.</param>
    /// <param name="bodySize">The size of the request body.</param>
    /// <param name="redirectCount">The number of redirects that led to the request.</param>
    /// <param name="intercepts">The IDs of the intercepts blocking the request, or <see langword="null"/> if it is not blocked.</param>
    /// <param name="url">The URL.</param>
    /// <param name="navigation">The navigation the request is for, or <see langword="null"/> if it is not a navigation's request.</param>
    /// <param name="headers">Request headers, or <see langword="null"/> for the default.</param>
    /// <param name="timings">The fetch timing marks, or <see langword="null"/> for the default.</param>
    /// <param name="startMilliseconds">The event's timestamp, in milliseconds after the default start.</param>
    /// <param name="sizesKnown">A value indicating whether the body size is reported.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task BeforeRequestSentAsync(FakeRemoteEnd remoteEnd, string requestId, string method = "GET", ulong bodySize = 0, ulong redirectCount = 0, string[]? intercepts = null, string url = "https://example.com/", string? navigation = null, Dictionary<string, string>? headers = null, JsonObject? timings = null, long startMilliseconds = 0, bool sizesKnown = true)
    {
        JsonObject parameters = Base(requestId, redirectCount, intercepts, url, method, bodySize);
        if (!sizesKnown)
        {
            parameters["request"]!["bodySize"] = null;
        }

        parameters["navigation"] = navigation;
        parameters["timestamp"] = 1_790_000_000_000 + (long)redirectCount + startMilliseconds;
        if (headers is not null)
        {
            parameters["request"]!["headers"] = new JsonArray([.. headers.Select(header => (JsonNode?)Header(header.Key, header.Value))]);
        }

        if (timings is not null)
        {
            parameters["request"]!["timings"] = timings;
        }

        parameters["initiator"] = new JsonObject() { ["type"] = "other" };
        return remoteEnd.RaiseEventAsync("network.beforeRequestSent", parameters);
    }

    /// <summary>
    /// Raises network.responseCompleted.
    /// </summary>
    /// <param name="remoteEnd">The remote end.</param>
    /// <param name="requestId">The request ID.</param>
    /// <param name="status">The response status.</param>
    /// <param name="redirectCount">The number of redirects that led to the request.</param>
    /// <param name="headers">Response headers, or <see langword="null"/> for none.</param>
    /// <param name="protocol">The protocol the response came over.</param>
    /// <param name="mimeType">The response's MIME type.</param>
    /// <param name="timings">The fetch timing marks, or <see langword="null"/> for the default.</param>
    /// <param name="sizesKnown">A value indicating whether the header and body sizes are reported.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task ResponseCompletedAsync(FakeRemoteEnd remoteEnd, string requestId, ulong status = 200, ulong redirectCount = 0, Dictionary<string, string>? headers = null, string protocol = "http/1.1", string mimeType = "text/html", JsonObject? timings = null, bool sizesKnown = true)
    {
        JsonObject parameters = Base(requestId, redirectCount, null, "https://example.com/", "GET", 0);
        if (timings is not null)
        {
            parameters["request"]!["timings"] = timings;
        }

        JsonObject response = Response(status, headers, null);
        response["protocol"] = protocol;
        response["mimeType"] = mimeType;
        if (!sizesKnown)
        {
            response["headersSize"] = null;
            response["bodySize"] = null;
        }

        parameters["response"] = response;
        return remoteEnd.RaiseEventAsync("network.responseCompleted", parameters);
    }

    /// <summary>
    /// Raises network.fetchError.
    /// </summary>
    /// <param name="remoteEnd">The remote end.</param>
    /// <param name="requestId">The request ID.</param>
    /// <param name="errorText">The error text.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task FetchErrorAsync(FakeRemoteEnd remoteEnd, string requestId, string errorText)
    {
        JsonObject parameters = Base(requestId, 0, null, "https://example.com/", "GET", 0);
        parameters["errorText"] = errorText;
        return remoteEnd.RaiseEventAsync("network.fetchError", parameters);
    }

    /// <summary>
    /// Raises network.authRequired.
    /// </summary>
    /// <param name="remoteEnd">The remote end.</param>
    /// <param name="requestId">The request ID.</param>
    /// <param name="interceptId">The intercept blocking the request, or <see langword="null"/> if it is not blocked.</param>
    /// <param name="scheme">The challenge's scheme.</param>
    /// <param name="realm">The challenge's realm.</param>
    /// <param name="challenges">The challenges, overriding the one described by <paramref name="scheme"/> and <paramref name="realm"/>; an empty string omits them.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task AuthRequiredAsync(FakeRemoteEnd remoteEnd, string? interceptId, string requestId = "request-1", string scheme = "Basic", string realm = "site", string? challenges = null)
    {
        JsonObject parameters = Base(requestId, 0, interceptId is null ? null : [interceptId], "https://example.com/", "GET", 0);
        JsonArray? authChallenges = challenges switch
        {
            null => new JsonArray(new JsonObject() { ["scheme"] = scheme, ["realm"] = realm }),
            "" => null,
            _ => JsonNode.Parse(challenges)!.AsArray(),
        };
        parameters["response"] = Response(401, null, authChallenges);
        return remoteEnd.RaiseEventAsync("network.authRequired", parameters);
    }

    private static JsonObject Base(string requestId, ulong redirectCount, string[]? intercepts, string url, string method, ulong bodySize)
    {
        JsonObject parameters = new()
        {
            ["context"] = ContextId,
            ["navigation"] = null,
            ["isBlocked"] = intercepts is not null,
            ["redirectCount"] = redirectCount,
            ["timestamp"] = 1_790_000_000_000 + (long)redirectCount,
            ["request"] = new JsonObject()
            {
                ["request"] = requestId,
                ["url"] = url,
                ["method"] = method,
                ["headers"] = new JsonArray(Header("accept", "*/*")),
                ["cookies"] = new JsonArray(),
                ["destination"] = "document",
                ["initiatorType"] = "other",
                ["headersSize"] = 100,
                ["bodySize"] = bodySize,
                ["timings"] = new JsonObject()
                {
                    ["timeOrigin"] = 0, ["requestTime"] = 0, ["redirectStart"] = 0, ["redirectEnd"] = 0, ["fetchStart"] = 1, ["dnsStart"] = 0, ["dnsEnd"] = 0,
                    ["connectStart"] = 0, ["connectEnd"] = 0, ["tlsStart"] = 0, ["requestStart"] = 2, ["responseStart"] = 5, ["responseEnd"] = 8,
                },
            },
        };
        if (intercepts is not null)
        {
            parameters["intercepts"] = new JsonArray([.. intercepts.Select(intercept => (JsonNode?)intercept)]);
        }

        return parameters;
    }

    private static JsonObject Response(ulong status, Dictionary<string, string>? headers, JsonArray? authChallenges)
    {
        JsonObject response = new()
        {
            ["url"] = "https://example.com/",
            ["protocol"] = "http/1.1",
            ["status"] = status,
            ["statusText"] = status == 200 ? "OK" : "Status",
            ["fromCache"] = false,
            ["headers"] = new JsonArray([.. (headers ?? []).Select(header => (JsonNode?)Header(header.Key, header.Value))]),
            ["mimeType"] = "text/html",
            ["bytesReceived"] = 300,
            ["headersSize"] = 100,
            ["bodySize"] = 200,
            ["content"] = new JsonObject() { ["size"] = 200 },
        };
        if (authChallenges is not null)
        {
            response["authChallenges"] = authChallenges;
        }

        return response;
    }

    private static JsonObject Header(string name, string value) => new() { ["name"] = name, ["value"] = new JsonObject() { ["type"] = "string", ["value"] = value } };
}
