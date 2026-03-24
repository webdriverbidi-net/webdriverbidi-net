// <copyright file="CommonScenariosSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/examples/common-scenarios.md

#pragma warning disable CS8600, CS8602, CS8618

namespace WebDriverBiDi.Docs.Code.Examples;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.Storage;

/// <summary>
/// Snippets for common scenarios documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class CommonScenariosSamples
{
    /// <summary>
    /// Basic page navigation.
    /// </summary>
    public static async Task BasicPageNavigation()
    {
#region BasicPageNavigation
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        try
        {
            // Get the active context
            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                new GetTreeCommandParameters());
            string contextId = tree.ContextTree[0].BrowsingContextId;

            // Navigate and wait for page load
            NavigateCommandParameters navParams = new NavigateCommandParameters(
                contextId,
                "https://example.com")
            {
                Wait = ReadinessState.Complete
            };

            NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(navParams);
            Console.WriteLine($"Navigated to: {result.Url}");
        }
        catch (WebDriverBiDiTimeoutException ex)
        {
            Console.WriteLine($"Navigation timeout: {ex.Message}");
            // Handle timeout - maybe retry or fail gracefully
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"Navigation failed: {ex.Message}");
            // Handle other protocol errors
        }
        finally
        {
            await driver.StopAsync();
        }
#endregion
    }

    /// <summary>
    /// Form submission with Script and Input.
    /// </summary>
    public static async Task FormSubmission()
    {
#region FormSubmission
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        try
        {
            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                new GetTreeCommandParameters());
            string contextId = tree.ContextTree[0].BrowsingContextId;

            // Navigate to form page
            await driver.BrowsingContext.NavigateAsync(
                new NavigateCommandParameters(contextId, "https://example.com/form")
                { Wait = ReadinessState.Complete });

            // Find the input field
            string findInputScript = "document.querySelector('input[name=\"username\"]')";
            EvaluateResult inputResult = await driver.Script.EvaluateAsync(
                new EvaluateCommandParameters(findInputScript, new ContextTarget(contextId), true));

            if (inputResult is EvaluateResultSuccess inputSuccess &&
                inputSuccess.Result is NodeRemoteValue inputElement)
            {
                // Click the input to focus it
                // PointerSource and KeySource are hypothetical helper classes to build input actions.
                PerformActionsCommandParameters clickParams = new PerformActionsCommandParameters(contextId);
                PointerSource mouse = new PointerSource("mouse", PointerType.Mouse);
                mouse.CreatePointerMoveToElement(inputElement.ToSharedReference(), 0, 0, TimeSpan.Zero);
                mouse.CreatePointerDown(MouseButton.Left);
                mouse.CreatePointerUp(MouseButton.Left);
                clickParams.Actions.Add(mouse.ToSourceActions());
                await driver.Input.PerformActionsAsync(clickParams);

                // Type into the field
                PerformActionsCommandParameters typeParams = new PerformActionsCommandParameters(contextId);
                KeySource keyboard = new KeySource("keyboard");
                
                string username = "testuser";
                foreach (char c in username)
                {
                    keyboard.CreateKeyDown(c.ToString());
                    keyboard.CreateKeyUp(c.ToString());
                }
                
                typeParams.Actions.Add(keyboard.ToSourceActions());
                await driver.Input.PerformActionsAsync(typeParams);

                // Submit the form (press Enter)
                PerformActionsCommandParameters submitParams = new PerformActionsCommandParameters(contextId);
                KeySource submitKey = new KeySource("keyboard");
                submitKey.CreateKeyDown(Keys.Enter);
                submitKey.CreateKeyUp(Keys.Enter);
                submitParams.Actions.Add(submitKey.ToSourceActions());
                await driver.Input.PerformActionsAsync(submitParams);
            }
        }
        finally
        {
            await driver.StopAsync();
        }
