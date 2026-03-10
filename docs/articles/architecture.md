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
│  ┌─────────────────────────────────────────────┐   │
│  │           Module Layer                      │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  │   │
│  │  │ Browser  │  │ Browsing │  │  Script  │  │   │
│  │  │  Module  │  │  Context │  │  Module  │  │   │
│  │  └──────────┘  │  Module  │  └──────────┘  │   │
│  │                └──────────┘                 │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  │   │
│  │  │ Network  │  │  Input   │  │   Log    │  │   │
│  │  │  Module  │  │  Module  │  │  Module  │  │   │
│  │  └──────────┘  └──────────┘  └──────────┘  │   │
│  └─────────────────────────────────────────────┘   │
│                      │                              │
│  ┌─────────────────────────────────────────────┐   │
│  │           Protocol Layer                    │   │
│  │         ┌──────────┐                        │   │
│  │         │Transport │                        │   │
│  │         └────┬─────┘                        │   │
│  │              │                               │   │
│  │         ┌────▼─────────┐                    │   │
│  │         │  Connection  │  (Abstract)        │   │
│  │         └────┬─────────┘                    │   │
│  │              │                               │   │
│  │      ┌───────┴────────┐                     │   │
│  │      ▼                ▼                     │   │
│  │  ┌────────────┐  ┌──────────┐              │   │
│  │  │  WebSocket │  │  Pipes   │              │   │
│  │  │ Connection │  │Connection│              │   │
│  │  └─────┬──────┘  └────┬─────┘              │   │
│  └────────┼──────────────┼──────────────────────┘
└───────────┼──────────────┼────────────────────────┘
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
┌──────────────┐   Serialize    ┌───────────┐   WebSocket   ┌─────────┐
│ Command      │────────────────▶│  JSON     │──────────────▶│ Browser │
│ Parameters   │                 │  Message  │               └─────────┘
└──────────────┘                 └───────────┘

Responses (Browser → Your Code):
┌─────────┐   WebSocket   ┌───────────┐   Deserialize   ┌──────────────┐
│ Browser │──────────────▶│  JSON     │─────────────────▶│ Command      │
└─────────┘                │  Message  │                  │ Result       │
                           └───────────┘                  └──────────────┘

Events (Browser → Observers):
┌─────────┐   WebSocket   ┌───────────┐   Deserialize   ┌──────────────┐
│ Browser │──────────────▶│  JSON     │─────────────────▶│ Event Args   │
└─────────┘                │  Message  │                  └──────┬───────┘
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

```csharp
public class BrowsingContextModule : Module
{
    // Constructor
    public BrowsingContextModule(IBiDiCommandExecutor driver)
        : base(driver) { }

    // Commands
    public async Task<NavigateCommandResult> NavigateAsync(
        NavigateCommandParameters parameters) { }
    
    public async Task<GetTreeCommandResult> GetTreeAsync(
        GetTreeCommandParameters parameters) { }

    // Observable Events
    public ObservableEvent<NavigationEventArgs> OnLoad { get; }
    public ObservableEvent<NavigationEventArgs> OnNavigationStarted { get; }
}
```

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

## Connection Types

WebDriverBiDi.NET uses an abstract `Connection` class to support multiple transport layers, allowing communication with browsers through different mechanisms.

### Connection Architecture

The `Connection` abstract class defines the contract for all transport implementations:

```csharp
public abstract class Connection : IAsyncDisposable
{
    // Abstract members that implementations must provide
    public abstract bool IsActive { get; }
    public abstract ConnectionType ConnectionType { get; }
    public abstract Task StartAsync(string connectionString, CancellationToken cancellationToken);
    public abstract Task StopAsync(CancellationToken cancellationToken);
    public abstract Task SendDataAsync(byte[] data, CancellationToken cancellationToken);

    // Observable events for connection lifecycle
    public ObservableEvent<ConnectionDataReceivedEventArgs> OnDataReceived { get; }
    public ObservableEvent<ConnectionErrorEventArgs> OnConnectionError { get; }
    public ObservableEvent<LogMessageEventArgs> OnLogMessage { get; }

    // Configurable timeouts
    public TimeSpan StartupTimeout { get; set; }  // Default: 10 seconds
    public TimeSpan ShutdownTimeout { get; set; } // Default: 10 seconds
    public TimeSpan DataTimeout { get; set; }     // Default: 10 seconds
}
```

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

```csharp
using WebDriverBiDi;

// Connect to browser at WebSocket URL
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.StartAsync("ws://localhost:9222/devtools/browser/abc-123");

try
{
    // Use the driver
    await driver.BrowsingContext.NavigateAsync(navParams);
}
finally
{
    await driver.StopAsync();
}
```

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

