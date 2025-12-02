# Network Module

The Network module provides comprehensive control over HTTP/HTTPS traffic, including monitoring requests and responses, intercepting network calls, and capturing response bodies.

## Overview

The Network module enables you to:

- Monitor all network requests and responses
- Intercept requests before they're sent
- Modify or block network traffic
- Capture request and response bodies
- Handle authentication challenges
- Track network errors

## Accessing the Module

```csharp
NetworkModule network = driver.Network;
```

## Monitoring Network Traffic

### Basic Response Monitoring

```csharp
// Add observer
driver.Network.OnResponseCompleted.AddObserver((ResponseCompletedEventArgs e) =>
{
    Console.WriteLine($"URL: {e.Response.Url}");
    Console.WriteLine($"Status: {e.Response.Status} {e.Response.StatusText}");
});

// Subscribe to events
SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.Network.OnResponseCompleted.EventName);
await driver.Session.SubscribeAsync(subscribe);

// Navigate - events will fire for all requests
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com"));
```

### Monitor Request Details

```csharp
driver.Network.OnBeforeRequestSent.AddObserver((BeforeRequestSentEventArgs e) =>
{
    Console.WriteLine($"Request: {e.Request.Method} {e.Request.Url}");
    Console.WriteLine("Headers:");
    foreach (var header in e.Request.Headers)
    {
        Console.WriteLine($"  {header.Name}: {header.Value.Value}");
    }
});

SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.Network.OnBeforeRequestSent.EventName);
await driver.Session.SubscribeAsync(subscribe);
```

### Filter by Content Type

```csharp
driver.Network.OnResponseCompleted.AddObserver((ResponseCompletedEventArgs e) =>
{
    // Find Content-Type header
    var contentType = e.Response.Headers
        .FirstOrDefault(h => h.Name.Equals("content-type", StringComparison.OrdinalIgnoreCase));
    
    if (contentType != null && contentType.Value.Value.Contains("application/json"))
    {
        Console.WriteLine($"JSON response from: {e.Response.Url}");
    }
});
```

### Filter by URL Pattern

```csharp
driver.Network.OnBeforeRequestSent.AddObserver((BeforeRequestSentEventArgs e) =>
{
    if (e.Request.Url.Contains("/api/"))
    {
        Console.WriteLine($"API call: {e.Request.Url}");
    }
});
```

## Network Events

### BeforeRequestSent

Fired when a request is about to be sent:

```csharp
driver.Network.OnBeforeRequestSent.AddObserver((BeforeRequestSentEventArgs e) =>
{
    Console.WriteLine($"Method: {e.Request.Method}");
    Console.WriteLine($"URL: {e.Request.Url}");
    Console.WriteLine($"Request ID: {e.Request.RequestId}");
    Console.WriteLine($"Timestamp: {e.Request.Timings.TimeOrigin}");
    Console.WriteLine($"Is Blocked: {e.IsBlocked}");
});
```

### ResponseStarted

Fired when response headers are received:

```csharp
driver.Network.OnResponseStarted.AddObserver((ResponseStartedEventArgs e) =>
{
    Console.WriteLine($"Status: {e.Response.Status}");
    Console.WriteLine($"Headers received for: {e.Response.Url}");
});
```

### ResponseCompleted

Fired when response is fully received:

```csharp
driver.Network.OnResponseCompleted.AddObserver((ResponseCompletedEventArgs e) =>
{
    Console.WriteLine($"Response complete: {e.Response.Url}");
    Console.WriteLine($"Bytes received: {e.Response.BytesReceived}");
});
```

### FetchError

Fired when a network error occurs:

```csharp
driver.Network.OnFetchError.AddObserver((FetchErrorEventArgs e) =>
{
    Console.WriteLine($"Network error for: {e.Request.Url}");
    Console.WriteLine($"Error: {e.ErrorText}");
});
```

### AuthRequired

Fired when authentication is needed:

```csharp
driver.Network.OnAuthRequired.AddObserver(async (AuthRequiredEventArgs e) =>
{
    // Provide credentials
    ProvideCredentialsCommandParameters params = 
        new ProvideCredentialsCommandParameters(e.Request.RequestId)
        {
            Username = "myuser",
            Password = "mypassword"
        };
    
    await driver.Network.ProvideCredentialsAsync(params);
},
ObservableEventHandlerOptions.RunHandlerAsynchronously);
```

## Intercepting Network Traffic

Network interception allows you to block, modify, or replace network requests.

### Add Intercept

```csharp
AddInterceptCommandParameters params = new AddInterceptCommandParameters();

// Specify which phase to intercept
params.Phases.Add(InterceptPhase.BeforeRequestSent);

// Optional: limit to specific contexts
params.BrowsingContextIds = new List<string> { contextId };

// Optional: URL patterns to intercept
params.UrlPatterns = new List<UrlPattern>
{
    new UrlPatternPattern { Hostname = "example.com" }
};

AddInterceptCommandResult result = await driver.Network.AddInterceptAsync(params);
string interceptId = result.InterceptId;
```

### Intercept Specific URLs

```csharp
AddInterceptCommandParameters params = new AddInterceptCommandParameters();
params.Phases.Add(InterceptPhase.BeforeRequestSent);

// Intercept all .jpg images
params.UrlPatterns = new List<UrlPattern>
{
    new UrlPatternString("*.jpg")
};

await driver.Network.AddInterceptAsync(params);
```

### Block Requests

```csharp
// Add intercept
AddInterceptCommandParameters addIntercept = new AddInterceptCommandParameters();
addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
addIntercept.UrlPatterns = new List<UrlPattern>
{
    new UrlPatternPattern { Hostname = "ads.example.com" }
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
```

### Continue Requests

```csharp
driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
{
    if (e.IsBlocked)
    {
        // Optionally modify request
        ContinueRequestCommandParameters params = 
            new ContinueRequestCommandParameters(e.Request.RequestId);
        
        // Add custom header
        params.Headers = new List<Header>(e.Request.Headers);
        params.Headers.Add(new Header("X-Custom-Header", 
            new BytesValue(BytesValueType.String, "MyValue")));
        
        await driver.Network.ContinueRequestAsync(params);
    }
},
ObservableEventHandlerOptions.RunHandlerAsynchronously);
```

### Provide Custom Response

```csharp
driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
{
    if (e.IsBlocked && e.Request.Url.Contains("/api/data"))
    {
        // Return custom JSON response
        string jsonResponse = "{\"message\": \"Mocked response\"}";
        
        ProvideResponseCommandParameters params = 
            new ProvideResponseCommandParameters(e.Request.RequestId)
            {
                StatusCode = 200,
                ReasonPhrase = "OK",
                Body = BytesValue.FromString(jsonResponse)
            };
        
        params.Headers.Add(new Header("Content-Type", 
            new BytesValue(BytesValueType.String, "application/json")));
        
        await driver.Network.ProvideResponseAsync(params);
    }
},
ObservableEventHandlerOptions.RunHandlerAsynchronously);
```

### Remove Intercept

```csharp
RemoveInterceptCommandParameters params = 
    new RemoveInterceptCommandParameters(interceptId);

await driver.Network.RemoveInterceptAsync(params);
```

## Capturing Response Bodies

To capture response bodies, you must set up a data collector.

### Create Data Collector

```csharp
// Allocate memory for data collection (in bytes)
ulong maxSize = Convert.ToUInt64(Math.Pow(2, 24));  // 16 MB

AddDataCollectorCommandParameters params = 
    new AddDataCollectorCommandParameters(maxSize);

// Limit to specific context
params.BrowsingContexts.Add(contextId);

AddDataCollectorCommandResult result = 
    await driver.Network.AddDataCollectorAsync(params);

string collectorId = result.CollectorId;
```

### Get Response Body

