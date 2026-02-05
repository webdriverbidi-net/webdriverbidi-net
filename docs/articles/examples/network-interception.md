# Network Interception Example

This example demonstrates how to intercept, modify, and replace network requests and responses using WebDriverBiDi.NET-Relaxed.

## Overview

This example shows:
- Intercepting network requests before they're sent
- Blocking specific requests (ad blocking)
- Providing custom responses (mocking APIs)
- Modifying request headers
- Capturing request and response bodies

## Example 1: Basic Network Interception

```csharp
using System;
using System.Threading.Tasks;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Network;
using WebDriverBiDi.Session;

namespace NetworkInterceptionExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string webSocketUrl = "ws://localhost:9222/devtools/browser/YOUR-ID-HERE";
            BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

            try
            {
                await driver.StartAsync(webSocketUrl);
                Console.WriteLine("Connected to browser");

                // Subscribe to network events
                SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
                subscribe.Events.Add(driver.Network.OnBeforeRequestSent.EventName);
                subscribe.Events.Add(driver.Network.OnResponseCompleted.EventName);
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
        }
    }
}
```

## Example 2: Blocking Ad Domains

```csharp
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
        Hostname = domain 
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
```

## Example 3: Mocking API Responses

```csharp
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

            provideParams.Headers.Add(new Header("Content-Type",
                new BytesValue(BytesValueType.String, "application/json")));
            provideParams.Headers.Add(new Header("X-Mocked",
                new BytesValue(BytesValueType.String, "true")));

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
```

## Example 4: Adding Custom Headers

```csharp
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
        continueParams.Headers = new List<Header>(e.Request.Headers);

        // Add custom headers
        continueParams.Headers.Add(new Header("X-Custom-Header",
            new BytesValue(BytesValueType.String, "MyValue")));
        continueParams.Headers.Add(new Header("X-Request-ID",
            new BytesValue(BytesValueType.String, Guid.NewGuid().ToString())));

        // Add authorization header
        continueParams.Headers.Add(new Header("Authorization",
            new BytesValue(BytesValueType.String, "Bearer my-token-123")));

        await driver.Network.ContinueRequestAsync(continueParams);
    }
},
ObservableEventHandlerOptions.RunHandlerAsynchronously);
```

## Example 5: Capturing Response Bodies

```csharp
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
SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.Network.OnResponseCompleted.EventName);
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

            string body = dataResult.Bytes.GetStringValue();
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
```

## Example 6: Slow Down Specific Resources

```csharp
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
```

## Example 7: Redirect Requests

```csharp
// Intercept requests to old domain
AddInterceptCommandParameters addIntercept = new AddInterceptCommandParameters();
addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
addIntercept.UrlPatterns = new List<UrlPattern>
{
    new UrlPatternPattern { Hostname = "old-api.example.com" }
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
```

## Pattern: Conditional Interception

```csharp
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
```

## Best Practices

1. **Use async handlers**: Network interception requires calling commands within handlers
2. **Handle all intercepted requests**: Always call Continue, Fail, or ProvideResponse
3. **Clean up intercepts**: Remove intercepts when done with `RemoveInterceptAsync`
4. **Manage data collectors**: Remove collectors to free memory
5. **Use URL patterns**: Limit interception scope for better performance
6. **Test error cases**: Handle network failures gracefully

## Common Issues

### Requests Hang

**Problem**: Intercepted requests never complete.

**Solution**: Ensure every intercepted request (when `IsBlocked` is true) calls Continue, Fail, or ProvideResponse.

### Memory Issues

**Problem**: Memory usage grows over time.

**Solution**: Set `DisownCollectedData = true` when getting response bodies.

### Timing Problems

**Problem**: Intercept not capturing requests.

**Solution**: Set up intercept before navigating to the page.

## Next Steps

- [Network Module](../modules/network.md): Complete network module guide
- [Events and Observables](../events-observables.md): Understanding event handling
- [Common Scenarios](common-scenarios.md): More examples

