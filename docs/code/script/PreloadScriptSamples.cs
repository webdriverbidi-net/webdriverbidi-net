// <copyright file="PreloadScriptSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/examples/preload-scripts.md

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.

namespace WebDriverBiDi.Docs.Code.Script;

using System.Collections.Generic;
using OpenQA.Selenium.BiDi.Modules.Input;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for preload scripts documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class PreloadScriptSamples
{
    /// <summary>
    /// Example 1: Basic preload script that creates a utility object.
    /// </summary>
    public static async Task BasicPreloadScript()
    {
#region BasicPreloadScript
        string webSocketUrl = "ws://localhost:9222/devtools/browser/YOUR-ID-HERE";
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        try
        {
            await driver.StartAsync(webSocketUrl);
            Console.WriteLine("Connected to browser");

            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                new GetTreeCommandParameters());
            string contextId = tree.ContextTree[0].BrowsingContextId;

            // Add a preload script that creates a utility object
            Console.WriteLine("Adding preload script...");
            string preloadScript = """
                () => {
                    window.myUtils = {
                        getElementText: (selector) => {
                            const element = document.querySelector(selector);
                            return element ? element.textContent : null;
                        },
                        clickElement: (selector) => {
                            const element = document.querySelector(selector);
                            if (element) {
                                element.click();
                                return true;
                            }
                            return false;
                        }
                    };
                    console.log('Preload script: utilities injected');
                }
                """;

            AddPreloadScriptCommandParameters preloadParams = 
                new AddPreloadScriptCommandParameters(preloadScript);

            AddPreloadScriptCommandResult preloadResult = 
                await driver.Script.AddPreloadScriptAsync(preloadParams);
            
            Console.WriteLine($"Preload script added: {preloadResult.PreloadScriptId}");

            // Navigate - the preload script will run before page scripts
            Console.WriteLine("\nNavigating to example.com...");
            await driver.BrowsingContext.NavigateAsync(
                new NavigateCommandParameters(contextId, "https://example.com")
                { Wait = ReadinessState.Complete });

            // Use the injected utilities
            Console.WriteLine("\nUsing preload script utilities...");
            
            EvaluateCommandParameters evalParams = new EvaluateCommandParameters(
                "window.myUtils.getElementText('h1')",
                new ContextTarget(contextId),
                true);

            EvaluateResult result = await driver.Script.EvaluateAsync(evalParams);
            
            if (result is EvaluateResultSuccess success &&
                success.Result is StringRemoteValue textValue)
            {
                string heading = textValue.Value ?? "No heading";
                Console.WriteLine($"Page heading: {heading}");
            }

            // Clean up
            await driver.Script.RemovePreloadScriptAsync(
                new RemovePreloadScriptCommandParameters(preloadResult.PreloadScriptId));

            Console.WriteLine("\n✓ Preload script example complete");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            await driver.StopAsync();
        }
#endregion
    }

    /// <summary>
    /// Example 2: Preload script with channel communication.
    /// </summary>
    public static async Task PreloadScriptWithChannel(BiDiDriver driver, string contextId)
    {
#region PreloadScriptwithChannel
        // Subscribe to script messages
        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Script.OnMessage.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        // Set up message handler
        TaskCompletionSource<string> pageLoadedSignal = new TaskCompletionSource<string>();

        driver.Script.OnMessage.AddObserver((MessageEventArgs e) =>
        {
            if (e.ChannelId == "pageLoadChannel")
            {
                Console.WriteLine($"📨 Received message from preload script");
                
                if (e.Data.Type == RemoteValueType.Object &&
                    e.Data is KeyValuePairCollectionRemoteValue dataRemoteValue)
                {
                    RemoteValueDictionary data = dataRemoteValue.Value;
                    Console.WriteLine($"Page ready: {data["ready"].ConvertTo<BooleanRemoteValue>().Value}");
                    Console.WriteLine($"Load time: {data["loadTime"].ConvertTo<LongRemoteValue>().Value}ms");
                    
                    pageLoadedSignal.SetResult("complete");
                }
            }
        });

        // Add preload script with channel
        string preloadScript = """
            (channel) => {
                const startTime = Date.now();
                
                window.addEventListener('load', () => {
                    const loadTime = Date.now() - startTime;
                    channel({
                        ready: true,
                        loadTime: loadTime,
                        url: window.location.href
                    });
                });
            }
            """;

        ChannelValue channel = new ChannelValue(new ChannelProperties("pageLoadChannel"));

        AddPreloadScriptCommandParameters preloadParams = 
            new AddPreloadScriptCommandParameters(preloadScript);
        preloadParams.Arguments.Add(channel);

        AddPreloadScriptCommandResult preloadResult = 
            await driver.Script.AddPreloadScriptAsync(preloadParams);

        Console.WriteLine("Preload script with channel added");

        // Navigate
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com")
            { Wait = ReadinessState.Complete });

        // Wait for signal from preload script
        await pageLoadedSignal.Task;
        Console.WriteLine("✅ Page load detected by preload script");
