// <copyright file="EventObserverSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/events-observables.md

#pragma warning disable CS1591, CS0168, CS0219, CS8600, CS8602

namespace WebDriverBiDi.Docs.Code.EventsObservables;

using System.Collections.Generic;
using OpenQA.Selenium.DevTools.V129.Browser;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for event observer documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class EventObserverSamples
{
    /// <summary>
    /// Complete example - add observer, subscribe, navigate.
    /// </summary>
    public static async Task CompleteExample(BiDiDriver driver, string contextId)
    {
#region CompleteExample
        // Step 1: Add observer
        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            Console.WriteLine($"Console: {e.Text}");
        });

        // Step 2: Subscribe to events
        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        // Step 3: Events will now trigger your observer
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com"));
#endregion
    }

    /// <summary>
    /// Event name property for subscription.
    /// </summary>
    public static void EventNames(BiDiDriver driver)
    {
#region EventNames
        Console.WriteLine(driver.Log.OnEntryAdded.EventName);
        // Output: "log.entryAdded"

        Console.WriteLine(driver.Network.OnBeforeRequestSent.EventName);
        // Output: "network.beforeRequestSent"
#endregion
    }

    /// <summary>
    /// Simple observer with explicit type.
    /// </summary>
    public static void SimpleObserver(BiDiDriver driver)
    {
#region SimpleObserver
        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            Console.WriteLine($"Level: {e.Level}");
            Console.WriteLine($"Text: {e.Text}");
            Console.WriteLine($"Timestamp: {e.Timestamp}");
        });
#endregion
    }

    /// <summary>
    /// Observer with type inference.
    /// </summary>
    public static void ObserverWithTypeInference(BiDiDriver driver)
    {
#region ObserverwithTypeInference
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            // Type is inferred as EntryAddedEventArgs
            Console.WriteLine(e.Text);
        });
#endregion
    }

    /// <summary>
    /// Async observer for long-running operations.
    /// </summary>
    public static void AsyncObserver(BiDiDriver driver)
    {
#region AsyncObserver
        driver.Network.OnBeforeRequestSent.AddObserver(
            async (BeforeRequestSentEventArgs e) =>
            {
                // Can use await
                await LogRequestAsync(e.Request.Url);
                await Task.Delay(100);
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);
#endregion
    }

    /// <summary>
    /// Basic cleanup with try/finally.
    /// </summary>
    public static async Task BasicCleanupTryFinally(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
#region BasicCleanuptry-finally
        EventObserver<EntryAddedEventArgs> observer =
            driver.Log.OnEntryAdded.AddObserver((e) =>
            {
                Console.WriteLine(e.Text);
            });

        try
        {
            SubscribeCommandParameters subscribe = 
                new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName);
            await driver.Session.SubscribeAsync(subscribe);

            // Use the driver...
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        finally
        {
            // Remove observer when done to prevent memory leaks
            observer.Unobserve();
        }
#endregion
    }

    /// <summary>
    /// Using statement for automatic cleanup.
    /// </summary>
    public static async Task UsingStatementCleanup(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
#region UsingStatementCleanup
        using EventObserver<EntryAddedEventArgs> observer =
            driver.Log.OnEntryAdded.AddObserver((e) =>
            {
                Console.WriteLine(e.Text);
            });

        SubscribeCommandParameters subscribe = 
            new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        // Use the driver...
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Observer automatically removed when scope exits
#endregion
    }

    /// <summary>
    /// Cleanup when using checkpoints.
    /// </summary>
    public static async Task CleanupWithCheckpoints(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
#region CleanupwithCheckpoints
        EventObserver<NavigationEventArgs> observer =
            driver.BrowsingContext.OnLoad.AddObserver((e) =>
            {
                Console.WriteLine($"Loaded: {e.Url}");
            });

        try
        {
            SubscribeCommandParameters subscribe = 
                new SubscribeCommandParameters(driver.BrowsingContext.OnLoad.EventName);
            await driver.Session.SubscribeAsync(subscribe);

            observer.SetCheckpoint();
            await driver.BrowsingContext.NavigateAsync(navParams);
            bool loaded = await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(30));
        }
        finally
        {
            observer.Unobserve();
        }
#endregion
    }

    /// <summary>
    /// Checkpoint reset - use for multiple navigations.
    /// </summary>
    public static async Task CheckpointReset(
        BiDiDriver driver,
        NavigateCommandParameters params1,
        NavigateCommandParameters params2)
    {
#region CheckpointReset
        EventObserver<EntryAddedEventArgs> observer = 
            driver.Log.OnEntryAdded.AddObserver((e) => { });

        // First navigation
        observer.SetCheckpoint(3);
        await driver.BrowsingContext.NavigateAsync(params1);
        await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(5));

        // Second navigation - reset checkpoint
        observer.SetCheckpoint(2);
        await driver.BrowsingContext.NavigateAsync(params2);
        await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(5));
