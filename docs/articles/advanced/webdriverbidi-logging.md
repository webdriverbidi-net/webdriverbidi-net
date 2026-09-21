# WebDriverBiDi.Logging Package

Microsoft.Extensions.Logging integration for the WebDriver BiDi .NET client library.

## Overview

This package provides `ILogger` support for WebDriver BiDi diagnostic events by bridging the
library's `EventSource` (named `"WebDriverBiDi"`) to the standard .NET logging infrastructure via
`WebDriverBiDiEventSourceLogger`, an `EventListener` that forwards events to an `ILogger` instance.

For the full catalogue of available events, payload properties, and usage of the underlying
`EventSource` directly (without this package), see [Observability and Diagnostics](observability.md).

### What the Bridge Does Not Forward

The bridge forwards `WebDriverBiDiEventSource` events only. The driver's
[`OnLogMessage`](../events-observables.md#onlogmessage) event is a separate channel. It carries the
library's own log messages: connection and transport lifecycle at `Info`, a message per command at
`Debug`, and every message exchanged with the remote end at `Trace`. Those messages are filtered by
`BiDiDriver.TransportConfiguration.LogLevel` (see [Log Level](connection-management.md#log-level)), not
by the `EventLevel` passed to `AddWebDriverBiDi`, and nothing in this package sends them to an `ILogger`.
To route them there as well, observe the event yourself:

[!code-csharp[Forward Log Messages to ILogger](../../code/advanced/ObservabilitySamples.cs#ForwardLogMessagesToILogger)]

## Installation

```bash
dotnet add package WebDriverBiDi.Logging
```

## Quick Start

Register the bridge with `AddWebDriverBiDi()` on `ILoggingBuilder`:

[!code-csharp[Logging Quick Start](../../code/advanced/ObservabilitySamples.cs#LoggingQuickStart)]

The default overload captures events at `EventLevel.Informational` and above. Resolving `ILoggerFactory`
is what starts the bridge; see [How the Bridge Works](#how-the-bridge-works).

## Controlling the Minimum Log Level

Pass a `System.Diagnostics.Tracing.EventLevel` to capture more or fewer events:

<!-- inline-csharp: two calls on a logging builder the surrounding registration supplies -->
```csharp
builder.AddWebDriverBiDi(EventLevel.Verbose);   // all events, including debug-level
builder.AddWebDriverBiDi(EventLevel.Warning);   // warnings and errors only
```

The `EventLevel` → `LogLevel` mapping applied by `WebDriverBiDiEventSourceLogger` is:

| `EventLevel` | `LogLevel` |
|---|---|
| `LogAlways` | `Information` |
| `Verbose` | `Debug` |
| `Informational` | `Information` |
| `Warning` | `Warning` |
| `Error` | `Error` |
| `Critical` | `Critical` |
| anything else | `Trace` |

As the `minimumLevel` argument, `LogAlways` means "capture every event whatever its level"; as an event's
own level it maps to `Information`, because an event that must always be logged is not an error.

**Call `AddWebDriverBiDi` once.** The listener is registered with `TryAddSingleton`, so the first call on
a given service collection is the one that takes effect. A later call is silently ignored, including the
overload that takes a level, and the level from the first call remains in force. Pass the level you want
on the first call rather than adding a second one to change it.

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
| `{OriginalFormat}` | `string` | The event's message template, with each hole named after its payload property (e.g., `[{connectionId}/{sessionId}] Command {commandId} ({method}) completed in {elapsedMilliseconds}ms`). Omitted for an event that declares no template |

The payload properties vary by event. Every event except `AsyncHandlerTaskCount` begins with
`connectionId` and `sessionId`, which name the driver and the session the event belongs to; filter or group
on them to separate two drivers running in one process. Common examples:

| Event | Payload Properties |
|---|---|
| `ConnectionOpening` / `ConnectionOpened` | `connectionId`, `sessionId`, `url` |
| `CommandSending` | `connectionId`, `sessionId`, `commandId`, `method` |
| `CommandCompleted` | `connectionId`, `sessionId`, `commandId`, `method`, `elapsedMilliseconds` |
| `CommandError` | `connectionId`, `sessionId`, `commandId`, `method`, `errorCode`, `errorType`, `errorMessage` |
| `CommandTimeout` | `connectionId`, `sessionId`, `commandId`, `method`, `timeoutMilliseconds` |
| `EventReceived` | `connectionId`, `sessionId`, `eventMethod` |
| `EventHandlerError` | `connectionId`, `sessionId`, `eventMethod`, `errorMessage` |
| `ProtocolError` | `connectionId`, `sessionId`, `errorMessage`, `messageSnippet` |

See [Observability and Diagnostics — Available Events](observability.md#available-events) for the
complete list of events and their payloads.

## How the Bridge Works

The listener subscribes to the event source when the logging pipeline is built. A generic or web host
builds it at startup. A `ServiceProvider` you build yourself builds it the first time `ILoggerFactory`,
or an `ILogger`, is resolved from it. Until then no event is forwarded, so a provider that is built and
never resolved from logs nothing. Disposing the provider unsubscribes the listener.

Activation rides on an `ILoggerProvider`: `AddWebDriverBiDi` registers one whose only job is to take the
listener as a dependency, so building the pipeline constructs the listener. Two shapes therefore start
nothing, silently:

- **`ClearProviders()` after `AddWebDriverBiDi()`.** `ClearProviders` removes every registered
  `ILoggerProvider`, including the one that activates the bridge. Call `AddWebDriverBiDi()` *after* any
  `ClearProviders()`.
- **A replaced `ILoggerFactory`.** Only `Microsoft.Extensions.Logging.LoggerFactory` resolves
  `IEnumerable<ILoggerProvider>`. A factory registration that replaces it — Serilog's `UseSerilog()` or
  `AddSerilog()` with the default `writeToProviders: false`, for instance — never constructs the
  providers, so the bridge never subscribes. Resolve it yourself once at startup instead:

<!-- inline-csharp: one line of a startup sequence, resolving the listener the container already holds -->
```csharp
_ = serviceProvider.GetRequiredService<WebDriverBiDiEventSourceLogger>();
```

Neither case reports an error; nothing is forwarded.

`WebDriverBiDiEventSourceLogger` extends `System.Diagnostics.Tracing.EventListener`. When
registered via `AddWebDriverBiDi()`:

1. .NET calls `OnEventSourceCreated` for every active `EventSource`. The bridge enables the
   `"WebDriverBiDi"` source at the requested `EventLevel`.
2. For each event, `OnEventWritten` is called synchronously on the thread that fired the event.
   The bridge maps the `EventLevel` to a `LogLevel`, collects all payload name/value pairs as
   structured log state, and calls `ILogger.Log`.
3. The log message is rendered from the message template the `EventSource` declares for the event: for
   example, `Command 1 (session.status) completed in 42ms` for `CommandCompleted`. The template's holes
   are positional (`{0}`), so the bridge renames each after the payload property it refers to and adds
   the result to the state as `{OriginalFormat}`, the key that template-aware providers such as Serilog
   read. An event that declares no template is logged as `EventName, key1=value1, key2=value2, …`.

Because `OnEventWritten` is synchronous, avoid blocking operations inside logging providers
attached to this bridge. Queue events for asynchronous processing if the provider is slow.

## See Also

- [Observability and Diagnostics](observability.md) — EventSource usage, CLI tools, OpenTelemetry, best practices
- [WebDriverBiDi NuGet package](https://www.nuget.org/packages/WebDriverBiDi)
- [WebDriverBiDi.Logging NuGet package](https://www.nuget.org/packages/WebDriverBiDi.Logging)
