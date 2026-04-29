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
| **BIDI012** | Info | `DisposeAsync()` called without `StopAsync()` first; suggests calling `StopAsync` |
| **BIDI013** | Warning | Long-running operation (e.g., `NavigateAsync`) called without `CancellationToken` |
| **BIDI014** | Warning | Parameterless constructor used for a command with a command-level reset property (i.e., a `static Reset*` property returning the same `CommandParameters` type); suggests using `.Reset*`. Does not apply to property-level sentinel classes such as `SetViewportCommandParameters`. |
| **BIDI015** | Warning | String literal used for event name instead of `ObservableEvent.EventName` |
| **BIDI016** | Warning | Deadlock-prone pattern (e.g., `.Result`, `.Wait()`) in event handler |
| **BIDI017** | Warning | Adding to nullable list property without `??= new List<T>()` |
| **BIDI020** | Error | `WaitForCapturedTasksAsync()` or `WaitForCapturedTasksCompleteAsync()` called without a prior `StartCapturingTasks()` in the same method |
| **BIDI021** | Warning | `StartCapturingTasks()` called but no read method (`WaitForCapturedTasksAsync`, `WaitForCapturedTasksCompleteAsync`, `GetCapturedTasks`) follows in the same method |
| **BIDI022** | Warning | Writing a value into `CommandParameters.AdditionalData` (via `Add`, `TryAdd`, or indexer assignment). The `Dictionary<string, object?>` values are serialized through reflection-based `JsonSerializer` overloads, which are not compatible with native AOT or IL trimming unless every value's runtime type is registered via `BiDiDriver.RegisterTypeInfoResolverAsync` |
| **BIDI023** | Warning | Module command (e.g., `NavigateAsync`, `EvaluateAsync`) called inside an `AddObserver` event handler without `RunHandlerAsynchronously`. The driver's command pipeline dispatches events synchronously by default; calling a module command from within the handler can deadlock or produce unexpected behavior. |

## Code Fixes

Many analyzers provide automatic code fixes. In Visual Studio or VS Code, use the lightbulb or quick-action menu on the diagnostic to apply the suggested fix.

The following analyzers have code fix providers:

- **BIDI001** — Moves `RegisterModule()` call before `StartAsync()`
- **BIDI002** — Moves `AddObserver()` call before `StartAsync()`
- **BIDI003** — Moves `RegisterTypeInfoResolver()` call before `StartAsync()`
- **BIDI004** — Adds `CancellationToken` parameter to long-running operations
- **BIDI005** — Adds missing event name to `Session.SubscribeAsync()` call
- **BIDI006** — Adds `using` statement or `.Dispose()` call for `EventObserver`
- **BIDI007** — Replaces blocking call with `await` in event handler
- **BIDI008** — Replaces unsafe cast with pattern matching
- **BIDI009** — Adds `await driver.StartAsync()` before command execution
- **BIDI012** — Adds `await driver.StopAsync()` before `DisposeAsync()`
- **BIDI014** — Replaces parameterless constructor with `.Reset*` property
- **BIDI015** — Replaces string literal with `ObservableEvent.EventName` property
- **BIDI017** — Adds null-coalescing assignment before adding to nullable list
- **BIDI020** — Inserts `observer.StartCapturingTasks()` before the offending `WaitForCapturedTasksAsync` or `WaitForCapturedTasksCompleteAsync` call
- **BIDI023** — Adds `ObservableEventHandlerOptions.RunHandlerAsynchronously` to the `AddObserver` call

## Related Documentation

