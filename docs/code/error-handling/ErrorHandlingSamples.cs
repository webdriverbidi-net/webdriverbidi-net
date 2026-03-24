// <copyright file="ErrorHandlingSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/advanced/error-handling.md

#pragma warning disable CS0168, CS8600, CS8618, CS0169, CS0649

namespace WebDriverBiDi.Docs.Code.ErrorHandling;

using System.Linq;
using System.Text;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for error handling documentation. Compiled at build time to prevent API drift.
/// </summary>
public class ErrorHandlingSamples
{
    /// <summary>
    /// WebDriverBiDiException - basic try/catch.
    /// </summary>
    public static async Task WebDriverBiDiException(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
        #region WebDriverBiDiException
        try
        {
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"WebDriver BiDi error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        #endregion
    }

    /// <summary>
    /// Common error scenarios - multiple catch blocks.
    /// </summary>
    public static async Task CommonErrorScenarios(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
        #region CommonErrorScenarios
        try
        {
            NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(navParams);
        }
        catch (WebDriverBiDiTimeoutException ex)
        {
            Console.WriteLine("Navigation timeout - page took too long to load");
            // Handle timeout specifically
        }
        catch (WebDriverBiDiException ex) when (ex.Message.Contains("no such frame"))
        {
            Console.WriteLine("Browsing context no longer exists");
            // Handle missing context
        }
        catch (WebDriverBiDiException ex) when (ex.Message.Contains("invalid argument"))
        {
            Console.WriteLine("Invalid command parameters");
            // Handle parameter error
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"Other BiDi error: {ex.Message}");
            // Handle general errors
        }
        #endregion
    }

    /// <summary>
    /// Ignore mode - default behavior.
    /// </summary>
    public static async Task IgnoreMode(
        SubscribeCommandParameters subscribeParams,
        NavigateCommandParameters navParams,
        Action<EntryAddedEventArgs> processLogEntry)
    {
        #region IgnoreMode
        // Default behavior - errors are ignored
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        try
        {
            await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

            // Event handler errors will be silently discarded
            driver.Log.OnEntryAdded.AddObserver((e) =>
            {
                // This runs on a separate thread
                // If it throws, the exception is discarded and logged
                ProcessLogEntry(e);  // May throw
            });

            await driver.Session.SubscribeAsync(subscribeParams);

            // Commands proceed normally, event handler errors are invisible
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        finally
        {
            await driver.StopAsync();
        }
        #endregion
    }

    /// <summary>
    /// Collect mode - errors stored, thrown on Stop.
    /// </summary>
    public static async Task CollectMode(
        SubscribeCommandParameters subscribeParams,
        NavigateCommandParameters navParams,
        EvaluateCommandParameters evalParams,
        Action<BeforeRequestSentEventArgs> processNetworkRequest)
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

        try
        {
            await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

            // Subscribe to events with potentially failing handlers
            driver.Log.OnEntryAdded.AddObserver((e) =>
            {
                // This runs on a separate thread
                // If it throws, error is collected but never thrown
                ProcessLogEntry(e);
            });

            driver.Network.OnBeforeRequestSent.AddObserver((e) =>
            {
                // If this throws, error is collected
                ProcessNetworkRequest(e);
            });

            await driver.Session.SubscribeAsync(subscribeParams);

            // Send commands - event handler errors won't be thrown
            await driver.BrowsingContext.NavigateAsync(navParams);
            await driver.Script.EvaluateAsync(evalParams);

            await driver.StopAsync();
        }
        catch (AggregateException ex)
        {
            // Explicitly check for collected errors when ready
            if (ex.InnerExceptions.Count > 0)
            {
                Console.WriteLine($"\nCollected {ex.InnerExceptions.Count} transport errors:");
                foreach (Exception error in ex.InnerExceptions)
                {
                    Console.WriteLine($"  [{error.GetType().Name}] {error.Message}");
                    Console.WriteLine($"    From: {error.StackTrace?.Split('\n')[0].Trim()}");
                }

                // Analyze error types
                var eventHandlerErrors = ex.InnerExceptions
                    .Where(e => e.StackTrace?.Contains("AddObserver") == true)
                    .ToList();

                var protocolErrors = ex.InnerExceptions
                    .OfType<WebDriverBiDiProtocolException>()
                    .ToList();

                Console.WriteLine($"  Event handler errors: {eventHandlerErrors.Count}");
                Console.WriteLine($"  Protocol errors: {protocolErrors.Count}");
            }
        }
        finally
        {
            await driver.DisposeAsync();
        }
        #endregion
    }

