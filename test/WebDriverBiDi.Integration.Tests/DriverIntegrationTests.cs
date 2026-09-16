namespace WebDriverBiDi.Integration.Tests;

using System.Diagnostics;
using System.Net;
using PinchHitter;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client.Inputs;
using WebDriverBiDi.Client.Launchers;
using WebDriverBiDi.Emulation;
using WebDriverBiDi.Input;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.Storage;

public class DriverIntegrationTests
{
    private static readonly TimeSpan IntegrationTestTimeout = TimeSpan.FromSeconds(60);
    private readonly List<string> driverLog = [];
    private BrowserLauncher? browserLauncher;

    [Theory]
    [InlineData(TestBrowser.Firefox)]
    [InlineData(TestBrowser.Chrome)]
    public async Task TestCanNavigate(TestBrowser browser)
    {
        // Ensure the browser is available before running each test
        // In CI: skips test if browser executable not configured
        // Locally: allows test to run with system-installed browser
        BrowserTestHelper.EnsureBrowserAvailable(browser);

        await using BrowserLauncher launcher = await this.CreateBrowserLauncher(browser);
        await using Server server = await this.CreateTestServer();
        await using BiDiDriver driver = await this.StartBiDiDriverSession(launcher);

        string navigatedUrl = string.Empty;
        await using EventObserver<NavigationEventArgs> navigationObserver = driver.BrowsingContext.OnLoad.AddObserver(e => navigatedUrl = e.Url);
        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(driver.BrowsingContext.OnLoad.EventName), cancellationToken: TestContext.Current.CancellationToken);

        string browsingContextId = await this.GetBrowsingContext(driver);

        navigationObserver.StartCapturingTasks();
        NavigateCommandParameters navigateParams = new(browsingContextId, $"http://localhost:{server.Port}/index.html")
        {
            Wait = ReadinessState.Complete
        };
        await driver.BrowsingContext.NavigateAsync(navigateParams, cancellationToken: TestContext.Current.CancellationToken);

        Task[] capturedTasks = await navigationObserver.WaitForCapturedTasksAsync(1, IntegrationTestTimeout, TestContext.Current.CancellationToken);
        await Assert.Single(capturedTasks);
        Assert.Equal($"http://localhost:{server.Port}/index.html", navigatedUrl);

