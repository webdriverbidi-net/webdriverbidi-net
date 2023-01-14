using System.Diagnostics;
using WebDriverBidi;
using WebDriverBidi.Client;
using WebDriverBidi.BrowsingContext;
using WebDriverBidi.Script;
using WebDriverBidi.Session;

// See https://aka.ms/new-console-template for more information

int port = 38267;
string testProfilePath = Path.Join(Path.GetTempPath(), Path.GetTempFileName());

BrowserType testBrowserName = BrowserType.Chrome;
string testBrowserCommandLine = @"/Applications/Firefox.app/Contents/MacOS/firefox-bin";
string testBrowserArguments = $"--remote-debugging-port {port} --no-remote --profile {testProfilePath}";
Process? testProcess = null;
AutoResetEvent syncEvent = new(false);

try
{
    if (testBrowserName == BrowserType.Firefox)
    {
        testProcess = StartServer(testProfilePath, testBrowserName, testBrowserCommandLine, testBrowserArguments);
    }
    await DriveBrowser();
}
finally
{
    if (testBrowserName == BrowserType.Firefox)
    {
        StopServer(testProfilePath, testBrowserName, testProcess);
    }
}

async Task DriveBrowser()
{
    Driver driver = new();
    driver.LogMessage += OnDriverLogMessage;
    driver.BrowsingContext.NavigationStarted += delegate(object? sender, NavigationEventArgs e)
    {
        Console.WriteLine($"Navigation to {e.Url} started");
    };

    driver.BrowsingContext.Load += delegate(object? sender, NavigationEventArgs e)
    {
        Console.WriteLine($"Load of {e.Url} complete!");
    };

    await driver.Start($"ws://localhost:{port}/session");

    var status = await driver.Session.Status(new StatusCommandSettings());
    Console.WriteLine($"Is ready? {status.IsReady}");

    if (testBrowserName == BrowserType.Firefox)
    {
        var session = await driver.Session.NewSession(new NewCommandSettings());
        Console.WriteLine($"Started session {session.SessionId}");
    }

    var subscribe = new SubscribeCommandSettings();
    subscribe.Events.Add("browsingContext.load");
    await driver.Session.Subscribe(subscribe);

    var tree = await driver.BrowsingContext.GetTree(new GetTreeCommandSettings());
    string contextId = tree.ContextTree[0].BrowsingContextId;
    Console.WriteLine($"Active context: {contextId}");

    var navigation = await driver.BrowsingContext.Navigate(new NavigateCommandSettings(contextId, "https://google.com") { Wait = ReadinessState.Complete });
    Console.WriteLine($"Performed navigation to {navigation.Url}");

    string functionDefinition = "function(){ return document.querySelector('input'); }";
    var scriptResult = await driver.Script.CallFunction(new CallFunctionCommandSettings(functionDefinition, new ContextTarget(contextId), true));
    var scriptSuccessResult = scriptResult as ScriptEvaluateResultSuccess;
    var scriptExceptionResult = scriptResult as ScriptEvaluateResultException;
    if (scriptSuccessResult is not null)
    {
        Console.WriteLine($"Script result: {scriptSuccessResult.Result.Value}");
        NodeProperties? nodeProperties = scriptSuccessResult.Result.ValueAs<NodeProperties>();
        if (nodeProperties is not null)
        {
            Console.WriteLine($"Found element on page with local name '{nodeProperties.LocalName}'");
        }
    }
    else if (scriptExceptionResult is not null)
    {
        Console.WriteLine($"Script exception: {scriptExceptionResult.ExceptionDetails.Text}");
    }

    await driver.Stop();
}

void OnDriverLogMessage(object? sender, LogMessageEventArgs e)
{
    Console.WriteLine(e.Message);
    syncEvent.Set();
}

Process StartServer(string profilePath, BrowserType browserName, string browserCommandLine, string browserArguments)
{
    Console.WriteLine($"Creating temp folder for profile at {profilePath}");
    DirectoryInfo profileDirectory = Directory.CreateDirectory(profilePath);

    Console.WriteLine($"Starting {browserName}");
    Process process = new();
    process.StartInfo.FileName = browserCommandLine;
    process.StartInfo.Arguments = browserArguments;
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.RedirectStandardOutput = true;
    process.Start();
    return process;
}

void StopServer(string profilePath, BrowserType browserName, Process? process)
{
    Console.WriteLine($"Closing {browserName}");
    process?.Kill();
    
    Console.WriteLine($"Deleting temp folder for profile {profilePath}");
    Directory.Delete(profilePath, true);
}