#endregion
    }

    /// <summary>
    /// Example 3: Wait for element with preload script.
    /// </summary>
    public static async Task WaitForElementPreloadScript(BiDiDriver driver, string contextId)
    {
#region WaitforElementPreloadScript
        // This preload script waits for a specific element to appear
        string waitForElementScript = @"""
            (channel) => {
                const checkForElement = () => {
                    const element = document.querySelector('.dynamic-content');
                    if (element) {
                        channel({
                            found: true,
                            text: element.textContent,
                            timestamp: Date.now()
                        });
                    } else {
                        // Check again in 100ms
                        setTimeout(checkForElement, 100);
                    }
                };
                
                // Start checking when DOM is ready
                if (document.readyState === 'loading') {
                    document.addEventListener('DOMContentLoaded', checkForElement);
                } else {
                    checkForElement();
                }
            }
            """;

        TaskCompletionSource<RemoteValueDictionary> elementFoundSignal = 
            new TaskCompletionSource<RemoteValueDictionary>();

        driver.Script.OnMessage.AddObserver((MessageEventArgs e) =>
        {
            if (e.ChannelId == "elementWatcher" &&
                e.Data is KeyValuePairCollectionRemoteValue dataRemoteValue)
            {
                RemoteValueDictionary data = dataRemoteValue.Value;
                elementFoundSignal.SetResult(data);
            }
        });

        ChannelValue channel = new ChannelValue(new ChannelProperties("elementWatcher"));

        AddPreloadScriptCommandParameters preloadParams = 
            new AddPreloadScriptCommandParameters(waitForElementScript);
        preloadParams.Arguments.Add(channel);

        await driver.Script.AddPreloadScriptAsync(preloadParams);

        // Navigate to page with dynamic content
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com/dynamic")
            { Wait = ReadinessState.Complete });

        // Wait for element (with timeout)
        Task<RemoteValueDictionary> elementTask = elementFoundSignal.Task;
        Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));

        if (await Task.WhenAny(elementTask, timeoutTask) == elementTask)
        {
            RemoteValueDictionary data = await elementTask;
            Console.WriteLine($"✅ Element found: {data["text"].ConvertTo<StringRemoteValue>().Value}");
        }
        else
        {
            Console.WriteLine("❌ Timeout waiting for element");
        }
