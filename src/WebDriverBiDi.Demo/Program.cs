using WebDriverBiDi;
using WebDriverBiDi.Client;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.Input;
using WebDriverBiDi.DemoWebSite;

DemoWebSiteServer demoSiteServer = new();

// To use a specific port, uncomment the line below and modify it to
// use the desired port.
// int port = 38267;

// Path to the directory containing the browser launcher executables.
// We use the WebDriver Classic browser drivers (chromedriver, geckodriver, etc.)
// as browser launchers.
string browserLauncherDirectory = string.Empty;

// The level at which to log to the console in this demo app. Adjust this
// to control how verbose the logging is.
WebDriverBiDiLogLevel logReportingLevel = WebDriverBiDiLogLevel.Info;

// Select the browser type for which to run this demo.
BrowserType testBrowserType = BrowserType.Chrome;

// Optionally select the location of the browser executable to use.
// The empty string will launch the browser executable from its default
// installed location.
string browserExecutableLocation = string.Empty;

BrowserLauncher launcher = BrowserLauncher.Create(testBrowserType, browserLauncherDirectory, browserExecutableLocation);
launcher.LogMessage += OnDriverLogMessage;
try
{
    demoSiteServer.Launch();
    await launcher.StartAsync();
    await launcher.LaunchBrowserAsync();
    await DriveBrowserAsync(launcher.WebSocketUrl);
}
finally
{
    await launcher.QuitBrowserAsync();
    await launcher.StopAsync();
    demoSiteServer.Shutdown();
}

async Task DriveBrowserAsync(string webSocketUrl)
{
    BiDiDriver driver = new();
    driver.LogMessage += OnDriverLogMessage;
    driver.BrowsingContext.NavigationStarted += (sender, e) =>
    {
        Console.WriteLine($"Navigation to {e.Url} started");
    };

    driver.BrowsingContext.Load += (sender, e) =>
    {
        Console.WriteLine($"Load of {e.Url} complete!");
    };

    await driver.StartAsync(webSocketUrl);

    StatusCommandResult status = await driver.Session.StatusAsync(new StatusCommandParameters());
    Console.WriteLine($"Is ready? {status.IsReady}");

    SubscribeCommandParameters subscribe = new();
    subscribe.Events.Add("browsingContext.navigationStarted");
    subscribe.Events.Add("browsingContext.load");
    await driver.Session.SubscribeAsync(subscribe);

    GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
    string contextId = tree.ContextTree[0].BrowsingContextId;
    Console.WriteLine($"Active context: {contextId}");

    NavigateCommandParameters navigateParams = new(contextId, $"http://localhost:{demoSiteServer.Port}/inputForm.html")
    {
        Wait = ReadinessState.Complete
    };
    NavigationResult navigation = await driver.BrowsingContext.NavigateAsync(navigateParams);
    Console.WriteLine($"Performed navigation to {navigation.Url}");

    string functionDefinition = @"() => document.querySelector('input[name=""dataToSend""]')";
    CallFunctionCommandParameters callFunctionParams = new(functionDefinition, new ContextTarget(contextId), true);
    EvaluateResult scriptResult = await driver.Script.CallFunctionAsync(callFunctionParams);
    if (scriptResult is EvaluateResultSuccess scriptSuccessResult)
    {
        Console.WriteLine($"Script result type: {scriptSuccessResult.Result.Type} (.NET type: {scriptSuccessResult.Result.Value!.GetType()})");
        Console.WriteLine($"Script returned element with ID {scriptSuccessResult.Result.SharedId}");

        RemoteValue scriptResultValue = scriptSuccessResult.Result;
        NodeProperties? nodeProperties = scriptResultValue.ValueAs<NodeProperties>();
        if (nodeProperties is not null)
        {
            Console.WriteLine($"Found element on page with local name '{nodeProperties.LocalName}'");
            SharedReference elementReference = scriptResultValue.ToSharedReference();
            List<SourceActions> actions = GenerateSendKeysToElementActionList(elementReference, "Hello WebDriver BiDi" + Keys.Enter);
            PerformActionsCommandParameters actionsParams = new(contextId);
            actionsParams.Actions.AddRange(actions);
            await driver.Input.PerformActionsAsync(actionsParams);
        }
        else
        {
            Console.WriteLine($"Script result value did not describe a node; returned type {scriptResultValue.Type}");
        }
    }
    else if (scriptResult is EvaluateResultException scriptExceptionResult)
    {
        Console.WriteLine($"Script exception: {scriptExceptionResult.ExceptionDetails.Text}");
    }

    Console.WriteLine("Pausing 3 seconds to view results");
    await Task.Delay(TimeSpan.FromSeconds(3));

    await driver.StopAsync();
}

List<SourceActions> GenerateSendKeysToElementActionList(SharedReference elementReference, string keysToSend)
{
    InputBuilder builder = new();
    PointerInputSource mouse = builder.CreatePointerInputSource(PointerType.Mouse);
    KeyInputSource keyboard = builder.CreateKeyInputSource();
    builder.AddAction(mouse.CreatePointerMove(0, 0, Origin.Element(new ElementOrigin(elementReference))))
        .AddAction(mouse.CreatePointerDown(PointerButton.Left))
        .AddAction(mouse.CreatePointerUp());
    foreach (char character in keysToSend)
    {
        builder.AddAction(keyboard.CreateKeyDown(character))
            .AddAction(keyboard.CreateKeyUp(character));
    }
    return builder.Build();
}

void OnDriverLogMessage(object? sender, LogMessageEventArgs e)
{
    if (e.Level >= logReportingLevel)
    {
        Console.WriteLine(e.Message);
    }
}