    /// <summary>
    /// Terminate mode - errors thrown on next command.
    /// </summary>
    public static async Task TerminateMode(
        SubscribeCommandParameters subscribeParams,
        NavigateCommandParameters navParams)
    {
        #region TerminateMode
        // Create transport with Terminate behavior (opt-in to stricter error handling)
        WebSocketConnection connection = new WebSocketConnection();
        Transport transport = new Transport(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
            ProtocolErrorBehavior = TransportErrorBehavior.Terminate,
            UnknownMessageBehavior = TransportErrorBehavior.Terminate,
            UnexpectedErrorBehavior = TransportErrorBehavior.Terminate,
        };
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);

        try
        {
            await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

            // Subscribe to events
            driver.Log.OnEntryAdded.AddObserver((e) =>
            {
                // This runs on a separate thread
                if (e.Level == LogLevel.Error)
                {
                    throw new InvalidOperationException("Error log entry received");
                }
            });

            await driver.Session.SubscribeAsync(subscribeParams);

            // If an error log event occurs, the exception won't throw immediately
            // because the event handler runs on a separate thread

            // The exception will be thrown here when we send the next command
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"Transport error: {ex.Message}");
            // This catch block will receive the event handler exception
        }
        finally
        {
            await driver.StopAsync();
        }
        #endregion
    }

    /// <summary>
    /// Threading model - event handlers run on transport thread.
    /// </summary>
    public static async Task ThreadingModel(
        SubscribeCommandParameters subscribeParams,
        NavigateCommandParameters navParams)
    {
        #region ThreadingModel
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        // Main thread (your code)
        Console.WriteLine("Main thread: Setting up event handler");

        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            // Transport thread (separate from main thread)
            Console.WriteLine($"Transport thread: Processing event {e.Type}");

            // If this throws, the exception occurs on the transport thread
            if (e.Level == LogLevel.Error)
            {
                throw new InvalidOperationException("Error log entry");
            }
        });

        await driver.Session.SubscribeAsync(subscribeParams);

        Console.WriteLine("Main thread: Executing command");

        try
        {
            // Main thread (your code)
            // With Terminate mode: event handler exceptions from transport thread
            // are surfaced here when we synchronize via this command
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        catch (WebDriverBiDiException ex)
        {
            // Main thread catches the exception that originated on transport thread
            Console.WriteLine($"Main thread: Caught exception from transport: {ex.Message}");
        }
        #endregion
    }

    /// <summary>
    /// Connection-level error monitoring via OnConnectionError and OnLogMessage.
    /// </summary>
    public static async Task ConnectionLevelErrorMonitoring()
    {
        #region Connection-LevelErrorMonitoring
        WebSocketConnection connection = new WebSocketConnection();

        // These run on the transport thread but don't throw to main thread
        connection.OnConnectionError.AddObserver((errorArgs) =>
        {
            Console.WriteLine($"[Connection Error] {errorArgs.Exception.Message}");
            LogToFile($"Transport error: {errorArgs.Exception}");
        });

        connection.OnLogMessage.AddObserver((logArgs) =>
        {
            if (logArgs.Level == WebDriverBiDiLogLevel.Error)
            {
                Console.WriteLine($"[Transport Error Log] {logArgs.Message}");
            }
        });

        Transport transport = new Transport(connection);
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);

        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");
        #endregion
    }

    /// <summary>
    /// FindElementSafelyAsync usage example.
    /// </summary>
    public async Task FindElementSafelyAsyncUsage(
        BiDiDriver driver,
        string contextId)
    {
        #region FindElementSafelyAsyncUsage
        // Usage
        RemoteValue? element = await FindElementSafelyAsync(
            driver,
            contextId,
            "button.submit",
            TimeSpan.FromSeconds(10));

        if (element == null)
        {
            Console.WriteLine("Submit button not found");
            // Handle missing element
        }
        #endregion
    }

    /// <summary>
    /// ObservableEventHandlerOptions enum values: RunHandlerSynchronously, RunHandlerAsynchronously.
    /// </summary>
    public static void ObservableEventHandlerOptionsValues()
    {
        #region ObservableEventHandlerOptions
        _ = ObservableEventHandlerOptions.RunHandlerSynchronously;
        _ = ObservableEventHandlerOptions.RunHandlerAsynchronously;
        #endregion
    }

    /// <summary>
    /// IsContextValidAsync usage example.
    /// </summary>
    public async Task IsContextValidAsyncUsage(
        BiDiDriver driver,
        string contextId)
    {
        #region IsContextValidAsyncUsage
        // Usage
        if (!await IsContextValidAsync(driver, contextId))
        {
            Console.WriteLine("Context no longer exists");
            // Handle invalid context
        }
        #endregion
    }

    /// <summary>
    /// ErrorContext usage - capture and save on exception.
    /// </summary>
    public static async Task ErrorContextUsage(
        BiDiDriver driver,
        NavigateCommandParameters navParams,
        string contextId)
    {
        #region ErrorContextUsage
        // Usage
        try
        {
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        catch (Exception ex)
        {
            ErrorContext context = new ErrorContext
            {
                Operation = "Navigation",
                Timestamp = DateTime.Now,
                ContextId = contextId,
                Url = navParams.Url,
                Exception = ex
            };

            context.SaveToFile();
            throw;
        }
        #endregion
    }

    /// <summary>
    /// CircuitBreaker usage example.
    /// </summary>
    public static async Task CircuitBreakerUsage(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
        #region CircuitBreakerUsage
        // Usage
        CircuitBreaker breaker = new CircuitBreaker();

        try
        {
            NavigateCommandResult result = await breaker.ExecuteAsync(
                async () => await driver.BrowsingContext.NavigateAsync(navParams));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Circuit breaker"))
        {
            Console.WriteLine("Too many failures - circuit breaker activated");
            // Handle circuit breaker state
        }
        #endregion
    }

    private readonly string webSocketUrl = string.Empty;
    private readonly string contextId = string.Empty;

    /// <summary>
    /// Simulating errors - navigation timeout test pattern.
    /// </summary>
#region TestNavigationTimeoutPattern
    [Test]
    public async Task TestNavigationTimeout()
    {
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(1));

        try
        {
            await driver.StartAsync(webSocketUrl);

            // This should timeout
            NavigateCommandParameters parameters = new NavigateCommandParameters(
                contextId,
                "https://httpstat.us/200?sleep=5000");  // 5 second delay

            await driver.BrowsingContext.NavigateAsync(parameters);

            Assert.Fail("Expected timeout exception");
        }
        catch (WebDriverBiDiTimeoutException ex)
        {
            Assert.That(ex.Message, Does.Contain("Timed out"));
        }
    }
    #endregion

    /// <summary>
    /// Simulating errors - invalid context test pattern.
    /// </summary>
    #region TestInvalidContextPattern
    [Test]
    public async Task TestInvalidContext()
    {
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(1));

        try
        {
            await driver.StartAsync(webSocketUrl);

            // Use invalid context ID
            NavigateCommandParameters parameters = new NavigateCommandParameters(
                "invalid-context-id",
                "https://example.com");

            await driver.BrowsingContext.NavigateAsync(parameters);

            Assert.Fail("Expected context error");
        }
        catch (WebDriverBiDiException ex)
        {
            Assert.That(ex.Message, Does.Contain("no such frame").IgnoreCase);
        }
    }
    #endregion

    /// <summary>
    /// Set error behavior on BiDiDriver directly (simpler).
    /// </summary>
    public static void SetErrorBehaviorOnDriver(BiDiDriver driver)
    {
        driver.EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate;
        driver.ProtocolErrorBehavior = TransportErrorBehavior.Terminate;

        driver.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
        driver.ProtocolErrorBehavior = TransportErrorBehavior.Collect;
    }

    /// <summary>
    /// ResilientDriver - retry with reconnect on connection errors.
    /// </summary>
    public static async Task ResilientDriverExecuteWithRetry(
        BiDiDriver driver,
        string webSocketUrl,
        NavigateCommandParameters navParams)
    {
        #region ResilientDriverUsage
        // Usage
        ResilientDriver resilientDriver = new ResilientDriver();

        NavigateCommandResult result = await resilientDriver.ExecuteWithRetryAsync(
            async (d) => await d.BrowsingContext.NavigateAsync(navParams));
        #endregion
    }

    /// <summary>
    /// Safe element location with polling.
    /// </summary>
