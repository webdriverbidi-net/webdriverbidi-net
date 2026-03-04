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

### Complete Example

```csharp
// Step 1: Add observer
driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    Console.WriteLine($"Console: {e.Text}");
});

// Step 2: Subscribe to events
SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.Log.OnEntryAdded.EventName);
await driver.Session.SubscribeAsync(subscribe);

// Step 3: Events will now trigger your observer
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com"));
```

## Observable Events

Each event is exposed as an `ObservableEvent<TEventArgs>` property on the relevant module.

### Available Events by Module

#### BrowsingContext Module

```csharp
driver.BrowsingContext.OnLoad                    // Page load complete
driver.BrowsingContext.OnDomContentLoaded        // DOM ready
driver.BrowsingContext.OnNavigationStarted       // Navigation begins
driver.BrowsingContext.OnNavigationAborted       // Navigation cancelled
driver.BrowsingContext.OnNavigationFailed        // Navigation error
driver.BrowsingContext.OnFragmentNavigated       // Hash navigation
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

```csharp
Console.WriteLine(driver.Log.OnEntryAdded.EventName);
// Output: "log.entryAdded"

Console.WriteLine(driver.Network.OnBeforeRequestSent.EventName);
// Output: "network.beforeRequestSent"
```

## Adding Observers

Observers are functions that get called when an event occurs.

### Simple Observer

```csharp
driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    Console.WriteLine($"Level: {e.Level}");
    Console.WriteLine($"Text: {e.Text}");
    Console.WriteLine($"Timestamp: {e.Timestamp}");
});
```

### Observer with Type Inference

```csharp
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    // Type is inferred as EntryAddedEventArgs
    Console.WriteLine(e.Text);
});
```

### Async Observer

For long-running or async operations in handlers:

```csharp
driver.Network.OnBeforeRequestSent.AddObserver(
    async (BeforeRequestSentEventArgs e) =>
    {
        // Can use await
        await LogRequestAsync(e.Request.Url);
        await Task.Delay(100);
    },
    ObservableEventHandlerOptions.RunHandlerAsynchronously
);
```

### Storing Observer Reference

Store the `EventObserver<T>` to control it later:

```csharp
EventObserver<EntryAddedEventArgs> observer = 
    driver.Log.OnEntryAdded.AddObserver((e) =>
    {
        Console.WriteLine(e.Text);
    });

// Later: remove the observer
observer.Unobserve();
```

## Subscribing to Events

Before events are sent by the browser, you must subscribe to them.

### Basic Subscription

```csharp
SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add("log.entryAdded");
subscribe.Events.Add("network.responseCompleted");

SubscribeCommandResult result = await driver.Session.SubscribeAsync(subscribe);
Console.WriteLine($"Subscription ID: {result.SubscriptionId}");
```

### Using Event Names

Use the `EventName` property for type safety:

```csharp
SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.Log.OnEntryAdded.EventName);
subscribe.Events.Add(driver.Network.OnResponseCompleted.EventName);

await driver.Session.SubscribeAsync(subscribe);
```

### Subscription Scope

You can limit subscriptions to specific contexts:

```csharp
SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.Network.OnBeforeRequestSent.EventName);

// Only receive events for this specific context
subscribe.BrowsingContextIds.Add(contextId);

await driver.Session.SubscribeAsync(subscribe);
```

### Unsubscribing

```csharp
// Unsubscribe by subscription ID
UnsubscribeByIdsCommandParameters unsubscribe = 
    new UnsubscribeByIdsCommandParameters();
unsubscribe.SubscriptionIds.Add(subscriptionId);
await driver.Session.UnsubscribeAsync(unsubscribe);

// Or unsubscribe by event names
UnsubscribeByNamesCommandParameters unsubscribe = 
    new UnsubscribeByNamesCommandParameters();
