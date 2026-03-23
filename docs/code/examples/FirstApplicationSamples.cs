// <copyright file="FirstApplicationSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/first-application.md

#pragma warning disable CS8600, CS8602

namespace WebDriverBiDi.Docs.Code.Examples;

using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Log;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for first application tutorial.
/// </summary>
public static class FirstApplicationSamples
{
    /// <summary>
    /// Full first application with console log, navigation, script, screenshot.
    /// </summary>
    public static async Task FullFirstApplication()
    {
#region FullFirstApplication
        // Replace with your WebSocket URL from step 4
        string webSocketUrl = "ws://localhost:9222/devtools/browser/YOUR-ID-HERE";

        // Create driver with 30 second timeout
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        try
        {
            Console.WriteLine("Connecting to browser...");
            await driver.StartAsync(webSocketUrl);
            Console.WriteLine("Connected!");

            // Set up console log monitoring
            driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
            {
                Console.WriteLine($"[Browser Console] {e.Level}: {e.Text}");
            });

            SubscribeCommandParameters subscribe = 
                new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName);
            await driver.Session.SubscribeAsync(subscribe);

            // Get the current browsing context
            Console.WriteLine("\nGetting browsing contexts...");
            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                new GetTreeCommandParameters());

            string contextId = tree.ContextTree[0].BrowsingContextId;
            Console.WriteLine($"Active context: {contextId}");

            // Navigate to a website
            Console.WriteLine("\nNavigating to example.com...");
            NavigateCommandParameters navParams = new NavigateCommandParameters(
                contextId,
                "https://example.com")
            {
                Wait = ReadinessState.Complete
            };

            NavigateCommandResult navResult = await driver.BrowsingContext.NavigateAsync(navParams);
            Console.WriteLine($"Navigation complete! URL: {navResult.Url}");

            // Get the page title
            Console.WriteLine("\nGetting page title...");
            EvaluateCommandParameters evalParams = new EvaluateCommandParameters(
                "document.title",
                new ContextTarget(contextId),
                true);

            EvaluateResult scriptResult = await driver.Script.EvaluateAsync(evalParams);

            if (scriptResult is EvaluateResultSuccess success &&
                success.Result is StringRemoteValue titleValue)
            {
                string title = titleValue.Value ?? "No title";
                Console.WriteLine($"Page title: {title}");
            }

            // Get page information
            Console.WriteLine("\nGetting page information...");
            string infoScript = @"
            {
                url: window.location.href,
                linkCount: document.querySelectorAll('a').length,
                headingCount: document.querySelectorAll('h1, h2, h3, h4, h5, h6').length,
                paragraphCount: document.querySelectorAll('p').length
            }";

            EvaluateCommandParameters infoParams = new EvaluateCommandParameters(
                infoScript,
                new ContextTarget(contextId),
                true);

            EvaluateResult infoResult = await driver.Script.EvaluateAsync(infoParams);

            if (infoResult is EvaluateResultSuccess infoSuccess &&
                infoSuccess.Result is KeyValuePairCollectionRemoteValue infoValue)
            {
                RemoteValueDictionary info = infoValue.Value;
                Console.WriteLine("Page Analysis:");
                Console.WriteLine($"  URL: {info["url"].ConvertTo<StringRemoteValue>().Value}");
                Console.WriteLine($"  Links: {info["linkCount"].ConvertTo<LongRemoteValue>().Value}");
                Console.WriteLine($"  Headings: {info["headingCount"].ConvertTo<LongRemoteValue>().Value}");
                Console.WriteLine($"  Paragraphs: {info["paragraphCount"].ConvertTo<LongRemoteValue>().Value}");
            }

            // Take a screenshot
            Console.WriteLine("\nCapturing screenshot...");
            CaptureScreenshotCommandParameters screenshotParams =
                new CaptureScreenshotCommandParameters(contextId);

            CaptureScreenshotCommandResult screenshot =
                await driver.BrowsingContext.CaptureScreenshotAsync(screenshotParams);

            byte[] imageBytes = Convert.FromBase64String(screenshot.Data);
            await File.WriteAllBytesAsync("example-screenshot.png", imageBytes);
            Console.WriteLine("Screenshot saved to example-screenshot.png");

            Console.WriteLine("\n✓ All operations completed successfully!");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"\n✗ WebDriver BiDi Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Unexpected Error: {ex.Message}");
        }
        finally
        {
            // Disconnect from the browser
            Console.WriteLine("\nDisconnecting from browser...");
            await driver.StopAsync();
            Console.WriteLine("Disconnected!");
        }
#endregion
    }
}

public static class FirstApplicationSamplesIndividual
{
    /// <summary>
    /// Driver initialization.
    /// </summary>
    public static async Task DriverInitialization(string webSocketUrl)
    {
#region DriverInitialization
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync(webSocketUrl);
#endregion
    }

    /// <summary>
    /// Event subscription for console logs.
    /// </summary>
    public static async Task EventSubscription(BiDiDriver driver)
    {
#region EventSubscription
        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) => { /* handle log */ });
        SubscribeCommandParameters subscribe = new SubscribeCommandParameters(
            driver.Log.OnEntryAdded.EventName);
        await driver.Session.SubscribeAsync(subscribe);
#endregion
    }

    /// <summary>
    /// Getting the context.
    /// </summary>
    public static async Task<string> GettingContext(BiDiDriver driver)
    {
#region GettingContext
        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
            new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
#endregion
        return contextId;
    }

    /// <summary>
    /// Navigation with wait for complete.
    /// </summary>
    public static async Task Navigation(BiDiDriver driver, string contextId)
    {
#region Navigation
        NavigateCommandParameters navParams = new NavigateCommandParameters(
            contextId,
            "https://example.com")
        {
            Wait = ReadinessState.Complete
        };
        await driver.BrowsingContext.NavigateAsync(navParams);
#endregion
    }

    /// <summary>
    /// JavaScript execution.
    /// </summary>
    public static async Task JavaScriptExecution(BiDiDriver driver, string contextId)
    {
#region JavaScriptExecution
        EvaluateCommandParameters evalParams = new EvaluateCommandParameters(
            "document.title",
            new ContextTarget(contextId),
            true);
        EvaluateResult result = await driver.Script.EvaluateAsync(evalParams);
#endregion
    }

    /// <summary>
    /// Screenshot capture.
    /// </summary>
    public static async Task ScreenshotCapture(BiDiDriver driver, string contextId)
    {
#region ScreenshotCapture
        CaptureScreenshotCommandParameters screenshotParams =
            new CaptureScreenshotCommandParameters(contextId);
        CaptureScreenshotCommandResult screenshot =
            await driver.BrowsingContext.CaptureScreenshotAsync(screenshotParams);
        byte[] imageBytes = Convert.FromBase64String(screenshot.Data);
        await File.WriteAllBytesAsync("screenshot.png", imageBytes);
#endregion
    }
}
