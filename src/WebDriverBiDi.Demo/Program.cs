using WebDriverBiDi;
using WebDriverBiDi.Client;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.Input;

// See https://aka.ms/new-console-template for more information

// To use a specific port, uncomment the line below and modify it to
// use the desired port.
// int port = 38267;

// Path to the directory containing the browser launcher executables.
// We use the WebDriver Classic browser drivers (chromedriver, geckodriver, etc.)
// as browser launchers.
string browserLauncherDirectory = string.Empty;

// The level at which to log to the console in this demo app. Adjust this
// to control how verbose the logging is.
WebDriverBiDiLogLevel logReportingLevel = WebDriverBiDiLogLevel.Debug;

// Select the browser type for which to run this demo.
BrowserType testBrowserType = BrowserType.Chrome;

// Optionally select the location of the browser executable to use.
// The empty string will launch the browser executable from its default
// installed location.
string browserExecutableLocation = string.Empty;

BrowserLauncher launcher = BrowserLauncher.Create(testBrowserType, browserLauncherDirectory, browserExecutableLocation);
try
{
    await launcher.StartAsync();
    await launcher.LaunchBrowserAsync();
    await DriveBrowserAsync(launcher.WebSocketUrl);
}
finally
{
    await launcher.QuitBrowserAsync();
    await launcher.StopAsync();
}

async Task DriveBrowserAsync(string webSocketUrl)
{
    Driver driver = new();
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

    var status = await driver.Session.StatusAsync(new StatusCommandParameters());
    Console.WriteLine($"Is ready? {status.IsReady}");

    var subscribe = new SubscribeCommandParameters();
    subscribe.Events.Add("browsingContext.load");
    await driver.Session.SubscribeAsync(subscribe);

    var tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
    string contextId = tree.ContextTree[0].BrowsingContextId;
    Console.WriteLine($"Active context: {contextId}");

    var navigation = await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters(contextId, "https://google.com/") { Wait = ReadinessState.Complete });
    Console.WriteLine($"Performed navigation to {navigation.Url}");

    string functionDefinition = @"function(){ return document.querySelector('*[name=""q""]'); }";
    var scriptResult = await driver.Script.CallFunctionAsync(new CallFunctionCommandParameters(functionDefinition, new ContextTarget(contextId), true));
    if (scriptResult is EvaluateResultSuccess scriptSuccessResult)
    {
        Console.WriteLine($"Script result type: {scriptSuccessResult.Result.Value!.GetType()}");
        Console.WriteLine($"Script returned element with ID {scriptSuccessResult.Result.SharedId}");
        NodeProperties? nodeProperties = scriptSuccessResult.Result.ValueAs<NodeProperties>();
        if (nodeProperties is not null)
        {
            Console.WriteLine($"Found element on page with local name '{nodeProperties.LocalName}'");
        }

        SharedReference elementReference = scriptSuccessResult.Result.ToSharedReference();
        InputBuilder builder = new();
        PointerInputSource mouse = builder.CreatePointerInputSource(PointerType.Mouse);
        KeyInputSource keyboard = builder.CreateKeyInputSource();
        builder.AddAction(mouse.CreatePointerMove(0, 0, Origin.Element(new ElementOrigin(elementReference))))
            .AddAction(mouse.CreatePointerDown())
            .AddAction(mouse.CreatePointerUp())
            .AddAction(keyboard.CreateKeyDown('h'))
            .AddAction(keyboard.CreateKeyUp('h'))
            .AddAction(keyboard.CreateKeyDown('e'))
            .AddAction(keyboard.CreateKeyUp('e'))
            .AddAction(keyboard.CreateKeyDown('l'))
            .AddAction(keyboard.CreateKeyUp('l'))
            .AddAction(keyboard.CreateKeyDown('l'))
            .AddAction(keyboard.CreateKeyUp('l'))
            .AddAction(keyboard.CreateKeyDown('o'))
            .AddAction(keyboard.CreateKeyUp('o'));
        PerformActionsCommandParameters actionsParams = new(contextId);
        actionsParams.Actions.AddRange(builder.Build());
        await driver.Input.PerformActionsAsync(actionsParams);
    }
    else if (scriptResult is EvaluateResultException scriptExceptionResult)
    {
        Console.WriteLine($"Script exception: {scriptExceptionResult.ExceptionDetails.Text}");
    }

    await driver.StopAsync();
}

void OnDriverLogMessage(object? sender, LogMessageEventArgs e)
{
    if (e.Level >= logReportingLevel)
    {
        Console.WriteLine(e.Message);
    }
}
