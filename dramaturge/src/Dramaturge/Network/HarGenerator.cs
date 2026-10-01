// <copyright file="HarGenerator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Network;

using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Network;

/// <summary>
/// Generates an HTTP Archive (HAR 1.2) document from captured <see cref="NetworkRequest"/> objects.
/// </summary>
/// <remarks>
/// Each navigation becomes a page, and every other request is assigned to the latest page of its browsing context.
/// A request body that is not text is written as base64 in <c>postData.text</c>, marked by the custom field
/// <c>_encoding</c>, as HAR's <c>postData</c> has no encoding of its own; a failed request has status 0 and its
/// error in the custom field <c>_error</c>.
/// </remarks>
public static partial class HarGenerator
{
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    /// <summary>
    /// Generates a HAR 1.2 JSON document from the supplied network requests.
    /// </summary>
    /// <param name="requests">The captured network requests.</param>
    /// <param name="creatorName">The name recorded as the HAR's creator.</param>
    /// <param name="creatorVersion">The version recorded as the HAR's creator, or <see langword="null"/> for this library's version.</param>
    /// <returns>The HAR document.</returns>
    public static string Generate(IEnumerable<NetworkRequest> requests, string creatorName = "Dramaturge", string? creatorVersion = null)
    {
        List<NetworkRequest> ordered = [.. requests.OrderBy(request => request.StartedDateTime).ThenBy(request => request.RedirectCount)];
        List<HarPage> pages = BuildPages(ordered);
        HarLog log = new()
        {
            Creator = new HarCreator { Name = creatorName, Version = creatorVersion ?? GetLibraryVersion() },
            Pages = pages,
            Entries = [.. ordered.Select(request => BuildEntry(request, FindPageId(request, ordered, pages)))],
        };
        return JsonSerializer.Serialize(new HarRoot { Log = log }, HarJsonSerializerContext.Default.HarRoot);
    }

    private static string GetLibraryVersion()
    {
        // The SDK always generates the attribute.
        return typeof(HarGenerator).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
    }

    // A page for each navigation, named by the URL its redirects ended at.
    private static List<HarPage> BuildPages(List<NetworkRequest> ordered)
    {
        return [.. ordered.Where(request => request.NavigationId is not null)
            .GroupBy(request => request.NavigationId!)
            .Select(navigation => new HarPage
            {
                Id = navigation.Key,
                StartedDateTime = FormatTime(navigation.First().StartedDateTime),
                Title = navigation.Last().Url,
            })];
    }

    private static string? FindPageId(NetworkRequest request, List<NetworkRequest> ordered, List<HarPage> pages)
    {
        if (request.NavigationId is not null)
        {
            return request.NavigationId;
        }

        return ordered.LastOrDefault(candidate => candidate.NavigationId is not null
            && candidate.BrowsingContextId == request.BrowsingContextId
            && candidate.StartedDateTime <= request.StartedDateTime)?.NavigationId;
    }

    private static HarEntry BuildEntry(NetworkRequest request, string? pageId)
    {
        HarTimings timings = BuildTimings(request.Timings);
        string httpVersion = ToHttpVersion(request.ResponseProtocol);
        return new HarEntry
        {
            PageRef = pageId,
            StartedDateTime = FormatTime(request.StartedDateTime),
            Time = Math.Max(0, timings.Blocked) + Math.Max(0, timings.Dns) + Math.Max(0, timings.Connect) + timings.Send + timings.Wait + timings.Receive,
            Request = BuildRequest(request, httpVersion),
            Response = BuildResponse(request, httpVersion),
            Timings = timings,
            Error = request.FetchErrorText,
        };
    }

