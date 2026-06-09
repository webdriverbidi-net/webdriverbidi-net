// <copyright file="HarGenerator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Network;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Network;

/// <summary>
/// Generates an HTTP Archive (HAR) JSON document from a list of captured <see cref="NetworkRequest"/> objects.
/// </summary>
public static class HarGenerator
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Generates a HAR 1.2 JSON string from the supplied network requests.
    /// </summary>
    /// <param name="requests">The captured network requests to include in the HAR.</param>
    /// <param name="creatorName">The name to use in the HAR creator field.</param>
    /// <param name="creatorVersion">The version to use in the HAR creator field.</param>
    /// <returns>A JSON string containing the HAR document.</returns>
    public static string Generate(IEnumerable<NetworkRequest> requests, string creatorName = "WebDriverBiDi.Client", string creatorVersion = "1.0")
    {
        HarLog log = BuildLog(requests, creatorName, creatorVersion);
        HarRoot root = new() { Log = log };
        return JsonSerializer.Serialize(root, SerializerOptions);
    }

    private static HarLog BuildLog(IEnumerable<NetworkRequest> requests, string creatorName, string creatorVersion)
    {
        HarLog log = new()
        {
            Version = "1.2",
            Creator = new HarCreator { Name = creatorName, Version = creatorVersion },
            Entries = [.. requests.Select(BuildEntry)],
        };
        return log;
    }

    private static HarEntry BuildEntry(NetworkRequest request)
    {
        FetchTimingInfo t = request.Timings;

        double dns = t.DnsEnd > 0 ? t.DnsEnd - t.DnsStart : -1;
        double connect = t.ConnectEnd > 0 ? t.ConnectEnd - t.ConnectStart : -1;
        double ssl = t.ConnectEnd > 0 && t.TlsStart > 0 ? t.ConnectEnd - t.TlsStart : -1;
        double send = t.RequestStart > 0 ? t.RequestStart - t.FetchStart : 0;
        double wait = t.ResponseStart > 0 && t.RequestStart > 0 ? t.ResponseStart - t.RequestStart : -1;
        double receive = t.ResponseEnd > 0 && t.ResponseStart > 0 ? t.ResponseEnd - t.ResponseStart : -1;
        double blocked = t.FetchStart > 0 ? t.FetchStart - t.RequestTime : -1;
        double totalTime = Math.Max(0, t.ResponseEnd - t.RequestTime);

        HarTimings timings = new()
        {
            Blocked = blocked >= 0 ? blocked : -1,
            Dns = dns,
            Connect = connect,
            Ssl = ssl,
            Send = send,
            Wait = wait >= 0 ? wait : 0,
            Receive = receive >= 0 ? receive : 0,
        };

        string requestBodyText = request.IsRequestBodyBase64Encoded
            ? Convert.ToBase64String(Convert.FromBase64String(request.RequestBody))
            : request.RequestBody;

        long requestBodySize = requestBodyText.Length > 0
            ? (long)(request.RequestBodySize ?? (ulong)requestBodyText.Length)
            : 0;

        HarRequest harRequest = new()
        {
            Method = request.Method,
            Url = request.Url,
            HttpVersion = "HTTP/1.1",
            Headers = [.. request.RequestHeaders.Select(h => new HarNameValuePair { Name = h.Name, Value = h.Value.Value })],
            Cookies = [.. request.RequestCookies.Select(BuildRequestCookie)],
            QueryString = ParseQueryString(request.Url),
            HeadersSize = (long?)request.RequestHeadersSize ?? -1,
            BodySize = requestBodySize,
            PostData = requestBodyText.Length > 0
                ? new HarPostData
                {
                    MimeType = GetRequestMimeType(request.RequestHeaders),
                    Text = request.IsRequestBodyBase64Encoded ? null : requestBodyText,
                    Encoding = request.IsRequestBodyBase64Encoded ? "base64" : null,
                }
                : null,
        };

        string responseBodyText = request.IsResponseBodyBase64Encoded
            ? Encoding.UTF8.GetString(Convert.FromBase64String(request.ResponseBody))
            : request.ResponseBody;

        HarResponse harResponse = new()
        {
            Status = (long)request.ResponseStatusCode,
            StatusText = request.ResponseStatusText,
            HttpVersion = request.ResponseProtocol,
            Headers = [.. request.ResponseHeaders.Select(h => new HarNameValuePair { Name = h.Name, Value = h.Value.Value })],
            Cookies = [.. ParseResponseCookies(request.ResponseHeaders)],
            Content = new HarContent
            {
                Size = (long)request.ResponseContentSize,
                MimeType = request.ResponseMimeType,
                Text = responseBodyText.Length > 0 ? responseBodyText : null,
                Encoding = request.IsResponseBodyBase64Encoded ? "base64" : null,
            },
            RedirectUrl = GetHeaderValue(request.ResponseHeaders, "location") ?? string.Empty,
            HeadersSize = (long?)request.ResponseHeadersSize ?? -1,
            BodySize = (long?)request.ResponseBodySize ?? -1,
        };

        return new HarEntry
        {
            StartedDateTime = request.StartedDateTime.ToString("o"),
            Time = totalTime,
            Request = harRequest,
            Response = harResponse,
            Timings = timings,
        };
    }

    private static HarCookie BuildRequestCookie(Cookie cookie) => new()
    {
        Name = cookie.Name,
        Value = cookie.Value.Value,
        Domain = cookie.Domain,
        Path = cookie.Path,
        Expires = cookie.Expires?.ToString("o"),
        HttpOnly = cookie.HttpOnly,
        Secure = cookie.Secure,
    };

    private static IEnumerable<HarCookie> ParseResponseCookies(IReadOnlyList<ReadOnlyHeader> headers)
    {
        foreach (ReadOnlyHeader header in headers)
        {
            if (!string.Equals(header.Name, "set-cookie", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            HarCookie? parsed = ParseSetCookieHeader(header.Value.Value);
            if (parsed is not null)
            {
                yield return parsed;
            }
        }
    }

    private static HarCookie? ParseSetCookieHeader(string headerValue)
    {
        string[] parts = headerValue.Split(';');
        if (parts.Length == 0)
        {
            return null;
        }

        int firstEq = parts[0].IndexOf('=');
        if (firstEq < 0)
        {
            return null;
        }

        HarCookie cookie = new()
        {
            Name = parts[0].Substring(0, firstEq).Trim(),
            Value = parts[0].Substring(firstEq + 1).Trim(),
        };

        foreach (string part in parts.Skip(1))
        {
            string trimmed = part.Trim();
            if (string.Equals(trimmed, "HttpOnly", StringComparison.OrdinalIgnoreCase))
            {
                cookie.HttpOnly = true;
            }
            else if (string.Equals(trimmed, "Secure", StringComparison.OrdinalIgnoreCase))
            {
                cookie.Secure = true;
            }
            else if (trimmed.StartsWith("Domain=", StringComparison.OrdinalIgnoreCase))
            {
                cookie.Domain = trimmed.Substring("Domain=".Length);
            }
            else if (trimmed.StartsWith("Path=", StringComparison.OrdinalIgnoreCase))
            {
                cookie.Path = trimmed.Substring("Path=".Length);
            }
            else if (trimmed.StartsWith("Expires=", StringComparison.OrdinalIgnoreCase))
            {
                cookie.Expires = trimmed.Substring("Expires=".Length);
            }
        }

        return cookie;
    }

    private static List<HarNameValuePair> ParseQueryString(string url)
    {
        List<HarNameValuePair> pairs = [];
        int questionIndex = url.IndexOf('?');
        if (questionIndex < 0 || questionIndex == url.Length - 1)
        {
            return pairs;
        }

        string query = url.Substring(questionIndex + 1);
        int fragmentIndex = query.IndexOf('#');
        if (fragmentIndex >= 0)
        {
            query = query.Substring(0, fragmentIndex);
        }

        foreach (string pair in query.Split('&'))
        {
            int eqIndex = pair.IndexOf('=');
            string kvName = eqIndex >= 0 ? pair.Substring(0, eqIndex) : pair;
            string kvValue = eqIndex >= 0 ? pair.Substring(eqIndex + 1) : string.Empty;
            pairs.Add(new HarNameValuePair
            {
                Name = Uri.UnescapeDataString(kvName),
                Value = Uri.UnescapeDataString(kvValue),
            });
        }

        return pairs;
    }

    private static string GetRequestMimeType(IReadOnlyList<ReadOnlyHeader> headers) =>
        GetHeaderValue(headers, "content-type") ?? "application/octet-stream";

    private static string? GetHeaderValue(IReadOnlyList<ReadOnlyHeader> headers, string name)
    {
        foreach (ReadOnlyHeader header in headers)
        {
            if (string.Equals(header.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return header.Value.Value;
            }
        }

        return null;
    }

    // HAR POCO types — internal, serialised only by this class.
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

    private sealed class HarEntry
    {
        [JsonPropertyName("startedDateTime")]
        public string StartedDateTime { get; set; } = string.Empty;

        [JsonPropertyName("time")]
        public double Time { get; set; }

        [JsonPropertyName("request")]
        public HarRequest Request { get; set; } = new();

        [JsonPropertyName("response")]
        public HarResponse Response { get; set; } = new();

        [JsonPropertyName("timings")]
        public HarTimings Timings { get; set; } = new();
    }

    private sealed class HarRequest
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("httpVersion")]
        public string HttpVersion { get; set; } = string.Empty;

        [JsonPropertyName("headers")]
        public List<HarNameValuePair> Headers { get; set; } = [];

        [JsonPropertyName("cookies")]
        public List<HarCookie> Cookies { get; set; } = [];

        [JsonPropertyName("queryString")]
        public List<HarNameValuePair> QueryString { get; set; } = [];

        [JsonPropertyName("postData")]
        public HarPostData? PostData { get; set; }

        [JsonPropertyName("headersSize")]
        public long HeadersSize { get; set; }

        [JsonPropertyName("bodySize")]
        public long BodySize { get; set; }
    }

    private sealed class HarResponse
    {
        [JsonPropertyName("status")]
        public long Status { get; set; }

        [JsonPropertyName("statusText")]
        public string StatusText { get; set; } = string.Empty;

        [JsonPropertyName("httpVersion")]
        public string HttpVersion { get; set; } = string.Empty;

        [JsonPropertyName("headers")]
        public List<HarNameValuePair> Headers { get; set; } = [];

        [JsonPropertyName("cookies")]
        public List<HarCookie> Cookies { get; set; } = [];

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
    }

    private sealed class HarPostData
    {
        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("encoding")]
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

        [JsonPropertyName("ssl")]
        public double Ssl { get; set; }

        [JsonPropertyName("send")]
        public double Send { get; set; }

        [JsonPropertyName("wait")]
        public double Wait { get; set; }

        [JsonPropertyName("receive")]
        public double Receive { get; set; }
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

        [JsonPropertyName("domain")]
        public string? Domain { get; set; }

        [JsonPropertyName("path")]
        public string? Path { get; set; }

        [JsonPropertyName("expires")]
        public string? Expires { get; set; }

        [JsonPropertyName("httpOnly")]
        public bool HttpOnly { get; set; }

        [JsonPropertyName("secure")]
        public bool Secure { get; set; }
    }
}
