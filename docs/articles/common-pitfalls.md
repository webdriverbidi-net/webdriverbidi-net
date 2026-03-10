# Common Pitfalls

This guide covers frequently encountered issues and misunderstandings when working with WebDriverBiDi.NET. Understanding these pitfalls will help you avoid common mistakes and build more robust automation.

## Event Handler Execution

### Pitfall: Blocking the Transport Thread with Synchronous Handlers

**The Problem:**

By default, event handlers run **synchronously on the transport thread**, which blocks all message processing until the handler completes. This can cause serious performance issues.

```csharp
// ❌ BAD: Blocks transport thread for 5 seconds
driver.Network.OnBeforeRequestSent.AddObserver((e) =>
{
    Thread.Sleep(5000);  // Blocks ALL message processing!
    Console.WriteLine($"Request: {e.Request.Url}");

    // During these 5 seconds:
    // - No other events are processed
    // - No command responses are received
    // - Commands may timeout
    // - The browser may become unresponsive
});
```

**Why This Happens:**

WebDriverBiDi.NET uses a single transport thread to process all incoming messages from the browser. When your handler blocks that thread, nothing else can be processed.

**The Solution:**

Use `ObservableEventHandlerOptions.RunHandlerAsynchronously` for any handler that:
- Performs I/O operations (file, network, database)
- Does CPU-intensive work
- Calls other async APIs
- Takes more than a few milliseconds

```csharp
// ✅ GOOD: Runs asynchronously without blocking
driver.Network.OnBeforeRequestSent.AddObserver(
    async (e) =>
    {
        await Task.Delay(5000);  // Doesn't block transport thread
        Console.WriteLine($"Request: {e.Request.Url}");

        // Transport thread continues processing other messages
        // while this handler runs on a task pool thread
    },
    ObservableEventHandlerOptions.RunHandlerAsynchronously
);
```

**When Synchronous is OK:**

Synchronous handlers are fine for quick operations:

```csharp
// ✅ Fine: Quick in-memory operation
int requestCount = 0;
driver.Network.OnBeforeRequestSent.AddObserver((e) =>
{
    requestCount++;  // Fast, synchronous is acceptable
});

// ✅ Fine: Simple logging to console
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    Console.WriteLine($"[{e.Level}] {e.Text}");  // Fast operation
});
```

**Key Takeaway:** If your handler does anything more than updating in-memory state or simple console output, use `RunHandlerAsynchronously`.

---

## Event Subscription

### Pitfall: Forgetting the Two-Step Subscription Process

**The Problem:**

Many developers expect that adding an observer is sufficient to receive events. It's not.

```csharp
// ❌ INCOMPLETE: Observer added but no events will be received
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    Console.WriteLine(e.Text);
});

await driver.BrowsingContext.NavigateAsync(navParams);
// No log events will fire - you forgot to subscribe!
```

**Why This Design:**

The two-step process (add observer + subscribe) is **intentional** and prevents race conditions. It ensures:
1. Your handlers are in place before events start flowing
2. You have explicit control over which events are subscribed
3. You can scope subscriptions to specific browsing contexts

**The Solution:**

Always add observer first, then subscribe through the Session module:

```csharp
// ✅ CORRECT: Add observer AND subscribe
// Step 1: Add observer
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    Console.WriteLine(e.Text);
});

// Step 2: Subscribe to events
SubscribeCommandParameters subscribe = new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName);
await driver.Session.SubscribeAsync(subscribe);

// Now events will be received
await driver.BrowsingContext.NavigateAsync(navParams);
```

**Best Practice - Subscribe Multiple Events at Once:**

```csharp
// Add all observers first
driver.Log.OnEntryAdded.AddObserver(logHandler);
driver.Network.OnBeforeRequestSent.AddObserver(networkHandler);
driver.BrowsingContext.OnLoad.AddObserver(loadHandler);

// Then subscribe to all events in one call
SubscribeCommandParameters subscribe = new SubscribeCommandParameters(
    [
        driver.Log.OnEntryAdded.EventName,
        driver.Network.OnBeforeRequestSent.EventName,
        driver.BrowsingContext.OnLoad.EventName,
    ]
);
await driver.Session.SubscribeAsync(subscribe);
```

**Key Takeaway:** Adding an observer only registers your handler locally. You must explicitly subscribe through `Session.SubscribeAsync()` to tell the browser to send events.

---

## Module Registration Timing

### Pitfall: Registering Modules After StartAsync()