unsubscribe.Events.Add(driver.Log.OnEntryAdded.EventName);
await driver.Session.UnsubscribeAsync(unsubscribe);
```

## Event Synchronization

The `EventObserver<T>` class provides checkpoints for synchronizing with events.

### Waiting for a Single Event

```csharp
EventObserver<NavigationEventArgs> observer = 
    driver.BrowsingContext.OnLoad.AddObserver((e) =>
    {
        Console.WriteLine($"Loaded: {e.Url}");
    });

// Set checkpoint for 1 event (default)
observer.SetCheckpoint();

// Trigger navigation
await driver.BrowsingContext.NavigateAsync(params);

// Wait up to 10 seconds for the event
bool eventOccurred = observer.WaitForCheckpoint(TimeSpan.FromSeconds(10));

if (eventOccurred)
{
    Console.WriteLine("Page loaded!");
}
else
{
    Console.WriteLine("Timeout waiting for page load");
}
```

### Waiting for Multiple Events

```csharp
EventObserver<ResponseCompletedEventArgs> observer = 
    driver.Network.OnResponseCompleted.AddObserver((e) =>
    {
        Console.WriteLine($"Response: {e.Response.Url}");
    });

// Wait for 5 network responses
observer.SetCheckpoint(5);

await driver.BrowsingContext.NavigateAsync(params);

// Wait for all 5 responses
bool allReceived = observer.WaitForCheckpoint(TimeSpan.FromSeconds(10));
Console.WriteLine($"Received all 5 responses: {allReceived}");
```

### Checkpoint Reset

```csharp
EventObserver<EntryAddedEventArgs> observer = 
    driver.Log.OnEntryAdded.AddObserver((e) => { });

// First navigation
observer.SetCheckpoint(3);
await driver.BrowsingContext.NavigateAsync(params1);
observer.WaitForCheckpoint(TimeSpan.FromSeconds(5));