```csharp
using WebDriverBiDi;
using WebDriverBiDi.Client.Launchers;
using WebDriverBiDi.Protocol;

// Launcher implements IPipeServerProcessProvider
ChromeLauncher launcher = new ChromeLauncher()
{
    ConnectionType = ConnectionType.Pipes
};

await launcher.StartAsync();
await launcher.LaunchBrowserAsync();

try
{
    // Create driver with launcher's connection
    BiDiDriver driver = new BiDiDriver(
        TimeSpan.FromSeconds(30),
        launcher.CreateTransport());

    await driver.StartAsync("pipes");

    // Use the driver
    await driver.BrowsingContext.NavigateAsync(navParams);

    await driver.StopAsync();
}
finally
{
    await launcher.QuitBrowserAsync();
    await launcher.StopAsync();
}
```

**Browser Launch:**
```bash
chrome --remote-debugging-pipe
```

The `--remote-debugging-pipe` flag instructs the browser to communicate via stdin/stdout/file descriptors instead of opening a TCP port.

### IPipeServerProcessProvider Interface

The `IPipeServerProcessProvider` interface enables dependency injection for pipe connections:

```csharp
public interface IPipeServerProcessProvider
{
    Process? PipeServerProcess { get; }
}
```

This allows `PipeConnection` to access the browser process and its stdin/stdout handles. Implementations (like `ChromeLauncher`) manage the browser process lifecycle and provide it to the connection.

### ConnectionType Enum

The `ConnectionType` enum identifies which transport mechanism is being used:

```csharp
public enum ConnectionType
{
    WebSocket,  // WebSocket-based communication
    Pipes       // Pipe-based communication
}
```

Launchers use this to determine which browser flags to use (`--remote-debugging-port` vs `--remote-debugging-pipe`).

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

```csharp
// 1. Create parameters (mutable)
NavigateCommandParameters params = new NavigateCommandParameters(contextId, url);
params.Wait = ReadinessState.Complete;

// 2-11. Execute command (returns immutable result)
NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(params);
```

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
┌──────────────────────────┐
│  EventObserver<TEventArgs>│
│  EventObserver<TEventArgs>│
│  EventObserver<TEventArgs>│
└──────────────────────────┘
  │
  │ Invokes
  ▼
Your Event Handlers
```

**Observable Event:**

```csharp
public class ObservableEvent<T> where T : WebDriverBiDiEventArgs
{
    // Subscribe to event
    public EventObserver<T> AddObserver(
        Func<T, Task> handler, 
        ObservableEventHandlerOptions? options = null);
    
    // Notify all observers
    public async Task NotifyObserversAsync(T eventArgs);
}
```

**Event Observer:**

```csharp
public class EventObserver<T>
{
    // Set checkpoint to wait for N events
    public void SetCheckpoint(uint numberOfNotifications = 1);

    // Wait for checkpoint to be fulfilled
    public Task<bool> WaitForCheckpointAsync(TimeSpan timeout, CancellationToken cancellationToken = default);

    // Wait for checkpoint and all async handler tasks to complete
    public Task<bool> WaitForCheckpointAndTasksAsync(TimeSpan timeout, CancellationToken cancellationToken = default);

    // Get tasks from async handlers, transferring ownership to
    // the caller, and unsetting the checkpoint
    public Task[] GetCheckpointTasks();

    // Unset a previously-set checkpoint
    public void UnsetCheckpoint();
    
    // Remove observer
    public void Unobserve();
}
```

## Data Flow Patterns

### Command Execution

```csharp
// Synchronous-looking code (with async/await)
NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(params);

// What actually happens:
// 1. NavigateAsync creates a Command object
// 2. Command is serialized to JSON
// 3. JSON sent via WebSocket
// 4. Method awaits response
// 5. Browser processes navigation
// 6. Browser sends response JSON
// 7. Transport deserializes to NavigateCommandResult
// 8. Awaited method returns result
```

### Event Handling

```csharp
// Setup (before events occur)
driver.Log.OnEntryAdded.AddObserver((e) => {
    Console.WriteLine(e.Text);
});

await driver.Session.SubscribeAsync(subscribeParams);