**The Problem:**

Attempting to register custom modules or add event observers after calling `StartAsync()` will throw an exception.

```csharp
// ❌ WRONG: Registration after starting
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

// This will throw InvalidOperationException!
driver.RegisterModule(new CustomModule(driver));
driver.Log.OnEntryAdded.AddObserver(handler);  // May also fail
```

**Why This Restriction:**

This timing restriction ensures:
1. All handlers are in place before events start flowing from the browser
2. No race conditions between module registration and event dispatch
3. Predictable initialization order
4. Thread-safe module setup

**The Solution:**

Always register modules and add observers BEFORE calling `StartAsync()`:

```csharp
// ✅ CORRECT: Registration before starting
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

// 1. Register custom modules (if any)
driver.RegisterModule(new CustomModule(driver));

// 2. Add event observers
driver.Log.OnEntryAdded.AddObserver((e) => Console.WriteLine(e.Text));
driver.BrowsingContext.OnLoad.AddObserver((e) => Console.WriteLine($"Loaded: {e.Url}"));

// 3. NOW start the driver
await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

// 4. Subscribe to events through Session module
SubscribeCommandParameters subscribe = new SubscribeCommandParameters(
    [
        driver.Log.OnEntryAdded.EventName,
        driver.BrowsingContext.OnLoad.EventName,
    ]
);
await driver.Session.SubscribeAsync(subscribe);

// 5. Execute commands
await driver.BrowsingContext.NavigateAsync(navParams);
```

**Correct Initialization Order:**

```
1. Create BiDiDriver
2. Register custom modules (if needed)
3. Add event observers (if needed)
4. Call StartAsync()
5. Subscribe to events via Session.SubscribeAsync()
6. Execute commands
```

**Key Takeaway:** Think of module registration and observer setup as "configuration" that must happen before "connection" (StartAsync).

---

## Null vs Empty Collections

### Pitfall: Not Understanding Nullable Collection Properties

**The Problem:**

Many `CommandParameters` classes have nullable collection properties like `List<string>? Contexts`. Developers often wonder why these aren't initialized to empty lists.

```csharp
SetLocaleOverrideCommandParameters parameters = new SetLocaleOverrideCommandParameters()
{
    Locale = "en-US",
};

// Why is Contexts null instead of an empty list?
if (parameters.Contexts == null)  // This is true!
{
    parameters.Contexts = new List<string>();
}
```

**Why Nullable Collections:**

For the WebDriver BiDi protocol, there's an important distinction:
- **`null`**: The property is **omitted from the JSON payload entirely**
- **Empty list**: An **empty array `[]` is sent** in the JSON payload

These have **different meanings** in the protocol specification.

**Example Protocol Difference:**

```csharp
// Case 1: Contexts is null
SetLocaleOverrideCommandParameters parameters = new SetLocaleOverrideCommandParameters()
{
    Locale = "en-US",
};
// JSON sent: { "locale": "en-US" /* no "contexts" property */ }

// Case 2: Contexts is empty list
SetLocaleOverrideCommandParameters parameters = new SetLocaleOverrideCommandParameters()
{
    Locale = "en-US",
};
parameters.Contexts = new List<string>();
// JSON sent: { "locale": "en-US",  "contexts": [] }

// Case 3: Events has items
SetLocaleOverrideCommandParameters parameters = new SetLocaleOverrideCommandParameters()
{
    Locale = "en-US",
};
parameters.Contexts = new List<string> { "<valid browsing context ID>" };
// JSON sent: { "locale": "en-US",  "contexts": ["<valid browsing context ID>"] }
```

**The Solution:**

Always check for null before adding items, or initialize when needed:

```csharp
// ✅ Option 1: Null-conditional + null-coalescing
parameters.Contexts ??= new List<string>();
parameters.Contexts.Add("<valid browsing context ID>");

// ✅ Option 2: Check before adding
if (parameters.Contexts == null)
{
    parameters.Contexts = new List<string>();
}
parameters.Contexts.Add("<valid browsing context ID>");

// ✅ Option 3: Initialize in one line
parameters.Contexts = new List<string>
{
    "<valid browsing context ID>",
    "<another valid browsing context ID>"
};
```

**Key Takeaway:** Nullable collections are intentional. They allow distinguishing between "omit this property" (null) and "send an empty array" (empty list), which have different protocol meanings.

---

## Command Timeouts

### Pitfall: Not Understanding the Default Timeout

**The Problem:**

