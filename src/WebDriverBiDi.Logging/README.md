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
builder.AddLogging(b => b.AddWebDriverBiDi());
```

### Custom Event Level

Specify a minimum event level to capture:

```csharp
using System.Diagnostics.Tracing;

builder.AddLogging(b => b.AddWebDriverBiDi(EventLevel.Verbose)); // Capture all events
```

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
// Example log output with structured properties:
// Information: CommandCompleted, commandId=1, method=session.status, elapsedMilliseconds=42
```

Structured logging providers (Application Insights, Serilog, etc.) can capture these properties for powerful diagnostics.

## Filtering

Use standard ILogger filtering to control which events are logged:

```csharp
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddWebDriverBiDi();
    builder.AddFilter("WebDriverBiDi.Logging", LogLevel.Information); // Only Info and above
});
```

Or use configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "WebDriverBiDi.Logging": "Debug"
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
using Microsoft.Extensions.Logging;

using ILoggerFactory factory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.AddWebDriverBiDi(EventLevel.Verbose);
});
```

For comprehensive examples including Serilog, Application Insights, custom filtering, and performance monitoring, see the [Observability Documentation](../../docs/observability.md#integration-with-microsoftextensionslogging).

## Available Events

WebDriverBiDi emits events for:
- **Connection lifecycle**: Opening, Opened, Closing, Closed, Error
- **Command execution**: Sending, Completed, Timeout, Error
- **Event handling**: EventReceived, EventHandlerError
- **Protocol processing**: UnknownMessage, ProtocolError
- **Transport lifecycle**: Started, Stopped

See the [observability documentation](../../docs/observability.md) for complete event reference.

## Performance

The EventSource bridge has minimal overhead:
- Events are only processed when logging is enabled
- Structured properties are created on-demand
- No allocations when logging is disabled
- Thread-safe and async-friendly

## See Also

- [WebDriverBiDi Package](https://www.nuget.org/packages/WebDriverBiDi)
- [Observability Documentation](../../docs/observability.md)
- [Microsoft.Extensions.Logging Documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging)