        // Attempt to gracefully close the browser. If the test fails, the
        // browser process will be cleaned up when the launcher is disposed.
        await driver.Browser.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(TestBrowser.Firefox)]
    [InlineData(TestBrowser.Chrome)]
    public async Task TestCanExecuteScript(TestBrowser browser)
    {
        // Ensure the browser is available before running each test
        // In CI: skips test if browser executable not configured
        // Locally: allows test to run with system-installed browser
        BrowserTestHelper.EnsureBrowserAvailable(browser);

        await using BrowserLauncher launcher = await this.CreateBrowserLauncher(browser);
        await using Server server = await this.CreateTestServer();
        await using BiDiDriver driver = await this.StartBiDiDriverSession(launcher);

        string browsingContextId = await this.GetBrowsingContext(driver);
        NavigateCommandParameters navigateParams = new(browsingContextId, $"http://localhost:{server.Port}/index.html")
        {
            Wait = ReadinessState.Complete
        };
        await driver.BrowsingContext.NavigateAsync(navigateParams, cancellationToken: TestContext.Current.CancellationToken);

        string functionDefinition = "(first, second) => first + second";
        List<LocalValue> arguments =
        [
            LocalValue.Number(3),
            LocalValue.Number(5),
        ];

        CallFunctionCommandParameters callFunctionParams = new(functionDefinition, new ContextTarget(browsingContextId), true);
        callFunctionParams.Arguments.AddRange(arguments);

        EvaluateResult scriptResult = await driver.Script.CallFunctionAsync(callFunctionParams, cancellationToken: TestContext.Current.CancellationToken);
        Assert.IsType<EvaluateResultSuccess>(scriptResult);
        EvaluateResultSuccess successResult = (EvaluateResultSuccess)scriptResult;
        Assert.IsType<NumberRemoteValue>(successResult.Result);
        NumberRemoteValue resultValue = (NumberRemoteValue)successResult.Result;
        Assert.Equal(8, resultValue.ToInt());

        // Attempt to gracefully close the browser. If the test fails, the
        // browser process will be cleaned up when the launcher is disposed.
        await driver.Browser.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(TestBrowser.Firefox)]
    [InlineData(TestBrowser.Chrome)]
    public async Task TestCanClickLink(TestBrowser browser)
    {
        // Ensure the browser is available before running each test
        // In CI: skips test if browser executable not configured
        // Locally: allows test to run with system-installed browser
        BrowserTestHelper.EnsureBrowserAvailable(browser);

        await using BrowserLauncher launcher = await this.CreateBrowserLauncher(browser);
        await using Server server = await this.CreateTestServer();
        await using BiDiDriver driver = await this.StartBiDiDriverSession(launcher);

        string browsingContextId = await this.GetBrowsingContext(driver);
        NavigateCommandParameters navigateParams = new(browsingContextId, $"http://localhost:{server.Port}/index.html")
        {
            Wait = ReadinessState.Complete
        };
        await driver.BrowsingContext.NavigateAsync(navigateParams, cancellationToken: TestContext.Current.CancellationToken);
        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(driver.BrowsingContext.OnLoad.EventName), cancellationToken: TestContext.Current.CancellationToken);

        string navigatedUrl = string.Empty;
        await using EventObserver<NavigationEventArgs> navigationObserver = driver.BrowsingContext.OnLoad.AddObserver(e => navigatedUrl = e.Url);

        LocateNodesCommandParameters locateNodesParams = new(browsingContextId, new CssLocator("a"));
        LocateNodesCommandResult locateResult = await driver.BrowsingContext.LocateNodesAsync(locateNodesParams, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Single(locateResult.Nodes);

        NodeRemoteValue nodeRemoteValue = locateResult.Nodes[0];
        SharedReference elementReference = nodeRemoteValue.ToSharedReference();

        navigationObserver.StartCapturingTasks();
        InputBuilder inputBuilder = new();
        inputBuilder.AddClickOnElementAction(elementReference);
        PerformActionsCommandParameters actionsParams = new(browsingContextId);
        actionsParams.Actions.AddRange(inputBuilder.Build());
        await driver.Input.PerformActionsAsync(actionsParams, cancellationToken: TestContext.Current.CancellationToken);

        Task[] capturedTasks = await navigationObserver.WaitForCapturedTasksAsync(1, IntegrationTestTimeout, TestContext.Current.CancellationToken);
        await Assert.Single(capturedTasks);
        Assert.Equal($"http://localhost:{server.Port}/details.html", navigatedUrl);

        // Attempt to gracefully close the browser. If the test fails, the
        // browser process will be cleaned up when the launcher is disposed.
        await driver.Browser.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(TestBrowser.Firefox)]
    [InlineData(TestBrowser.Chrome)]
    public async Task TestCanSubmitForm(TestBrowser browser)
    {
        // Ensure the browser is available before running each test
        // In CI: skips test if browser executable not configured
        // Locally: allows test to run with system-installed browser
        BrowserTestHelper.EnsureBrowserAvailable(browser);

        await using BrowserLauncher launcher = await this.CreateBrowserLauncher(browser);
        await using Server server = await this.CreateTestServer();
        await using BiDiDriver driver = await this.StartBiDiDriverSession(launcher);

        string browsingContextId = await this.GetBrowsingContext(driver);
        NavigateCommandParameters navigateParams = new(browsingContextId, $"http://localhost:{server.Port}/formInput.html")
        {
            Wait = ReadinessState.Complete
        };
        await driver.BrowsingContext.NavigateAsync(navigateParams, cancellationToken: TestContext.Current.CancellationToken);
        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(driver.BrowsingContext.OnLoad.EventName), cancellationToken: TestContext.Current.CancellationToken);

        string navigatedUrl = string.Empty;
        await using EventObserver<NavigationEventArgs> navigationObserver = driver.BrowsingContext.OnLoad.AddObserver(e => navigatedUrl = e.Url);

        LocateNodesCommandParameters locateNodesParams = new(browsingContextId, new CssLocator("input#dataToSend"));
        LocateNodesCommandResult locateResult = await driver.BrowsingContext.LocateNodesAsync(locateNodesParams, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Single(locateResult.Nodes);

        NodeRemoteValue nodeRemoteValue = locateResult.Nodes[0];
        SharedReference elementReference = nodeRemoteValue.ToSharedReference();

        navigationObserver.StartCapturingTasks();
        InputBuilder inputBuilder = new();
        inputBuilder.AddClickOnElementAction(elementReference);
        inputBuilder.AddSendKeysToActiveElementAction("Hello WebDriver BiDi" + Keys.Enter);
        PerformActionsCommandParameters actionsParams = new(browsingContextId);
        actionsParams.Actions.AddRange(inputBuilder.Build());
        await driver.Input.PerformActionsAsync(actionsParams, cancellationToken: TestContext.Current.CancellationToken);

        Task[] capturedTasks = await navigationObserver.WaitForCapturedTasksAsync(1, IntegrationTestTimeout, TestContext.Current.CancellationToken);
        await Assert.Single(capturedTasks);
        Assert.Equal($"http://localhost:{server.Port}/processForm", navigatedUrl);

        LocateNodesCommandParameters locateFormResultParams = new(browsingContextId, new CssLocator("span"))
        {
            SerializationOptions = new()
            {
                MaxDomDepth = SerializationOptions.InfiniteMaxDomDepth,
            },
        };
        LocateNodesCommandResult locateFormResult = await driver.BrowsingContext.LocateNodesAsync(locateFormResultParams, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Single(locateFormResult.Nodes);

        NodeRemoteValue resultNodeRemoteValue = locateFormResult.Nodes[0];
        NodeProperties resultNodeProperties = resultNodeRemoteValue.GetNodeProperties();
        Assert.NotNull(resultNodeProperties.Children);
        Assert.Single(resultNodeProperties.Children);
        NodeRemoteValue textContentValue = resultNodeProperties.Children[0];
        Assert.Equal("Hello WebDriver BiDi", textContentValue.GetNodeProperties().NodeValue);

        // Attempt to gracefully close the browser. If the test fails, the
        // browser process will be cleaned up when the launcher is disposed.
        await driver.Browser.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(TestBrowser.Firefox)]
    [InlineData(TestBrowser.Chrome)]
    public async Task TestCanReceiveLogEntries(TestBrowser browser)
    {
        BrowserTestHelper.EnsureBrowserAvailable(browser);

        await using BrowserLauncher launcher = await this.CreateBrowserLauncher(browser);
        await using Server server = await this.CreateTestServer();
        await using BiDiDriver driver = await this.StartBiDiDriverSession(launcher);

        string browsingContextId = await this.GetBrowsingContext(driver);
        await this.NavigateAsync(driver, browsingContextId, $"http://localhost:{server.Port}/index.html");

        EntryAddedEventArgs? capturedEntry = null;
        await using EventObserver<EntryAddedEventArgs> logObserver = driver.Log.OnEntryAdded.AddObserver(e => capturedEntry = e);
        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName), cancellationToken: TestContext.Current.CancellationToken);

        logObserver.StartCapturingTasks();
        EvaluateCommandParameters consoleParams = new("console.log('integration log entry')", new ContextTarget(browsingContextId), true);
        await driver.Script.EvaluateAsync(consoleParams, cancellationToken: TestContext.Current.CancellationToken);

        Task[] capturedTasks = await logObserver.WaitForCapturedTasksAsync(1, IntegrationTestTimeout, TestContext.Current.CancellationToken);
        await Assert.Single(capturedTasks);
        Assert.NotNull(capturedEntry);
        Assert.Equal("console", capturedEntry.Type);
        Assert.Equal("log", capturedEntry.Method);
        Assert.Equal(LogLevel.Info, capturedEntry.Level);
        Assert.Equal("integration log entry", capturedEntry.Text);

        // Attempt to gracefully close the browser. If the test fails, the
        // browser process will be cleaned up when the launcher is disposed.
        await driver.Browser.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(TestBrowser.Firefox)]
    [InlineData(TestBrowser.Chrome)]
    public async Task TestCanInterceptAndContinueRequest(TestBrowser browser)
    {
        BrowserTestHelper.EnsureBrowserAvailable(browser);

        await using BrowserLauncher launcher = await this.CreateBrowserLauncher(browser);
        await using Server server = await this.CreateTestServer();
        await using BiDiDriver driver = await this.StartBiDiDriverSession(launcher);

        string browsingContextId = await this.GetBrowsingContext(driver);
        await this.NavigateAsync(driver, browsingContextId, $"http://localhost:{server.Port}/index.html");

        // The handler only records the blocked request. Continuing it is a command, and a command sent from a
        // synchronous handler would wait on the very dispatch that is running the handler.
        string detailsUrl = $"http://localhost:{server.Port}/details.html";
        TaskCompletionSource<BeforeRequestSentEventArgs> blockedRequest = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await using EventObserver<BeforeRequestSentEventArgs> requestObserver = driver.Network.OnBeforeRequestSent.AddObserver(e =>
        {
            if (e.IsBlocked && e.Request.Url == detailsUrl)
            {
                blockedRequest.TrySetResult(e);
            }
        });
        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(driver.Network.OnBeforeRequestSent.EventName), cancellationToken: TestContext.Current.CancellationToken);

        AddInterceptCommandParameters interceptParams = new(InterceptPhase.BeforeRequestSent);
        interceptParams.Contexts.Add(browsingContextId);
        interceptParams.UrlPatterns.Add(new UrlPatternString(detailsUrl));
        AddInterceptCommandResult intercept = await driver.Network.AddInterceptAsync(interceptParams, cancellationToken: TestContext.Current.CancellationToken);

        // The navigation cannot finish while its request is held by the intercept, so its completing after the
        // continue is what proves the continue reached the browser.
        NavigateCommandParameters navigateParams = new(browsingContextId, detailsUrl)
        {
            Wait = ReadinessState.Complete
        };
        Task<NavigateCommandResult> navigation = driver.BrowsingContext.NavigateAsync(navigateParams, cancellationToken: TestContext.Current.CancellationToken);

        BeforeRequestSentEventArgs blocked = await blockedRequest.Task.WaitAsync(IntegrationTestTimeout, TestContext.Current.CancellationToken);
        Assert.False(navigation.IsCompleted);
        Assert.NotNull(blocked.Intercepts);
        Assert.Contains(intercept.InterceptId, blocked.Intercepts);

        await driver.Network.ContinueRequestAsync(new ContinueRequestCommandParameters(blocked.Request.RequestId), cancellationToken: TestContext.Current.CancellationToken);
        NavigateCommandResult navigationResult = await navigation.WaitAsync(IntegrationTestTimeout, TestContext.Current.CancellationToken);
        Assert.Equal(detailsUrl, navigationResult.Url);

        // Attempt to gracefully close the browser. If the test fails, the
        // browser process will be cleaned up when the launcher is disposed.
        await driver.Browser.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(TestBrowser.Firefox)]
    [InlineData(TestBrowser.Chrome)]
    public async Task TestCanSetAndGetCookies(TestBrowser browser)
    {
        BrowserTestHelper.EnsureBrowserAvailable(browser);

        await using BrowserLauncher launcher = await this.CreateBrowserLauncher(browser);
        await using Server server = await this.CreateTestServer();
        await using BiDiDriver driver = await this.StartBiDiDriverSession(launcher);

        string browsingContextId = await this.GetBrowsingContext(driver);
        await this.NavigateAsync(driver, browsingContextId, $"http://localhost:{server.Port}/index.html");

        PartialCookie cookie = new("integrationCookie", BytesValue.FromString("cookieValue"), "localhost")
        {
            Path = "/",
        };
        SetCookieCommandParameters setCookieParams = new(cookie)
        {
            Partition = new BrowsingContextPartitionDescriptor(browsingContextId),
        };
        await driver.Storage.SetCookieAsync(setCookieParams, cancellationToken: TestContext.Current.CancellationToken);

        GetCookiesCommandParameters getCookiesParams = new()
        {
            Filter = new CookieFilter { Name = "integrationCookie" },
            Partition = new BrowsingContextPartitionDescriptor(browsingContextId),
        };
        GetCookiesCommandResult cookies = await driver.Storage.GetCookiesAsync(getCookiesParams, cancellationToken: TestContext.Current.CancellationToken);
        WebDriverBiDi.Network.Cookie storedCookie = Assert.Single(cookies.Cookies);
        Assert.Equal("integrationCookie", storedCookie.Name);
        Assert.Equal("cookieValue", storedCookie.Value.Value);

        // The page sees the cookie too, so it was set in the partition the page uses rather than merely recorded.
        EvaluateCommandParameters readCookieParams = new("document.cookie", new ContextTarget(browsingContextId), true);
        EvaluateResultSuccess readCookieResult = Assert.IsType<EvaluateResultSuccess>(await driver.Script.EvaluateAsync(readCookieParams, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("integrationCookie=cookieValue", readCookieResult.Result.As<StringRemoteValue>().Value);

        // Attempt to gracefully close the browser. If the test fails, the
        // browser process will be cleaned up when the launcher is disposed.
        await driver.Browser.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(TestBrowser.Firefox)]
    [InlineData(TestBrowser.Chrome)]
    public async Task TestCanOverrideAndResetUserAgent(TestBrowser browser)
    {
        BrowserTestHelper.EnsureBrowserAvailable(browser);

        await using BrowserLauncher launcher = await this.CreateBrowserLauncher(browser);
        await using Server server = await this.CreateTestServer();
        await using BiDiDriver driver = await this.StartBiDiDriverSession(launcher);

        string browsingContextId = await this.GetBrowsingContext(driver);
        string pageUrl = $"http://localhost:{server.Port}/index.html";
        const string OverrideUserAgent = "WebDriverBiDi.NET integration test user agent";

        SetUserAgentOverrideCommandParameters overrideParams = new()
        {
            UserAgent = OverrideUserAgent,
        };
        overrideParams.Contexts.Add(browsingContextId);
        await driver.Emulation.SetUserAgentOverrideAsync(overrideParams, cancellationToken: TestContext.Current.CancellationToken);

        await this.NavigateAsync(driver, browsingContextId, pageUrl);
        Assert.Equal(OverrideUserAgent, await this.GetUserAgentAsync(driver, browsingContextId));

        SetUserAgentOverrideCommandParameters resetParams = SetUserAgentOverrideCommandParameters.ResetUserAgentOverride;
        resetParams.Contexts.Add(browsingContextId);
        await driver.Emulation.SetUserAgentOverrideAsync(resetParams, cancellationToken: TestContext.Current.CancellationToken);

        await this.NavigateAsync(driver, browsingContextId, pageUrl);
        Assert.NotEqual(OverrideUserAgent, await this.GetUserAgentAsync(driver, browsingContextId));

        // Attempt to gracefully close the browser. If the test fails, the
        // browser process will be cleaned up when the launcher is disposed.
        await driver.Browser.CloseAsync(cancellationToken: TestContext.Current.CancellationToken);
    }

    private async Task NavigateAsync(BiDiDriver driver, string browsingContextId, string url)
    {
        NavigateCommandParameters navigateParams = new(browsingContextId, url)
        {
            Wait = ReadinessState.Complete
        };
        await driver.BrowsingContext.NavigateAsync(navigateParams, cancellationToken: TestContext.Current.CancellationToken);
    }

    private async Task<string?> GetUserAgentAsync(BiDiDriver driver, string browsingContextId)
    {
        EvaluateCommandParameters userAgentParams = new("navigator.userAgent", new ContextTarget(browsingContextId), true);
        EvaluateResult result = await driver.Script.EvaluateAsync(userAgentParams, cancellationToken: TestContext.Current.CancellationToken);
        return Assert.IsType<EvaluateResultSuccess>(result).Result.As<StringRemoteValue>().Value;
    }

    private async Task<string> GetBrowsingContext(BiDiDriver driver)
    {
        GetTreeCommandResult tree;
        try
        {
            tree = await driver.BrowsingContext.GetTreeAsync(cancellationToken: TestContext.Current.CancellationToken);
        }
        catch (WebDriverBiDiTimeoutException ex)
        {
            // This command has timed out intermittently in CI with nothing but the timeout to go on, which
            // cannot distinguish a browser that crashed or hung from a response that arrived late or was
            // lost. A follow-up command shows whether the browser still answers, and the driver log shows
            // whether the command was sent and whether any response to it arrived, and when.
            string statusProbe;
            try
            {
                await driver.Session.StatusAsync(timeoutOverride: IntegrationTestTimeout, cancellationToken: TestContext.Current.CancellationToken);
                statusProbe = "responded";
            }
            catch (Exception probeException)
            {
                statusProbe = $"{probeException.GetType().Name}: {probeException.Message}";
            }

            string log;
            lock (this.driverLog)
            {
                log = this.driverLog.Count == 0 ? "(none)" : string.Join(" | ", this.driverLog);
            }

            Assert.Fail($"{ex.Message}. Browser process running: {this.browserLauncher?.IsRunning}. Subsequent session.status: {statusProbe}. Driver log: {log}");
            throw;
        }

        return tree.ContextTree[0].BrowsingContextId;
    }

    private async Task<BiDiDriver> StartBiDiDriverSession(BrowserLauncher launcher)
    {
        this.browserLauncher = launcher;
        Transport transport = launcher.CreateTransport();
        transport.LogLevel = WebDriverBiDiLogLevel.Debug;
        Stopwatch sessionStopwatch = Stopwatch.StartNew();
        BiDiDriver driver = new(IntegrationTestTimeout, transport);
        driver.OnLogMessage.AddObserver(e =>
        {
            lock (this.driverLog)
            {
                this.driverLog.Add($"+{sessionStopwatch.ElapsedMilliseconds}ms [{e.Level}] {e.Message}");
            }

            return Task.CompletedTask;
        });
        await driver.StartAsync(launcher.ConnectionString, TestContext.Current.CancellationToken);

        if (!launcher.IsBiDiSessionInitialized)
        {
            // Using a classic WebDriver browser driver to launch the browser
            // automatically gives you a WebDriver BiDi session. Without the
            // driver executable, you must start your own session.
            await driver.Session.NewSessionAsync(new NewCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        }

        return driver;
    }

    private async Task<BrowserLauncher> CreateBrowserLauncher(TestBrowser browser)
    {
        BrowserLauncher launcher = BrowserTestHelper.GetBrowserLauncher(browser);
        launcher.IsBrowserHeadless = true;
        launcher.InitializationTimeout = IntegrationTestTimeout;
        await launcher.StartAsync();
        await launcher.LaunchBrowserAsync();
        return launcher;
    }

    private async Task<Server> CreateTestServer()
    {
        string indexPage = """
            <h1>Welcome to the WebDriverBiDi.NET project</h1>
            <p>You can browse using localhost.</p>
            <div>
              <a href="/details.html">Click here to go to the details page</a>
            </div>
            """;

        string formInputPage = """
            <h1>Form input demo</h1>
                <form action="./processForm" method="POST" enctype="text/plain">
                <div class="form-example">
                    <label for="dataToSend">Enter data to send: </label>
                    <input type="text" name="dataToSend" id="dataToSend" required />
                </div>
                <div class="form-example">
                    <input type="submit" value="Send!" />
                </div>
                </form>
            """;

        string detailsPage = """
            <h1>Details Page</h1>
            <div id="details">This is the details page.</div>
            """;

        Server server = new();
        server.RegisterHandler("/index.html", new WebResourceRequestHandler(WebContent.AsHtmlDocument(indexPage, "<title>WebDriverBiDi.NET Testing</title>")));
        server.RegisterHandler("/details.html", new WebResourceRequestHandler(WebContent.AsHtmlDocument(detailsPage, "<title>Details - WebDriverBiDi.NET Testing</title>")));
        server.RegisterHandler("/formInput.html", new WebResourceRequestHandler(WebContent.AsHtmlDocument(formInputPage, "<title>Form Input - WebDriverBiDi.NET Testing</title>")));
        server.RegisterHandler("/processForm", HttpRequestMethod.Post, new FormSubmitRequestHandler());
        await server.StartAsync();
        return server;
    }

    private class FormSubmitRequestHandler : WebResourceRequestHandler
    {
        private static readonly string requestSubmitTemplate = """
            <h1>Data sent!</h1>
            <div>
              Data submitted via the form: <span>{0}</span>
            </div>
            """;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormSubmitRequestHandler"/> class.
        /// </summary>
        public FormSubmitRequestHandler()
            : base(WebContent.AsHtmlDocument(requestSubmitTemplate))
        {
        }

        /// <summary>
        /// Processes an HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request to handle.</param>
        /// <param name="additionalData">Additional data passed into the method for handling requests.</param>
        /// <returns>The response to the HTTP request.</returns>
        protected override Task<HttpResponse> ProcessRequestAsync(HttpRequest request)
        {
            Dictionary<string, string> formData = this.ParseRequestBody(request.Body);
            HttpResponse response = base.CreateHttpResponse(request.Id, HttpStatusCode.OK);
            response.TextBodyContent = string.Format(response.TextBodyContent, formData["dataToSend"]);
            response.Headers["Content-Length"][0] = response.BodyContentBytes.Length.ToString();
            return Task.FromResult<HttpResponse>(response);
        }

        private Dictionary<string, string> ParseRequestBody(string requestBody)
        {
            Dictionary<string, string> formData = [];
            string[] lines = requestBody.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] entry = line.Split('=', 2);
                formData[entry[0]] = entry[1];
            }

            return formData;
        }
    }
}
