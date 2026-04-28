namespace WebDriverBiDi.Integration;

using System.Net;
using PinchHitter;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client.Inputs;
using WebDriverBiDi.Client.Launchers;
using WebDriverBiDi.Input;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

[TestFixture]
public class DriverIntegrationTests
{
    [TestCase(TestBrowser.Firefox)]
    [TestCase(TestBrowser.Chrome)]
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
        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(driver.BrowsingContext.OnLoad.EventName));

        string browsingContextId = await this.GetBrowsingContext(driver);

        navigationObserver.StartCapturingTasks();
        NavigateCommandParameters navigateParams = new(browsingContextId, $"http://localhost:{server.Port}/index.html")
        {
            Wait = ReadinessState.Complete
        };
        await driver.BrowsingContext.NavigateAsync(navigateParams);

        Task[] capturedTasks = await navigationObserver.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5));
        Assert.That(capturedTasks, Has.Length.EqualTo(1));
        Assert.That(navigatedUrl, Is.EqualTo($"http://localhost:{server.Port}/index.html"));

        // Attempt to gracefully close the browser. If the test fails, the
        // browser process will be cleaned up when the launcher is disposed.
        await driver.Browser.CloseAsync();
    }

    [TestCase(TestBrowser.Firefox)]
    [TestCase(TestBrowser.Chrome)]
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
        await driver.BrowsingContext.NavigateAsync(navigateParams);

        string functionDefinition = "(first, second) => first + second";
        List<LocalValue> arguments =
        [
            LocalValue.Number(3),
            LocalValue.Number(5),
        ];

        CallFunctionCommandParameters callFunctionParams = new(functionDefinition, new ContextTarget(browsingContextId), true);
        callFunctionParams.Arguments.AddRange(arguments);

        EvaluateResult scriptResult = await driver.Script.CallFunctionAsync(callFunctionParams);
        Assert.That(scriptResult, Is.InstanceOf<EvaluateResultSuccess>());
        EvaluateResultSuccess successResult = (EvaluateResultSuccess)scriptResult;
        Assert.That(successResult.Result, Is.InstanceOf<NumberRemoteValue>());
        NumberRemoteValue resultValue = (NumberRemoteValue)successResult.Result;
        Assert.That(resultValue.ToInt(), Is.EqualTo(8));

        // Attempt to gracefully close the browser. If the test fails, the
        // browser process will be cleaned up when the launcher is disposed.
        await driver.Browser.CloseAsync();
    }

    [TestCase(TestBrowser.Firefox)]
    [TestCase(TestBrowser.Chrome)]
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
        await driver.BrowsingContext.NavigateAsync(navigateParams);
        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(driver.BrowsingContext.OnLoad.EventName));

        string navigatedUrl = string.Empty;
        await using EventObserver<NavigationEventArgs> navigationObserver = driver.BrowsingContext.OnLoad.AddObserver(e => navigatedUrl = e.Url);

        LocateNodesCommandParameters locateNodesParams = new(browsingContextId, new CssLocator("a"));
        LocateNodesCommandResult locateResult = await driver.BrowsingContext.LocateNodesAsync(locateNodesParams);
        Assert.That(locateResult.Nodes, Has.Count.EqualTo(1));

        NodeRemoteValue nodeRemoteValue = locateResult.Nodes[0];
        SharedReference elementReference = nodeRemoteValue.ToSharedReference();

        navigationObserver.StartCapturingTasks();
        InputBuilder inputBuilder = new();
        inputBuilder.AddClickOnElementAction(elementReference);
        PerformActionsCommandParameters actionsParams = new(browsingContextId);
        actionsParams.Actions.AddRange(inputBuilder.Build());
        await driver.Input.PerformActionsAsync(actionsParams);

        Task[] capturedTasks = await navigationObserver.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5));
        Assert.That(capturedTasks, Has.Length.EqualTo(1));
        Assert.That(navigatedUrl, Is.EqualTo($"http://localhost:{server.Port}/details.html"));

        // Attempt to gracefully close the browser. If the test fails, the
        // browser process will be cleaned up when the launcher is disposed.
        await driver.Browser.CloseAsync();
    }

    [TestCase(TestBrowser.Firefox)]
    [TestCase(TestBrowser.Chrome)]
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
        await driver.BrowsingContext.NavigateAsync(navigateParams);
        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(driver.BrowsingContext.OnLoad.EventName));

        string navigatedUrl = string.Empty;
        await using EventObserver<NavigationEventArgs> navigationObserver = driver.BrowsingContext.OnLoad.AddObserver(e => navigatedUrl = e.Url);

        LocateNodesCommandParameters locateNodesParams = new(browsingContextId, new CssLocator("input#dataToSend"));
        LocateNodesCommandResult locateResult = await driver.BrowsingContext.LocateNodesAsync(locateNodesParams);
        Assert.That(locateResult.Nodes, Has.Count.EqualTo(1));

        NodeRemoteValue nodeRemoteValue = locateResult.Nodes[0];
        SharedReference elementReference = nodeRemoteValue.ToSharedReference();

        navigationObserver.StartCapturingTasks();
        InputBuilder inputBuilder = new();
        inputBuilder.AddClickOnElementAction(elementReference);
        inputBuilder.AddSendKeysToActiveElementAction("Hello WebDriver BiDi" + Keys.Enter);
        PerformActionsCommandParameters actionsParams = new(browsingContextId);
        actionsParams.Actions.AddRange(inputBuilder.Build());
        await driver.Input.PerformActionsAsync(actionsParams);

        Task[] capturedTasks = await navigationObserver.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5));
        Assert.That(capturedTasks, Has.Length.EqualTo(1));
        Assert.That(navigatedUrl, Is.EqualTo($"http://localhost:{server.Port}/processForm"));

        LocateNodesCommandParameters locateFormResultParams = new(browsingContextId, new CssLocator("span"))
        {
            SerializationOptions = new()
            {
                MaxDomDepth = SerializationOptions.InfiniteMaxDomDepth,
            },
        };
        LocateNodesCommandResult locateFormResult = await driver.BrowsingContext.LocateNodesAsync(locateFormResultParams);
        Assert.That(locateFormResult.Nodes, Has.Count.EqualTo(1));

        NodeRemoteValue resultNodeRemoteValue = locateFormResult.Nodes[0];
        NodeProperties resultNodeProperties = resultNodeRemoteValue.GetNodeProperties();
        Assert.That(resultNodeProperties.Children, Is.Not.Null);
        Assert.That(resultNodeProperties.Children, Has.Count.EqualTo(1));
        NodeRemoteValue textContentValue = resultNodeProperties.Children[0];
        Assert.That(textContentValue.GetNodeProperties().NodeValue, Is.EqualTo("Hello WebDriver BiDi"));

        // Attempt to gracefully close the browser. If the test fails, the
        // browser process will be cleaned up when the launcher is disposed.
        await driver.Browser.CloseAsync();
    }

    private async Task<string> GetBrowsingContext(BiDiDriver driver)
    {
        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync();
        return tree.ContextTree[0].BrowsingContextId;
    }

    private async Task<BiDiDriver> StartBiDiDriverSession(BrowserLauncher launcher)
    {
        BiDiDriver driver = new(TimeSpan.FromSeconds(10), launcher.CreateTransport());
        await driver.StartAsync(launcher.WebSocketUrl);

        if (!launcher.IsBiDiSessionInitialized)
        {
            // Using a classic WebDriver browser driver to launch the browser
            // automatically gives you a WebDriver BiDi session. Without the
            // driver executable, you must start your own session.
            await driver.Session.NewSessionAsync(new NewCommandParameters());
        }

        return driver;
    }

    private async Task<BrowserLauncher> CreateBrowserLauncher(TestBrowser browser)
    {
        BrowserLauncher launcher = BrowserTestHelper.GetBrowserLauncher(browser);
        launcher.IsBrowserHeadless = true;
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