// Second navigation - reset checkpoint
observer.SetCheckpoint(2);
await driver.BrowsingContext.NavigateAsync(params2);
observer.WaitForCheckpoint(TimeSpan.FromSeconds(5));
```

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

```csharp
// Default behavior - runs synchronously
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    // This runs on the transport thread
    // Blocks all message processing until complete
    Console.WriteLine(e.Text);
});
```

**Use When:**
- Handler completes quickly (<10ms)
- Performing simple, in-memory operations (counters, collections)
- No I/O operations
- Order of execution matters

### The Blocking Problem

```csharp
// ❌ BAD: Handler blocks message processing
driver.Network.OnBeforeRequestSent.AddObserver((e) =>
{
    // This blocks the Transport thread for 5 seconds!
    Thread.Sleep(5000);
    Console.WriteLine($"Request: {e.Request.Url}");

    // During these 5 seconds:
    // - No other events are processed
    // - No responses are received
    // - Commands may timeout
    // - Browser may become unresponsive
});
```

### Asynchronous Handlers

Use `RunHandlerAsynchronously` for I/O operations or long-running work:

```csharp
// ✅ GOOD: Handler runs asynchronously
EventObserver<BeforeRequestSentEventArgs> observer =
    driver.Network.OnBeforeRequestSent.AddObserver(
        async (e) =>
        {
            // Doesn't block message processing
            await Task.Delay(5000);
            Console.WriteLine($"Request: {e.Request.Url}");

            // During these 5 seconds:
            // - Transport thread continues processing
            // - Other events are handled normally
            // - Handler runs on Task pool thread
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously
    );
```

**Use When:**
- Handler performs I/O (file, network, database)
- Handler does CPU-intensive work
- You need to call driver commands from the handler
- Handler might take more than a few milliseconds

### Practical Examples

#### Quick Operations (Synchronous)

```csharp
// Counter - quick in-memory operation
int requestCount = 0;
driver.Network.OnBeforeRequestSent.AddObserver((e) =>
{
    requestCount++;  // Fast, synchronous is fine
});

// List collection - quick in-memory operation
List<string> urls = new List<string>();
driver.Network.OnResponseCompleted.AddObserver((e) =>
{
    urls.Add(e.Response.Url);  // Quick, synchronous is fine
});
```

#### I/O Operations (Asynchronous)

```csharp
// File I/O - use async
driver.Log.OnEntryAdded.AddObserver(
    async (e) =>
    {
        await File.AppendAllTextAsync("log.txt", $"{e.Text}\n");
    },
    ObservableEventHandlerOptions.RunHandlerAsynchronously
);

// Database operations - use async
driver.Log.OnEntryAdded.AddObserver(
    async (e) =>
    {
        await dbContext.Logs.AddAsync(new LogEntry
        {
            Level = e.Level,
            Message = e.Text,
            Timestamp = e.Timestamp
        });
        await dbContext.SaveChangesAsync();
    },
    ObservableEventHandlerOptions.RunHandlerAsynchronously
);
```

### Synchronizing with Async Handlers

When handlers are async, you need to synchronize if you want to ensure they complete before continuing:

```csharp
EventObserver<BeforeRequestSentEventArgs> observer =
    driver.Network.OnBeforeRequestSent.AddObserver(
        async (e) =>
        {
            await ProcessRequestAsync(e);
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously
    );

// Set checkpoint for 3 events
observer.SetCheckpoint(3);

// Trigger events
await driver.BrowsingContext.NavigateAsync(params);

// Wait for events to occur
bool occurred = observer.WaitForCheckpoint(TimeSpan.FromSeconds(10));

if (occurred)
{
    // Wait for all async handlers to complete
    Task[] handlerTasks = observer.GetCheckpointTasks();
    await Task.WhenAll(handlerTasks);
    Console.WriteLine("All handlers completed");
}
```

### Using TaskCompletionSource for Complex Synchronization

For long-running handlers, use `TaskCompletionSource` to track completion:

```csharp
List<Task> handlerTasks = new();

EventObserver<BeforeRequestSentEventArgs> observer =
    driver.Network.OnBeforeRequestSent.AddObserver(
        async (e) =>
        {
            TaskCompletionSource taskCompletionSource = new();
            handlerTasks.Add(taskCompletionSource.Task);

            try
            {
                Console.WriteLine($"Processing request: {e.Request.Url}");

                // Long-running operation
                await Task.Delay(TimeSpan.FromSeconds(4));
                await ProcessRequestAsync(e);

                Console.WriteLine($"Completed request: {e.Request.Url}");
                taskCompletionSource.SetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handler error: {ex.Message}");
                taskCompletionSource.SetException(ex);
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously
    );

// Subscribe to events
SubscribeCommandParameters subscribe = new();
subscribe.Events.Add(driver.Network.OnBeforeRequestSent.EventName);
await driver.Session.SubscribeAsync(subscribe);

// Set checkpoint for expected number of events (e.g., 5 requests)
observer.SetCheckpoint(5);

// Trigger navigation
NavigateCommandParameters navParams = new(contextId, "https://example.com")
{
    Wait = ReadinessState.Complete
};
NavigateCommandResult navigation = await driver.BrowsingContext.NavigateAsync(navParams);
Console.WriteLine("Navigation command completed");

// Important: The navigation command completes before handlers finish
// Wait for all events to occur
observer.WaitForCheckpoint(TimeSpan.FromSeconds(10));

// Wait for all async handlers to complete
Task.WaitAll(handlerTasks.ToArray());
Console.WriteLine("All event handlers completed");
```

**Why This Matters:**

Without synchronization, your main code might exit before async handlers complete:

```csharp
// ❌ PROBLEM: Main thread may exit before handlers complete
EventObserver<BeforeRequestSentEventArgs> observer =
    driver.Network.OnBeforeRequestSent.AddObserver(
        async (e) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5));  // Long operation
            Console.WriteLine($"Request: {e.Request.Url}");
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously
    );

observer.SetCheckpoint(5);
await driver.BrowsingContext.NavigateAsync(params);

// Navigation completes...
// Main code continues...
// Application might exit before handlers finish!

// ✅ SOLUTION: Use TaskCompletionSource and wait
List<Task> handlerTasks = new();

EventObserver<BeforeRequestSentEventArgs> observer =
    driver.Network.OnBeforeRequestSent.AddObserver(
        async (e) =>
        {
            TaskCompletionSource tcs = new();
            handlerTasks.Add(tcs.Task);

            await Task.Delay(TimeSpan.FromSeconds(5));
            Console.WriteLine($"Request: {e.Request.Url}");

            tcs.SetResult();
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously
    );

observer.SetCheckpoint(5);
await driver.BrowsingContext.NavigateAsync(params);
observer.WaitForCheckpoint(TimeSpan.FromSeconds(10));

// Wait for all handlers to complete before continuing
Task.WaitAll(handlerTasks.ToArray());
```

### Calling Commands in Event Handlers

Calling commands within event handlers requires async mode:

```csharp
EventObserver<BeforeRequestSentEventArgs> observer = 
    driver.Network.OnBeforeRequestSent.AddObserver(
        async (e) =>
        {
            if (e.IsBlocked)
            {
                // Can call commands in async handler
                ProvideResponseCommandParameters provideResponse = 
                    new ProvideResponseCommandParameters(e.Request.RequestId)
                    {
                        StatusCode = 404,
                        ReasonPhrase = "Not Found"
                    };
                
                await driver.Network.ProvideResponseAsync(provideResponse);
            }
        },
        ObservableEventHandlerOptions.RunHandlerAsynchronously
    );
```

## Event Filtering

You can filter events in your observer:

```csharp
// Only log errors
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    if (e.Level == LogLevel.Error)
    {
        Console.WriteLine($"ERROR: {e.Text}");
    }
});

// Only log HTML requests
driver.Network.OnResponseCompleted.AddObserver((e) =>
{
    if (e.Response.Url.EndsWith(".html"))
    {
        Console.WriteLine($"HTML page: {e.Response.Url}");
    }
});
```

## Multiple Observers

You can add multiple observers for the same event:

```csharp
// Observer 1: Log to console
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    Console.WriteLine($"Console: {e.Text}");
});