#endregion
    }

    /// <summary>
    /// Synchronous handlers (default behavior).
    /// </summary>
    public static void SynchronousHandlers(BiDiDriver driver)
    {
#region SynchronousHandlers
        // Default behavior - runs synchronously
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            // This runs on the transport thread
            // Blocks all message processing until complete
            Console.WriteLine(e.Text);
        });
#endregion
    }

    /// <summary>
    /// Blocking problem - BAD example.
    /// </summary>
    public static void BlockingProblem(BiDiDriver driver)
    {
#region BlockingProblem
        // ❌ BAD: Handler blocks message processing
        driver.Network.OnBeforeRequestSent.AddObserver((e) =>
        {
            // This blocks the Transport thread for 5 seconds!
            Thread.Sleep(5000);
            Console.WriteLine($"Request: {e.Request.Url}");

            // During these 5 seconds:
            // - No other events are processed
            // - No responses are received
            // - Commands may timeout
            // - Browser may become unresponsive
        });
#endregion
    }

    /// <summary>
    /// Asynchronous handlers - GOOD example.
    /// </summary>
    public static void AsynchronousHandlers(BiDiDriver driver)
    {
#region AsynchronousHandlers
        // ✅ GOOD: Handler runs asynchronously
        EventObserver<BeforeRequestSentEventArgs> observer =
            driver.Network.OnBeforeRequestSent.AddObserver(
                async (e) =>
                {
                    // Doesn't block message processing
                    await Task.Delay(5000);
                    Console.WriteLine($"Request: {e.Request.Url}");

                    // During these 5 seconds:
                    // - Transport thread continues processing
                    // - Other events are handled normally
                    // - Handler runs on Task pool thread
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously
            );
#endregion
    }

    /// <summary>
    /// Quick operations - synchronous is fine.
    /// </summary>
    public static void QuickOperations(BiDiDriver driver)
    {
#region QuickOperations
        // Counter - quick in-memory operation
        int requestCount = 0;
        driver.Network.OnBeforeRequestSent.AddObserver((e) =>
        {
            requestCount++;  // Fast, synchronous is fine
        });

        // List collection - quick in-memory operation
        List<string> urls = new List<string>();
        driver.Network.OnResponseCompleted.AddObserver((e) =>
        {
            urls.Add(e.Response.Url);  // Quick, synchronous is fine
        });
#endregion
    }

    /// <summary>
    /// I/O operations - use async.
    /// </summary>
    public static void IoOperations(BiDiDriver driver, DbContext dbContext)
    {
#region IOOperations
        // File I/O - use async
        driver.Log.OnEntryAdded.AddObserver(
            async (e) =>
            {
                await File.AppendAllTextAsync("log.txt", $"{e.Text}\n");
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously
        );

        // Database operations - use async
        driver.Log.OnEntryAdded.AddObserver(
            async (e) =>
            {
                await dbContext.Logs.AddAsync(new DbLogEntry
                {
                    Level = e.Level,
                    Message = e.Text,
                    Timestamp = e.Timestamp
                });
                await dbContext.SaveChangesAsync();
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously
        );
#endregion
    }

    /// <summary>
    /// WaitForCheckpointAndTasksAsync - recommended for async handlers.
    /// </summary>
    public static async Task WaitForCheckpointAndTasksAsync(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
#region WaitForCheckpointAndTasksAsync
        EventObserver<BeforeRequestSentEventArgs> observer =
            driver.Network.OnBeforeRequestSent.AddObserver(
                async (e) =>
                {
                    await ProcessRequestAsync(e);
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously
            );

        // Set checkpoint for 3 events
        observer.SetCheckpoint(3);

        // Trigger events
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Wait for events to occur AND all handlers to complete
        bool occurred = await observer.WaitForCheckpointAndTasksAsync(TimeSpan.FromSeconds(10));

        if (occurred)
        {
            Console.WriteLine("All 3 events occurred and their handlers completed");
        }
        else
        {
            Console.WriteLine("Timeout waiting for events");
        }
#endregion
    }

    /// <summary>
    /// Manual synchronization with GetCheckpointTasks.
    /// </summary>
    public static async Task ManualSynchronization(
        BiDiDriver driver,
        NavigateCommandParameters navParams,
        Func<BeforeRequestSentEventArgs, Task> processRequestAsync)
    {
#region ManualSynchronization
        EventObserver<BeforeRequestSentEventArgs> observer =
            driver.Network.OnBeforeRequestSent.AddObserver(
                async (e) =>
                {
                    await ProcessRequestAsync(e);
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously
            );

        // Set checkpoint for 3 events
        observer.SetCheckpoint(3);

        // Trigger events
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Wait for events to occur
        bool occurred = await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(10));

        if (occurred)
        {
            // Get handler tasks for inspection or custom handling
            Task[] handlerTasks = observer.GetCheckpointTasks();

            Console.WriteLine($"Waiting for {handlerTasks.Length} handlers to complete...");

            // Wait for all async handlers to complete
            await Task.WhenAll(handlerTasks);
            Console.WriteLine("All handlers completed");
        }
#endregion
    }

    /// <summary>
    /// TaskCompletionSource for complex synchronization.
    /// </summary>
    public static async Task TaskCompletionSourceSynchronization(
        BiDiDriver driver,
        string contextId,
        Func<BeforeRequestSentEventArgs, Task> processRequestAsync)
    {
#region TaskCompletionSourceSynchronization
        List<Task> handlerTasks = new();

        EventObserver<BeforeRequestSentEventArgs> observer =
            driver.Network.OnBeforeRequestSent.AddObserver(
                async (e) =>
                {
                    TaskCompletionSource taskCompletionSource = new();
                    handlerTasks.Add(taskCompletionSource.Task);

                    try
                    {
                        Console.WriteLine($"Processing request: {e.Request.Url}");

                        // Long-running operation
                        await Task.Delay(TimeSpan.FromSeconds(4));
                        await ProcessRequestAsync(e);

                        Console.WriteLine($"Completed request: {e.Request.Url}");
                        taskCompletionSource.SetResult();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Handler error: {ex.Message}");
                        taskCompletionSource.SetException(ex);
                    }
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously
            );

        // Subscribe to events
        SubscribeCommandParameters subscribe = new(driver.Network.OnBeforeRequestSent.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        // Set checkpoint for expected number of events (e.g., 5 requests)
        observer.SetCheckpoint(5);

        // Trigger navigation
        NavigateCommandParameters navParams = new(contextId, "https://example.com")
        {
            Wait = ReadinessState.Complete
        };
        NavigateCommandResult navigation = await driver.BrowsingContext.NavigateAsync(navParams);
        Console.WriteLine("Navigation command completed");

        // Important: The navigation command completes before handlers finish
        // Wait for all events to occur
        bool occurred = await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(10));

        if (occurred)
        {
            // Wait for all async handlers to complete
            await Task.WhenAll(handlerTasks);
            Console.WriteLine("All event handlers completed");
        }
        else
        {
            Console.WriteLine("Timeout waiting for events");
        }
#endregion
    }

    /// <summary>
    /// Without synchronization - problem and solution.
    /// </summary>
    public static async Task WithoutSynchronizationProblem(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
#region WithoutSynchronizationProblem
        // ❌ PROBLEM: Main thread may exit before handlers complete
        EventObserver<BeforeRequestSentEventArgs> badObserver =
            driver.Network.OnBeforeRequestSent.AddObserver(
                async (e) =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));  // Long operation
                    Console.WriteLine($"Request: {e.Request.Url}");
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously
            );

        badObserver.SetCheckpoint(5);
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Navigation completes...
        // Main code continues...
        // Application might exit before handlers finish!

        // ✅ SOLUTION: Use TaskCompletionSource and wait
        List<Task> handlerTasks = new();

        EventObserver<BeforeRequestSentEventArgs> goodObserver =
            driver.Network.OnBeforeRequestSent.AddObserver(
                async (e) =>
                {
                    TaskCompletionSource tcs = new();
                    handlerTasks.Add(tcs.Task);

                    await Task.Delay(TimeSpan.FromSeconds(5));
                    Console.WriteLine($"Request: {e.Request.Url}");

                    tcs.SetResult();
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously
            );

        goodObserver.SetCheckpoint(5);
        await driver.BrowsingContext.NavigateAsync(navParams);

        bool occurred = await goodObserver.WaitForCheckpointAsync(TimeSpan.FromSeconds(10));
        if (occurred)
        {
            // Wait for all handlers to complete before continuing
            await Task.WhenAll(handlerTasks);
        }
#endregion
    }

    /// <summary>
    /// Calling commands in event handlers - requires async mode.
    /// </summary>
    public static void CallingCommandsInHandlers(BiDiDriver driver)
    {
#region CallingCommandsinHandlers
        EventObserver<BeforeRequestSentEventArgs> observer = 
            driver.Network.OnBeforeRequestSent.AddObserver(
                async (e) =>
                {
                    if (e.IsBlocked)
                    {
                        // Can call commands in async handler
                        ProvideResponseCommandParameters provideResponse = 
                            new ProvideResponseCommandParameters(e.Request.RequestId)
                            {
                                StatusCode = 404,
                                ReasonPhrase = "Not Found"
                            };
                        
                        await driver.Network.ProvideResponseAsync(provideResponse);
                    }
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously // MUST use async mode to call commands
            );
#endregion
    }

    /// <summary>
    /// Event filtering - filter by level.
    /// </summary>
    public static void EventFiltering(BiDiDriver driver)
    {
#region EventFiltering
        // Only log errors
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            if (e.Level == LogLevel.Error)
            {
                Console.WriteLine($"ERROR: {e.Text}");
            }
        });

        // Only log HTML requests
        driver.Network.OnResponseCompleted.AddObserver((e) =>
        {
            if (e.Response.Url.EndsWith(".html"))
            {
                Console.WriteLine($"HTML page: {e.Response.Url}");
            }
        });
#endregion
    }

    /// <summary>
    /// Multiple observers for same event.
    /// </summary>
    public static void MultipleObservers(BiDiDriver driver)
    {
#region MultipleObservers
        // Observer 1: Log to console
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            Console.WriteLine($"Console: {e.Text}");
        });

        // Observer 2: Write to file
        EventObserver<EntryAddedEventArgs> fileLogger = 
            driver.Log.OnEntryAdded.AddObserver(async (e) =>
            {
                await File.AppendAllTextAsync("log.txt", e.Text + "\n");
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        // Observer 3: Count errors
        int errorCount = 0;
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            if (e.Level == LogLevel.Error)
            {
                errorCount++;
            }
        });
#endregion
    }

    /// <summary>
    /// Pattern 1: Wait for page load.
    /// </summary>
    public static async Task Pattern1WaitForPageLoad(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
#region Pattern1-WaitforPageLoad
        // Add observer for page load event
        EventObserver<NavigationEventArgs> observer =
            driver.BrowsingContext.OnLoad.AddObserver((e) => { });

        // Subscribe to the event
        SubscribeCommandParameters subscribe = 
            new SubscribeCommandParameters(driver.BrowsingContext.OnLoad.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        // Set checkpoint and trigger navigation
        observer.SetCheckpoint();
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Wait for page load event (use async version when possible)
        bool loaded = await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(30));
        if (loaded)
        {
            Console.WriteLine("Page loaded successfully");
        }
#endregion
    }

    /// <summary>
    /// Pattern 2: Collect network responses.
    /// </summary>
    public static async Task Pattern2CollectNetworkResponses(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
#region Pattern2-CollectNetworkResponses
        List<ResponseData> responses = new List<ResponseData>();

        driver.Network.OnResponseCompleted.AddObserver((e) =>
        {
            responses.Add(e.Response);
        });

        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Network.OnResponseCompleted.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        await driver.BrowsingContext.NavigateAsync(navParams);

        // Wait a bit for all responses
        await Task.Delay(2000);

        Console.WriteLine($"Collected {responses.Count} responses");
#endregion
    }

    /// <summary>
    /// Pattern 3: Wait for specific condition with preload script.
    /// </summary>
    public static async Task Pattern3WaitForSpecificCondition(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
#region Pattern3-WaitforSpecificCondition
        TaskCompletionSource<RemoteValue> elementFound = 
            new TaskCompletionSource<RemoteValue>();

        driver.Script.OnMessage.AddObserver((e) =>
        {
            if (e.ChannelId == "elementWatcher")
            {
                elementFound.SetResult(e.Data);
            }
        });

        // Preload script watches for element
        string preloadScript = """
            (channel) => {
                const interval = setInterval(() => {
                    const element = document.querySelector('.target');
                    if (element) {
                        clearInterval(interval);
                        channel(element);
                    }
                }, 100);
            }
            """;

        ChannelValue channel = new ChannelValue(
            new ChannelProperties("elementWatcher"));

        AddPreloadScriptCommandParameters preloadParams = 
            new AddPreloadScriptCommandParameters(preloadScript)
            {
                Arguments = new List<ChannelValue> { channel }
            };

        await driver.Script.AddPreloadScriptAsync(preloadParams);
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Wait for element to appear
        RemoteValue element = await elementFound.Task;
        Console.WriteLine($"Element found: {element.SharedId}");
#endregion
    }

    /// <summary>
    /// Pattern 4: Temporary observer.
    /// </summary>
    public static async Task Pattern4TemporaryObserver(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
#region Pattern4-TemporaryObserver
        // Add observer just for one operation
        EventObserver<EntryAddedEventArgs> observer = 
            driver.Log.OnEntryAdded.AddObserver((e) =>
            {
                Console.WriteLine(e.Text);
            });

        // Do something
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Remove observer
        observer.Unobserve();
#endregion
    }

    /// <summary>
    /// NavigationEventArgs properties.
    /// </summary>
    public static void NavigationEventArgsExample(BiDiDriver driver)
    {
#region NavigationEventArgs
        driver.BrowsingContext.OnLoad.AddObserver((NavigationEventArgs e) =>
        {
            string contextId = e.BrowsingContextId;
            string navigationId = e.NavigationId;
            string url = e.Url;
            DateTime timestamp = e.Timestamp;
        });
#endregion
    }

    /// <summary>
    /// EntryAddedEventArgs properties.
    /// </summary>
    public static void EntryAddedEventArgsExample(BiDiDriver driver)
    {
#region EntryAddedEventArgs
        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            LogLevel level = e.Level;          // Error, Warn, Info, Debug
            string text = e.Text;              // Log message
            DateTime timestamp = e.Timestamp;
            string? source = e.Source.RealmId; // JavaScript source location
            List<string> stackLines = new List<string>();
            foreach (StackFrame frame in e.StackTrace.CallFrames)
            {
                stackLines.Add($"{frame.FunctionName} at {frame.Url}:{frame.LineNumber}:{frame.ColumnNumber}");
            }
            string? stackTrace = string.Join("\n", stackLines);
        });
