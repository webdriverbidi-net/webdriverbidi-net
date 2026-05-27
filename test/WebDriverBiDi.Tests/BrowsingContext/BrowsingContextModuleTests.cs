namespace WebDriverBiDi.BrowsingContext;

using TestUtilities;

public class BrowsingContextModuleTests
{
    [Fact]
    public async Task TestExecuteActivateCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        ActivateCommandResult result = await module.ActivateAsync(new ActivateCommandParameters("myContextId"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteCaptureScreenshotCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
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
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        CaptureScreenshotCommandResult result = await module.CaptureScreenshotAsync(new CaptureScreenshotCommandParameters("myContextId"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("encodedScreenshotData", result.Data);
    }

    [Fact]
    public async Task TestExecuteCloseCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        CloseCommandResult result = await module.CloseAsync(new CloseCommandParameters("myContextId"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteCreateCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
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
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        CreateCommandResult result = await module.CreateAsync(new CreateCommandParameters(CreateType.Tab), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("myContext", result.BrowsingContextId);
    }

    [Fact]
    public async Task TestExecuteGetTreeCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
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
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        GetTreeCommandResult result = await module.GetTreeAsync(new GetTreeCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);

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
        connection.OnDataSendComplete.AddObserver(async e =>
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
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        GetTreeCommandResult result = await module.GetTreeAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Empty(result.ContextTree);
    }

    [Fact]
    public async Task TestExecuteHandleUserPromptCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        HandleUserPromptCommandResult result = await module.HandleUserPromptAsync(new HandleUserPromptCommandParameters("myContextId"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteLocateNodesCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
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
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        LocateNodesCommandResult result = await module.LocateNodesAsync(new LocateNodesCommandParameters("myContextId", new CssLocator(".selector")), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteNavigateCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
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
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        NavigateCommandResult result = await module.NavigateAsync(new NavigateCommandParameters("myContext", "https://example.com") { Wait = ReadinessState.Complete }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal("myNavigationId", result.NavigationId);
        Assert.Equal("https://example.com", result.Url);
    }

    [Fact]
    public async Task TestExecutePrintCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
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
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        PrintCommandResult result = await module.PrintAsync(new PrintCommandParameters("myContextId"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("encodedPdf", result.Data);
    }

    [Fact]
    public async Task TestExecuteReloadCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
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
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        ReloadCommandResult result = await module.ReloadAsync(new ReloadCommandParameters("myContext"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal("myNavigationId", result.NavigationId);
        Assert.Equal("https://example.com", result.Url);
    }

    [Fact]
    public async Task TestExecuteSetBypassCSPCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        SetBypassCSPCommandResult result = await module.SetBypassCSPAsync(new SetBypassCSPCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteSetViewportCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        SetViewportCommandResult result = await module.SetViewportAsync(new SetViewportCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestExecuteStartScreencastCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "screencast": "myScreencastId",
                                      "path": "path/to/screencast/file"
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        StartScreencastCommandResult result = await module.StartScreencastAsync(new StartScreencastCommandParameters("myContextId"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal("myScreencastId", result.ScreencastId);
        Assert.Equal("path/to/screencast/file", result.Path);
    }

    [Fact]
    public async Task TestExecuteStopScreencastCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "path": "path/to/screencast/file"
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        StopScreencastCommandResult result = await module.StopScreencastAsync(new StopScreencastCommandParameters("myScreencastId"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        Assert.Equal("path/to/screencast/file", result.Path);
    }

    [Fact]
    public async Task TestExecuteTraverseHistoryCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        TraverseHistoryCommandResult result = await module.TraverseHistoryAsync(new TraverseHistoryCommandParameters("myContextId", -3), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task TestCanReceiveContextCreatedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnContextCreated.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("default", e.UserContextId);
            Assert.Equal("openerContext", e.OriginalOpener);
            Assert.Equal("https://example.com", e.Url);
            Assert.Empty(e.Children);
            Assert.Null(e.Parent);

            taskCompletionSource.TrySetResult();
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveContextDestroyedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnContextDestroyed.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Empty(e.Children);
            Assert.Null(e.Parent);

            taskCompletionSource.TrySetResult();
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveDomContentLoadedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnDomContentLoaded.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);

            taskCompletionSource.TrySetResult();
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveDownloadWillBeginEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnDownloadWillBegin.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);
            Assert.Equal("myDownloadId", e.DownloadId);
            Assert.Equal("myFile.file", e.SuggestedFileName);

            taskCompletionSource.TrySetResult();
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveDownloadEndEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnDownloadEnd.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);
            Assert.Equal(DownloadEndStatus.Complete, e.Status);
            Assert.Equal("myDownloadId", e.DownloadId);
            Assert.Equal("myFile.file", e.FilePath);

            taskCompletionSource.TrySetResult();
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveFragmentNavigatedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnFragmentNavigated.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);

            taskCompletionSource.TrySetResult();
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveLoadEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnLoad.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);

            taskCompletionSource.TrySetResult();
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveNavigationAbortedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnNavigationAborted.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);

            taskCompletionSource.TrySetResult();
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveNavigationCommittedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnNavigationCommitted.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);

            taskCompletionSource.TrySetResult();
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveNavigationFailedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnNavigationFailed.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);

            taskCompletionSource.TrySetResult();
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveNavigationStartedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnNavigationStarted.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);
            Assert.Equal("myNavigationId", e.NavigationId);
            Assert.Equal((ulong)((ulong)(epochTimestamp)), e.EpochTimestamp);
            Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), e.Timestamp);

            taskCompletionSource.TrySetResult();
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveHistoryUpdatedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnHistoryUpdated.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal("https://example.com", e.Url);

            taskCompletionSource.TrySetResult();
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveUserPromptClosedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnUserPromptClosed.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.True(e.IsAccepted);
            Assert.Equal("my prompt text", e.UserText);

            taskCompletionSource.TrySetResult();
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveUserPromptOpenedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new(connection));
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        BrowsingContextModule module = driver.BrowsingContext;

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnUserPromptOpened.AddObserver(e =>
        {
            Assert.Equal("myContext", e.BrowsingContextId);
            Assert.Equal(UserPromptType.Confirm, e.PromptType);
            Assert.Equal("my message text", e.Message);

            taskCompletionSource.TrySetResult();
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
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }
}
