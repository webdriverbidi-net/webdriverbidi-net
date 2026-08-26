// <copyright file="GettingStartedSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/getting-started.md

#pragma warning disable CS8600, CS8602

namespace WebDriverBiDi.Docs.Code.Examples;

using System.Net.Http;
using System.Text;
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
    public static async Task<string> CreateBiDiSessionAsync(string driverUrl = "http://localhost:9515")
    {
        using HttpClient client = new HttpClient();

        // Ask the driver for a WebDriver BiDi WebSocket for the session it creates
        const string body = """
            { "capabilities": { "alwaysMatch": { "webSocketUrl": true } } }
            """;
        using StringContent content = new StringContent(body, Encoding.UTF8, "application/json");

        try
        {
            using HttpResponseMessage response = await client.PostAsync($"{driverUrl}/session", content);
            string json = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement capabilities = doc.RootElement.GetProperty("value").GetProperty("capabilities");
            if (capabilities.TryGetProperty("webSocketUrl", out JsonElement urlElement))
            {
                string? webSocketUrl = urlElement.GetString();
                if (!string.IsNullOrEmpty(webSocketUrl))
                {
                    return webSocketUrl;
                }
            }

            throw new Exception("The driver did not return a webSocketUrl; the browser may not support WebDriver BiDi");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to reach the driver at {driverUrl}. " +
                "Ensure chromedriver (or msedgedriver/geckodriver) is running, e.g. chromedriver --port=9515", ex);
        }
    }
    #endregion

    public static async Task DiscoverUsage()
    {
        #region DiscoverWebSocketUrlUsage
        // Usage
        string webSocketUrl = await CreateBiDiSessionAsync("http://localhost:9515");
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync(webSocketUrl);
        // The session already exists; do not call driver.Session.NewSessionAsync
        #endregion
    }

    #region ConnecttoBrowser
    public class BrowserConnection
    {
        public static async Task<BiDiDriver> ConnectToBrowserAsync(string driverUrl = "http://localhost:9515")
        {
            // Create a session through the driver executable and get its BiDi endpoint
            string webSocketUrl;

            try
            {
                webSocketUrl = await CreateBiDiSessionAsync(driverUrl);
                Console.WriteLine($"Session created; WebDriver BiDi endpoint: {webSocketUrl}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create a WebDriver BiDi session through the driver at {driverUrl}. " +
                    "For Chrome or Edge, run chromedriver/msedgedriver and request the webSocketUrl capability. " +
                    "For Firefox, connect to ws://localhost:<port>/session and call Session.NewSessionAsync instead.",
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

        private static async Task<string> CreateBiDiSessionAsync(string driverUrl)
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            using StringContent content = new StringContent(
                """{ "capabilities": { "alwaysMatch": { "webSocketUrl": true } } }""",
                Encoding.UTF8,
                "application/json");
            using HttpResponseMessage response = await client.PostAsync($"{driverUrl}/session", content);
            response.EnsureSuccessStatusCode();

            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement.GetProperty("value").GetProperty("capabilities").GetProperty("webSocketUrl").GetString()
                ?? throw new Exception("webSocketUrl is null");
        }
    }
    #endregion

    public static async Task ConnectToBrowserUsage()
    {
        #region ConnectToBrowserUsage
        // Usage
        BiDiDriver driver = await BrowserConnection.ConnectToBrowserAsync("http://localhost:9515");
        #endregion
    }

    /// <summary>
    /// First application - full flow.
    /// </summary>
    public static async Task FirstApplication()
    {
        #region FirstApplication
        // The webSocketUrl returned when you created the session through chromedriver
        string webSocketUrl = "ws://localhost:9515/session/YOUR-SESSION-ID";

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
