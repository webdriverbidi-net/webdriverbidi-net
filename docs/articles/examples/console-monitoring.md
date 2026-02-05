# Console Monitoring Example

This example demonstrates how to capture and monitor browser console logs, JavaScript errors, and warnings using WebDriverBiDi.NET-Relaxed.

## Overview

This example shows:
- Capturing all console log messages
- Filtering by log level (error, warn, info, debug)
- Collecting logs for analysis
- Failing tests on JavaScript errors
- Logging to files

## Example 1: Basic Console Monitoring

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Log;
using WebDriverBiDi.Session;

namespace ConsoleMonitoringExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string webSocketUrl = "ws://localhost:9222/devtools/browser/YOUR-ID-HERE";
            BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

            try
            {
                await driver.StartAsync(webSocketUrl);
                Console.WriteLine("Connected to browser");

                // Set up console monitoring
                driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
                {
                    string timestamp = e.Timestamp.ToString("HH:mm:ss.fff");
                    Console.WriteLine($"[{timestamp}] [{e.Level}] {e.Text}");
                });

                // Subscribe to log events
                SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
                subscribe.Events.Add(driver.Log.OnEntryAdded.EventName);
                await driver.Session.SubscribeAsync(subscribe);

                Console.WriteLine("Console monitoring enabled\n");

                // Get browsing context
                GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                    new GetTreeCommandParameters());
                string contextId = tree.ContextTree[0].BrowsingContextId;

                // Navigate to a page that has console logs
                Console.WriteLine("Navigating to page with console output...\n");
                await driver.BrowsingContext.NavigateAsync(
                    new NavigateCommandParameters(contextId, "https://example.com")
                    { Wait = ReadinessState.Complete });

                // Trigger some console logs via JavaScript
                Console.WriteLine("\nGenerating console messages...\n");
                
                await driver.Script.EvaluateAsync(
                    new EvaluateCommandParameters(
                        "console.log('Info message')",
                        new ContextTarget(contextId),
                        false));

                await driver.Script.EvaluateAsync(
                    new EvaluateCommandParameters(
                        "console.warn('Warning message')",
                        new ContextTarget(contextId),
                        false));

                await driver.Script.EvaluateAsync(
                    new EvaluateCommandParameters(
                        "console.error('Error message')",
                        new ContextTarget(contextId),
                        false));

                await driver.Script.EvaluateAsync(
                    new EvaluateCommandParameters(
                        "console.debug('Debug message')",
                        new ContextTarget(contextId),
                        false));

                // Wait for logs to arrive
                await Task.Delay(1000);

                Console.WriteLine("\n✓ Console monitoring complete");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                await driver.StopAsync();
            }
        }
    }
}
```

## Example 2: Error Detection and Reporting

```csharp
// Track JavaScript errors
List<EntryAddedEventArgs> errors = new List<EntryAddedEventArgs>();
List<EntryAddedEventArgs> warnings = new List<EntryAddedEventArgs>();

driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    if (e.Level == LogLevel.Error)
    {
        errors.Add(e);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ ERROR: {e.Text}");
        Console.ResetColor();

        if (e.StackTrace != null)
        {
            Console.WriteLine("Stack trace:");
            foreach (var frame in e.StackTrace.CallFrames)
            {
                Console.WriteLine($"  at {frame.FunctionName} ({frame.Url}:{frame.LineNumber}:{frame.ColumnNumber})");
            }
        }
    }
    else if (e.Level == LogLevel.Warn)
    {
        warnings.Add(e);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠️ WARNING: {e.Text}");
        Console.ResetColor();
    }
});

// Subscribe
SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.Log.OnEntryAdded.EventName);
await driver.Session.SubscribeAsync(subscribe);

// Navigate and perform actions...

// Report summary
Console.WriteLine($"\n📊 Summary:");
Console.WriteLine($"   Errors: {errors.Count}");
Console.WriteLine($"   Warnings: {warnings.Count}");

// Fail if there were errors
if (errors.Count > 0)
{
    Console.WriteLine("\n❌ Test failed due to JavaScript errors");
    Environment.Exit(1);
}
else
{
    Console.WriteLine("\n✅ No JavaScript errors detected");
}
```

## Example 3: Detailed Log Analysis

```csharp
public class LogAnalyzer
{
    private List<EntryAddedEventArgs> allLogs = new List<EntryAddedEventArgs>();

    public void AddLog(EntryAddedEventArgs log)
    {
        allLogs.Add(log);
    }

    public void PrintSummary()
    {
        Console.WriteLine("\n📊 Log Analysis Report");
        Console.WriteLine("=".PadRight(50, '='));

        // Count by level
        var byLevel = allLogs.GroupBy(l => l.Level)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count);