    // The marks are milliseconds from the time origin, and 0 when the phase did not happen (a reused connection has
    // no DNS lookup or connect, for example). The phases HAR requires are never negative.
    private static HarTimings BuildTimings(FetchTimingInfo marks)
    {
        static double Phase(double start, double end) => start > 0 && end >= start ? end - start : -1;
        double firstNetworkMark = new[] { marks.DnsStart, marks.ConnectStart, marks.RequestStart }.FirstOrDefault(mark => mark > 0);
        return new HarTimings
        {
            Blocked = Phase(marks.FetchStart, firstNetworkMark),
            Dns = Phase(marks.DnsStart, marks.DnsEnd),
            Connect = Phase(marks.ConnectStart, marks.ConnectEnd),
            Ssl = Phase(marks.TlsStart, marks.ConnectEnd),
            Send = 0,
            Wait = Math.Max(0, Phase(marks.RequestStart, marks.ResponseStart)),
            Receive = Math.Max(0, Phase(marks.ResponseStart, marks.ResponseEnd)),
        };
    }

    private static HarRequest BuildRequest(NetworkRequest request, string httpVersion)
    {
        HarRequest harRequest = new()
        {
            Method = request.Method,
            Url = request.Url,
            HttpVersion = httpVersion,
            Headers = [.. request.RequestHeaders.Select(header => new HarNameValuePair { Name = header.Name, Value = header.Value.Value })],
            Cookies = [.. request.RequestCookies.Select(cookie => new HarCookie { Name = cookie.Name, Value = cookie.Value.Value })],
            QueryString = ParseQueryString(request.Url),
            HeadersSize = (long)request.RequestHeadersSize,
            BodySize = (long?)request.RequestBodySize ?? -1,
        };

        if (request.RequestBody.Length > 0)
        {
            string mimeType = GetHeaderValue(request.RequestHeaders, "content-type") ?? "application/octet-stream";
            (string text, bool isBase64) = request.IsRequestBodyBase64Encoded ? DecodeIfText(request.RequestBody) : (request.RequestBody, false);
            harRequest.PostData = new HarPostData
            {
                MimeType = mimeType,
                Text = text,
                Encoding = isBase64 ? "base64" : null,
                Params = !isBase64 && mimeType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) ? ParseFormParameters(text) : null,
            };
        }
        else if (request.RequestBodyErrorText is not null)
        {
            harRequest.Comment = $"Body unavailable: {request.RequestBodyErrorText}";
        }

