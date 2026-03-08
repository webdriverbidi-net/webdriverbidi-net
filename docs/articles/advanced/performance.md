# Performance Considerations

This guide covers strategies for optimizing the performance of your WebDriverBiDi.NET automation.

## Overview

Performance in browser automation involves multiple factors:
- Command execution time
- Network communication overhead
- Event processing efficiency
- Resource management
- Browser responsiveness

Understanding and optimizing these factors can significantly improve automation speed and reliability.

## Connection Type Performance

WebDriverBiDi.NET primarily uses WebSocket connections for browser communication. An alternative pipe-based connection is available for specialized scenarios.

### WebSocket Connections (Recommended)

WebSocket connections are the standard transport mechanism for WebDriver BiDi:

**Characteristics:**
- **Universal compatibility**: Supported by all browsers with WebDriver BiDi
- **Network flexibility**: Connect to local or remote browsers
- **Simple setup**: Just provide a WebSocket URL
- **Low latency**: Typically 1-3ms per command for local connections

**Performance:**
- Command execution: 5-15ms average (including browser processing)
- Event delivery: Real-time with minimal overhead
- Suitable for all automation scenarios

```csharp
// Standard WebSocket connection
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.StartAsync("ws://localhost:9222/devtools/browser/abc-123");
```

### Pipe Connections (Advanced)

For specific scenarios where the browser and test runner are co-located, pipe connections provide an alternative transport:

**When to consider:**
- Browser implementation supports pipe protocol (currently only Chromium-based browsers)
- Both browser and tests run on the same machine
- Absolute minimum latency is critical

**Trade-offs:**
- Slightly lower latency (~0.5-1ms reduction per message)
- No network stack overhead
- Requires process lifecycle management
- Limited browser support
- No remote debugging capability

```csharp
// Pipe connection (Chromium only)
using WebDriverBiDi.Client;

ChromeLauncher launcher = new ChromeLauncher()
{
    ConnectionType = ConnectionType.Pipes
};

await launcher.StartAsync();
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), launcher.Transport);
await driver.StartAsync("pipes");
```

**Recommendation**: Use WebSocket connections unless you have specific requirements for pipe-based transport and are using a compatible browser implementation.

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
    RemoteValueDictionary data = success.Result.ValueAs<RemoteValueDictionary>();
    string title = data["title"].ValueAs<string>();
    string url = data["url"].ValueAs<string>();
    long linkCount = data["linkCount"].ValueAs<long>();
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

## Message Queue and High-Throughput Scenarios

### Understanding the Transport Message Queue

WebDriverBiDi.NET uses an **unbounded message queue** to buffer incoming messages from the browser. This design choice provides flexibility and avoids blocking the connection, but has important implications for high-throughput scenarios.

**Architecture:**
- Messages received from the WebSocket connection are queued immediately
- A dedicated reader task processes messages sequentially
- No limit on queue depth (unbounded)
- Single-reader, single-writer for optimal throughput

**Normal Operation:**
In typical usage, message processing is fast enough that the queue remains nearly empty. Messages arrive, get processed within milliseconds, and the queue clears immediately.

**High-Throughput Risks:**

```csharp
// ⚠️ Problematic: Thousands of rapid events with slow handlers
driver.Network.OnBeforeRequestSent.AddObserver((e) =>
{
    // Slow synchronous operation (200ms)
    Thread.Sleep(200);
    ProcessRequest(e);
});

// If 100 events arrive per second:
// - Processing rate: 5 events/second (200ms each)
// - Queue growth: 95 events/second
// - After 10 seconds: ~950 messages queued
// - Memory usage grows unbounded
```

### Symptoms of Queue Backlog

Monitor for these indicators:

1. **Increasing Memory Usage**: Process memory grows during high-event periods
2. **Event Lag**: Events processed long after they occurred
3. **Delayed Command Responses**: Commands take longer as queue backs up
4. **OutOfMemoryException**: In extreme cases with thousands of queued messages

### Preventing Queue Backlog

#### 1. Use Asynchronous Event Handlers

```csharp
// ✅ Good: Async handler doesn't block message thread
driver.Network.OnBeforeRequestSent.AddObserver(
    async (e) =>
    {
        // Runs on task pool, doesn't block queue processing
        await Task.Delay(200);
        await ProcessRequestAsync(e);
    },
    ObservableEventHandlerOptions.RunHandlerAsynchronously
);
```

**Impact:**
- Message processing thread continues immediately
- Queue stays near-empty even with slow handlers
- Multiple events can be processed concurrently

#### 2. Keep Event Handlers Fast

