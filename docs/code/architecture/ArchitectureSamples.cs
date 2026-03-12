// <copyright file="ArchitectureSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/architecture.md

#pragma warning disable CS8600, CS8602, CS1591, CS8604

namespace WebDriverBiDi.Docs.Code.Architecture;

using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Session;
using WebDriverBiDi.Client.Launchers;

/// <summary>
/// Snippets for architecture documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class ArchitectureSamples
{
    /// <summary>
    /// WebSocket connection example.
    /// </summary>
    public static async Task WebSocketExample(NavigateCommandParameters navParams)
    {
#region WebSocketExample
        // Connect to browser at WebSocket URL
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync("ws://localhost:9222/devtools/browser/abc-123");

        try
        {
            // Use the driver
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        finally
        {
            await driver.StopAsync();
        }
#endregion
    }

    /// <summary>
    /// Pipe connection - implement IPipeServerProcessProvider.
    /// </summary>
    public static async Task PipeExample(NavigateCommandParameters navParams)
    {
#region PipeExample
        // Launcher implements IPipeServerProcessProvider
        ChromeLauncher launcher = new ChromeLauncher()
        {
            ConnectionType = ConnectionType.Pipes
        };

        await launcher.StartAsync();
        await launcher.LaunchBrowserAsync();

        try
        {
            // Create driver with launcher's connection
            BiDiDriver driver = new BiDiDriver(
                TimeSpan.FromSeconds(30),
                launcher.CreateTransport());

            await driver.StartAsync("pipes");

            // Use the driver
            await driver.BrowsingContext.NavigateAsync(navParams);

            await driver.StopAsync();
        }
        finally
        {
            await launcher.QuitBrowserAsync();
            await launcher.StopAsync();
        }
#endregion
    }

    /// <summary>
    /// Command pattern - create params and execute.
    /// </summary>
    public static async Task CommandPattern(BiDiDriver driver, string contextId, string url)
    {
#region CommandPattern
        // 1. Create parameters (mutable)
        NavigateCommandParameters parameters = new NavigateCommandParameters(contextId, url);
        parameters.Wait = ReadinessState.Complete;

        // 2-11. Execute command (returns immutable result)
        NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(parameters);
#endregion
    }

    /// <summary>
    /// Command execution flow - synchronous-looking async.
    /// </summary>
    public static async Task CommandExecutionFlow(BiDiDriver driver, NavigateCommandParameters navParams)
    {
#region CommandExecutionFlow
        NavigateCommandParameters parameters = null;

        // Synchronous-looking code (with async/await)
        NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(parameters);

        // What actually happens:
        // 1. NavigateAsync creates a Command object
        // 2. Command is serialized to JSON
        // 3. JSON sent via WebSocket
        // 4. Method awaits response
        // 5. Browser processes navigation
        // 6. Browser sends response JSON
        // 7. Transport deserializes to NavigateCommandResult
        // 8. Awaited method returns result
#endregion
    }

    /// <summary>
    /// Event handling setup and runtime.
    /// </summary>
    public static async Task EventHandling(BiDiDriver driver, SubscribeCommandParameters subscribeParams)
    {
#region EventHandling
        // Setup (before events occur)
        driver.Log.OnEntryAdded.AddObserver((e) => {
            Console.WriteLine(e.Text);
        });

        await driver.Session.SubscribeAsync(subscribeParams);

        // Runtime (when event occurs):
        // 1. Browser emits log.entryAdded event
        // 2. Transport receives JSON message
        // 3. Transport deserializes to EntryAddedEventArgs
        // 4. ObservableEvent.NotifyObserversAsync called
        // 5. All registered observers invoked
        // 6. Your handler executes
#endregion
    }

    /// <summary>
    /// Synchronous event handler - runs on Transport thread.
    /// </summary>
    public static void SyncEventHandler(BiDiDriver driver)
    {
#region SyncEventHandler
        driver.Log.OnEntryAdded.AddObserver((e) => {
            // Runs on Transport thread
            // Blocks other message processing until complete
            Console.WriteLine(e.Text);
        });
#endregion
    }

    /// <summary>
    /// Asynchronous event handler - runs on Task pool.
    /// </summary>
    public static void AsyncEventHandler(BiDiDriver driver)
    {
#region AsyncEventHandler
        driver.Log.OnEntryAdded.AddObserver(
            async (e) => {
                // Runs on Task pool
                // Doesn't block message processing
                await ProcessLogEntryAsync(e);
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously
        );
#endregion
    }

    /// <summary>
    /// Terminate mode - throws on next command after error.
    /// </summary>
    public static async Task TerminateMode(string url)
    {
#region TerminateMode
        // Throws on next command call after error occurs
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30))
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
            ProtocolErrorBehavior = TransportErrorBehavior.Terminate,
            UnknownMessageBehavior = TransportErrorBehavior.Terminate,
            UnexpectedErrorBehavior = TransportErrorBehavior.Terminate,
        };
        try
        {
            await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
        }
#endregion
    }

    /// <summary>
    /// Collect mode - errors stored, thrown at Stop.
    /// </summary>
    public static async Task CollectMode(NavigateCommandParameters navParams)
    {
#region CollectMode
        WebSocketConnection connection = new WebSocketConnection();
        Transport transport = new Transport(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
            UnknownMessageBehavior = TransportErrorBehavior.Collect,
            UnexpectedErrorBehavior = TransportErrorBehavior.Collect,
        };
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);

        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        // Perform operations...
        await driver.BrowsingContext.NavigateAsync(navParams);

        try
        {
            await driver.StopAsync();
        }
        catch (AggregateException ex)
        {
            // Check for collected errors
            if (ex.InnerExceptions.Count > 0)
            {
                Console.WriteLine($"Encountered {ex.InnerExceptions.Count} transport errors:");
                foreach (Exception error in ex.InnerExceptions)
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
    /// Ignore mode - errors silently discarded.
    /// </summary>
    public static async Task IgnoreMode(NavigateCommandParameters navParams)
    {
#region IgnoreMode
        WebSocketConnection connection = new WebSocketConnection();
        Transport transport = new Transport(connection);
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);

        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        // Errors won't be thrown or collected
        await driver.BrowsingContext.NavigateAsync(navParams);

        await driver.StopAsync(); 
#endregion
    }

    /// <summary>
    /// Connection-level error handling.
    /// </summary>
    public static async Task ConnectionLevelErrorHandling()
    {
#region Connection-LevelErrorHandling
        WebSocketConnection connection = new WebSocketConnection();

        // Subscribe to connection errors
        connection.OnConnectionError.AddObserver((errorArgs) =>
        {
            Console.WriteLine($"Connection error: {errorArgs.Exception.Message}");
        });

        // Subscribe to log messages
        connection.OnLogMessage.AddObserver((logArgs) =>
        {
            Console.WriteLine($"[{logArgs.Level}] {logArgs.Message}");
        });

        Transport transport = new Transport(connection);
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);

        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");
#endregion
    }

    /// <summary>
    /// Event handler error behavior - sync vs async.
    /// </summary>
    public static void EventHandlerErrorBehavior(BiDiDriver driver, Action<EntryAddedEventArgs> processLogEntry)
    {
#region EventHandlerErrorBehavior
        // Synchronous handler: exceptions bubble up immediately
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            ProcessLogEntry(e);  // If this throws, exception propagates
        });

        // Asynchronous handler: exceptions are captured
        driver.Network.OnBeforeRequestSent.AddObserver(
            async (e) =>
            {
                await ProcessRequestAsync(e);  // Exceptions captured
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously
        );
#endregion
    }

    /// <summary>
    /// EventHandlerExceptionBehavior - terminate on handler failure.
    /// </summary>
    public static async Task EventHandlerExceptionBehavior(string url, NavigateCommandParameters navParams, Action<EntryAddedEventArgs> processLogEntry)
    {
#region EventHandlerExceptionBehavior
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        driver.EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate;

        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            // If this throws, driver will terminate on next command
            ProcessLogEntry(e);
        });

        try
        {
            await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

            // Perform operations...
            await driver.BrowsingContext.NavigateAsync(navParams);  // Exception thrown here if handler failed
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"Event handler error: {ex.Message}");
        }
