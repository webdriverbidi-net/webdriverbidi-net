// <copyright file="GettingStartedSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/getting-started.md

#pragma warning disable CS8600, CS8602

namespace WebDriverBiDi.Docs.Code.Examples;

using System.Net.Http;
using System.Text.Json;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

/// <summary>
/// Snippets for getting started documentation.
/// </summary>
public static class GettingStartedSamples
{
    /// <summary>
    /// Programmatic WebSocket URL discovery.
    /// </summary>
#region DiscoverWebSocketURL
    public static async Task<string> DiscoverWebSocketUrlAsync(int port = 9222)
    {
        using HttpClient client = new HttpClient();

        try
        {
            string json = await client.GetStringAsync($"http://localhost:{port}/json/version");
            using JsonDocument doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("webSocketDebuggerUrl", out JsonElement urlElement))
            {
                string? webSocketUrl = urlElement.GetString();
                if (!string.IsNullOrEmpty(webSocketUrl))
                {
                    return webSocketUrl;
                }
            }

            throw new Exception("WebSocket URL not found in response");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to connect to browser on port {port}. " +
                "Ensure Chrome is running with --remote-debugging-port={port}", ex);
        }
    }
#endregion

    public static async Task DiscoverUsage()
    {
#region DiscoverWebSocketUrlUsage
        // Usage
        string webSocketUrl = await DiscoverWebSocketUrlAsync();
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync(webSocketUrl);
#endregion
    }

#region ConnecttoBrowser
    public class BrowserConnection
    {
        public static async Task<BiDiDriver> ConnectToBrowserAsync(int port = 9222)
        {
            // Try to discover WebSocket URL
            string webSocketUrl;

            try
            {
                webSocketUrl = await DiscoverWebSocketUrlAsync(port);
                Console.WriteLine($"Discovered WebSocket URL: {webSocketUrl}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to discover a browser WebSocket URL on port {port}. " +
                    "For Chromium-based browsers, query /json/version and use the returned devtools/browser URL. " +
                    "For Firefox via geckodriver, use ws://localhost:4444/session.",
                    ex);
            }

            // Create and start driver
            BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

            try
            {
                await driver.StartAsync(webSocketUrl);
                Console.WriteLine("Connected to browser successfully");
                return driver;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                throw;
            }
        }

        private static async Task<string> DiscoverWebSocketUrlAsync(int port)
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            string json = await client.GetStringAsync($"http://localhost:{port}/json/version");
            using JsonDocument doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("webSocketDebuggerUrl").GetString()
                ?? throw new Exception("WebSocket URL is null");
        }
    }
#endregion

    public static async Task ConnectToBrowserUsage()
    {
#region ConnectToBrowserUsage
        // Usage
        BiDiDriver driver = await BrowserConnection.ConnectToBrowserAsync(9222);
#endregion
    }

    /// <summary>
    /// First application - full flow.
    /// </summary>
    public static async Task FirstApplication()
    {
#region FirstApplication
        // Set the WebSocket URL for your browser
        string webSocketUrl = "ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID";

        // Create a driver with a 10-second command timeout
        // Using default WebSocket connection
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));

        try
        {
            // Connect to the browser
            Console.WriteLine("Connecting to browser...");
            await driver.StartAsync(webSocketUrl);
            Console.WriteLine("Connected!");

            // Get the current browsing contexts (tabs/windows)
            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                new GetTreeCommandParameters());
            
            string contextId = tree.ContextTree[0].BrowsingContextId;
            Console.WriteLine($"Active context ID: {contextId}");

            // Navigate to a webpage
            Console.WriteLine("Navigating to example.com...");
            NavigateCommandParameters navParams = new NavigateCommandParameters(
                contextId, 
                "https://example.com")
            {
                Wait = ReadinessState.Complete
            };
            
            NavigateCommandResult navResult = await driver.BrowsingContext.NavigateAsync(navParams);
            Console.WriteLine($"Navigation complete! URL: {navResult.Url}");

            // Execute JavaScript to get the page title
            EvaluateCommandParameters evalParams = new EvaluateCommandParameters(
                "document.title",
                new ContextTarget(contextId),
                true);
            
            EvaluateResult scriptResult = await driver.Script.EvaluateAsync(evalParams);
            
            if (scriptResult is EvaluateResultSuccess success &&
                success.Result is StringRemoteValue stringValue)
            {
                string title = stringValue.Value ?? "No title";
                Console.WriteLine($"Page title: {title}");
            }

            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            // Disconnect from the browser
            await driver.StopAsync();
            Console.WriteLine("Disconnected from browser");
        }
#endregion
    }

    /// <summary>
    /// Creating the driver.
    /// </summary>
    public static void CreatingTheDriver()
    {
#region CreatingtheDriver
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
#endregion
    }

    /// <summary>
    /// Connecting to the browser.
    /// </summary>
    public static async Task ConnectingToBrowser(BiDiDriver driver, string webSocketUrl)
    {
#region ConnectingtoBrowser
        await driver.StartAsync(webSocketUrl);
#endregion
    }

    /// <summary>
    /// Getting the browsing context.
    /// </summary>
    public static async Task<string> GettingBrowsingContext(BiDiDriver driver)
    {
#region GettingBrowsingContext
        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
            new GetTreeCommandParameters());
        string contextId = tree.ContextTree[0].BrowsingContextId;
        return contextId;
#endregion
    }

    /// <summary>
    /// Navigating to a URL.
    /// </summary>
    public static async Task Navigating(BiDiDriver driver, string contextId)
    {
#region Navigating
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
    /// Executing JavaScript.
    /// </summary>
    public static async Task ExecutingJavaScript(BiDiDriver driver, string contextId)
    {
#region ExecutingJavaScript
        EvaluateCommandParameters evalParams = new EvaluateCommandParameters(
            "document.title",
            new ContextTarget(contextId),
            true);

        EvaluateResult scriptResult = await driver.Script.EvaluateAsync(evalParams);
#endregion
    }
}
