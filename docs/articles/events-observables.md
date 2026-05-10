# Events and Observables

WebDriver BiDi is an event-driven protocol. This guide explains how to work with events and the observable pattern in WebDriverBiDi.NET.

## Overview

Browser events allow you to react to things happening in the browser in real-time:

- Navigation events (page loads, redirects)
- Network events (requests, responses)
- Console log messages
- User context creation/destruction
- DOM mutations
- And more...

## The Three-Step Process

Working with events involves three steps:

1. **Add an observer or data collector** to handle or accumulate the event
2. **Subscribe to the event** through the Session module
3. **Wait for events** or let them trigger as they occur

**Important**: Steps 1 and 2 are separate by design to prevent race conditions. Adding an observer or data collector registers your handler locally; subscribing tells the browser to send events. The recommended order is to add observers/collectors first (step 1), then subscribe (step 2).

### Complete Example

[!code-csharp[Complete Example](../code/events-observables/EventObserverSamples.cs#CompleteExample)]

## Observable Events

Each event is exposed as an `ObservableEvent<TEventArgs>` property on the relevant module.

### Available Events by Module

#### BrowsingContext Module

[!code-csharp[BrowsingContext Events](../code/events-observables/EventObserverSamples.cs#BrowsingContextEvents)]

```csharp
driver.BrowsingContext.OnLoad                    // Page load complete
driver.BrowsingContext.OnDomContentLoaded        // DOM ready
driver.BrowsingContext.OnNavigationStarted       // Navigation begins
driver.BrowsingContext.OnNavigationCommitted     // Navigation committed
driver.BrowsingContext.OnNavigationAborted       // Navigation cancelled
driver.BrowsingContext.OnNavigationFailed        // Navigation error
driver.BrowsingContext.OnFragmentNavigated       // Hash navigation
driver.BrowsingContext.OnHistoryUpdated          // History entry updated
driver.BrowsingContext.OnDownloadWillBegin       // Download about to begin
driver.BrowsingContext.OnDownloadEnd             // Download completed
driver.BrowsingContext.OnContextCreated          // New tab/window/iframe
driver.BrowsingContext.OnContextDestroyed        // Tab/window closed
driver.BrowsingContext.OnUserPromptOpened        // Alert/confirm/prompt
driver.BrowsingContext.OnUserPromptClosed        // Dialog closed
```

#### Network Module

[!code-csharp[Network Events](../code/events-observables/EventObserverSamples.cs#NetworkEvents)]

```csharp
driver.Network.OnBeforeRequestSent     // Request about to be sent
driver.Network.OnResponseStarted       // Response headers received
driver.Network.OnResponseCompleted     // Response fully received
driver.Network.OnFetchError            // Network error occurred
driver.Network.OnAuthRequired          // Authentication needed
```

#### Log Module

[!code-csharp[Log Events](../code/events-observables/EventObserverSamples.cs#LogEvents)]

```csharp
driver.Log.OnEntryAdded               // Console log message
```

#### Script Module

[!code-csharp[Script Events](../code/events-observables/EventObserverSamples.cs#ScriptEvents)]

```csharp
driver.Script.OnMessage               // Message from preload script
driver.Script.OnRealmCreated          // New execution realm
driver.Script.OnRealmDestroyed        // Realm destroyed
```

#### Session Module

```csharp
// Session module has no observable events
```

### Driver-Level Observable Events

In addition to the module events above, `BiDiDriver` exposes five observable events that reflect the
library's own communication layer. These events do **not** correspond to WebDriver BiDi protocol events
and do **not** require a `session.SubscribeAsync` call — they fire whenever the transport or driver
itself raises the underlying condition.

[!code-csharp[Driver-Level Events Listing](../code/events-observables/EventObserverSamples.cs#DriverLevelEventsListing)]

These events are also available through the `IBiDiDriverEvents` interface, which means they can be
observed on any object that implements the interface.

#### OnEventReceived

Fires once for every protocol event message that the transport delivers to the driver, **before** the
event is dispatched to the relevant module observer. This is useful for protocol-level logging, auditing,
or routing custom module events.

[!code-csharp[OnEventReceived](../code/events-observables/EventObserverSamples.cs#OnEventReceived)]

The `EventName` property contains the full protocol event name (e.g., `"log.entryAdded"`). `EventData`
contains the deserialized event payload, whose concrete type depends on the event. For events the driver
does not recognize, `EventData` will be `null`.

#### OnUnexpectedErrorReceived

Fires when the browser sends an error response that does not correspond to any pending command — for
example, a spontaneous error message. This is distinct from command errors, which are surfaced as
exceptions from `ExecuteCommandAsync`.

[!code-csharp[OnUnexpectedErrorReceived](../code/events-observables/EventObserverSamples.cs#OnUnexpectedErrorReceived)]

The `ErrorData` property is an `ErrorResult` containing `ErrorCode`, `ErrorMessage`, and an optional
`StackTrace`. Whether this event causes the driver to throw depends on `UnexpectedErrorBehavior`
(default: `TransportErrorBehavior.Ignore`).

#### OnUnknownMessageReceived

Fires when the transport receives a message that is valid JSON but does not match any recognized protocol
structure — neither a command response, an error response, nor a known event. The raw JSON string is
available in `Message`.

[!code-csharp[OnUnknownMessageReceived](../code/events-observables/EventObserverSamples.cs#OnUnknownMessageReceived)]

Whether this event causes the driver to throw depends on `UnknownMessageBehavior` (default:
`TransportErrorBehavior.Ignore`).

#### OnEventHandlerErrorOccurred

Fires when an exception is thrown inside any observer registered on any `ObservableEvent<T>` in the
driver — including module-level events. This is a cross-cutting diagnostic hook; it fires in addition to
(not instead of) the normal error-behavior flow controlled by `EventHandlerExceptionBehavior`.

[!code-csharp[OnEventHandlerErrorOccurred](../code/events-observables/EventObserverSamples.cs#OnEventHandlerErrorOccurred)]

The `ErrorInfo` property is an `EventObserverErrorInfo` record with the following fields:

| Property | Description |
|---|---|
| `ObservableEventName` | The event name whose observer faulted (e.g., `"log.entryAdded"`) |
| `ObserverId` | A unique string identifier for the observer instance |
| `ObserverDescription` | A human-readable description of the observer |
| `Exception` | The exception thrown by the observer |
| `IsAsynchronousHandler` | `true` if the observer was registered with `RunHandlerAsynchronously` |
| `FaultOccurredAfterHandlerReturned` | `true` for async handlers that faulted after their `Task` was returned |

#### OnLogMessage

Fires when the library itself emits a diagnostic log message. These are library-internal messages (e.g.,
"connecting to transport", "disposing driver"), not browser console messages. Use this event to route
library diagnostics to your own logging infrastructure.

[!code-csharp[OnLogMessage](../code/events-observables/EventObserverSamples.cs#OnLogMessage)]

`Level` is a `WebDriverBiDiLogLevel` value (`Trace`, `Debug`, `Info`, `Warn`, `Error`, `Fatal`).
`ComponentName` identifies the part of the library that emitted the message. `Timestamp` is set to
`DateTime.UtcNow` at the time the message was created.

> **Note:** For browser console log messages, use `driver.Log.OnEntryAdded` (a module-level event that
> requires `session.SubscribeAsync`). `OnLogMessage` is for library diagnostics only.

### Event Names

Each observable event has an `EventName` property with the protocol event name:

[!code-csharp[Event Names](../code/events-observables/EventObserverSamples.cs#EventNames)]

## Collecting Event Data

`EventDataCollector<T>` is an alternative to observers for scenarios where you want to accumulate event data and examine it at a convenient point, rather than reacting to each event as it arrives. It is created from any `ObservableEvent<T>` via `AddDataCollector()` and counts as one observer.

### When to Use a Data Collector

Use a data collector instead of an observer when:

- You want to **inspect results after an operation** rather than reacting event-by-event
- You are writing **test assertions** against events that occurred during a step
- You need a **per-step snapshot**: drain before an action, do the action, drain again — each drain contains only that step's events
- You don't need to run code on every individual event — just need the list when you're done

Use an observer when you need to **react immediately** to each event (e.g., intercept a network request, log to a stream, abort on an error condition).

### Basic Usage

[!code-csharp[Basic Data Collector](../code/events-observables/EventObserverSamples.cs#BasicDataCollector)]

### Drain and Reset Pattern

`GetCollectedEventData()` atomically drains the internal queue and returns all accumulated events as a read-only list. Events that arrive after the call will appear in the next drain. This makes it straightforward to isolate events from individual steps:

[!code-csharp[Data Collector Drain Pattern](../code/events-observables/EventObserverSamples.cs#DataCollectorDrainPattern)]

### Filtering Collected Events

Pass a predicate to `AddDataCollector` to discard events at collection time rather than filtering the
drained list after the fact. Only events for which the predicate returns `true` are enqueued; all others
are silently dropped and never appear in `GetCollectedEventData()`.

[!code-csharp[Data Collector Filtering](../code/events-observables/EventObserverSamples.cs#DataCollectorFiltering)]

Filtering at the collector level keeps the queue small and eliminates the need for a post-drain `Where`
call on the result. If no filter is provided the collector behaves as before, accumulating every event.

### Cleanup

`EventDataCollector<T>` implements `IDisposable` and `IAsyncDisposable`. Use `await using` to ensure the collector is automatically removed from the event when the scope exits:

[!code-csharp[Data Collector Cleanup](../code/events-observables/EventObserverSamples.cs#DataCollectorCleanup)]

Always dispose the collector when you no longer need it. A collector that is never disposed continues to receive and queue events, which is a memory leak.

### Data Collector vs Observer

[!code-csharp[Data Collector vs Observer](../code/events-observables/EventObserverSamples.cs#DataCollectorVsObserver)]

| | Observer | Data Collector |
|---|---|---|
| **Runs code on each event** | Yes — your handler runs immediately | No — events are queued |
| **Access event data later** | Only if you capture it yourself | Yes — `GetCollectedEventData()` |
| **Per-step isolation** | Manual (clear a list yourself) | Built-in (each drain is independent) |
| **Built-in filtering** | Manual (if-check inside handler) | Yes — predicate passed to `AddDataCollector` |
| **Thread safety** | Handler options control execution | Internally locked; always safe |
| **Cleanup** | `Unobserve()` / `using` / `DisposeAsync()` | `using` / `await using` / `DisposeAsync()` |

## Adding Observers

Observers are functions that get called when an event occurs.

### Simple Observer

[!code-csharp[Simple Observer](../code/events-observables/EventObserverSamples.cs#SimpleObserver)]

### Observer with Type Inference

[!code-csharp[Observer with Type Inference](../code/events-observables/EventObserverSamples.cs#ObserverwithTypeInference)]

### Async Observer

For long-running or async operations in handlers:

[!code-csharp[Async Observer](../code/events-observables/EventObserverSamples.cs#AsyncObserver)]

### EventObserver Cleanup Pattern

`AddObserver` returns an `EventObserver<T>` that you should store when you need to:

- **Remove the observer** when it is no longer needed (`Unobserve()`)
- **Use the capture API** for synchronization (`StartCapturingTasks()`, `WaitForCapturedTasksAsync()`, `WaitForCapturedTasksCompleteAsync()`)
- **Dispose resources** when the observer goes out of scope

Always store the observer reference when you intend to remove it or use the capture API. Failing to remove observers when done can lead to memory leaks and handlers continuing to run after they are no longer needed.

#### Basic Cleanup with try/finally

[!code-csharp[Basic Cleanup try/finally](../code/events-observables/EventObserverSamples.cs#BasicCleanuptry-finally)]

#### Using Statement for Automatic Cleanup

`EventObserver<T>` implements `IDisposable`, so you can use `using` for automatic cleanup:

[!code-csharp[Using Statement Cleanup](../code/events-observables/EventObserverSamples.cs#UsingStatementCleanup)]

#### Cleanup When Using the Capture API

When using the capture API, you must store the observer to call `StartCapturingTasks()`, `WaitForCapturedTasksAsync()`, or `WaitForCapturedTasksCompleteAsync()`. Clean up the observer when you are done:

[!code-csharp[Cleanup with Capture](../code/events-observables/EventObserverSamples.cs#CleanupwithCapture)]

#### Unobserve vs Dispose

`Unobserve()` removes the observer from the event. `Dispose()` (and `DisposeAsync()`) does the same and also releases internal resources. For most scenarios, either is sufficient. Use `Unobserve()` when you only need to stop receiving events; use `using` with `Dispose()` when you want automatic cleanup at scope exit.

## Subscribing to Events

Before events are sent by the browser, you must subscribe to them.

### Basic Subscription

Prefer the `EventName` property from observable events to avoid typos and stay in sync with the API:

[!code-csharp[Basic Subscription](../code/events-observables/SubscribeSamples.cs#BasicSubscription)]

### Single Event Subscription

For a single event, use the constructor that accepts one event name:

[!code-csharp[Single Event Subscription](../code/events-observables/SubscribeSamples.cs#SingleEventSubscription)]

### Subscription Scope

You can limit subscriptions to specific contexts:

[!code-csharp[Subscription with Context](../code/events-observables/SubscribeSamples.cs#SubscriptionwithContext)]

### Unsubscribing

[!code-csharp[Unsubscribe by ID](../code/events-observables/SubscribeSamples.cs#UnsubscribebyID)]

[!code-csharp[Unsubscribe by Event Names](../code/events-observables/SubscribeSamples.cs#UnsubscribebyEventNames)]

## Event Synchronization

The `EventObserver<T>` class provides a capture API for synchronizing with events.

### Waiting for a Single Event

[!code-csharp[Wait for Single Event](../code/events-observables/EventSynchronizationSamples.cs#WaitforSingleEvent)]

### Waiting for Multiple Events

[!code-csharp[Wait for Multiple Events](../code/events-observables/EventSynchronizationSamples.cs#WaitforMultipleEvents)]

### Restarting a Capture Session

[!code-csharp[Capture Session Restart](../code/events-observables/EventObserverSamples.cs#CaptureSessionRestart)]

### Capture API Thread Safety

Capture API methods are thread-safe. Concurrent calls to `WaitForCapturedTasksAsync` or `GetCapturedTasks` are
serialized internally — each caller gets a contiguous, non-interleaved slice of captured tasks.
`StartCapturingTasks`, `StopCapturingTasks`, and the observer's notification path are all safe to call from
any thread.

> **Note for async callers:** `GetCapturedTasks()` is a synchronous method that acquires an internal reader
> lock. If a concurrent `WaitForCapturedTasksAsync` call holds the lock, `GetCapturedTasks()` will block the
> calling thread until it is released. In async contexts — particularly those with a single-threaded
> `SynchronizationContext`, such as WPF or legacy ASP.NET — prefer `WaitForCapturedTasksAsync` to avoid
> blocking the calling thread.

Only one capture session may be active at a time. Calling `StartCapturingTasks` when a session is already
active throws `WebDriverBiDiException`.

## Async Event Handlers

When event handlers perform async operations or I/O, you must use asynchronous handler execution to avoid blocking the transport thread.

### Observable Event Handler Options

The `ObservableEventHandlerOptions` enum controls how event handlers execute:

```csharp
public enum ObservableEventHandlerOptions
{
    RunHandlerSynchronously = 0,  // Synchronous execution (default)
    RunHandlerAsynchronously = 1   // Asynchronous execution
}
```

### Synchronous Handlers (Default)

By default, event handlers run synchronously on the transport thread:

[!code-csharp[Synchronous Handlers](../code/events-observables/EventObserverSamples.cs#SynchronousHandlers)]

**Use When:**
- Handler completes quickly (<10ms)
- Performing simple, in-memory operations (counters, collections)
- No I/O operations
- Order of execution matters

### The Blocking Problem

[!code-csharp[Blocking Problem](../code/events-observables/EventObserverSamples.cs#BlockingProblem)]

### Asynchronous Handlers

Use `RunHandlerAsynchronously` for I/O operations or long-running work:

[!code-csharp[Asynchronous Handlers](../code/events-observables/EventObserverSamples.cs#AsynchronousHandlers)]

**Use When:**
- Handler performs I/O (file, network, database)
- Handler does CPU-intensive work
- You need to call driver commands from the handler
- Handler might take more than a few milliseconds

### Practical Examples

#### Quick Operations (Synchronous)

[!code-csharp[Quick Operations](../code/events-observables/EventObserverSamples.cs#QuickOperations)]

#### I/O Operations (Asynchronous)

[!code-csharp[I/O Operations](../code/events-observables/EventObserverSamples.cs#IOOperations)]

### Synchronizing with Async Handlers

When handlers are async, you need to synchronize if you want to ensure they complete before continuing.

#### Using WaitForCapturedTasksAsync (Recommended)

The simplest way is to use the built-in helper method:

[!code-csharp[WaitForCapturedTasksAsync](../code/events-observables/EventObserverSamples.cs#WaitForCapturedTasksAsync)]

This method waits for:
1. The requested number of events to arrive
2. All captured handler tasks to complete

**Note**: The timeout only applies to waiting for events to arrive. Handler execution time is not limited by the timeout.

**Important**: When you use `WaitForCapturedTasksCompleteAsync()`, exceptions from the captured async handler tasks are propagated through this method. Those exceptions are considered owned by the caller and are not surfaced again through transport-level `EventHandlerExceptionBehavior`.

#### Manual Synchronization (For Fine-Grained Control)

For scenarios where you need to inspect or manipulate tasks before waiting:

[!code-csharp[Manual Synchronization](../code/events-observables/EventObserverSamples.cs#ManualSynchronization)]

When using `WaitForCapturedTasksAsync()` followed by `Task.WhenAll()`, you take ownership of those tasks and their exceptions. This lets you inspect or await handler failures directly without having those same failures also re-surfaced through the transport's event handler error behavior.

### Waiting for Async Handlers to Complete

For long-running async handlers, use `WaitForCapturedTasksCompleteAsync` or `WaitForCapturedTasksAsync` with `Task.WhenAll`:

[!code-csharp[Wait For Async Handlers](../code/events-observables/EventObserverSamples.cs#WaitForAsyncHandlers)]

**Why This Matters:**

Without synchronization, your main code might exit before async handlers complete:

[!code-csharp[Without Synchronization Problem](../code/events-observables/EventObserverSamples.cs#WithoutSynchronizationProblem)]

### Calling Commands in Event Handlers

Calling commands within event handlers **requires** async mode:

[!code-csharp[Calling Commands in Handlers](../code/events-observables/EventObserverSamples.cs#CallingCommandsinHandlers)]

## Event Filtering

You can filter events in your observer:

[!code-csharp[Event Filtering](../code/events-observables/EventObserverSamples.cs#EventFiltering)]

## Multiple Observers

You can add multiple observers for the same event:

[!code-csharp[Multiple Observers](../code/events-observables/EventObserverSamples.cs#MultipleObservers)]

## Common Patterns

### Pattern 1: Wait for Page Load

[!code-csharp[Pattern 1: Wait for Page Load](../code/events-observables/EventObserverSamples.cs#Pattern1-WaitforPageLoad)]

### Pattern 2: Collect Network Responses

With a data collector, response accumulation requires no manual list or lock:

[!code-csharp[Pattern 2: Collect Network Responses](../code/events-observables/EventObserverSamples.cs#Pattern2-CollectNetworkResponsesWithCollector)]

The original observer-based approach (manually appending to a `List<T>` in a handler) still works but requires you to manage the list and its thread safety yourself. The data collector is the simpler choice when immediate per-event reaction is not required.

### Pattern 3: Wait for Specific Condition

[!code-csharp[Pattern 3: Wait for Specific Condition](../code/events-observables/EventObserverSamples.cs#Pattern3-WaitforSpecificCondition)]

### Pattern 4: Temporary Observer

[!code-csharp[Pattern 4: Temporary Observer](../code/events-observables/EventObserverSamples.cs#Pattern4-TemporaryObserver)]

## Event Args Properties

Each event type has specific properties:

### NavigationEventArgs

[!code-csharp[NavigationEventArgs](../code/events-observables/EventObserverSamples.cs#NavigationEventArgs)]

### EntryAddedEventArgs

[!code-csharp[EntryAddedEventArgs](../code/events-observables/EventObserverSamples.cs#EntryAddedEventArgs)]

### BeforeRequestSentEventArgs

[!code-csharp[BeforeRequestSentEventArgs](../code/events-observables/EventObserverSamples.cs#BeforeRequestSentEventArgs)]

### ResponseCompletedEventArgs

[!code-csharp[ResponseCompletedEventArgs](../code/events-observables/EventObserverSamples.cs#ResponseCompletedEventArgs)]

### BrowsingContext Navigation and Download Events

The BrowsingContext module provides additional events for navigation lifecycle, history, and downloads:

[!code-csharp[BrowsingContext Navigation and Download Events](../code/events-observables/EventObserverSamples.cs#BrowsingContextNavigationAndDownloadEvents)]

## Best Practices

### 1. Add Observers Before Subscribing

The recommended order is to add observers first, then subscribe through the Session module:

[!code-csharp[Add Observers Before Subscribing](../code/events-observables/EventObserverSamples.cs#AddObserversBeforeSubscribing)]

**Why Add Observers First?**

While both orders work, adding observers before subscribing ensures your handlers are ready before the browser starts sending events. This is especially important when:
- Setting up multiple observers
- The browser might send events immediately after subscription
- You want predictable initialization order

The two-step design (add observer + subscribe) is intentional to prevent race conditions where events arrive before handlers are registered.

### 2. Remove Observers When Done

[!code-csharp[Remove Observers When Done](../code/events-observables/EventObserverSamples.cs#RemoveObserversWhenDone)]

### 3. Use Async Mode for Long Operations

[!code-csharp[Use Async Mode for Long Operations](../code/events-observables/EventObserverSamples.cs#UseAsyncModeforLongOperations)]

### 4. Handle Exceptions in Observers

[!code-csharp[Handle Exceptions in Observers](../code/events-observables/EventObserverSamples.cs#HandleExceptionsinObservers)]

## Next Steps

- [Common Pitfalls](common-pitfalls.md): Avoid common mistakes with event handling
- [Module Guides](modules/browser.md): Learn what events each module provides
- [Network Interception Example](examples/network-interception.md): Practical event usage
- [Preload Scripts Example](examples/preload-scripts.md): Using script.message events

## Summary

- Events require two steps: add an observer or data collector locally, then subscribe through the Session module
- Recommended order: add observers/collectors first, then subscribe (ensures handlers are ready before events arrive)
- Use **observers** (`AddObserver`) to react to each event immediately as it occurs
- Use **data collectors** (`AddDataCollector`) to accumulate events and inspect them on demand — `GetCollectedEventData()` drains the queue atomically and resets it for the next interval; pass an optional filter predicate to `AddDataCollector` to discard unwanted events at collection time
- Store the observer returned by `AddObserver` when you need to remove it or use the capture API
- Use `await using` on `EventDataCollector<T>` for automatic cleanup; never leave a collector attached after you no longer need it
- Use try/finally or `using` to ensure observers are removed when done (prevents memory leaks)
- Use `StartCapturingTasks()`/`WaitForCapturedTasksAsync()` to synchronize with events — when `WaitForCapturedTasksAsync` returns a full batch it automatically ends the capture session; an explicit `StopCapturingTasks()` call is a no-op and safe to include for clarity
- Use `WaitForCapturedTasksCompleteAsync()` to wait for async handlers to complete — it also ends the capture session when the requested number of tasks is collected
- Use `RunHandlerAsynchronously` option for long-running operations or I/O
- Multiple observers and data collectors can observe the same event simultaneously

