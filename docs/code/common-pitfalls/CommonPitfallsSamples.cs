// <copyright file="CommonPitfallsSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/common-pitfalls.md

#pragma warning disable CS8600, CS8602

namespace WebDriverBiDi.Docs.Code.CommonPitfalls;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Emulation;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for common pitfalls documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class CommonPitfallsSamples
{
    /// <summary>
    /// BAD: Blocking the transport thread with synchronous handler.
    /// </summary>
    public static void BlockingHandler(BiDiDriver driver)
    {
#region BlockingHandler
        // ❌ BAD: Blocks transport thread for 5 seconds
        driver.Network.OnBeforeRequestSent.AddObserver((e) =>
        {
            Thread.Sleep(5000);
            Console.WriteLine($"Request: {e.Request.Url}");

            // During these 5 seconds:
            // - No other events are processed
            // - No command responses are received
            // - Commands may timeout
            // - The browser may become unresponsive
        });
#endregion
    }

    /// <summary>
    /// GOOD: Asynchronous handler that does not block.
    /// </summary>
    public static void NonBlockingHandler(BiDiDriver driver)
    {
#region Non-BlockingHandler
        // ✅ GOOD: Runs asynchronously without blocking
        driver.Network.OnBeforeRequestSent.AddObserver(
            async (e) =>
            {
                await Task.Delay(5000);  // Doesn't block transport thread
                Console.WriteLine($"Request: {e.Request.Url}");

                // Transport thread continues processing other messages
                // while this handler runs on a task pool thread
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously
        );
#endregion
    }

    /// <summary>
    /// Fine: Quick in-memory operation - synchronous is acceptable.
    /// </summary>
    public static void QuickSynchronousHandler(BiDiDriver driver)
    {
#region QuickSynchronousHandler
        // ✅ Fine: Quick in-memory operation
        int requestCount = 0;
        driver.Network.OnBeforeRequestSent.AddObserver((e) =>
        {
            requestCount++;
        });

        // ✅ Fine: Simple logging to console
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            Console.WriteLine($"[{e.Level}] {e.Text}");
        });
#endregion
    }

    /// <summary>
    /// INCOMPLETE: Observer added but no Session.SubscribeAsync - no events received.
    /// </summary>
    public static async Task IncompleteSubscription(BiDiDriver driver, NavigateCommandParameters navParams)
    {
#region IncompleteSubscription
        // ❌ INCOMPLETE: Observer added but no events will be received
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            Console.WriteLine(e.Text);
        });

        await driver.BrowsingContext.NavigateAsync(navParams);
        // No log events will fire - you forgot to subscribe!
#endregion
    }

    /// <summary>
    /// CORRECT: Add observer AND subscribe (two-step process).
    /// </summary>
    public static async Task TwoStepSubscription(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
#region Two-StepSubscription
        // ✅ CORRECT: Add observer AND subscribe
        // Step 1: Add observer
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            Console.WriteLine(e.Text);
        });

        // Step 2: Subscribe to events
        SubscribeCommandParameters subscribe = new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        // Now events will be received
        await driver.BrowsingContext.NavigateAsync(navParams);