// Runtime (when event occurs):
// 1. Browser emits log.entryAdded event
// 2. Transport receives JSON message
// 3. Transport deserializes to EntryAddedEventArgs
// 4. ObservableEvent.NotifyObserversAsync called
// 5. All registered observers invoked
// 6. Your handler executes
```

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

- `CommandParametersJsonConverter`: Serializes command parameters
- `CommandResultJsonConverter`: Deserializes command results
- `RemoteValueJsonConverter`: Handles RemoteValue polymorphism
- `LocalValueJsonConverter`: Serializes JavaScript values
- `EventArgsJsonConverter`: Deserializes event data

### Extension Data

Command results and event args capture unknown properties:

```csharp
public class CommandResult
{
    // Known properties
    public bool IsError { get; }
    
    // Unknown properties stored here
    [JsonExtensionData]
    internal Dictionary<string, JsonElement> SerializableAdditionalData { get; }
    
    // Exposed as read-only dictionary
    public ReceivedDataDictionary AdditionalData { get; }
}
```

This allows forward compatibility with new protocol versions.

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
```csharp
driver.Log.OnEntryAdded.AddObserver((e) => {
    // Runs on Transport thread
    // Blocks other message processing until complete
    Console.WriteLine(e.Text);
});
```

**Asynchronous Mode:**
```csharp
driver.Log.OnEntryAdded.AddObserver(
    async (e) => {
        // Runs on Task pool
        // Doesn't block message processing
        await ProcessLogEntryAsync(e);
    },
    ObservableEventHandlerOptions.RunHandlerAsynchronously
);
```

## Error Handling Configuration

WebDriverBiDi.NET provides configurable error handling behavior at multiple levels of the system.

### Transport Error Behavior

The `Transport` class can be configured with different error behaviors:

```csharp
public enum TransportErrorBehavior
{
    Ignore,     // Silently ignore transport errors (default)
    Collect,    // Store errors for later inspection
    Terminate   // Throw exception on next command execution
}
```

### Terminate Mode

Throws an exception when the next command is sent after a transport error:

```csharp
// Throws on next command call after error occurs
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30))
{
    EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
    ProtocolErrorBehavior = TransportErrorBehavior.Terminate,
    UnknownMessageBehavior = TransportErrorBehavior.Terminate,
    UnexpectedErrorBehavior = TransportErrorBehavior.Terminate,
};
try
{
    await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");
}
catch (WebDriverBiDiException ex)
{
    Console.WriteLine($"Connection failed: {ex.Message}");
}
```

**Use When:**
- You want fast failure on errors
- Errors indicate unrecoverable conditions
- You prefer explicit error handling

### Collect Mode

Stores transport errors in a list for later inspection:

```csharp
using WebDriverBiDi.Protocol;

WebSocketConnection connection = new WebSocketConnection();
Transport transport = new Transport(connection)
{
    EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
    ProtocolErrorBehavior = TransportErrorBehavior.Collect,
    UnknownMessageBehavior = TransportErrorBehavior.Collect,
    UnexpectedErrorBehavior = TransportErrorBehavior.Collect,
};
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);

await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

// Perform operations...
await driver.BrowsingContext.NavigateAsync(navParams);

try
{
    await driver.StopAsync();
}
catch (AggregateException ex)
{
    // Check for collected errors
    if (ex.InnerExceptions.Count > 0)
    {
        Console.WriteLine($"Encountered {ex.InnerExceptions.Count} transport errors:");
        foreach (Exception error in ex.InnerExceptions)
        {
            Console.WriteLine($"  - {error.Message}");
        }
    }
}
finally
{
    await driver.DisposeAsync();
}
```

**Use When:**
- You want to continue operation despite errors
- Collecting diagnostics for troubleshooting
- Errors might be transient or non-critical

### Ignore Mode

Silently discards transport errors without notification:

```csharp
using WebDriverBiDi.Protocol;

WebSocketConnection connection = new WebSocketConnection();
Transport transport = new Transport(connection);
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);

await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

// Errors won't be thrown or collected
await driver.BrowsingContext.NavigateAsync(navParams);

await driver.StopAsync();
```

**Use When:**
- Operating in fire-and-forget mode
- Errors are expected and acceptable
- You have alternative error detection mechanisms

**Warning:** Use this mode cautiously—it can mask real problems.

### Connection-Level Error Handling

Connections provide observable events for error monitoring:

```csharp
using WebDriverBiDi.Protocol;

WebSocketConnection connection = new WebSocketConnection();

// Subscribe to connection errors
connection.OnConnectionError.AddObserver((errorArgs) =>
{
    Console.WriteLine($"Connection error: {errorArgs.Exception.Message}");
});

// Subscribe to log messages
connection.OnLogMessage.AddObserver((logArgs) =>
{
    Console.WriteLine($"[{logArgs.Level}] {logArgs.Message}");
});

Transport transport = new Transport(connection);
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);

