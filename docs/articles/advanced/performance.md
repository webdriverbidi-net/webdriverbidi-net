# Performance Considerations

This guide covers strategies for optimizing the performance of your WebDriverBiDi.NET-Relaxed automation.

## Overview

Performance in browser automation involves multiple factors:
- Command execution time
- Network communication overhead
- Event processing efficiency
- Resource management
- Browser responsiveness

Understanding and optimizing these factors can significantly improve automation speed and reliability.

## Command Optimization

### Parallel Command Execution

Execute independent commands in parallel:

```csharp
// ❌ Slow: Sequential execution
var tree = await driver.BrowsingContext.GetTreeAsync(new());
var status = await driver.Session.StatusAsync(new());
var cookies = await driver.Storage.GetCookiesAsync(new());

// ✅ Fast: Parallel execution
Task<GetTreeCommandResult> treeTask = driver.BrowsingContext.GetTreeAsync(new());
Task<StatusCommandResult> statusTask = driver.Session.StatusAsync(new());
Task<GetCookiesCommandResult> cookiesTask = driver.Storage.GetCookiesAsync(new());

await Task.WhenAll(treeTask, statusTask, cookiesTask);

var tree = treeTask.Result;
var status = statusTask.Result;
var cookies = cookiesTask.Result;
```

### Batch Operations

```csharp
// ❌ Slow: Multiple round trips
foreach (string url in urls)
{
    await driver.BrowsingContext.NavigateAsync(
        new NavigateCommandParameters(contextId, url));
}

// ✅ Fast: Parallel navigation in different contexts
List<Task<NavigateCommandResult>> navigationTasks = new();

foreach (var (url, context) in urls.Zip(contextIds))
{
    navigationTasks.Add(driver.BrowsingContext.NavigateAsync(
        new NavigateCommandParameters(context, url)));
}

await Task.WhenAll(navigationTasks);
```

## Navigation Optimization

### Use Appropriate Readiness States

```csharp
// For content-only needs
NavigateCommandParameters params = new NavigateCommandParameters(contextId, url)
{
    Wait = ReadinessState.Interactive  // Don't wait for images/CSS
};

// For visual validation
NavigateCommandParameters params = new NavigateCommandParameters(contextId, url)
{
    Wait = ReadinessState.Complete  // Wait for all resources
};

// For fastest possible navigation
NavigateCommandParameters params = new NavigateCommandParameters(contextId, url)
{
    Wait = ReadinessState.None  // Return immediately
};
```

### Smart Waiting

```csharp
// ❌ Slow: Fixed delays
await driver.BrowsingContext.NavigateAsync(navParams);
await Task.Delay(5000);  // Always waits 5 seconds

// ✅ Fast: Wait for specific condition
await driver.BrowsingContext.NavigateAsync(navParams);

string waitScript = @"
    new Promise((resolve) => {
        if (document.querySelector('.content-loaded')) {
            resolve(true);
        } else {
            const observer = new MutationObserver(() => {
                if (document.querySelector('.content-loaded')) {
                    observer.disconnect();
                    resolve(true);
                }
            });
            observer.observe(document.body, { childList: true, subtree: true });
        }
    })";

await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(waitScript, new ContextTarget(contextId), true));
```

## Script Execution Optimization

### Minimize Script Calls

```csharp
// ❌ Slow: Multiple script calls
var title = await GetScriptValue("document.title");
var url = await GetScriptValue("window.location.href");
var linkCount = await GetScriptValue("document.querySelectorAll('a').length");

// ✅ Fast: Single script call
string script = @"
({
    title: document.title,
    url: window.location.href,
    linkCount: document.querySelectorAll('a').length
})";

EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(script, new ContextTarget(contextId), true));

if (result is EvaluateResultSuccess success)
{
    var data = success.Result.ValueAs<Dictionary<string, object>>();
    var title = data["title"];
    var url = data["url"];
    var linkCount = data["linkCount"];
}
```

### Efficient Element Operations

```csharp
// ❌ Slow: Multiple element queries
for (int i = 0; i < 10; i++)
{
    var element = await FindElementAsync($".item-{i}");
    await ClickElementAsync(element);
}

// ✅ Fast: Batch element operations
string script = @"
    Array.from(document.querySelectorAll('[class^=""item-""]'))
        .slice(0, 10)
        .forEach(el => el.click());";

await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(script, new ContextTarget(contextId), false));
```