#endregion
    }

    /// <summary>
    /// Subscribe multiple events at once.
    /// </summary>
    public static async Task SubscribeMultipleEvents(
        BiDiDriver driver,
        Action<EntryAddedEventArgs> logHandler,
        Action<BeforeRequestSentEventArgs> networkHandler,
        Action<NavigationEventArgs> loadHandler)
    {
#region SubscribeMultipleEvents
        // Add all observers first
        driver.Log.OnEntryAdded.AddObserver(logHandler);
        driver.Network.OnBeforeRequestSent.AddObserver(networkHandler);
        driver.BrowsingContext.OnLoad.AddObserver(loadHandler);

        // Then subscribe to all events in one call
        SubscribeCommandParameters subscribe = new SubscribeCommandParameters(
            [
                driver.Log.OnEntryAdded.EventName,
                driver.Network.OnBeforeRequestSent.EventName,
                driver.BrowsingContext.OnLoad.EventName,
            ]
        );
        await driver.Session.SubscribeAsync(subscribe);
#endregion
    }

    /// <summary>
    /// WRONG: Registration after StartAsync throws.
    /// </summary>
    public static async Task WrongRegistrationOrder(string webSocketUrl, Module customModule, Action<EntryAddedEventArgs> handler)
    {
#region WrongRegistrationOrder
        // ❌ WRONG: Registration after starting
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        // This will throw InvalidOperationException!
        driver.RegisterModule(new CustomModule(driver));
        driver.Log.OnEntryAdded.AddObserver(handler);  // May also fail
#endregion
    }

    /// <summary>
    /// CORRECT: Registration before StartAsync.
    /// </summary>
    public static async Task CorrectRegistrationOrder(
        string webSocketUrl,
        Module customModule,
        NavigateCommandParameters navParams)
    {
#region CorrectRegistrationOrder
        // ✅ CORRECT: Registration before starting
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        // 1. Register custom modules (if any)
        driver.RegisterModule(new CustomModule(driver));

        // 2. Add event observers
        driver.Log.OnEntryAdded.AddObserver((e) => Console.WriteLine(e.Text));
        driver.BrowsingContext.OnLoad.AddObserver((e) => Console.WriteLine($"Loaded: {e.Url}"));

        // 3. NOW start the driver
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        // 4. Subscribe to events through Session module
        SubscribeCommandParameters subscribe = new SubscribeCommandParameters(
            [
                driver.Log.OnEntryAdded.EventName,
                driver.BrowsingContext.OnLoad.EventName,
            ]
        );
        await driver.Session.SubscribeAsync(subscribe);

        // 5. Execute commands
        await driver.BrowsingContext.NavigateAsync(navParams);
#endregion
    }

    /// <summary>
    /// Nullable collection - null vs empty list protocol difference.
    /// </summary>
    public static void NullableCollectionExample()
    {
#region NullableCollectionExample
        SetLocaleOverrideCommandParameters parameters = new SetLocaleOverrideCommandParameters()
        {
            Locale = "en-US",
        };

        // Why is Contexts null instead of an empty list?
        if (parameters.Contexts == null)  // This is true!
        {
            parameters.Contexts = new List<string>();
        }
#endregion
    }

    /// <summary>
    /// Null vs empty vs items - protocol JSON difference.
    /// </summary>
    public static void NullVsEmptyVsItems()
    {
#region NullvsEmptyvsItems
        // Case 1: Contexts is null
        SetLocaleOverrideCommandParameters p1 = new SetLocaleOverrideCommandParameters()
        {
            Locale = "en-US",
        };
        // JSON sent: { "locale": "en-US" /* no "contexts" property */ }

        // Case 2: Contexts is empty list
        SetLocaleOverrideCommandParameters p2 = new SetLocaleOverrideCommandParameters()
        {
            Locale = "en-US",
        };
        p2.Contexts = new List<string>();
        // JSON sent: { "locale": "en-US",  "contexts": [] }

        // Case 3: Events has items
        SetLocaleOverrideCommandParameters p3 = new SetLocaleOverrideCommandParameters()
        {
            Locale = "en-US",
        };
        p3.Contexts = new List<string> { "<valid browsing context ID>" };
        // JSON sent: { "locale": "en-US",  "contexts": ["<valid browsing context ID>"] }
#endregion
    }

    /// <summary>
    /// Handle nullable collections - three options.
    /// </summary>
    public static void HandleNullableCollections(SetLocaleOverrideCommandParameters parameters)
    {
#region HandleNullableCollections
        // ✅ Option 1: Null-conditional + null-coalescing
        parameters.Contexts ??= new List<string>();
        parameters.Contexts.Add("<valid browsing context ID>");

        // ✅ Option 2: Check before adding
        if (parameters.Contexts == null)
        {
            parameters.Contexts = new List<string>();
        }
        parameters.Contexts.Add("<valid browsing context ID>");

        // ✅ Option 3: Initialize in one line
        parameters.Contexts = new List<string>
        {
            "<valid browsing context ID>",
            "<another valid browsing context ID>"
        };
#endregion
    }

    /// <summary>
    /// Default 60 second timeout.
    /// </summary>
    public static async Task DefaultTimeout(NavigateCommandParameters navParams)
    {
#region DefaultTimeout
        // Default timeout is 60 seconds!
        BiDiDriver driver = new BiDiDriver();

        // This command has 60 seconds to complete
        await driver.BrowsingContext.NavigateAsync(navParams);
#endregion
    }

    /// <summary>
    /// Configure timeouts for different scenarios.
    /// </summary>
    public static async Task ConfigureTimeouts(
        NavigateCommandParameters navParams,
        NavigateCommandParameters slowPageParams)
    {
#region ConfigureTimeouts
        // ✅ For fast operations (local testing)
        BiDiDriver fastDriver = new BiDiDriver(TimeSpan.FromSeconds(10));

        // ✅ For normal web automation
        BiDiDriver normalDriver = new BiDiDriver(TimeSpan.FromSeconds(30));

        // ✅ For slow operations or large pages
        BiDiDriver slowDriver = new BiDiDriver(TimeSpan.FromMinutes(2));

        // ✅ Override per-command for specific cases (preferred: use timeoutOverride on module method)
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        // Most commands use 30 second timeout

        // But this specific navigation gets longer timeout
        await driver.BrowsingContext.NavigateAsync(
            slowPageParams,
            TimeSpan.FromMinutes(5));
#endregion
    }

    /// <summary>
    /// Async handler may not finish before program exits.
    /// </summary>
    public static async Task AsyncHandlerProblem(BiDiDriver driver, SubscribeCommandParameters subscribeParams, NavigateCommandParameters navParams)
    {
#region AsyncHandlerProblem
        // ❌ PROBLEM: Handler might not finish before program exits
        driver.Network.OnBeforeRequestSent.AddObserver(
            async (e) =>
            {
                await Task.Delay(5000);  // Long-running operation
                await SaveRequestToFileAsync(e.Request);
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously
        );

        await driver.Session.SubscribeAsync(subscribeParams);
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Navigation completes, but handlers might still be running!
        // If program exits here, handlers may not finish
#endregion
    }

    private static async Task SaveRequestToFileAsync(RequestData request)
    {
        // Simulate saving request to file
        await Task.Delay(1000);
    }

    /// <summary>
    /// Use WaitForCheckpointAndTasksAsync for async handlers.
    /// </summary>
    public static async Task WaitForCheckpointAndTasks(
        BiDiDriver driver,
        SubscribeCommandParameters subscribeParams,
        NavigateCommandParameters navParams,
        Func<BeforeRequestSentEventArgs, Task> saveRequest)
    {
#region WaitForCheckpointAndTasks
        // ✅ GOOD: Use built-in helper
        EventObserver<BeforeRequestSentEventArgs> observer =
            driver.Network.OnBeforeRequestSent.AddObserver(
                async (e) =>
                {
                    await Task.Delay(5000);
                    await SaveRequestToFileAsync(e.Request);
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously
            );

        await driver.Session.SubscribeAsync(subscribeParams);

        // Set checkpoint for expected number of events
        observer.SetCheckpoint(5);

        // Trigger events
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Wait for events to occur AND handlers to complete
        await observer.WaitForCheckpointAndTasksAsync(TimeSpan.FromSeconds(10));

        // Now all handlers have completed
#endregion
    }

    /// <summary>
    /// Manual synchronization with GetCheckpointTasks.
    /// </summary>
    public static async Task ManualSynchronization(
        BiDiDriver driver,
        SubscribeCommandParameters subscribeParams,
        NavigateCommandParameters navParams,
        Func<BeforeRequestSentEventArgs, Task> saveRequest)
    {
#region ManualSynchronization
        // ✅ GOOD: Manual synchronization for complex scenarios
        EventObserver<BeforeRequestSentEventArgs> observer =
            driver.Network.OnBeforeRequestSent.AddObserver(
                async (e) =>
                {
                    await SaveRequestToFileAsync(e.Request);
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously
            );

        await driver.Session.SubscribeAsync(subscribeParams);

        observer.SetCheckpoint(5);
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Wait for events to occur
        bool fulfilled = await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(10));

        if (fulfilled)
        {
            // Get handler tasks
            Task[] handlerTasks = observer.GetCheckpointTasks();

            // Inspect or manipulate tasks if needed
            Console.WriteLine($"Waiting for {handlerTasks.Length} handlers to complete...");

            // Wait for all handlers to finish
            await Task.WhenAll(handlerTasks);
        }
#endregion
    }

    /// <summary>
    /// TaskCompletionSource for custom handler synchronization.
    /// </summary>
    public static async Task TaskCompletionSourceSynchronization(
        BiDiDriver driver,
        SubscribeCommandParameters subscribeParams,
        NavigateCommandParameters navParams,
        Func<BeforeRequestSentEventArgs, Task> saveRequest)
    {
#region TaskCompletionSourceSynchronization
        // ✅ GOOD: TaskCompletionSource for fine-grained control
        List<Task> completionTasks = new();

        driver.Network.OnBeforeRequestSent.AddObserver(
            async (e) =>
            {
                TaskCompletionSource tcs = new();
                completionTasks.Add(tcs.Task);

                try
                {
                    await SaveRequestToFileAsync(e.Request);
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously
        );

        await driver.Session.SubscribeAsync(subscribeParams);
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Wait for all handlers
        await Task.WhenAll(completionTasks);
#endregion
    }

    /// <summary>
    /// Handler exceptions ignored by default.
    /// </summary>
    public static async Task HandlerExceptionsIgnored(BiDiDriver driver, SubscribeCommandParameters subscribeParams, NavigateCommandParameters navParams, Action<EntryAddedEventArgs> processLogEntry)
    {
#region HandlerExceptionsIgnored
        // ❌ PROBLEM: Handler exceptions are silently ignored by default
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            // If this throws, the exception is IGNORED by default
            ProcessLogEntry(e);  // Might throw
        });

        await driver.Session.SubscribeAsync(subscribeParams);
        await driver.BrowsingContext.NavigateAsync(navParams);
        // If handler threw, you'll never know!
#endregion
    }

    private static void ProcessLogEntry(EntryAddedEventArgs e)
    {
        // Simulate processing that might throw
        if (e.Text.Contains("error"))
        {
            throw new Exception("Error log entry!");
        }
    }

    /// <summary>
    /// Terminate mode - surface handler exceptions.
    /// </summary>
    public static async Task TerminateErrorBehavior(
        string webSocketUrl,
        Action<EntryAddedEventArgs> processLogEntry,
        SubscribeCommandParameters subscribeParams,
        NavigateCommandParameters navParams)
    {
#region TerminateErrorBehavior
        // ✅ Option 1: Terminate mode (throws on next command)
        WebSocketConnection connection = new WebSocketConnection();
        Transport transport = new Transport(connection);

        // Change error behavior
        transport.EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate;
        transport.ProtocolErrorBehavior = TransportErrorBehavior.Terminate;

        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);

        try
        {
            await driver.StartAsync("ws://localhost:9222");

            // Add handler that might throw
            driver.Log.OnEntryAdded.AddObserver((e) => ProcessLogEntry(e));

            await driver.Session.SubscribeAsync(subscribeParams);

            // If handler throws, exception surfaces here on next command
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"Event handler error: {ex.Message}");
        }
#endregion
    }

    /// <summary>
    /// Collect mode - gather errors.
    /// </summary>
    public static async Task CollectErrorBehavior(
        string webSocketUrl,
        Action<EntryAddedEventArgs> processLogEntry,
        SubscribeCommandParameters subscribeParams,
        NavigateCommandParameters navParams)
    {
#region CollectErrorBehavior
        // ✅ Option 2: Collect mode (gather all errors)
        WebSocketConnection connection = new WebSocketConnection();
        Transport transport = new Transport(connection);

        transport.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
        transport.ProtocolErrorBehavior = TransportErrorBehavior.Collect;

        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);

        await driver.StartAsync("ws://localhost:9222");
        driver.Log.OnEntryAdded.AddObserver((e) => ProcessLogEntry(e));
        await driver.Session.SubscribeAsync(subscribeParams);
        await driver.BrowsingContext.NavigateAsync(navParams);

        try
        {
            await driver.StopAsync();
        }
        catch (AggregateException ex)
        {
            // Check collected errors
            if (ex.InnerExceptions.Count > 0)
            {
                Console.WriteLine($"Collected {ex.InnerExceptions.Count} errors:");
                foreach (var error in ex.InnerExceptions)
                {
                    Console.WriteLine($"  - {error.Message}");
                }
            }
        }
        finally
        {
            await driver.DisposeAsync();
        }