Developers are sometimes surprised that the default command timeout is **60 seconds**.

```csharp
// Default timeout is 60 seconds!
BiDiDriver driver = new BiDiDriver();

// This command has 60 seconds to complete
await driver.BrowsingContext.NavigateAsync(navParams);
```

**Why 60 Seconds:**

The high default timeout is **intentional** because:
1. Browser automation can involve genuinely slow operations
2. Page loads can take a long time (slow networks, heavy pages)
3. Script execution might be CPU-intensive
4. Network requests in tests might be slow
5. Better to have a long default than frequent timeouts

**The Solution:**

Set appropriate timeouts for your use case:

```csharp
// ✅ For fast operations (local testing)
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));

// ✅ For normal web automation
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

// ✅ For slow operations or large pages
BiDiDriver driver = new BiDiDriver(TimeSpan.FromMinutes(2));

// ✅ Override per-command for specific cases
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
// Most commands use 30 second timeout

// But this specific navigation gets longer timeout
await driver.ExecuteCommandAsync<NavigateCommandResult>(
    slowPageParams,
    TimeSpan.FromMinutes(5)
);
```

**Key Takeaway:** The 60-second default is intentionally set to accommodate slow operations. Choose a timeout that matches your typical use case, and override per-command when needed.

---

## Event Handler Synchronization

### Pitfall: Not Waiting for Async Handlers to Complete

**The Problem:**

When using `RunHandlerAsynchronously`, the handler runs on a background task. Your main code might continue before the handler finishes.

```csharp
// ❌ PROBLEM: Handler might not finish before program exits
driver.Network.OnBeforeRequestSent.AddObserver(
    async (e) =>
    {
        await Task.Delay(5000);  // Long-running operation
        await SaveRequestToFileAsync(e.Request);
    },
    ObservableEventHandlerOptions.RunHandlerAsynchronously
);

await driver.Session.SubscribeAsync(subscribeParams);
await driver.BrowsingContext.NavigateAsync(navParams);

// Navigation completes, but handlers might still be running!
// If program exits here, handlers may not finish
```

**Why This Happens:**

- `NavigateAsync()` completes when the browser responds to the command
- Async event handlers run independently on background tasks
- There's no automatic synchronization between command completion and handler completion

**The Solution - Option 1: Use WaitForCheckpointAndTasksAsync (Recommended):**

```csharp
// ✅ GOOD: Use built-in helper
EventObserver<BeforeRequestSentEventArgs> observer =
    driver.Network.OnBeforeRequestSent.AddObserver(
        async (e) =>
        {
            await Task.Delay(5000);
            await SaveRequestToFileAsync(e.Request);
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously
    );

await driver.Session.SubscribeAsync(subscribeParams);

// Set checkpoint for expected number of events
observer.SetCheckpoint(5);

// Trigger events
await driver.BrowsingContext.NavigateAsync(navParams);

// Wait for events to occur AND handlers to complete
await observer.WaitForCheckpointAndTasksAsync(TimeSpan.FromSeconds(10));

// Now all handlers have completed
```

**The Solution - Option 2: Manual Synchronization:**

```csharp
// ✅ GOOD: Manual synchronization for complex scenarios
EventObserver<BeforeRequestSentEventArgs> observer =
    driver.Network.OnBeforeRequestSent.AddObserver(
        async (e) =>
        {
            await SaveRequestToFileAsync(e.Request);
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously
    );

await driver.Session.SubscribeAsync(subscribeParams);

observer.SetCheckpoint(5);
await driver.BrowsingContext.NavigateAsync(navParams);

// Wait for events to occur
bool fulfilled = await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(10));

if (fulfilled)
{
    // Get handler tasks
    Task[] handlerTasks = observer.GetCheckpointTasks();

    // Inspect or manipulate tasks if needed
    Console.WriteLine($"Waiting for {handlerTasks.Length} handlers to complete...");

    // Wait for all handlers to finish
    await Task.WhenAll(handlerTasks);
}
```

**The Solution - Option 3: TaskCompletionSource for Custom Control:**

```csharp
// ✅ GOOD: TaskCompletionSource for fine-grained control
List<Task> completionTasks = new();

driver.Network.OnBeforeRequestSent.AddObserver(
    async (e) =>
    {
        TaskCompletionSource tcs = new();
        completionTasks.Add(tcs.Task);

        try
        {
            await SaveRequestToFileAsync(e.Request);
            tcs.SetResult();
        }
        catch (Exception ex)
        {
            tcs.SetException(ex);
        }
    },
    ObservableEventHandlerOptions.RunHandlerAsynchronously
);

await driver.Session.SubscribeAsync(subscribeParams);
await driver.BrowsingContext.NavigateAsync(navParams);

// Wait for all handlers
await Task.WhenAll(completionTasks);
```

