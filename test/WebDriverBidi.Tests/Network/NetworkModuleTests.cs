namespace WebDriverBidi.Network;

using WebDriverBidi.TestUtilities;

[TestFixture]
public class NetworkModuleTests
{
    [Test]
    public void TestCanReceiveBeforeRequestSendEvent()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);

        TestConnection connection = new();
        Driver driver = new(new(TimeSpan.FromMilliseconds(500), connection));
        NetworkModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.BeforeRequestSent += (object? obj, BeforeRequestSentEventArgs e) => {
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.RedirectCount, Is.EqualTo(0));
                Assert.That(e.Timestamp, Is.EqualTo(eventTime));
                Assert.That(e.EpochTimestamp, Is.EqualTo(milliseconds));
                Assert.That(e.Request.RequestId, Is.EqualTo("myRequestId"));
                Assert.That(e.Request.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.Request.Method, Is.EqualTo("get"));
                Assert.That(e.Request.Headers, Has.Count.EqualTo(1));
                Assert.That(e.Request.Headers[0].Name, Is.EqualTo("headerName"));
                Assert.That(e.Request.Headers[0].Value.Type, Is.EqualTo(BytesValueType.String));
                Assert.That(e.Request.Headers[0].Value.Value, Is.EqualTo("headerValue"));
                Assert.That(e.Request.Cookies, Has.Count.EqualTo(1));
                Assert.That(e.Request.Cookies[0].Name, Is.EqualTo("cookieName"));
                Assert.That(e.Request.Cookies[0].Value.Type, Is.EqualTo(BytesValueType.String));
                Assert.That(e.Request.Cookies[0].Value.Value, Is.EqualTo("cookieValue"));
                Assert.That(e.Request.Cookies[0].Domain, Is.EqualTo("cookieDomain"));
                Assert.That(e.Request.Cookies[0].Path, Is.EqualTo("/cookiePath"));
                Assert.That(e.Request.Cookies[0].SameSite, Is.EqualTo(CookieSameSiteValue.Strict));
                Assert.That(e.Request.Cookies[0].Secure, Is.False);
                Assert.That(e.Request.Cookies[0].HttpOnly, Is.True);
                Assert.That(e.Request.Cookies[0].Size, Is.EqualTo(10));
                Assert.That(e.Request.Cookies[0].Expires, Is.Null);
                Assert.That(e.Request.HeadersSize, Is.EqualTo(100));
                Assert.That(e.Request.BodySize, Is.EqualTo(300));
                Assert.That(e.Request.Timings, Is.Not.Null);
                Assert.That(e.Initiator.Type, Is.EqualTo(InitiatorType.Parser));
            });
            syncEvent.Set();
        };

        string eventJson = @"{
    ""type"": ""event"", 
    ""method"": ""network.beforeRequestSent"",
    ""params"": {
        ""context"": ""myContext"",
        ""navigation"": ""myNavigationId"",
        ""redirectCount"": 0,
        ""timestamp"": " + milliseconds + @",
        ""request"": {
            ""request"": ""myRequestId"",
            ""url"": ""https://example.com"",
            ""method"": ""get"",
            ""headers"": [
                {
                    ""name"": ""headerName"",
                    ""value"":  {
                        ""type"": ""string"",
                        ""value"": ""headerValue""
                    }
                }
            ],
            ""cookies"": [
                {
                    ""name"": ""cookieName"",
                    ""value"": {
                        ""type"": ""string"",
                        ""value"": ""cookieValue""
                    },
                    ""domain"": ""cookieDomain"",
                    ""path"": ""/cookiePath"",
                    ""sameSite"": ""strict"",
                    ""httpOnly"": true,
                    ""secure"": false,
                    ""size"": 10
                }
            ],
            ""headersSize"": 100,
            ""bodySize"": 300,
            ""timings"": {
                ""timeOrigin"": 1,
                ""requestTime"": 2,
                ""redirectStart"": 3,
                ""redirectEnd"": 4,
                ""fetchStart"": 5,
                ""dnsStart"": 6,
                ""dnsEnd"": 7,
                ""connectStart"": 8,
                ""connectEnd"": 9,
                ""tlsStart"": 10,
                ""requestStart"": 11,
                ""responseStart"": 12,
                ""responseEnd"": 13
            }
        },
        ""initiator"": {
            ""type"": ""parser""
        }
    }
}";
        connection.RaiseDataReceivedEvent(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveFetchErrorEvent()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);

        TestConnection connection = new();
        Driver driver = new(new(TimeSpan.FromMilliseconds(500), connection));
        NetworkModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.FetchError += (object? obj, FetchErrorEventArgs e) => {
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.RedirectCount, Is.EqualTo(0));
                Assert.That(e.Timestamp, Is.EqualTo(eventTime));
                Assert.That(e.EpochTimestamp, Is.EqualTo(milliseconds));
                Assert.That(e.Request.RequestId, Is.EqualTo("myRequestId"));
                Assert.That(e.Request.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.Request.Method, Is.EqualTo("get"));
                Assert.That(e.Request.Headers, Has.Count.EqualTo(1));
                Assert.That(e.Request.Headers[0].Name, Is.EqualTo("headerName"));
                Assert.That(e.Request.Headers[0].Value.Type, Is.EqualTo(BytesValueType.String));
                Assert.That(e.Request.Headers[0].Value.Value, Is.EqualTo("headerValue"));
                Assert.That(e.Request.Cookies, Has.Count.EqualTo(1));
                Assert.That(e.Request.Cookies[0].Name, Is.EqualTo("cookieName"));
                Assert.That(e.Request.Cookies[0].Value.Type, Is.EqualTo(BytesValueType.String));
                Assert.That(e.Request.Cookies[0].Value.Value, Is.EqualTo("cookieValue"));
                Assert.That(e.Request.Cookies[0].Domain, Is.EqualTo("cookieDomain"));
                Assert.That(e.Request.Cookies[0].Path, Is.EqualTo("/cookiePath"));
                Assert.That(e.Request.Cookies[0].SameSite, Is.EqualTo(CookieSameSiteValue.Strict));
                Assert.That(e.Request.Cookies[0].Secure, Is.False);
                Assert.That(e.Request.Cookies[0].HttpOnly, Is.True);
                Assert.That(e.Request.Cookies[0].Size, Is.EqualTo(10));
                Assert.That(e.Request.Cookies[0].Expires, Is.Null);
                Assert.That(e.Request.HeadersSize, Is.EqualTo(100));
                Assert.That(e.Request.BodySize, Is.EqualTo(300));
                Assert.That(e.Request.Timings, Is.Not.Null);
                Assert.That(e.ErrorText, Is.EqualTo("An error occurred"));
            });
            syncEvent.Set();
        };

        string eventJson = @"{
    ""type"": ""event"", 
    ""method"": ""network.fetchError"",
    ""params"": {
        ""context"": ""myContext"",
        ""navigation"": ""myNavigationId"",
        ""redirectCount"": 0,
        ""timestamp"": " + milliseconds + @",
        ""request"": {
            ""request"": ""myRequestId"",
            ""url"": ""https://example.com"",
            ""method"": ""get"",
            ""headers"": [
                {
                    ""name"": ""headerName"",
                    ""value"": {
                        ""type"": ""string"",
                        ""value"": ""headerValue""
                    }
                }
            ],
            ""cookies"": [
                {
                    ""name"": ""cookieName"",
                    ""value"": {
                        ""type"": ""string"",
                        ""value"": ""cookieValue""
                    },
                    ""domain"": ""cookieDomain"",
                    ""path"": ""/cookiePath"",
                    ""sameSite"": ""strict"",
                    ""httpOnly"": true,
                    ""secure"": false,
                    ""size"": 10
                }
            ],
            ""headersSize"": 100,
            ""bodySize"": 300,
            ""timings"": {
                ""timeOrigin"": 1,
                ""requestTime"": 2,
                ""redirectStart"": 3,
                ""redirectEnd"": 4,
                ""fetchStart"": 5,
                ""dnsStart"": 6,
                ""dnsEnd"": 7,
                ""connectStart"": 8,
                ""connectEnd"": 9,
                ""tlsStart"": 10,
                ""requestStart"": 11,
                ""responseStart"": 12,
                ""responseEnd"": 13
            }
        },
        ""errorText"": ""An error occurred""
    }
}";
        connection.RaiseDataReceivedEvent(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveResponseStartedEvent()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);

        TestConnection connection = new();
        Driver driver = new(new(TimeSpan.FromMilliseconds(500), connection));
        NetworkModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.ResponseStarted += (object? obj, ResponseStartedEventArgs e) => {
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.RedirectCount, Is.EqualTo(0));
                Assert.That(e.Timestamp, Is.EqualTo(eventTime));
                Assert.That(e.EpochTimestamp, Is.EqualTo(milliseconds));
                Assert.That(e.Request.RequestId, Is.EqualTo("myRequestId"));
                Assert.That(e.Request.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.Request.Method, Is.EqualTo("get"));
                Assert.That(e.Request.Headers, Has.Count.EqualTo(1));
                Assert.That(e.Request.Headers[0].Name, Is.EqualTo("headerName"));
                Assert.That(e.Request.Headers[0].Value.Type, Is.EqualTo(BytesValueType.String));
                Assert.That(e.Request.Headers[0].Value.Value, Is.EqualTo("headerValue"));
                Assert.That(e.Request.Cookies, Has.Count.EqualTo(1));
                Assert.That(e.Request.Cookies[0].Name, Is.EqualTo("cookieName"));
                Assert.That(e.Request.Cookies[0].Value.Type, Is.EqualTo(BytesValueType.String));
                Assert.That(e.Request.Cookies[0].Value.Value, Is.EqualTo("cookieValue"));
                Assert.That(e.Request.Cookies[0].Domain, Is.EqualTo("cookieDomain"));
                Assert.That(e.Request.Cookies[0].Path, Is.EqualTo("/cookiePath"));
                Assert.That(e.Request.Cookies[0].SameSite, Is.EqualTo(CookieSameSiteValue.Strict));
                Assert.That(e.Request.Cookies[0].Secure, Is.False);
                Assert.That(e.Request.Cookies[0].HttpOnly, Is.True);
                Assert.That(e.Request.Cookies[0].Size, Is.EqualTo(10));
                Assert.That(e.Request.Cookies[0].Expires, Is.Null);
                Assert.That(e.Request.HeadersSize, Is.EqualTo(100));
                Assert.That(e.Request.BodySize, Is.EqualTo(300));
                Assert.That(e.Request.Timings, Is.Not.Null);
                Assert.That(e.Response.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.Response.Protocol, Is.EqualTo("https"));
                Assert.That(e.Response.Status, Is.EqualTo(200));
                Assert.That(e.Response.StatusText, Is.EqualTo("OK"));
                Assert.That(e.Response.FromCache, Is.False);
                Assert.That(e.Response.Headers, Has.Count.EqualTo(1));
                Assert.That(e.Response.Headers[0].Name, Is.EqualTo("headerName"));
                Assert.That(e.Response.Headers[0].Value.Type, Is.EqualTo(BytesValueType.String));
                Assert.That(e.Response.Headers[0].Value.Value, Is.EqualTo("headerValue"));
                Assert.That(e.Response.MimeType, Is.EqualTo("text/html"));
                Assert.That(e.Response.BytesReceived, Is.EqualTo(400));
                Assert.That(e.Response.HeadersSize, Is.EqualTo(100));
                Assert.That(e.Response.BodySize, Is.EqualTo(300));
                Assert.That(e.Response.Content.Size, Is.EqualTo(300));
            });
            syncEvent.Set();
        };

        string eventJson = @"{
    ""type"": ""event"", 
    ""method"": ""network.responseStarted"",
    ""params"": {
        ""context"": ""myContext"",
        ""navigation"": ""myNavigationId"",
        ""redirectCount"": 0,
        ""timestamp"": " + milliseconds + @",
        ""request"": {
            ""request"": ""myRequestId"",
            ""url"": ""https://example.com"",
            ""method"": ""get"",
            ""headers"": [
                {
                    ""name"": ""headerName"",
                    ""value"": {
                        ""type"": ""string"",
                        ""value"": ""headerValue""
                    }
                }
            ],
            ""cookies"": [
                {
                    ""name"": ""cookieName"",
                    ""value"": {
                        ""type"": ""string"",
                        ""value"": ""cookieValue""
                    },
                    ""domain"": ""cookieDomain"",
                    ""path"": ""/cookiePath"",
                    ""sameSite"": ""strict"",
                    ""httpOnly"": true,
                    ""secure"": false,
                    ""size"": 10
                }
            ],
            ""headersSize"": 100,
            ""bodySize"": 300,
            ""timings"": {
                ""timeOrigin"": 1,
                ""requestTime"": 2,
                ""redirectStart"": 3,
                ""redirectEnd"": 4,
                ""fetchStart"": 5,
                ""dnsStart"": 6,
                ""dnsEnd"": 7,
                ""connectStart"": 8,
                ""connectEnd"": 9,
                ""tlsStart"": 10,
                ""requestStart"": 11,
                ""responseStart"": 12,
                ""responseEnd"": 13
            }
        },
        ""response"": {
            ""url"": ""https://example.com"",
            ""protocol"": ""https"",
            ""status"": 200,
            ""statusText"": ""OK"",
            ""fromCache"": false,
            ""headers"": [
                {
                    ""name"": ""headerName"",
                    ""value"": {
                        ""type"": ""string"",
                        ""value"": ""headerValue""
                    }
                }
            ],
            ""mimeType"": ""text/html"",
            ""bytesReceived"": 400,
            ""headersSize"": 100,
            ""bodySize"": 300,
            ""content"": {
                ""size"": 300
            }
        }
    }
}";
        connection.RaiseDataReceivedEvent(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveResponseCompletedEvent()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);

        TestConnection connection = new();
        Driver driver = new(new(TimeSpan.FromMilliseconds(500), connection));
        NetworkModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.ResponseCompleted += (object? obj, ResponseCompletedEventArgs e) => {
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.RedirectCount, Is.EqualTo(0));
                Assert.That(e.Timestamp, Is.EqualTo(eventTime));
                Assert.That(e.EpochTimestamp, Is.EqualTo(milliseconds));
                Assert.That(e.Request.RequestId, Is.EqualTo("myRequestId"));
                Assert.That(e.Request.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.Request.Method, Is.EqualTo("get"));
                Assert.That(e.Request.Headers, Has.Count.EqualTo(1));
                Assert.That(e.Request.Headers[0].Name, Is.EqualTo("headerName"));
                Assert.That(e.Request.Headers[0].Value.Type, Is.EqualTo(BytesValueType.String));
                Assert.That(e.Request.Headers[0].Value.Value, Is.EqualTo("headerValue"));
                Assert.That(e.Request.Cookies, Has.Count.EqualTo(1));
                Assert.That(e.Request.Cookies[0].Name, Is.EqualTo("cookieName"));
                Assert.That(e.Request.Cookies[0].Value.Type, Is.EqualTo(BytesValueType.String));
                Assert.That(e.Request.Cookies[0].Value.Value, Is.EqualTo("cookieValue"));
                Assert.That(e.Request.Cookies[0].Domain, Is.EqualTo("cookieDomain"));
                Assert.That(e.Request.Cookies[0].Path, Is.EqualTo("/cookiePath"));
                Assert.That(e.Request.Cookies[0].SameSite, Is.EqualTo(CookieSameSiteValue.Strict));
                Assert.That(e.Request.Cookies[0].Secure, Is.False);
                Assert.That(e.Request.Cookies[0].HttpOnly, Is.True);
                Assert.That(e.Request.Cookies[0].Size, Is.EqualTo(10));
                Assert.That(e.Request.Cookies[0].Expires, Is.Null);
                Assert.That(e.Request.HeadersSize, Is.EqualTo(100));
                Assert.That(e.Request.BodySize, Is.EqualTo(300));
                Assert.That(e.Request.Timings, Is.Not.Null);
                Assert.That(e.Response.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.Response.Protocol, Is.EqualTo("https"));
                Assert.That(e.Response.Status, Is.EqualTo(200));
                Assert.That(e.Response.StatusText, Is.EqualTo("OK"));
                Assert.That(e.Response.FromCache, Is.False);
                Assert.That(e.Response.Headers, Has.Count.EqualTo(1));
                Assert.That(e.Response.Headers[0].Name, Is.EqualTo("headerName"));
                Assert.That(e.Response.Headers[0].Value.Type, Is.EqualTo(BytesValueType.String));
                Assert.That(e.Response.Headers[0].Value.Value, Is.EqualTo("headerValue"));
                Assert.That(e.Response.MimeType, Is.EqualTo("text/html"));
                Assert.That(e.Response.BytesReceived, Is.EqualTo(400));
                Assert.That(e.Response.HeadersSize, Is.EqualTo(100));
                Assert.That(e.Response.BodySize, Is.EqualTo(300));
                Assert.That(e.Response.Content.Size, Is.EqualTo(300));
            });
            syncEvent.Set();
        };

        string eventJson = @"{
    ""type"": ""event"", 
    ""method"": ""network.responseCompleted"",
    ""params"": {
        ""context"": ""myContext"",
        ""navigation"": ""myNavigationId"",
        ""redirectCount"": 0,
        ""timestamp"": " + milliseconds + @",
        ""request"": {
            ""request"": ""myRequestId"",
            ""url"": ""https://example.com"",
            ""method"": ""get"",
            ""headers"": [
                {
                    ""name"": ""headerName"",
                    ""value"": {
                        ""type"": ""string"",
                        ""value"": ""headerValue""
                    }
                }
            ],
            ""cookies"": [
                {
                    ""name"": ""cookieName"",
                    ""value"": {
                        ""type"": ""string"", 
                        ""value"": ""cookieValue""
                    },
                    ""domain"": ""cookieDomain"",
                    ""path"": ""/cookiePath"",
                    ""sameSite"": ""strict"",
                    ""httpOnly"": true,
                    ""secure"": false,
                    ""size"": 10
                }
            ],
            ""headersSize"": 100,
            ""bodySize"": 300,
            ""timings"": {
                ""timeOrigin"": 1,
                ""requestTime"": 2,
                ""redirectStart"": 3,
                ""redirectEnd"": 4,
                ""fetchStart"": 5,
                ""dnsStart"": 6,
                ""dnsEnd"": 7,
                ""connectStart"": 8,
                ""connectEnd"": 9,
                ""tlsStart"": 10,
                ""requestStart"": 11,
                ""responseStart"": 12,
                ""responseEnd"": 13
            }
        },
        ""response"": {
            ""url"": ""https://example.com"",
            ""protocol"": ""https"",
            ""status"": 200,
            ""statusText"": ""OK"",
            ""fromCache"": false,
            ""headers"": [
                {
                    ""name"": ""headerName"",
                    ""value"": {
                        ""type"": ""string"",
                        ""value"": ""headerValue""
                    }
                }
            ],
            ""mimeType"": ""text/html"",
            ""bytesReceived"": 400,
            ""headersSize"": 100,
            ""bodySize"": 300,
            ""content"": {
                ""size"": 300
            }
        }
    }
}";
        connection.RaiseDataReceivedEvent(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }
}