#endregion
    }

    /// <summary>
    /// Handle exceptions inside handlers.
    /// </summary>
    public static void HandleExceptionsInHandlers(BiDiDriver driver, Action<EntryAddedEventArgs> processLogEntry)
    {
#region HandleExceptionsInHandlers
        // ✅ BEST: Handle exceptions inside handlers
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            try
            {
                ProcessLogEntry(e);
            }
            catch (Exception ex)
            {
                // Log error, but don't let it propagate
                Console.WriteLine($"Error processing log entry: {ex.Message}");
            }
        });
#endregion
    }

    /// <summary>
    /// Thread safety - sequential setup, parallel commands.
    /// </summary>
    public static async Task ThreadSafetyExample(
        BiDiDriver driver,
        string url,
        Module module1,
        Module module2,
        Action<EntryAddedEventArgs> handler1,
        Action<EntryAddedEventArgs> handler2,
        NavigateCommandParameters params1,
        NavigateCommandParameters params2)
    {
#region ThreadSafetyExample
        // ✅ GOOD: Register modules before concurrent operations
        driver.RegisterModule(module1);
        driver.RegisterModule(module2);
        await driver.StartAsync(url);

        // ✅ GOOD: Add observers sequentially during setup
        driver.Log.OnEntryAdded.AddObserver(handler1);
        driver.Log.OnEntryAdded.AddObserver(handler2);

        // ✅ GOOD: Execute commands concurrently (this IS safe)
        Task<NavigateCommandResult> nav1 =
            driver.BrowsingContext.NavigateAsync(params1);
        Task<NavigateCommandResult> nav2 =
            driver.BrowsingContext.NavigateAsync(params2);
        await Task.WhenAll(nav1, nav2);

        // ✅ GOOD: Use locks for shared state in handlers
        object stateLock = new();
        int counter = 0;

        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            lock (stateLock)
            {
                counter++;
            }
        });
