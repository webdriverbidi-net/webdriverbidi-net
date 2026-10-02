# Events and Observers

Each module exposes its events as `ObservableEvent<T>` properties named `On…`, such as `driver.BrowsingContext.OnLoad`, `driver.Network.OnResponseCompleted`, and `driver.Log.OnEntryAdded`. `EventName` gives the protocol name to subscribe with.

## Two Steps

1. `AddObserver(handler, options)` registers a handler locally and returns an `EventObserver<T>`.
2. `driver.Session.SubscribeAsync(new SubscribeCommandParameters(eventName, ...))` tells the browser to send the events. It can be scoped to browsing contexts or user contexts, and returns a subscription ID for `UnsubscribeAsync`.

Add the observer first, so that no event arrives before a handler exists.

## Where Handlers Run

Messages are read and dispatched on a single task. By default a handler runs synchronously on it, so:

- a handler that blocks delays every later message;
- a handler that awaits a command's response deadlocks, because the response is read by the same task.

`ObservableEventHandlerOptions.RunHandlerAsynchronously` detaches the `Task` a `Func<T, Task>` handler returns, and queues an `Action<T>` handler to the thread pool. It does not move where an `async` handler starts: the code before its first real `await` still runs on the reading task. Start such a handler with `await Task.Yield();`. The analyzers (BIDI007, BIDI023) flag handlers where the option cannot help.

Synchronous handlers are right for quick, non-blocking work, such as recording into a collection.

## Waiting for Handlers

A command completes when the browser answers it, not when the handlers of the events it caused finish. To wait:

- `observer.StartCapturingTasks()` before the action, then `await observer.WaitForCapturedTasksCompleteAsync(count, timeout)`, which returns whether `count` events arrived and their handlers finished in time; `StopCapturingTasks()` after. One capture session per observer at a time.
- `WaitForCapturedTasksAsync(count, timeout)` returns the handler tasks themselves, whose exceptions are observed there.

## Handler Exceptions

An exception thrown by a handler is, by default, ignored and reported through the driver's diagnostic events. Set `driver.TransportConfiguration.EventHandlerExceptionBehavior` to `Terminate` (the next command throws it) or `Collect` (`StopAsync` throws them all). In production, catch exceptions inside handlers.

## Cleanup

Dispose each `EventObserver<T>`, or call `Unobserve()`, when the handler is no longer needed. Observers can be added and removed at any time, from any thread.

## Data Collectors

To read response bodies, add a network data collector (`Network.AddDataCollectorAsync`) and fetch bodies with `Network.GetDataAsync`; observers alone do not keep bodies. More: https://webdriverbidi-net.github.io/webdriverbidi-net/articles/events-observables.html
