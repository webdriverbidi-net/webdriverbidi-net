# Common Pitfalls

This guide covers frequently encountered issues and misunderstandings when working with WebDriverBiDi.NET. Understanding these pitfalls will help you avoid common mistakes and build more robust automation.

## Event Handler Execution

### Pitfall: Blocking the Transport Thread with Synchronous Handlers

**The Problem:**

By default, event handlers run **synchronously on the transport thread**, which blocks all message processing until the handler completes. This can cause serious performance issues.

[!code-csharp[Blocking Handler](../code/common-pitfalls/CommonPitfallsSamples.cs#BlockingHandler)]

**Why This Happens:**

WebDriverBiDi.NET uses a single transport thread to process all incoming messages from the browser. When your handler blocks that thread, nothing else can be processed.

**The Solution:**

Use `ObservableEventHandlerOptions.RunHandlerAsynchronously` for any handler that:
- Performs I/O operations (file, network, database)
- Does CPU-intensive work
- Calls other async APIs
- Takes more than a few milliseconds

[!code-csharp[Non-Blocking Handler](../code/common-pitfalls/CommonPitfallsSamples.cs#Non-BlockingHandler)]

**When Synchronous is OK:**

Synchronous handlers are fine for quick operations:

[!code-csharp[Quick Synchronous Handler](../code/common-pitfalls/CommonPitfallsSamples.cs#QuickSynchronousHandler)]

**Key Takeaway:** If your handler does anything more than updating in-memory state or simple console output, use `RunHandlerAsynchronously`.

---

## Event Subscription

### Pitfall: Forgetting the Two-Step Subscription Process

**The Problem:**

Many developers expect that adding an observer is sufficient to receive events. It's not.

[!code-csharp[Incomplete Subscription](../code/common-pitfalls/CommonPitfallsSamples.cs#IncompleteSubscription)]

**Why This Design:**

The two-step process (add observer + subscribe) is **intentional** and prevents race conditions. It ensures:
1. Your handlers are in place before events start flowing
2. You have explicit control over which events are subscribed
3. You can scope subscriptions to specific browsing contexts

**The Solution:**

Always add observer first, then subscribe through the Session module:

[!code-csharp[Two-Step Subscription](../code/common-pitfalls/CommonPitfallsSamples.cs#Two-StepSubscription)]

**Best Practice - Subscribe Multiple Events at Once:**

[!code-csharp[Subscribe Multiple Events](../code/common-pitfalls/CommonPitfallsSamples.cs#SubscribeMultipleEvents)]

**Key Takeaway:** Adding an observer only registers your handler locally. You must explicitly subscribe through `Session.SubscribeAsync()` to tell the browser to send events.

---

## Module Registration Timing

### Pitfall: Registering Modules After StartAsync()

**The Problem:**

Attempting to register custom modules or add event observers after calling `StartAsync()` will throw an exception.

[!code-csharp[Wrong Registration Order](../code/common-pitfalls/CommonPitfallsSamples.cs#WrongRegistrationOrder)]

**Why This Restriction:**

This timing restriction ensures:
1. All handlers are in place before events start flowing from the browser
2. No race conditions between module registration and event dispatch
3. Predictable initialization order
4. Thread-safe module setup

**The Solution:**

Always register modules and add observers BEFORE calling `StartAsync()`:

[!code-csharp[Correct Registration Order](../code/common-pitfalls/CommonPitfallsSamples.cs#CorrectRegistrationOrder)]

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

[!code-csharp[Nullable Collection Example](../code/common-pitfalls/CommonPitfallsSamples.cs#NullableCollectionExample)]

**Why Nullable Collections:**

For the WebDriver BiDi protocol, there's an important distinction:
- **`null`**: The property is **omitted from the JSON payload entirely**
- **Empty list**: An **empty array `[]` is sent** in the JSON payload

These have **different meanings** in the protocol specification.

**Example Protocol Difference:**

[!code-csharp[Null vs Empty vs Items](../code/common-pitfalls/CommonPitfallsSamples.cs#NullvsEmptyvsItems)]

**The Solution:**

Always check for null before adding items, or initialize when needed:

[!code-csharp[Handle Nullable Collections](../code/common-pitfalls/CommonPitfallsSamples.cs#HandleNullableCollections)]

**Key Takeaway:** Nullable collections are intentional. They allow distinguishing between "omit this property" (null) and "send an empty array" (empty list), which have different protocol meanings.

---

## Command Timeouts

### Pitfall: Not Understanding the Default Timeout

**The Problem:**

Developers are sometimes surprised that the default command timeout is **60 seconds**.

[!code-csharp[Default Timeout](../code/common-pitfalls/CommonPitfallsSamples.cs#DefaultTimeout)]

**Why 60 Seconds:**

The high default timeout is **intentional** because:
1. Browser automation can involve genuinely slow operations
2. Page loads can take a long time (slow networks, heavy pages)
3. Script execution might be CPU-intensive
4. Network requests in tests might be slow
5. Better to have a long default than frequent timeouts

**The Solution:**

Set appropriate timeouts for your use case. **Prefer the `timeoutOverride` parameter on module methods** (e.g., `NavigateAsync(parameters, TimeSpan.FromSeconds(60))`) over `ExecuteCommandAsync` when you need per-command overrides:

[!code-csharp[Configure Timeouts](../code/common-pitfalls/CommonPitfallsSamples.cs#ConfigureTimeouts)]

**Key Takeaway:** The 60-second default is intentionally set to accommodate slow operations. Choose a timeout that matches your typical use case, and use the `timeoutOverride` parameter on module methods when only specific commands need a different timeout.

---

## Event Handler Synchronization

### Pitfall: Not Waiting for Async Handlers to Complete

**The Problem:**

When using `RunHandlerAsynchronously`, the handler runs on a background task. Your main code might continue before the handler finishes.

[!code-csharp[Async Handler Problem](../code/common-pitfalls/CommonPitfallsSamples.cs#AsyncHandlerProblem)]

**Why This Happens:**

- `NavigateAsync()` completes when the browser responds to the command
- Async event handlers run independently on background tasks
- There's no automatic synchronization between command completion and handler completion

**The Solution - Option 1: Use WaitForCapturedTasksAsync (Recommended):**

[!code-csharp[Wait For Captured Tasks](../code/common-pitfalls/CommonPitfallsSamples.cs#WaitForCapturedTasks)]

**The Solution - Option 2: Manual Synchronization:**

[!code-csharp[Manual Synchronization](../code/common-pitfalls/CommonPitfallsSamples.cs#ManualSynchronization)]

**The Solution - Option 3: GetCapturedTasks for Custom Control:**

[!code-csharp[Get Captured Tasks](../code/common-pitfalls/CommonPitfallsSamples.cs#GetCapturedTasksExample)]

**Key Takeaway:** With async handlers, use `WaitForCapturedTasksCompleteAsync()` or `WaitForCapturedTasksAsync()` with manual task management to ensure handlers complete before your code continues.

---

## Transport Error Behavior

### Pitfall: Not Understanding Default Error Handling

**The Problem:**

By default, transport-level errors (invalid protocol messages, event handler exceptions) are **ignored**. This can hide bugs in your event handlers.

[!code-csharp[Handler Exceptions Ignored](../code/common-pitfalls/CommonPitfallsSamples.cs#HandlerExceptionsIgnored)]

**Why Default is Ignore:**

The library defaults to `TransportErrorBehavior.Ignore` to prevent event handler exceptions from disrupting automation. However, this can mask bugs during development.

**The Solution - For Development: Use Terminate or Collect:**

[!code-csharp[Terminate Error Behavior](../code/common-pitfalls/CommonPitfallsSamples.cs#TerminateErrorBehavior)]

This also applies to exceptions from handlers using `ObservableEventHandlerOptions.RunHandlerAsynchronously` when those tasks are not captured by a capture session. If you instead capture handler tasks using `WaitForCapturedTasksAsync()` or `WaitForCapturedTasksCompleteAsync()`, those exceptions are owned by the returned tasks and should be observed there.

[!code-csharp[Collect Error Behavior](../code/common-pitfalls/CommonPitfallsSamples.cs#CollectErrorBehavior)]

**The Solution - For Production: Handle Exceptions in Handlers:**

[!code-csharp[Handle Exceptions In Handlers](../code/common-pitfalls/CommonPitfallsSamples.cs#HandleExceptionsInHandlers)]

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

**What IS Thread-Safe:**

- `BiDiDriver.RegisterModule()`
- Command execution (`ExecuteCommandAsync`)
- Event observer notification
- Adding and removing observers (`AddObserver`, `RemoveObserver`, `Unobserve`) on the same event
- Transport message processing
- EventObserver capture API (`StartCapturingTasks`, `StopCapturingTasks`, `WaitForCapturedTasksAsync`,
`WaitForCapturedTasksCompleteAsync`, `GetCapturedTasks`) - individually thread-safe; only one
capture session per observer at a time; the channel queues captured tasks so that no events
are missed even if they arrive before the consumer is ready

**What to Be Careful With:**

- Modifying shared state from multiple event handlers
- Concurrent access to command parameter objects

**The Solution:**

[!code-csharp[Thread Safety Example](../code/common-pitfalls/CommonPitfallsSamples.cs#ThreadSafetyExample)]

**Key Takeaway:** Transport processing, command execution, and observer registration/removal are thread-safe. You can parallelize those operations, but still protect any shared mutable application state used by handlers.

---

## Resource Cleanup

### Pitfall: Not Disposing Observers and Driver

**The Problem:**

Event observers and the driver hold resources that should be properly disposed.

[!code-csharp[Bad Cleanup](../code/common-pitfalls/CommonPitfallsSamples.cs#BadCleanup)]

**The Solution:**

Always clean up resources:

[!code-csharp[Good Cleanup](../code/common-pitfalls/CommonPitfallsSamples.cs#GoodCleanup)]

[!code-csharp[Better Cleanup](../code/common-pitfalls/CommonPitfallsSamples.cs#BetterCleanup)]

**Key Takeaway:** Use `try-finally` blocks or `using`/`await using` statements to ensure proper cleanup of observers and the driver.

---

## Summary Checklist

Before running your WebDriverBiDi.NET code, verify:

> **Tip:** Add the [WebDriverBiDi.Analyzers](advanced/analyzers.md) package to get compile-time diagnostics for many of these pitfalls.

- [ ] Event handlers use `RunHandlerAsynchronously` for I/O operations
- [ ] You've called both `AddObserver()` AND `Session.SubscribeAsync()`
- [ ] Modules and observers registered BEFORE `StartAsync()`
- [ ] Nullable collections checked for null before adding items
- [ ] Timeout configured appropriately for your use case
- [ ] Async handlers synchronized with capture API if needed
- [ ] Transport error behavior set for development/production
- [ ] Using correct WebSocket URL format (`ws://`)
- [ ] Thread safety considered for concurrent operations
- [ ] Resources properly cleaned up with try-finally or using statements

**Tip:** Add the [WebDriverBiDi.Analyzers](advanced/analyzers.md) package to get compile-time diagnostics for many of these pitfalls.

---

## See Also

- [Roslyn Analyzers](advanced/analyzers.md): Compile-time diagnostics for common pitfalls
- [Error Handling](advanced/error-handling.md): Troubleshooting, timeout patterns, TransportErrorBehavior
- [API Design Guide](advanced/api-design.md): Timeout and cancellation patterns

## Next Steps

- [Events and Observables](events-observables.md): Deep dive into event handling
- [Error Handling](advanced/error-handling.md): Comprehensive error management strategies
- [Core Concepts](core-concepts.md): Understanding the fundamentals
- [Architecture](architecture.md): System design and patterns