#endregion
    }

    private static void ProcessLogEntry(EntryAddedEventArgs e) {}

    /// <summary>
    /// ProtocolErrorBehavior - collect protocol errors.
    /// </summary>
    public static async Task ProtocolErrorBehavior(string url, NavigateCommandParameters navParams)
    {
#region ProtocolErrorBehavior
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        driver.ProtocolErrorBehavior = TransportErrorBehavior.Collect;

        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        // Perform operations...
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Collected errors are thrown when stopping
        try
        {
            await driver.StopAsync();
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"Protocol errors encountered: {ex.Message}");
        }
#endregion
    }

    /// <summary>
    /// UnknownMessageBehavior - ignore unknown messages.
    /// </summary>
    public static async Task UnknownMessageBehavior(string url, NavigateCommandParameters navParams)
    {
#region UnknownMessageBehavior
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        driver.UnknownMessageBehavior = TransportErrorBehavior.Ignore;

        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        // Browser sends new event type not yet supported by library
        // With Ignore mode, these are logged but don't cause errors

        await driver.BrowsingContext.NavigateAsync(navParams);
        await driver.StopAsync();  // Completes without exception
#endregion
    }

    /// <summary>
    /// UnexpectedErrorBehavior - terminate on unexpected errors.
    /// </summary>
    public static async Task UnexpectedErrorBehavior(string url, NavigateCommandParameters navParams)
    {
#region UnexpectedErrorBehavior
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        driver.UnexpectedErrorBehavior = TransportErrorBehavior.Terminate;

        try
        {
            await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

            // If browser sends error response without matching command ID, exception thrown on next command
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
#endregion
    }

    /// <summary>
    /// Configuring multiple error behaviors.
    /// </summary>
    public static async Task MultipleErrorBehaviors(string url, NavigateCommandParameters navParams)
    {
#region MultipleErrorBehaviors
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        // Different strategies for different error types
        driver.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;  // Collect handler errors
        driver.ProtocolErrorBehavior = TransportErrorBehavior.Terminate;        // Fail fast on protocol errors
        driver.UnknownMessageBehavior = TransportErrorBehavior.Ignore;         // Ignore unknown messages
        driver.UnexpectedErrorBehavior = TransportErrorBehavior.Collect;       // Collect unexpected errors

        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        // Perform operations...
        await driver.BrowsingContext.NavigateAsync(navParams);

        // Errors with Collect behavior are thrown here
        try
        {
            await driver.StopAsync();
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"Collected errors: {ex.Message}");
            // Exception may contain multiple errors as inner exceptions
        }
#endregion
    }

    /// <summary>
    /// Custom module registration.
    /// </summary>