await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");
```

### Event Handler Error Behavior

Event handlers can also throw exceptions. Control this with `ObservableEventHandlerOptions`:

```csharp
// Synchronous handler: exceptions bubble up immediately
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    ProcessLogEntry(e);  // If this throws, exception propagates
});

// Asynchronous handler: exceptions are captured
driver.Network.OnBeforeRequestSent.AddObserver(
    async (e) =>
    {
        await ProcessRequestAsync(e);  // Exceptions captured
    },
    ObservableEventHandlerOptions.RunHandlerAsynchronously
);
```

See the [Error Handling Guide](advanced/error-handling.md) for comprehensive error management strategies.

### Granular Error Control

Beyond the transport-level error behavior, `BiDiDriver` exposes four properties for fine-grained control over different error scenarios. All use `TransportErrorBehavior` (Ignore/Collect/Terminate) and default to `Ignore`.

**Important:** With `Terminate` mode, exceptions don't propagate immediately when they occur. Due to the asynchronous nature of the library, termination errors are thrown when the next command is sent by the driver, not when the error is first encountered on the message processing thread.

#### EventHandlerExceptionBehavior

Controls how exceptions thrown by your event handlers are handled:

```csharp
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
driver.EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate;

driver.Log.OnEntryAdded.AddObserver((e) =>
{
    // If this throws, driver will terminate on next command
    ProcessLogEntry(e);
});

try
{
    await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

    // Perform operations...
    await driver.BrowsingContext.NavigateAsync(navParams);  // Exception thrown here if handler failed
}
catch (WebDriverBiDiException ex)
{
    Console.WriteLine($"Event handler error: {ex.Message}");
}
```

**When to Use Each Mode:**
- **Ignore** (default): Event handler exceptions are logged but don't interrupt message processing. The same applies to exceptions from asynchronously run handlers when those tasks are not captured by an observer checkpoint. Use for non-critical handlers.
- **Collect**: Exceptions are stored and thrown when `StopAsync()` is called. Exceptions from asynchronously run handlers are collected the same way when those tasks are not captured by an observer checkpoint. Use when debugging event handler issues.
- **Terminate**: Driver terminates when next command is sent after exception. Exceptions from asynchronously run handlers also surface on the next command when those tasks are not captured by an observer checkpoint. Use when event handler failure indicates unrecoverable state.

If you explicitly capture async handler tasks with `WaitForCheckpointAndTasksAsync()` or `GetCheckpointTasks()`, those task exceptions are instead owned by the caller and are not surfaced a second time through transport termination or collection.

#### ProtocolErrorBehavior

Controls how protocol errors are handled (invalid JSON, missing required properties, deserialization failures):

```csharp
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
driver.ProtocolErrorBehavior = TransportErrorBehavior.Collect;

await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

// Perform operations...
await driver.BrowsingContext.NavigateAsync(navParams);

// Collected errors are thrown when stopping
try
{
    await driver.StopAsync();
}
catch (WebDriverBiDiException ex)
{
    Console.WriteLine($"Protocol errors encountered: {ex.Message}");
}
```

**When to Use Each Mode:**
- **Ignore** (default): Protocol errors are logged but processing continues. Use when working with experimental or unstable protocol versions.
- **Collect**: Errors are stored and thrown at shutdown. Use when troubleshooting browser compatibility issues.
- **Terminate**: Driver terminates when next command is sent after error. Use in production when protocol violations indicate serious issues.

#### UnknownMessageBehavior

Controls how unknown messages are handled (valid JSON that doesn't match any known protocol structure):

```csharp
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
driver.UnknownMessageBehavior = TransportErrorBehavior.Ignore;

await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

// Browser sends new event type not yet supported by library
// With Ignore mode, these are logged but don't cause errors

await driver.BrowsingContext.NavigateAsync(navParams);
await driver.StopAsync();  // Completes without exception
```

**When to Use Each Mode:**
- **Ignore** (default): Unknown messages are logged but don't interrupt processing. Use when working with browsers implementing experimental features.
- **Collect**: Unknown messages are stored and thrown at shutdown. Use when discovering new protocol features or debugging compatibility.
- **Terminate**: Driver terminates when next command is sent after unknown message. Use when strict protocol conformance is required.

#### UnexpectedErrorBehavior

Controls how unexpected errors are handled (error responses received with no corresponding command):

```csharp
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
driver.UnexpectedErrorBehavior = TransportErrorBehavior.Terminate;