        Console.WriteLine("\nBy Log Level:");
        foreach (var item in byLevel)
        {
            Console.WriteLine($"  {item.Level,-10} {item.Count,5}");
        }

        // Count by type
        var byType = allLogs.GroupBy(l => l.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count);

        Console.WriteLine("\nBy Log Type:");
        foreach (var item in byType)
        {
            Console.WriteLine($"  {item.Type,-15} {item.Count,5}");
        }

        // Show recent errors
        var recentErrors = allLogs
            .Where(l => l.Level == LogLevel.Error)
            .OrderByDescending(l => l.Timestamp)
            .Take(5);

        if (recentErrors.Any())
        {
            Console.WriteLine("\nRecent Errors:");
            foreach (var error in recentErrors)
            {
                Console.WriteLine($"  [{error.Timestamp:HH:mm:ss}] {error.Text}");
            }
        }

        // Show unique error messages
        var uniqueErrors = allLogs
            .Where(l => l.Level == LogLevel.Error)
            .GroupBy(l => l.Text)
            .Select(g => new { Message = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count);

        if (uniqueErrors.Any())
        {
            Console.WriteLine("\nMost Common Errors:");
            foreach (var error in uniqueErrors.Take(5))
            {
                Console.WriteLine($"  [{error.Count}x] {error.Message}");
            }
        }
    }
}

// Usage
LogAnalyzer analyzer = new LogAnalyzer();

driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    analyzer.AddLog(e);
    Console.WriteLine($"[{e.Level}] {e.Text}");
});

// ... navigate and perform actions ...

analyzer.PrintSummary();
```

## Example 4: Logging to File

```csharp
string logFilePath = $"browser-console-{DateTime.Now:yyyyMMdd-HHmmss}.log";

driver.Log.OnEntryAdded.AddObserver(async (EntryAddedEventArgs e) =>
{
    string logLine = $"{e.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{e.Level,-5}] [{e.Type}] {e.Text}";
    
    if (e.Source != null)
    {
        logLine += $" (Source: {e.Source.Realm})";
    }

    logLine += Environment.NewLine;

    if (e.StackTrace != null)
    {
        logLine += "Stack Trace:" + Environment.NewLine;
        foreach (var frame in e.StackTrace.CallFrames)
        {
            logLine += $"  at {frame.FunctionName} ({frame.Url}:{frame.LineNumber}:{frame.ColumnNumber})" 
                + Environment.NewLine;
        }
    }

    await File.AppendAllTextAsync(logFilePath, logLine);
},
ObservableEventHandlerOptions.RunHandlerAsynchronously);

Console.WriteLine($"Logging to file: {logFilePath}");

// ... perform test ...

Console.WriteLine($"\n✓ Complete log saved to {logFilePath}");
```

## Example 5: Filtering Console Logs

```csharp
// Only capture errors and warnings
driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    if (e.Level == LogLevel.Error || e.Level == LogLevel.Warn)
    {
        Console.WriteLine($"[{e.Level}] {e.Text}");
        
        // Additional processing for errors/warnings
        if (e.Source != null && e.Source.Context != null)
        {
            Console.WriteLine($"  Context: {e.Source.Context}");
        }
    }
});

// Only capture console API calls (not JavaScript errors)
driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    if (e is ConsoleLogEntry consoleEntry)
    {
        Console.WriteLine($"console.{consoleEntry.Method}() called");
        Console.WriteLine($"  Arguments: {consoleEntry.Args.Count}");
        
        foreach (var arg in consoleEntry.Args)
        {
            Console.WriteLine($"    Type: {arg.Type}, Value: {arg.Value}");
        }
    }
});

// Only capture JavaScript exceptions
driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    if (e.Type == "javascript" && e.Level == LogLevel.Error)
    {
        Console.WriteLine($"JavaScript Exception: {e.Text}");
    }
});
```

## Example 6: Console Log Assertions

```csharp
public class ConsoleAsserter
{
    private List<EntryAddedEventArgs> logs = new List<EntryAddedEventArgs>();

    public void AddLog(EntryAddedEventArgs log)
    {
        logs.Add(log);
    }

    public void AssertNoErrors()
    {
        var errors = logs.Where(l => l.Level == LogLevel.Error).ToList();
        if (errors.Any())
        {
            throw new Exception($"Expected no errors, but found {errors.Count}");
        }
    }

    public void AssertContainsMessage(string expectedText)
    {
        bool found = logs.Any(l => l.Text.Contains(expectedText));
        if (!found)
        {
            throw new Exception($"Expected log message containing '{expectedText}', but not found");
        }
    }