#region CustomModuleDefinition
    public class MyCustomModule : Module
    {
        public const string MyCustomModuleName = "myCustom";

        public MyCustomModule(IBiDiCommandExecutor driver)
            : base(driver) { }

        public override string ModuleName => MyCustomModuleName;
        
        public async Task<MyCommandResult> MyCommandAsync(
            MyCommandParameters parameters)
        {
            return await this.Driver.ExecuteCommandAsync<MyCommandResult>(
                parameters);
        }
    }
#endregion

    public static void CustomModuleRegistration(BiDiDriver driver)
    {
#region CustomModuleRegistration
        // Register with driver
        driver.RegisterModule(new MyCustomModule(driver));
#endregion
    }

    /// <summary>
    /// Command batching - parallel execution.
    /// </summary>
    public static async Task CommandBatching(BiDiDriver driver)
    {
        GetTreeCommandParameters params1 = new GetTreeCommandParameters();
        StatusCommandParameters params2 = new StatusCommandParameters();

#region CommandBatching
        // ✅ Good: Execute independent commands in parallel
        Task<GetTreeCommandResult> t1 = driver.BrowsingContext.GetTreeAsync(params1);
        Task<StatusCommandResult> t2 = driver.Session.StatusAsync(params2);
        await Task.WhenAll(t1, t2);

        // ❌ Slower: Execute sequentially when not needed
        var r1 = await driver.BrowsingContext.GetTreeAsync(params1);
        var r2 = await driver.Session.StatusAsync(params2);
#endregion
    }

    /// <summary>
    /// Event handler performance - avoid blocking.
    /// </summary>
    public static void EventHandlerPerformance(BiDiDriver driver)
    {
#region EventHandlerPerformance
        // ❌ Bad: Blocks message processing
        driver.Log.OnEntryAdded.AddObserver((e) => {
            Thread.Sleep(1000); // Blocks for 1 second
        });

        // ✅ Good: Run asynchronously
        driver.Log.OnEntryAdded.AddObserver(
            async (e) => {
                await Task.Delay(1000); // Doesn't block
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously
        );
#endregion
    }

    /// <summary>
    /// Memory management - remove observers and unsubscribe.
    /// </summary>
    public static async Task MemoryManagement(BiDiDriver driver, Func<EntryAddedEventArgs, Task> handler, UnsubscribeCommandParameters unsubscribeParams)
    {
#region MemoryManagement
        // Remove observer when done
        EventObserver<EntryAddedEventArgs> observer = 
            driver.Log.OnEntryAdded.AddObserver(handler);
            
        // Later...
        observer.Unobserve();

        // Unsubscribe from events
        await driver.Session.UnsubscribeAsync(unsubscribeParams);

        // Stop driver
        await driver.StopAsync();
#endregion
    }

    public class MyCommandParameters : CommandParameters
    {
        public override string MethodName => throw new NotImplementedException();

        public override Type ResponseType => throw new NotImplementedException();
    }

    public record MyCommandResult : CommandResult;

    private static Task ProcessLogEntryAsync(EntryAddedEventArgs e) => Task.CompletedTask;

    private static Task ProcessRequestAsync(BeforeRequestSentEventArgs e) => Task.CompletedTask;
}