```csharp
List<string> capturedBodies = new List<string>();

driver.Network.OnResponseCompleted.AddObserver(async (ResponseCompletedEventArgs e) =>
{
    // Only capture specific responses
    if (e.Response.Url.EndsWith(".json"))
    {
        GetDataCommandParameters getDataParams = 
            new GetDataCommandParameters(e.Request.RequestId)
            {
                CollectorId = collectorId,
                DisownCollectedData = true  // Free memory after retrieval
            };
        
        GetDataCommandResult dataResult = 
            await driver.Network.GetDataAsync(getDataParams);
        
        string body = dataResult.Bytes.GetStringValue();
        capturedBodies.Add(body);
    }
},
ObservableEventHandlerOptions.RunHandlerAsynchronously);
```

### Remove Data Collector

```csharp
RemoveDataCollectorCommandParameters params = 
    new RemoveDataCollectorCommandParameters(collectorId);

await driver.Network.RemoveDataCollectorAsync(params);
```

## Request/Response Headers

### Reading Headers

```csharp
driver.Network.OnResponseCompleted.AddObserver((ResponseCompletedEventArgs e) =>
{
    foreach (var header in e.Response.Headers)
    {
        string name = header.Name;
        string value = header.Value.Value;
        Console.WriteLine($"{name}: {value}");
    }
});
```

### Setting Custom Headers

```csharp
// Use intercept to add headers
driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
{
    if (e.IsBlocked)
    {
        ContinueRequestCommandParameters params = 
            new ContinueRequestCommandParameters(e.Request.RequestId);
        
        // Copy existing headers
        params.Headers = new List<Header>(e.Request.Headers);
        
        // Add authorization header
        params.Headers.Add(new Header("Authorization",
            new BytesValue(BytesValueType.String, "Bearer mytoken")));
        
        await driver.Network.ContinueRequestAsync(params);
    }
},
ObservableEventHandlerOptions.RunHandlerAsynchronously);
```

## Cookies

### Add Cookie

```csharp
// Use Storage module for cookies
SetCookieCommandParameters params = new SetCookieCommandParameters(
    new PartialCookie("sessionId", new BytesValue(BytesValueType.String, "abc123"))
    {
        Domain = "example.com",
        Path = "/",
        Secure = true,
        HttpOnly = true,
        SameSite = SameSite.Strict
    });

await driver.Storage.SetCookieAsync(params);
```

### Get Cookies

```csharp
GetCookiesCommandParameters params = new GetCookiesCommandParameters();
params.BrowsingContexts.Add(contextId);

GetCookiesCommandResult result = await driver.Storage.GetCookiesAsync(params);

foreach (var cookie in result.Cookies)
{
    Console.WriteLine($"{cookie.Name}: {cookie.Value.Value}");
    Console.WriteLine($"  Domain: {cookie.Domain}");
    Console.WriteLine($"  Expires: {cookie.Expiry}");
}
```

## Timing Information

```csharp
driver.Network.OnResponseCompleted.AddObserver((ResponseCompletedEventArgs e) =>
{
    var timings = e.Response.Timings;
    
    Console.WriteLine($"Time origin: {timings.TimeOrigin}");
    Console.WriteLine($"Request time: {timings.RequestTime}");
    Console.WriteLine($"Response start: {timings.ResponseStart}");
    Console.WriteLine($"Response end: {timings.ResponseEnd}");
});
```

## Common Patterns

### Pattern 1: Collect All Requests

```csharp
List<RequestData> allRequests = new List<RequestData>();

driver.Network.OnBeforeRequestSent.AddObserver((BeforeRequestSentEventArgs e) =>
{
    allRequests.Add(e.Request);
});

SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.Network.OnBeforeRequestSent.EventName);
await driver.Session.SubscribeAsync(subscribe);

await driver.BrowsingContext.NavigateAsync(navParams);

// Wait for requests to complete
await Task.Delay(2000);

Console.WriteLine($"Total requests: {allRequests.Count}");
foreach (var request in allRequests)
{
    Console.WriteLine($"  {request.Method} {request.Url}");
}
```

### Pattern 2: Block Ad Domains

```csharp
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
        new UrlPatternPattern { Hostname = domain });
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
```

### Pattern 3: Mock API Responses