#endregion
    }

    /// <summary>
    /// BeforeRequestSentEventArgs properties.
    /// </summary>
    public static void BeforeRequestSentEventArgsExample(BiDiDriver driver)
    {
#region BeforeRequestSentEventArgs
        driver.Network.OnBeforeRequestSent.AddObserver((e) =>
        {
            string requestId = e.Request.RequestId;
            string url = e.Request.Url;
            string method = e.Request.Method;
            IList<ReadOnlyHeader> headers = e.Request.Headers;
            bool isBlocked = e.IsBlocked;    // True if intercepted
            string contextId = e.BrowsingContextId;
        });
#endregion
    }

    /// <summary>
    /// ResponseCompletedEventArgs properties.
    /// </summary>
    public static void ResponseCompletedEventArgsExample(BiDiDriver driver)
    {
#region ResponseCompletedEventArgs
        driver.Network.OnResponseCompleted.AddObserver((e) =>
        {
            RequestData request = e.Request;
            ResponseData response = e.Response;
            
            string url = response.Url;
            ulong status = response.Status;
            string statusText = response.StatusText;
            IList<ReadOnlyHeader> headers = response.Headers;
        });
#endregion
    }

    /// <summary>
    /// Best practice - add observers before subscribing.
    /// </summary>
    public static async Task BestPracticeAddObserversFirst(
        BiDiDriver driver,
        Action<EntryAddedEventArgs> handler)
    {
#region AddObserversBeforeSubscribing
        // ✅ Recommended: Add observer first, then subscribe
        driver.Log.OnEntryAdded.AddObserver(handler);

        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        // ✅ Also acceptable: Subscribe then add observer (but less clear)
        await driver.Session.SubscribeAsync(subscribe);
        driver.Log.OnEntryAdded.AddObserver(handler);
#endregion
    }

    /// <summary>
    /// Best practice - remove observers when done.
    /// </summary>
    public static async Task BestPracticeRemoveObservers(
        BiDiDriver driver,
        Action<EntryAddedEventArgs> handler)
    {
#region RemoveObserversWhenDone
        EventObserver<EntryAddedEventArgs> observer = 
            driver.Log.OnEntryAdded.AddObserver(handler);

        try
        {
            // Use observer
        }
        finally
        {
            observer.Unobserve();
        }
#endregion
    }

    /// <summary>
    /// Best practice - use async mode for long operations.
    /// </summary>
    public static void BestPracticeAsyncMode(BiDiDriver driver)
    {
#region UseAsyncModeforLongOperations
        // ✅ Good: Won't block message processing
        driver.Network.OnBeforeRequestSent.AddObserver(
            async (e) => await SlowOperationAsync(e),
            ObservableEventHandlerOptions.RunHandlerAsynchronously
        );
#endregion
    }

    private static async Task SlowOperationAsync(BeforeRequestSentEventArgs e) {}

    /// <summary>
    /// Best practice - handle exceptions in observers.
    /// </summary>
    public static void BestPracticeHandleExceptions(
        BiDiDriver driver)
    {
#region HandleExceptionsinObservers
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            try
            {
                ProcessLogEntry(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Observer error: {ex.Message}");
            }
        });
