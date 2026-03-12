// <copyright file="NetworkModuleSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/network.md and docs/articles/examples/network-interception.md

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace WebDriverBiDi.Docs.Code.Modules;

using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Network;
using WebDriverBiDi.Session;
using WebDriverBiDi.Storage;

/// <summary>
/// Snippets for Network module documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class NetworkModuleSamples
{
    /// <summary>
    /// Accessing the module.
    /// </summary>
    public static void AccessingModule(BiDiDriver driver)
    {
#region AccessingModule
        NetworkModule network = driver.Network;
#endregion
    }

    /// <summary>
    /// Basic response monitoring.
    /// </summary>
    public static async Task BasicResponseMonitoring(
        BiDiDriver driver,
        string contextId,
        string url)
    {
#region BasicResponseMonitoring
        // Add observer
        driver.Network.OnResponseCompleted.AddObserver((ResponseCompletedEventArgs e) =>
        {
            Console.WriteLine($"URL: {e.Response.Url}");
            Console.WriteLine($"Status: {e.Response.Status} {e.Response.StatusText}");
        });

        // Subscribe to events
        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Network.OnResponseCompleted.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        // Navigate - events will fire for all requests
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com"));
#endregion
    }

    /// <summary>
    /// Monitor request details.
    /// </summary>
    public static async Task MonitorRequestDetails(BiDiDriver driver)
    {
#region MonitorRequestDetails
        driver.Network.OnBeforeRequestSent.AddObserver((BeforeRequestSentEventArgs e) =>
        {
            Console.WriteLine($"Request: {e.Request.Method} {e.Request.Url}");
            Console.WriteLine("Headers:");
            foreach (ReadOnlyHeader header in e.Request.Headers)
            {
                Console.WriteLine($"  {header.Name}: {header.Value.Value}");
            }
        });

        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Network.OnBeforeRequestSent.EventName);
        await driver.Session.SubscribeAsync(subscribe);
#endregion
    }

    /// <summary>
    /// Filter by content type.
    /// </summary>
    public static void FilterByContentType(BiDiDriver driver)
    {
#region FilterbyContentType
        // Find Content-Type header
        driver.Network.OnResponseCompleted.AddObserver((ResponseCompletedEventArgs e) =>
        {
            ReadOnlyHeader? contentType = e.Response.Headers
                .FirstOrDefault(h => h.Name.Equals("content-type", StringComparison.OrdinalIgnoreCase));

            if (contentType != null && contentType.Value.Value.Contains("application/json"))
            {
                Console.WriteLine($"JSON response from: {e.Response.Url}");
            }
        });
#endregion
    }

    /// <summary>
    /// Filter by URL pattern.
    /// </summary>
    public static void FilterByUrlPattern(BiDiDriver driver)
    {
#region FilterbyURLPattern
        driver.Network.OnBeforeRequestSent.AddObserver((BeforeRequestSentEventArgs e) =>
        {
            if (e.Request.Url.Contains("/api/"))
            {
                Console.WriteLine($"API call: {e.Request.Url}");
            }
        });
#endregion
    }

    /// <summary>
    /// BeforeRequestSent event.
    /// </summary>
    public static void BeforeRequestSent(BiDiDriver driver)
    {
#region BeforeRequestSent
        driver.Network.OnBeforeRequestSent.AddObserver((BeforeRequestSentEventArgs e) =>
        {
            Console.WriteLine($"Method: {e.Request.Method}");
            Console.WriteLine($"URL: {e.Request.Url}");
            Console.WriteLine($"Request ID: {e.Request.RequestId}");
            Console.WriteLine($"Timestamp: {e.Request.Timings.TimeOrigin}");
            Console.WriteLine($"Is Blocked: {e.IsBlocked}");
        });
#endregion
    }

    /// <summary>
    /// ResponseStarted event.
    /// </summary>
    public static void ResponseStarted(BiDiDriver driver)
    {
#region ResponseStarted
        driver.Network.OnResponseStarted.AddObserver((ResponseStartedEventArgs e) =>
        {
            Console.WriteLine($"Status: {e.Response.Status}");
            Console.WriteLine($"Headers received for: {e.Response.Url}");
        });
#endregion
    }

    /// <summary>
    /// ResponseCompleted event.
    /// </summary>
    public static void ResponseCompleted(BiDiDriver driver)
    {
#region ResponseCompleted
        driver.Network.OnResponseCompleted.AddObserver((ResponseCompletedEventArgs e) =>
        {
            Console.WriteLine($"Response complete: {e.Response.Url}");
            Console.WriteLine($"Bytes received: {e.Response.BytesReceived}");
        });
#endregion
    }

    /// <summary>
    /// FetchError event.
    /// </summary>
    public static void FetchError(BiDiDriver driver)
    {
#region FetchError
        driver.Network.OnFetchError.AddObserver((FetchErrorEventArgs e) =>
        {
            Console.WriteLine($"Network error for: {e.Request.Url}");
            Console.WriteLine($"Error: {e.ErrorText}");
        });
#endregion
    }

    /// <summary>
    /// AuthRequired event.
    /// </summary>
    public static void AuthRequired(BiDiDriver driver)
    {
#region AuthRequired
        driver.Network.OnAuthRequired.AddObserver(async (AuthRequiredEventArgs e) =>
        {
            // Provide credentials
            ContinueWithAuthCommandParameters parameters =
                new ContinueWithAuthCommandParameters(e.Request.RequestId)
                {
                    Action = ContinueWithAuthActionType.ProvideCredentials,
                    Credentials = new AuthCredentials("myuser", "mypassword"),
                };

            await driver.Network.ContinueWithAuthAsync(parameters);
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);
#endregion
    }

    /// <summary>
    /// Add intercept.
    /// </summary>
    public static async Task AddIntercept(BiDiDriver driver, string contextId)
    {
#region AddIntercept
        AddInterceptCommandParameters parameters = new AddInterceptCommandParameters();

        // Specify which phase to intercept
        parameters.Phases.Add(InterceptPhase.BeforeRequestSent);

        // Optional: limit to specific contexts
        parameters.BrowsingContextIds = new List<string> { contextId };

        // Optional: URL patterns to intercept
        parameters.UrlPatterns = new List<UrlPattern>
        {
            new UrlPatternPattern { HostName = "example.com" }
        };

        AddInterceptCommandResult result = await driver.Network.AddInterceptAsync(parameters);
        string interceptId = result.InterceptId;
#endregion
    }

    /// <summary>
    /// Intercept specific URLs with UrlPattern.
    /// </summary>
    public static async Task InterceptSpecificUrls(BiDiDriver driver)
    {
#region InterceptSpecificURLs
        AddInterceptCommandParameters parameters = new AddInterceptCommandParameters();
        parameters.Phases.Add(InterceptPhase.BeforeRequestSent);

        // Intercept all .jpg images
        parameters.UrlPatterns = new List<UrlPattern>
        {
            new UrlPatternString("*.jpg"),
        };

        await driver.Network.AddInterceptAsync(parameters);
#endregion
    }

    /// <summary>
    /// Block requests with FailRequest.
    /// </summary>
    public static async Task BlockRequests(BiDiDriver driver)
    {
#region BlockRequests
        // Add intercept
        AddInterceptCommandParameters addIntercept = new AddInterceptCommandParameters();
        addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
        addIntercept.UrlPatterns = new List<UrlPattern>
        {
            new UrlPatternPattern { HostName = "ads.example.com" },
        };
        await driver.Network.AddInterceptAsync(addIntercept);

        // Handle intercepted requests
        driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
        {
            if (e.IsBlocked)
            {
                // Fail the request
                FailRequestCommandParameters failParams =
                    new FailRequestCommandParameters(e.Request.RequestId);

                await driver.Network.FailRequestAsync(failParams);
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);
#endregion
    }

    /// <summary>
    /// Continue request with optional header modification.
    /// </summary>
    public static async Task ContinueRequest(BiDiDriver driver)
    {
#region ContinueRequest
        driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
        {
            if (e.IsBlocked)
            {
                // Optionally modify request
                ContinueRequestCommandParameters parameters =
                    new ContinueRequestCommandParameters(e.Request.RequestId);

                // Add custom header
                parameters.Headers = new List<Header>();
                foreach (ReadOnlyHeader readOnlyHeader in e.Request.Headers)
                {
                    parameters.Headers.Add(new Header(readOnlyHeader.Name, readOnlyHeader.Value.Value));
                }
                parameters.Headers.Add(new Header("X-Custom-Header", "MyValue"));

                await driver.Network.ContinueRequestAsync(parameters);
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);
#endregion
    }

    /// <summary>
    /// Provide custom response for intercepted request.
    /// </summary>
    public static async Task ProvideCustomResponse(BiDiDriver driver)
    {
#region ProvideCustomResponse
        driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
        {
            if (e.IsBlocked && e.Request.Url.Contains("/api/data"))
            {
                // Return custom JSON response
                string jsonResponse = "{\"message\": \"Mocked response\"}";

                ProvideResponseCommandParameters parameters =
                    new ProvideResponseCommandParameters(e.Request.RequestId)
                    {
                        StatusCode = 200,
                        ReasonPhrase = "OK",
                        Body = BytesValue.FromString(jsonResponse),
                    };

                parameters.Headers =
                [
                    new Header("Content-Type", "application/json"),
                ];

                await driver.Network.ProvideResponseAsync(parameters);
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);
#endregion
    }

    /// <summary>
    /// Remove intercept.
    /// </summary>
    public static async Task RemoveIntercept(BiDiDriver driver, string interceptId)
    {
#region RemoveIntercept
        RemoveInterceptCommandParameters parameters =
            new RemoveInterceptCommandParameters(interceptId);

        await driver.Network.RemoveInterceptAsync(parameters);
#endregion
    }

    /// <summary>
    /// Create data collector.
    /// </summary>
    public static async Task CreateDataCollector(BiDiDriver driver, string contextId)
    {
#region CreateDataCollector
        // Allocate memory for data collection (in bytes)
        ulong maxSize = Convert.ToUInt64(Math.Pow(2, 24));  // 16 MB

        AddDataCollectorCommandParameters parameters =
            new AddDataCollectorCommandParameters(maxSize);

        parameters.BrowsingContexts.Add(contextId);

        AddDataCollectorCommandResult result =
            await driver.Network.AddDataCollectorAsync(parameters);

        string collectorId = result.CollectorId;
#endregion
    }

    /// <summary>
    /// Get response body from data collector.
    /// </summary>
    public static async Task GetResponseBody(
        BiDiDriver driver,
        string collectorId,
        List<string> capturedBodies)
    {
#region GetResponseBody
        driver.Network.OnResponseCompleted.AddObserver(async (ResponseCompletedEventArgs e) =>
        {
            // Only capture specific responses
            if (e.Response.Url.EndsWith(".json"))
            {
                GetDataCommandParameters getDataParams =
                    new GetDataCommandParameters(e.Request.RequestId)
                    {
                        CollectorId = collectorId,
                        DisownCollectedData = true,  // Free memory after retrieval
                    };

                GetDataCommandResult dataResult =
                    await driver.Network.GetDataAsync(getDataParams);

                string body = dataResult.Bytes.Value;
                capturedBodies.Add(body);
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);
#endregion
    }

    /// <summary>
    /// Remove data collector.
    /// </summary>
    public static async Task RemoveDataCollector(BiDiDriver driver, string collectorId)
    {
#region RemoveDataCollector
        RemoveDataCollectorCommandParameters parameters =
            new RemoveDataCollectorCommandParameters(collectorId);

        await driver.Network.RemoveDataCollectorAsync(parameters);
#endregion
    }

    /// <summary>
    /// Reading headers.
    /// </summary>
    public static void ReadingHeaders(BiDiDriver driver)
    {
#region ReadingHeaders
        driver.Network.OnResponseCompleted.AddObserver((ResponseCompletedEventArgs e) =>
        {
            foreach (ReadOnlyHeader header in e.Response.Headers)
            {
                string name = header.Name;
                string value = header.Value.Value;
                Console.WriteLine($"{name}: {value}");
            }
        });
#endregion
    }

    /// <summary>
    /// Setting custom headers via intercept.
    /// </summary>
    public static void SettingCustomHeaders(BiDiDriver driver)
    {
#region SettingCustomHeaders
        driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
        {
            if (e.IsBlocked)
            {
                ContinueRequestCommandParameters parameters =
                    new ContinueRequestCommandParameters(e.Request.RequestId);

                // Copy existing headers
                parameters.Headers = new List<Header>();
                foreach (ReadOnlyHeader readOnlyHeader in e.Request.Headers)
                {
                    parameters.Headers.Add(new Header(readOnlyHeader.Name, readOnlyHeader.Value.Value));
                }

                // Add authorization header
                parameters.Headers.Add(new Header("Authorization", "Bearer mytoken"));

                await driver.Network.ContinueRequestAsync(parameters);
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);
#endregion
    }

    /// <summary>
    /// Set global extra headers.
    /// </summary>
    public static async Task SetGlobalExtraHeaders(BiDiDriver driver, string contextId)
    {
#region SetGlobalExtraHeaders
        SetExtraHeadersCommandParameters parameters = new SetExtraHeadersCommandParameters
        {
            Headers =
            [
                "Authorization: Bearer mytoken",
                "X-API-Key: my-api-key",
            ],
            Contexts = new List<string> { contextId },
        };

        await driver.Network.SetExtraHeadersAsync(parameters);
#endregion
    }

    /// <summary>
    /// Clear extra headers.
    /// </summary>
    public static async Task ClearExtraHeaders(BiDiDriver driver)
    {
#region ClearExtraHeaders
        SetExtraHeadersCommandParameters parameters =
            SetExtraHeadersCommandParameters.ResetExtraHeaders;

        await driver.Network.SetExtraHeadersAsync(parameters);
#endregion
    }

    /// <summary>
    /// Bypass cache for network requests.
    /// </summary>
    public static async Task BypassCache(BiDiDriver driver, string contextId)
    {
#region BypassCache
        SetCacheBehaviorCommandParameters parameters =
            new SetCacheBehaviorCommandParameters(CacheBehavior.Bypass)
            {
                Contexts = new List<string> { contextId },
            };

        await driver.Network.SetCacheBehaviorAsync(parameters);
#endregion
    }

    /// <summary>
    /// Restore default cache behavior.
    /// </summary>
    public static async Task RestoreDefaultCacheBehavior(BiDiDriver driver, string contextId)
    {
#region RestoreDefaultCacheBehavior
        SetCacheBehaviorCommandParameters parameters =
            new SetCacheBehaviorCommandParameters(CacheBehavior.Default)
            {
                Contexts = new List<string> { contextId },
            };

        await driver.Network.SetCacheBehaviorAsync(parameters);
#endregion
    }

    /// <summary>
    /// Add cookie via Storage module.
    /// </summary>
    public static async Task AddCookie(BiDiDriver driver)
    {
#region AddCookie
        // Use Storage module for cookies
        SetCookieCommandParameters parameters = new SetCookieCommandParameters(
            new PartialCookie("sessionId", BytesValue.FromString("abc123"), "example.com")
            {
                Path = "/",
                Secure = true,
                HttpOnly = true,
                SameSite = CookieSameSiteValue.Strict,
            });

        await driver.Storage.SetCookieAsync(parameters);
#endregion
    }

    /// <summary>
    /// Get cookies via Storage module.
    /// </summary>
    public static async Task GetCookies(BiDiDriver driver, string contextId)
    {
#region GetCookies
        GetCookiesCommandParameters parameters = new GetCookiesCommandParameters();
        parameters.Partition = new BrowsingContextPartitionDescriptor(contextId);

        GetCookiesCommandResult result = await driver.Storage.GetCookiesAsync(parameters);

        foreach (Cookie cookie in result.Cookies)
        {
            Console.WriteLine($"{cookie.Name}: {cookie.Value.Value}");
            Console.WriteLine($"  Domain: {cookie.Domain}");
            Console.WriteLine($"  Expires: {cookie.Expires}");
        }
#endregion
    }

    /// <summary>
    /// Timing information.
    /// </summary>
    public static void TimingInformation(BiDiDriver driver)
    {
#region TimingInformation
        driver.Network.OnResponseCompleted.AddObserver((ResponseCompletedEventArgs e) =>
        {
            FetchTimingInfo timings = e.Request.Timings;

            Console.WriteLine($"Time origin: {timings.TimeOrigin}");
            Console.WriteLine($"Request time: {timings.RequestTime}");
            Console.WriteLine($"Response start: {timings.ResponseStart}");
            Console.WriteLine($"Response end: {timings.ResponseEnd}");
        });
#endregion
    }

    /// <summary>
    /// Pattern: Collect all requests.
    /// </summary>
    public static async Task CollectAllRequests(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
#region CollectAllRequests
        List<RequestData> allRequests = new List<RequestData>();

        driver.Network.OnBeforeRequestSent.AddObserver((BeforeRequestSentEventArgs e) =>
        {
            allRequests.Add(e.Request);
        });

        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Network.OnBeforeRequestSent.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        await driver.BrowsingContext.NavigateAsync(navParams);

        // Wait for requests to complete
        await Task.Delay(2000);

        Console.WriteLine($"Total requests: {allRequests.Count}");
        foreach (var request in allRequests)
        {
            Console.WriteLine($"  {request.Method} {request.Url}");
        }
#endregion
    }

    /// <summary>
    /// Pattern: Block ad domains.
    /// </summary>
    public static async Task BlockAdDomains(BiDiDriver driver)
    {
#region BlockAdDomains
        List<string> adDomains = new List<string> 
        { 
            "ads.example.com", 
            "tracker.example.com" 
        };

        AddInterceptCommandParameters addIntercept = new AddInterceptCommandParameters();
        addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);

        foreach (string domain in adDomains)
        {
            addIntercept.UrlPatterns.Add(
                new UrlPatternPattern { HostName = domain });
        }

        await driver.Network.AddInterceptAsync(addIntercept);

        driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
        {
            if (e.IsBlocked)
            {
                Console.WriteLine($"Blocking: {e.Request.Url}");
                await driver.Network.FailRequestAsync(
                    new FailRequestCommandParameters(e.Request.RequestId));
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);
#endregion
    }

    /// <summary>
    /// Pattern: Mock API responses.
    /// </summary>
    public static async Task MockApiResponses(BiDiDriver driver)
    {
#region MockAPIResponses
        Dictionary<string, string> mockResponses = new Dictionary<string, string>
        {
            { "/api/user", "{\"name\": \"Test User\", \"id\": 123}" },
            { "/api/settings", "{\"theme\": \"dark\", \"lang\": \"en\"}" },
        };

        AddInterceptCommandParameters addIntercept = new AddInterceptCommandParameters();
        addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
        await driver.Network.AddInterceptAsync(addIntercept);

        driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
        {
            if (e.IsBlocked)
            {
                string path = new Uri(e.Request.Url).AbsolutePath;

                if (mockResponses.TryGetValue(path, out string? mockData))
                {
                    ProvideResponseCommandParameters parameters =
                        new ProvideResponseCommandParameters(e.Request.RequestId)
                        {
                            StatusCode = 200,
                            Body = BytesValue.FromString(mockData),
                        };

                    parameters.Headers =
                    [
                        new Header("Content-Type", "application/json"),
                    ];

                    await driver.Network.ProvideResponseAsync(parameters);
                }
                else
                {
                    await driver.Network.ContinueRequestAsync(
                        new ContinueRequestCommandParameters(e.Request.RequestId));
                }
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);
#endregion
    }

    /// <summary>
    /// Pattern: Capture complete HTTP transaction.
    /// </summary>
    public static async Task CaptureCompleteHttpTransaction(
        BiDiDriver driver,
        string contextId)
    {
#region CaptureCompleteHTTPTransaction
        Dictionary<string, HttpTransaction> transactions =
            new Dictionary<string, HttpTransaction>();

        // Set up data collector
        AddDataCollectorCommandParameters addParams =
            new AddDataCollectorCommandParameters(Convert.ToUInt64(Math.Pow(2, 26)));
        addParams.BrowsingContexts.Add(contextId);

        AddDataCollectorCommandResult collectorResult =
            await driver.Network.AddDataCollectorAsync(addParams);
        string collectorId = collectorResult.CollectorId;

        // Capture requests
        driver.Network.OnBeforeRequestSent.AddObserver((BeforeRequestSentEventArgs e) =>
        {
            transactions[e.Request.RequestId] = new HttpTransaction
            {
                Request = e.Request,
            };
        });

        // Capture responses and bodies
        driver.Network.OnResponseCompleted.AddObserver(async (ResponseCompletedEventArgs e) =>
        {
            if (transactions.TryGetValue(e.Request.RequestId, out HttpTransaction? transaction))
            {
                transaction.Response = e.Response;

                // Get response body
                GetDataCommandParameters getDataParams =
                    new GetDataCommandParameters(e.Request.RequestId)
                    {
                        CollectorId = collectorId,
                        DisownCollectedData = true,
                    };

                GetDataCommandResult dataResult =
                    await driver.Network.GetDataAsync(getDataParams);

                transaction.ResponseBody = dataResult.Bytes.Value;
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);
    }
#endregion

    /// <summary>
    /// Basic network interception setup.
    /// </summary>
    public static async Task BasicInterceptionSetup(
        BiDiDriver driver,
        string contextId,
        string url)
    {
        SubscribeCommandParameters subscribe = new SubscribeCommandParameters(
            [
                driver.Network.OnBeforeRequestSent.EventName,
                driver.Network.OnResponseCompleted.EventName,
            ]
        );
        await driver.Session.SubscribeAsync(subscribe);

        AddInterceptCommandParameters addIntercept = new AddInterceptCommandParameters();
        addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
        addIntercept.BrowsingContextIds = new List<string> { contextId };

        AddInterceptCommandResult interceptResult = await driver.Network.AddInterceptAsync(addIntercept);

        driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
        {
            if (e.IsBlocked)
            {
                ContinueRequestCommandParameters continueParams =
                    new ContinueRequestCommandParameters(e.Request.RequestId);

                await driver.Network.ContinueRequestAsync(continueParams);
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, url) { Wait = ReadinessState.Complete });

        await driver.Network.RemoveInterceptAsync(
            new RemoveInterceptCommandParameters(interceptResult.InterceptId));
    }
}

/// <summary>
/// Helper class for HTTP transaction capture pattern.
/// </summary>
public class HttpTransaction
{
    public RequestData Request { get; set; } = null!;
    public ResponseData? Response { get; set; }
    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
}

#pragma warning restore CS8602 // Dereference of a possibly null reference.