## Event Processing Optimization

### Use Async Event Handlers

```csharp
// ❌ Slow: Synchronous handler blocks message processing
driver.Network.OnBeforeRequestSent.AddObserver((e) =>
{
    Thread.Sleep(1000);  // Blocks for 1 second!
    ProcessRequest(e);
});

// ✅ Fast: Async handler doesn't block
driver.Network.OnBeforeRequestSent.AddObserver(async (e) =>
{
    await Task.Delay(1000);  // Doesn't block message processing
    await ProcessRequestAsync(e);
},
ObservableEventHandlerOptions.RunHandlerAsynchronously);
```

### Filter Events Early

```csharp
// ❌ Slow: Process all events then filter
driver.Network.OnResponseCompleted.AddObserver((e) =>
{
    // Process every single response
    if (e.Response.Url.Contains(".json"))
    {
        // Only use JSON responses
        ProcessResponse(e);
    }
});

// ✅ Fast: Filter in observer
driver.Network.OnResponseCompleted.AddObserver((e) =>
{
    if (!e.Response.Url.Contains(".json"))
        return;  // Exit early
    
    ProcessResponse(e);
});
```

### Selective Event Subscription

```csharp
// ❌ Slow: Subscribe to everything
SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add("network.beforeRequestSent");
subscribe.Events.Add("network.responseStarted");
subscribe.Events.Add("network.responseCompleted");
subscribe.Events.Add("network.fetchError");

// ✅ Fast: Only subscribe to what you need
SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add("network.responseCompleted");  // Only this one

// Even better: Subscribe only for specific contexts
subscribe.BrowsingContextIds.Add(contextId);
```

## Resource Management

### Connection Pooling

```csharp
public class DriverPool
{
    private readonly Stack<BiDiDriver> availableDrivers = new();
    private readonly int maxPoolSize = 5;
    private readonly SemaphoreSlim semaphore;
    
    public DriverPool()
    {
        semaphore = new SemaphoreSlim(maxPoolSize, maxPoolSize);
    }

    public async Task<BiDiDriver> AcquireAsync()
    {
        await semaphore.WaitAsync();
        
        lock (availableDrivers)
        {
            if (availableDrivers.Count > 0)
            {
                return availableDrivers.Pop();
            }
        }
        
        // Create new driver if none available
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync(webSocketUrl);
        return driver;
    }

    public void Release(BiDiDriver driver)
    {
        lock (availableDrivers)
        {
            if (availableDrivers.Count < maxPoolSize)
            {
                availableDrivers.Push(driver);
            }
            else
            {
                driver.StopAsync().Wait();
            }
        }
        
        semaphore.Release();
    }
}

// Usage
DriverPool pool = new DriverPool();

BiDiDriver driver = await pool.AcquireAsync();
try
{
    // Use driver
    await driver.BrowsingContext.NavigateAsync(navParams);
}
finally
{
    pool.Release(driver);
}
```

### Memory Management

```csharp
// Always clean up data collectors
AddDataCollectorCommandResult collector = 
    await driver.Network.AddDataCollectorAsync(collectorParams);

try
{
    // Use collector
    await CaptureNetworkTraffic();
}
finally
{
    await driver.Network.RemoveDataCollectorAsync(
        new RemoveDataCollectorCommandParameters(collector.CollectorId));
}

// Disown collected data to free memory
GetDataCommandParameters getDataParams = new GetDataCommandParameters(requestId)
{
    CollectorId = collectorId,
    DisownCollectedData = true  // Free memory after retrieval
};
```

### Observer Cleanup

```csharp
public class ManagedObserver<T> : IDisposable where T : WebDriverBiDiEventArgs
{
    private EventObserver<T>? observer;
    
    public ManagedObserver(ObservableEvent<T> observableEvent, Func<T, Task> handler)
    {
        observer = observableEvent.AddObserver(handler);
    }
    
    public void Dispose()
    {
        observer?.Unobserve();
        observer = null;
    }
}

// Usage with automatic cleanup
using (new ManagedObserver<EntryAddedEventArgs>(
    driver.Log.OnEntryAdded,
    (e) => { Console.WriteLine(e.Text); return Task.CompletedTask; }))
{
    // Observer active here
    await driver.BrowsingContext.NavigateAsync(navParams);
}
// Observer automatically removed
```

## Network Optimization

### Reduce Network Interception Overhead

