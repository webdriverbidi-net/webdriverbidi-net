// <copyright file="ObservabilitySamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/advanced/observability.md

#pragma warning disable CS8600, CS8602, CS8604, CS1591, CS0169, CS0649

namespace WebDriverBiDi.Docs.Code.Advanced;

using System.Diagnostics.Tracing;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Trace;
using WebDriverBiDi;

/// <summary>
/// Snippets for observability documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class ObservabilitySamples
{
    /// <summary>
    /// Console logging with EventListener.
    /// </summary>
    public static async Task ConsoleLogging()
    {
        #region ConsoleLogging
        // Create a simple console event listener
        using var listener = new ConsoleEventListener();

        // Use WebDriverBiDi normally - events will be logged to console
        await using var driver = new BiDiDriver();
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");
        #endregion
    }

    /// <summary>
    /// Custom EventListener implementation.
    /// </summary>
    public static void CustomEventListenerUsage()
    {
        #region CustomEventListenerUsage
        // Usage
        using var listener = new MyEventListener();
        #endregion
    }

    /// <summary>
    /// Performance monitor - track command execution times.
    /// </summary>
    public static void PerformanceMonitorUsage()
    {
        using var monitor = new PerformanceMonitor();
        monitor.PrintStatistics();
    }

    /// <summary>
    /// Error tracker - monitor errors.
    /// </summary>
    public static void ErrorTrackerUsage()
    {
        using var tracker = new ErrorTracker();
    }

    /// <summary>
    /// Filter by event name - handle only CommandError.
    /// </summary>
    public static void FilterByEventName(EventWrittenEventArgs eventData)
    {
        #region FilterbyEventName
        if (eventData.EventName == "CommandError")
        {
            // Handle only command errors
        }
        #endregion
    }

    /// <summary>
    /// Resource management - dispose listener.
    /// </summary>
    public static void ResourceManagement()
    {
        #region ResourceManagement
        using var listener = new MyEventListener();
        #endregion
    }

    /// <summary>
    /// Resource management - try/finally.
    /// </summary>
    public static void ResourceManagementTryFinally()
    {
        #region ResourceManagementTryFinally
        // or
        var listener = new MyEventListener();
        try
        {
            // use listener
        }
        finally
        {
            listener?.Dispose();
        }
        #endregion
    }

    /// <summary>
    /// Verify EventSource is enabled.
    /// </summary>
    public static void VerifyEventSourceEnabled()
    {
        #region VerifyEventSourceEnabled
        if (WebDriverBiDiEventSource.RaiseEvent.IsEnabled())
        {
            Console.WriteLine("EventSource is enabled");
        }
        #endregion
    }

    /// <summary>
    /// Check Verbose level enabled.
    /// </summary>
    public static void CheckVerboseEnabled()
    {
        #region CheckVerboseEnabled
        if (WebDriverBiDiEventSource.RaiseEvent.IsEnabled(EventLevel.Verbose, EventKeywords.None))
        {
            Console.WriteLine("Verbose events are enabled");
        }
        #endregion
    }

    /// <summary>
    /// Listener created before driver - correct order.
    /// </summary>
    public static async Task ListenerBeforeDriver()
    {
        #region ListenerBeforeDriver
        // CORRECT: Listener created first
        using var listener1 = new MyEventListener();
        await using var driver1 = new BiDiDriver();

        // INCORRECT: Listener created after driver
        await using var driver2 = new BiDiDriver();
        using var listener2 = new MyEventListener(); // May miss early events
        #endregion
    }

    /// <summary>
    /// Basic console application with Microsoft.Extensions.Logging and AddWebDriverBiDi.
    /// </summary>
    public static async Task LoggingBasicConsoleApplication()
    {
        #region BasicConsoleApplication
        // Setup dependency injection
        var services = new ServiceCollection();

        // Configure logging with WebDriverBiDi events
        services.AddLogging(builder =>
        {
            builder.AddConsole()
                .SetMinimumLevel(LogLevel.Debug);

            // Add WebDriverBiDi diagnostic event logging
            builder.AddWebDriverBiDi(EventLevel.Informational);
        });

        var serviceProvider = services.BuildServiceProvider();

        // Use WebDriverBiDi - events will be automatically logged
        await using var driver = new BiDiDriver();
        await driver.StartAsync("ws://localhost:9222");

        // Logs will show:
        // [12:34:56 INF] ConnectionOpening, connectionId=12345, url=ws://localhost:9222
        // [12:34:56 INF] ConnectionOpened, connectionId=12345, url=ws://localhost:9222
        // [12:34:56 INF] TransportStarted

        await driver.Session.StatusAsync();

        // Logs will show:
        // [12:34:56 DBG] CommandSending, commandId=1, method=session.status
        // [12:34:56 INF] CommandCompleted, commandId=1, method=session.status, elapsedMilliseconds=42
        #endregion
    }

    /// <summary>
    /// ASP.NET Core web application with WebDriverBiDi logging.
    /// </summary>
    public static WebApplication LoggingAspNetCoreWebApplication(string[] args)
    {
        #region ASPNETCoreWebApplication
        var builder = WebApplication.CreateBuilder(args);

        // Add WebDriverBiDi event logging to the ASP.NET Core logging pipeline
        builder.Logging.AddWebDriverBiDi(EventLevel.Informational);

        // Configure services
        builder.Services.AddScoped<IBrowserAutomationService, BrowserAutomationService>();

        var app = builder.Build();

        app.MapGet("/test", async (IBrowserAutomationService automation) =>
        {
            // WebDriverBiDi events will be logged through the ASP.NET Core logging infrastructure
            var result = await automation.RunTestAsync();
            return Results.Ok(result);
        });

        app.Run();
        #endregion
        return app;
    }

    /// <summary>
    /// Application Insights with AddWebDriverBiDi.
    /// </summary>
    public static void LoggingWithApplicationInsights()
    {
        #region ApplicationInsights
        var services = new ServiceCollection();

        // Add Application Insights
        services.AddApplicationInsightsTelemetry();

        // Configure logging with WebDriverBiDi events
        services.AddLogging(builder =>
        {
            builder.AddApplicationInsights();
            builder.AddWebDriverBiDi(EventLevel.Informational);
        });

        var serviceProvider = services.BuildServiceProvider();

        // WebDriverBiDi events will be sent to Application Insights with structured properties
        // allowing you to query and analyze automation telemetry
        #endregion
    }

    /// <summary>
    /// Filtering by event level - Warning and Error only.
    /// </summary>
    public static void LoggingFilterByEventLevel(ServiceCollection services, ILoggingBuilder builder)
    {
        #region FilterbyEventLevel
        services.AddLogging(builder =>
        {
            builder.AddConsole();

            // Only capture Warning and Error events
            builder.AddWebDriverBiDi(EventLevel.Warning);

            // Or use ILogger filtering
            builder.AddFilter("WebDriverBiDi.Logging", LogLevel.Warning);
        });
        #endregion
    }

    /// <summary>
    /// Configuration-based setup - AddWebDriverBiDi respects appsettings.json.
    /// </summary>
    public static WebApplication LoggingConfigurationBasedSetup(string[] args)
    {
        #region Configuration-basedSetup
        var builder = WebApplication.CreateBuilder(args);

        // Configuration is loaded from appsettings.json
        builder.Logging.AddWebDriverBiDi(); // Respects configured log levels

        var app = builder.Build();
        #endregion
        return app;
    }

    /// <summary>
    /// Custom event processing - register CustomWebDriverEventListener.
    /// </summary>
    public static void LoggingCustomEventProcessingRegistration(IServiceCollection services)
    {
        #region RegisterCustomEventListener
        // Register as singleton
        services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<CustomWebDriverEventListener>>();
            return new CustomWebDriverEventListener(logger);
        });
        #endregion
    }

    /// <summary>
    /// OpenTelemetry integration for distributed tracing.
    /// </summary>
    public static void OpenTelemetryIntegration()
    {
        #region OpenTelemetryIntegration
        var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("WebDriverBiDi")
            .AddConsoleExporter()
            .Build();

        // Now use WebDriverBiDi - traces will be collected
        #endregion
    }
}

