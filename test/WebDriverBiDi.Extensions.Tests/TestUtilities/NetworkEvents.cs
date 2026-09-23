// <copyright file="NetworkEvents.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.TestUtilities;

using System.Text.Json.Nodes;

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
    /// Raises network.beforeRequestSent.
    /// </summary>
    /// <param name="remoteEnd">The remote end.</param>
    /// <param name="requestId">The request ID.</param>
    /// <param name="method">The HTTP method.</param>
    /// <param name="bodySize">The size of the request body.</param>
    /// <param name="redirectCount">The number of redirects that led to the request.</param>
    /// <param name="intercepts">The IDs of the intercepts blocking the request, or <see langword="null"/> if it is not blocked.</param>
    /// <param name="url">The URL.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task BeforeRequestSentAsync(FakeRemoteEnd remoteEnd, string requestId, string method = "GET", ulong bodySize = 0, ulong redirectCount = 0, string[]? intercepts = null, string url = "https://example.com/")
    {
        JsonObject parameters = Base(requestId, redirectCount, intercepts, url, method, bodySize);
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
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task ResponseCompletedAsync(FakeRemoteEnd remoteEnd, string requestId, ulong status = 200, ulong redirectCount = 0, Dictionary<string, string>? headers = null)
    {
        JsonObject parameters = Base(requestId, redirectCount, null, "https://example.com/", "GET", 0);
        parameters["response"] = Response(status, headers, null);
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
    /// Raises network.authRequired for a request blocked by an intercept.
    /// </summary>
    /// <param name="remoteEnd">The remote end.</param>
    /// <param name="requestId">The request ID.</param>
    /// <param name="interceptId">The intercept blocking the request.</param>
    /// <param name="scheme">The challenge's scheme.</param>
    /// <param name="realm">The challenge's realm.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task AuthRequiredAsync(FakeRemoteEnd remoteEnd, string requestId, string interceptId, string scheme = "Basic", string realm = "site")
    {
        JsonObject parameters = Base(requestId, 0, [interceptId], "https://example.com/", "GET", 0);
        parameters["response"] = Response(401, null, new JsonArray(new JsonObject() { ["scheme"] = scheme, ["realm"] = realm }));
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