try
{
    await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

    // If browser sends error response without matching command ID, exception thrown on next command
    await driver.BrowsingContext.NavigateAsync(navParams);
}
catch (WebDriverBiDiException ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

**When to Use Each Mode:**
- **Ignore** (default): Unexpected errors are logged but don't interrupt processing. Use when browser may send asynchronous errors.
- **Collect**: Errors are stored and thrown at shutdown. Use when debugging communication issues.
- **Terminate**: Driver terminates when next command is sent after unexpected error. Use when unexpected errors indicate protocol implementation bugs.

### Configuring Multiple Behaviors

All four error behaviors can be configured independently:

```csharp
using WebDriverBiDi;

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

// Different strategies for different error types
driver.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;  // Collect handler errors
driver.ProtocolErrorBehavior = TransportErrorBehavior.Terminate;        // Fail fast on protocol errors
driver.UnknownMessageBehavior = TransportErrorBehavior.Ignore;         // Ignore unknown messages
driver.UnexpectedErrorBehavior = TransportErrorBehavior.Collect;       // Collect unexpected errors

await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

// Perform operations...
await driver.BrowsingContext.NavigateAsync(navParams);

// Errors with Collect behavior are thrown here
try
{
    await driver.StopAsync();
}
catch (WebDriverBiDiException ex)
{
    Console.WriteLine($"Collected errors: {ex.Message}");
    // Exception may contain multiple errors as inner exceptions
}
```

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

```csharp
public class MyCustomModule : Module
{
    public const string MyCustomModuleName = "myCustom";

    public MyCustomModule(IBiDiCommandExecutor driver)
        : base(driver) { }

    public override string ModuleName => MyCustomModuleName;
    
    public async Task<MyCommandResult> MyCommandAsync(
        MyCommandParameters parameters)
    {
        return await this.Driver.ExecuteCommandAsync<MyCommandResult>(
            parameters);
    }
}

// Register with driver
driver.RegisterModule(new MyCustomModule(driver));
```

### Custom Transport

```csharp
public class MyTransport : Transport
{
    protected override JsonElement DeserializeMessage(byte[] messageData)
    {
        // Custom message processing
        return base.DeserializeMessage(messageData);
    }
}

BiDiDriver driver = new BiDiDriver(
    TimeSpan.FromSeconds(10), 
    new MyTransport()
);
```

## Performance Considerations

### Command Batching

Commands are executed sequentially per connection. To improve performance:

```csharp
// ✓ Good: Execute independent commands in parallel
Task<GetTreeCommandResult> t1 = driver.BrowsingContext.GetTreeAsync(params1);
Task<StatusCommandResult> t2 = driver.Session.StatusAsync(params2);
await Task.WhenAll(t1, t2);

// ✗ Slower: Execute sequentially when not needed
var r1 = await driver.BrowsingContext.GetTreeAsync(params1);
var r2 = await driver.Session.StatusAsync(params2);
```

### Event Handler Performance

Long-running event handlers block message processing:

```csharp
// ✗ Bad: Blocks message processing
driver.Log.OnEntryAdded.AddObserver((e) => {
    Thread.Sleep(1000); // Blocks for 1 second
});

// ✓ Good: Run asynchronously
driver.Log.OnEntryAdded.AddObserver(
    async (e) => {
        await Task.Delay(1000); // Doesn't block
    },
    ObservableEventHandlerOptions.RunHandlerAsynchronously
);
```

### Memory Management

- **Unsubscribe from events** when no longer needed
- **Remove observers** to prevent memory leaks
- **Dispose of driver** to close WebSocket connection

```csharp
// Remove observer when done
EventObserver<EntryAddedEventArgs> observer = 
    driver.Log.OnEntryAdded.AddObserver(handler);
    
// Later...
observer.Unobserve();

// Unsubscribe from events
await driver.Session.UnsubscribeAsync(unsubscribeParams);

// Stop driver
await driver.StopAsync();
```

## Summary

- **BiDiDriver**: Main facade and entry point
- **Transport**: WebSocket communication layer
- **Modules**: Organize functionality by domain
- **Commands**: Request-response pattern with type safety
- **Events**: Observable pattern with async support
- **Serialization**: System.Text.Json with custom converters
- **Threading**: Fully async with dedicated message processing thread
- **Extensibility**: Custom modules and transport implementations

## Next Steps

- [Core Concepts](core-concepts.md): Understand modules, commands, and events
- [Browser Setup](browser-setup.md): Configure browsers for WebSocket or Pipe connections
- [Connection Management](advanced/connection-management.md): Deep dive into connection architecture
- [Events and Observables](events-observables.md): Deep dive into event handling
- [Error Handling](advanced/error-handling.md): Comprehensive error management strategies
- [Module Guides](modules/browser.md): Learn each module in detail

