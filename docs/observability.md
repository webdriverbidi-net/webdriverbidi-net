# Observability and Diagnostics

WebDriverBiDi.NET provides comprehensive observability through `System.Diagnostics.Tracing.EventSource`, enabling you to monitor, troubleshoot, and optimize your WebDriver BiDi automation.

## Overview

The library emits structured diagnostic events that can be consumed by:

- **EventListener** - Custom in-process listeners for real-time monitoring
- **ETW (Event Tracing for Windows)** - Windows performance monitoring
- **EventPipe** - Cross-platform event collection and analysis
- **dotnet-trace** - CLI diagnostics tool for production troubleshooting
- **OpenTelemetry** - Distributed tracing and metrics collection
- **APM Tools** - Application Insights, Dynatrace, New Relic (via bridges)

**Key Benefits:**
- ✅ **Zero dependencies** - Uses only core .NET APIs
- ✅ **Low overhead** - Minimal performance impact when not actively listening
- ✅ **Production-ready** - Designed for high-throughput scenarios
- ✅ **Extensible** - Easy to integrate with existing logging infrastructure

## Quick Start

### Console Logging

The simplest way to see diagnostic events is using the included example `EventListener`:

```csharp
using System.Diagnostics.Tracing;
using WebDriverBiDi;

// Create a simple console event listener
using var listener = new ConsoleEventListener();

// Use WebDriverBiDi normally - events will be logged to console
await using var driver = new BiDiDriver();
await driver.StartAsync("ws://localhost:9222/session");
```

### Custom EventListener

Create a custom listener for more control:

```csharp
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

// Usage
using var listener = new MyEventListener();
```

## Event Levels

Choose the appropriate level based on your needs:

| Level | Use Case | Events Included |
|-------|----------|-----------------|
| **Critical** | Production alerts | Critical failures (none currently emitted) |
| **Error** | Error tracking | Command errors, protocol errors, connection errors |
| **Warning** | Operational monitoring | Command timeouts, event handler errors, unknown messages |
| **Informational** | General monitoring | Connection lifecycle, command completion, subscriptions |
| **Verbose** | Development/debugging | All events including command sending, event receipt, statistics |

**Recommendation for Production:** Use `EventLevel.Informational` or `EventLevel.Warning` to balance observability with overhead.

## Available Events

### Connection Lifecycle

| Event | Level | Description | Payload |
|-------|-------|-------------|---------|
| `ConnectionOpening` | Info | Connection is being established | `connectionId`, `url` |
| `ConnectionOpened` | Info | Connection successfully established | `connectionId`, `url` |
| `ConnectionClosing` | Info | Connection is being closed | `connectionId`, `reason` |
| `ConnectionClosed` | Info | Connection fully closed | `connectionId` |
| `ConnectionError` | Error | Connection error occurred | `connectionId`, `errorMessage` |

### Command Execution

| Event | Level | Description | Payload |
|-------|-------|-------------|---------|
| `CommandSending` | Verbose | Command being sent to remote end | `commandId`, `method` |
| `CommandCompleted` | Info | Command completed successfully | `commandId`, `method`, `elapsedMilliseconds` |
| `CommandTimeout` | Warning | Command timed out | `commandId`, `method`, `timeoutMilliseconds` |
| `CommandError` | Error | Command failed with error response | `commandId`, `method`, `errorType`, `errorMessage` |

### Event Handling

| Event | Level | Description | Payload |
|-------|-------|-------------|---------|
| `EventReceived` | Verbose | Protocol event received | `eventMethod` |
| `EventSubscribing` | Info | Subscribing to protocol events | `eventNames`, `contextCount` |
| `EventUnsubscribing` | Info | Unsubscribing from protocol events | `eventNames`, `contextCount` |
| `EventHandlerError` | Warning | User event handler threw exception | `eventMethod`, `errorMessage` |

### Protocol Processing

