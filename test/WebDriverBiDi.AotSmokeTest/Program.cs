// <copyright file="Program.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client.Launchers;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

string browser = args.Length > 0 ? args[0].ToLowerInvariant() : "firefox";
Console.WriteLine($"Browser: {browser}");

BrowserLauncher launcher;
switch (browser)
{
    case "firefox":
        string firefoxPath = await FirefoxNightlyFetcher.GetFirefoxPathAsync();
        Console.WriteLine($"Firefox path: {firefoxPath}");
        FirefoxLauncher firefoxLauncher = new(firefoxPath);
        firefoxLauncher.IsBrowserHeadless = true;
        launcher = firefoxLauncher;
        break;
    case "chrome":
        string chromePath = Environment.GetEnvironmentVariable("CHROME_EXECUTABLE") ?? string.Empty;
        if (!string.IsNullOrEmpty(chromePath))
        {
            Console.WriteLine($"Chrome path (from env): {chromePath}");
        }

        ChromeLauncher chromeLauncher = string.IsNullOrEmpty(chromePath)
            ? new ChromeLauncher()
            : new ChromeLauncher(chromePath);
        chromeLauncher.IsBrowserHeadless = true;
        launcher = chromeLauncher;
        break;
    default:
        Console.Error.WriteLine($"Unknown browser: {browser}. Use 'firefox' or 'chrome'.");
        return 1;
}

BiDiDriver? driver = null;
try
{
    await launcher.StartAsync();
    await launcher.LaunchBrowserAsync();
    Console.WriteLine("Browser launched.");

    Transport transport = launcher.CreateTransport();
    driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);
    await driver.StartAsync(launcher.WebSocketUrl);
    Console.WriteLine("BiDi connection established.");

    await driver.Session.NewSessionAsync(new NewCommandParameters());
    Console.WriteLine("Session created.");

    GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
    if (tree.ContextTree.Count == 0)
    {
        throw new InvalidOperationException("No browsing contexts found.");
    }

    string contextId = tree.ContextTree[0].BrowsingContextId;
    Console.WriteLine($"Browsing context: {contextId}");

    NavigateCommandParameters navigateParams = new(contextId, "https://github.com");
    navigateParams.Wait = ReadinessState.Complete;
    await driver.BrowsingContext.NavigateAsync(navigateParams);
    Console.WriteLine("Navigation to github.com complete.");

    EvaluateCommandParameters evalParams = new("document.title", new ContextTarget(contextId), true);
    EvaluateResult evalResult = await driver.Script.EvaluateAsync(evalParams);

    if (evalResult is not EvaluateResultSuccess success)
    {
        throw new InvalidOperationException($"Script evaluation failed: result type was {evalResult.ResultType}");
    }

    string? title = success.Result.ValueAs<string>();
    Console.WriteLine($"Page title: {title}");

    if (title is null || !title.Contains("GitHub", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"Expected page title to contain 'GitHub', but got: '{title}'");
    }

    // Call a function with arguments — this exercises the CallFunctionCommandParameters
    // serialization path, which resolves the ArgumentValue polymorphic type hierarchy
    // including RemoteReference subtypes (RemoteObjectReference, SharedReference).
    CallFunctionCommandParameters callParams = new("(name) => `Hello, ${name}!`", new ContextTarget(contextId), true);
    callParams.Arguments.Add(LocalValue.String("World"));
    EvaluateResult callResult = await driver.Script.CallFunctionAsync(callParams);

    if (callResult is not EvaluateResultSuccess callSuccess)
    {
        throw new InvalidOperationException($"CallFunction failed: result type was {callResult.ResultType}");
    }

    string? greeting = callSuccess.Result.ValueAs<string>();
    Console.WriteLine($"CallFunction result: {greeting}");

    if (greeting != "Hello, World!")
    {
        throw new InvalidOperationException($"Expected 'Hello, World!' but got: '{greeting}'");
    }

    Console.WriteLine($"PASS: Integration test succeeded — connected to {browser}, navigated to GitHub, verified page title and callFunction.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"FAIL: {ex.GetType().Name}: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    return 1;
}
finally
{
    if (driver is not null)
    {
        try
        {
            await driver.StopAsync();
        }
        catch
        {
            // Best effort cleanup
        }
    }

    try
    {
        await launcher.QuitBrowserAsync();
    }
    catch
    {
        // Best effort cleanup
    }
}
