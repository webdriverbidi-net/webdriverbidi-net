// <copyright file="NetworkInterceptionSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/examples/network-interception.md

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace WebDriverBiDi.Docs.Code.Examples;

using System.Collections.Generic;
using System.Linq;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Network;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for network interception documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class NetworkInterceptionSamples
{
    /// <summary>
    /// Example 1: Basic network interception.
    /// </summary>
    public static async Task BasicNetworkInterception()
    {
#region BasicNetworkInterception
        string webSocketUrl = "ws://localhost:9222/devtools/browser/YOUR-ID-HERE";
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        try
        {
            await driver.StartAsync(webSocketUrl);
            Console.WriteLine("Connected to browser");

            // Subscribe to network events
            SubscribeCommandParameters subscribe = new SubscribeCommandParameters(
                [
                    driver.Network.OnBeforeRequestSent.EventName,
                    driver.Network.OnResponseCompleted.EventName
                ]
            );
            await driver.Session.SubscribeAsync(subscribe);

            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                new GetTreeCommandParameters());
            string contextId = tree.ContextTree[0].BrowsingContextId;

            // Set up intercept for all requests
            Console.WriteLine("Setting up network intercept...");
            AddInterceptCommandParameters addIntercept = new AddInterceptCommandParameters();
            addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
            addIntercept.BrowsingContextIds = new List<string> { contextId };
            
            AddInterceptCommandResult interceptResult = 
                await driver.Network.AddInterceptAsync(addIntercept);
            Console.WriteLine($"Intercept ID: {interceptResult.InterceptId}");

            // Handle intercepted requests
            driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
            {
                if (e.IsBlocked)
                {
                    Console.WriteLine($"Intercepted: {e.Request.Method} {e.Request.Url}");

                    // Continue request (allow it through)
                    ContinueRequestCommandParameters continueParams = 
                        new ContinueRequestCommandParameters(e.Request.RequestId);
                    
                    await driver.Network.ContinueRequestAsync(continueParams);
                }
                else
                {
                    Console.WriteLine($"Request: {e.Request.Method} {e.Request.Url}");
                }
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

            // Navigate
            Console.WriteLine("\nNavigating to example.com...");
            await driver.BrowsingContext.NavigateAsync(
                new NavigateCommandParameters(contextId, "https://example.com")
                { Wait = ReadinessState.Complete });

            Console.WriteLine("\n✓ Navigation complete");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();

            // Clean up
            await driver.Network.RemoveInterceptAsync(
                new RemoveInterceptCommandParameters(interceptResult.InterceptId));
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
    /// Example 2: Blocking ad domains.
    /// </summary>
    public static async Task BlockingAdDomains(BiDiDriver driver)
    {
#region BlockingAdDomains
        // List of ad domains to block
        List<string> adDomains = new List<string>
        {
            "doubleclick.net",
            "googlesyndication.com",
            "google-analytics.com",
            "facebook.com",
            "twitter.com"
        };

        // Set up intercept with URL patterns
        AddInterceptCommandParameters addIntercept = new AddInterceptCommandParameters();
        addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);

        foreach (string domain in adDomains)
        {
            addIntercept.UrlPatterns.Add(new UrlPatternPattern 
            { 
                HostName = domain 
            });
        }

        await driver.Network.AddInterceptAsync(addIntercept);

        // Block matching requests
        driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
        {
            if (e.IsBlocked)
            {
                Console.WriteLine($"🚫 Blocking: {e.Request.Url}");
                
                FailRequestCommandParameters failParams = 
                    new FailRequestCommandParameters(e.Request.RequestId);
                
                await driver.Network.FailRequestAsync(failParams);
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);

        Console.WriteLine("Ad blocking enabled");
#endregion
    }

    /// <summary>
    /// Example 3: Mocking API responses.
    /// </summary>
    public static async Task MockingApiResponses(BiDiDriver driver)
    {
#region MockingAPIResponses
        // Set up intercept for API endpoints
        AddInterceptCommandParameters addIntercept = new AddInterceptCommandParameters();
        addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
        addIntercept.UrlPatterns = new List<UrlPattern>
        {
            new UrlPatternPattern { PathName = "/api/users" },
            new UrlPatternPattern { PathName = "/api/settings" }
        };

        await driver.Network.AddInterceptAsync(addIntercept);

        // Mock API responses
        Dictionary<string, string> mockResponses = new Dictionary<string, string>
        {
            { "/api/users", @"{
                ""users"": [
                    {""id"": 1, ""name"": ""Alice"", ""email"": ""alice@example.com""},
                    {""id"": 2, ""name"": ""Bob"", ""email"": ""bob@example.com""}
                ]
            }" },
            { "/api/settings", @"{
                ""theme"": ""dark"",
                ""language"": ""en"",
                ""notifications"": true
            }" }
        };

        driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
        {
            if (e.IsBlocked)
            {
                Uri uri = new Uri(e.Request.Url);
                string path = uri.AbsolutePath;

                if (mockResponses.TryGetValue(path, out string mockData))
                {
                    Console.WriteLine($"🎭 Mocking: {path}");

                    ProvideResponseCommandParameters provideParams = 
                        new ProvideResponseCommandParameters(e.Request.RequestId)
                        {
                            StatusCode = 200,
                            ReasonPhrase = "OK",
                            Body = BytesValue.FromString(mockData)
                        };

                    provideParams.Headers =
                    [
                        new Header("Content-Type", "application/json"),
                        new Header("X-Mocked", "true"),
                    ];

                    await driver.Network.ProvideResponseAsync(provideParams);
                }
                else
                {
                    // Not a mocked endpoint, continue normally
                    await driver.Network.ContinueRequestAsync(
                        new ContinueRequestCommandParameters(e.Request.RequestId));
                }
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);

        Console.WriteLine("API mocking enabled");
#endregion
    }

    /// <summary>
    /// Example 4: Adding custom headers.
    /// </summary>
    public static async Task AddingCustomHeaders(BiDiDriver driver)
    {
#region AddingCustomHeaders
        // Set up intercept
        AddInterceptCommandParameters addIntercept = new AddInterceptCommandParameters();
        addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
        await driver.Network.AddInterceptAsync(addIntercept);

        // Add custom headers to all requests
        driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
        {
            if (e.IsBlocked)
            {
                Console.WriteLine($"Adding headers to: {e.Request.Url}");

                ContinueRequestCommandParameters continueParams = 
                    new ContinueRequestCommandParameters(e.Request.RequestId);

                // Copy existing headers
                continueParams.Headers = e.Request.Headers
                    .Select(h => new Header(h.Name, h.Value.Value))
                    .ToList();

                // Add custom headers
                continueParams.Headers.Add(new Header("X-Custom-Header", "MyValue"));
                continueParams.Headers.Add(new Header("X-Request-ID", Guid.NewGuid().ToString()));

                // Add authorization header
                continueParams.Headers.Add(new Header("Authorization", "Bearer my-token-123"));

                await driver.Network.ContinueRequestAsync(continueParams);
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);
#endregion
    }

    /// <summary>
    /// Example 5: Capturing response bodies.
    /// </summary>
    public static async Task CapturingResponseBodies(
        BiDiDriver driver,
        string contextId)
    {
#region CapturingResponseBodies
        // Set up data collector
        Console.WriteLine("Setting up data collector...");
        ulong maxSize = Convert.ToUInt64(Math.Pow(2, 24)); // 16 MB
        AddDataCollectorCommandParameters collectorParams = 
            new AddDataCollectorCommandParameters(maxSize);
        collectorParams.BrowsingContexts.Add(contextId);

        AddDataCollectorCommandResult collectorResult = 
            await driver.Network.AddDataCollectorAsync(collectorParams);
        string collectorId = collectorResult.CollectorId;
        Console.WriteLine($"Data collector ID: {collectorId}");

        // Subscribe to response events
        SubscribeCommandParameters subscribe = 
            new SubscribeCommandParameters(driver.Network.OnResponseCompleted.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        // Capture response bodies
        Dictionary<string, string> responseBodies = new Dictionary<string, string>();

        driver.Network.OnResponseCompleted.AddObserver(async (ResponseCompletedEventArgs e) =>
        {
            // Only capture JSON responses
            var contentType = e.Response.Headers
                .FirstOrDefault(h => h.Name.Equals("content-type", StringComparison.OrdinalIgnoreCase));

            if (contentType != null && contentType.Value.Value.Contains("application/json"))
            {
                Console.WriteLine($"📥 Capturing response from: {e.Response.Url}");

                try
                {
                    GetDataCommandParameters getDataParams = 
                        new GetDataCommandParameters(e.Request.RequestId)
                        {
                            CollectorId = collectorId,
                            DisownCollectedData = true
                        };

                    GetDataCommandResult dataResult = 
                        await driver.Network.GetDataAsync(getDataParams);

                    string body = dataResult.Bytes.Value;
                    responseBodies[e.Response.Url] = body;

                    Console.WriteLine($"Body preview: {body.Substring(0, Math.Min(100, body.Length))}...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error capturing body: {ex.Message}");
                }
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);

        // Navigate and collect responses
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://jsonplaceholder.typicode.com/users")
            { Wait = ReadinessState.Complete });

        await Task.Delay(2000); // Wait for async handlers

        Console.WriteLine($"\n✓ Captured {responseBodies.Count} JSON responses");
        foreach (var kvp in responseBodies)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value.Length} bytes");
        }

        // Clean up
        await driver.Network.RemoveDataCollectorAsync(
            new RemoveDataCollectorCommandParameters(collectorId));
#endregion
    }

    /// <summary>
    /// Example 6: Slow down specific resources.
    /// </summary>
    public static async Task SlowDownSpecificResources(BiDiDriver driver)
    {
#region SlowDownSpecificResources
        // Intercept image requests
        AddInterceptCommandParameters addIntercept = new AddInterceptCommandParameters();
        addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
        addIntercept.UrlPatterns = new List<UrlPattern>
        {
            new UrlPatternString("*.jpg"),
            new UrlPatternString("*.png"),
            new UrlPatternString("*.gif")
        };

        await driver.Network.AddInterceptAsync(addIntercept);

        // Add delay to image requests
        driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
        {
            if (e.IsBlocked)
            {
                Console.WriteLine($"⏱️ Delaying image request: {e.Request.Url}");
                
                // Simulate slow network
                await Task.Delay(2000);

                // Continue request
                await driver.Network.ContinueRequestAsync(
                    new ContinueRequestCommandParameters(e.Request.RequestId));
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);

        Console.WriteLine("Network throttling enabled for images");
#endregion
    }

    /// <summary>
    /// Example 7: Redirect requests.
    /// </summary>
    public static async Task RedirectRequests(BiDiDriver driver)
    {
#region RedirectRequests
        // Intercept requests to old domain
        AddInterceptCommandParameters addIntercept = new AddInterceptCommandParameters();
        addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
        addIntercept.UrlPatterns = new List<UrlPattern>
        {
            new UrlPatternPattern { HostName = "old-api.example.com" }
        };

        await driver.Network.AddInterceptAsync(addIntercept);

        // Redirect to new domain
        driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
        {
            if (e.IsBlocked)
            {
                string oldUrl = e.Request.Url;
                string newUrl = oldUrl.Replace("old-api.example.com", "new-api.example.com");
                
                Console.WriteLine($"↪️ Redirecting: {oldUrl} → {newUrl}");

                ContinueRequestCommandParameters continueParams = 
                    new ContinueRequestCommandParameters(e.Request.RequestId);
                continueParams.Url = newUrl;

                await driver.Network.ContinueRequestAsync(continueParams);
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);
#endregion
    }

    /// <summary>
    /// Pattern: Conditional interception.
    /// </summary>
    public static async Task ConditionalInterception(BiDiDriver driver, string contextId)
    {
#region ConditionalInterception
        // Intercept only on specific pages
        AddInterceptCommandParameters addIntercept = new AddInterceptCommandParameters();
        addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
        addIntercept.BrowsingContextIds = new List<string> { contextId };

        await driver.Network.AddInterceptAsync(addIntercept);

        driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
        {
            if (e.IsBlocked)
            {
                // Only intercept requests from specific page
                if (e.Request.Url.Contains("example.com/api/"))
                {
                    // Apply interception logic
                    Console.WriteLine($"🔍 Intercepting API call: {e.Request.Url}");
                    // ... handle request
                }
                else
                {
                    // Let other requests through unchanged
                    await driver.Network.ContinueRequestAsync(
                        new ContinueRequestCommandParameters(e.Request.RequestId));
                }
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);
#endregion
    }
}