| Analyzer Topic | See Also |
|----------------|----------|
| Registration timing (BIDI001, BIDI002, BIDI003) | [Common Pitfalls - Module Registration Timing](../common-pitfalls.md#module-registration-timing) |
| Event subscription (BIDI005) | [Common Pitfalls - Event Subscription](../common-pitfalls.md#event-subscription) |
| Blocking handlers (BIDI007, BIDI016) | [Common Pitfalls - Blocking the Transport Thread](../common-pitfalls.md#pitfall-blocking-the-transport-thread-with-synchronous-handlers) |
| Module commands in event handlers (BIDI023) | [Common Pitfalls - Blocking the Transport Thread](../common-pitfalls.md#pitfall-blocking-the-transport-thread-with-synchronous-handlers) |
| Observer disposal (BIDI006) | [Common Pitfalls - Resource Cleanup](../common-pitfalls.md#resource-cleanup) |
| Nullable collections (BIDI017) | [Common Pitfalls - Null vs Empty Collections](../common-pitfalls.md#null-vs-empty-collections) |
| Reset parameters (BIDI014) | [API Design Guide - Required vs Optional Parameters](api-design.md#required-vs-optional-parameters) |
| Capture session ordering (BIDI020, BIDI021) | [Events and Observables - Event Synchronization](../events-observables.md#event-synchronization) |
| AdditionalData and AOT (BIDI022) | [API Design Guide - Protocol Extensions via AdditionalData](api-design.md#protocol-extensions-via-additionaldata), [AOT Compatibility](aot-compatibility.md) |

## Known Limitations

No analyzer performs whole-program flow analysis; none of them correlate data across files, classes, or — for most of them — across method boundaries. Each rule falls into one of the scopes below. Understanding which scope a given rule uses helps explain why it may not fire in a particular situation.

### Analyzer scope by rule

| Scope | What the analyzer sees | Rules |
|-------|------------------------|-------|
| **Intra-procedural** — single method body | The analyzer walks one method at a time and correlates statements within that method (e.g., "was `StartAsync` called before this line?"). It cannot see into other methods. | BIDI001, BIDI002, BIDI003, BIDI005, BIDI006, BIDI009, BIDI012, BIDI014, BIDI015, BIDI020, BIDI021 |
| **Per-invocation** — single call site | The analyzer examines each matching invocation in isolation (argument list, surrounding expression). There is no correlation with other statements in the method. | BIDI004, BIDI010, BIDI013, BIDI017, BIDI022 |
| **Per-expression** — single expression | The analyzer examines each matching syntactic expression (e.g., a cast) in isolation. | BIDI008 |
| **Per-invocation with handler-body descent** — call site plus the handler it passes | The analyzer inspects each matching `AddObserver(...)` call and also walks into the handler body to look for patterns. When the handler is an inline lambda, the body is right there. When the handler is passed as a method reference (e.g., `AddObserver(this.HandleEvent)`), the analyzer resolves the reference and walks that method body too. It does not continue transitively into further methods that handler body calls. | BIDI007, BIDI016, BIDI023 |

### What this means in practice

If you split setup across helper methods (common in test frameworks or automation wrappers), analyzers in the **intra-procedural** or **per-invocation** tiers will not correlate calls in different methods:

```csharp
// SetupAsync() — BIDI001/009 tracks driver state here
async Task SetupAsync() { driver = new BiDiDriver(); await driver.StartAsync(...); }

// TestAsync() — BIDI009 cannot detect that StartAsync was called in SetupAsync
async Task TestAsync() { driver.RegisterModule(new CustomModule(driver)); } // no diagnostic
```

BIDI007, BIDI016, and BIDI023 are the exceptions: they will follow a single hop from an `AddObserver(...)` call to a method reference used as the handler, but they will not walk further than that.

**Runtime enforcement remains correct.** The library still throws `InvalidOperationException` or `ObjectDisposedException` at runtime when these patterns are violated. The analyzers provide compile-time guidance where they can; they do not replace runtime validation.

## See Also

- [Common Pitfalls](../common-pitfalls.md): Detailed explanations of the issues the analyzers catch
- [Error Handling](error-handling.md): Transport error behavior and exception handling
- [API Design Guide](api-design.md): Command parameter patterns and timeout/cancellation