#endregion
    }

    /// <summary>
    /// Example 4: Sandbox isolation.
    /// </summary>
    public static async Task SandboxedPreloadScript(BiDiDriver driver, string contextId)
    {
#region SandboxedPreloadScript
        // This preload script waits for a specific element to appear
        string waitForElementScript = @"""
            (channel) => {
                const checkForElement = () => {
                    const element = document.querySelector('.dynamic-content');
                    if (element) {
                        channel({
                            found: true,
                            text: element.textContent,
                            timestamp: Date.now()
                        });
                    } else {
                        // Check again in 100ms
                        setTimeout(checkForElement, 100);
                    }
                };
                
                // Start checking when DOM is ready
                if (document.readyState === 'loading') {
                    document.addEventListener('DOMContentLoaded', checkForElement);
                } else {
                    checkForElement();
                }
            }
            """;

        TaskCompletionSource<RemoteValueDictionary> elementFoundSignal = 
            new TaskCompletionSource<RemoteValueDictionary>();

        driver.Script.OnMessage.AddObserver((MessageEventArgs e) =>
        {
            if (e.ChannelId == "elementWatcher")
            {
                RemoteValueDictionary data = e.Data.ConvertTo<KeyValuePairCollectionRemoteValue>().Value;
                elementFoundSignal.SetResult(data);
            }
        });

        ChannelValue channel = new ChannelValue(new ChannelProperties("elementWatcher"));

        AddPreloadScriptCommandParameters preloadParams = 
            new AddPreloadScriptCommandParameters(waitForElementScript);
        preloadParams.Arguments.Add(channel);

        await driver.Script.AddPreloadScriptAsync(preloadParams);

        // Navigate to page with dynamic content
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com/dynamic")
            { Wait = ReadinessState.Complete });

        // Wait for element (with timeout)
        Task<RemoteValueDictionary> elementTask = elementFoundSignal.Task;
        Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));

        if (await Task.WhenAny(elementTask, timeoutTask) == elementTask)
        {
            RemoteValueDictionary data = await elementTask;
            Console.WriteLine($"✅ Element found: {data["text"].ConvertTo<StringRemoteValue>().Value}");
        }
        else
        {
            Console.WriteLine("❌ Timeout waiting for element");
        }
#endregion
    }

    /// <summary>
    /// Example 5: Intercept page behavior (fetch).
    /// </summary>
    public static async Task InterceptFetchPreloadScript(BiDiDriver driver, string contextId)
    {
#region InterceptFetchPreloadScript
        // Preload script that intercepts fetch calls
        string interceptFetchScript = """
            (channel) => {
                const originalFetch = window.fetch;
                
                window.fetch = async function(...args) {
                    const url = args[0];
                    channel({
                        type: 'fetch',
                        url: url,
                        timestamp: Date.now()
                    });
                    
                    return originalFetch.apply(this, args);
                };
                
                console.log('Fetch interceptor installed');
            }
            """;

        List<RemoteValueDictionary> fetchCalls = new List<RemoteValueDictionary>();

        driver.Script.OnMessage.AddObserver((MessageEventArgs e) =>
        {
            if (e.ChannelId == "fetchInterceptor")
            {
                RemoteValueDictionary data = e.Data.ConvertTo<KeyValuePairCollectionRemoteValue>().Value;
                fetchCalls.Add(data);
                Console.WriteLine($"🌐 Fetch intercepted: {data["url"].ConvertTo<StringRemoteValue>().Value}");
            }
        });

        ChannelValue channel = new ChannelValue(new ChannelProperties("fetchInterceptor"));

        AddPreloadScriptCommandParameters preloadParams = 
            new AddPreloadScriptCommandParameters(interceptFetchScript);
        preloadParams.Arguments.Add(channel);

        await driver.Script.AddPreloadScriptAsync(preloadParams);

        // Navigate to page that makes fetch calls
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://jsonplaceholder.typicode.com")
            { Wait = ReadinessState.Complete });

        await Task.Delay(2000);  // Wait for fetch calls

        Console.WriteLine($"\n📊 Total fetch calls: {fetchCalls.Count}");
#endregion
    }

    /// <summary>
    /// Example 6: Performance monitoring.
    /// </summary>
    public static async Task PerformanceMonitorPreloadScript(BiDiDriver driver, string contextId)
    {
#region PerformanceMonitorPreloadScript
        string performanceMonitorScript = """
            (channel) => {
                window.addEventListener('load', () => {
                    const perfData = performance.getEntriesByType('navigation')[0];
                    
                    channel({
                        domContentLoaded: perfData.domContentLoadedEventEnd - perfData.domContentLoadedEventStart,
                        loadComplete: perfData.loadEventEnd - perfData.loadEventStart,
                        domInteractive: perfData.domInteractive - perfData.fetchStart,
                        totalTime: perfData.loadEventEnd - perfData.fetchStart
                    });
                });
            }
            """;

        RemoteValueDictionary? performanceData = null;

        driver.Script.OnMessage.AddObserver((MessageEventArgs e) =>
        {
            if (e.ChannelId == "performanceMonitor")
            {
                performanceData = e.Data.ConvertTo<KeyValuePairCollectionRemoteValue>().Value;
            }
        });

        ChannelValue channel = new ChannelValue(new ChannelProperties("performanceMonitor"));

        AddPreloadScriptCommandParameters preloadParams = 
            new AddPreloadScriptCommandParameters(performanceMonitorScript);
        preloadParams.Arguments.Add(channel);

        await driver.Script.AddPreloadScriptAsync(preloadParams);

        // Navigate
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com")
            { Wait = ReadinessState.Complete });

        await Task.Delay(1000);  // Wait for load event

        if (performanceData != null)
        {
            Console.WriteLine("\n⏱️ Performance Metrics:");
            Console.WriteLine($"  DOM Content Loaded: {performanceData["domContentLoaded"].ConvertTo<DoubleRemoteValue>().Value}ms");
            Console.WriteLine($"  Load Complete: {performanceData["loadComplete"].ConvertTo<DoubleRemoteValue>().Value}ms");
            Console.WriteLine($"  DOM Interactive: {performanceData["domInteractive"].ConvertTo<DoubleRemoteValue>().Value}ms");
            Console.WriteLine($"  Total Time: {performanceData["totalTime"].ConvertTo<DoubleRemoteValue>().Value}ms");
        }
