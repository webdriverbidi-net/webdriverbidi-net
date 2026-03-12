// <copyright file="LogModuleSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/log.md

#pragma warning disable CS8600, CS8602

namespace WebDriverBiDi.Docs.Code.Modules;

using System.Collections.Generic;
using System.Linq;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Log;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for Log module documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class LogModuleSamples
{
    /// <summary>
    /// Accessing the module.
    /// </summary>
    public static void AccessingModule(BiDiDriver driver)
    {
#region AccessingModule
        LogModule log = driver.Log;
#endregion
    }

    /// <summary>
    /// Basic log monitoring with subscribe.
    /// </summary>
    public static async Task BasicLogMonitoring(BiDiDriver driver, string contextId)
    {
#region BasicLogMonitoring
        // Add observer
        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            Console.WriteLine($"[{e.Level}] {e.Text}");
        });

        // Subscribe to events
        SubscribeCommandParameters subscribe = 
            new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        // Navigate - console logs will be captured
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com"));
#endregion
    }

    /// <summary>
    /// Log entry properties.
    /// </summary>
    public static void LogEntryProperties(BiDiDriver driver)
    {
#region LogEntryProperties
        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            Console.WriteLine($"Level: {e.Level}");          // Error, Warn, Info, Debug
            Console.WriteLine($"Text: {e.Text}");            // Log message
            Console.WriteLine($"Timestamp: {e.Timestamp}");  // When logged
            Console.WriteLine($"Type: {e.Type}");            // Console, JavaScript
            Console.WriteLine($"Method: {e.Method}");        // log, error, warn, etc.

            if (e.Source != null)
            {
                Console.WriteLine($"Source: {e.Source.RealmId}");
                Console.WriteLine($"Context: {e.Source.Context}");
            }

            if (e.StackTrace != null)
            {
                Console.WriteLine("Stack trace:");
                foreach (StackFrame frame in e.StackTrace.CallFrames)
                {
                    Console.WriteLine($"  {frame.FunctionName} at {frame.Url}:{frame.LineNumber}");
                }
            }
        });
#endregion
    }

    /// <summary>
    /// Monitor only errors.
    /// </summary>
    public static void MonitorOnlyErrors(BiDiDriver driver)
    {
#region MonitorOnlyErrors
        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            if (e.Level == LogLevel.Error)
            {
                Console.WriteLine($"ERROR: {e.Text}");

                if (e.StackTrace != null)
                {
                    foreach (StackFrame frame in e.StackTrace.CallFrames)
                    {
                        Console.WriteLine($"  at {frame.Url}:{frame.LineNumber}:{frame.ColumnNumber}");
                    }
                }
            }
        });
#endregion
    }

    /// <summary>
    /// Monitor warnings and errors.
    /// </summary>
    public static void MonitorWarningsAndErrors(BiDiDriver driver)
    {
#region MonitorWarningsandErrors
        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            if (e.Level == LogLevel.Error || e.Level == LogLevel.Warn)
            {
                Console.WriteLine($"[{e.Level}] {e.Text}");
            }
        });
#endregion
    }

    /// <summary>
    /// Console logs from console.log, console.error, etc.
    /// </summary>
    public static void ConsoleLogs(BiDiDriver driver)
    {
#region ConsoleLogs
        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            if (e.Type == "console")
            {
                Console.WriteLine($"Console method: {e.Method}");
                Console.WriteLine($"Args: {e.Arguments?.Count ?? 0}");

                if (e.Arguments != null)
                {
                    foreach (RemoteValue arg in e.Arguments)
                    {
                        Console.WriteLine($"  Type: {arg.Type}");
                        Console.WriteLine($"  Value: {arg.Value}");
                    }
                }
            }
        });
#endregion
    }

    /// <summary>
    /// JavaScript errors (uncaught exceptions).
    /// </summary>
    public static void JavaScriptErrors(BiDiDriver driver)
    {
#region JavaScriptErrors
        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            if (e.Type == "javascript" && e.Level == LogLevel.Error)
            {
                Console.WriteLine($"JavaScript Error: {e.Text}");
            }
        });
#endregion
    }

    /// <summary>
    /// Collect all logs.
    /// </summary>
    public static async Task CollectAllLogs(BiDiDriver driver)
    {
#region CollectAllLogs
        List<EntryAddedEventArgs> logs = new List<EntryAddedEventArgs>();

        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            logs.Add(e);
        });

        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        // Perform actions...

        // Later, analyze logs
        int errorCount = logs.Count(l => l.Level == LogLevel.Error);
        int warnCount = logs.Count(l => l.Level == LogLevel.Warn);

        Console.WriteLine($"Errors: {errorCount}, Warnings: {warnCount}");
#endregion
    }

    /// <summary>
    /// Fail on JavaScript errors.
    /// </summary>
    public static async Task FailOnJavaScriptErrors(
        BiDiDriver driver,
        string contextId,
        NavigateCommandParameters navParams)
    {
#region FailonJavaScriptErrors
        bool hasErrors = false;

        driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
        {
            if (e.Type == "javascript" && e.Level == LogLevel.Error)
            {
                hasErrors = true;
                Console.WriteLine($"JavaScript error detected: {e.Text}");
            }
        });

        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        // Perform test actions...
        await driver.BrowsingContext.NavigateAsync(navParams);

        if (hasErrors)
        {
            throw new Exception("Test failed due to JavaScript errors");
        }
#endregion
    }

    /// <summary>
    /// Log to file.
    /// </summary>
    public static void LogToFile(BiDiDriver driver)
    {
#region LogtoFile
        string logFilePath = "browser-console.log";

        driver.Log.OnEntryAdded.AddObserver(async (EntryAddedEventArgs e) =>
        {
            string logLine = $"{e.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{e.Level}] {e.Text}\n";
            await File.AppendAllTextAsync(logFilePath, logLine);
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously);
#endregion
    }
}