#endregion
    }

    private static async Task LogRequestAsync(string url) {}

    /// <summary>
    /// ObservableEventHandlerOptions enum values.
    /// </summary>
    public static void ObservableEventHandlerOptionsValues()
    {
        _ = ObservableEventHandlerOptions.None;
        _ = ObservableEventHandlerOptions.RunHandlerAsynchronously;
    }

    public class DbContext
    {
        public DbContext()
        {
            Logs = new LogEntrySet();
        }
        public LogEntrySet Logs { get; set; }
        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    public class LogEntrySet
    {
        public Task AddAsync(DbLogEntry entry) => Task.CompletedTask;
    }

    public class DbLogEntry
    {
        public LogLevel Level { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }
    }

    private static async Task ProcessRequestAsync(BeforeRequestSentEventArgs e) {}

    private static void ProcessLogEntry(EntryAddedEventArgs e) {}

    /// <summary>
    /// BrowsingContext module observable events.
    /// </summary>
    public static void BrowsingContextEvents(BiDiDriver driver)
    {
#region BrowsingContextEvents
        _ = driver.BrowsingContext.OnLoad;
        _ = driver.BrowsingContext.OnDomContentLoaded;
        _ = driver.BrowsingContext.OnNavigationStarted;
        _ = driver.BrowsingContext.OnNavigationAborted;
        _ = driver.BrowsingContext.OnNavigationFailed;
        _ = driver.BrowsingContext.OnFragmentNavigated;
        _ = driver.BrowsingContext.OnContextCreated;
        _ = driver.BrowsingContext.OnContextDestroyed;
        _ = driver.BrowsingContext.OnUserPromptOpened;
        _ = driver.BrowsingContext.OnUserPromptClosed;
#endregion
    }

    /// <summary>
    /// Network module observable events.
    /// </summary>
    public static void NetworkEvents(BiDiDriver driver)
    {
#region NetworkEvents
        _ = driver.Network.OnBeforeRequestSent;
        _ = driver.Network.OnResponseStarted;
        _ = driver.Network.OnResponseCompleted;
        _ = driver.Network.OnFetchError;
        _ = driver.Network.OnAuthRequired;
#endregion
    }

    /// <summary>
    /// Log module observable events.
    /// </summary>
    public static void LogEvents(BiDiDriver driver)
    {
#region LogEvents
        _ = driver.Log.OnEntryAdded;
#endregion
    }

    /// <summary>
    /// Script module observable events.
    /// </summary>
    public static void ScriptEvents(BiDiDriver driver)
    {
#region ScriptEvents
        _ = driver.Script.OnMessage;
        _ = driver.Script.OnRealmCreated;
        _ = driver.Script.OnRealmDestroyed;
#endregion
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CS0168 // Variable declared but never used
#pragma warning restore CS0219 // Variable assigned but never used
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type
#pragma warning restore CS8602 // Dereference of a possibly null reference