        return harRequest;
    }

    private static HarResponse BuildResponse(NetworkRequest request, string httpVersion)
    {
        if (request.IsFailed)
        {
            return new HarResponse { HttpVersion = httpVersion, Content = new HarContent { MimeType = "x-unknown" }, HeadersSize = -1, BodySize = -1 };
        }

        return new HarResponse
        {
            Status = (long)request.ResponseStatusCode,
            StatusText = request.ResponseStatusText,
            HttpVersion = httpVersion,
            Headers = [.. request.ResponseHeaders.Select(header => new HarNameValuePair { Name = header.Name, Value = header.Value.Value })],
            Cookies = [.. request.ResponseHeaders.Where(header => string.Equals(header.Name, "set-cookie", StringComparison.OrdinalIgnoreCase)).Select(header => ParseSetCookieHeader(header.Value.Value)).OfType<HarCookie>()],
            Content = new HarContent
            {
                Size = (long)request.ResponseContentSize,
                MimeType = string.IsNullOrEmpty(request.ResponseMimeType) ? "x-unknown" : request.ResponseMimeType,
                Text = request.ResponseBody.Length > 0 ? request.ResponseBody : null,
                Encoding = request.ResponseBody.Length > 0 && request.IsResponseBodyBase64Encoded ? "base64" : null,
                Comment = request.ResponseBodyErrorText is null ? null : $"Body unavailable: {request.ResponseBodyErrorText}",
            },
            RedirectUrl = GetHeaderValue(request.ResponseHeaders, "location") ?? string.Empty,
            HeadersSize = (long?)request.ResponseHeadersSize ?? -1,
            BodySize = (long?)request.ResponseBodySize ?? -1,
        };
    }

    // A body the browser sent as base64 is written as text when it is text, and as base64 otherwise.
    private static (string Text, bool IsBase64) DecodeIfText(string base64)
    {
        try
        {
            return (StrictUtf8.GetString(Convert.FromBase64String(base64)), false);
        }
        catch (Exception ex) when (ex is FormatException || ex is ArgumentException)
        {
            return (base64, true);
        }
    }

    // The protocol names browsers report ("http/1.1", "h2", "h3") as the HTTP versions HAR viewers expect.
    private static string ToHttpVersion(string protocol)
    {
        return protocol.ToLowerInvariant() switch
        {
            "http/1.0" => "HTTP/1.0",
            "http/1.1" => "HTTP/1.1",
            "h2" or "http/2" or "http/2.0" => "HTTP/2",
            "h3" or "http/3" => "HTTP/3",
            _ => protocol,
        };
    }

    private static string FormatTime(DateTime time) => time.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);

    private static HarCookie? ParseSetCookieHeader(string headerValue)
    {
        string[] parts = headerValue.Split(';');
        int firstEquals = parts[0].IndexOf('=');
        if (firstEquals < 0)
        {
            return null;
        }

        HarCookie cookie = new()
        {
            Name = parts[0].Substring(0, firstEquals).Trim(),
            Value = parts[0].Substring(firstEquals + 1).Trim(),
        };
        foreach (string part in parts.Skip(1))
        {
            string attribute = part.Trim();
            int equals = attribute.IndexOf('=');
            string name = equals < 0 ? attribute : attribute.Substring(0, equals);
            string value = equals < 0 ? string.Empty : attribute.Substring(equals + 1);
            switch (name.ToLowerInvariant())
            {
                case "httponly":
                    cookie.HttpOnly = true;
                    break;
                case "secure":
                    cookie.Secure = true;
                    break;
                case "domain":
                    cookie.Domain = value;
                    break;
                case "path":
                    cookie.Path = value;
                    break;
                case "expires" when DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime expires):
                    cookie.Expires = FormatTime(expires);
                    break;
            }
        }

        return cookie;
    }

    private static List<HarNameValuePair> ParseQueryString(string url)
    {
        int queryStart = url.IndexOf('?');
        if (queryStart < 0)
        {
            return [];
        }

        string query = url.Substring(queryStart + 1);
        int fragmentStart = query.IndexOf('#');
        return ParsePairs(fragmentStart < 0 ? query : query.Substring(0, fragmentStart));
    }

    private static List<HarNameValuePair> ParseFormParameters(string body) => ParsePairs(body);

    // Name-value pairs in the URL-encoded form of query strings and form bodies, where "+" is a space.
    private static List<HarNameValuePair> ParsePairs(string encoded)
    {
        return [.. encoded.Split(['&'], StringSplitOptions.RemoveEmptyEntries).Select(pair =>
        {
            int equals = pair.IndexOf('=');
            string name = equals < 0 ? pair : pair.Substring(0, equals);
            string value = equals < 0 ? string.Empty : pair.Substring(equals + 1);
            return new HarNameValuePair { Name = Uri.UnescapeDataString(name.Replace('+', ' ')), Value = Uri.UnescapeDataString(value.Replace('+', ' ')) };
        })];
    }

    private static string? GetHeaderValue(IReadOnlyList<ReadOnlyHeader> headers, string name)
    {
        return headers.FirstOrDefault(header => string.Equals(header.Name, name, StringComparison.OrdinalIgnoreCase))?.Value.Value;
    }

    // HAR POCO types — internal, serialised only by this class.
    [JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(HarRoot))]
    private sealed partial class HarJsonSerializerContext : JsonSerializerContext
    {
    }

    private sealed class HarRoot
    {
        [JsonPropertyName("log")]
        public HarLog Log { get; set; } = new();
    }

    private sealed class HarLog
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.2";

        [JsonPropertyName("creator")]
        public HarCreator Creator { get; set; } = new();

        [JsonPropertyName("pages")]
        public List<HarPage> Pages { get; set; } = [];

        [JsonPropertyName("entries")]
        public List<HarEntry> Entries { get; set; } = [];
    }

    private sealed class HarCreator
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    }

    private sealed class HarPage
    {
        [JsonPropertyName("startedDateTime")]
        public string StartedDateTime { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        // The page's load events are not observed, so their times are unknown.
        [JsonPropertyName("pageTimings")]
        public HarPageTimings PageTimings { get; set; } = new();
    }

    private sealed class HarPageTimings
    {
        [JsonPropertyName("onContentLoad")]
        public double OnContentLoad { get; set; } = -1;

        [JsonPropertyName("onLoad")]
        public double OnLoad { get; set; } = -1;
    }

    private sealed class HarEntry
    {
        [JsonPropertyName("pageref")]
        public string? PageRef { get; set; }

        [JsonPropertyName("startedDateTime")]
        public string StartedDateTime { get; set; } = string.Empty;

        [JsonPropertyName("time")]
        public double Time { get; set; }

        [JsonPropertyName("request")]
        public HarRequest Request { get; set; } = new();

        [JsonPropertyName("response")]
        public HarResponse Response { get; set; } = new();

        // Nothing about the browser cache is observed, and HAR requires the object.
        [JsonPropertyName("cache")]
        public HarCache Cache { get; set; } = new();

        [JsonPropertyName("timings")]
        public HarTimings Timings { get; set; } = new();

        [JsonPropertyName("_error")]
        public string? Error { get; set; }
    }

    private sealed class HarCache
    {
    }

    private sealed class HarRequest
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("httpVersion")]
        public string HttpVersion { get; set; } = string.Empty;

        [JsonPropertyName("cookies")]
        public List<HarCookie> Cookies { get; set; } = [];

        [JsonPropertyName("headers")]
        public List<HarNameValuePair> Headers { get; set; } = [];

        [JsonPropertyName("queryString")]
        public List<HarNameValuePair> QueryString { get; set; } = [];

        [JsonPropertyName("postData")]
        public HarPostData? PostData { get; set; }

        [JsonPropertyName("headersSize")]
        public long HeadersSize { get; set; }

        [JsonPropertyName("bodySize")]
        public long BodySize { get; set; }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }
    }

    private sealed class HarResponse
    {
        [JsonPropertyName("status")]
        public long Status { get; set; }

        [JsonPropertyName("statusText")]
        public string StatusText { get; set; } = string.Empty;

        [JsonPropertyName("httpVersion")]
        public string HttpVersion { get; set; } = string.Empty;

        [JsonPropertyName("cookies")]
        public List<HarCookie> Cookies { get; set; } = [];

        [JsonPropertyName("headers")]
        public List<HarNameValuePair> Headers { get; set; } = [];

        [JsonPropertyName("content")]
        public HarContent Content { get; set; } = new();

        [JsonPropertyName("redirectURL")]
        public string RedirectUrl { get; set; } = string.Empty;

        [JsonPropertyName("headersSize")]
        public long HeadersSize { get; set; }

        [JsonPropertyName("bodySize")]
        public long BodySize { get; set; }
    }

    private sealed class HarContent
    {
        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("encoding")]
        public string? Encoding { get; set; }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }
    }

    private sealed class HarPostData
    {
        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; } = string.Empty;

        [JsonPropertyName("params")]
        public List<HarNameValuePair>? Params { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("_encoding")]
        public string? Encoding { get; set; }
    }

    private sealed class HarTimings
    {
        [JsonPropertyName("blocked")]
        public double Blocked { get; set; }

        [JsonPropertyName("dns")]
        public double Dns { get; set; }

        [JsonPropertyName("connect")]
        public double Connect { get; set; }

        [JsonPropertyName("send")]
        public double Send { get; set; }

        [JsonPropertyName("wait")]
        public double Wait { get; set; }

        [JsonPropertyName("receive")]
        public double Receive { get; set; }

        [JsonPropertyName("ssl")]
        public double Ssl { get; set; }
    }

    private sealed class HarNameValuePair
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    private sealed class HarCookie
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("path")]
        public string? Path { get; set; }

        [JsonPropertyName("domain")]
        public string? Domain { get; set; }

        [JsonPropertyName("expires")]
        public string? Expires { get; set; }

        [JsonPropertyName("httpOnly")]
        public bool? HttpOnly { get; set; }

        [JsonPropertyName("secure")]
        public bool? Secure { get; set; }
    }
}
