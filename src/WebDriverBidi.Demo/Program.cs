using WebDriverBidi;
using WebDriverBidi.Client;
using WebDriverBidi.BrowsingContext;
using WebDriverBidi.Script;
using WebDriverBidi.Session;
using WebDriverBidi.Input;

// See https://aka.ms/new-console-template for more information

// To use a specific port, uncomment the line below and modify it to
// use the desired port.
// int port = 38267;

// Path to the directory containing the browser launcher executables.
// We use the WebDriver Classic browser drivers (chromedriver, geckodriver, etc.)
// as browser launchers.
string browserLauncherDirectory = "/Users/james.evans/Downloads";

// The level at which to log to the console in this demo app. Adjust this
// to control how verbose the logging is.
WebDriverBidiLogLevel logReportingLevel = WebDriverBidiLogLevel.Debug;

// Select the browser type for which to run this demo.
BrowserType testBrowserType = BrowserType.Firefox;

// Optionally select the location of the browser executable to use.
// The empty string will launch the browser executable from its default
// installed location.
string browserExecutableLocation = "/Applications/Firefox Nightly.app/Contents/MacOS/firefox";

BrowserLauncher launcher = BrowserLauncher.Create(testBrowserType, browserLauncherDirectory, browserExecutableLocation);
try
{
    await launcher.Start();
    await launcher.LaunchBrowser();
    await DriveBrowser(launcher.WebSocketUrl);
}
finally
{
    await launcher.QuitBrowser();
    await launcher.Stop();
}

async Task DriveBrowser(string webSocketUrl)
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

    await driver.Start(webSocketUrl);

    var status = await driver.Session.Status(new StatusCommandParameters());
    Console.WriteLine($"Is ready? {status.IsReady}");

    var subscribe = new SubscribeCommandParameters();
    subscribe.Events.Add("browsingContext.load");
    await driver.Session.Subscribe(subscribe);

    var tree = await driver.BrowsingContext.GetTree(new GetTreeCommandParameters());
    string contextId = tree.ContextTree[0].BrowsingContextId;
    Console.WriteLine($"Active context: {contextId}");

    var navigation = await driver.BrowsingContext.Navigate(new NavigateCommandParameters(contextId, "https://google.com/") { Wait = ReadinessState.Complete });
    Console.WriteLine($"Performed navigation to {navigation.Url}");

    string functionDefinition = @"function(){ return document.querySelector('*[name=""q""]'); }";
    var scriptResult = await driver.Script.CallFunction(new CallFunctionCommandParameters(functionDefinition, new ContextTarget(contextId), true));
    if (scriptResult is EvaluateResultSuccess scriptSuccessResult)
    {
        Console.WriteLine($"Script result type: {scriptSuccessResult.Result.Value!.GetType()}");
        NodeProperties? nodeProperties = scriptSuccessResult.Result.ValueAs<NodeProperties>();
        if (nodeProperties is not null)
        {
            Console.WriteLine($"Found element on page with local name '{nodeProperties.LocalName}'");
        }
    }
    else if (scriptResult is EvaluateResultException scriptExceptionResult)
    {
        Console.WriteLine($"Script exception: {scriptExceptionResult.ExceptionDetails.Text}");
    }

    await driver.Stop();
}

void OnDriverLogMessage(object? sender, LogMessageEventArgs e)
{
    if (e.Level >= logReportingLevel)
    {
        Console.WriteLine(e.Message);
    }
}
