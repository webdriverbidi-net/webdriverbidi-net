namespace WebDriverBiDi.Network;

using WebDriverBiDi.TestUtilities;

[TestFixture]
public class NetworkModuleTests
{
    private readonly string requestDataJson = """
                                              {
                                                "request": "myRequestId",
                                                "url": "https://example.com",
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
                                                "cookies": [
                                                  {
                                                    "name": "cookieName",
                                                    "value": {
                                                      "type": "string",
                                                      "value": "cookieValue"
                                                    },
                                                    "domain": "cookieDomain",
                                                    "path": "/cookiePath",
                                                    "sameSite": "strict",
                                                    "httpOnly": true,
                                                    "secure": false,
                                                    "size": 10
                                                  }
                                                ],
                                                "destination": "document",
                                                "initiatorType": "other",
                                                "headersSize": 100,
                                                "bodySize": 300,
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

    private readonly string responseDataJson = """
                                               {
                                                 "url": "https://example.com",
                                                 "protocol": "https",
                                                 "status": 200,
                                                 "statusText": "OK",
                                                 "fromCache": false,
                                                 "headers": [
                                                  {
                                                    "name": "headerName",
                                                    "value": {
                                                      "type": "string",
                                                      "value": "headerValue"
                                                    }
                                                  }
                                                ],
                                                "mimeType": "text/html",
                                                "bytesReceived": 400,
                                                "headersSize": 100,
                                                "bodySize": 300,
                                                "content": {
                                                  "size": 300
                                                }
                                               }
                                               """;

    [Test]
    public async Task TestExecuteAddInterceptCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "intercept": "myInterceptId"
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        AddInterceptCommandParameters commandParameters = new()
        {
            UrlPatterns = [new UrlPatternString("https://example.com/*")]
        };
        commandParameters.Phases.Add(InterceptPhase.BeforeRequestSent);
        Task<AddInterceptCommandResult> task = module.AddInterceptAsync(commandParameters);

        task.Wait(TimeSpan.FromSeconds(1));
        AddInterceptCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.InterceptId, Is.EqualTo("myInterceptId"));
    }

    [Test]
    public async Task TestExecuteAddDataCollectorCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "collector": "myCollectorId"
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        AddDataCollectorCommandParameters commandParameters = new(1024 * 1024);
        Task<AddDataCollectorCommandResult> task = module.AddDataCollectorAsync(commandParameters);

        task.Wait(TimeSpan.FromSeconds(1));
        AddDataCollectorCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.CollectorId, Is.EqualTo("myCollectorId"));
    }

    [Test]
    public async Task TestExecuteContinueRequestCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        Task<ContinueRequestCommandResult> task = module.ContinueRequestAsync(new ContinueRequestCommandParameters("requestId"));

        task.Wait(TimeSpan.FromSeconds(1));
        ContinueRequestCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteContinueResponseCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        Task<ContinueResponseCommandResult> task = module.ContinueResponseAsync(new ContinueResponseCommandParameters("requestId"));

        task.Wait(TimeSpan.FromSeconds(1));
        ContinueResponseCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteContinueWithAuthCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        Task<ContinueWithAuthCommandResult> task = module.ContinueWithAuthAsync(new ContinueWithAuthCommandParameters("requestId")
        {
            Action = ContinueWithAuthActionType.ProvideCredentials,
            Credentials = new AuthCredentials("username", "password")
        });

        task.Wait(TimeSpan.FromSeconds(1));
        ContinueWithAuthCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteDisownDataCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        DisownDataCommandParameters commandParameters = new("myCollectorId", "myRequestId");
        Task<DisownDataCommandResult> task = module.DisownDataAsync(commandParameters);

        task.Wait(TimeSpan.FromSeconds(1));
        DisownDataCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteFailRequestCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        Task<FailRequestCommandResult> task = module.FailRequestAsync(new FailRequestCommandParameters("requestId"));

        task.Wait(TimeSpan.FromSeconds(1));
        FailRequestCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteGetDataCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "bytes": {
                                        "type": "string",
                                        "value": "myNetworkData"
                                      }
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        GetDataCommandParameters commandParameters = new("myRequestId");
        Task<GetDataCommandResult> task = module.GetDataAsync(commandParameters);

        task.Wait(TimeSpan.FromSeconds(1));
        GetDataCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Bytes.Type, Is.EqualTo(BytesValueType.String));
        Assert.That(result.Bytes.Value, Is.EqualTo("myNetworkData"));
    }

    [Test]
    public async Task TestExecuteProvideResponseCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        Task<ProvideResponseCommandResult> task = module.ProvideResponseAsync(new ProvideResponseCommandParameters("requestId"));

        task.Wait(TimeSpan.FromSeconds(1));
        ProvideResponseCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteRemoveDataCollectorCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        RemoveDataCollectorCommandParameters commandParameters = new("myCollectorId");
        Task<RemoveDataCollectorCommandResult> task = module.RemoveDataCollectorAsync(commandParameters);

        task.Wait(TimeSpan.FromSeconds(1));
        RemoveDataCollectorCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteRemoveInterceptCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        Task<RemoveInterceptCommandResult> task = module.RemoveInterceptAsync(new RemoveInterceptCommandParameters("interceptId"));

        task.Wait(TimeSpan.FromSeconds(1));
        RemoveInterceptCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteSetCacheBehaviorCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        Task<SetCacheBehaviorCommandResult> task = module.SetCacheBehaviorAsync(new SetCacheBehaviorCommandParameters());

        task.Wait(TimeSpan.FromSeconds(1));
        SetCacheBehaviorCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteSetExtraHeadersCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        SetExtraHeadersCommandParameters commandParameters = new();
        commandParameters.Headers.Add("X-Extra-Header: headerValue");
        Task<SetExtraHeadersCommandResult> task = module.SetExtraHeadersAsync(commandParameters);

        task.Wait(TimeSpan.FromSeconds(1));
        SetExtraHeadersCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestCanReceiveAuthRequiredEvent()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);

        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnAuthRequired.AddObserver((AuthRequiredEventArgs e) =>
        {
            using (Assert.EnterMultipleScope())
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
             }
            syncEvent.Set();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "network.authRequired",
                             "params": {
                               "context": "myContext",
                               "navigation": "myNavigationId",
                               "isBlocked": false,
                               "redirectCount": 0,
                               "timestamp": {{milliseconds}},
                               "request": {{requestDataJson}},
                               "response": {{responseDataJson}}
                            }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveBeforeRequestSendEvent()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);

        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnBeforeRequestSent.AddObserver((BeforeRequestSentEventArgs e) =>
        {
            using (Assert.EnterMultipleScope())
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
                Assert.That(e.Initiator!.Type, Is.EqualTo(InitiatorType.Parser));
            }
            syncEvent.Set();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "network.beforeRequestSent",
                             "params": {
                               "context": "myContext",
                               "navigation": "myNavigationId",
                               "isBlocked": false,
                               "redirectCount": 0,
                               "timestamp": {{milliseconds}},
                               "request": {{requestDataJson}},
                               "initiator": {
                                 "type": "parser"
                               }
                            }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveFetchErrorEvent()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);

        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnFetchError.AddObserver((FetchErrorEventArgs e) =>
        {
            using (Assert.EnterMultipleScope())
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
            }
            syncEvent.Set();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "network.fetchError",
                             "params": {
                               "context": "myContext",
                               "navigation": "myNavigationId",
                               "isBlocked": false,
                               "redirectCount": 0,
                               "timestamp": {{milliseconds}},
                               "request": {{requestDataJson}},
                               "errorText": "An error occurred"
                            }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveResponseStartedEvent()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);

        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnResponseStarted.AddObserver((ResponseStartedEventArgs e) =>
        {
            using (Assert.EnterMultipleScope())
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
            }
            syncEvent.Set();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "network.responseStarted",
                             "params": {
                               "context": "myContext",
                               "navigation": "myNavigationId",
                               "isBlocked": false,
                               "redirectCount": 0,
                               "timestamp": {{milliseconds}},
                               "request": {{requestDataJson}},
                               "response": {{responseDataJson}}
                            }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveResponseCompletedEvent()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);

        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        NetworkModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnResponseCompleted.AddObserver((ResponseCompletedEventArgs e) =>
        {
            using (Assert.EnterMultipleScope())
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
            }
            syncEvent.Set();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "network.responseCompleted",
                             "params": {
                               "context": "myContext",
                               "navigation": "myNavigationId",
                               "isBlocked": false,
                               "redirectCount": 0,
                               "timestamp": {{milliseconds}},
                               "request": {{requestDataJson}},
                               "response": {{responseDataJson}}
                            }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }
}
