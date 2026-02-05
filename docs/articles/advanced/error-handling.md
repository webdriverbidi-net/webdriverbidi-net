# Error Handling

This guide covers comprehensive error handling strategies for WebDriverBiDi.NET-Relaxed applications.

## Overview

WebDriverBiDi.NET-Relaxed operations can fail for various reasons:
- Network connectivity issues
- Browser crashes or disconnections
- Invalid command parameters
- Timeout waiting for responses
- JavaScript exceptions in the browser
- Protocol-level errors

Understanding how to handle these errors properly is crucial for building robust automation.

## Exception Types

### WebDriverBiDiException

The primary exception type thrown by WebDriverBiDi.NET-Relaxed for protocol-level errors:

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

### Handling Exceptions in Observers

```csharp
// Bad: Unhandled exception crashes the observer
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    ProcessLogEntry(e);  // May throw
});

// Good: Exception handling in observer
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    try
    {
        ProcessLogEntry(e);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing log entry: {ex.Message}");
        // Log error but don't crash
    }
});

// Good: Async handler with error handling
driver.Network.OnBeforeRequestSent.AddObserver(async (e) =>
{
    try
    {
        if (e.IsBlocked)
        {
            await driver.Network.ContinueRequestAsync(
                new ContinueRequestCommandParameters(e.Request.RequestId));
        }
    }
    catch (WebDriverBiDiException ex)
    {
        Console.WriteLine($"Error handling request: {ex.Message}");
        // Continue execution, don't crash
    }
},
ObservableEventHandlerOptions.RunHandlerAsynchronously);
```

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

