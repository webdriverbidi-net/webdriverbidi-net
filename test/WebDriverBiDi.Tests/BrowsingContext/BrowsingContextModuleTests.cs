namespace WebDriverBiDi.BrowsingContext;

using TestUtilities;

public class BrowsingContextModuleTests
{
    [Fact]
    public async Task TestExecuteActivateCommand()
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

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        Task<ActivateCommandResult> task = module.ActivateAsync(new ActivateCommandParameters("myContextId"), cancellationToken: TestContext.Current.CancellationToken);
        ActivateCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteCaptureScreenshotCommand()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "data": "encodedScreenshotData"
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        Task<CaptureScreenshotCommandResult> task = module.CaptureScreenshotAsync(new CaptureScreenshotCommandParameters("myContextId"), cancellationToken: TestContext.Current.CancellationToken);
        CaptureScreenshotCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("encodedScreenshotData", result.Data);
    }

    [Fact]
    public async Task TestExecuteCloseCommand()
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

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        Task<CloseCommandResult> task = module.CloseAsync(new CloseCommandParameters("myContextId"), cancellationToken: TestContext.Current.CancellationToken);
        CloseCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteCreateCommand()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "context": "myContext"
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        Task<CreateCommandResult> task = module.CreateAsync(new CreateCommandParameters(CreateType.Tab), cancellationToken: TestContext.Current.CancellationToken);
        CreateCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("myContext", result.BrowsingContextId);
    }

    [Fact]
    public async Task TestExecuteGetTreeCommand()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "contexts": [
                                        {
                                          "context": "myContext",
                                          "clientWindow": "myClientWindow",
                                          "url": "https://example.com",
                                          "originalOpener": null,
                                          "userContext": "default",
                                          "children": []
                                        }
                                      ]
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        Task<GetTreeCommandResult> task = module.GetTreeAsync(new GetTreeCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        GetTreeCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Single(result.ContextTree);

        Assert.Equal("myContext", result.ContextTree[0].BrowsingContextId);
        Assert.Equal("https://example.com", result.ContextTree[0].Url);
        Assert.Empty(result.ContextTree[0].Children);
    }

    [Fact]
    public async Task TestExecuteGetTreeCommandWithNoArgument()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "contexts": []
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        Task<GetTreeCommandResult> task = module.GetTreeAsync(cancellationToken: TestContext.Current.CancellationToken);
        GetTreeCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Empty(result.ContextTree);
    }

    [Fact]
    public async Task TestExecuteHandleUserPromptCommand()
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

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        Task<HandleUserPromptCommandResult> task = module.HandleUserPromptAsync(new HandleUserPromptCommandParameters("myContextId"), cancellationToken: TestContext.Current.CancellationToken);
        HandleUserPromptCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteLocateNodesCommand()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "nodes": [
                                        {
                                          "type": "node",
                                          "sharedId": "mySharedId",
                                          "value": {
                                            "nodeType": 1,
                                            "nodeValue": "",
                                            "childNodeCount": 0
                                          }
                                        }
                                      ]
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        Task<LocateNodesCommandResult> task = module.LocateNodesAsync(new LocateNodesCommandParameters("myContextId", new CssLocator(".selector")), cancellationToken: TestContext.Current.CancellationToken);
        LocateNodesCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteNavigateCommand()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "navigation": "myNavigationId",
                                      "url": "https://example.com"
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        Task<NavigateCommandResult> task = module.NavigateAsync(new NavigateCommandParameters("myContext", "https://example.com") { Wait = ReadinessState.Complete }, cancellationToken: TestContext.Current.CancellationToken);
        NavigateCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal("myNavigationId", result.NavigationId);
        Assert.Equal("https://example.com", result.Url);
    }

    [Fact]
    public async Task TestExecutePrintCommand()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "data": "encodedPdf"
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        Task<PrintCommandResult> task = module.PrintAsync(new PrintCommandParameters("myContextId"), cancellationToken: TestContext.Current.CancellationToken);
        PrintCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("encodedPdf", result.Data);
    }

    [Fact]
    public async Task TestExecuteReloadCommand()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "navigation": "myNavigationId",
                                      "url": "https://example.com"
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        Task<ReloadCommandResult> task = module.ReloadAsync(new ReloadCommandParameters("myContext"), cancellationToken: TestContext.Current.CancellationToken);
        ReloadCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal("myNavigationId", result.NavigationId);
        Assert.Equal("https://example.com", result.Url);
    }

    [Fact]
    public async Task TestExecuteSetBypassCSPCommand()
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

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        Task<SetBypassCSPCommandResult> task = module.SetBypassCSPAsync(new SetBypassCSPCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        SetBypassCSPCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteSetViewportCommand()
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

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        Task<SetViewportCommandResult> task = module.SetViewportAsync(new SetViewportCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        SetViewportCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteTraverseHistoryCommand()
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

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        Task<TraverseHistoryCommandResult> task = module.TraverseHistoryAsync(new TraverseHistoryCommandParameters("myContextId", -3), cancellationToken: TestContext.Current.CancellationToken);
        TraverseHistoryCommandResult result = await task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestCanReceiveContextCreatedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        ManualResetEvent syncEvent = new(false);
        module.OnContextCreated.AddObserver((BrowsingContextEventArgs e) =>
        {

            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("default", e.UserContextId);
            Assert.Equal("openerContext", e.OriginalOpener);
            Assert.Equal("https://example.com", e.Url);
            Assert.Empty(e.Children);
            Assert.Null(e.Parent);

            syncEvent.Set();
        });

        string eventJson = """
                           {
                             "type": "event",
                             "method": "browsingContext.contextCreated",
                             "params": {
                               "context": "myContext",
                               "clientWindow": "myClientWindow",
                               "url": "https://example.com",
                               "originalOpener": "openerContext",
                               "userContext": "default",
                               "children": []
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task TestCanReceiveContextDestroyedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        ManualResetEvent syncEvent = new(false);
        module.OnContextDestroyed.AddObserver((BrowsingContextEventArgs e) =>
        {

            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Empty(e.Children);
            Assert.Null(e.Parent);

            syncEvent.Set();
        });

        string eventJson = """
                           {
                             "type": "event",
                             "method": "browsingContext.contextDestroyed",
                             "params": {
                               "context": "myContext",
                               "clientWindow": "myClientWindow",
                               "url": "https://example.com",
                               "originalOpener": "openerContext",
                               "userContext": "default",
                               "children": []
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task TestCanReceiveDomContentLoadedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnDomContentLoaded.AddObserver((NavigationEventArgs e) =>
        {

            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);

            syncEvent.Set();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "browsingContext.domContentLoaded",
                             "params": {
                               "context": "myContext",
                               "url": "https://example.com",
                               "timestamp": {{epochTimestamp}},
                               "navigation": "myNavigationId"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(12500));
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task TestCanReceiveDownloadWillBeginEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnDownloadWillBegin.AddObserver((DownloadWillBeginEventArgs e) =>
        {

            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);
            Assert.Equal("myDownloadId", e.DownloadId);
            Assert.Equal("myFile.file", e.SuggestedFileName);

            syncEvent.Set();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "browsingContext.downloadWillBegin",
                             "params": {
                               "context": "myContext",
                               "url": "https://example.com",
                               "timestamp": {{epochTimestamp}},
                               "navigation": "myNavigationId",
                               "download": "myDownloadId",
                               "suggestedFileName": "myFile.file"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task TestCanReceiveDownloadEndEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnDownloadEnd.AddObserver((DownloadEndEventArgs e) =>
        {

            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);
            Assert.Equal(DownloadEndStatus.Complete, e.Status);
            Assert.Equal("myDownloadId", e.DownloadId);
            Assert.Equal("myFile.file", e.FilePath);

            syncEvent.Set();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "browsingContext.downloadEnd",
                             "params": {
                               "context": "myContext",
                               "url": "https://example.com",
                               "timestamp": {{epochTimestamp}},
                               "navigation": "myNavigationId",
                               "download": "myDownloadId",
                               "status": "complete",
                               "filepath": "myFile.file"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task TestCanReceiveFragmentNavigatedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnFragmentNavigated.AddObserver((NavigationEventArgs e) =>
        {

            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);

            syncEvent.Set();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "browsingContext.fragmentNavigated",
                             "params": {
                               "context": "myContext",
                               "url": "https://example.com",
                               "timestamp": {{epochTimestamp}},
                               "navigation": "myNavigationId"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task TestCanReceiveLoadEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnLoad.AddObserver((NavigationEventArgs e) =>
        {

            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);

            syncEvent.Set();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "browsingContext.load",
                             "params": {
                               "context": "myContext",
                               "url": "https://example.com",
                               "timestamp": {{epochTimestamp}},
                               "navigation": "myNavigationId"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task TestCanReceiveNavigationAbortedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnNavigationAborted.AddObserver((NavigationEventArgs e) =>
        {

            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);

            syncEvent.Set();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "browsingContext.navigationAborted",
                             "params": {
                               "context": "myContext",
                               "url": "https://example.com",
                               "timestamp": {{epochTimestamp}},
                               "navigation": "myNavigationId"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task TestCanReceiveNavigationCommittedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnNavigationCommitted.AddObserver((NavigationEventArgs e) =>
        {

            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);

            syncEvent.Set();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "browsingContext.navigationCommitted",
                             "params": {
                               "context": "myContext",
                               "url": "https://example.com",
                               "timestamp": {{epochTimestamp}},
                               "navigation": "myNavigationId"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task TestCanReceiveNavigationFailedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnNavigationFailed.AddObserver((NavigationEventArgs e) =>
        {

            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);

            syncEvent.Set();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "browsingContext.navigationFailed",
                             "params": {
                               "context": "myContext",
                               "url": "https://example.com",
                               "timestamp": {{epochTimestamp}},
                               "navigation": "myNavigationId"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task TestCanReceiveNavigationStartedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnNavigationStarted.AddObserver((NavigationEventArgs e) =>
        {

            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);

            syncEvent.Set();
        });

        string eventJson = $$"""
                           {
                             "type": "event",
                             "method": "browsingContext.navigationStarted",
                             "params": {
                               "context": "myContext",
                               "url": "https://example.com",
                               "timestamp": {{epochTimestamp}},
                               "navigation": "myNavigationId"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task TestCanReceiveHistoryUpdatedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        ManualResetEvent syncEvent = new(false);
        module.OnHistoryUpdated.AddObserver((HistoryUpdatedEventArgs e) =>
        {

            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);

            syncEvent.Set();
        });

        string eventJson = """
                           {
                             "type": "event",
                             "method": "browsingContext.historyUpdated",
                             "params": {
                               "context": "myContext",
                               "url": "https://example.com",
                               "timestamp": 300
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task TestCanReceiveUserPromptClosedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        ManualResetEvent syncEvent = new(false);
        module.OnUserPromptClosed.AddObserver((UserPromptClosedEventArgs e) =>
        {

            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.True(e.IsAccepted);
            Assert.Equal("my prompt text", e.UserText);

            syncEvent.Set();
        });

        string eventJson = """
                           {
                             "type": "event",
                             "method": "browsingContext.userPromptClosed",
                             "params": {
                               "context": "myContext",
                               "accepted": true,
                               "userText": "my prompt text"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task TestCanReceiveUserPromptOpenedEvent()
    {
        TestWebSocketConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        ManualResetEvent syncEvent = new(false);
        module.OnUserPromptOpened.AddObserver((UserPromptOpenedEventArgs e) =>
        {

            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal(UserPromptType.Confirm, e.PromptType);
            Assert.Equal("my message text", e.Message);

            syncEvent.Set();
        });

        string eventJson = """
                           {
                             "type": "event",
                             "method": "browsingContext.userPromptOpened",
                             "params": {
                               "context": "myContext",
                               "type": "confirm",
                               "handler": "accept",
                               "message": "my message text"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.True(eventRaised);
    }
}