    public void AssertDoesNotContain(string unexpectedText)
    {
        var found = logs.Where(l => l.Text.Contains(unexpectedText)).ToList();
        if (found.Any())
        {
            throw new Exception($"Found unexpected log message: {found[0].Text}");
        }
    }

    public void AssertMessageCount(LogLevel level, int expectedCount)
    {
        int actualCount = logs.Count(l => l.Level == level);
        if (actualCount != expectedCount)
        {
            throw new Exception($"Expected {expectedCount} {level} messages, but found {actualCount}");
        }
    }
}

// Usage
ConsoleAsserter asserter = new ConsoleAsserter();

driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    asserter.AddLog(e);
});

// ... perform test actions ...

// Verify expectations
asserter.AssertNoErrors();
asserter.AssertContainsMessage("Page loaded successfully");
asserter.AssertDoesNotContain("undefined is not a function");
asserter.AssertMessageCount(LogLevel.Warn, 0);

Console.WriteLine("✅ All console log assertions passed");
```

## Example 7: Real-Time Console Display

```csharp
// Display console logs with color coding
driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    // Color code by level
    ConsoleColor color = e.Level switch
    {
        LogLevel.Error => ConsoleColor.Red,
        LogLevel.Warn => ConsoleColor.Yellow,
        LogLevel.Info => ConsoleColor.Cyan,
        LogLevel.Debug => ConsoleColor.Gray,
        _ => ConsoleColor.White
    };

    Console.ForegroundColor = color;
    
    string icon = e.Level switch
    {
        LogLevel.Error => "❌",
        LogLevel.Warn => "⚠️",
        LogLevel.Info => "ℹ️",
        LogLevel.Debug => "🔍",
        _ => "📝"
    };

    Console.Write($"{icon} ");
    Console.ResetColor();
    
    Console.Write($"[{e.Timestamp:HH:mm:ss.fff}] ");
    
    Console.ForegroundColor = color;
    Console.WriteLine(e.Text);
    Console.ResetColor();

    // Show source for errors
    if (e.Level == LogLevel.Error && e.Source != null)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"    Source: {e.Source.Realm}");
        Console.ResetColor();
    }
});
```

## Pattern: Collecting Logs Per Test

```csharp
public class TestLogger
{
    private List<EntryAddedEventArgs> testLogs = new List<EntryAddedEventArgs>();
    private EventObserver<EntryAddedEventArgs>? observer;

    public void StartLogging(BiDiDriver driver)
    {
        testLogs.Clear();
        observer = driver.Log.OnEntryAdded.AddObserver((e) =>
        {
            testLogs.Add(e);
        });
    }

    public void StopLogging()
    {
        observer?.Unobserve();
    }

    public List<EntryAddedEventArgs> GetLogs()
    {
        return new List<EntryAddedEventArgs>(testLogs);
    }

    public bool HasErrors()
    {
        return testLogs.Any(l => l.Level == LogLevel.Error);
    }

    public void SaveToFile(string testName)
    {
        string fileName = $"test-{testName}-{DateTime.Now:yyyyMMdd-HHmmss}.log";
        var lines = testLogs.Select(l => 
            $"{l.Timestamp:HH:mm:ss.fff} [{l.Level}] {l.Text}");
        File.WriteAllLines(fileName, lines);
    }
}

// Usage in test framework
TestLogger logger = new TestLogger();

// Before each test
logger.StartLogging(driver);

// ... run test ...

// After each test
if (logger.HasErrors())
{
    logger.SaveToFile("MyTest");
    throw new Exception("Test failed due to JavaScript errors");
}

logger.StopLogging();
```

## Best Practices

1. **Subscribe early**: Subscribe to log events before navigating
2. **Filter appropriately**: Don't process every log if you only need errors
3. **Use async handlers**: For file writing or database logging
4. **Store important logs**: Keep error logs for debugging
5. **Clean up**: Remove observers when done to prevent memory leaks
6. **Test for errors**: Fail tests if unexpected errors occur

## Common Issues

### Missing Logs

**Problem**: Not all console logs are captured.

**Solution**: 
- Ensure subscription happens before navigation
- Check that log level filtering isn't too restrictive
- Wait for async operations to complete

### Too Many Logs

**Problem**: Overwhelming amount of log output.

**Solution**:
- Filter by log level (errors/warnings only)
- Filter by log type (JavaScript errors only)
- Use conditional logging based on test phase

### Stack Traces Missing

**Problem**: Error logs don't include stack traces.

**Solution**: Stack traces are only available for some error types. Check `e.StackTrace != null` before accessing.

## Next Steps

- [Log Module](../modules/log.md): Complete log module guide
- [Events and Observables](../events-observables.md): Understanding event handling
- [Common Scenarios](common-scenarios.md): More examples