```csharp
// ❌ Bad: Heavy processing in handler
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    // CPU-intensive operation
    var analysis = PerformComplexAnalysis(e.Text);
    SaveToDatabase(analysis);  // I/O operation
    SendNotification(analysis); // Network call
});

// ✅ Good: Offload heavy work
ConcurrentQueue<EntryAddedEventArgs> logQueue = new();

driver.Log.OnEntryAdded.AddObserver((e) =>
{
    // Fast: Just queue for later processing
    logQueue.Enqueue(e);
});

// Separate background task processes the queue
Task.Run(async () =>
{
    while (!cancellationToken.IsCancellationRequested)
    {
        if (logQueue.TryDequeue(out var logEvent))
        {
            var analysis = await PerformComplexAnalysisAsync(logEvent.Text);
            await SaveToDatabaseAsync(analysis);
            await SendNotificationAsync(analysis);
        }
        else
        {
            await Task.Delay(10);
        }
    }
});
```

#### 3. Reduce Event Subscriptions

```csharp
// ❌ Bad: Subscribe to high-frequency events you don't need
subscribe.Events.Add("network.beforeRequestSent");   // Very high frequency
subscribe.Events.Add("network.responseStarted");     // Very high frequency
subscribe.Events.Add("network.responseCompleted");   // High frequency

// ✅ Good: Only subscribe to events you actually use
subscribe.Events.Add("network.responseCompleted");   // Only this one

// ✅ Even better: Scope to specific contexts
subscribe.Events.Add("network.responseCompleted");
subscribe.BrowsingContextIds.Add(contextId);  // Only for this tab
```

#### 4. Implement Event Throttling

```csharp
// Throttle high-frequency events
DateTime lastProcessed = DateTime.MinValue;
TimeSpan throttleInterval = TimeSpan.FromMilliseconds(100);

driver.Network.OnBeforeRequestSent.AddObserver((e) =>
{
    DateTime now = DateTime.Now;
    if (now - lastProcessed < throttleInterval)
    {
        return;  // Skip this event
    }

    lastProcessed = now;
    ProcessRequest(e);
});
```

#### 5. Use Event Sampling

```csharp
// Sample 10% of events for analysis
Random random = new Random();

driver.Network.OnResponseCompleted.AddObserver((e) =>
{
    if (random.Next(100) < 10)  // 10% sample rate
    {
        AnalyzeResponse(e);
    }
});
```

### Monitoring and Diagnostics

Since there's no built-in queue depth metric, monitor at the process level:

```csharp
using System.Diagnostics;

// Monitor memory usage
Process currentProcess = Process.GetCurrentProcess();

Timer memoryMonitor = new Timer(_ =>
{
    currentProcess.Refresh();
    long memoryMB = currentProcess.WorkingSet64 / (1024 * 1024);

    if (memoryMB > 500)  // Alert if over 500MB
    {
        Console.WriteLine($"⚠️ High memory usage: {memoryMB} MB");
    }
}, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
```

### Practical Guidelines

**For Low-Traffic Applications** (< 100 events/second):
- Default configuration works well
- No special considerations needed
- Synchronous event handlers are acceptable for quick operations

**For Medium-Traffic Applications** (100-1000 events/second):
- Use `RunHandlerAsynchronously` for all handlers doing I/O
- Keep handlers under 10ms for synchronous execution
- Monitor memory usage during peak load

**For High-Traffic Applications** (> 1000 events/second):
- Use `RunHandlerAsynchronously` for ALL event handlers
- Implement event sampling or throttling
- Consider reducing event subscriptions
- Offload processing to background queues
- Monitor memory continuously
- Consider multiple driver instances to distribute load

### Example: High-Throughput Network Monitoring

```csharp
using System.Collections.Concurrent;
using WebDriverBiDi;
using WebDriverBiDi.Network;

// Efficient high-throughput event handling
public class NetworkMonitor
{
    private readonly ConcurrentQueue<ResponseData> responseQueue = new();
    private readonly SemaphoreSlim processingSignal = new(0);
    private readonly CancellationTokenSource cancellation = new();
    private int eventCount = 0;

    public async Task StartAsync(BiDiDriver driver)
    {
        // Lightweight event handler - just queue
        driver.Network.OnResponseCompleted.AddObserver(
            async (e) =>
            {
                responseQueue.Enqueue(e.Response);
                processingSignal.Release();
                Interlocked.Increment(ref eventCount);
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously
        );

        // Background processor - handles heavy work
        _ = Task.Run(async () =>
        {
            while (!cancellation.Token.IsCancellationRequested)
            {
                await processingSignal.WaitAsync(cancellation.Token);

                if (responseQueue.TryDequeue(out var response))
                {
                    try
                    {
                        await AnalyzeResponseAsync(response);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Analysis error: {ex.Message}");
                    }
                }
            }
        });

        // Metrics reporter
        _ = Task.Run(async () =>
        {
            while (!cancellation.Token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                int count = Interlocked.Exchange(ref eventCount, 0);
                int queueDepth = responseQueue.Count;
                Console.WriteLine($"Events/sec: {count / 10.0:F1}, Queue depth: {queueDepth}");
            }
        });
    }

    private async Task AnalyzeResponseAsync(ResponseData response)
    {
        // Heavy analysis happens here, off the message thread
        await Task.Delay(50);  // Simulated analysis
    }

    public void Stop()
    {
        cancellation.Cancel();
    }
}
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
            new GetTreeCommandParameters { RootBrowsingContextId = contextId });
        
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