#endregion
    }

    /// <summary>
    /// BAD: No cleanup of observer and driver.
    /// </summary>
    public static async Task BadCleanup(string url, Action<EntryAddedEventArgs> handler)
    {
#region BadCleanup
        // ❌ BAD: No cleanup
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync(url);

        EventObserver<EntryAddedEventArgs> observer =
            driver.Log.OnEntryAdded.AddObserver(handler);

        // ... use driver ...

        // Oops! Never stopped driver or removed observer
        // Resources leaked!
#endregion
    }

    /// <summary>
    /// GOOD: Proper cleanup with try-finally.
    /// </summary>
    public static async Task GoodCleanup(
        string url,
        Action<EntryAddedEventArgs> handler,
        SubscribeCommandParameters subscribeParams)
    {
#region GoodCleanup
        // ✅ GOOD: Proper cleanup
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        try
        {
            await driver.StartAsync(url);

            EventObserver<EntryAddedEventArgs> observer =
                driver.Log.OnEntryAdded.AddObserver(handler);

            try
            {
                await driver.Session.SubscribeAsync(subscribeParams);

                // ... use driver ...
            }
            finally
            {
                // Remove observer when done
                observer.Unobserve();
            }
        }
        finally
        {
            // Always stop driver
            if (driver.IsStarted)
            {
                await driver.StopAsync();
            }
        }
#endregion
    }

    /// <summary>
    /// BETTER: Use async disposal.
    /// </summary>
    public static async Task BetterCleanup(string url, Action<EntryAddedEventArgs> handler)
    {
#region BetterCleanup
        // ✅ BETTER: Use async disposal
        await using BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync(url);

        using EventObserver<EntryAddedEventArgs> observer =
            driver.Log.OnEntryAdded.AddObserver(handler);

        // Automatic cleanup when scope exits
#endregion
    }

    private class CustomModule : Module
    {
        public CustomModule(BiDiDriver driver) : base(driver)
        {
        }

        public override string ModuleName => throw new NotImplementedException();
    }
}