#endregion
    }

    /// <summary>
    /// Monitoring console logs.
    /// </summary>
    public static async Task MonitoringConsoleLogs()
    {
#region MonitoringConsoleLogs
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        try
        {
            // Set up log monitoring with error handling inside the handler
            List<string> consoleMessages = new List<string>();

            driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
            {
                try
                {
                    string message = $"[{e.Timestamp:HH:mm:ss}] {e.Level}: {e.Text}";
                    consoleMessages.Add(message);
                    Console.WriteLine(message);
                }
                catch (Exception ex)
                {
                    // Handle errors within the event handler
                    Console.WriteLine($"Error processing log entry: {ex.Message}");
                }
            });

            // Subscribe to log events
            SubscribeCommandParameters subscribe = 
                new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName);
            await driver.Session.SubscribeAsync(subscribe);

            // Navigate to page
            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                new GetTreeCommandParameters());
            string contextId = tree.ContextTree[0].BrowsingContextId;

            await driver.BrowsingContext.NavigateAsync(
                new NavigateCommandParameters(contextId, "https://example.com")
                { Wait = ReadinessState.Complete });

            // Wait for logs to arrive
            await Task.Delay(2000);

            Console.WriteLine($"\nTotal console messages: {consoleMessages.Count}");
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"WebDriver BiDi error: {ex.Message}");
        }
        finally
        {
            await driver.StopAsync();
        }
#endregion
    }

    /// <summary>
    /// Capture screenshot.
    /// </summary>
    public static async Task CaptureScreenshot()
    {
#region CaptureScreenshot
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        try
        {
            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                new GetTreeCommandParameters());
            string contextId = tree.ContextTree[0].BrowsingContextId;

            // Navigate to page
            await driver.BrowsingContext.NavigateAsync(
                new NavigateCommandParameters(contextId, "https://example.com")
                { Wait = ReadinessState.Complete });

            // Capture full page screenshot
            CaptureScreenshotCommandParameters screenshotParams = 
                new CaptureScreenshotCommandParameters(contextId);
            
            CaptureScreenshotCommandResult result = 
                await driver.BrowsingContext.CaptureScreenshotAsync(screenshotParams);

            // Save to file
            byte[] imageBytes = Convert.FromBase64String(result.Data);
            await File.WriteAllBytesAsync("screenshot.png", imageBytes);
            
            Console.WriteLine("Screenshot saved to screenshot.png");
        }
        finally
        {
            await driver.StopAsync();
        }
#endregion
    }

    /// <summary>
    /// Network traffic monitoring.
    /// </summary>
    public static async Task NetworkTrafficMonitoring()
    {
#region NetworkTrafficMonitoring
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        try
        {
            // Track all network requests
            List<RequestData> requests = new List<RequestData>();
            
            driver.Network.OnResponseCompleted.AddObserver((ResponseCompletedEventArgs e) =>
            {
                requests.Add(e.Request);
                Console.WriteLine($"{e.Response.Status} - {e.Request.Method} {e.Request.Url}");
            });

            // Subscribe to network events
            SubscribeCommandParameters subscribe = 
                new SubscribeCommandParameters(driver.Network.OnResponseCompleted.EventName);
            await driver.Session.SubscribeAsync(subscribe);

            // Navigate
            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                new GetTreeCommandParameters());
            string contextId = tree.ContextTree[0].BrowsingContextId;

            await driver.BrowsingContext.NavigateAsync(
                new NavigateCommandParameters(contextId, "https://example.com")
                { Wait = ReadinessState.Complete });

            // Wait for all resources to load
            await Task.Delay(2000);

            Console.WriteLine($"\nTotal requests: {requests.Count}");
            
            // Analyze requests
            int htmlCount = requests.Count(r => r.Url.Contains(".html") || r.Url.EndsWith("/"));
            int cssCount = requests.Count(r => r.Url.Contains(".css"));
            int jsCount = requests.Count(r => r.Url.Contains(".js"));
            int imageCount = requests.Count(r => r.Url.Contains(".jpg") || r.Url.Contains(".png") || r.Url.Contains(".gif"));
            
            Console.WriteLine($"HTML: {htmlCount}, CSS: {cssCount}, JS: {jsCount}, Images: {imageCount}");
        }
        finally
        {
            await driver.StopAsync();
        }
