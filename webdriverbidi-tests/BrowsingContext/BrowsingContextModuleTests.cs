namespace WebDriverBidi.BrowsingContext;

using TestUtilities;
using WebDriverBidi.Protocol;

[TestFixture]
public class BrowsingContextModuleTests
{
    [Test]
    public void TestExecuteCaptureScreenshotCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": { ""data"": ""encodedScreenshotData"" } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        var task = module.CaptureScreenshot(new CaptureScreenshotCommandParameters("myContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void TestExecuteCloseCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": {} }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        var task = module.Close(new CloseCommandParameters("myContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;
        
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void TestExecuteCreateCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": { ""context"": ""myContext"" } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        var task = module.Create(new CreateCommandParameters(CreateType.Tab));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.BrowsingContextId, Is.EqualTo("myContext"));
    }

    [Test]
    public void TestExecuteGetTreeCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": { ""contexts"": [ { ""context"": ""myContext"", ""url"": ""https://example.com"", ""children"": [] } ] } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        var task = module.GetTree(new GetTreeCommandParameters());
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
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": {} }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        var task = module.HandleUserPrompt(new HandleUserPromptCommandParameters("myContextId"));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void TestExecuteNavigateCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": { ""navigation"": ""myNavigationId"", ""url"": ""https://example.com"" } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        var task = module.Navigate(new NavigateCommandParameters("myContext", "https://example.com") { Wait = ReadinessState.Complete });
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
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": { ""navigation"": ""myNavigationId"", ""url"": ""https://example.com"" } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        var task = module.Reload(new ReloadCommandParameters("myContext"));
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
        TestConnection connection = new();
        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        bool eventRaised = false;
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

        string eventJson = @"{ ""method"": ""browsingContext.contextCreated"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""children"": [] } }";
        connection.RaiseDataReceivedEvent(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveContextDestroyedEvent()
    {
        TestConnection connection = new();
        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        bool eventRaised = false;
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

        string eventJson = @"{ ""method"": ""browsingContext.contextDestroyed"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""children"": [] } }";
        connection.RaiseDataReceivedEvent(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveDomContentLoadedEvent()
    {
        TestConnection connection = new();
        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        bool eventRaised = false;
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

        string eventJson = @"{ ""method"": ""browsingContext.domContentLoaded"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""timestamp"": " + epochTimestamp +  @", ""navigation"": ""myNavigationId"" } }";
        connection.RaiseDataReceivedEvent(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveDownloadWillBeginEvent()
    {
        TestConnection connection = new();
        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        bool eventRaised = false;
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

        string eventJson = @"{ ""method"": ""browsingContext.downloadWillBegin"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""timestamp"": " + epochTimestamp +  @", ""navigation"": ""myNavigationId"" } }";
        connection.RaiseDataReceivedEvent(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveFragmentNavigatedEvent()
    {
        TestConnection connection = new();
        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        bool eventRaised = false;
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

        string eventJson = @"{ ""method"": ""browsingContext.fragmentNavigated"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""timestamp"": " + epochTimestamp +  @", ""navigation"": ""myNavigationId"" } }";
        connection.RaiseDataReceivedEvent(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveLoadEvent()
    {
        TestConnection connection = new();
        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        bool eventRaised = false;
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

        string eventJson = @"{ ""method"": ""browsingContext.load"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""timestamp"": " + epochTimestamp +  @", ""navigation"": ""myNavigationId"" } }";
        connection.RaiseDataReceivedEvent(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveNavigationAbortedEvent()
    {
        TestConnection connection = new();
        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        bool eventRaised = false;
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

        string eventJson = @"{ ""method"": ""browsingContext.navigationAborted"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""timestamp"": " + epochTimestamp +  @", ""navigation"": ""myNavigationId"" } }";
        connection.RaiseDataReceivedEvent(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveNavigationFailedEvent()
    {
        TestConnection connection = new();
        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        bool eventRaised = false;
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

        string eventJson = @"{ ""method"": ""browsingContext.navigationFailed"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""timestamp"": " + epochTimestamp +  @", ""navigation"": ""myNavigationId"" } }";
        connection.RaiseDataReceivedEvent(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveNavigationStartedEvent()
    {
        TestConnection connection = new();
        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        bool eventRaised = false;
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

        string eventJson = @"{ ""method"": ""browsingContext.navigationStarted"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""timestamp"": " + epochTimestamp +  @", ""navigation"": ""myNavigationId"" } }";
        connection.RaiseDataReceivedEvent(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveUserPromptClosedEvent()
    {
        TestConnection connection = new();
        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        bool eventRaised = false;
        module.UserPromptClosed += (object? obj, UserPromptClosedEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.IsAccepted, Is.EqualTo(true));
                Assert.That(e.UserText, Is.EqualTo("my prompt text"));
            });
        };

        string eventJson = @"{ ""method"": ""browsingContext.userPromptClosed"", ""params"": { ""context"": ""myContext"", ""accepted"": true, ""userText"": ""my prompt text"" } }";
        connection.RaiseDataReceivedEvent(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveUserPromptOpenedEvent()
    {
        TestConnection connection = new();
        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        BrowsingContextModule module = new(driver);

        bool eventRaised = false;
        module.UserPromptOpened += (object? obj, UserPromptOpenedEventArgs e) => {
            eventRaised = true;
            Assert.Multiple(() =>
            {
                Assert.That(e.BrowsingContextId, Is.EqualTo("myContext"));
                Assert.That(e.PromptType, Is.EqualTo(UserPromptType.Confirm));
                Assert.That(e.Message, Is.EqualTo("my message text"));
            });
        };

        string eventJson = @"{ ""method"": ""browsingContext.userPromptOpened"", ""params"": { ""context"": ""myContext"", ""type"": ""confirm"", ""message"": ""my message text"" } }";
        connection.RaiseDataReceivedEvent(eventJson);
        Assert.That(eventRaised, Is.True);
    }
}