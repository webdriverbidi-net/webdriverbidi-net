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

1. **Add an observer** to handle the event
2. **Subscribe to the event** through the Session module
3. **Wait for events** or let them trigger as they occur

**Important**: Steps 1 and 2 are separate by design to prevent race conditions. Adding an observer registers your handler locally; subscribing tells the browser to send events. The recommended order is to add observers first (step 1), then subscribe (step 2).

### Complete Example

[!code-csharp[Complete Example](../code/events-observables/EventObserverSamples.cs#CompleteExample)]

## Observable Events

Each event is exposed as an `ObservableEvent<TEventArgs>` property on the relevant module.

### Available Events by Module

#### BrowsingContext Module

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

```csharp
driver.Network.OnBeforeRequestSent     // Request about to be sent
driver.Network.OnResponseStarted       // Response headers received
driver.Network.OnResponseCompleted     // Response fully received
driver.Network.OnFetchError            // Network error occurred
driver.Network.OnAuthRequired          // Authentication needed
```

#### Log Module

```csharp
driver.Log.OnEntryAdded               // Console log message
```

#### Script Module

```csharp
driver.Script.OnMessage               // Message from preload script
driver.Script.OnRealmCreated          // New execution realm
driver.Script.OnRealmDestroyed        // Realm destroyed
```

#### Session Module

```csharp
// Session module has no observable events
```

### Event Names

Each observable event has an `EventName` property with the protocol event name:

[!code-csharp[Event Names](../code/events-observables/EventObserverSamples.cs#EventNames)]

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
- **Use checkpoints** for synchronization (`SetCheckpoint()`, `WaitForCheckpointAsync()`, `WaitForCheckpointAndTasksAsync()`)
- **Dispose resources** when the observer goes out of scope

Always store the observer reference when you intend to remove it or use checkpoints. Failing to remove observers when done can lead to memory leaks and handlers continuing to run after they are no longer needed.

#### Basic Cleanup with try/finally

[!code-csharp[Basic Cleanup try/finally](../code/events-observables/EventObserverSamples.cs#BasicCleanuptry-finally)]

#### Using Statement for Automatic Cleanup

`EventObserver<T>` implements `IDisposable`, so you can use `using` for automatic cleanup:

[!code-csharp[Using Statement Cleanup](../code/events-observables/EventObserverSamples.cs#UsingStatementCleanup)]

#### Cleanup When Using Checkpoints

When using checkpoints, you must store the observer to call `SetCheckpoint()`, `WaitForCheckpointAsync()`, or `WaitForCheckpointAndTasksAsync()`. Clean up the observer when you are done:

[!code-csharp[Cleanup with Checkpoints](../code/events-observables/EventObserverSamples.cs#CleanupwithCheckpoints)]

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

The `EventObserver<T>` class provides checkpoints for synchronizing with events.

### Waiting for a Single Event

[!code-csharp[Wait for Single Event](../code/events-observables/EventSynchronizationSamples.cs#WaitforSingleEvent)]

### Waiting for Multiple Events

[!code-csharp[Wait for Multiple Events](../code/events-observables/EventSynchronizationSamples.cs#WaitforMultipleEvents)]

### Checkpoint Reset

[!code-csharp[Checkpoint Reset](../code/events-observables/EventObserverSamples.cs#CheckpointReset)]

### Checkpoint Thread Safety

Checkpoint methods are thread-safe. You may:

- Call `WaitForCheckpointAsync` or `WaitForCheckpointAndTasksAsync` from multiple threads on the
same observer; all waiters complete when the checkpoint is fulfilled.
- Call `GetCheckpointTasks` or `UnsetCheckpoint` from any thread while another thread is waiting.

Only one checkpoint may be active at a time. Calling `SetCheckpoint` when a checkpoint is already set
(and not yet satisfied or unset) throws `WebDriverBiDiException`.

## Async Event Handlers

When event handlers perform async operations or I/O, you must use asynchronous handler execution to avoid blocking the transport thread.

### Observable Event Handler Options

The `ObservableEventHandlerOptions` enum controls how event handlers execute:

```csharp
[Flags]
public enum ObservableEventHandlerOptions
{
    None = 0,                      // Synchronous execution (default)
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

#### Using WaitForCheckpointAndTasksAsync (Recommended)

The simplest way is to use the built-in helper method:

[!code-csharp[WaitForCheckpointAndTasksAsync](../code/events-observables/EventObserverSamples.cs#WaitForCheckpointAndTasksAsync)]

This method waits for:
1. The checkpoint to be fulfilled (events occurred)
2. All handler tasks to complete

**Note**: The timeout only applies to waiting for the checkpoint. Handler execution time is not limited by the timeout.

**Important**: When you use `WaitForCheckpointAndTasksAsync()`, exceptions from the captured async handler tasks are propagated through this method. Those exceptions are considered owned by the caller and are not surfaced again through transport-level `EventHandlerExceptionBehavior`.

#### Manual Synchronization (For Fine-Grained Control)

For scenarios where you need to inspect or manipulate tasks before waiting:

[!code-csharp[Manual Synchronization](../code/events-observables/EventObserverSamples.cs#ManualSynchronization)]

When using `GetCheckpointTasks()`, you take ownership of those tasks and their exceptions. This lets you inspect or await handler failures directly without having those same failures also re-surfaced through the transport's event handler error behavior.

### Using TaskCompletionSource for Complex Synchronization

For long-running handlers, use `TaskCompletionSource` to track completion:

[!code-csharp[TaskCompletionSource Synchronization](../code/events-observables/EventObserverSamples.cs#TaskCompletionSourceSynchronization)]

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

[!code-csharp[Pattern 2: Collect Network Responses](../code/events-observables/EventObserverSamples.cs#Pattern2-CollectNetworkResponses)]

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

- Events require two steps: add observer locally, then subscribe through Session module
- Recommended order: add observers first, then subscribe (ensures handlers are ready)
- Observers handle events when they occur
- Store the observer returned by `AddObserver` when you need to remove it or use checkpoints
- Use try/finally or `using` to ensure observers are removed when done (prevents memory leaks)
- Use checkpoints to synchronize with events
- Use `WaitForCheckpointAndTasksAsync()` to wait for async handlers to complete
- Use `RunHandlerAsynchronously` option for long-running operations or I/O
- Multiple observers can handle the same event

