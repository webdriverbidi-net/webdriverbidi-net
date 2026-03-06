# Error Handling

This guide covers comprehensive error handling strategies for WebDriverBiDi.NET applications.

## Overview

WebDriverBiDi.NET operations can fail for various reasons:
- Network connectivity issues
- Browser crashes or disconnections
- Invalid command parameters
- Timeout waiting for responses
- JavaScript exceptions in the browser
- Protocol-level errors

Understanding how to handle these errors properly is crucial for building robust automation.

## Exception Types

### WebDriverBiDiException

The primary exception type thrown by WebDriverBiDi.NET for protocol-level errors:

```csharp
try
{
    await driver.BrowsingContext.NavigateAsync(navParams);
}
catch (WebDriverBiDiException ex)
{
    Console.WriteLine($"WebDriver BiDi error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
```

### Common Error Scenarios

```csharp
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
```

## Transport Error Behavior Configuration

WebDriverBiDi.NET allows you to configure how transport-layer errors are handled using the `TransportErrorBehavior` enum. This controls errors that occur in event handlers and protocol-level errors (like invalid JSON or incorrect payloads).

**Important:** Command errors always throw exceptions immediately, regardless of this setting. This behavior only affects:
- Exceptions thrown by event handlers
- Protocol errors (invalid JSON, malformed messages)
- Unexpected error responses without matching commands

```csharp
public enum TransportErrorBehavior
{
    Ignore,     // Silently ignore transport errors (default)
    Collect,    // Store errors for later inspection
    Terminate   // Throw exception on next command
}
```

### Understanding Event Handler Error Propagation

Event handlers run on separate threads from your main application code. This means exceptions in event handlers don't directly propagate to the calling code. The transport error behavior determines what happens when event handler exceptions occur:

- **Ignore (default)**: Exception is discarded and logged
- **Collect**: Exception is stored in a list for later inspection
- **Terminate**: Exception is stored and thrown when you send the next command

### Why Ignore is the Default

WebDriverBiDi.NET defaults all error behaviors to `Ignore` for several important reasons:

