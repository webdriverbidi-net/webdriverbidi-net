# Log Module

The Log module provides access to browser console logs and other logging events.

## Overview

The Log module allows you to:

- Capture console log messages
- Monitor JavaScript errors
- Track different log levels
- Access log metadata

## Accessing the Module

```csharp
LogModule log = driver.Log;
```

## Monitoring Console Logs

### Basic Log Monitoring

```csharp
// Add observer
driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    Console.WriteLine($"[{e.Level}] {e.Text}");
});

// Subscribe to events
SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.Log.OnEntryAdded.EventName);
await driver.Session.SubscribeAsync(subscribe);

// Navigate - console logs will be captured
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com"));
```

### Log Entry Properties

```csharp
driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    Console.WriteLine($"Level: {e.Level}");          // Error, Warn, Info, Debug
    Console.WriteLine($"Text: {e.Text}");            // Log message
    Console.WriteLine($"Timestamp: {e.Timestamp}");  // When logged
    Console.WriteLine($"Type: {e.Type}");            // Console, JavaScript
    Console.WriteLine($"Method: {e.Method}");        // log, error, warn, etc.
    
    if (e.Source != null)
    {
        Console.WriteLine($"Source: {e.Source.Realm}");
        Console.WriteLine($"Context: {e.Source.Context}");
    }
    
    if (e.StackTrace != null)
    {
        Console.WriteLine("Stack trace:");
        foreach (var frame in e.StackTrace.CallFrames)
        {
            Console.WriteLine($"  {frame.FunctionName} at {frame.Url}:{frame.LineNumber}");
        }
    }
});
```

## Filtering by Log Level

### Monitor Only Errors

```csharp
driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    if (e.Level == LogLevel.Error)
    {
        Console.WriteLine($"ERROR: {e.Text}");
        
        if (e.StackTrace != null)
        {
            foreach (var frame in e.StackTrace.CallFrames)
            {
                Console.WriteLine($"  at {frame.Url}:{frame.LineNumber}:{frame.ColumnNumber}");
            }
        }
    }
});
```

### Monitor Warnings and Errors

```csharp
driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    if (e.Level == LogLevel.Error || e.Level == LogLevel.Warn)
    {
        Console.WriteLine($"[{e.Level}] {e.Text}");
    }
});
```

## Log Types

### Console Logs

These come from `console.log()`, `console.error()`, etc.:

```csharp
driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    if (e is ConsoleLogEntry consoleEntry)
    {
        Console.WriteLine($"Console method: {consoleEntry.Method}");
        Console.WriteLine($"Args: {consoleEntry.Args.Count}");
        
        foreach (var arg in consoleEntry.Args)
        {
            Console.WriteLine($"  Type: {arg.Type}");
            Console.WriteLine($"  Value: {arg.Value}");
        }
    }
});
```

### JavaScript Errors

Uncaught JavaScript exceptions:

```csharp
driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    if (e.Type == "javascript" && e.Level == LogLevel.Error)
    {
        Console.WriteLine($"JavaScript Error: {e.Text}");
    }
});
```

## Common Patterns

### Collect All Logs

```csharp
List<EntryAddedEventArgs> logs = new List<EntryAddedEventArgs>();

driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    logs.Add(e);
});

SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.Log.OnEntryAdded.EventName);
await driver.Session.SubscribeAsync(subscribe);

// Perform actions...

// Later, analyze logs
int errorCount = logs.Count(l => l.Level == LogLevel.Error);
int warnCount = logs.Count(l => l.Level == LogLevel.Warn);

Console.WriteLine($"Errors: {errorCount}, Warnings: {warnCount}");
```

### Fail on JavaScript Errors

```csharp
bool hasErrors = false;

driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    if (e.Type == "javascript" && e.Level == LogLevel.Error)
    {
        hasErrors = true;
        Console.WriteLine($"JavaScript error detected: {e.Text}");
    }
});

SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.Log.OnEntryAdded.EventName);
await driver.Session.SubscribeAsync(subscribe);

// Perform test actions...
await driver.BrowsingContext.NavigateAsync(navParams);

if (hasErrors)
{
    throw new Exception("Test failed due to JavaScript errors");
}
```

### Log to File

```csharp
string logFilePath = "browser-console.log";

driver.Log.OnEntryAdded.AddObserver(async (EntryAddedEventArgs e) =>
{
    string logLine = $"{e.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{e.Level}] {e.Text}\n";
    await File.AppendAllTextAsync(logFilePath, logLine);
},
ObservableEventHandlerOptions.RunHandlerAsynchronously);
```

## Best Practices

1. **Subscribe early**: Subscribe to log events before navigating
2. **Filter appropriately**: Don't process every log if you only need errors
3. **Handle async logging**: Use async handlers if writing to files or databases
4. **Store important logs**: Keep error logs for debugging
5. **Test for errors**: Fail tests if unexpected JavaScript errors occur

## Next Steps

- [Events and Observables](../events-observables.md): Understanding event handling
- [Examples: Console Monitoring](../examples/console-monitoring.md): Complete examples
- [API Reference](../../api/index.md): Complete API documentation

