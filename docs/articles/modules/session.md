# Session Module

The Session module manages the WebDriver BiDi session and event subscriptions.

## Overview

The Session module provides:

- Session status checking
- Event subscription management
- Session capability negotiation

## Accessing the Module

```csharp
SessionModule session = driver.Session;
```

## Session Status

### Check Session Status

```csharp
StatusCommandParameters params = new StatusCommandParameters();
StatusCommandResult result = await driver.Session.StatusAsync(params);

Console.WriteLine($"Is ready: {result.IsReady}");
Console.WriteLine($"Message: {result.Message}");
```

## Event Subscription

### Subscribe to Events

```csharp
SubscribeCommandParameters params = new SubscribeCommandParameters();
params.Events.Add("log.entryAdded");
params.Events.Add("network.responseCompleted");
params.Events.Add("browsingContext.load");

SubscribeCommandResult result = await driver.Session.SubscribeAsync(params);
Console.WriteLine($"Subscription ID: {result.SubscriptionId}");
```

### Subscribe with Context Filter

```csharp
SubscribeCommandParameters params = new SubscribeCommandParameters();
params.Events.Add("network.beforeRequestSent");
params.BrowsingContextIds.Add(contextId);  // Only for this context

await driver.Session.SubscribeAsync(params);
```

### Unsubscribe by ID

```csharp
UnsubscribeByIdsCommandParameters params = new UnsubscribeByIdsCommandParameters();
params.SubscriptionIds.Add(subscriptionId);

await driver.Session.UnsubscribeAsync(params);
```

### Unsubscribe by Event Names

```csharp
UnsubscribeByNamesCommandParameters params = new UnsubscribeByNamesCommandParameters();
params.Events.Add("log.entryAdded");
params.Events.Add("network.responseCompleted");

await driver.Session.UnsubscribeAsync(params);
```

## Best Practices

1. **Subscribe early**: Subscribe to events before triggering actions that produce them
2. **Track subscription IDs**: Store IDs if you need to unsubscribe later
3. **Use context filtering**: Limit events to specific contexts when possible
4. **Clean up subscriptions**: Unsubscribe when events are no longer needed

## Next Steps

- [Events and Observables](../events-observables.md): Comprehensive event guide
- [Core Concepts](../core-concepts.md): Understanding subscriptions

