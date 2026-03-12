# Roslyn Analyzers

WebDriverBiDi.NET includes optional Roslyn analyzers that catch common usage errors at compile time. The analyzers run in your IDE and during build, helping you avoid pitfalls before they cause runtime failures.

## Installation

The analyzers are distributed in a separate NuGet package. Add it to your project:

```bash
dotnet add package WebDriverBiDi.Analyzers
```

Or add to your `.csproj`:

```xml
<PackageReference Include="WebDriverBiDi.Analyzers" Version="*" />
```

The analyzer package is marked as a development dependency, so it will not be included in your application's output.

## Available Analyzers

When an analyzer fires, your IDE will show a diagnostic with a suggestion or code fix where applicable. The following analyzers are available:

| ID | Severity | When It Fires |
|----|----------|----------------|
| **BIDI001** | Error | `RegisterModule()` called after `StartAsync()` |
| **BIDI002** | Error | Event registered (via `AddObserver`) after `StartAsync()` |
| **BIDI003** | Error | `RegisterTypeInfoResolver()` called after `StartAsync()` |
| **BIDI004** | Info | Long-running operation called without `CancellationToken`; suggests passing one |
| **BIDI005** | Warning | Event observer added but event name not included in `Session.SubscribeAsync()` |
| **BIDI006** | Warning | `EventObserver` not disposed or unobserved |
| **BIDI007** | Warning | Blocking operation (e.g., `Thread.Sleep`, `.Result`) in event handler |
| **BIDI008** | Warning | Unsafe cast of `EvaluateResult`; suggests pattern matching |
| **BIDI009** | Error | Module command called before `StartAsync()` |
| **BIDI010** | Error | Async module command not awaited (fire-and-forget) |
| **BIDI011** | Warning | `SetCheckpoint()` called but checkpoint never waited for or unset |
| **BIDI012** | Info | `DisposeAsync()` called without `StopAsync()` first; suggests calling `StopAsync` |
| **BIDI013** | Warning | Long-running operation (e.g., `NavigateAsync`) called without `CancellationToken` |
| **BIDI014** | Warning | Parameterless constructor used for command with reset property; suggests using `.Reset*` |
| **BIDI015** | Warning | String literal used for event name instead of `ObservableEvent.EventName` |
| **BIDI016** | Warning | Deadlock-prone pattern (e.g., `.Result`, `.Wait()`) in event handler |
| **BIDI017** | Warning | Adding to nullable list property without `??= new List<T>()` |
| **BIDI018** | Warning | `RemoteValue.ValueAs<T>()` with unsupported type; suggests `RemoteValueDictionary` or `RemoteValueList` |

## Code Fixes

Many analyzers provide automatic code fixes. In Visual Studio or VS Code, use the lightbulb or quick-action menu on the diagnostic to apply the suggested fix.

## Related Documentation

| Analyzer Topic | See Also |
|----------------|----------|
| Registration timing (BIDI001, BIDI002, BIDI003) | [Common Pitfalls - Module Registration Timing](../common-pitfalls.md#module-registration-timing) |
| Event subscription (BIDI005) | [Common Pitfalls - Event Subscription](../common-pitfalls.md#event-subscription) |
| Blocking handlers (BIDI007, BIDI016) | [Common Pitfalls - Blocking the Transport Thread](../common-pitfalls.md#pitfall-blocking-the-transport-thread-with-synchronous-handlers) |
| Observer disposal (BIDI006) | [Common Pitfalls - Resource Cleanup](../common-pitfalls.md#resource-cleanup) |
| Nullable collections (BIDI017) | [Common Pitfalls - Null vs Empty Collections](../common-pitfalls.md#null-vs-empty-collections) |
| RemoteValue (BIDI018) | [Working with Remote Values](../remote-values.md) |
| Reset parameters (BIDI014) | [API Design Guide - Required vs Optional Parameters](api-design.md#required-vs-optional-parameters) |

## See Also

- [Common Pitfalls](../common-pitfalls.md): Detailed explanations of the issues the analyzers catch
- [Error Handling](error-handling.md): Transport error behavior and exception handling
- [API Design Guide](api-design.md): Command parameter patterns and timeout/cancellation