| Event | Level | Description | Payload |
|-------|-------|-------------|---------|
| `UnknownMessageReceived` | Warning | Unknown message from remote end | `messageType`, `messageLength` |
| `ProtocolError` | Error | Protocol parsing/processing error | `errorMessage`, `messageSnippet` |

### Transport & Statistics

| Event | Level | Description | Payload |
|-------|-------|-------------|---------|
| `TransportStarted` | Info | Transport message processing started | (none) |
| `TransportStopped` | Info | Transport message processing stopped | `reason` |
| `PendingCommandCount` | Verbose | Current pending command count | `pendingCount` |
| `MessageStatistics` | Verbose | Message statistics snapshot | `messagesSent`, `messagesReceived`, `eventsReceived`, `errorsReceived` |

### Module & Extensibility

| Event | Level | Description | Payload |
|-------|-------|-------------|---------|
| `CustomModuleRegistered` | Info | Custom module registered | `moduleName` |
| `CustomEventRegistered` | Info | Custom event type registered | `eventName`, `eventType` |

## Common Scenarios

### Performance Monitoring

Track command execution times to identify bottlenecks:

```csharp
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
```

### Error Tracking

Monitor errors for debugging and alerting:

```csharp
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
            string error = $"{eventData.EventName}: {string.Join(", ", eventData.Payload ?? [])}";
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
```

### Integration with Microsoft.Extensions.Logging

For `ILogger` integration, use the **WebDriverBiDi.Logging** package:

```bash
dotnet add package WebDriverBiDi.Logging
```

This package bridges WebDriverBiDi EventSource events to the standard .NET logging infrastructure, enabling integration with Application Insights, Serilog, and other logging providers.

#### Basic Console Application

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Tracing;
using WebDriverBiDi;

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
```

#### ASP.NET Core Web Application

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Tracing;

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
```

#### With Serilog Structured Logging

```csharp
using Serilog;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Tracing;
using WebDriverBiDi;

// Configure Serilog with structured logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}")
    .Enrich.FromLogContext()
    .CreateLogger();

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.AddSerilog();
    builder.AddWebDriverBiDi(EventLevel.Verbose); // Capture all events including verbose
});

var serviceProvider = services.BuildServiceProvider();

// Structured properties will be captured by Serilog
await using var driver = new BiDiDriver();
await driver.StartAsync("ws://localhost:9222");

// Serilog output includes structured properties:
// [12:34:56 INF] CommandCompleted {"EventId":7,"EventName":"CommandCompleted","commandId":"1","method":"session.status","elapsedMilliseconds":42}
```

#### With Application Insights

```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Tracing;
using WebDriverBiDi;

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
```

#### Filtering by Event Level

```csharp
services.AddLogging(builder =>
{
    builder.AddConsole();

    // Only capture Warning and Error events
    builder.AddWebDriverBiDi(EventLevel.Warning);

    // Or use ILogger filtering
    builder.AddFilter("WebDriverBiDi.Logging", LogLevel.Warning);
});
```

#### Configuration-based Setup

**appsettings.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "WebDriverBiDi.Logging": "Debug"
    },
    "Console": {
      "IncludeScopes": true
    }
  }
}
```

**Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Configuration is loaded from appsettings.json
builder.Logging.AddWebDriverBiDi(); // Respects configured log levels

var app = builder.Build();
```

#### Custom Event Processing

For scenarios requiring custom processing beyond standard logging:

```csharp
using System.Diagnostics.Tracing;
using Microsoft.Extensions.Logging;

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

// Register as singleton
services.AddSingleton(sp =>
{
    var logger = sp.GetRequiredService<ILogger<CustomWebDriverEventListener>>();
    return new CustomWebDriverEventListener(logger);
});
```

See the [WebDriverBiDi.Logging README](../src/WebDriverBiDi.Logging/README.md) for package details.

## CLI Tools

### Using dotnet-trace

Collect events from a running process without code changes:

```bash
# List running .NET processes
dotnet-trace ps

# Collect WebDriverBiDi events
dotnet-trace collect --process-id <pid> --providers WebDriverBiDi

# Collect with specific event level (5=Verbose, 4=Info, 3=Warning, 2=Error)
dotnet-trace collect --process-id <pid> --providers WebDriverBiDi:4

# Convert to other formats
dotnet-trace convert trace.nettrace --format speedscope
```

### Using PerfView (Windows)

For Windows-specific ETW collection:

```bash
# Collect ETW events
PerfView.exe /OnlyProviders=*WebDriverBiDi collect

# Stop collection
# (Press 's' in PerfView window)

# View events
# Open .etl file in PerfView and navigate to Events view
```

## OpenTelemetry Integration

For distributed tracing and metrics:

```csharp
// Requires: OpenTelemetry, OpenTelemetry.Exporter.Console

using OpenTelemetry;
using OpenTelemetry.Trace;

var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource("WebDriverBiDi")
    .AddConsoleExporter()
    .Build();

// Now use WebDriverBiDi - traces will be collected
```

## Best Practices

### Production Environments

1. **Use Informational or Warning Level**
   ```csharp
   EnableEvents(source, EventLevel.Informational);
   ```

2. **Process Events Quickly**
   - `OnEventWritten` is called synchronously
   - Avoid blocking operations
   - Queue events for async processing if needed

3. **Filter by Event Name**
   ```csharp
   if (eventData.EventName == "CommandError")
   {
       // Handle only command errors
   }
   ```

4. **Monitor Key Metrics**
   - Command latency (`CommandCompleted`)
   - Error rate (`CommandError`, `ProtocolError`)
   - Connection stability (`ConnectionError`)

### Development Environments

1. **Use Verbose Level**
   ```csharp
   EnableEvents(source, EventLevel.Verbose);
   ```

2. **Enable Detailed Payload Logging**
   - Log full payload for debugging
   - Use `eventData.PayloadNames` and `eventData.Payload`

3. **Combine with Existing Logging**
   - Bridge to your preferred logging framework
   - Maintain consistent log format

### Resource Management

Always dispose EventListener instances:

```csharp
using var listener = new MyEventListener();
// or
try
{
    var listener = new MyEventListener();
    // use listener
}
finally
{
    listener?.Dispose();
}
```

## Performance Considerations

- **Zero allocation when not enabled** - EventSource uses [NonEvent] for helper methods
- **Low overhead** - Minimal impact even with Verbose logging
- **ETW optimized** - On Windows, uses highly optimized ETW infrastructure
- **Async-friendly** - Events use `RunContinuationsAsynchronously` pattern

**Measured Overhead:**
- No listener: < 5ns per event
- Single listener at Info level: ~100ns per event
- Single listener at Verbose level: ~150ns per event

## Troubleshooting

### Events Not Appearing

1. Verify EventSource is enabled:
   ```csharp
   if (WebDriverBiDiEventSource.Log.IsEnabled())
   {
       Console.WriteLine("EventSource is enabled");
   }
   ```

2. Check event level:
   ```csharp
   if (WebDriverBiDiEventSource.Log.IsEnabled(EventLevel.Verbose, EventKeywords.None))
   {
       Console.WriteLine("Verbose events are enabled");
   }
   ```

3. Ensure listener is created before WebDriverBiDi use:
   ```csharp
   // CORRECT: Listener created first
   using var listener = new MyEventListener();
   using var driver = new BiDiDriver();

   // INCORRECT: Listener created after driver
   using var driver = new BiDiDriver();
   using var listener = new MyEventListener(); // May miss early events
   ```

### High Event Volume

If experiencing performance issues:

1. **Increase event level** (Info instead of Verbose)
2. **Filter specific events** in `OnEventWritten`
3. **Async processing** - Queue events for background processing
4. **Sampling** - Process only 1 in N events for high-frequency events

## See Also

- [Getting Started](getting-started.md) - Basic library usage
- [Common Pitfalls](common-pitfalls.md) - Avoiding common mistakes
- [Architecture](architecture.md) - Understanding library structure
- [Microsoft EventSource Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.tracing.eventsource)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
