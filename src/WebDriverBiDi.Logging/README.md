# WebDriverBiDi.Logging

Microsoft.Extensions.Logging integration for the WebDriver BiDi .NET client library.

## Overview

This package provides `ILogger` support for WebDriver BiDi diagnostic events, enabling you to capture WebDriver BiDi EventSource events through the standard .NET logging infrastructure.

## Installation

```bash
dotnet add package WebDriverBiDi.Logging
```

## Quick Start

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using WebDriverBiDi;

// Configure logging with WebDriverBiDi events
var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddWebDriverBiDi(); // Add WebDriverBiDi event logging
});

var serviceProvider = services.BuildServiceProvider();

// Use WebDriverBiDi normally - events will be logged
await using var driver = new BiDiDriver();
await driver.StartAsync("ws://localhost:9222");
```

## Configuration

### Default Configuration

By default, events at `EventLevel.Informational` and above are captured:

```csharp
services.AddLogging(builder => builder.AddWebDriverBiDi());
```

### Custom Event Level

Specify a minimum event level to capture:

```csharp
using System.Diagnostics.Tracing;

services.AddLogging(builder => builder.AddWebDriverBiDi(EventLevel.Verbose)); // Capture all events
```

Call `AddWebDriverBiDi` once. The listener is registered with `TryAddSingleton`, so the first call on a
given service collection wins; a later call is silently ignored, and the level from the first call remains
in force. Pass the level you want on that first call.

## Event Level Mapping

WebDriver BiDi EventSource levels are mapped to ILogger levels as follows:

| EventSource Level | ILogger Level |
|-------------------|---------------|
| `Verbose` | `Debug` |
| `Informational` | `Information` |
| `Warning` | `Warning` |
| `Error` | `Error` |
| `Critical` | `Critical` |

## Structured Logging

Events are logged with structured properties, enabling rich filtering and querying:

```csharp
// A CommandCompleted event is logged with:
//   message:          Command 1 (session.status) completed in 42ms
//   {OriginalFormat}: Command {commandId} ({method}) completed in {elapsedMilliseconds}ms
//   properties:       EventId=7, EventName=CommandCompleted, EventSource=WebDriverBiDi,
//                     commandId=1, method=session.status, elapsedMilliseconds=42
```

Structured logging providers (Application Insights, Serilog, etc.) can capture these properties for powerful diagnostics.

## Filtering

Use standard ILogger filtering to control which events are logged:

```csharp
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddWebDriverBiDi();
    builder.AddFilter("WebDriverBiDi.Logging.WebDriverBiDiEventSourceLogger", LogLevel.Information); // Only Info and above
});
```

Or use configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "WebDriverBiDi.Logging.WebDriverBiDiEventSourceLogger": "Debug"
    }
  }
}
```

## Usage Examples

### ASP.NET Core

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddWebDriverBiDi();
var app = builder.Build();
```

### Console Application

```csharp
using System.Diagnostics.Tracing;
using Microsoft.Extensions.Logging;

using ILoggerFactory factory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.AddWebDriverBiDi(EventLevel.Verbose);
});
```

For comprehensive examples including Serilog, Application Insights, custom filtering, and performance monitoring, see the [Observability Documentation](https://github.com/webdriverbidi-net/webdriverbidi-net/blob/main/docs/articles/advanced/observability.md#integration-with-microsoftextensionslogging).

## Available Events

WebDriverBiDi emits 22 events:
- **Connection lifecycle**: `ConnectionOpening`, `ConnectionOpened`, `ConnectionClosing`, `ConnectionClosed`, `ConnectionError`
- **Command execution**: `CommandSending`, `CommandSendFailed`, `CommandCompleted`, `CommandTimeout`, `CommandError`, `CanceledCommandResponseDiscarded`
- **Event handling**: `EventReceived`, `EventHandlerError`, `AsyncHandlerTaskCount`
- **Protocol processing**: `UnknownMessageReceived`, `ProtocolError`
- **Transport lifecycle**: `TransportStarted`, `TransportStopped`
- **Registration**: `CustomModuleRegistered`, `CustomEventRegistered`
- **Counters**: `PendingCommandCount`, `MessageStatistics`

See the [observability documentation](https://github.com/webdriverbidi-net/webdriverbidi-net/blob/main/docs/articles/advanced/observability.md) for complete event reference.

## Native AOT

When publishing with `PublishAot`, the ILCompiler sets `EventSourceSupport` to `false` by default. That
makes `EventSource.IsEnabled()` permanently false, so the library raises no diagnostic events and this
bridge forwards nothing to `ILogger`. Nothing throws and nothing is written to explain the silence. Opt
back in from your project file:

```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <EventSourceSupport>true</EventSourceSupport>
</PropertyGroup>
```

This affects Native AOT publishing only; a normal build, including a trimmed one, is unaffected.

## Performance

The EventSource bridge has minimal overhead:
- Events are only processed when logging is enabled
- Structured properties are created on-demand
- The bridge allocates nothing for an event the target logger has filtered off; the EventSource's own
  per-event cost still applies at the subscribed level, because the listener is subscribed at that
  level and the `ILogger.IsEnabled` check runs after the event has been raised
- Thread-safe and async-friendly

## See Also

- [WebDriverBiDi Package](https://www.nuget.org/packages/WebDriverBiDi)
- [Observability Documentation](https://github.com/webdriverbidi-net/webdriverbidi-net/blob/main/docs/articles/advanced/observability.md)
- [Microsoft.Extensions.Logging Documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging)
