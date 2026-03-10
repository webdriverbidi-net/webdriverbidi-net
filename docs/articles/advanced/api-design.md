# API Design Guide

This guide documents the design principles and conventions used in WebDriverBiDi.NET. Understanding these patterns will help you use the library effectively and write idiomatic code.

## Overview

WebDriverBiDi.NET is a low-level protocol client. The API design prioritizes:

- **Explicit over implicit**: Commands that can reset state require explicit parameters so intent is clear
- **Consistency**: All module commands follow the same pattern for timeouts and cancellation
- **Protocol fidelity**: The API closely mirrors the WebDriver BiDi protocol structure

## Command Parameter Patterns

### Required vs Optional Parameters

Module commands fall into two categories based on their `CommandParameters`:

**Commands with optional parameters** accept `null` or omit the parameters object. Use these when the command has no required properties and no "reset" capability:

```csharp
// All equivalent—parameters optional
GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(null);
GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
StatusCommandResult status = await driver.Session.StatusAsync(null);
GetCookiesCommandResult cookies = await driver.Storage.GetCookiesAsync(null);
```

**Commands with required parameters** always require a parameters object. These commands have a static "reset" property that clears a value on the remote end. Passing no parameters would be ambiguous—are you setting or resetting?

```csharp
// ✅ Explicit reset
await driver.UserAgentClientHints.SetClientHintsOverrideAsync(
    SetClientHintsOverrideCommandParameters.ResetClientHintsOverride);

// ✅ Explicit set
SetClientHintsOverrideCommandParameters setParams = new SetClientHintsOverrideCommandParameters();
setParams.ClientHints = new ClientHintsMetadata { /* ... */ };
await driver.UserAgentClientHints.SetClientHintsOverrideAsync(setParams);
```

### Complete Lists

| Optional Parameters | Required Parameters (Reset Property) |
|--------------------|--------------------------------------|
| `Browser.CloseAsync` | `UserAgentClientHints.SetClientHintsOverrideAsync` |
| `Browser.CreateUserContextAsync` | `Browser.SetDownloadBehaviorAsync` |
| `Browser.GetClientWindowsAsync` | `BrowsingContext.SetBypassCSPAsync` |
| `Browser.GetUserContextsAsync` | `BrowsingContext.SetViewportAsync` |
| `BrowsingContext.GetTreeAsync` | `Network.SetExtraHeadersAsync` |
| `Script.GetRealmsAsync` | All Emulation `Set*OverrideAsync` commands |
| `Session.EndAsync` | |
| `Session.StatusAsync` | |
| `Storage.DeleteCookiesAsync` | |
| `Storage.GetCookiesAsync` | |

### Nullable List Properties

Many `CommandParameters` expose nullable list properties (e.g., `List<string>? Contexts`). This is intentional:

- **`null`**: Property omitted from the JSON payload (protocol treats as "not specified")
- **Empty list `[]`**: Explicit empty array sent to the remote end (protocol treats differently)

Always check the XML documentation on the property for the rationale. See [Core Concepts - Command Parameters](../core-concepts.md#command-parameters) for details.

## Timeout and Cancellation

Every module command accepts two optional parameters. **This is the preferred way to set per-command timeouts** when using the module API (e.g., `driver.BrowsingContext.NavigateAsync`):

```csharp
Task<T> CommandAsync(
    CommandParameters? parameters,
    TimeSpan? timeoutOverride = null,
    CancellationToken cancellationToken = default)
```

- **`timeoutOverride`**: When `null`, the driver uses `BiDiDriver.DefaultCommandTimeout` (60 seconds by default). Pass a value to override for long-running or quick-fail scenarios.
- **`cancellationToken`**: Propagates cancellation. Use for cooperative cancellation (e.g., user cancel, test timeout).

```csharp
// Use default timeout
await driver.BrowsingContext.NavigateAsync(navParams);

// Override timeout for quick operation
await driver.Session.StatusAsync(null, timeoutOverride: TimeSpan.FromSeconds(5));

// With cancellation
using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
await driver.BrowsingContext.NavigateAsync(navParams, cancellationToken: cts.Token);
```

For timeout patterns (e.g., returning `null` instead of throwing) and connection-level timeout configuration, see [Error Handling - Timeout Handling](error-handling.md#timeout-handling).

## Error Handling Configuration

The library uses `TransportErrorBehavior` (Ignore, Collect, Terminate) to control how transport-level errors are handled. Four properties on `BiDiDriver` provide fine-grained control in normal application code. These same members are also exposed through the advanced `IBiDiDriverEvents` interface for framework and testing scenarios.

| Property | Default | Controls |
|----------|---------|----------|
| `EventHandlerExceptionBehavior` | Ignore | Exceptions thrown by event handlers |
| `ProtocolErrorBehavior` | Ignore | Invalid JSON, deserialization failures |
| `UnknownMessageBehavior` | Ignore | Valid JSON that doesn't match any known structure |
| `UnexpectedErrorBehavior` | Ignore | Error response with no corresponding command |

See [Error Handling](error-handling.md) for detailed guidance on when to use each mode.

## Versioning and Compatibility

### Package Versioning

WebDriverBiDi.NET follows [Semantic Versioning](https://semver.org/) (SemVer):

- **Major**: Breaking API changes
- **Minor**: New features, backward compatible
- **Patch**: Bug fixes, backward compatible

### Framework Support

The main library targets .NET Standard 2.0, ensuring compatibility with:

- .NET Framework 4.6.1+
- .NET Core 2.0+
- .NET 5, 6, 7, 8, 9, 10

### Protocol Compatibility

The WebDriver BiDi protocol is evolving. The library defaults to `TransportErrorBehavior.Ignore` for protocol errors and unknown messages to support:

- **Forward compatibility**: Older library versions working with newer browsers that send new message types
- **Graceful degradation**: Automation continuing when protocol versions diverge slightly

When strict conformance is required (e.g., production with known protocol versions), consider `ProtocolErrorBehavior.Terminate` and `UnknownMessageBehavior.Terminate`.

### Breaking Changes

Breaking changes are documented in release notes. When upgrading major versions, review the changelog for:

- Removed or renamed types and members
- Changed method signatures
- Changed default behavior

## Related Documentation

- [Core Concepts](../core-concepts.md): Command parameters, events, lifecycle
- [Error Handling](error-handling.md): TransportErrorBehavior, exception handling, timeout patterns, troubleshooting
- [Architecture](../architecture.md): Transport, connection, error configuration
- [Quick Reference](../quick-reference.md): Common commands at a glance
