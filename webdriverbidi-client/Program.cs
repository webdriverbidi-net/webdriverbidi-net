﻿using System.Diagnostics;
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

    var status = await driver.Session.Status(new StatusCommandParameters());
    Console.WriteLine($"Is ready? {status.IsReady}");

    if (testBrowserName == BrowserType.Firefox)
    {
        var session = await driver.Session.NewSession(new NewCommandParameters());
        Console.WriteLine($"Started session {session.SessionId}");
    }

    var subscribe = new SubscribeCommandParameters();
    subscribe.Events.Add("browsingContext.load");
    await driver.Session.Subscribe(subscribe);

    var tree = await driver.BrowsingContext.GetTree(new GetTreeCommandParameters());
    string contextId = tree.ContextTree[0].BrowsingContextId;
    Console.WriteLine($"Active context: {contextId}");

    var navigation = await driver.BrowsingContext.Navigate(new NavigateCommandParameters(contextId, "https://google.com") { Wait = ReadinessState.Complete });
    Console.WriteLine($"Performed navigation to {navigation.Url}");

    string functionDefinition = "function(){ return document.querySelector('input'); }";
    var scriptResult = await driver.Script.CallFunction(new CallFunctionCommandParameters(functionDefinition, new ContextTarget(contextId), true));
    var scriptSuccessResult = scriptResult as EvaluateResultSuccess;
    var scriptExceptionResult = scriptResult as EvaluateResultException;
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