```csharp
// ❌ Slow: Intercept everything
AddInterceptCommandParameters intercept = new AddInterceptCommandParameters();
intercept.Phases.Add(InterceptPhase.BeforeRequestSent);
await driver.Network.AddInterceptAsync(intercept);

// ✅ Fast: Intercept only what you need
AddInterceptCommandParameters intercept = new AddInterceptCommandParameters();
intercept.Phases.Add(InterceptPhase.BeforeRequestSent);
intercept.UrlPatterns = new List<UrlPattern>
{
    new UrlPatternPattern { Hostname = "api.example.com" }
};
await driver.Network.AddInterceptAsync(intercept);
```

### Block Unnecessary Resources

```csharp
// Speed up page loads by blocking images, CSS, fonts
AddInterceptCommandParameters intercept = new AddInterceptCommandParameters();
intercept.Phases.Add(InterceptPhase.BeforeRequestSent);
intercept.UrlPatterns = new List<UrlPattern>
{
    new UrlPatternString("*.jpg"),
    new UrlPatternString("*.png"),
    new UrlPatternString("*.gif"),
    new UrlPatternString("*.css"),
    new UrlPatternString("*.woff*")
};

await driver.Network.AddInterceptAsync(intercept);

driver.Network.OnBeforeRequestSent.AddObserver(async (e) =>
{
    if (e.IsBlocked)
    {
        await driver.Network.FailRequestAsync(
            new FailRequestCommandParameters(e.Request.RequestId));
    }
},
ObservableEventHandlerOptions.RunHandlerAsynchronously);
```

## Browser Context Optimization

### Reuse Contexts

```csharp
// ❌ Slow: Create new context for each test
[Test]
public async Task Test1()
{
    CreateCommandResult ctx = await driver.BrowsingContext.CreateAsync(
        new CreateCommandParameters(ContextType.Tab));
    // Test...
    await driver.BrowsingContext.CloseAsync(new CloseCommandParameters(ctx.BrowsingContextId));
}

// ✅ Fast: Reuse context, just navigate
private string sharedContextId;

[SetUp]
public async Task Setup()
{
    CreateCommandResult ctx = await driver.BrowsingContext.CreateAsync(
        new CreateCommandParameters(ContextType.Tab));
    sharedContextId = ctx.BrowsingContextId;
}

[Test]
public async Task Test1()
{
    await driver.BrowsingContext.NavigateAsync(
        new NavigateCommandParameters(sharedContextId, url));
    // Test...
}

[TearDown]
public async Task Teardown()
{
    await driver.BrowsingContext.CloseAsync(
        new CloseCommandParameters(sharedContextId));
}
```

### Use User Contexts for Isolation

```csharp
// Create isolated user context once
CreateUserContextCommandResult userContext = 
    await driver.Browser.CreateUserContextAsync(new());

// Create multiple tabs in same user context (shares cache, cookies)
List<string> contextIds = new();
for (int i = 0; i < 5; i++)
{
    CreateCommandResult tab = await driver.BrowsingContext.CreateAsync(
        new CreateCommandParameters(ContextType.Tab)
        {
            UserContext = userContext.UserContextId
        });
    contextIds.Add(tab.BrowsingContextId);
}

// All tabs share cookies and cache = faster subsequent loads
```

## Measurement and Profiling

### Performance Tracking

```csharp
public class PerformanceTracker
{
    private Dictionary<string, List<TimeSpan>> metrics = new();
    
    public async Task<T> TrackAsync<T>(string operationName, Func<Task<T>> operation)
    {
        DateTime start = DateTime.Now;
        
        try
        {
            return await operation();
        }
        finally
        {
            TimeSpan duration = DateTime.Now - start;
            
            if (!metrics.ContainsKey(operationName))
            {
                metrics[operationName] = new List<TimeSpan>();
            }
            
            metrics[operationName].Add(duration);
        }
    }
    
    public void PrintStats()
    {
        Console.WriteLine("\n Performance Statistics");
        Console.WriteLine("========================");
        
        foreach (var kvp in metrics)
        {
            var durations = kvp.Value;
            double avgMs = durations.Average(d => d.TotalMilliseconds);
            double minMs = durations.Min(d => d.TotalMilliseconds);
            double maxMs = durations.Max(d => d.TotalMilliseconds);
            
            Console.WriteLine($"\n{kvp.Key}:");
            Console.WriteLine($"  Count: {durations.Count}");
            Console.WriteLine($"  Avg:   {avgMs:F2}ms");
            Console.WriteLine($"  Min:   {minMs:F2}ms");
            Console.WriteLine($"  Max:   {maxMs:F2}ms");
        }
    }
}

// Usage
PerformanceTracker tracker = new PerformanceTracker();

await tracker.TrackAsync("Navigation", async () =>
    await driver.BrowsingContext.NavigateAsync(navParams));

await tracker.TrackAsync("Script Execution", async () =>
    await driver.Script.EvaluateAsync(evalParams));

tracker.PrintStats();
```

