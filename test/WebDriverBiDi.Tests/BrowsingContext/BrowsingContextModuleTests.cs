namespace WebDriverBiDi.BrowsingContext;

using TestUtilities;

[TestFixture]
public class BrowsingContextModuleTests
{
    [Test]
    public async Task TestExecuteActivateCommand()
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
        BrowsingContextModule module = new(driver);

        Task<ActivateCommandResult> task = module.ActivateAsync(new ActivateCommandParameters("myContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        ActivateCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteCaptureScreenshotCommand()
    {
        TestConnection connection = new();
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
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        Task<CaptureScreenshotCommandResult> task = module.CaptureScreenshotAsync(new CaptureScreenshotCommandParameters("myContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        CaptureScreenshotCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data, Is.EqualTo("encodedScreenshotData"));
    }

    [Test]
    public async Task TestExecuteCloseCommand()
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
        BrowsingContextModule module = new(driver);

        Task<CloseCommandResult> task = module.CloseAsync(new CloseCommandParameters("myContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        CloseCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteCreateCommand()
    {
        TestConnection connection = new();
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
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        Task<CreateCommandResult> task = module.CreateAsync(new CreateCommandParameters(CreateType.Tab));
        task.Wait(TimeSpan.FromSeconds(1));
        CreateCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.BrowsingContextId, Is.EqualTo("myContext"));
    }

    [Test]
    public async Task TestExecuteGetTreeCommand()
    {
        TestConnection connection = new();
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
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        Task<GetTreeCommandResult> task = module.GetTreeAsync(new GetTreeCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        GetTreeCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ContextTree, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ContextTree[0].BrowsingContextId, Is.EqualTo("myContext"));
            Assert.That(result.ContextTree[0].Url, Is.EqualTo("https://example.com"));
            Assert.That(result.ContextTree[0].Children, Has.Count.EqualTo(0));
        }
    }

    [Test]
    public async Task TestExecuteHandleUserPromptCommand()
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
        BrowsingContextModule module = new(driver);

        Task<HandleUserPromptCommandResult> task = module.HandleUserPromptAsync(new HandleUserPromptCommandParameters("myContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        HandleUserPromptCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteLocateNodesCommand()
    {
        TestConnection connection = new();
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
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        Task<LocateNodesCommandResult> task = module.LocateNodesAsync(new LocateNodesCommandParameters("myContextId", new CssLocator(".selector")));
        task.Wait(TimeSpan.FromSeconds(1));
        LocateNodesCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteNavigateCommand()
    {
        TestConnection connection = new();
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
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        Task<NavigateCommandResult> task = module.NavigateAsync(new NavigateCommandParameters("myContext", "https://example.com") { Wait = ReadinessState.Complete });
        task.Wait(TimeSpan.FromSeconds(1));
        NavigateCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.NavigationId, Is.EqualTo("myNavigationId"));
            Assert.That(result.Url, Is.EqualTo("https://example.com"));
        }
    }

    [Test]
    public async Task TestExecutePrintCommand()
    {
        TestConnection connection = new();
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
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        Task<PrintCommandResult> task = module.PrintAsync(new PrintCommandParameters("myContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        PrintCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data, Is.EqualTo("encodedPdf"));
    }

    [Test]
    public async Task TestExecuteReloadCommand()
    {
        TestConnection connection = new();
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
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        Task<ReloadCommandResult> task = module.ReloadAsync(new ReloadCommandParameters("myContext"));
        task.Wait(TimeSpan.FromSeconds(1));
        ReloadCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.NavigationId, Is.EqualTo("myNavigationId"));
            Assert.That(result.Url, Is.EqualTo("https://example.com"));
        }
    }

    [Test]
    public async Task TestExecuteSetViewportCommand()
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
        BrowsingContextModule module = new(driver);

        Task<SetViewportCommandResult> task = module.SetViewportAsync(new SetViewportCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        SetViewportCommandResult result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteTraverseHistoryCommand()
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
        BrowsingContextModule module = new(driver);

        Task<TraverseHistoryCommandResult> task = module.TraverseHistoryAsync(new TraverseHistoryCommandParameters("myContextId", -3));
        task.Wait(TimeSpan.FromSeconds(1));
        TraverseHistoryCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task TestCanReceiveContextCreatedEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnContextCreated.AddObserver((BrowsingContextEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.UserContextId, Is.EqualTo("default"));
                Assert.That(e.OriginalOpener, Is.EqualTo("openerContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.Children, Has.Count.EqualTo(0));
                Assert.That(e.Parent, Is.Null);
            }
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
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveContextDestroyedEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnContextDestroyed.AddObserver((BrowsingContextEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.Children, Has.Count.EqualTo(0));
                Assert.That(e.Parent, Is.Null);
            }
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
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveDomContentLoadedEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnDomContentLoaded.AddObserver((NavigationEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.EpochTimestamp, Is.EqualTo(epochTimestamp));
                Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            }
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
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveDownloadWillBeginEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnDownloadWillBegin.AddObserver((DownloadWillBeginEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.EpochTimestamp, Is.EqualTo(epochTimestamp));
                Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
                Assert.That(e.SuggestedFileName, Is.EqualTo("myFile.file"));
            }
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
                               "suggestedFileName": "myFile.file"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveDownloadEndEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnDownloadEndEvent.AddObserver((DownloadEndEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.EpochTimestamp, Is.EqualTo(epochTimestamp));
                Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
                Assert.That(e.Status, Is.EqualTo(DownloadEndStatus.Complete));
                Assert.That(e.FilePath, Is.EqualTo("myFile.file"));
            }
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
                               "status": "complete",
                               "filepath": "myFile.file"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromMilliseconds(250));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveFragmentNavigatedEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnFragmentNavigated.AddObserver((NavigationEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.EpochTimestamp, Is.EqualTo(epochTimestamp));
                Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            }
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
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveLoadEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnLoad.AddObserver((NavigationEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.EpochTimestamp, Is.EqualTo(epochTimestamp));
                Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            }
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
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveNavigationAbortedEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnNavigationAborted.AddObserver((NavigationEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.EpochTimestamp, Is.EqualTo(epochTimestamp));
                Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            }
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
        Assert.That(eventRaised, Is.True);
    }

  [Test]
  public async Task TestCanReceiveNavigationCommittedEvent()
  {
      TestConnection connection = new();
      BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
      await driver.StartAsync("ws:localhost");
      BrowsingContextModule module = new(driver);

      ManualResetEvent syncEvent = new(false);
      long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
      module.OnNavigationCommitted.AddObserver((NavigationEventArgs e) =>
      {
          using (Assert.EnterMultipleScope())
          {
          Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
          Assert.That(e.Url, Is.EqualTo("https://example.com"));
          Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
          Assert.That(e.EpochTimestamp, Is.EqualTo(epochTimestamp));
          Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
        }
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
      Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveNavigationFailedEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnNavigationFailed.AddObserver((NavigationEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.EpochTimestamp, Is.EqualTo(epochTimestamp));
                Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            }
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
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveNavigationStartedEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        module.OnNavigationStarted.AddObserver((NavigationEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.EpochTimestamp, Is.EqualTo(epochTimestamp));
                Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            }
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
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveHistoryUpdatedEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnHistoryUpdated.AddObserver((HistoryUpdatedEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
            }
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
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveUserPromptClosedEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnUserPromptClosed.AddObserver((UserPromptClosedEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.IsAccepted, Is.EqualTo(true));
                Assert.That(e.UserText, Is.EqualTo("my prompt text"));
            }
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
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveUserPromptOpenedEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new(connection));
        await driver.StartAsync("ws:localhost");
        BrowsingContextModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.OnUserPromptOpened.AddObserver((UserPromptOpenedEventArgs e) => {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.PromptType, Is.EqualTo(UserPromptType.Confirm));
                Assert.That(e.Message, Is.EqualTo("my message text"));
            }
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
        Assert.That(eventRaised, Is.True);
    }
}
