// <copyright file="ConsoleMonitoringSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/examples/console-monitoring.md

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace WebDriverBiDi.Docs.Code.Examples;

using System.Collections.Generic;
using System.Linq;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Log;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for console monitoring documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class ConsoleMonitoringSamples
{
    /// <summary>
    /// Basic console monitoring - trigger logs via Script.
    /// </summary>
    public static async Task BasicConsoleMonitoring()
    {
#region BasicConsoleMonitoring
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
            SubscribeCommandParameters subscribe = 
                new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName);
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
#endregion
    }

    /// <summary>
    /// Error detection and reporting.
    /// </summary>
    public static async Task ErrorDetectionAndReporting(BiDiDriver driver)
    {
#region ErrorDetectionandReporting
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
        SubscribeCommandParameters subscribe = 
            new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName);
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
#endregion
    }

    /// <summary>
    /// Filter console logs by level.
    /// </summary>
    public static void FilterConsoleLogs(BiDiDriver driver)
    {
#region FilterConsoleLogs
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
#endregion
    }

    /// <summary>
    /// Filter by console API calls using Method property.
    /// </summary>
    public static void FilterByConsoleApiCalls(BiDiDriver driver)
    {
#region FilterbyConsoleAPICalls
// Only capture console API calls (not JavaScript errors)
        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            if (e.Type == "console")
            {
                Console.WriteLine($"console.{e.Method}() called");
                Console.WriteLine($"  Arguments: {e.Arguments.Count}");
                
                foreach (var arg in e.Arguments)
                {
                    Console.WriteLine($"    Type: {arg.Type}, Value: {arg.ConvertTo<StringRemoteValue>().Value}");
                }
            }
        });
#endregion
    }

    /// <summary>
    /// Filter JavaScript exceptions.
    /// </summary>
    public static void FilterJavaScriptExceptions(BiDiDriver driver)
    {
#region FilterJavaScriptExceptions
        // Only capture JavaScript exceptions
        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            if (e.Type == "javascript" && e.Level == LogLevel.Error)
            {
                Console.WriteLine($"JavaScript Exception: {e.Text}");
            }
        });
#endregion
    }

    /// <summary>
    /// Logging to file with async handler.
    /// </summary>
    public static void LoggingToFile(BiDiDriver driver)
    {
#region LoggingtoFile
        string logFilePath = $"browser-console-{DateTime.Now:yyyyMMdd-HHmmss}.log";

        driver.Log.OnEntryAdded.AddObserver(async (EntryAddedEventArgs e) =>
        {
            string logLine = $"{e.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{e.Level,-5}] [{e.Type}] {e.Text}";
            
            if (e.Source != null)
            {
                logLine += $" (Source: {e.Source.RealmId})";
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
#endregion
    }

    /// <summary>
    /// Real-time console display with color coding.
    /// </summary>
    public static void RealTimeConsoleDisplay(BiDiDriver driver)
    {
#region Real-TimeConsoleDisplay
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
                Console.WriteLine($"    Source: {e.Source.RealmId}");
                Console.ResetColor();
            }
        });
#endregion
    }

    /// <summary>
    /// LogAnalyzer usage - add observer and print summary after actions.
    /// </summary>
    public static void LogAnalyzerUsage(BiDiDriver driver)
    {
#region LogAnalyzerusage
        // Usage
        LogAnalyzer analyzer = new LogAnalyzer();

        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            analyzer.AddLog(e);
            Console.WriteLine($"[{e.Level}] {e.Text}");
        });

        // ... navigate and perform actions ...

        analyzer.PrintSummary();
#endregion
    }
    /// <summary>
    /// ConsoleAsserter usage - add observer, perform actions, verify expectations.
    /// </summary>
    public static void ConsoleAsserterUsage(BiDiDriver driver)
    {
    #region ConsoleAsserterusage
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
    #endregion
    }
}

/// <summary>
/// Log analyzer for detailed log analysis and reporting.
/// </summary>
#region LogAnalyzerclass
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
#endregion

/// <summary>
/// Console log asserter for test verification.
/// </summary>
#region ConsoleAsserterclass
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
#endregion

/// <summary>
/// Test logger for collecting logs per test.
/// </summary>
#region TestLoggerclass
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
#endregion

/// <summary>
/// TestLogger usage in test framework.
/// </summary>
public static class TestLoggerUsage
{
    public static void Demonstrate(BiDiDriver driver)
    {
#region TestLoggerusage
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
#endregion
    }
}
