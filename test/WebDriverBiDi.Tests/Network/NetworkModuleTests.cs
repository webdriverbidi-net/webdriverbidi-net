namespace WebDriverBiDi.Network;

using WebDriverBiDi.TestUtilities;

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

    [Fact]
    public async Task TestExecuteAddInterceptCommand()
    {
        TestWebSocketConnection connection = new();
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        AddInterceptCommandParameters commandParameters = new(InterceptPhase.BeforeRequestSent)
        {
            UrlPatterns = [new UrlPatternString("https://example.com/*")]
        };
        Task<AddInterceptCommandResult> task = module.AddInterceptAsync(commandParameters, cancellationToken: TestContext.Current.CancellationToken);

        AddInterceptCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("myInterceptId", result.InterceptId);
    }

    [Fact]
    public async Task TestExecuteAddDataCollectorCommand()
    {
        TestWebSocketConnection connection = new();
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        AddDataCollectorCommandParameters commandParameters = new(1024 * 1024);
        Task<AddDataCollectorCommandResult> task = module.AddDataCollectorAsync(commandParameters, cancellationToken: TestContext.Current.CancellationToken);

        AddDataCollectorCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("myCollectorId", result.CollectorId);
    }

    [Fact]
    public async Task TestExecuteContinueRequestCommand()
    {
        TestWebSocketConnection connection = new();
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        Task<ContinueRequestCommandResult> task = module.ContinueRequestAsync(new ContinueRequestCommandParameters("requestId"), cancellationToken: TestContext.Current.CancellationToken);

        ContinueRequestCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteContinueResponseCommand()
    {
        TestWebSocketConnection connection = new();
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        Task<ContinueResponseCommandResult> task = module.ContinueResponseAsync(new ContinueResponseCommandParameters("requestId"), cancellationToken: TestContext.Current.CancellationToken);

        ContinueResponseCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteContinueWithAuthCommand()
    {
        TestWebSocketConnection connection = new();
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        Task<ContinueWithAuthCommandResult> task = module.ContinueWithAuthAsync(new ContinueWithAuthCommandParameters("requestId")
        {
            Action = ContinueWithAuthActionType.ProvideCredentials,
            Credentials = new AuthCredentials("username", "password")
        }, cancellationToken: TestContext.Current.CancellationToken);

        ContinueWithAuthCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteDisownDataCommand()
    {
        TestWebSocketConnection connection = new();
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        DisownDataCommandParameters commandParameters = new("myCollectorId", "myRequestId");
        Task<DisownDataCommandResult> task = module.DisownDataAsync(commandParameters, cancellationToken: TestContext.Current.CancellationToken);

        DisownDataCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteFailRequestCommand()
    {
        TestWebSocketConnection connection = new();
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        Task<FailRequestCommandResult> task = module.FailRequestAsync(new FailRequestCommandParameters("requestId"), cancellationToken: TestContext.Current.CancellationToken);

        FailRequestCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteGetDataCommand()
    {
        TestWebSocketConnection connection = new();
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        GetDataCommandParameters commandParameters = new("myRequestId");
        Task<GetDataCommandResult> task = module.GetDataAsync(commandParameters, cancellationToken: TestContext.Current.CancellationToken);

        GetDataCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(BytesValueType.String, result.Bytes.Type);
        Assert.Equal("myNetworkData", result.Bytes.Value);
    }

    [Fact]
    public async Task TestExecuteProvideResponseCommand()
    {
        TestWebSocketConnection connection = new();
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        Task<ProvideResponseCommandResult> task = module.ProvideResponseAsync(new ProvideResponseCommandParameters("requestId"), cancellationToken: TestContext.Current.CancellationToken);

        ProvideResponseCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteRemoveDataCollectorCommand()
    {
        TestWebSocketConnection connection = new();
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        RemoveDataCollectorCommandParameters commandParameters = new("myCollectorId");
        Task<RemoveDataCollectorCommandResult> task = module.RemoveDataCollectorAsync(commandParameters, cancellationToken: TestContext.Current.CancellationToken);

        RemoveDataCollectorCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteRemoveInterceptCommand()
    {
        TestWebSocketConnection connection = new();
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        Task<RemoveInterceptCommandResult> task = module.RemoveInterceptAsync(new RemoveInterceptCommandParameters("interceptId"), cancellationToken: TestContext.Current.CancellationToken);

        RemoveInterceptCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteSetCacheBehaviorCommand()
    {
        TestWebSocketConnection connection = new();
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        Task<SetCacheBehaviorCommandResult> task = module.SetCacheBehaviorAsync(new SetCacheBehaviorCommandParameters(CacheBehavior.Default), cancellationToken: TestContext.Current.CancellationToken);

        SetCacheBehaviorCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteSetExtraHeadersCommand()
    {
        TestWebSocketConnection connection = new();
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

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        SetExtraHeadersCommandParameters commandParameters = new();
        commandParameters.Headers.Add("X-Extra-Header: headerValue");
        Task<SetExtraHeadersCommandResult> task = module.SetExtraHeadersAsync(commandParameters, cancellationToken: TestContext.Current.CancellationToken);

        SetExtraHeadersCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestCanReceiveAuthRequiredEvent()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);

        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnAuthRequired.AddObserver((AuthRequiredEventArgs e) =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal(0u, e.RedirectCount);
            Assert.Equal(eventTime, e.Timestamp);
            Assert.Equal((ulong)((ulong)(milliseconds)), e.EpochTimestamp);
            Assert.Equal("myRequestId", e.Request.RequestId);
            Assert.Equal("https://example.com", e.Request.Url);
            Assert.Equal("get", e.Request.Method);
            Assert.Single(e.Request.Headers);
            Assert.Equal("headerName", e.Request.Headers[0].Name);
            Assert.Equal(BytesValueType.String, e.Request.Headers[0].Value.Type);
            Assert.Equal("headerValue", e.Request.Headers[0].Value.Value);
            Assert.Single(e.Request.Cookies);
            Assert.Equal("cookieName", e.Request.Cookies[0].Name);
            Assert.Equal(BytesValueType.String, e.Request.Cookies[0].Value.Type);
            Assert.Equal("cookieValue", e.Request.Cookies[0].Value.Value);
            Assert.Equal("cookieDomain", e.Request.Cookies[0].Domain);
            Assert.Equal("/cookiePath", e.Request.Cookies[0].Path);
            Assert.Equal(CookieSameSiteValue.Strict, e.Request.Cookies[0].SameSite);
            Assert.False(e.Request.Cookies[0].Secure);
            Assert.True(e.Request.Cookies[0].HttpOnly);
            Assert.Equal(10, e.Request.Cookies[0].Size);
            Assert.Null(e.Request.Cookies[0].Expires);
            Assert.Equal(100u, e.Request.HeadersSize);
            Assert.Equal(300u, e.Request.BodySize);
            Assert.NotNull(e.Request.Timings);
            Assert.Equal("https://example.com", e.Response.Url);
            Assert.Equal("https", e.Response.Protocol);
            Assert.Equal(200u, e.Response.Status);
            Assert.Equal("OK", e.Response.StatusText);
            Assert.False(e.Response.FromCache);
            Assert.Single(e.Response.Headers);
            Assert.Equal("headerName", e.Response.Headers[0].Name);
            Assert.Equal(BytesValueType.String, e.Response.Headers[0].Value.Type);
            Assert.Equal("headerValue", e.Response.Headers[0].Value.Value);
            Assert.Equal("text/html", e.Response.MimeType);
            Assert.Equal(400u, e.Response.BytesReceived);
            Assert.Equal(100u, e.Response.HeadersSize);
            Assert.Equal(300u, e.Response.BodySize);
            Assert.Equal(300u, e.Response.Content.Size);

            taskCompletionSource.TrySetResult();
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
                               "request": {{this.requestDataJson}},
                               "response": {{this.responseDataJson}}
                            }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveBeforeRequestSendEvent()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);

        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnBeforeRequestSent.AddObserver((BeforeRequestSentEventArgs e) =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal(0u, e.RedirectCount);
            Assert.Equal(eventTime, e.Timestamp);
            Assert.Equal((ulong)((ulong)(milliseconds)), e.EpochTimestamp);
            Assert.Equal("myRequestId", e.Request.RequestId);
            Assert.Equal("https://example.com", e.Request.Url);
            Assert.Equal("get", e.Request.Method);
            Assert.Single(e.Request.Headers);
            Assert.Equal("headerName", e.Request.Headers[0].Name);
            Assert.Equal(BytesValueType.String, e.Request.Headers[0].Value.Type);
            Assert.Equal("headerValue", e.Request.Headers[0].Value.Value);
            Assert.Single(e.Request.Cookies);
            Assert.Equal("cookieName", e.Request.Cookies[0].Name);
            Assert.Equal(BytesValueType.String, e.Request.Cookies[0].Value.Type);
            Assert.Equal("cookieValue", e.Request.Cookies[0].Value.Value);
            Assert.Equal("cookieDomain", e.Request.Cookies[0].Domain);
            Assert.Equal("/cookiePath", e.Request.Cookies[0].Path);
            Assert.Equal(CookieSameSiteValue.Strict, e.Request.Cookies[0].SameSite);
            Assert.False(e.Request.Cookies[0].Secure);
            Assert.True(e.Request.Cookies[0].HttpOnly);
            Assert.Equal(10, e.Request.Cookies[0].Size);
            Assert.Null(e.Request.Cookies[0].Expires);
            Assert.Equal(100u, e.Request.HeadersSize);
            Assert.Equal(300u, e.Request.BodySize);
            Assert.NotNull(e.Request.Timings);
            Assert.NotNull(e.Initiator);
            Assert.Equal(InitiatorType.Parser, e.Initiator.Type);

            taskCompletionSource.TrySetResult();
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
                               "request": {{this.requestDataJson}},
                               "initiator": {
                                 "type": "parser"
                               }
                            }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveFetchErrorEvent()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);

        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnFetchError.AddObserver((FetchErrorEventArgs e) =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal(0u, e.RedirectCount);
            Assert.Equal(eventTime, e.Timestamp);
            Assert.Equal((ulong)((ulong)(milliseconds)), e.EpochTimestamp);
            Assert.Equal("myRequestId", e.Request.RequestId);
            Assert.Equal("https://example.com", e.Request.Url);
            Assert.Equal("get", e.Request.Method);
            Assert.Single(e.Request.Headers);
            Assert.Equal("headerName", e.Request.Headers[0].Name);
            Assert.Equal(BytesValueType.String, e.Request.Headers[0].Value.Type);
            Assert.Equal("headerValue", e.Request.Headers[0].Value.Value);
            Assert.Single(e.Request.Cookies);
            Assert.Equal("cookieName", e.Request.Cookies[0].Name);
            Assert.Equal(BytesValueType.String, e.Request.Cookies[0].Value.Type);
            Assert.Equal("cookieValue", e.Request.Cookies[0].Value.Value);
            Assert.Equal("cookieDomain", e.Request.Cookies[0].Domain);
            Assert.Equal("/cookiePath", e.Request.Cookies[0].Path);
            Assert.Equal(CookieSameSiteValue.Strict, e.Request.Cookies[0].SameSite);
            Assert.False(e.Request.Cookies[0].Secure);
            Assert.True(e.Request.Cookies[0].HttpOnly);
            Assert.Equal(10, e.Request.Cookies[0].Size);
            Assert.Null(e.Request.Cookies[0].Expires);
            Assert.Equal(100u, e.Request.HeadersSize);
            Assert.Equal(300u, e.Request.BodySize);
            Assert.NotNull(e.Request.Timings);
            Assert.Equal("An error occurred", e.ErrorText);

            taskCompletionSource.TrySetResult();
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
                               "request": {{this.requestDataJson}},
                               "errorText": "An error occurred"
                            }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveResponseStartedEvent()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);

        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnResponseStarted.AddObserver((ResponseStartedEventArgs e) =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal(0u, e.RedirectCount);
            Assert.Equal(eventTime, e.Timestamp);
            Assert.Equal((ulong)((ulong)(milliseconds)), e.EpochTimestamp);
            Assert.Equal("myRequestId", e.Request.RequestId);
            Assert.Equal("https://example.com", e.Request.Url);
            Assert.Equal("get", e.Request.Method);
            Assert.Single(e.Request.Headers);
            Assert.Equal("headerName", e.Request.Headers[0].Name);
            Assert.Equal(BytesValueType.String, e.Request.Headers[0].Value.Type);
            Assert.Equal("headerValue", e.Request.Headers[0].Value.Value);
            Assert.Single(e.Request.Cookies);
            Assert.Equal("cookieName", e.Request.Cookies[0].Name);
            Assert.Equal(BytesValueType.String, e.Request.Cookies[0].Value.Type);
            Assert.Equal("cookieValue", e.Request.Cookies[0].Value.Value);
            Assert.Equal("cookieDomain", e.Request.Cookies[0].Domain);
            Assert.Equal("/cookiePath", e.Request.Cookies[0].Path);
            Assert.Equal(CookieSameSiteValue.Strict, e.Request.Cookies[0].SameSite);
            Assert.False(e.Request.Cookies[0].Secure);
            Assert.True(e.Request.Cookies[0].HttpOnly);
            Assert.Equal(10, e.Request.Cookies[0].Size);
            Assert.Null(e.Request.Cookies[0].Expires);
            Assert.Equal(100u, e.Request.HeadersSize);
            Assert.Equal(300u, e.Request.BodySize);
            Assert.NotNull(e.Request.Timings);
            Assert.Equal("https://example.com", e.Response.Url);
            Assert.Equal("https", e.Response.Protocol);
            Assert.Equal(200u, e.Response.Status);
            Assert.Equal("OK", e.Response.StatusText);
            Assert.False(e.Response.FromCache);
            Assert.Single(e.Response.Headers);
            Assert.Equal("headerName", e.Response.Headers[0].Name);
            Assert.Equal(BytesValueType.String, e.Response.Headers[0].Value.Type);
            Assert.Equal("headerValue", e.Response.Headers[0].Value.Value);
            Assert.Equal("text/html", e.Response.MimeType);
            Assert.Equal(400u, e.Response.BytesReceived);
            Assert.Equal(100u, e.Response.HeadersSize);
            Assert.Equal(300u, e.Response.BodySize);
            Assert.Equal(300u, e.Response.Content.Size);

            taskCompletionSource.TrySetResult();
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
                               "request": {{this.requestDataJson}},
                               "response": {{this.responseDataJson}}
                            }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveResponseCompletedEvent()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);

        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        NetworkModule module = driver.Network;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnResponseCompleted.AddObserver((ResponseCompletedEventArgs e) =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal(0u, e.RedirectCount);
            Assert.Equal(eventTime, e.Timestamp);
            Assert.Equal((ulong)((ulong)(milliseconds)), e.EpochTimestamp);
            Assert.Equal("myRequestId", e.Request.RequestId);
            Assert.Equal("https://example.com", e.Request.Url);
            Assert.Equal("get", e.Request.Method);
            Assert.Single(e.Request.Headers);
            Assert.Equal("headerName", e.Request.Headers[0].Name);
            Assert.Equal(BytesValueType.String, e.Request.Headers[0].Value.Type);
            Assert.Equal("headerValue", e.Request.Headers[0].Value.Value);
            Assert.Single(e.Request.Cookies);
            Assert.Equal("cookieName", e.Request.Cookies[0].Name);
            Assert.Equal(BytesValueType.String, e.Request.Cookies[0].Value.Type);
            Assert.Equal("cookieValue", e.Request.Cookies[0].Value.Value);
            Assert.Equal("cookieDomain", e.Request.Cookies[0].Domain);
            Assert.Equal("/cookiePath", e.Request.Cookies[0].Path);
            Assert.Equal(CookieSameSiteValue.Strict, e.Request.Cookies[0].SameSite);
            Assert.False(e.Request.Cookies[0].Secure);
            Assert.True(e.Request.Cookies[0].HttpOnly);
            Assert.Equal(10, e.Request.Cookies[0].Size);
            Assert.Null(e.Request.Cookies[0].Expires);
            Assert.Equal(100u, e.Request.HeadersSize);
            Assert.Equal(300u, e.Request.BodySize);
            Assert.NotNull(e.Request.Timings);
            Assert.Equal("https://example.com", e.Response.Url);
            Assert.Equal("https", e.Response.Protocol);
            Assert.Equal(200u, e.Response.Status);
            Assert.Equal("OK", e.Response.StatusText);
            Assert.False(e.Response.FromCache);
            Assert.Single(e.Response.Headers);
            Assert.Equal("headerName", e.Response.Headers[0].Name);
            Assert.Equal(BytesValueType.String, e.Response.Headers[0].Value.Type);
            Assert.Equal("headerValue", e.Response.Headers[0].Value.Value);
            Assert.Equal("text/html", e.Response.MimeType);
            Assert.Equal(400u, e.Response.BytesReceived);
            Assert.Equal(100u, e.Response.HeadersSize);
            Assert.Equal(300u, e.Response.BodySize);
            Assert.Equal(300u, e.Response.Content.Size);

            taskCompletionSource.TrySetResult();
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
                               "request": {{this.requestDataJson}},
                               "response": {{this.responseDataJson}}
                            }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }
}