### Bottleneck Identification

```csharp
public async Task AnalyzePageLoadAsync(BiDiDriver driver, string contextId, string url)
{
    Dictionary<string, int> resourceCounts = new();
    Dictionary<string, long> resourceSizes = new();
    DateTime startTime = DateTime.Now;
    
    driver.Network.OnResponseCompleted.AddObserver((e) =>
    {
        Uri uri = new Uri(e.Response.Url);
        string extension = Path.GetExtension(uri.LocalPath).ToLower();
        
        resourceCounts[extension] = resourceCounts.GetValueOrDefault(extension) + 1;
        resourceSizes[extension] = resourceSizes.GetValueOrDefault(extension) + 
            (long)e.Response.BytesReceived;
    });
    
    SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
    subscribe.Events.Add(driver.Network.OnResponseCompleted.EventName);
    await driver.Session.SubscribeAsync(subscribe);
    
    await driver.BrowsingContext.NavigateAsync(
        new NavigateCommandParameters(contextId, url)
        { Wait = ReadinessState.Complete });
    
    TimeSpan loadTime = DateTime.Now - startTime;
    
    Console.WriteLine($"\nPage Load Analysis for {url}");
    Console.WriteLine($"Total Time: {loadTime.TotalSeconds:F2}s");
    Console.WriteLine("\nResources by Type:");
    
    foreach (var kvp in resourceCounts.OrderByDescending(x => x.Value))
    {
        long sizeKB = resourceSizes[kvp.Key] / 1024;
        Console.WriteLine($"  {kvp.Key,-10} {kvp.Value,3} files  {sizeKB,6} KB");
    }
}
```

## Caching Strategies

### Cache Repeated Queries

```csharp
public class CachedContextInfo
{
    private Dictionary<string, BrowsingContextInfo> cache = new();
    private BiDiDriver driver;
    
    public async Task<BrowsingContextInfo> GetContextInfoAsync(string contextId)
    {
        if (cache.TryGetValue(contextId, out var cachedInfo))
        {
            return cachedInfo;
        }
        
        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
            new GetTreeCommandParameters { Root = contextId });
        
        if (tree.ContextTree.Count > 0)
        {
            cache[contextId] = tree.ContextTree[0];
            return tree.ContextTree[0];
        }
        
        throw new Exception("Context not found");
    }
    
    public void InvalidateCache(string contextId)
    {
        cache.Remove(contextId);
    }
}
```

## Best Practices Summary

1. **Parallelize Independent Operations**: Use `Task.WhenAll` for concurrent execution
2. **Minimize Round Trips**: Batch operations when possible
3. **Use Appropriate Wait States**: Don't wait for more than you need
4. **Filter Events Early**: Process only relevant events
5. **Clean Up Resources**: Remove observers, collectors, and intercepts
6. **Reuse Contexts**: Don't create/destroy contexts unnecessarily
7. **Block Unnecessary Resources**: Speed up loads by blocking images/CSS
8. **Use Async Handlers**: Don't block message processing
9. **Cache Repeated Queries**: Store frequently accessed data
10. **Profile Your Code**: Measure to find actual bottlenecks

## Performance Checklist

- [ ] Commands executed in parallel where possible?
- [ ] Appropriate navigation readiness state used?
- [ ] Event subscriptions limited to necessary events?
- [ ] Event handlers are async for long operations?
- [ ] Resources cleaned up properly?
- [ ] Contexts reused across tests?
- [ ] Network interception scoped appropriately?
- [ ] Data collectors removed when done?
- [ ] Observers unsubscribed when no longer needed?
- [ ] Performance metrics collected and analyzed?

## Next Steps

- [Error Handling](error-handling.md): Handle failures efficiently
- [Architecture](../architecture.md): Understand system design
- [Examples](../examples/common-scenarios.md): See optimized patterns in action

