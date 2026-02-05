# Architecture Overview

This document provides an architectural overview of WebDriverBiDi.NET-Relaxed, explaining how the library is organized and how data flows through the system.

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
│  └──────────────┼──────────────────────────────┘   │
└─────────────────┼────────────────────────────────┘
                  │ WebSocket (JSON messages)
                  ▼
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

The `Transport` class handles low-level WebSocket communication.

**Responsibilities:**
- Manages WebSocket connection
- Serializes commands to JSON
- Deserializes responses and events from JSON
- Correlates responses with sent commands
- Routes events to appropriate handlers

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

### Module Layer

Each module encapsulates a specific area of WebDriver BiDi functionality.

**Module Structure:**

```csharp
public class BrowsingContextModule : Module
{
    // Constructor
    public BrowsingContextModule(BiDiDriver driver) 
        : base(driver, BrowsingContextModuleName) { }

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

WebDriverBiDi.NET-Relaxed uses an observable event pattern for handling browser events.

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
    public void SetCheckpoint(int count = 1);
    
    // Wait for checkpoint to be fulfilled
    public bool WaitForCheckpoint(TimeSpan timeout);
    
    // Get tasks from async handlers
    public Task[] GetCheckpointTasks();
    
    // Remove observer
    public void Unobserve();
}
```

## Data Flow Patterns

### Command Execution

```csharp
// Synchronous-looking code (with async/await)
var result = await driver.BrowsingContext.NavigateAsync(params);

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

WebDriverBiDi.NET-Relaxed uses `System.Text.Json` for JSON serialization.

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

WebDriverBiDi.NET-Relaxed is fully asynchronous and thread-safe for most operations.

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

## Extension Points

WebDriverBiDi.NET-Relaxed can be extended in several ways:

### Custom Modules

```csharp
public class MyCustomModule : Module
{
    public MyCustomModule(BiDiDriver driver) 
        : base(driver, "myCustom") { }
    
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
    protected override async Task OnMessageReceived(string message)
    {
        // Custom message processing
        await base.OnMessageReceived(message);
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
- [Events and Observables](events-observables.md): Deep dive into event handling
- [Module Guides](modules/browser.md): Learn each module in detail