// Observer 2: Write to file
EventObserver<EntryAddedEventArgs> fileLogger = 
    driver.Log.OnEntryAdded.AddObserver(async (e) =>
    {
        await File.AppendAllTextAsync("log.txt", e.Text + "\n");
    },
    ObservableEventHandlerOptions.RunHandlerAsynchronously);

// Observer 3: Count errors
int errorCount = 0;
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    if (e.Level == LogLevel.Error)
    {
        errorCount++;
    }
});
```

## Common Patterns

### Pattern 1: Wait for Page Load

```csharp
EventObserver<NavigationEventArgs> observer = 
    driver.BrowsingContext.OnLoad.AddObserver((e) => { });

observer.SetCheckpoint();
await driver.BrowsingContext.NavigateAsync(params);
observer.WaitForCheckpoint(TimeSpan.FromSeconds(30));
```

### Pattern 2: Collect Network Responses

```csharp
List<ResponseData> responses = new List<ResponseData>();

driver.Network.OnResponseCompleted.AddObserver((e) =>
{
    responses.Add(e.Response);
});

SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.Network.OnResponseCompleted.EventName);
await driver.Session.SubscribeAsync(subscribe);

await driver.BrowsingContext.NavigateAsync(params);

// Wait a bit for all responses
await Task.Delay(2000);

Console.WriteLine($"Collected {responses.Count} responses");
```

### Pattern 3: Wait for Specific Condition

```csharp
TaskCompletionSource<RemoteValue> elementFound = 
    new TaskCompletionSource<RemoteValue>();

driver.Script.OnMessage.AddObserver((e) =>
{
    if (e.ChannelId == "elementWatcher")
    {
        elementFound.SetResult(e.Data);
    }
});

// Preload script watches for element
string preloadScript = @"
(channel) => {
    const interval = setInterval(() => {
        const element = document.querySelector('.target');
        if (element) {
            clearInterval(interval);
            channel(element);
        }
    }, 100);
}";