internal class BrowserAutomationService : IBrowserAutomationService
{
    public Task<bool> RunTestAsync()
    {
        throw new NotImplementedException();
    }
}

internal interface IBrowserAutomationService
{
    Task<bool> RunTestAsync();
}

internal class ConsoleEventListener : IDisposable
{
    public ConsoleEventListener()
    {
    }

    public void Dispose()
    {
    }
}

/// <summary>
/// Simple console event listener for WebDriverBiDi.
/// </summary>
public class ObservabilityConsoleEventListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource source)
    {
        if (source.Name == "WebDriverBiDi")
        {
            EnableEvents(source, EventLevel.Informational);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        Console.WriteLine($"[{eventData.Level}] {eventData.EventName}");
    }
}

/// <summary>
/// Custom EventListener with structured payload logging.
/// </summary>
#region CustomEventListener
public class MyEventListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource source)
    {
        // Enable WebDriverBiDi events at Informational level
        if (source.Name == "WebDriverBiDi")
        {
            EnableEvents(source, EventLevel.Informational);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        Console.WriteLine($"[{eventData.Level}] {eventData.EventName}");

        // Access structured payload
        if (eventData.PayloadNames != null)
        {
            for (int i = 0; i < eventData.PayloadNames.Count; i++)
            {
                Console.WriteLine($"  {eventData.PayloadNames[i]}: {eventData.Payload?[i]}");
            }
        }
    }
}
#endregion