#endregion
    }

    /// <summary>
    /// Multi-tab management with Script.EvaluateAsync for titles.
    /// </summary>
    public static async Task MultiTabManagement()
    {
#region Multi-TabManagement
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        try
        {
            // Create multiple tabs
            List<string> tabIds = new List<string>();
            
            for (int i = 0; i < 3; i++)
            {
                CreateCommandResult result = await driver.BrowsingContext.CreateAsync(
                    new CreateCommandParameters(CreateType.Tab));
                tabIds.Add(result.BrowsingContextId);
                Console.WriteLine($"Created tab {i + 1}: {result.BrowsingContextId}");
            }

            // Navigate each tab to different URL
            string[] urls = 
            {
                "https://example.com",
                "https://example.org",
                "https://example.net"
            };

            for (int i = 0; i < tabIds.Count; i++)
            {
                await driver.BrowsingContext.NavigateAsync(
                    new NavigateCommandParameters(tabIds[i], urls[i])
                    { Wait = ReadinessState.Complete });
                Console.WriteLine($"Tab {i + 1} navigated to {urls[i]}");
            }

            // Get page titles from all tabs
            for (int i = 0; i < tabIds.Count; i++)
            {
                EvaluateResult result = await driver.Script.EvaluateAsync(
                    new EvaluateCommandParameters(
                        "document.title",
                        new ContextTarget(tabIds[i]),
                        true));
                
                if (result is EvaluateResultSuccess success &&
                    success.Result is StringRemoteValue stringValue)
                {
                    string title = stringValue.Value ?? "No title";
                    Console.WriteLine($"Tab {i + 1} title: {title}");
                }
            }

            // Close all tabs
            foreach (string tabId in tabIds)
            {
                await driver.BrowsingContext.CloseAsync(new CloseCommandParameters(tabId));
            }
            
            Console.WriteLine("All tabs closed");
        }
        finally
        {
            await driver.StopAsync();
        }
#endregion
    }

    /// <summary>
    /// Wait for element with polling via Script.
    /// </summary>
    public static async Task WaitForElement()
    {
#region WaitforElement
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        try
        {
            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                new GetTreeCommandParameters());
            string contextId = tree.ContextTree[0].BrowsingContextId;

            await driver.BrowsingContext.NavigateAsync(
                new NavigateCommandParameters(contextId, "https://example.com")
                { Wait = ReadinessState.Complete });

            // Wait for element to appear (with polling)
            bool elementFound = false;
            int maxAttempts = 30;
            int attempt = 0;

            while (!elementFound && attempt < maxAttempts)
            {
                EvaluateResult result = await driver.Script.EvaluateAsync(
                    new EvaluateCommandParameters(
                        "document.querySelector('.dynamic-content') !== null",
                        new ContextTarget(contextId),
                        true));

                if (result is EvaluateResultSuccess success &&
                    success.Result is BooleanRemoteValue boolValue)
                {
                    elementFound = boolValue.Value;
                    if (elementFound)
                    {
                        Console.WriteLine($"Element found after {attempt * 100}ms");
                        break;
                    }
                }

                await Task.Delay(100);
                attempt++;
            }

            if (!elementFound)
            {
                Console.WriteLine("Element not found after timeout");
            }
        }
        finally
        {
            await driver.StopAsync();
        }
#endregion
    }

    /// <summary>
    /// Cookie management.
    /// </summary>
    public static async Task CookieManagement()
    {
#region CookieManagement
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        try
        {
            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                new GetTreeCommandParameters());
            string contextId = tree.ContextTree[0].BrowsingContextId;

            // Set a cookie before navigating
            PartialCookie cookie = new PartialCookie(
                "userPreference",
                BytesValue.FromString("darkMode"),
                "example.com")
            {
                Path = "/",
                Secure = false,
                HttpOnly = false
            };

            await driver.Storage.SetCookieAsync(new SetCookieCommandParameters(cookie));
            Console.WriteLine("Cookie set");

            // Navigate
            await driver.BrowsingContext.NavigateAsync(
                new NavigateCommandParameters(contextId, "https://example.com")
                { Wait = ReadinessState.Complete });

            // Retrieve cookies
            GetCookiesCommandParameters getCookiesParams = new GetCookiesCommandParameters();
            getCookiesParams.Partition = new BrowsingContextPartitionDescriptor(contextId);
            
            GetCookiesCommandResult cookies = await driver.Storage.GetCookiesAsync(getCookiesParams);

            Console.WriteLine($"\nAll cookies ({cookies.Cookies.Count}):");
            foreach (Cookie c in cookies.Cookies)
            {
                Console.WriteLine($"  {c.Name} = {c.Value.Value}");
            }

            // Delete specific cookie
            DeleteCookiesCommandParameters deleteParams = new DeleteCookiesCommandParameters();
            deleteParams.Filter = new CookieFilter { Name = "userPreference" };
            await driver.Storage.DeleteCookiesAsync(deleteParams);
            
            Console.WriteLine("\nCookie deleted");
        }
        finally
        {
            await driver.StopAsync();
        }
