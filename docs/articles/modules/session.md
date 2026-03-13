# Session Module

The Session module manages the WebDriver BiDi session and event subscriptions.

## Overview

The Session module provides:

- Creating new BiDi sessions
- Session status checking
- Event subscription management
- Session capability negotiation

## Accessing the Module

[!code-csharp[Accessing Module](../../code/modules/SessionModuleSamples.cs#AccessingModule)]

## Timeout and Cancellation

All commands in this module accept optional `timeoutOverride` and `CancellationToken` parameters. Use `timeoutOverride` to set a per-command timeout (defaults to `BiDiDriver.DefaultCommandTimeout` when omitted). Use `CancellationToken` for cooperative cancellation. See the [API Design Guide](../advanced/api-design.md#timeout-and-cancellation) for details and examples.

## Creating a New Session

Use `NewSessionAsync` to create a WebDriver BiDi session when connecting to a browser that does not automatically provide one. This is typically needed when connecting directly to a browser via Chrome DevTools Protocol (CDP) or similar, without a classic WebDriver driver (e.g., chromedriver) that would have already established a session.

### Create New Session

Call `NewSessionAsync` after `StartAsync` when the browser requires explicit session creation:

[!code-csharp[New Session](../../code/modules/SessionModuleSamples.cs#NewSession)]

The result provides `SessionId` and `Capabilities` (browser name, version, platform, and other negotiated capabilities).

### Create Session with Capability Requests

Use `NewCommandParameters.Capabilities` to request specific session capabilities. Set `AlwaysMatch` for required capabilities or `FirstMatch` for a list of capability sets (the first matching set is used):

[!code-csharp[New Session With Capabilities](../../code/modules/SessionModuleSamples.cs#NewSessionWithCapabilities)]

Supported capability requests include `BrowserName`, `BrowserVersion`, `PlatformName`, `AcceptInsecureCertificates`, `Proxy`, and `UnhandledPromptBehavior`. See `CapabilityRequest` and `CapabilitiesRequest` in the API reference for the full set.

## Session Status

### Check Session Status

[!code-csharp[Check Session Status](../../code/modules/SessionModuleSamples.cs#CheckSessionStatus)]

## Event Subscription

### Subscribe to Events

Prefer the `EventName` property from observable events to avoid typos:

[!code-csharp[Subscribe Multiple Events](../../code/events-observables/SubscribeSamples.cs#SubscribeMultipleEvents)]

### Subscribe with Context Filter

[!code-csharp[Subscription with Context](../../code/events-observables/SubscribeSamples.cs#SubscriptionwithContext)]

### Unsubscribe by ID

[!code-csharp[Unsubscribe by ID](../../code/events-observables/SubscribeSamples.cs#UnsubscribebyID)]

### Unsubscribe by Event Names

[!code-csharp[Unsubscribe by Event Names](../../code/events-observables/SubscribeSamples.cs#UnsubscribebyEventNames)]

## Best Practices

1. **Subscribe early**: Subscribe to events before triggering actions that produce them
2. **Track subscription IDs**: Store IDs if you need to unsubscribe later
3. **Use context filtering**: Limit events to specific contexts when possible
4. **Clean up subscriptions**: Unsubscribe when events are no longer needed

## Next Steps

- [Events and Observables](../events-observables.md): Comprehensive event guide
- [Core Concepts](../core-concepts.md): Understanding subscriptions

