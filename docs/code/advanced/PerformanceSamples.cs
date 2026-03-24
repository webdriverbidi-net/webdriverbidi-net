// <copyright file="PerformanceSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/advanced/performance.md

#pragma warning disable CS8600, CS8602, CS8604, CS8618, CS1591, CS4014, CS0649

namespace WebDriverBiDi.Docs.Code.Advanced;

using System.Collections.Concurrent;
using System.Diagnostics;
using WebDriverBiDi;
using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client.Launchers;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.Storage;

/// <summary>
/// Snippets for performance documentation. Compiled at build time to prevent API drift.
/// </summary>
public class PerformanceSamples
{
    /// <summary>
    /// Standard WebSocket connection.
    /// </summary>
    public static async Task WebSocketConnection()
    {
        #region WebSocketConnection
        // Standard WebSocket connection
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync("ws://localhost:9222/devtools/browser/abc-123");
        #endregion
    }

    /// <summary>
    /// Pipe connection - implement launcher with IPipeServerProcessProvider.
    /// </summary>
    public static async Task PipeConnection()
    {
        #region PipeConnection
        // Pipe connection (Chromium only)
        ChromeLauncher launcher = new ChromeLauncher()
        {
            ConnectionType = ConnectionType.Pipes
        };

        await launcher.StartAsync();
        await launcher.LaunchBrowserAsync();
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), launcher.CreateTransport());
        await driver.StartAsync("pipes");
        #endregion
    }

    /// <summary>
    /// Parallel command execution.
    /// </summary>
    public static async Task ParallelCommands(BiDiDriver driver)
    {
        #region ParallelCommands
        // ❌ Slow: Sequential execution
        var tree = await driver.BrowsingContext.GetTreeAsync(new());
        var status = await driver.Session.StatusAsync(new());
        var cookies = await driver.Storage.GetCookiesAsync(new());

        // ✅ Fast: Parallel execution
        Task<GetTreeCommandResult> treeTask = driver.BrowsingContext.GetTreeAsync(new());
        Task<StatusCommandResult> statusTask = driver.Session.StatusAsync(new());
        Task<GetCookiesCommandResult> cookiesTask = driver.Storage.GetCookiesAsync(new());

        await Task.WhenAll(treeTask, statusTask, cookiesTask);

        var parallelTree = treeTask.Result;
        var parallelStatus = statusTask.Result;
        var parallelCookies = cookiesTask.Result;
        #endregion
    }

    /// <summary>
    /// Batch navigation across contexts.
    /// </summary>
    public static async Task BatchNavigation(BiDiDriver driver, IEnumerable<string> urls, IEnumerable<string> contextIds, string contextId)
    {
        #region BatchNavigation
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
        #endregion
    }

    /// <summary>
    /// Navigation readiness states.
    /// </summary>
    public static void ReadinessStates(string contextId, string url)
    {
        #region ReadinessStates
        // For content-only needs
        NavigateCommandParameters parameters1 = new NavigateCommandParameters(contextId, url)
        {
            Wait = ReadinessState.Interactive  // Don't wait for images/CSS
        };

        // For visual validation
        NavigateCommandParameters parameters2 = new NavigateCommandParameters(contextId, url)
        {
            Wait = ReadinessState.Complete  // Wait for all resources
        };

        // For fastest possible navigation
        NavigateCommandParameters parameters3 = new NavigateCommandParameters(contextId, url)
        {
            Wait = ReadinessState.None  // Return immediately
        };
        #endregion
    }

    /// <summary>
    /// Smart waiting with script instead of fixed delay.
    /// </summary>
    public static async Task SmartWaiting(BiDiDriver driver, string contextId, NavigateCommandParameters navParams)
    {
        #region SmartWaiting
        // ❌ Slow: Fixed delays
        await driver.BrowsingContext.NavigateAsync(navParams);
        await Task.Delay(5000);  // Always waits 5 seconds

        // ✅ Fast: Wait for specific condition
        await driver.BrowsingContext.NavigateAsync(navParams);

        string waitScript = """
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
            })
            """;

        await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(waitScript, new ContextTarget(contextId), true));
        #endregion
    }

    /// <summary>
    /// Single script call instead of multiple.
    /// </summary>
    public static async Task MinimizeScriptCalls(BiDiDriver driver, string contextId)
    {
        #region MinimizeScriptCalls
        // ❌ Slow: Multiple script calls
        var title = await GetScriptValue("document.title");
        var url = await GetScriptValue("window.location.href");
        var linkCount = await GetScriptValue("document.querySelectorAll('a').length");

        // ✅ Fast: Single script call
        string script = """
            ({
                title: document.title,
                url: window.location.href,
                linkCount: document.querySelectorAll('a').length
            })
            """;

        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(script, new ContextTarget(contextId), true));

        if (result is EvaluateResultSuccess success &&
            success.Result is KeyValuePairCollectionRemoteValue remoteValue)
        {
            RemoteValueDictionary data = remoteValue.Value;
            string actualTitle = data["title"].ConvertTo<StringRemoteValue>().Value;
            string actualUrl = data["url"].ConvertTo<StringRemoteValue>().Value;
            long actualLinkCount = data["linkCount"].ConvertTo<LongRemoteValue>().Value;
        }
        #endregion
    }

    private static async Task<string> GetScriptValue(string v)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Batch element operations via script.
    /// </summary>
    public static async Task EfficientElementOperations(BiDiDriver driver, string contextId)
    {
        #region EfficientElementOperations
        // ❌ Slow: Multiple element queries
        for (int i = 0; i < 10; i++)
        {
            var element = await FindElementAsync($".item-{i}");
            await ClickElementAsync(element);
        }

        // ✅ Fast: Batch element operations
        string script = """
            Array.from(document.querySelectorAll('[class^=""item-""]'))
                .slice(0, 10)
                .forEach(el => el.click());
            """;

        await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(script, new ContextTarget(contextId), false));
        #endregion
    }

    private static async Task ClickElementAsync(object element)
    {
        throw new NotImplementedException();
    }

    private static async Task<object> FindElementAsync(string v)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Async event handler - does not block message processing.
    /// </summary>
    public static void AsyncEventHandler(BiDiDriver driver)
    {
        #region AsyncEventHandler
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
        #endregion
    }

    /// <summary>
    /// Filter events early in observer.
    /// </summary>
    public static void FilterEventsEarly(BiDiDriver driver)
    {
        #region FilterEventsEarly
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
            {
                return;  // Exit early
            }

            ProcessResponse(e);
        });
        #endregion
    }

    /// <summary>
    /// Selective event subscription - only what you need.
    /// </summary>
    public static async Task SelectiveEventSubscription(BiDiDriver driver, string contextId)
    {
        #region SelectiveEventSubscription
        // ❌ Slow: Subscribe to everything
        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Network.OnBeforeRequestSent.EventName);
        subscribe.Events.Add(driver.Network.OnResponseStarted.EventName);
        subscribe.Events.Add(driver.Network.OnResponseCompleted.EventName);
        subscribe.Events.Add(driver.Network.OnFetchError.EventName);

        // ✅ Fast: Only subscribe to what you need
        SubscribeCommandParameters subscribeParameters =
            new SubscribeCommandParameters(driver.Network.OnResponseCompleted.EventName);

        // Even better: Subscribe only for specific contexts
        subscribeParameters.Contexts.Add(contextId);
        #endregion
    }

    /// <summary>
    /// Problematic: slow synchronous handler blocks queue.
    /// </summary>
    public static void SlowHandlerProblem(BiDiDriver driver)
    {
        #region SlowHandlerProblem
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
        #endregion
    }

    /// <summary>
    /// Good: async handler does not block message thread.
    /// </summary>
    public static void AsyncHandlerGood(BiDiDriver driver)
    {
        #region AsyncHandlerGood
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
        #endregion
    }

    /// <summary>
    /// Offload heavy work to background queue.
    /// </summary>
    public static void OffloadHeavyWork(BiDiDriver driver, CancellationToken cancellationToken)
    {
        #region OffloadHeavyWork
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
        #endregion
    }

    private static async Task SendNotificationAsync(AnalysisResult analysis)
    {
        throw new NotImplementedException();
    }

    private static async Task SaveToDatabaseAsync(AnalysisResult analysis)
    {
        throw new NotImplementedException();
    }

    private static void SendNotification(object analysis)
    {
        throw new NotImplementedException();
    }

    private static void SaveToDatabase(object analysis)
    {
        throw new NotImplementedException();
    }

    private static object PerformComplexAnalysis(string? text)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Scope subscriptions to specific contexts.
    /// </summary>
    public static async Task ScopeSubscriptions(BiDiDriver driver, SubscribeCommandParameters subscribe, string contextId)
    {
        #region ScopeSubscriptions
        // ❌ Bad: Subscribe to high-frequency events you don't need
        subscribe.Events.Add("network.beforeRequestSent");   // Very high frequency
        subscribe.Events.Add("network.responseStarted");     // Very high frequency
        subscribe.Events.Add("network.responseCompleted");   // High frequency

        // ✅ Good: Only subscribe to events you actually use
        subscribe.Events.Add("network.responseCompleted");   // Only this one

        // ✅ Even better: Scope to specific contexts
        subscribe.Events.Add("network.responseCompleted");
        subscribe.Contexts.Add(contextId);  // Only for this tab
        #endregion
    }

    /// <summary>
    /// Event throttling.
    /// </summary>
    public static void EventThrottling(BiDiDriver driver)
    {
        #region EventThrottling
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
        #endregion
    }

    /// <summary>
    /// Event sampling.
    /// </summary>
    public static void EventSampling(BiDiDriver driver)
    {
        #region EventSampling
        // Sample 10% of events for analysis
        Random random = new Random();

        driver.Network.OnResponseCompleted.AddObserver((e) =>
        {
            if (random.Next(100) < 10)  // 10% sample rate
            {
                AnalyzeResponse(e);
            }
        });
        #endregion
    }

    /// <summary>
    /// Memory monitoring.
    /// </summary>
    public static void MemoryMonitoring()
    {
        #region MemoryMonitoring
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
        #endregion
    }

    /// <summary>
    /// NetworkMonitor - high-throughput event handling.
    /// </summary>
    public static void NetworkMonitorStart(NetworkMonitor monitor, BiDiDriver driver)
    {
        monitor.StartAsync(driver);
    }

    /// <summary>
    /// Driver pool usage.
    /// </summary>
    public static async Task DriverPoolUsage(string websocketUrl, NavigateCommandParameters navParams)
    {
        #region DriverPoolUsage
        // Usage
        DriverPool pool = new DriverPool();

        BiDiDriver driver = await pool.AcquireAsync(websocketUrl);
        try
        {
            // Use driver
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        finally
        {
            pool.Release(driver);
        }
        #endregion
    }

    /// <summary>
    /// Memory management - clean up data collectors.
    /// </summary>
    public static async Task MemoryManagementCollectors(BiDiDriver driver, string requestId, AddDataCollectorCommandParameters collectorParams)
    {
        #region MemoryManagementCollectors
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
            CollectorId = collector.CollectorId,
            DisownCollectedData = true  // Free memory after retrieval
        };
        #endregion
    }

    private static async Task CaptureNetworkTraffic()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// ManagedObserver - automatic observer cleanup.
    /// </summary>
    public static async Task ManagedObserverUsage(BiDiDriver driver, NavigateCommandParameters navParams)
    {
        #region ManagedObserverUsage
        // Usage with automatic cleanup
        using (new ManagedObserver<EntryAddedEventArgs>(
            driver.Log.OnEntryAdded,
            (e) => { Console.WriteLine(e.Text); return Task.CompletedTask; }))
        {
            // Observer active here
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        // Observer automatically removed
        #endregion
    }

    /// <summary>
    /// Scoped network interception.
    /// </summary>
    public static async Task ScopedInterception(BiDiDriver driver)
    {
        #region ScopedInterception
        // ❌ Slow: Intercept everything
        AddInterceptCommandParameters slowIntercept = new AddInterceptCommandParameters();
        slowIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
        await driver.Network.AddInterceptAsync(slowIntercept);

        // ✅ Fast: Intercept only what you need
        AddInterceptCommandParameters fastIntercept = new AddInterceptCommandParameters();
        fastIntercept.Phases.Add(InterceptPhase.BeforeRequestSent);
        fastIntercept.UrlPatterns = new List<UrlPattern>
        {
            new UrlPatternPattern { HostName = "api.example.com" }
        };
        await driver.Network.AddInterceptAsync(fastIntercept);
        #endregion
    }

    /// <summary>
    /// Block unnecessary resources.
    /// </summary>
    public static async Task BlockUnnecessaryResources(BiDiDriver driver)
    {
        #region BlockUnnecessaryResources
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
        #endregion
    }

    /// <summary>
    /// User contexts for isolation and cache sharing.
    /// </summary>
    public static async Task UserContextsForIsolation(BiDiDriver driver)
    {
        #region UserContextsforIsolation
        // Create isolated user context once
        CreateUserContextCommandResult userContext =
            await driver.Browser.CreateUserContextAsync(new());

        // Create multiple tabs in same user context (shares cache, cookies)
        List<string> contextIds = new();
        for (int i = 0; i < 5; i++)
        {
            CreateCommandResult tab = await driver.BrowsingContext.CreateAsync(
                new CreateCommandParameters(CreateType.Tab)
                {
                    UserContextId = userContext.UserContextId
                });
            contextIds.Add(tab.BrowsingContextId);
        }

        // All tabs share cookies and cache = faster subsequent loads
        #endregion
    }

    /// <summary>
    /// Performance tracker usage.
    /// </summary>
    public static async Task PerformanceTrackerUsage(BiDiDriver driver, NavigateCommandParameters navParams, EvaluateCommandParameters evalParams)
    {
        #region PerformanceTrackerUsage
        // Usage
        PerformanceTracker tracker = new PerformanceTracker();

        await tracker.TrackAsync("Navigation", async () =>
            await driver.BrowsingContext.NavigateAsync(navParams));

        await tracker.TrackAsync("Script Execution", async () =>
            await driver.Script.EvaluateAsync(evalParams));

        tracker.PrintStats();
        #endregion
    }

    /// <summary>
    /// Bottleneck identification - page load analysis.
    /// </summary>
    #region AnalyzePageLoad
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

        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Network.OnResponseCompleted.EventName);
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
    #endregion

    /// <summary>
    /// CachedContextInfo usage.
    /// </summary>
    public static async Task CachedContextInfoUsage(CachedContextInfo cachedInfo, string contextId)
    {
        #region CachedContextInfoUsage
        // Usage
        BrowsingContextInfo info = await cachedInfo.GetContextInfoAsync(contextId);
        cachedInfo.InvalidateCache(contextId);
        #endregion
    }

    private static void ProcessRequest(BeforeRequestSentEventArgs e) { }

    private static Task ProcessRequestAsync(BeforeRequestSentEventArgs e) => Task.CompletedTask;

    private static void ProcessResponse(ResponseCompletedEventArgs e) { }

    private static void AnalyzeResponse(ResponseCompletedEventArgs e) { }

    private static Task<AnalysisResult> PerformComplexAnalysisAsync(string text) => Task.FromResult(new AnalysisResult());

    public class AnalysisResult
    {
    }
}

/// <summary>
/// High-throughput network monitor.
/// </summary>
#region NetworkMonitorClass
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
#endregion

/// <summary>
/// Connection pooling for drivers.
/// </summary>
#region DriverPoolClass
public class DriverPool
{
    private readonly Stack<BiDiDriver> availableDrivers = new();
    private readonly int maxPoolSize = 5;
    private readonly SemaphoreSlim semaphore;

    public DriverPool()
    {
        semaphore = new SemaphoreSlim(maxPoolSize, maxPoolSize);
    }

    public async Task<BiDiDriver> AcquireAsync(string webSocketUrl)
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
#endregion

/// <summary>
/// Managed observer with automatic cleanup.
/// </summary>
#region ManagedObserverClass
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
#endregion

/// <summary>
/// Performance tracking helper.
/// </summary>
#region PerformanceTrackerClass
public class PerformanceTracker
{
    private readonly Dictionary<string, List<TimeSpan>> metrics = new();

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
#endregion

/// <summary>
/// Cached context info.
/// </summary>
#region CachedContextInfoClass
public class CachedContextInfo
{
    private readonly Dictionary<string, BrowsingContextInfo> cache = new();
    private readonly BiDiDriver driver;

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
#endregion