#endregion
    }

    /// <summary>
    /// Example 7: Multiple preload scripts.
    /// </summary>
    public static async Task MultiplePreloadScripts(BiDiDriver driver)
    {
#region MultiplePreloadScripts
        // Script 1: Utilities
        string utilitiesScript = """
            () => {
                window.testUtils = {
                    highlight: (element) => {
                        element.style.border = '2px solid red';
                    }
                };
            }
            """;

        // Script 2: Monitoring
        string monitoringScript = """
            (channel) => {
                window.addEventListener('click', (e) => {
                    channel({
                        type: 'click',
                        target: e.target.tagName,
                        x: e.clientX,
                        y: e.clientY
                    });
                });
            }
            """;

        // Add both scripts
        AddPreloadScriptCommandResult utils = await driver.Script.AddPreloadScriptAsync(
            new AddPreloadScriptCommandParameters(utilitiesScript));

        ChannelValue channel = new ChannelValue(new ChannelProperties("clickMonitor"));
        AddPreloadScriptCommandParameters monitorParams = 
            new AddPreloadScriptCommandParameters(monitoringScript);
        monitorParams.Arguments.Add(channel);

        AddPreloadScriptCommandResult monitor = await driver.Script.AddPreloadScriptAsync(monitorParams);

        Console.WriteLine("Multiple preload scripts added");

        // Both scripts will run on every navigation
#endregion
    }

    /// <summary>
    /// Pattern: Conditional preload scripts (limit to specific contexts).
    /// </summary>
    public static async Task ConditionalPreloadScripts(
        BiDiDriver driver,
        string contextId,
        string preloadScript)
    {
#region ConditionalPreloadScripts
        // Add preload script only for specific contexts
        AddPreloadScriptCommandParameters preloadParams = 
            new AddPreloadScriptCommandParameters(preloadScript);

        // Limit to specific contexts
        preloadParams.Contexts = new List<string> { contextId };

        AddPreloadScriptCommandResult result = 
            await driver.Script.AddPreloadScriptAsync(preloadParams);

        // Script will only run in the specified context
#endregion
    }

    /// <summary>
    /// Pattern: Temporary preload script (remove when done).
    /// </summary>
    public static async Task TemporaryPreloadScript(
        BiDiDriver driver,
        AddPreloadScriptCommandParameters preloadParams,
        NavigateCommandParameters navParams1,
        NavigateCommandParameters navParams2)
    {
#region TemporaryPreloadScript
        // Add preload script
        AddPreloadScriptCommandResult preloadResult = 
            await driver.Script.AddPreloadScriptAsync(preloadParams);

        try
        {
            // Use preload script for several navigations
            await driver.BrowsingContext.NavigateAsync(navParams1);
            await driver.BrowsingContext.NavigateAsync(navParams2);
        }
        finally
        {
            // Remove when done
            await driver.Script.RemovePreloadScriptAsync(
                new RemovePreloadScriptCommandParameters(preloadResult.PreloadScriptId));
        }

        // Future navigations won't have the preload script
#endregion
    }
}

#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
