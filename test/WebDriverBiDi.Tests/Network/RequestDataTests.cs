namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class RequestDataTests
{
    [Test]
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
        Assert.That(request, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(request.RequestId, Is.EqualTo("myRequestId"));
            Assert.That(request.Url, Is.EqualTo("requestUrl"));
            Assert.That(request.Method, Is.EqualTo("get"));
            Assert.That(request.Headers, Is.Empty);
            Assert.That(request.Cookies, Is.Empty);
            Assert.That(request.Destination, Is.EqualTo("document"));
            Assert.That(request.InitiatorType, Is.EqualTo("other"));
            Assert.That(request.HeadersSize, Is.EqualTo(0));
            Assert.That(request.BodySize, Is.EqualTo(0));
            Assert.That(request.Timings, Is.Not.Null);
            Assert.That(request.Timings.TimeOrigin, Is.EqualTo(1));
            Assert.That(request.Timings.RequestTime, Is.EqualTo(2));
            Assert.That(request.Timings.RedirectStart, Is.EqualTo(3));
            Assert.That(request.Timings.RedirectEnd, Is.EqualTo(4));
            Assert.That(request.Timings.FetchStart, Is.EqualTo(5));
            Assert.That(request.Timings.DnsStart, Is.EqualTo(6));
            Assert.That(request.Timings.DnsEnd, Is.EqualTo(7));
            Assert.That(request.Timings.ConnectStart, Is.EqualTo(8));
            Assert.That(request.Timings.ConnectEnd, Is.EqualTo(9));
            Assert.That(request.Timings.TlsStart, Is.EqualTo(10));
            Assert.That(request.Timings.RequestStart, Is.EqualTo(11));
            Assert.That(request.Timings.ResponseStart, Is.EqualTo(12));
            Assert.That(request.Timings.ResponseEnd, Is.EqualTo(13));
        }       
    }

    [Test]
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
        Assert.That(request, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(request.RequestId, Is.EqualTo("myRequestId"));
            Assert.That(request.Url, Is.EqualTo("requestUrl"));
            Assert.That(request.Method, Is.EqualTo("get"));
            Assert.That(request.Headers, Has.Count.EqualTo(1));
            Assert.That(request.Headers[0].Name, Is.EqualTo("headerName"));
            Assert.That(request.Headers[0].Value.Type, Is.EqualTo(BytesValueType.String));
            Assert.That(request.Headers[0].Value.Value, Is.EqualTo("headerValue"));
            Assert.That(request.Cookies, Is.Empty);
            Assert.That(request.Destination, Is.EqualTo("document"));
            Assert.That(request.InitiatorType, Is.EqualTo("other"));
            Assert.That(request.HeadersSize, Is.EqualTo(0));
            Assert.That(request.BodySize, Is.EqualTo(0));
            Assert.That(request.Timings, Is.Not.Null);
            Assert.That(request.Timings.TimeOrigin, Is.EqualTo(1));
            Assert.That(request.Timings.RequestTime, Is.EqualTo(2));
            Assert.That(request.Timings.RedirectStart, Is.EqualTo(3));
            Assert.That(request.Timings.RedirectEnd, Is.EqualTo(4));
            Assert.That(request.Timings.FetchStart, Is.EqualTo(5));
            Assert.That(request.Timings.DnsStart, Is.EqualTo(6));
            Assert.That(request.Timings.DnsEnd, Is.EqualTo(7));
            Assert.That(request.Timings.ConnectStart, Is.EqualTo(8));
            Assert.That(request.Timings.ConnectEnd, Is.EqualTo(9));
            Assert.That(request.Timings.TlsStart, Is.EqualTo(10));
            Assert.That(request.Timings.RequestStart, Is.EqualTo(11));
            Assert.That(request.Timings.ResponseStart, Is.EqualTo(12));
            Assert.That(request.Timings.ResponseEnd, Is.EqualTo(13));
        }       
    }

    [Test]
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
        Assert.That(request, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(request.RequestId, Is.EqualTo("myRequestId"));
            Assert.That(request.Url, Is.EqualTo("requestUrl"));
            Assert.That(request.Method, Is.EqualTo("get"));
            Assert.That(request.Headers, Is.Empty);
            Assert.That(request.Cookies, Has.Count.EqualTo(1));
            Assert.That(request.Cookies[0].Name, Is.EqualTo("cookieName"));
            Assert.That(request.Cookies[0].Value.Type, Is.EqualTo(BytesValueType.String));
            Assert.That(request.Cookies[0].Value.Value, Is.EqualTo("cookieValue"));
            Assert.That(request.Cookies[0].Domain, Is.EqualTo("cookieDomain"));
            Assert.That(request.Cookies[0].Path, Is.EqualTo("/cookiePath"));
            Assert.That(request.Cookies[0].Secure, Is.True);
            Assert.That(request.Cookies[0].HttpOnly, Is.True);
            Assert.That(request.Cookies[0].SameSite, Is.EqualTo(CookieSameSiteValue.Lax));
            Assert.That(request.Cookies[0].Size, Is.EqualTo(100));
            Assert.That(request.Cookies[0].Expires, Is.Null);
            Assert.That(request.Cookies[0].EpochExpires, Is.Null);
            Assert.That(request.Destination, Is.EqualTo("document"));
            Assert.That(request.InitiatorType, Is.EqualTo("other"));
            Assert.That(request.HeadersSize, Is.EqualTo(0));
            Assert.That(request.BodySize, Is.EqualTo(0));
            Assert.That(request.Timings, Is.Not.Null);
            Assert.That(request.Timings.TimeOrigin, Is.EqualTo(1));
            Assert.That(request.Timings.RequestTime, Is.EqualTo(2));
            Assert.That(request.Timings.RedirectStart, Is.EqualTo(3));
            Assert.That(request.Timings.RedirectEnd, Is.EqualTo(4));
            Assert.That(request.Timings.FetchStart, Is.EqualTo(5));
            Assert.That(request.Timings.DnsStart, Is.EqualTo(6));
            Assert.That(request.Timings.DnsEnd, Is.EqualTo(7));
            Assert.That(request.Timings.ConnectStart, Is.EqualTo(8));
            Assert.That(request.Timings.ConnectEnd, Is.EqualTo(9));
            Assert.That(request.Timings.TlsStart, Is.EqualTo(10));
            Assert.That(request.Timings.RequestStart, Is.EqualTo(11));
            Assert.That(request.Timings.ResponseStart, Is.EqualTo(12));
            Assert.That(request.Timings.ResponseEnd, Is.EqualTo(13));
        }       
    }

    [Test]
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
        Assert.That(request, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(request.RequestId, Is.EqualTo("myRequestId"));
            Assert.That(request.Url, Is.EqualTo("requestUrl"));
            Assert.That(request.Method, Is.EqualTo("get"));
            Assert.That(request.Headers, Is.Empty);
            Assert.That(request.Cookies, Is.Empty);
            Assert.That(request.Destination, Is.EqualTo("document"));
            Assert.That(request.InitiatorType, Is.EqualTo("other"));
            Assert.That(request.HeadersSize, Is.EqualTo(0));
            Assert.That(request.BodySize, Is.Null);
            Assert.That(request.Timings, Is.Not.Null);
            Assert.That(request.Timings.TimeOrigin, Is.EqualTo(1));
            Assert.That(request.Timings.RequestTime, Is.EqualTo(2));
            Assert.That(request.Timings.RedirectStart, Is.EqualTo(3));
            Assert.That(request.Timings.RedirectEnd, Is.EqualTo(4));
            Assert.That(request.Timings.FetchStart, Is.EqualTo(5));
            Assert.That(request.Timings.DnsStart, Is.EqualTo(6));
            Assert.That(request.Timings.DnsEnd, Is.EqualTo(7));
            Assert.That(request.Timings.ConnectStart, Is.EqualTo(8));
            Assert.That(request.Timings.ConnectEnd, Is.EqualTo(9));
            Assert.That(request.Timings.TlsStart, Is.EqualTo(10));
            Assert.That(request.Timings.RequestStart, Is.EqualTo(11));
            Assert.That(request.Timings.ResponseStart, Is.EqualTo(12));
            Assert.That(request.Timings.ResponseEnd, Is.EqualTo(13));
        }       
    }

    [Test]
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
        Assert.That(request, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(request.RequestId, Is.EqualTo("myRequestId"));
            Assert.That(request.Url, Is.EqualTo("requestUrl"));
            Assert.That(request.Method, Is.EqualTo("get"));
            Assert.That(request.Headers, Is.Empty);
            Assert.That(request.Cookies, Is.Empty);
            Assert.That(request.Destination, Is.EqualTo("document"));
            Assert.That(request.InitiatorType, Is.Null);
            Assert.That(request.HeadersSize, Is.EqualTo(0));
            Assert.That(request.BodySize, Is.EqualTo(0));
            Assert.That(request.Timings, Is.Not.Null);
            Assert.That(request.Timings.TimeOrigin, Is.EqualTo(1));
            Assert.That(request.Timings.RequestTime, Is.EqualTo(2));
            Assert.That(request.Timings.RedirectStart, Is.EqualTo(3));
            Assert.That(request.Timings.RedirectEnd, Is.EqualTo(4));
            Assert.That(request.Timings.FetchStart, Is.EqualTo(5));
            Assert.That(request.Timings.DnsStart, Is.EqualTo(6));
            Assert.That(request.Timings.DnsEnd, Is.EqualTo(7));
            Assert.That(request.Timings.ConnectStart, Is.EqualTo(8));
            Assert.That(request.Timings.ConnectEnd, Is.EqualTo(9));
            Assert.That(request.Timings.TlsStart, Is.EqualTo(10));
            Assert.That(request.Timings.RequestStart, Is.EqualTo(11));
            Assert.That(request.Timings.ResponseStart, Is.EqualTo(12));
            Assert.That(request.Timings.ResponseEnd, Is.EqualTo(13));
        }       
    }

    [Test]
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
        Assert.That(request, Is.Not.Null);
        RequestData copy = request with { };
        Assert.That(copy, Is.EqualTo(request));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RequestData>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties including: 'request"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RequestData>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties including: 'url"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RequestData>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties including: 'method"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RequestData>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties including: 'headers"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RequestData>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties including: 'cookies"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RequestData>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties including: 'headersSize"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RequestData>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties including: 'bodySize"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RequestData>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties including: 'destination"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RequestData>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties including: 'initiatorType"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RequestData>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties including: 'timings"));
    }
}