/// <summary>
/// Performance monitor - track command execution times.
/// </summary>
#region PerformanceMonitor
public class PerformanceMonitor : EventListener
{
    private readonly Dictionary<string, List<long>> timings = new();

    protected override void OnEventSourceCreated(EventSource source)
    {
        if (source.Name == "WebDriverBiDi")
        {
            EnableEvents(source, EventLevel.Informational);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData.EventName == "CommandCompleted")
        {
            string method = eventData.Payload?[1]?.ToString() ?? "unknown";
            long elapsed = Convert.ToInt64(eventData.Payload?[2]);

            if (!timings.ContainsKey(method))
            {
                timings[method] = new List<long>();
            }
            timings[method].Add(elapsed);

            // Alert on slow commands
            if (elapsed > 1000)
            {
                Console.WriteLine($"SLOW: {method} took {elapsed}ms");
            }
        }
    }

    public void PrintStatistics()
    {
        foreach (var kvp in timings.OrderByDescending(x => x.Value.Average()))
        {
            Console.WriteLine($"{kvp.Key}: avg={kvp.Value.Average():F2}ms, max={kvp.Value.Max()}ms");
        }
    }
}
#endregion

/// <summary>
/// Error tracker - monitor errors for debugging.
/// </summary>
#region ErrorTracker
public class ErrorTracker : EventListener
{
    private int errorCount = 0;
    private readonly List<string> recentErrors = new();

    protected override void OnEventSourceCreated(EventSource source)
    {
        if (source.Name == "WebDriverBiDi")
        {
            EnableEvents(source, EventLevel.Warning); // Warning and above
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData.Level == EventLevel.Error)
        {
            errorCount++;
            string error = $"{eventData.EventName}: {string.Join(", ", eventData.PayloadNames)}";
            recentErrors.Add(error);

            // Keep only last 100 errors
            if (recentErrors.Count > 100)
            {
                recentErrors.RemoveAt(0);
            }

            // Send to monitoring service
            if (errorCount % 10 == 0)
            {
                Console.WriteLine($"ALERT: {errorCount} errors occurred!");
            }
        }
    }
}
#endregion

/// <summary>
/// Custom EventListener with ILogger for custom event processing.
/// </summary>
#region CustomEventListenerwithILogger
public class CustomWebDriverEventListener : EventListener
{
    private readonly ILogger logger;

    public CustomWebDriverEventListener(ILogger logger)
    {
        this.logger = logger;
    }

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "WebDriverBiDi")
        {
            EnableEvents(eventSource, EventLevel.Informational);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        // Custom processing here
        if (eventData.EventName == "CommandCompleted")
        {
            long elapsedMs = Convert.ToInt64(eventData.Payload?[2]);
            if (elapsedMs > 1000)
            {
                logger.LogWarning("Slow command detected: {Method} took {ElapsedMs}ms",
                    eventData.Payload?[1], elapsedMs);
            }
        }
    }
}
#endregion