**Key Takeaway:** With async handlers, use `WaitForCheckpointAndTasksAsync()` or manual checkpoint management to ensure handlers complete before your code continues.

---

## Transport Error Behavior

### Pitfall: Not Understanding Default Error Handling

**The Problem:**

By default, transport-level errors (invalid protocol messages, event handler exceptions) are **ignored**. This can hide bugs in your event handlers.

```csharp
// ❌ PROBLEM: Handler exceptions are silently ignored by default
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    // If this throws, the exception is IGNORED by default
    ProcessLogEntry(e);  // Might throw
});

await driver.Session.SubscribeAsync(subscribeParams);
await driver.BrowsingContext.NavigateAsync(navParams);
// If handler threw, you'll never know!
```

**Why Default is Ignore:**

The library defaults to `TransportErrorBehavior.Ignore` to prevent event handler exceptions from disrupting automation. However, this can mask bugs during development.

**The Solution - For Development: Use Terminate or Collect:**

```csharp
// ✅ Option 1: Terminate mode (throws on next command)
using WebDriverBiDi.Protocol;

WebSocketConnection connection = new WebSocketConnection();
Transport transport = new Transport(connection);

// Change error behavior
transport.EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate;
transport.ProtocolErrorBehavior = TransportErrorBehavior.Terminate;

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);

try
{
    await driver.StartAsync("ws://localhost:9222");

    // Add handler that might throw
    driver.Log.OnEntryAdded.AddObserver((e) => ProcessLogEntry(e));

    await driver.Session.SubscribeAsync(subscribeParams);

    // If handler throws, exception surfaces here on next command
    await driver.BrowsingContext.NavigateAsync(navParams);
}
catch (WebDriverBiDiException ex)
{
    Console.WriteLine($"Event handler error: {ex.Message}");
}
```

This also applies to exceptions from handlers using `ObservableEventHandlerOptions.RunHandlerAsynchronously` when those tasks are not captured by a checkpoint. If you instead capture handler tasks with checkpoints, those exceptions are owned by the returned tasks and should be observed there.

```csharp
// ✅ Option 2: Collect mode (gather all errors)
WebSocketConnection connection = new WebSocketConnection();
Transport transport = new Transport(connection);

transport.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
transport.ProtocolErrorBehavior = TransportErrorBehavior.Collect;

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);

await driver.StartAsync("ws://localhost:9222");
driver.Log.OnEntryAdded.AddObserver((e) => ProcessLogEntry(e));
await driver.Session.SubscribeAsync(subscribeParams);
await driver.BrowsingContext.NavigateAsync(navParams);

try
{
    await driver.StopAsync();
}
catch (AggregateException ex)
{
    // Check collected errors
    if (ex.InnerExceptions.Count > 0)
    {
        Console.WriteLine($"Collected {ex.InnerExceptions.Count} errors:");
        foreach (var error in ex.InnerExceptions)
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

**The Solution - For Production: Handle Exceptions in Handlers:**

```csharp
// ✅ BEST: Handle exceptions inside handlers
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    try
    {
        ProcessLogEntry(e);
    }
    catch (Exception ex)
    {
        // Log error, but don't let it propagate
        Console.WriteLine($"Error processing log entry: {ex.Message}");
    }
});
```

**Error Behavior Modes:**

| Mode | Behavior | Best For |
|------|----------|----------|
| **Ignore** (default) | Errors discarded silently | Production (with try-catch in handlers) |
| **Collect** | Errors stored in list | Development, diagnostics |
| **Terminate** | Throws on next command | Development, fast failure |

**Key Takeaway:** Default error behavior is Ignore. During development, use Terminate or Collect mode to catch handler bugs. In production, handle exceptions within your handlers.

---

## Thread Safety

### Pitfall: Assuming All Operations Are Thread-Safe

**The Problem:**

While many operations in WebDriverBiDi.NET are thread-safe, not all concurrent scenarios are supported.

```csharp
// ❌ RISKY: Concurrent module registration (now safe, but was risky)
Parallel.Invoke(
    () => driver.RegisterModule(module1),
    () => driver.RegisterModule(module2)
);

