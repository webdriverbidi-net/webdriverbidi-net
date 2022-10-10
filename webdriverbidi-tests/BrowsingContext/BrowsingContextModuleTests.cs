namespace WebDriverBidi.BrowsingContext;

using TestUtilities;

[TestFixture]
public class BrowsingContextModuleTests
{
    [Test]
    public void TestExecuteCaptureScreenshotCommand()
    {
        string responseJson = @"{ ""result"": { ""data"": ""encodedScreenshotData"" } }";
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        var task = module.CaptureScreenshot(new CaptureScreenshotCommandProperties("myContextId"));
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
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        var task = module.Close(new CloseCommandProperties("myContextId"));
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
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        var task = module.Create(new CreateCommandProperties(BrowsingContextCreateType.Tab));
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
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        var task = module.GetTree(new GetTreeCommandProperties());
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ContextTree.Count, Is.EqualTo(1));
        Assert.That(result.ContextTree[0].BrowsingContextId, Is.EqualTo("myContext"));
        Assert.That(result.ContextTree[0].Url, Is.EqualTo("https://example.com"));
        Assert.That(result.ContextTree[0].Children.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestExecuteHandleUserPromptCommand()
    {
        string responseJson = @"{ ""result"": {} }";
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        var task = module.HandleUserPrompt(new HandleUserPromptCommandProperties("myContextId"));
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
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        var task = module.Navigate(new NavigateCommandProperties("myContext", "https://example.com") { Wait = ReadinessState.Complete });
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.NavigationId, Is.EqualTo("myNavigationId"));
        Assert.That(result.Url, Is.EqualTo("https://example.com"));
    }

    [Test]
    public void TestExecuteReloadCommand()
    {
        string responseJson = @"{ ""result"": { ""navigation"": ""myNavigationId"", ""url"": ""https://example.com"" } }";
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        var task = module.Reload(new ReloadCommandProperties("myContext"));
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.NavigationId, Is.EqualTo("myNavigationId"));
        Assert.That(result.Url, Is.EqualTo("https://example.com"));
    }

    [Test]
    public void TestCanReceiveContextCreatedEvent()
    {
        string eventJson = @"{ ""method"": ""browsingContext.contextCreated"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""children"": [] } }";
        bool eventRaised = false;
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        module.ContextCreated += (object? obj, BrowsingContextEventArgs e) =>
        {
            eventRaised = true;
            Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
            Assert.That(e.Url, Is.EqualTo("https://example.com"));
            Assert.That(e.Children.Count, Is.EqualTo(0));
            Assert.That(e.Parent, Is.Null);
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveContextDestroyedEvent()
    {
        string eventJson = @"{ ""method"": ""browsingContext.contextDestroyed"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""children"": [] } }";
        bool eventRaised = false;
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        module.ContextDestroyed += (object? obj, BrowsingContextEventArgs e) =>
        {
            eventRaised = true;
            Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
            Assert.That(e.Url, Is.EqualTo("https://example.com"));
            Assert.That(e.Children.Count, Is.EqualTo(0));
            Assert.That(e.Parent, Is.Null);
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveDomContentLoadedEvent()
    {
        string eventJson = @"{ ""method"": ""browsingContext.domContentLoaded"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""navigation"": ""myNavigationId"" } }";
        bool eventRaised = false;
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        module.DomContentLoaded += (object? obj, NavigationEventArgs e) =>
        {
            eventRaised = true;
            Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
            Assert.That(e.Url, Is.EqualTo("https://example.com"));
            Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveDownloadWillBeginEvent()
    {
        string eventJson = @"{ ""method"": ""browsingContext.downloadWillBegin"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""navigation"": ""myNavigationId"" } }";
        bool eventRaised = false;
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        module.DownloadWillBegin += (object? obj, NavigationEventArgs e) =>
        {
            eventRaised = true;
            Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
            Assert.That(e.Url, Is.EqualTo("https://example.com"));
            Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveFragmentNavigatedEvent()
    {
        string eventJson = @"{ ""method"": ""browsingContext.fragmentNavigated"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""navigation"": ""myNavigationId"" } }";
        bool eventRaised = false;
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        module.FragmentNavigated += (object? obj, NavigationEventArgs e) =>
        {
            eventRaised = true;
            Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
            Assert.That(e.Url, Is.EqualTo("https://example.com"));
            Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveLoadEvent()
    {
        string eventJson = @"{ ""method"": ""browsingContext.load"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""navigation"": ""myNavigationId"" } }";
        bool eventRaised = false;
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        module.Load += (object? obj, NavigationEventArgs e) =>
        {
            eventRaised = true;
            Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
            Assert.That(e.Url, Is.EqualTo("https://example.com"));
            Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveNavigationAbortedEvent()
    {
        string eventJson = @"{ ""method"": ""browsingContext.navigationAborted"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""navigation"": ""myNavigationId"" } }";
        bool eventRaised = false;
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        module.NavigationAborted += (object? obj, NavigationEventArgs e) =>
        {
            eventRaised = true;
            Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
            Assert.That(e.Url, Is.EqualTo("https://example.com"));
            Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveNavigationFailedEvent()
    {
        string eventJson = @"{ ""method"": ""browsingContext.navigationFailed"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""navigation"": ""myNavigationId"" } }";
        bool eventRaised = false;
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        module.NavigationFailed += (object? obj, NavigationEventArgs e) =>
        {
            eventRaised = true;
            Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
            Assert.That(e.Url, Is.EqualTo("https://example.com"));
            Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveNavigationStartedEvent()
    {
        string eventJson = @"{ ""method"": ""browsingContext.navigationStarted"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""navigation"": ""myNavigationId"" } }";
        bool eventRaised = false;
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        module.NavigationStarted += (object? obj, NavigationEventArgs e) =>
        {
            eventRaised = true;
            Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
            Assert.That(e.Url, Is.EqualTo("https://example.com"));
            Assert.That(e.NavigationId, Is.EqualTo("myNavigationId"));
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveUserPromptClosedEvent()
    {
        string eventJson = @"{ ""method"": ""browsingContext.userPromptClosed"", ""params"": { ""context"": ""myContext"", ""accepted"": true, ""userText"": ""my prompt text"" } }";
        bool eventRaised = false;
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        module.UserPromptClosed += (object? obj, UserPromptClosedEventArgs e) =>
        {
            eventRaised = true;
            Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
            Assert.That(e.IsAccepted, Is.EqualTo(true));
            Assert.That(e.UserText, Is.EqualTo("my prompt text"));
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveUserPromptOpenedEvent()
    {
        string eventJson = @"{ ""method"": ""browsingContext.userPromptOpened"", ""params"": { ""context"": ""myContext"", ""type"": ""confirm"", ""message"": ""my message text"" } }";
        bool eventRaised = false;
        TestDriver driver = new TestDriver();
        BrowsingContextModule module = new BrowsingContextModule(driver);
        module.UserPromptOpened += (object? obj, UserPromptOpenedEventArgs e) =>
        {
            eventRaised = true;
            Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
            Assert.That(e.PromptType, Is.EqualTo(UserPromptType.Confirm));
            Assert.That(e.Message, Is.EqualTo("my message text"));
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }
}