ChannelValue channel = new ChannelValue(
    new ChannelProperties("elementWatcher"));

AddPreloadScriptCommandParameters preloadParams = 
    new AddPreloadScriptCommandParameters(preloadScript)
    {
        Arguments = new List<ArgumentValue> { channel }
    };

await driver.Script.AddPreloadScriptAsync(preloadParams);
await driver.BrowsingContext.NavigateAsync(navParams);

// Wait for element to appear
RemoteValue element = await elementFound.Task;
Console.WriteLine($"Element found: {element.SharedId}");
```

### Pattern 4: Temporary Observer

```csharp
// Add observer just for one operation
EventObserver<EntryAddedEventArgs> observer = 
    driver.Log.OnEntryAdded.AddObserver((e) =>
    {
        Console.WriteLine(e.Text);
    });

// Do something
await driver.BrowsingContext.NavigateAsync(params);

// Remove observer
observer.Unobserve();
```

## Event Args Properties

Each event type has specific properties:

### NavigationEventArgs

```csharp
driver.BrowsingContext.OnLoad.AddObserver((NavigationEventArgs e) =>
{
    string contextId = e.BrowsingContextId;
    string navigationId = e.NavigationId;
    string url = e.Url;
    DateTime timestamp = e.Timestamp;
});
```

### EntryAddedEventArgs

```csharp
driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    LogLevel level = e.Level;        // Error, Warn, Info, Debug
    string text = e.Text;            // Log message
    DateTime timestamp = e.Timestamp;
    string? source = e.Source;       // JavaScript source location
    string? stackTrace = e.StackTrace;
});
```

### BeforeRequestSentEventArgs

```csharp
driver.Network.OnBeforeRequestSent.AddObserver((e) =>
{
    string requestId = e.Request.RequestId;
    string url = e.Request.Url;
    string method = e.Request.Method;
    List<Header> headers = e.Request.Headers;
    bool isBlocked = e.IsBlocked;    // True if intercepted
    string contextId = e.Context.BrowsingContextId;
});
```

### ResponseCompletedEventArgs

```csharp
driver.Network.OnResponseCompleted.AddObserver((e) =>
{
    RequestData request = e.Request;
    ResponseData response = e.Response;
    
    string url = response.Url;
    ulong status = response.Status;
    string statusText = response.StatusText;
    List<Header> headers = response.Headers;
});
```

## Best Practices

### 1. Subscribe Before Adding Observers

```csharp
// ✓ Good: Subscribe first
await driver.Session.SubscribeAsync(subscribe);
driver.Log.OnEntryAdded.AddObserver(handler);

// ✗ May miss early events
driver.Log.OnEntryAdded.AddObserver(handler);
await driver.Session.SubscribeAsync(subscribe);
```

### 2. Remove Observers When Done

```csharp
EventObserver<EntryAddedEventArgs> observer = 
    driver.Log.OnEntryAdded.AddObserver(handler);

try
{
    // Use observer
}
finally
{
    observer.Unobserve();
}
```

### 3. Use Async Mode for Long Operations

```csharp
// ✓ Good: Won't block message processing
driver.Network.OnBeforeRequestSent.AddObserver(
    async (e) => await SlowOperationAsync(e),
    ObservableEventHandlerOptions.RunHandlerAsynchronously
);
```

### 4. Handle Exceptions in Observers

```csharp
driver.Log.OnEntryAdded.AddObserver((e) =>
{
    try
    {
        ProcessLogEntry(e);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Observer error: {ex.Message}");
    }
});
```

## Next Steps

- [Module Guides](modules/browser.md): Learn what events each module provides
- [Network Interception Example](examples/network-interception.md): Practical event usage
- [Preload Scripts Example](examples/preload-scripts.md): Using script.message events

## Summary

- Events require subscription through Session module
- Observers handle events when they occur
- Use checkpoints to synchronize with events
- Async handlers for long-running operations
- Remove observers to prevent memory leaks
- Multiple observers can handle the same event

