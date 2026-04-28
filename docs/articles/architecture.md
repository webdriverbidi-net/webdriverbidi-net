# Architecture Overview

This document provides an architectural overview of WebDriverBiDi.NET, explaining how the library is organized and how data flows through the system.

## High-Level Architecture

```
┌─────────────────────────────────────────────────────┐
│              Your .NET Application                  │
└────────────────┬────────────────────────────────────┘
                 │
                 │ Uses
                 ▼
┌─────────────────────────────────────────────────────┐
│              BiDiDriver                             │
│  ┌─────────────────────────────────────────────┐    │
│  │           Module Layer                      │    │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐   │    │
│  │  │ Browser  │  │ Browsing │  │  Script  │   │    │
│  │  │  Module  │  │  Context │  │  Module  │   │    │
│  │  └──────────┘  │  Module  │  └──────────┘   │    │
│  │                └──────────┘                 │    │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐   │    │
│  │  │ Network  │  │  Input   │  │   Log    │   │    │
│  │  │  Module  │  │  Module  │  │  Module  │   │    │
│  │  └──────────┘  └──────────┘  └──────────┘   │    │
│  └─────────────────────────────────────────────┘    │
│                      │                              │
│  ┌─────────────────────────────────────────────┐    │
│  │           Protocol Layer                    │    │
│  │         ┌──────────┐                        │    │
│  │         │Transport │                        │    │
│  │         └────┬─────┘                        │    │
│  │              │                              │    │
│  │         ┌────▼─────────┐                    │    │
│  │         │  Connection  │  (Abstract)        │    │
│  │         └────┬─────────┘                    │    │
│  │              │                              │    │
│  │      ┌───────┴────────┐                     │    │
│  │      ▼                ▼                     │    │
│  │  ┌────────────┐  ┌──────────┐               │    │
│  │  │  WebSocket │  │  Pipes   │               │    │
│  │  │ Connection │  │Connection│               │    │
│  │  └─────┬──────┘  └────┬─────┘               │    │
│  └────────┼──────────────┼─────────────────────┘.   |
└───────────┼──────────────┼─────────────────--───────┘
            │              │
            ▼              ▼
       WebSocket        Pipes
   (JSON messages)  (Null-terminated)
            │              │
            ▼              ▼
┌─────────────────────────────────────────────────────┐
│              Browser (Remote End)                   │
│         Chrome / Edge / Firefox / etc.              │
└─────────────────────────────────────────────────────┘
```

## Core Components

### BiDiDriver

The `BiDiDriver` class is the facade that provides access to all functionality.

**Responsibilities:**
- Manages the WebSocket connection lifecycle
- Provides access to all modules
- Coordinates command execution
- Dispatches events to observers
- Exposes driver-level events

**Key Methods:**
- `StartAsync(url)`: Establishes WebSocket connection
- `StopAsync()`: Closes connection
- `ExecuteCommandAsync<T>(command)`: Sends commands and waits for responses
- `RegisterModule(module)`: Registers custom modules

### Transport Layer

The `Transport` class handles low-level communication with the browser through an abstract `Connection`.

**Responsibilities:**
- Manages connection lifecycle through the Connection abstraction
- Serializes commands to JSON
- Deserializes responses and events from JSON
- Correlates responses with sent commands
- Routes events to appropriate handlers
- Supports multiple transport types (WebSocket, Pipes)

**Message Flow:**

```
Commands (Your Code → Browser):
┌──────────────┐   Serialize     ┌───────────┐   WebSocket   ┌─────────┐
│ Command      │────────────────▶│  JSON     │──────────────▶│ Browser │
│ Parameters   │                 │  Message  │               └─────────┘
└──────────────┘                 └───────────┘

Responses (Browser → Your Code):
┌─────────┐   WebSocket   ┌───────────-┐   Deserialize    ┌──────────────┐
│ Browser │──────────────▶│  JSON      │─────────────────▶│ Command      │
└─────────┘               │  Message   │                  │ Result       │
                          └─────-──────┘                  └──────────────┘

Events (Browser → Observers):
┌─────────┐   WebSocket   ┌───────────┐   Deserialize    ┌──────────────┐
│ Browser │──────────────▶│  JSON     │─────────────────▶│ Event Args   │
└─────────┘               │  Message  │                  └──────┬───────┘
                          └───────────┘                         │
                                                                ▼
                                                     ┌────────────────────┐
                                                     │ Event Observers    │
                                                     └────────────────────┘
```

**Message Queue Architecture:**

The Transport uses an unbounded `Channel<byte[]>` to buffer incoming messages:

- **Design**: Single-reader, single-writer unbounded channel
- **Purpose**: Decouple connection I/O from message processing
- **Normal Behavior**: Queue stays near-empty as processing is typically faster than message arrival
- **High-Throughput**: Queue can grow if messages arrive faster than processing (see [Performance Considerations](advanced/performance.md#message-queue-and-high-throughput-scenarios))

```
Connection → [Unbounded Queue] → Message Processor → Event Dispatch
                    ↓
            No size limit
            Fast enqueue
            Sequential processing
```

**Key Characteristics:**
- No backpressure mechanism (unbounded)
- Optimal for typical usage patterns
- Requires attention in high-event scenarios (>1000 events/second)
- See XML documentation on `Transport` class for mitigation strategies

### Module Layer

Each module encapsulates a specific area of WebDriver BiDi functionality.

**Module Structure:**

See `BrowsingContextModule` in the WebDriverBiDi.BrowsingContext namespace. Each module has a constructor taking `IBiDiCommandExecutor`, command methods returning `Task<CommandResult>`, and observable events of type `ObservableEvent<TEventArgs>`.

**All Modules:**
- `BrowserModule`: Browser windows and user contexts
- `BrowsingContextModule`: Tab/window/iframe management
- `ScriptModule`: JavaScript execution
- `NetworkModule`: Network traffic control
- `InputModule`: User input simulation
- `LogModule`: Console logs
- `SessionModule`: Session and subscription management
- `StorageModule`: Cookies and storage
- `EmulationModule`: Device emulation
- `PermissionsModule`: Permission control
- `BluetoothModule`: Web Bluetooth API
- `WebExtensionModule`: Extension management
- `SpeculationModule`: Navigation prefetching
- `UserAgentClientHintsModule`: User agent client hints emulation

## Connection Types

WebDriverBiDi.NET uses an abstract `Connection` class to support multiple transport layers, allowing communication with browsers through different mechanisms.

### Connection Architecture

The `Connection` abstract class defines the contract for all transport implementations. See the `Connection` class in the WebDriverBiDi.Protocol namespace for the full API, including `IsActive`, `ConnectionType`, `StartAsync`, `StopAsync`, `SendDataAsync`, observable events (`OnDataReceived`, `OnConnectionError`, `OnLogMessage`), and configurable timeouts (`StartupTimeout`, `ShutdownTimeout`, `DataTimeout`).

### WebSocket Connection

**When to Use:**
- Development and debugging scenarios
- Remote browser control
- Multiple clients connecting to the same browser
- Browser launched separately from your application
- Cross-machine communication

**Implementation Details:**
- Uses `System.Net.WebSockets.ClientWebSocket`
- Validates URL scheme (must be `ws://` or `wss://`)
- Supports secure WebSocket connections
- Text-based JSON message protocol
- Handles multi-frame WebSocket messages

**Example:**

[!code-csharp[WebSocket Example](../code/architecture/ArchitectureSamples.cs#WebSocketExample)]

**Browser Launch:**
```bash
chrome --remote-debugging-port=9222
```

The browser provides the WebSocket URL in its output or via the `/json/version` endpoint.

### Pipe Connection

**When to Use:**
- Automation frameworks controlling browser lifecycle
- Programmatic browser management
- Single-client scenarios
- Enhanced process isolation
- Lower latency requirements

**Implementation Details:**
- Uses anonymous pipes (`AnonymousPipeServerStream`)
- Protocol: Null-terminated JSON messages
- Windows: Anonymous pipes via `CreatePipe` API
- Unix/Linux/macOS: File descriptors 3 (browser reads) and 4 (browser writes)
- Requires `IPipeServerProcessProvider` for process management
- Direct process-to-process communication

**Example:**

> **Note:** WebDriverBiDi.NET does not ship a browser launcher. Implement `IPipeServerProcessProvider` yourself to launch the browser and provide a `Transport`:

[!code-csharp[Pipe Example](../code/architecture/ArchitectureSamples.cs#PipeExample)]

**Browser Launch:**
```bash
chrome --remote-debugging-pipe
```

The `--remote-debugging-pipe` flag instructs the browser to communicate via stdin/stdout/file descriptors instead of opening a TCP port.

### IPipeServerProcessProvider Interface

The `IPipeServerProcessProvider` interface enables dependency injection for pipe connections. See the interface in the WebDriverBiDi.Protocol namespace—it defines `Process? PipeServerProcess { get; }`. This allows `PipeConnection` to access the browser process and its stdin/stdout handles. You must implement this interface to manage the browser process lifecycle and provide it to the connection.

### ConnectionType Enum

The `ConnectionType` enum identifies which transport mechanism is being used (`WebSocket` or `Pipes`). See the enum in the WebDriverBiDi.Protocol namespace. Launchers use this to determine which browser flags to use (`--remote-debugging-port` vs `--remote-debugging-pipe`).

### Choosing a Connection Type

| Factor | WebSocket | Pipes |
|--------|-----------|-------|
| **Latency** | Moderate (TCP overhead) | Lower (direct IPC) |
| **Remote Access** | ✓ Yes | ✗ No (same machine only) |
| **Multi-Client** | ✓ Yes | ✗ No (single client) |
| **Process Coupling** | Loose | Tight |
| **Debugging** | Easy (can inspect traffic) | Moderate |
| **Platform Support** | Universal | Universal |
| **Setup Complexity** | Simple | Moderate |

**Recommendation:**
- Use **WebSocket** for development, debugging, and flexible deployment scenarios
- Use **Pipes** for automation frameworks and when you control the browser lifecycle

### Command Pattern

Commands follow a strict pattern for type safety and consistency.

**Command Flow:**

```
1. Create Parameters Object
   ↓
2. Pass to Module Method
   ↓
3. Module Creates Command
   ↓
4. Driver Executes via Transport
   ↓
5. Transport Sends WebSocket Message
   ↓
6. Browser Processes Command
   ↓
7. Browser Sends Response
   ↓
8. Transport Receives Message
   ↓
9. Transport Deserializes to Result
   ↓
10. Driver Returns Result to Module
   ↓
11. Module Returns to Your Code
```

**Example:**

[!code-csharp[Command Pattern](../code/architecture/ArchitectureSamples.cs#CommandPattern)]

### Event System

WebDriverBiDi.NET uses an observable event pattern for handling browser events.

**Event Architecture:**

```
Browser
  │
  │ Emits Event
  ▼
Transport
  │
  │ Deserializes
  ▼
ObservableEvent<TEventArgs>
  │
  │ Notifies
  ▼
┌────────────────────────--─-─┐
│  EventObserver<TEventArgs>  │
│  EventObserver<TEventArgs>  │
│  EventObserver<TEventArgs>  │
└─────────────────────────--─-┘
  │
  │ Invokes
  ▼
Your Event Handlers
```

**Observable Event:** See `ObservableEvent<T>` in the WebDriverBiDi namespace—it provides `AddObserver(Func<T, Task> handler, ObservableEventHandlerOptions handlerOptions = ObservableEventHandlerOptions.RunHandlerSynchronously, string description = "")` and `NotifyObserversAsync(T eventArgs)`.

**Event Observer:** See `EventObserver<T>` in the WebDriverBiDi namespace—it provides `StartCapturingTasks`, `StopCapturingTasks`, `WaitForCapturedTasksAsync`, `WaitForCapturedTasksCompleteAsync`, `GetCapturedTasks`, and `Unobserve`.

## Data Flow Patterns

### Command Execution

[!code-csharp[Command Execution Flow](../code/architecture/ArchitectureSamples.cs#CommandExecutionFlow)]

### Event Handling

[!code-csharp[Event Handling](../code/architecture/ArchitectureSamples.cs#EventHandling)]

### Bidirectional Communication

WebDriver BiDi is truly bidirectional:

```
Your Code ────Commands────▶ Browser
          ◀───Responses────

Your Code ◀────Events────── Browser
          ───Subscribe────▶
```

## Serialization

WebDriverBiDi.NET uses `System.Text.Json` for JSON serialization.

### Custom JSON Converters

The library includes specialized converters for WebDriver BiDi types:

- `CommandJsonConverter`: Serializes command parameters
- `SentinelNullJsonConverter<T, TSentinelChecker>`: Serializes types where a specific, "sentinel" value yields a `null` value in the serialized JSON
- `DiscriminatedUnionJsonConverter<T>`: Deserializes command result values with discriminated union types
- `BigIntegerJsonConverter`: Deserializes BigInteger values
- `NumberJsonConverter`: Deserializes JavaScript numeric values
- `RemoteValueDictionaryJsonConverter`: Deserializes RemoteValues for types containing dictionary types (maps, objects, etc.)
- `RemoteValueListJsonConverter`: Deserializes RemoteValues for list types (arrays, sets, etc.)

### Extension Data

Command results and event args capture unknown properties via `[JsonExtensionData]` and expose them as `AdditionalData`. See the `CommandResult` base class in the WebDriverBiDi namespace. This allows forward compatibility with new protocol versions.

## Threading Model

WebDriverBiDi.NET is fully asynchronous and thread-safe for most operations.

### Transport Thread

The transport maintains a dedicated thread for WebSocket message processing:

- **Receives messages** from WebSocket
- **Deserializes JSON** to objects
- **Dispatches events** to observers
- **Completes command Tasks** when responses arrive

### Your Thread(s)

Your application code runs on your own threads:

- **Sends commands** via async methods
- **Awaits results** (doesn't block threads)
- **Receives event notifications** on Transport thread or Task pool

### Event Handler Execution

Event handlers have two modes:

**Synchronous Mode (default):**

[!code-csharp[Sync Event Handler](../code/architecture/ArchitectureSamples.cs#SyncEventHandler)]

**Asynchronous Mode:**

[!code-csharp[Async Event Handler](../code/architecture/ArchitectureSamples.cs#AsyncEventHandler)]

## Error Handling Configuration

WebDriverBiDi.NET provides configurable error handling behavior at multiple levels of the system.

### Transport Error Behavior

The `Transport` class can be configured with different error behaviors. See `TransportErrorBehavior` enum in the WebDriverBiDi.Protocol namespace: `Ignore` (default), `Collect`, and `Terminate`.

### Terminate Mode

Throws an exception when the next command is sent after a transport error:

[!code-csharp[Terminate Mode](../code/architecture/ArchitectureSamples.cs#TerminateMode)]

**Use When:**
- You want fast failure on errors
- Errors indicate unrecoverable conditions
- You prefer explicit error handling

### Collect Mode

Stores transport errors in a list for later inspection:

[!code-csharp[Collect Mode](../code/architecture/ArchitectureSamples.cs#CollectMode)]

**Use When:**
- You want to continue operation despite errors
- Collecting diagnostics for troubleshooting
- Errors might be transient or non-critical

### Ignore Mode

Silently discards transport errors without notification:

[!code-csharp[Ignore Mode](../code/architecture/ArchitectureSamples.cs#IgnoreMode)]

**Use When:**
- Operating in fire-and-forget mode
- Errors are expected and acceptable
- You have alternative error detection mechanisms

**Warning:** Use this mode cautiously—it can mask real problems.

### Connection-Level Error Handling

Connections provide observable events for error monitoring:

[!code-csharp[Connection-Level Error Handling](../code/architecture/ArchitectureSamples.cs#Connection-LevelErrorHandling)]

### Event Handler Error Behavior

Event handlers can also throw exceptions. Control this with `ObservableEventHandlerOptions`:

[!code-csharp[Event Handler Error Behavior](../code/architecture/ArchitectureSamples.cs#EventHandlerErrorBehavior)]

See the [Error Handling Guide](advanced/error-handling.md) for comprehensive error management strategies.

### Granular Error Control

Beyond the transport-level error behavior, `BiDiDriver` exposes four properties for fine-grained control over different error scenarios. All use `TransportErrorBehavior` (Ignore/Collect/Terminate) and default to `Ignore`.

**Important:** With `Terminate` mode, exceptions don't propagate immediately when they occur. Due to the asynchronous nature of the library, termination errors are thrown when the next command is sent by the driver, not when the error is first encountered on the message processing thread.

#### EventHandlerExceptionBehavior

Controls how exceptions thrown by your event handlers are handled:

[!code-csharp[EventHandlerExceptionBehavior](../code/architecture/ArchitectureSamples.cs#EventHandlerExceptionBehavior)]

**When to Use Each Mode:**
- **Ignore** (default): Event handler exceptions are logged but don't interrupt message processing. The same applies to exceptions from asynchronously run handlers when those tasks are not captured via a capture session. Use for non-critical handlers.
- **Collect**: Exceptions are stored and thrown when `StopAsync()` is called. Exceptions from asynchronously run handlers are collected the same way when those tasks are not captured via a capture session. Use when debugging event handler issues.
- **Terminate**: Driver terminates when next command is sent after exception. Exceptions from asynchronously run handlers also surface on the next command when those tasks are not captured via a capture session. Use when event handler failure indicates unrecoverable state.

If you explicitly capture async handler tasks with `WaitForCapturedTasksAsync()` or `WaitForCapturedTasksCompleteAsync()`, those task exceptions are instead owned by the caller and are not surfaced a second time through transport termination or collection.

#### ProtocolErrorBehavior

Controls how protocol errors are handled (invalid JSON, missing required properties, deserialization failures):

[!code-csharp[ProtocolErrorBehavior](../code/architecture/ArchitectureSamples.cs#ProtocolErrorBehavior)]

**When to Use Each Mode:**
- **Ignore** (default): Protocol errors are logged but processing continues. Use when working with experimental or unstable protocol versions.
- **Collect**: Errors are stored and thrown at shutdown. Use when troubleshooting browser compatibility issues.
- **Terminate**: Driver terminates when next command is sent after error. Use in production when protocol violations indicate serious issues.

#### UnknownMessageBehavior

Controls how unknown messages are handled (valid JSON that doesn't match any known protocol structure):

[!code-csharp[UnknownMessageBehavior](../code/architecture/ArchitectureSamples.cs#UnknownMessageBehavior)]

**When to Use Each Mode:**
- **Ignore** (default): Unknown messages are logged but don't interrupt processing. Use when working with browsers implementing experimental features.
- **Collect**: Unknown messages are stored and thrown at shutdown. Use when discovering new protocol features or debugging compatibility.
- **Terminate**: Driver terminates when next command is sent after unknown message. Use when strict protocol conformance is required.

#### UnexpectedErrorBehavior

Controls how unexpected errors are handled (error responses received with no corresponding command):

[!code-csharp[UnexpectedErrorBehavior](../code/architecture/ArchitectureSamples.cs#UnexpectedErrorBehavior)]

**When to Use Each Mode:**
- **Ignore** (default): Unexpected errors are logged but don't interrupt processing. Use when browser may send asynchronous errors.
- **Collect**: Errors are stored and thrown at shutdown. Use when debugging communication issues.
- **Terminate**: Driver terminates when next command is sent after unexpected error. Use when unexpected errors indicate protocol implementation bugs.

### Configuring Multiple Behaviors

All four error behaviors can be configured independently:

[!code-csharp[Multiple Error Behaviors](../code/architecture/ArchitectureSamples.cs#MultipleErrorBehaviors)]

**Best Practices:**
- Start with **Terminate** during development to catch issues early
- Use **Collect** when diagnosing intermittent problems
- Switch to **Ignore** in production for non-critical errors
- Monitor logs regardless of behavior setting
- Consider your application's error tolerance when choosing behaviors
- Remember that **Collect** mode defers errors until `StopAsync()` is called
- Remember that **Terminate** mode throws errors on the next command, not immediately when the error occurs

## Extension Points

WebDriverBiDi.NET can be extended in several ways:

### Custom Modules

See [Custom Modules](advanced/custom-modules.md) for the full pattern. Register with the driver:

[!code-csharp[Custom Module Definition](../code/architecture/ArchitectureSamples.cs#CustomModuleDefinition)]

[!code-csharp[Custom Module Registration](../code/architecture/ArchitectureSamples.cs#CustomModuleRegistration)]

### Custom Transport

Create a class that extends `Transport` and overrides `DeserializeMessage` for custom message processing. Pass an instance to `BiDiDriver(TimeSpan, Transport)`.

## Performance Considerations

### Command Batching

Commands are executed sequentially per connection. To improve performance:

[!code-csharp[Command Batching](../code/architecture/ArchitectureSamples.cs#CommandBatching)]

### Event Handler Performance

Long-running event handlers block message processing:

[!code-csharp[Event Handler Performance](../code/architecture/ArchitectureSamples.cs#EventHandlerPerformance)]

### Memory Management

- **Unsubscribe from events** when no longer needed
- **Remove observers** to prevent memory leaks
- **Dispose of driver** to close WebSocket connection

[!code-csharp[Memory Management](../code/architecture/ArchitectureSamples.cs#MemoryManagement)]

## Summary

- **BiDiDriver**: Main facade and entry point
- **Transport**: WebSocket communication layer
- **Modules**: Organize functionality by domain
- **Commands**: Request-response pattern with type safety
- **Events**: Observable pattern with async support
- **Serialization**: System.Text.Json with custom converters
- **Threading**: Fully async with dedicated message processing thread
- **Extensibility**: Custom modules and transport implementations

## See Also

- [Error Handling](advanced/error-handling.md): Exception handling, timeout patterns, troubleshooting
- [API Design Guide](advanced/api-design.md): Timeout and cancellation, command parameter patterns

## Next Steps

- [Core Concepts](core-concepts.md): Understand modules, commands, and events
- [Browser Setup](browser-setup.md): Configure browsers for WebSocket or Pipe connections
- [Connection Management](advanced/connection-management.md): Deep dive into connection architecture
- [Events and Observables](events-observables.md): Deep dive into event handling
- [Error Handling](advanced/error-handling.md): Comprehensive error management strategies
- [Module Guides](modules/browser.md): Learn each module in detail