#region FindElementSafelyAsync
    public async Task<RemoteValue?> FindElementSafelyAsync(
        BiDiDriver driver,
        string contextId,
        string selector,
        TimeSpan timeout)
    {
        DateTime endTime = DateTime.Now + timeout;

        while (DateTime.Now < endTime)
        {
            try
            {
                LocateNodesCommandResult result =
                    await driver.BrowsingContext.LocateNodesAsync(
                        new LocateNodesCommandParameters(contextId, new CssLocator(selector)));

                if (result.Nodes.Count > 0)
                {
                    return result.Nodes[0];
                }
            }
            catch (WebDriverBiDiException ex)
            {
                Console.WriteLine($"Error locating element: {ex.Message}");
            }

            await Task.Delay(100);
        }

        Console.WriteLine($"Element not found after {timeout.TotalSeconds}s: {selector}");
        return null;
    }
    #endregion

    /// <summary>
    /// Wait for element using JavaScript Promise.
    /// </summary>
    #region WaitForElementAsync
    public async Task<bool> WaitForElementAsync(
        BiDiDriver driver,
        string contextId,
        string selector,
        TimeSpan timeout)
    {
        string waitScript = $$"""
            new Promise((resolve) => {
                const checkElement = () => {
                    const element = document.querySelector('{{selector}}');
                    if (element) {
                        resolve(true);
                    } else {
                        setTimeout(checkElement, 100);
                    }
                };
                checkElement();
                setTimeout(() => resolve(false), {timeout.TotalMilliseconds});
            })
            """;

        try
        {
            EvaluateResult result = await driver.Script.EvaluateAsync(
                new EvaluateCommandParameters(
                    waitScript,
                    new ContextTarget(contextId),
                    true));

            if (result is EvaluateResultSuccess success &&
                success.Result is BooleanRemoteValue boolValue)
            {
                return boolValue.Value;
            }
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"Error waiting for element: {ex.Message}");
        }

        return false;
    }
    #endregion

    /// <summary>
    /// Synchronous vs asynchronous handlers - bad and good examples.
    /// </summary>
    public static void SyncVsAsyncHandlers(
        BiDiDriver driver,
        Action<EntryAddedEventArgs> processLogEntry,
        Func<EntryAddedEventArgs, Task> processLogEntryAsync)
    {
        #region SynchronousvsAsynchronousHandlers
        // ❌ Bad: Synchronous handler blocks transport thread
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            // This runs on the transport thread, blocking it
            Thread.Sleep(1000);  // Blocks ALL message processing for 1 second!
            ProcessLogEntry(e);
        });

        // ❌ Also bad: Default (RunHandlerSynchronously) still blocks
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            ProcessLogEntry(e);  // Blocks transport thread until complete
        }, ObservableEventHandlerOptions.RunHandlerSynchronously);

        // ✅ Good: Asynchronous handler doesn't block transport
        driver.Log.OnEntryAdded.AddObserver(
            async (e) =>
            {
                // This runs on a Task pool thread
                await Task.Delay(1000);  // Doesn't block transport thread
                await ProcessLogEntryAsync(e);
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously
        );
        #endregion
    }


    /// <summary>
    /// Exception handling in event handlers - unhandled vs handled.
    /// </summary>
    public static void ExceptionHandlingInHandlers(
        BiDiDriver driver,
        Action<EntryAddedEventArgs> processLogEntry,
        Func<BeforeRequestSentEventArgs, Task> processNetworkRequestAsync,
        Func<string, Task> logErrorAsync)
    {
        #region ExceptionHandlinginEventHandlers
        // Unhandled exception - behavior depends on TransportErrorBehavior
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            ProcessLogEntry(e);  // May throw
            // If throws:
            //   Terminate mode: Exception thrown on next command
            //   Collect mode: Exception stored in transport.Errors
            //   Ignore mode: Exception discarded
        });

        // ✅ Better: Handle exceptions within the handler
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            try
            {
                ProcessLogEntry(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing log entry: {ex.Message}");
                // Error handled, won't be reported to TransportErrorBehavior
            }
        });

        // ✅ Best: Async handler with error handling
        driver.Network.OnBeforeRequestSent.AddObserver(
            async (e) =>
            {
                try
                {
                    await ProcessNetworkRequestAsync(e);
                }
                catch (Exception ex)
                {
                    await LogErrorAsync($"Error processing request: {ex.Message}");
                    // Error handled within handler
                }
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously
        );
        #endregion
    }

    /// <summary>
    /// Handler execution behavior - sync vs async.
    /// </summary>
    public static async Task HandlerExecutionBehavior(
        Action<EntryAddedEventArgs> processLogEntry,
        Func<ResponseCompletedEventArgs, Task> analyzeResponseAsync)
    {
        #region HandlerExecutionBehavior
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

        // Synchronous handler (default)
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            // Runs on transport thread
            // Blocks transport until complete
            // Other messages wait for this to finish
            ProcessLogEntry(e);
        });

        // Explicit synchronous
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            ProcessLogEntry(e);
        }, ObservableEventHandlerOptions.RunHandlerSynchronously);

        // Asynchronous handler
        driver.Network.OnResponseCompleted.AddObserver(
            async (e) =>
            {
                // Starts on transport thread, continues on task pool
                // Transport thread returns immediately
                // Other messages processed concurrently
                await AnalyzeResponseAsync(e);
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously
        );
        #endregion
    }

    /// <summary>
    /// Multiple handlers for same event - synchronous.
    /// </summary>
    public static void MultipleHandlersSync(BiDiDriver driver)
    {
        #region MultipleHandlersSync
        // Handler 1 (synchronous)
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            Console.WriteLine($"Handler 1: {e.Text}");
        });

        // Handler 2 (synchronous)
        driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            Console.WriteLine($"Handler 2: {e.Text}");
        });

        // With synchronous handlers:
        // - Handler 1 executes completely, then Handler 2 executes
        // - If Handler 1 throws, TransportErrorBehavior determines what happens
        // - With Terminate: Exception thrown on next command
        // - With Collect: Exception collected, Handler 2 still executes
        // - With Ignore: Exception ignored, Handler 2 still executes
        #endregion
    }

    /// <summary>
    /// Multiple handlers for same event - asynchronous.
    /// </summary>
    public static void MultipleHandlersAsync(
        BiDiDriver driver,
        Func<string, Task> logRequestAsync,
        Func<RequestData, Task> analyzeSecurityHeadersAsync)
    {
        #region MultipleHandlersAsync
        // Handler 1 (asynchronous)
        driver.Network.OnBeforeRequestSent.AddObserver(
            async (e) =>
            {
                await LogRequestAsync(e.Request.Url);
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously
        );

        // Handler 2 (asynchronous)
        driver.Network.OnBeforeRequestSent.AddObserver(
            async (e) =>
            {
                await AnalyzeSecurityHeadersAsync(e.Request);
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously
        );

        // With asynchronous handlers:
        // - Both handlers start and run concurrently
        // - Transport thread doesn't wait for either to complete
        // - Exceptions handled according to TransportErrorBehavior
        #endregion
    }

    /// <summary>
    /// Complete event handler pattern with Collect mode.
    /// </summary>
    public static async Task CompleteEventHandlerPattern(
        SubscribeCommandParameters subscribeParams,
        Func<ResponseData, Task> saveResponseToFileAsync,
        Func<string, Task> logErrorAsync)
    {
        #region CompleteEventHandlerPattern
        // Use Collect mode to see all handler errors
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

        try
        {
            // Synchronous handler for quick operations
            driver.Log.OnEntryAdded.AddObserver((e) =>
            {
                try
                {
                    // Quick, synchronous operation
                    if (e.Level == LogLevel.Error || e.Level == LogLevel.Warn)
                    {
                        Console.WriteLine($"[{e.Level}] {e.Text}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in log handler: {ex.Message}");
                }
            });

            // Asynchronous handler for I/O operations
            driver.Network.OnResponseCompleted.AddObserver(
                async (e) =>
                {
                    try
                    {
                        // Async operation doesn't block transport
                        await SaveResponseToFileAsync(e.Response);
                    }
                    catch (Exception ex)
                    {
                        await LogErrorAsync($"Failed to save response: {ex.Message}");
                    }
                },
                ObservableEventHandlerOptions.RunHandlerAsynchronously
            );

            await driver.Session.SubscribeAsync(subscribeParams);

        }
        catch (AggregateException ex)
        {
            // After operations, check for any unhandled handler errors
            if (ex.InnerExceptions.Count > 0)
            {
                Console.WriteLine($"Transport errors occurred: {ex.InnerExceptions.Count}");
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
    /// Parameter validation - SafeNavigateAsync.
    /// </summary>
    #region ParameterValidation-SafeNavigateAsync
    public async Task<NavigateCommandResult> SafeNavigateAsync(
        BiDiDriver driver,
        string contextId,
        string url)
    {
        // Validate inputs
        if (driver == null)
        {
            throw new ArgumentNullException(nameof(driver));
        }

        if (string.IsNullOrWhiteSpace(contextId))
        {
            throw new ArgumentException("Context ID cannot be empty", nameof(contextId));
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be empty", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            throw new ArgumentException("Invalid URL format", nameof(url));
        }

        // Execute with error handling
        try
        {
            NavigateCommandParameters navParams = new NavigateCommandParameters(contextId, url)
            {
                Wait = ReadinessState.Complete
            };

            return await driver.BrowsingContext.NavigateAsync(navParams);
        }
        catch (WebDriverBiDiException ex)
        {
            throw new InvalidOperationException(
                $"Failed to navigate to {url}: {ex.Message}",
                ex);
        }
    }
    #endregion

    /// <summary>
    /// Context validation - IsContextValidAsync.
    /// </summary>
    #region ContextValidation-IsContextValidAsync
    public async Task<bool> IsContextValidAsync(BiDiDriver driver, string contextId)
    {
        try
        {
            // Use RootBrowsingContextId to fetch only this context (more efficient than full tree)
            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                new GetTreeCommandParameters { RootBrowsingContextId = contextId });

            return tree.ContextTree.Count > 0;
        }
        catch (WebDriverBiDiException)
        {
            return false;
        }
    }
    #endregion

    /// <summary>
    /// Comprehensive logging - DiagnosticDriver.
    /// </summary>
    #region ComprehensiveLogging-ExecuteWithLoggingAsync
    public class DiagnosticDriver
    {
        private readonly BiDiDriver driver;
        private readonly ILogger logger;

        public async Task<T> ExecuteWithLoggingAsync<T>(
            string operationName,
            Func<Task<T>> operation)
        {
            logger.LogInformation($"Starting {operationName}");
            DateTime startTime = DateTime.Now;

            try
            {
                T result = await operation();
                TimeSpan duration = DateTime.Now - startTime;

                logger.LogInformation(
                    $"Completed {operationName} in {duration.TotalMilliseconds}ms");

                return result;
            }
            catch (WebDriverBiDiException ex)
            {
                TimeSpan duration = DateTime.Now - startTime;

                logger.LogError(
                    ex,
                    $"Failed {operationName} after {duration.TotalMilliseconds}ms: {ex.Message}");

                throw;
            }
        }
    }
    #endregion

    /// <summary>
    /// Error context capture - ErrorContext class.
    /// </summary>
    public static void ErrorContextSaveToFile(ErrorContext context)
    {
        context.SaveToFile();
    }

    /// <summary>
    /// Graceful degradation - GetPageTitleAsync with fallback.
    /// </summary>
#region GracefulDegradation-GetPageTitleAsync
    public async Task<string> GetPageTitleAsync(BiDiDriver driver, string contextId)
    {
        // Try JavaScript first
        try
        {
            EvaluateResult result = await driver.Script.EvaluateAsync(
                new EvaluateCommandParameters(
                    "document.title",
                    new ContextTarget(contextId),
                    true));

            if (result is EvaluateResultSuccess success &&
                success.Result is StringRemoteValue stringValue)
            {
                return stringValue.Value ?? "Unknown";
            }
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"JavaScript method failed: {ex.Message}");
        }

        // Fallback to context info
        try
        {
            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                new GetTreeCommandParameters { RootBrowsingContextId = contextId });

            if (tree.ContextTree.Count > 0)
            {
                return tree.ContextTree[0].Url;
            }
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"Context method failed: {ex.Message}");
        }

        return "Unknown";
    }
    #endregion

    /// <summary>
    /// Don't swallow exceptions - bad vs good.
    /// </summary>
    public static async Task DontSwallowExceptions(
        BiDiDriver driver,
        NavigateCommandParameters navParams,
        ILogger logger)
    {
        #region DontSwallowExceptions
        // ❌ Bad: Silent failure
        try
        {
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        catch
        {
            // Error ignored
        }

        // ✅ Good: Proper handling
        try
        {
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        catch (WebDriverBiDiException ex)
        {
            logger.LogError(ex, "Navigation failed");
            throw; // or handle appropriately
        }
        #endregion
    }

    /// <summary>
    /// Don't use generic catch - bad vs good.
    /// </summary>
    public static async Task DontUseGenericCatch(
        BiDiDriver driver,
        NavigateCommandParameters navParams)
    {
        #region DontUseGenericCatch
        // ❌ Bad: Too broad
        try
        {
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        catch (Exception ex)
        {
            // Catches everything, including bugs
        }

        // ✅ Good: Specific handling
        try
        {
            await driver.BrowsingContext.NavigateAsync(navParams);
        }
        catch (WebDriverBiDiException ex)
        {
            // Handle protocol errors
        }
        catch (TaskCanceledException ex)
        {
            // Handle cancellation
        }
    }
    #endregion

    private static void ProcessLogEntry(EntryAddedEventArgs e)
    {
        if (e.Level == LogLevel.Error)
        {
            throw new InvalidOperationException("Error log entry received");
        }
    }

    private static void ProcessNetworkRequest(BeforeRequestSentEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static void LogToFile(string v)
    {
        throw new NotImplementedException();
    }

    private static async Task ProcessLogEntryAsync(EntryAddedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static async Task LogErrorAsync(string v)
    {
        throw new NotImplementedException();
    }

    private static async Task ProcessNetworkRequestAsync(BeforeRequestSentEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static async Task AnalyzeResponseAsync(ResponseCompletedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static async Task AnalyzeSecurityHeadersAsync(RequestData request)
    {
        throw new NotImplementedException();
    }

    private static async Task LogRequestAsync(string url)
    {
        throw new NotImplementedException();
    }

    private static async Task SaveResponseToFileAsync(ResponseData response)
    {
        throw new NotImplementedException();
    }
}

public sealed class TestAttribute : Attribute
{
}

public sealed class SetUpAttribute : Attribute
{
}

public sealed class TearDownAttribute : Attribute
{
}


internal class Does
{
    internal static Container Contain(string v)
    {
        throw new NotImplementedException();
    }
}

internal class Container
{
    public object IgnoreCase { get; internal set; }
}

internal class Assert
{
    internal static void Fail(string v)
    {
        throw new NotImplementedException();
    }

    internal static void That(string message, object value)
    {
        throw new NotImplementedException();
    }
}

public interface ILogger
{
    void LogError(WebDriverBiDiException ex, string v);
    void LogInformation(string v);
}

/// <summary>
/// ResilientDriver - retry with reconnect on connection errors.
/// </summary>
#region ResilientDriverClass
public class ResilientDriver
{
    private BiDiDriver driver;
    private readonly string webSocketUrl;
    private readonly int maxRetries = 3;

    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<BiDiDriver, Task<T>> operation)
    {
        int attempt = 0;
        Exception? lastException = null;

        while (attempt < maxRetries)
        {
            try
            {
                return await operation(driver);
            }
            catch (WebDriverBiDiException ex) when (
                ex.Message.Contains("WebSocket") ||
                ex.Message.Contains("connection"))
            {
                lastException = ex;
                attempt++;

                if (attempt < maxRetries)
                {
                    Console.WriteLine($"Connection lost. Reconnecting... (attempt {attempt})");
                    await Task.Delay(TimeSpan.FromSeconds(2));

                    // Reconnect
                    await driver.StopAsync();
                    driver = new BiDiDriver(TimeSpan.FromSeconds(30));
                    await driver.StartAsync(webSocketUrl);
                }
            }
        }

        throw new Exception(
            $"Operation failed after {maxRetries} attempts",
            lastException);
    }
}
#endregion

/// <summary>
/// Error context capture for debugging.
/// </summary>
#region ErrorContextClass
public class ErrorContext
{
    public string Operation { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ContextId { get; set; }
    public string? Url { get; set; }
    public Exception Exception { get; set; }

    public void SaveToFile()
    {
        string fileName = $"error-{Timestamp:yyyyMMdd-HHmmss}.log";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Operation: {Operation}");
        sb.AppendLine($"Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
        sb.AppendLine($"Context ID: {ContextId ?? "N/A"}");
        sb.AppendLine($"URL: {Url ?? "N/A"}");
        sb.AppendLine($"\nException: {Exception.Message}");
        sb.AppendLine($"\nStack Trace:\n{Exception.StackTrace}");

        if (Exception.InnerException != null)
        {
            sb.AppendLine($"\nInner Exception: {Exception.InnerException.Message}");
        }

        File.WriteAllText(fileName, sb.ToString());
    }
}
#endregion

/// <summary>
/// Circuit breaker pattern for failure handling.
/// </summary>
#region CircuitBreakerClass
public class CircuitBreaker
{
    private int failureCount = 0;
    private readonly int threshold = 5;
    private readonly TimeSpan resetTimeout = TimeSpan.FromMinutes(1);
    private DateTime? openedAt = null;
    private bool isOpen = false;

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        if (isOpen)
        {
            if (openedAt.HasValue &&
                DateTime.Now - openedAt.Value > resetTimeout)
            {
                // Try to reset
                isOpen = false;
                failureCount = 0;
                openedAt = null;
            }
            else
            {
                throw new InvalidOperationException(
                    "Circuit breaker is open - too many failures");
            }
        }

        try
        {
            T result = await operation();

            // Success - reset counter
            if (failureCount > 0)
            {
                failureCount = 0;
            }

            return result;
        }
        catch (Exception)
        {
            failureCount++;

            if (failureCount >= threshold)
            {
                isOpen = true;
                openedAt = DateTime.Now;
            }

            throw;
        }
    }
}
#endregion

#pragma warning restore CS0168 // Variable declared but never used