**1. Protocol Stability During Evolution**
- The WebDriver BiDi protocol is actively evolving with new features being added regularly
- Browsers may send events or messages that aren't yet fully specified
- Unknown message types (valid JSON that doesn't match any known structure) are common during protocol transitions
- `Ignore` mode allows automation to continue working even when protocols diverge slightly between library and browser versions

**2. Event Handler Resilience**
- Event handlers are secondary to the main automation flow
- In many cases, a failing log observer or network monitor shouldn't halt critical automation workflows
- Users can explicitly handle errors within their handlers using try-catch blocks
- Forcing all users to handle potential event handler exceptions would add significant boilerplate

**3. Graceful Degradation**
- Libraries like this are often used for web scraping, testing, and monitoring where partial success is acceptable
- Stopping the entire driver on a malformed protocol message would be overly aggressive
- Users can opt-in to stricter error handling (Terminate mode) when needed

**4. Backward Compatibility**
- As browsers implement new WebDriver BiDi features, older library versions will encounter unknown protocol extensions
- `Ignore` allows older library versions to continue functioning with newer browsers
- This is especially important for long-running automation infrastructure

**When to Change from the Default:**
- **Development/Testing**: Use `Terminate` mode to catch issues early and ensure proper error handling
- **Diagnostics**: Use `Collect` mode to gather all errors for troubleshooting
- **Production (with mature code)**: Consider `Terminate` mode once your automation is stable and tested

### Ignore Mode (Default)

Ignore mode silently discards all transport errors. This is the default behavior for all error types:

```csharp
using WebDriverBiDi;

// Default behavior - errors are ignored
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

try
{
    await driver.StartAsync("ws://localhost:9222/session");

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
```

**Use Ignore Mode When:**
- Working with evolving protocol versions where unknown messages are expected
- Event handler failures shouldn't stop critical automation workflows
- You handle all errors within event handlers themselves using try-catch
- Operating in scenarios where graceful degradation is preferred
- Backward compatibility is more important than strict error reporting

**⚠️ Note:** While this is the default, consider using `Terminate` mode during development to catch issues early. The default exists primarily for protocol stability and backward compatibility, not because it's always the best choice for your use case

### Collect Mode

Collect mode stores transport errors in a list without ever throwing them:

```csharp
using WebDriverBiDi;
using WebDriverBiDi.Protocol;

// Create transport with Collect behavior
WebSocketConnection connection = new WebSocketConnection();
Transport transport = new Transport(connection, TransportErrorBehavior.Collect);
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);

try
{
    await driver.StartAsync("ws://localhost:9222/session");

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

    // Explicitly check for collected errors when ready
    if (transport.Errors.Count > 0)
    {
        Console.WriteLine($"\nCollected {transport.Errors.Count} transport errors:");
        foreach (Exception error in transport.Errors)
        {
            Console.WriteLine($"  [{error.GetType().Name}] {error.Message}");
            Console.WriteLine($"    From: {error.StackTrace?.Split('\n')[0].Trim()}");
        }

        // Analyze error types
        var eventHandlerErrors = transport.Errors
            .Where(e => e.StackTrace?.Contains("AddObserver") == true)
            .ToList();

        var protocolErrors = transport.Errors
            .OfType<WebDriverBiDiProtocolException>()
            .ToList();

        Console.WriteLine($"  Event handler errors: {eventHandlerErrors.Count}");
        Console.WriteLine($"  Protocol errors: {protocolErrors.Count}");
    }
}
finally
{
    await driver.StopAsync();
}
```

**Use Collect Mode When:**
- Diagnosing flaky or failing event handlers
- You want to continue operation despite event handler errors
- Testing error resilience of your event handling code
- You need a complete error report after operations
- Protocol errors might be transient

**Important:** Your code continues normally—errors never throw, only accumulate in the list.

### Terminate Mode

Terminate mode stores exceptions from event handlers and throws them when you send the next command:

```csharp
using WebDriverBiDi;
using WebDriverBiDi.Protocol;

// Create transport with Terminate behavior (opt-in to stricter error handling)
WebSocketConnection connection = new WebSocketConnection();
Transport transport = new Transport(connection, TransportErrorBehavior.Terminate);
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);

try
{
    await driver.StartAsync("ws://localhost:9222/session");

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
```

**Why This Matters:**
- Event handlers execute asynchronously on the transport thread
- Your main code doesn't directly wait for event handlers to complete
- Terminate mode ensures errors are eventually reported to your code
- The error surfaces when you send the next command, which is a natural synchronization point

**Use Terminate Mode When:**
- You want event handler errors to be reported (recommended for development)
- You need fast failure on protocol errors
- Operating in production with known stable protocol versions
- You prefer explicit error handling over silent failures

### Behavior Comparison

| Mode | Event Handler Exceptions | Protocol Errors | Command Errors | When Error Surfaces |
|------|-------------------------|-----------------|----------------|---------------------|
| **Ignore (default)** | Discarded and logged | Discarded and logged | Always throws immediately | Never |
| **Collect** | Stored in list | Stored in list | Always throws immediately | Never (inspect list manually) |
| **Terminate** | Throws on next command | Throws on next command | Always throws immediately | Synchronization point (next command) |

### Threading Model and Error Propagation

Understanding the threading model is crucial for error handling:

```csharp
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.StartAsync("ws://localhost:9222/session");

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
```

### Connection-Level Error Monitoring

For real-time error visibility without affecting behavior, use connection events:

```csharp
using WebDriverBiDi.Protocol;

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

await driver.StartAsync("ws://localhost:9222/session");
```

### Complete Error Handling Pattern

Combine all approaches for comprehensive error management:

```csharp
using WebDriverBiDi;
using WebDriverBiDi.Protocol;

// Use Collect mode to gather all errors
WebSocketConnection connection = new WebSocketConnection();
Transport transport = new Transport(connection, TransportErrorBehavior.Collect);

// Monitor errors in real-time via connection events
connection.OnConnectionError.AddObserver((errorArgs) =>
{
    LogErrorAsync($"Transport error occurred: {errorArgs.Exception.Message}");
});

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);

try
{
    await driver.StartAsync("ws://localhost:9222/session");

    // Set up event handlers with internal error handling
    driver.Log.OnEntryAdded.AddObserver((e) =>
    {
        try
        {
            ProcessLogEntry(e);
        }
        catch (Exception ex)
        {
            // Handle error in handler itself
            LogError($"Error in log handler: {ex.Message}");
            throw;  // Still collected by transport
        }
    });

    await driver.Session.SubscribeAsync(subscribeParams);

    // Execute commands (errors always throw immediately)
    try
    {
        await driver.BrowsingContext.NavigateAsync(navParams);
    }
    catch (WebDriverBiDiException ex)
    {
        Console.WriteLine($"Command failed: {ex.Message}");
        throw;
    }

    // Check collected transport errors at appropriate checkpoints
    if (transport.Errors.Count > 0)
    {
        Console.WriteLine($"\nTransport errors collected during operation:");
        ReportCollectedErrors(transport.Errors);

        // Decide how to handle
        if (transport.Errors.Any(e => e is WebDriverBiDiProtocolException))
        {
            Console.WriteLine("Protocol errors detected - connection may be unstable");
        }
    }
}
finally
{
    await driver.StopAsync();
}
```

### Best Practices

1. **Use Terminate mode (default) in production**: Ensures errors are reported
2. **Handle errors inside event handlers**: Use try-catch within handlers when possible
3. **Use Collect mode for diagnostics**: Helpful for troubleshooting event handler issues
4. **Monitor connection events**: Use OnConnectionError for real-time error visibility
5. **Remember the threading model**: Event handlers run on separate threads
6. **Check for errors at logical points**: With Collect mode, inspect errors after operations
7. **Never rely on Ignore mode**: Always prefer explicit error handling

## Script Execution Errors

### Handling JavaScript Exceptions

JavaScript errors are returned as `EvaluateResultException` rather than thrown:

```csharp
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "document.querySelector('.missing').textContent",
        new ContextTarget(contextId),
        true));

if (result is EvaluateResultSuccess success)
{
    string text = success.Result.ValueAs<string>();
    Console.WriteLine($"Text: {text}");
}
else if (result is EvaluateResultException exception)
{
    Console.WriteLine($"JavaScript error: {exception.ExceptionDetails.Text}");
    Console.WriteLine($"Line: {exception.ExceptionDetails.LineNumber}");
    Console.WriteLine($"Column: {exception.ExceptionDetails.ColumnNumber}");
    
    if (exception.ExceptionDetails.StackTrace != null)
    {
        Console.WriteLine("Stack trace:");
        foreach (var frame in exception.ExceptionDetails.StackTrace.CallFrames)
        {
            Console.WriteLine($"  at {frame.FunctionName} ({frame.Url}:{frame.LineNumber})");
        }
    }
    
    // Handle the error appropriately
}
```

### Safe Script Execution Pattern

```csharp
public async Task<T?> TryEvaluateAsync<T>(
    BiDiDriver driver,
    string expression,
    string contextId,
    T? defaultValue = default)
{
    try
    {
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                expression,
                new ContextTarget(contextId),
                true));

        if (result is EvaluateResultSuccess success)
        {
            return success.Result.ValueAs<T>();
        }
        else if (result is EvaluateResultException exception)
        {
            Console.WriteLine($"Script error: {exception.ExceptionDetails.Text}");
            return defaultValue;
        }
    }
    catch (WebDriverBiDiException ex)
    {
        Console.WriteLine($"Command error: {ex.Message}");
    }
    
    return defaultValue;
}

// Usage
string? title = await TryEvaluateAsync<string>(
    driver, 
    "document.title", 
    contextId,
    "Unknown");
```

## Connection Errors

### Handling Disconnections

```csharp
public class ResilientDriver
{
    private BiDiDriver driver;
    private string webSocketUrl;
    private int maxRetries = 3;

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

// Usage
ResilientDriver resilientDriver = new ResilientDriver();

NavigateCommandResult result = await resilientDriver.ExecuteWithRetryAsync(
    async (d) => await d.BrowsingContext.NavigateAsync(navParams));
```

## Timeout Handling

### Configuring Timeouts

```csharp
// Per-driver timeout
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

// Per-command timeout
NavigateCommandResult result = await driver.ExecuteCommandAsync<NavigateCommandResult>(
    navParams,
    TimeSpan.FromSeconds(60));  // Override for this command
```

### Timeout Patterns

```csharp
public async Task<NavigateCommandResult?> NavigateWithTimeoutAsync(
    BiDiDriver driver,
    NavigateCommandParameters parameters,
    TimeSpan timeout)
{
    Task<NavigateCommandResult> navigationTask = 
        driver.BrowsingContext.NavigateAsync(parameters);
    
    Task timeoutTask = Task.Delay(timeout);
    
    Task completedTask = await Task.WhenAny(navigationTask, timeoutTask);
    
    if (completedTask == navigationTask)
    {
        return await navigationTask;
    }
    else
    {
        Console.WriteLine($"Navigation timeout after {timeout.TotalSeconds}s");
        return null;
    }
}

// Usage
NavigateCommandResult? result = await NavigateWithTimeoutAsync(
    driver,
    navParams,
    TimeSpan.FromSeconds(30));

if (result == null)
{
    Console.WriteLine("Navigation failed - timeout");
    // Handle timeout
}
```

## Element Not Found Handling

### Safe Element Location

```csharp
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
```

### Wait for Element Pattern

```csharp
public async Task<bool> WaitForElementAsync(
    BiDiDriver driver,
    string contextId,
    string selector,
    TimeSpan timeout)
{
    string waitScript = $@"
        new Promise((resolve) => {{
            const checkElement = () => {{
                const element = document.querySelector('{selector}');
                if (element) {{
                    resolve(true);
                }} else {{
                    setTimeout(checkElement, 100);
                }}
            }};
            checkElement();
            setTimeout(() => resolve(false), {timeout.TotalMilliseconds});
        }})";
    
    try
    {
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                waitScript,
                new ContextTarget(contextId),
                true));
        
        if (result is EvaluateResultSuccess success)
        {
            return success.Result.ValueAs<bool>();
        }
    }
    catch (WebDriverBiDiException ex)
    {
        Console.WriteLine($"Error waiting for element: {ex.Message}");
    }
    
    return false;
}
```

## Event Handler Errors

### Observable Event Handler Options

Event handlers can be configured with `ObservableEventHandlerOptions` to control execution behavior:

```csharp
[Flags]
public enum ObservableEventHandlerOptions
{
    None = 0,                      // Synchronous execution (default)
    RunHandlerAsynchronously = 1   // Asynchronous execution
}
```

### Synchronous vs. Asynchronous Handlers

By default (`None`), event handlers run synchronously on the transport thread, which blocks message processing:

```csharp
// ❌ Bad: Synchronous handler blocks transport thread
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    // This runs on the transport thread, blocking it
    Thread.Sleep(1000);  // Blocks ALL message processing for 1 second!
    ProcessLogEntry(e);
});

// ❌ Also bad: Default (None) still blocks
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    ProcessLogEntry(e);  // Blocks transport thread until complete
}, ObservableEventHandlerOptions.None);

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
```

**When to Use Asynchronous Handlers:**
- Handler performs I/O operations (file, network, database)
- Handler does CPU-intensive work
- Handler calls async APIs
- You want to avoid blocking message processing
- **Recommended for most scenarios where handler does more than trivial work**

### Exception Handling in Event Handlers

Event handler exceptions are subject to the `TransportErrorBehavior` setting:

```csharp
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
```

### Handler Execution Behavior

```csharp
using WebDriverBiDi;

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.StartAsync("ws://localhost:9222/session");

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
}, ObservableEventHandlerOptions.None);

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
```

### Multiple Handlers for Same Event

When multiple handlers are registered, they all execute in sequence:

```csharp
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
```

```csharp
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
```

### Complete Event Handler Pattern

```csharp
using WebDriverBiDi;
using WebDriverBiDi.Protocol;

// Use Collect mode to see all handler errors
WebSocketConnection connection = new WebSocketConnection();
Transport transport = new Transport(connection, TransportErrorBehavior.Collect);
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);

await driver.StartAsync("ws://localhost:9222/session");

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

// After operations, check for any unhandled handler errors
if (transport.Errors.Count > 0)
{
    Console.WriteLine($"Transport errors occurred: {transport.Errors.Count}");
    foreach (var error in transport.Errors)
    {
        Console.WriteLine($"  - {error.Message}");
    }
}
```

### Handler Options Decision Guide

| Handler Does | Use Option | Why |
|--------------|------------|-----|
| Quick in-memory work (<10ms) | None (default) | Minimal overhead, acceptable blocking |
| File I/O | RunHandlerAsynchronously | Don't block transport on disk operations |
| Network requests | RunHandlerAsynchronously | Don't block transport on network |
| Database queries | RunHandlerAsynchronously | Don't block transport on DB operations |
| CPU-intensive work | RunHandlerAsynchronously | Don't block transport thread |
| Logging to console | None | Quick operation, synchronous is fine |
| Updating counters/state | None | Quick operation, synchronous is fine |

### Best Practices for Event Handlers

1. **Use async handlers for I/O**: Prevent blocking the transport thread
2. **Handle exceptions internally**: Use try-catch within handlers when possible
3. **Keep handlers fast**: Even async handlers should complete quickly
4. **Don't perform long operations**: Offload heavy work to background services
5. **Test handler error paths**: Verify your error handling works correctly
6. **Monitor handler performance**: Track execution time to identify bottlenecks

## Validation and Defensive Programming

### Parameter Validation

```csharp
public async Task<NavigateCommandResult> SafeNavigateAsync(
    BiDiDriver driver,
    string contextId,
    string url)
{
    // Validate inputs
    if (driver == null)
        throw new ArgumentNullException(nameof(driver));
    
    if (string.IsNullOrWhiteSpace(contextId))
        throw new ArgumentException("Context ID cannot be empty", nameof(contextId));
    
    if (string.IsNullOrWhiteSpace(url))
        throw new ArgumentException("URL cannot be empty", nameof(url));
    
    if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        throw new ArgumentException("Invalid URL format", nameof(url));
    
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
```

### Context Validation

```csharp
public async Task<bool> IsContextValidAsync(BiDiDriver driver, string contextId)
{
    try
    {
        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
            new GetTreeCommandParameters());
        
        return tree.ContextTree.Any(c => c.BrowsingContextId == contextId);
    }
    catch (WebDriverBiDiException)
    {
        return false;
    }
}

// Usage
if (!await IsContextValidAsync(driver, contextId))
{
    Console.WriteLine("Context no longer exists");
    // Handle invalid context
}
```

## Logging and Diagnostics

### Comprehensive Logging

```csharp
public class DiagnosticDriver
{
    private BiDiDriver driver;
    private ILogger logger;

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
```

### Error Context Capture

```csharp
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
```

## Recovery Strategies

### Graceful Degradation

```csharp
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
        
        if (result is EvaluateResultSuccess success)
        {
            return success.Result.ValueAs<string>() ?? "Unknown";
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
            new GetTreeCommandParameters { Root = contextId });
        
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
```

### Circuit Breaker Pattern

```csharp
public class CircuitBreaker
{
    private int failureCount = 0;
    private int threshold = 5;
    private TimeSpan resetTimeout = TimeSpan.FromMinutes(1);
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
```

## Testing Error Scenarios

### Simulating Errors

```csharp
[Test]
public async Task TestNavigationTimeout()
{
    BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(1));
    
    try
    {
        await driver.StartAsync(webSocketUrl);
        
        // This should timeout
        NavigateCommandParameters params = new NavigateCommandParameters(
            contextId,
            "https://httpstat.us/200?sleep=5000");  // 5 second delay
        
        await driver.BrowsingContext.NavigateAsync(params);
        
        Assert.Fail("Expected timeout exception");
    }
    catch (WebDriverBiDiTimeoutException ex)
    {
        Assert.That(ex.Message, Does.Contain("Timed out"));
    }
}

[Test]
public async Task TestInvalidContext()
{
    try
    {
        // Use invalid context ID
        NavigateCommandParameters params = new NavigateCommandParameters(
            "invalid-context-id",
            "https://example.com");
        
        await driver.BrowsingContext.NavigateAsync(params);
        
        Assert.Fail("Expected context error");
    }
    catch (WebDriverBiDiException ex)
    {
        Assert.That(ex.Message, Does.Contain("no such frame").IgnoreCase);
    }
}
```

## Best Practices

1. **Always handle WebDriverBiDiException**: Protocol errors should be caught and handled
2. **Check script result types**: Don't assume success, check for exceptions
3. **Validate inputs**: Check parameters before sending commands
4. **Log errors comprehensively**: Include context for debugging
5. **Implement retries**: Handle transient failures with retry logic
6. **Use timeouts**: Always specify appropriate timeouts
7. **Clean up resources**: Use try-finally or using statements
8. **Test error paths**: Write tests for failure scenarios
9. **Provide fallbacks**: Implement graceful degradation where possible
10. **Monitor health**: Track error rates and patterns

## Common Pitfalls

### Don't Swallow Exceptions

```csharp
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
```

### Don't Use Generic Catch

```csharp
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
```

## Next Steps

- [Performance Considerations](performance.md): Optimize your automation
- [Core Concepts](../core-concepts.md): Understanding the fundamentals
- [Architecture](../architecture.md): System design and patterns

