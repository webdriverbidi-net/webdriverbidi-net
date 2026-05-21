namespace WebDriverBiDi.Network;

using System.Text.Json;

public class RequestDataTests
{
    [Fact]
    public void CanDeserializeRequestData()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "url": "requestUrl",
                        "method": "get",
                        "headers": [],
                        "cookies": [],
                        "headersSize": 0,
                        "bodySize": 0,
                        "destination": "document",
                        "initiatorType": "other",
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        RequestData? request = JsonSerializer.Deserialize<RequestData>(json);
        Assert.NotNull(request);

        Assert.Equal("myRequestId", request.RequestId);
        Assert.Equal("requestUrl", request.Url);
        Assert.Equal("get", request.Method);
        Assert.Empty(request.Headers);
        Assert.Empty(request.Cookies);
        Assert.Equal("document", request.Destination);
        Assert.Equal("other", request.InitiatorType);
        Assert.Equal(0u, request.HeadersSize);
        Assert.Equal(0u, request.BodySize);
        Assert.NotNull(request.Timings);
        Assert.Equal(1, request.Timings.TimeOrigin);
        Assert.Equal(2, request.Timings.RequestTime);
        Assert.Equal(3, request.Timings.RedirectStart);
        Assert.Equal(4, request.Timings.RedirectEnd);
        Assert.Equal(5, request.Timings.FetchStart);
        Assert.Equal(6, request.Timings.DnsStart);
        Assert.Equal(7, request.Timings.DnsEnd);
        Assert.Equal(8, request.Timings.ConnectStart);
        Assert.Equal(9, request.Timings.ConnectEnd);
        Assert.Equal(10, request.Timings.TlsStart);
        Assert.Equal(11, request.Timings.RequestStart);
        Assert.Equal(12, request.Timings.ResponseStart);
        Assert.Equal(13, request.Timings.ResponseEnd);
    }

    [Fact]
    public void CanDeserializeRequestDataWithHeaders()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "url": "requestUrl",
                        "method": "get",
                        "headers": [
                          {
                            "name": "headerName",
                            "value": {
                              "type": "string",
                              "value": "headerValue" 
                            }
                          }
                        ],
                        "cookies": [],
                        "destination": "document",
                        "initiatorType": "other",
                        "headersSize": 0,
                        "bodySize": 0,
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        RequestData? request = JsonSerializer.Deserialize<RequestData>(json);
        Assert.NotNull(request);

        Assert.Equal("myRequestId", request.RequestId);
        Assert.Equal("requestUrl", request.Url);
        Assert.Equal("get", request.Method);
        Assert.Single(request.Headers);
        Assert.Equal("headerName", request.Headers[0].Name);
        Assert.Equal(BytesValueType.String, request.Headers[0].Value.Type);
        Assert.Equal("headerValue", request.Headers[0].Value.Value);
        Assert.Empty(request.Cookies);
        Assert.Equal("document", request.Destination);
        Assert.Equal("other", request.InitiatorType);
        Assert.Equal(0u, request.HeadersSize);
        Assert.Equal(0u, request.BodySize);
        Assert.NotNull(request.Timings);
        Assert.Equal(1, request.Timings.TimeOrigin);
        Assert.Equal(2, request.Timings.RequestTime);
        Assert.Equal(3, request.Timings.RedirectStart);
        Assert.Equal(4, request.Timings.RedirectEnd);
        Assert.Equal(5, request.Timings.FetchStart);
        Assert.Equal(6, request.Timings.DnsStart);
        Assert.Equal(7, request.Timings.DnsEnd);
        Assert.Equal(8, request.Timings.ConnectStart);
        Assert.Equal(9, request.Timings.ConnectEnd);
        Assert.Equal(10, request.Timings.TlsStart);
        Assert.Equal(11, request.Timings.RequestStart);
        Assert.Equal(12, request.Timings.ResponseStart);
        Assert.Equal(13, request.Timings.ResponseEnd);
    }

    [Fact]
    public void CanDeserializeRequestDataWithCookies()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "url": "requestUrl",
                        "method": "get",
                        "headers": [],
                        "cookies": [
                          {
                            "name": "cookieName",
                            "value": {
                              "type": "string",
                              "value": "cookieValue"
                            },
                            "domain": "cookieDomain",
                            "path": "/cookiePath",
                            "secure": true,
                            "httpOnly": true,
                            "sameSite": "lax",
                            "size": 100
                          }
                        ],
                        "destination": "document",
                        "initiatorType": "other",
                        "headersSize": 0,
                        "bodySize": 0,
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        RequestData? request = JsonSerializer.Deserialize<RequestData>(json);
        Assert.NotNull(request);

        Assert.Equal("myRequestId", request.RequestId);
        Assert.Equal("requestUrl", request.Url);
        Assert.Equal("get", request.Method);
        Assert.Empty(request.Headers);
        Assert.Single(request.Cookies);
        Assert.Equal("cookieName", request.Cookies[0].Name);
        Assert.Equal(BytesValueType.String, request.Cookies[0].Value.Type);
        Assert.Equal("cookieValue", request.Cookies[0].Value.Value);
        Assert.Equal("cookieDomain", request.Cookies[0].Domain);
        Assert.Equal("/cookiePath", request.Cookies[0].Path);
        Assert.True(request.Cookies[0].Secure);
        Assert.True(request.Cookies[0].HttpOnly);
        Assert.Equal(CookieSameSiteValue.Lax, request.Cookies[0].SameSite);
        Assert.Equal(100, request.Cookies[0].Size);
        Assert.Null(request.Cookies[0].Expires);
        Assert.Null(request.Cookies[0].EpochExpires);
        Assert.Equal("document", request.Destination);
        Assert.Equal("other", request.InitiatorType);
        Assert.Equal(0u, request.HeadersSize);
        Assert.Equal(0u, request.BodySize);
        Assert.NotNull(request.Timings);
        Assert.Equal(1, request.Timings.TimeOrigin);
        Assert.Equal(2, request.Timings.RequestTime);
        Assert.Equal(3, request.Timings.RedirectStart);
        Assert.Equal(4, request.Timings.RedirectEnd);
        Assert.Equal(5, request.Timings.FetchStart);
        Assert.Equal(6, request.Timings.DnsStart);
        Assert.Equal(7, request.Timings.DnsEnd);
        Assert.Equal(8, request.Timings.ConnectStart);
        Assert.Equal(9, request.Timings.ConnectEnd);
        Assert.Equal(10, request.Timings.TlsStart);
        Assert.Equal(11, request.Timings.RequestStart);
        Assert.Equal(12, request.Timings.ResponseStart);
        Assert.Equal(13, request.Timings.ResponseEnd);
    }

    [Fact]
    public void CanDeserializeRequestDataWithNullBodySize()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "url": "requestUrl",
                        "method": "get",
                        "headers": [],
                        "cookies": [],
                        "destination": "document",
                        "initiatorType": "other",
                        "headersSize": 0,
                        "bodySize": null,
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        RequestData? request = JsonSerializer.Deserialize<RequestData>(json);
        Assert.NotNull(request);

        Assert.Equal("myRequestId", request.RequestId);
        Assert.Equal("requestUrl", request.Url);
        Assert.Equal("get", request.Method);
        Assert.Empty(request.Headers);
        Assert.Empty(request.Cookies);
        Assert.Equal("document", request.Destination);
        Assert.Equal("other", request.InitiatorType);
        Assert.Equal(0u, request.HeadersSize);
        Assert.Null(request.BodySize);
        Assert.NotNull(request.Timings);
        Assert.Equal(1, request.Timings.TimeOrigin);
        Assert.Equal(2, request.Timings.RequestTime);
        Assert.Equal(3, request.Timings.RedirectStart);
        Assert.Equal(4, request.Timings.RedirectEnd);
        Assert.Equal(5, request.Timings.FetchStart);
        Assert.Equal(6, request.Timings.DnsStart);
        Assert.Equal(7, request.Timings.DnsEnd);
        Assert.Equal(8, request.Timings.ConnectStart);
        Assert.Equal(9, request.Timings.ConnectEnd);
        Assert.Equal(10, request.Timings.TlsStart);
        Assert.Equal(11, request.Timings.RequestStart);
        Assert.Equal(12, request.Timings.ResponseStart);
        Assert.Equal(13, request.Timings.ResponseEnd);
    }

    [Fact]
    public void CanDeserializeRequestDataWithNullInitiatorType()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "url": "requestUrl",
                        "method": "get",
                        "headers": [],
                        "cookies": [],
                        "destination": "document",
                        "initiatorType": null,
                        "headersSize": 0,
                        "bodySize": 0,
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        RequestData? request = JsonSerializer.Deserialize<RequestData>(json);
        Assert.NotNull(request);

        Assert.Equal("myRequestId", request.RequestId);
        Assert.Equal("requestUrl", request.Url);
        Assert.Equal("get", request.Method);
        Assert.Empty(request.Headers);
        Assert.Empty(request.Cookies);
        Assert.Equal("document", request.Destination);
        Assert.Null(request.InitiatorType);
        Assert.Equal(0u, request.HeadersSize);
        Assert.Equal(0u, request.BodySize);
        Assert.NotNull(request.Timings);
        Assert.Equal(1, request.Timings.TimeOrigin);
        Assert.Equal(2, request.Timings.RequestTime);
        Assert.Equal(3, request.Timings.RedirectStart);
        Assert.Equal(4, request.Timings.RedirectEnd);
        Assert.Equal(5, request.Timings.FetchStart);
        Assert.Equal(6, request.Timings.DnsStart);
        Assert.Equal(7, request.Timings.DnsEnd);
        Assert.Equal(8, request.Timings.ConnectStart);
        Assert.Equal(9, request.Timings.ConnectEnd);
        Assert.Equal(10, request.Timings.TlsStart);
        Assert.Equal(11, request.Timings.RequestStart);
        Assert.Equal(12, request.Timings.ResponseStart);
        Assert.Equal(13, request.Timings.ResponseEnd);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "url": "requestUrl",
                        "method": "get",
                        "headers": [],
                        "cookies": [],
                        "headersSize": 0,
                        "bodySize": 0,
                        "destination": "document",
                        "initiatorType": "other",
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        RequestData? request = JsonSerializer.Deserialize<RequestData>(json);
        Assert.NotNull(request);
        RequestData copy = request with { };
        Assert.Equal(request, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingRequestIdThrows()
    {
        string json = """
                      {
                        "url": "requestUrl",
                        "method": "get",
                        "headers": [],
                        "cookies": [],
                        "destination": "document",
                        "initiatorType": "other",
                        "headersSize": 0,
                        "bodySize": 0,
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'request", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingUrlThrows()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "method": "get",
                        "headers": [],
                        "cookies": [],
                        "destination": "document",
                        "initiatorType": "other",
                        "headersSize": 0,
                        "bodySize": 0,
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'url", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingMethodThrows()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "url": "requestUrl",
                        "headers": [],
                        "cookies": [],
                        "destination": "document",
                        "initiatorType": "other",
                        "headersSize": 0,
                        "bodySize": 0,
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'method", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingHeadersThrows()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "url": "requestUrl",
                        "method": "get",
                        "cookies": [],
                        "destination": "document",
                        "initiatorType": "other",
                        "headersSize": 0,
                        "bodySize": 0,
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'headers", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingCookiesThrows()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "url": "requestUrl",
                        "method": "get",
                        "headers": [],
                        "destination": "document",
                        "initiatorType": "other",
                        "headersSize": 0,
                        "bodySize": 0,
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'cookies", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingHeadersSizeThrows()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "url": "requestUrl",
                        "method": "get",
                        "headers": [],
                        "cookies": [],
                        "destination": "document",
                        "initiatorType": "other",
                        "bodySize": 0,
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'headersSize", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingBodySizeThrows()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "url": "requestUrl",
                        "method": "get",
                        "headers": [],
                        "cookies": [],
                        "destination": "document",
                        "initiatorType": "other",
                        "headersSize": 0,
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'bodySize", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingDestinationThrows()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "url": "requestUrl",
                        "method": "get",
                        "headers": [],
                        "cookies": [],
                        "initiatorType": "other",
                        "headersSize": 0,
                        "bodySize": 0,
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'destination", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingInitiatorTypeThrows()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "url": "requestUrl",
                        "method": "get",
                        "headers": [],
                        "cookies": [],
                        "destination": "document",
                        "headersSize": 0,
                        "bodySize": 0,
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'initiatorType", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestData>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithMissingTimingsThrows()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "url": "requestUrl",
                        "method": "get",
                        "headers": [],
                        "cookies": [],
                        "destination": "document",
                        "initiatorType": "other",
                        "headersSize": 0,
                        "bodySize": 0
                      }
                      """;
        Assert.Contains("missing required properties including: 'timings", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestData>(json)).Message);
    }
}