#endregion
    }

    /// <summary>
    /// JavaScript execution examples.
    /// </summary>
    public static async Task JavaScriptExecution()
    {
#region JavaScriptExecution
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        try
        {
            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                new GetTreeCommandParameters());
            string contextId = tree.ContextTree[0].BrowsingContextId;

            await driver.BrowsingContext.NavigateAsync(
                new NavigateCommandParameters(contextId, "https://example.com")
                { Wait = ReadinessState.Complete });

            // Execute simple expression
            EvaluateResult titleResult = await driver.Script.EvaluateAsync(
                new EvaluateCommandParameters("document.title", new ContextTarget(contextId), true));

            if (titleResult is EvaluateResultSuccess titleSuccess &&
                titleSuccess.Result is StringRemoteValue titleValue)
            {
                Console.WriteLine($"Page title: {titleValue.Value}");
            }

            // Call function with arguments
            string addFunction = "(a, b) => a + b";
            CallFunctionCommandParameters funcParams = new CallFunctionCommandParameters(
                addFunction,
                new ContextTarget(contextId),
                true);
            funcParams.Arguments.Add(LocalValue.Number(10));
            funcParams.Arguments.Add(LocalValue.Number(20));

            EvaluateResult sumResult = await driver.Script.CallFunctionAsync(funcParams);
            if (sumResult is EvaluateResultSuccess sumSuccess &&
                sumSuccess.Result is LongRemoteValue sumValue)
            {
                Console.WriteLine($"10 + 20 = {sumValue.Value}");
            }

            // Execute complex script
            string complexScript = @"
            {
                url: window.location.href,
                linkCount: document.querySelectorAll('a').length,
                imageCount: document.querySelectorAll('img').length,
                hasTitle: !!document.title
            }";

            EvaluateResult complexResult = await driver.Script.EvaluateAsync(
                new EvaluateCommandParameters(complexScript, new ContextTarget(contextId), true));

            if (complexResult is EvaluateResultSuccess complexSuccess)
            {
                RemoteValueDictionary data = complexSuccess.Result.ConvertTo<KeyValuePairCollectionRemoteValue>().Value;
                Console.WriteLine($"\nPage analysis:");
                Console.WriteLine($"  URL: {data["url"].ConvertTo<StringRemoteValue>().Value}");
                Console.WriteLine($"  Links: {data["linkCount"].ConvertTo<LongRemoteValue>().Value}");
                Console.WriteLine($"  Images: {data["imageCount"].ConvertTo<LongRemoteValue>().Value}");
                Console.WriteLine($"  Has title: {data["hasTitle"].ConvertTo<BooleanRemoteValue>().Value}");
            }
        }
        finally
        {
            await driver.StopAsync();
        }
#endregion
    }
}

internal class Keys
{
    public static string Enter { get; internal set; }
}

internal class KeySource
{
    private string v;

    public KeySource(string v)
    {
        this.v = v;
    }

    internal void CreateKeyDown(string v)
    {
        throw new NotImplementedException();
    }

    internal void CreateKeyUp(string v)
    {
        throw new NotImplementedException();
    }

    internal SourceActions ToSourceActions()
    {
        throw new NotImplementedException();
    }
}

internal class MouseButton
{
    public static object Left { get; internal set; }
}

internal class PointerSource
{
    public PointerSource(string v, PointerType mouse)
    {
    }

    internal void CreatePointerDown(object left)
    {
        throw new NotImplementedException();
    }

    internal void CreatePointerMoveToElement(SharedReference sharedReference, int v1, int v2, TimeSpan zero)
    {
        throw new NotImplementedException();
    }

    internal void CreatePointerUp(object left)
    {
        throw new NotImplementedException();
    }

    internal SourceActions ToSourceActions()
    {
        throw new NotImplementedException();
    }
}