// ❌ RISKY: Concurrent observer management on same observable
Parallel.For(0, 10, i =>
{
    driver.Log.OnEntryAdded.AddObserver((e) =>
        Console.WriteLine($"Handler {i}: {e.Text}"));
});
```

**What IS Thread-Safe:**

- `BiDiDriver.RegisterModule()` (as of recent updates)
- Command execution (`ExecuteCommandAsync`)
- Event observer notification
- Transport message processing
- EventObserver checkpoint methods (`SetCheckpoint`, `WaitForCheckpointAsync`,
`WaitForCheckpointAndTasksAsync`, `GetCheckpointTasks`, `UnsetCheckpoint`) - multiple
threads may wait on the same checkpoint; only one checkpoint per observer at a time

**What to Be Careful With:**

- Adding multiple observers concurrently to the same event
- Modifying shared state from multiple event handlers
- Concurrent access to command parameter objects

**The Solution:**

```csharp
// ✅ GOOD: Register modules before concurrent operations
driver.RegisterModule(module1);
driver.RegisterModule(module2);
await driver.StartAsync(url);

// ✅ GOOD: Add observers sequentially during setup
driver.Log.OnEntryAdded.AddObserver(handler1);
driver.Log.OnEntryAdded.AddObserver(handler2);

// ✅ GOOD: Execute commands concurrently (this IS safe)
Task<NavigateCommandResult> nav1 =
    driver.BrowsingContext.NavigateAsync(params1);
Task<NavigateCommandResult> nav2 =
    driver.BrowsingContext.NavigateAsync(params2);
await Task.WhenAll(nav1, nav2);

// ✅ GOOD: Use locks for shared state in handlers
object stateLock = new();
int counter = 0;

driver.Log.OnEntryAdded.AddObserver((e) =>
{
    lock (stateLock)
    {
        counter++;
    }
});
```

**Key Takeaway:** While the transport layer is thread-safe, setup operations (registration, observer management) should typically be done sequentially. Command execution is safe to parallelize.

---

## Resource Cleanup

### Pitfall: Not Disposing Observers and Driver

**The Problem:**

Event observers and the driver hold resources that should be properly disposed.

```csharp
// ❌ BAD: No cleanup
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.StartAsync(url);

EventObserver<EntryAddedEventArgs> observer =
    driver.Log.OnEntryAdded.AddObserver(handler);

// ... use driver ...

// Oops! Never stopped driver or removed observer
// Resources leaked!
```

**The Solution:**

Always clean up resources:

```csharp
// ✅ GOOD: Proper cleanup
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

try
{
    await driver.StartAsync(url);

    EventObserver<EntryAddedEventArgs> observer =
        driver.Log.OnEntryAdded.AddObserver(handler);

    try
    {
        await driver.Session.SubscribeAsync(subscribeParams);

        // ... use driver ...
    }
    finally
    {
        // Remove observer when done
        observer.Unobserve();
    }
}
finally
{
    // Always stop driver
    if (driver.IsStarted)
    {
        await driver.StopAsync();
    }
}

// ✅ BETTER: Use async disposal
await using BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.StartAsync(url);

using EventObserver<EntryAddedEventArgs> observer =
    driver.Log.OnEntryAdded.AddObserver(handler);

// Automatic cleanup when scope exits
```

**Key Takeaway:** Use `try-finally` blocks or `using`/`await using` statements to ensure proper cleanup of observers and the driver.

---

## Summary Checklist

Before running your WebDriverBiDi.NET code, verify:

- [ ] Event handlers use `RunHandlerAsynchronously` for I/O operations
- [ ] You've called both `AddObserver()` AND `Session.SubscribeAsync()`
- [ ] Modules and observers registered BEFORE `StartAsync()`
- [ ] Nullable collections checked for null before adding items
- [ ] Timeout configured appropriately for your use case
- [ ] Async handlers synchronized with checkpoints if needed
- [ ] Transport error behavior set for development/production
- [ ] Using correct WebSocket URL format (`ws://`)
- [ ] Thread safety considered for concurrent operations
- [ ] Resources properly cleaned up with try-finally or using statements

---

## Next Steps

- [Events and Observables](events-observables.md): Deep dive into event handling
- [Error Handling](advanced/error-handling.md): Comprehensive error management strategies
- [Core Concepts](core-concepts.md): Understanding the fundamentals
- [Architecture](architecture.md): System design and patterns
