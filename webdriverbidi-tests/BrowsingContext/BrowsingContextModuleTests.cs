namespace WebDriverBidi.BrowsingContext;

using TestUtilities;

[TestFixture]
public class BrowsingContextModuleTests
{
    [Test]
    public void TestExecuteCaptureScreenshotCommand()
    {
        string responseJson = @"{ ""result"": { ""data"": ""encodedScreenshotData"" } }";
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        var task = module.CaptureScreenshot(new CaptureScreenshotCommandSettings("myContextId"));
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data, Is.EqualTo("encodedScreenshotData"));
    }

    [Test]
    public void TestExecuteCloseCommand()
    {
        string responseJson = @"{ ""result"": {} }";
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        var task = module.Close(new CloseCommandSettings("myContextId"));
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void TestExecuteCreateCommand()
    {
        string responseJson = @"{ ""result"": { ""context"": ""myContext"" } }";
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        var task = module.Create(new CreateCommandSettings(BrowsingContextCreateType.Tab));
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.BrowsingContextId, Is.EqualTo("myContext"));
    }

    [Test]
    public void TestExecuteGetTreeCommand()
    {
        string responseJson = @"{ ""result"": { ""contexts"": [ { ""context"": ""myContext"", ""url"": ""https://example.com"", ""children"": [] } ] } }";
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        var task = module.GetTree(new GetTreeCommandSettings());
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ContextTree, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result.ContextTree[0].BrowsingContextId, Is.EqualTo("myContext"));
            Assert.That(result.ContextTree[0].Url, Is.EqualTo("https://example.com"));
            Assert.That(result.ContextTree[0].Children, Has.Count.EqualTo(0));
        });
    }

    [Test]
    public void TestExecuteHandleUserPromptCommand()
    {
        string responseJson = @"{ ""result"": {} }";
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        var task = module.HandleUserPrompt(new HandleUserPromptCommandSettings("myContextId"));
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void TestExecuteNavigateCommand()
    {
        string responseJson = @"{ ""result"": { ""navigation"": ""myNavigationId"", ""url"": ""https://example.com"" } }";
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        var task = module.Navigate(new NavigateCommandSettings("myContext", "https://example.com") { Wait = ReadinessState.Complete });
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.NavigationId, Is.EqualTo("myNavigationId"));
            Assert.That(result.Url, Is.EqualTo("https://example.com"));
        });
    }

    [Test]
    public void TestExecuteReloadCommand()
    {
        string responseJson = @"{ ""result"": { ""navigation"": ""myNavigationId"", ""url"": ""https://example.com"" } }";
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        var task = module.Reload(new ReloadCommandSettings("myContext"));
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.NavigationId, Is.EqualTo("myNavigationId"));
            Assert.That(result.Url, Is.EqualTo("https://example.com"));
        });
    }

    [Test]
    public void TestCanReceiveContextCreatedEvent()
    {
        string eventJson = @"{ ""method"": ""browsingContext.contextCreated"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""children"": [] } }";
        bool eventRaised = false;
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        module.ContextCreated += (object? obj, BrowsingContextEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.Children, Has.Count.EqualTo(0));
                Assert.That(e.Parent, Is.Null);
            });
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveContextDestroyedEvent()
    {
        string eventJson = @"{ ""method"": ""browsingContext.contextDestroyed"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""children"": [] } }";
        bool eventRaised = false;
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        module.ContextDestroyed += (object? obj, BrowsingContextEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.Children, Has.Count.EqualTo(0));
                Assert.That(e.Parent, Is.Null);
            });
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveDomContentLoadedEvent()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = @"{ ""method"": ""browsingContext.domContentLoaded"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""timestamp"": " + epochTimestamp +  @", ""navigation"": ""myNavigationId"" } }";
        bool eventRaised = false;
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        module.DomContentLoaded += (object? obj, NavigationEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.EpochTimestamp, Is.EqualTo(epochTimestamp));
                Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            });
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveDownloadWillBeginEvent()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = @"{ ""method"": ""browsingContext.downloadWillBegin"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""timestamp"": " + epochTimestamp +  @", ""navigation"": ""myNavigationId"" } }";
        bool eventRaised = false;
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        module.DownloadWillBegin += (object? obj, NavigationEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.EpochTimestamp, Is.EqualTo(epochTimestamp));
                Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            });
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveFragmentNavigatedEvent()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = @"{ ""method"": ""browsingContext.fragmentNavigated"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""timestamp"": " + epochTimestamp +  @", ""navigation"": ""myNavigationId"" } }";
        bool eventRaised = false;
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        module.FragmentNavigated += (object? obj, NavigationEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.EpochTimestamp, Is.EqualTo(epochTimestamp));
                Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            });
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveLoadEvent()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = @"{ ""method"": ""browsingContext.load"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""timestamp"": " + epochTimestamp +  @", ""navigation"": ""myNavigationId"" } }";
        bool eventRaised = false;
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        module.Load += (object? obj, NavigationEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.EpochTimestamp, Is.EqualTo(epochTimestamp));
                Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            });
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveNavigationAbortedEvent()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = @"{ ""method"": ""browsingContext.navigationAborted"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""timestamp"": " + epochTimestamp +  @", ""navigation"": ""myNavigationId"" } }";
        bool eventRaised = false;
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        module.NavigationAborted += (object? obj, NavigationEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.EpochTimestamp, Is.EqualTo(epochTimestamp));
                Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            });
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveNavigationFailedEvent()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = @"{ ""method"": ""browsingContext.navigationFailed"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""timestamp"": " + epochTimestamp +  @", ""navigation"": ""myNavigationId"" } }";
        bool eventRaised = false;
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        module.NavigationFailed += (object? obj, NavigationEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.EpochTimestamp, Is.EqualTo(epochTimestamp));
                Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            });
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveNavigationStartedEvent()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = @"{ ""method"": ""browsingContext.navigationStarted"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""timestamp"": " + epochTimestamp +  @", ""navigation"": ""myNavigationId"" } }";
        bool eventRaised = false;
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        module.NavigationStarted += (object? obj, NavigationEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.Url, Is.EqualTo("https://example.com"));
                Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
                Assert.That(e.EpochTimestamp, Is.EqualTo(epochTimestamp));
                Assert.That(e.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            });
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveUserPromptClosedEvent()
    {
        string eventJson = @"{ ""method"": ""browsingContext.userPromptClosed"", ""params"": { ""context"": ""myContext"", ""accepted"": true, ""userText"": ""my prompt text"" } }";
        bool eventRaised = false;
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        module.UserPromptClosed += (object? obj, UserPromptClosedEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.IsAccepted, Is.EqualTo(true));
                Assert.That(e.UserText, Is.EqualTo("my prompt text"));
            });
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveUserPromptOpenedEvent()
    {
        string eventJson = @"{ ""method"": ""browsingContext.userPromptOpened"", ""params"": { ""context"": ""myContext"", ""type"": ""confirm"", ""message"": ""my message text"" } }";
        bool eventRaised = false;
        TestDriver driver = new();
        BrowsingContextModule module = new(driver);
        module.UserPromptOpened += (object? obj, UserPromptOpenedEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.PromptType, Is.EqualTo(UserPromptType.Confirm));
                Assert.That(e.Message, Is.EqualTo("my message text"));
            });
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }
}