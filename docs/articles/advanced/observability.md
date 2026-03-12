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

The simplest way to see diagnostic events is using a console event listener:

[!code-csharp[Console Logging](../../code/advanced/ObservabilitySamples.cs#ConsoleLogging)]

### Custom EventListener

Create a custom listener for more control:

[!code-csharp[Custom EventListener](../../code/advanced/ObservabilitySamples.cs#CustomEventListener)]

[!code-csharp[Custom EventListener Usage](../../code/advanced/ObservabilitySamples.cs#CustomEventListenerUsage)]

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

[!code-csharp[Performance Monitor](../../code/advanced/ObservabilitySamples.cs#PerformanceMonitor)]

### Error Tracking

Monitor errors for debugging and alerting:

[!code-csharp[Error Tracker](../../code/advanced/ObservabilitySamples.cs#ErrorTracker)]

### Integration with Microsoft.Extensions.Logging

For `ILogger` integration, use the **WebDriverBiDi.Logging** NuGet package:

```bash
dotnet add package WebDriverBiDi.Logging
```

This package bridges WebDriverBiDi EventSource events to the standard .NET logging infrastructure, enabling integration with Application Insights, Serilog, and other logging providers. The `AddWebDriverBiDi()` extension method is available on `ILoggingBuilder` (in the `Microsoft.Extensions.Logging` namespace) once the package is referenced.

#### Basic Console Application

[!code-csharp[Basic Console Application](../../code/advanced/ObservabilitySamples.cs#BasicConsoleApplication)]

Logs will show connection lifecycle and command completion events (e.g., `ConnectionOpening`, `ConnectionOpened`, `CommandSending`, `CommandCompleted`).

#### ASP.NET Core Web Application

[!code-csharp[ASP.NET Core Web Application](../../code/advanced/ObservabilitySamples.cs#ASPNETCoreWebApplication)]

Add your services (e.g., `IBrowserAutomationService`) and endpoints as needed. WebDriverBiDi events will be logged through the ASP.NET Core logging infrastructure.

#### With Serilog Structured Logging

[!code-csharp[Serilog Structured Logging](../../code/advanced/SerilogObservabilitySamples.cs#SerilogStructuredLogging)]

Serilog output includes structured properties (e.g., `commandId`, `method`, `elapsedMilliseconds`).

#### With Application Insights

[!code-csharp[Application Insights](../../code/advanced/ObservabilitySamples.cs#ApplicationInsights)]

WebDriverBiDi events will be sent to Application Insights with structured properties for querying and analysis.

#### Filtering by Event Level

[!code-csharp[Filter by Event Level](../../code/advanced/ObservabilitySamples.cs#FilterbyEventLevel)]

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

[!code-csharp[Configuration-based Setup](../../code/advanced/ObservabilitySamples.cs#Configuration-basedSetup)]

#### Custom Event Processing

For scenarios requiring custom processing beyond standard logging:

[!code-csharp[Custom EventListener with ILogger](../../code/advanced/ObservabilitySamples.cs#CustomEventListenerwithILogger)]

[!code-csharp[Register Custom EventListener](../../code/advanced/ObservabilitySamples.cs#RegisterCustomEventListener)]

See the [WebDriverBiDi.Logging package](webdriverbidi-logging.md) for details.

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

[!code-csharp[OpenTelemetry Integration](../../code/advanced/ObservabilitySamples.cs#OpenTelemetryIntegration)]

Requires: `OpenTelemetry`, `OpenTelemetry.Trace`, `OpenTelemetry.Exporter.Console`.

## Best Practices

### Production Environments

1. **Use Informational or Warning Level**
   See `ObservabilityMyEventListener` and `ObservabilityPerformanceMonitor` in the samples—they use `EnableEvents(source, EventLevel.Informational)` in `OnEventSourceCreated`.

2. **Process Events Quickly**
   - `OnEventWritten` is called synchronously
   - Avoid blocking operations
   - Queue events for async processing if needed

3. **Filter by Event Name**

   [!code-csharp[Filter by Event Name](../../code/advanced/ObservabilitySamples.cs#FilterbyEventName)]

4. **Monitor Key Metrics**
   - Command latency (`CommandCompleted`)
   - Error rate (`CommandError`, `ProtocolError`)
   - Connection stability (`ConnectionError`)

### Development Environments

1. **Use Verbose Level**
   Use `EventLevel.Verbose` in `EnableEvents(source, EventLevel.Verbose)` within your EventListener's `OnEventSourceCreated`.

2. **Enable Detailed Payload Logging**
   - Log full payload for debugging
   - Use `eventData.PayloadNames` and `eventData.Payload`

3. **Combine with Existing Logging**
   - Bridge to your preferred logging framework
   - Maintain consistent log format

### Resource Management

Always dispose EventListener instances:

[!code-csharp[Resource Management](../../code/advanced/ObservabilitySamples.cs#ResourceManagement)]

[!code-csharp[Resource Management Try Finally](../../code/advanced/ObservabilitySamples.cs#ResourceManagementTryFinally)]

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

   [!code-csharp[Verify EventSource Enabled](../../code/advanced/ObservabilitySamples.cs#VerifyEventSourceEnabled)]

2. Check event level:

   [!code-csharp[Check Verbose Enabled](../../code/advanced/ObservabilitySamples.cs#CheckVerboseEnabled)]

3. Ensure listener is created before WebDriverBiDi use:

   [!code-csharp[Listener Before Driver](../../code/advanced/ObservabilitySamples.cs#ListenerBeforeDriver)]

### High Event Volume

If experiencing performance issues:

1. **Increase event level** (Info instead of Verbose)
2. **Filter specific events** in `OnEventWritten`
3. **Async processing** - Queue events for background processing
4. **Sampling** - Process only 1 in N events for high-frequency events

## See Also

- [Getting Started](../getting-started.md) - Basic library usage
- [Common Pitfalls](../common-pitfalls.md) - Avoiding common mistakes
- [Architecture](../architecture.md) - Understanding library structure
- [Microsoft EventSource Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.tracing.eventsource)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