```csharp
Dictionary<string, string> mockResponses = new Dictionary<string, string>
{
    { "/api/user", "{\"name\": \"Test User\", \"id\": 123}" },
    { "/api/settings", "{\"theme\": \"dark\", \"lang\": \"en\"}" }
};

AddInterceptCommandParameters addIntercept = new AddInterceptCommandParameters();
addIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
await driver.Network.AddInterceptAsync(addIntercept);

driver.Network.OnBeforeRequestSent.AddObserver(async (BeforeRequestSentEventArgs e) =>
{
    if (e.IsBlocked)
    {
        string path = new Uri(e.Request.Url).AbsolutePath;
        
        if (mockResponses.TryGetValue(path, out string mockData))
        {
            ProvideResponseCommandParameters params = 
                new ProvideResponseCommandParameters(e.Request.RequestId)
                {
                    StatusCode = 200,
                    Body = BytesValue.FromString(mockData)
                };
            
            params.Headers.Add(new Header("Content-Type",
                new BytesValue(BytesValueType.String, "application/json")));
            
            await driver.Network.ProvideResponseAsync(params);
        }
        else
        {
            await driver.Network.ContinueRequestAsync(
                new ContinueRequestCommandParameters(e.Request.RequestId));
        }
    }
},
ObservableEventHandlerOptions.RunHandlerAsynchronously);
```

### Pattern 4: Capture Complete HTTP Transaction

```csharp
public class HttpTransaction
{
    public RequestData Request { get; set; }
    public ResponseData Response { get; set; }
    public string RequestBody { get; set; }
    public string ResponseBody { get; set; }
}

Dictionary<string, HttpTransaction> transactions = 
    new Dictionary<string, HttpTransaction>();

// Set up data collector
AddDataCollectorCommandResult collectorResult = 
    await driver.Network.AddDataCollectorAsync(
        new AddDataCollectorCommandParameters(Convert.ToUInt64(Math.Pow(2, 26))));
string collectorId = collectorResult.CollectorId;

// Capture requests
driver.Network.OnBeforeRequestSent.AddObserver((BeforeRequestSentEventArgs e) =>
{
    transactions[e.Request.RequestId] = new HttpTransaction
    {
        Request = e.Request
    };
});

// Capture responses and bodies
driver.Network.OnResponseCompleted.AddObserver(async (ResponseCompletedEventArgs e) =>
{
    if (transactions.TryGetValue(e.Request.RequestId, out var transaction))
    {
        transaction.Response = e.Response;
        
        // Get response body
        GetDataCommandParameters getDataParams = 
            new GetDataCommandParameters(e.Request.RequestId)
            {
                CollectorId = collectorId,
                DisownCollectedData = true
            };
        
        GetDataCommandResult dataResult = 
            await driver.Network.GetDataAsync(getDataParams);
        
        transaction.ResponseBody = dataResult.Bytes.GetStringValue();
    }
},
ObservableEventHandlerOptions.RunHandlerAsynchronously);
```

## Best Practices

1. **Use data collectors wisely**: They consume memory; remove when done
2. **Run async handlers**: Network event handlers often need to call commands
3. **Filter events**: Don't process every request if you only need specific ones
4. **Clean up intercepts**: Remove intercepts when no longer needed
5. **Handle errors**: Network operations can fail; use try-catch
6. **Consider timing**: Some network events happen very quickly

## Troubleshooting

### Events Not Firing

- Ensure you've subscribed to events through Session module
- Check that navigation has actually started
- Verify URL patterns in intercepts are correct

### Missing Response Bodies

- Data collector must be added before navigation
- Ensure sufficient memory allocated
- Check that `DisownCollectedData` is set appropriately

### Intercepts Not Working

- Verify intercept was added before navigation
- Check URL patterns match the requests
- Ensure `IsBlocked` is true in event handler

## Next Steps

- [Examples: Network Interception](../examples/network-interception.md): Complete examples
- [Examples: Network Monitoring](../examples/common-scenarios.md): Practical scenarios
- [Storage Module](storage.md): Working with cookies
- [API Reference](../../api/index.md): Complete API documentation

## API Reference

See the [API documentation](../../api/index.md) for complete details on all classes and methods in the Network module.

