# WebDriverBiDi.Logging Package

Microsoft.Extensions.Logging integration for the WebDriver BiDi .NET client library.

## Overview

This package provides `ILogger` support for WebDriver BiDi diagnostic events by bridging the
library's `EventSource` (named `"WebDriverBiDi"`) to the standard .NET logging infrastructure via
`WebDriverBiDiEventSourceLogger`, an `EventListener` that forwards events to an `ILogger` instance.

For the full catalogue of available events, payload properties, and usage of the underlying
`EventSource` directly (without this package), see [Observability and Diagnostics](observability.md).

## Installation

```bash
dotnet add package WebDriverBiDi.Logging
```

## Quick Start

Register the bridge with `AddWebDriverBiDi()` on `ILoggingBuilder`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddWebDriverBiDi();
});

var serviceProvider = services.BuildServiceProvider();

await using var driver = new BiDiDriver();
await driver.StartAsync("ws://localhost:9222");
```

The default overload captures events at `EventLevel.Informational` and above.

## Controlling the Minimum Log Level

Pass a `System.Diagnostics.Tracing.EventLevel` to capture more or fewer events:

```csharp
builder.AddWebDriverBiDi(EventLevel.Verbose);   // all events, including debug-level
builder.AddWebDriverBiDi(EventLevel.Warning);   // warnings and errors only
```

The `EventLevel` → `LogLevel` mapping applied by `WebDriverBiDiEventSourceLogger` is:

| `EventLevel` | `LogLevel` |
|---|---|
| `Verbose` | `Debug` |
| `Informational` | `Information` |
| `Warning` | `Warning` |
| `Error` | `Error` |
| `Critical` | `Critical` |

## Log Category Name

All events are logged under the category `WebDriverBiDi.Logging.WebDriverBiDiEventSourceLogger`.
Use this name in `appsettings.json` or filtering rules to control the minimum level independently
of other categories:

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

## Structured Properties

Each log entry carries the following structured properties in addition to the formatted message
string:

| Property | Type | Description |
|---|---|---|
| `EventId` | `int` | Numeric EventSource event ID |
| `EventName` | `string` | EventSource event name (e.g., `CommandCompleted`) |
| `EventSource` | `string` | Always `"WebDriverBiDi"` |
| *(event payload fields)* | varies | All payload properties from the EventSource event (see below) |

The payload properties vary by event. Common examples:

| Event | Payload Properties |
|---|---|
| `ConnectionOpening` / `ConnectionOpened` | `connectionId`, `url` |
| `CommandSending` | `commandId`, `method` |
| `CommandCompleted` | `commandId`, `method`, `elapsedMilliseconds` |
| `CommandError` | `commandId`, `method`, `errorType`, `errorMessage` |
| `CommandTimeout` | `commandId`, `method`, `timeoutMilliseconds` |
| `EventReceived` | `eventMethod` |
| `EventHandlerError` | `eventMethod`, `errorMessage` |
| `ProtocolError` | `errorMessage`, `messageSnippet` |

See [Observability and Diagnostics — Available Events](observability.md#available-events) for the
complete list of events and their payloads.

## How the Bridge Works

`WebDriverBiDiEventSourceLogger` extends `System.Diagnostics.Tracing.EventListener`. When
registered via `AddWebDriverBiDi()`:

1. .NET calls `OnEventSourceCreated` for every active `EventSource`. The bridge enables the
   `"WebDriverBiDi"` source at the requested `EventLevel`.
2. For each event, `OnEventWritten` is called synchronously on the thread that fired the event.
   The bridge maps the `EventLevel` to a `LogLevel`, collects all payload name/value pairs as
   structured log state, and calls `ILogger.Log`.
3. The formatted log message is `EventName, key1=value1, key2=value2, …`.

Because `OnEventWritten` is synchronous, avoid blocking operations inside logging providers
attached to this bridge. Queue events for asynchronous processing if the provider is slow.

## See Also

- [Observability and Diagnostics](observability.md) — EventSource usage, CLI tools, OpenTelemetry, best practices
- [WebDriverBiDi NuGet package](https://www.nuget.org/packages/WebDriverBiDi)
- [WebDriverBiDi.Logging NuGet package](https://www.nuget.org/packages/WebDriverBiDi.Logging)
