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
| **BIDI001** | Error | `RegisterModule()` called after `StartAsync()`. A `StopAsync()` call returns the driver to the not-started state, so registration after a stop is not reported |
| **BIDI002** | Error | Custom event registered (via `RegisterEvent()`) after `StartAsync()`; not reported after a `StopAsync()`. Adding observers with `AddObserver()` is not reported: observers may be added to an observable event at any time, including while the driver is running |
| **BIDI003** | Error | `RegisterTypeInfoResolverAsync()` called after `StartAsync()`; not reported after a `StopAsync()` |
| **BIDI004** | Info | Cancellable operation (`ExecuteCommandAsync`, `EvaluateAsync`, `CallFunctionAsync`, `GetTreeAsync`, `LocateNodesAsync`) called without `CancellationToken`; suggests passing one. `NavigateAsync` is reported by BIDI013 instead, never by both |
| **BIDI005** | Warning | Event observer added but event name not included in `Session.SubscribeAsync()`. Both the list constructor (`new SubscribeCommandParameters(["a", "b"])`) and the single-event constructor (`new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName)`) are recognized; string literals, constants and `EventName` property accesses all resolve |
| **BIDI006** | Warning | `EventObserver` not disposed or unobserved |
| **BIDI007** | Warning | Blocking operation (e.g., `Thread.Sleep`, `.Result`) in event handler. `RunHandlerAsynchronously` suppresses the diagnostic only when the handler actually runs off the dispatching thread: an `Action<T>` handler, an `async` lambda, or an `async` method group. A non-`async` `Task`-returning handler is still reported (with a message saying the option cannot help), because the option detaches the returned `Task` rather than moving where the handler starts |
| **BIDI008** | Warning | Unsafe cast of `EvaluateResult`; suggests pattern matching |
| **BIDI009** | Error | Module command or `driver.ExecuteCommandAsync()` called before `StartAsync()`, or after `StopAsync()` without a new `StartAsync()` |
| **BIDI010** | Error | Async module command not awaited (fire-and-forget) |
| **BIDI012** | Info / Warning | `DisposeAsync()` called without `StopAsync()` first, including the implicit disposal of `await using var driver = ...` and `await using (driver) { ... }`; suggests calling `StopAsync`. Reported as a **Warning** when the same method also assigns `TransportErrorBehavior.Collect` to any of the four error-behavior properties, because `DisposeAsync()` logs and discards collected errors—only `StopAsync()` throws them |
| **BIDI013** | Warning | Long-running operation (e.g., `NavigateAsync`) called without `CancellationToken` |
| **BIDI014** | Warning | Parameterless constructor used for a command with a command-level reset property (i.e., a `public static Reset*` property, declared on the class or inherited from a base class, that returns the constructed `CommandParameters` type or one of its base types — this covers `SetGeolocationOverrideCoordinatesCommandParameters`, whose reset helper lives on `SetGeolocationOverrideCommandParameters`); suggests using `.Reset*`. Does not apply to property-level sentinel classes such as `SetViewportCommandParameters`, whose `Reset*` members return unrelated types. |
| **BIDI015** | Warning | String literal used for event name instead of `ObservableEvent.EventName`, in either `SubscribeCommandParameters` constructor form |
| **BIDI016** | Warning | Deadlock-prone synchronization in an `async` event handler: `lock`, `Monitor.Enter`, `Semaphore`/`SemaphoreSlim.Wait`, `WaitHandle.WaitOne`, `SynchronizationContext.Send`, `Task.WaitAll`/`WaitAny`. Blocking calls such as `.Result` and `.Wait()` are BIDI007. `RunHandlerAsynchronously` suppresses the diagnostic |
| **BIDI017** | Warning | Adding to nullable list property without `??= new List<T>()` |
| **BIDI020** | Error | `WaitForCapturedTasksAsync()` or `WaitForCapturedTasksCompleteAsync()` called without a prior `StartCapturingTasks()` in the same method |
| **BIDI021** | Warning | `StartCapturingTasks()` called but no read method (`WaitForCapturedTasksAsync`, `WaitForCapturedTasksCompleteAsync`, `GetCapturedTasks`) follows in the same method |
| **BIDI022** | Warning | Writing a value into `CommandParameters.AdditionalData` (via `Add`, `TryAdd`, or indexer assignment). The `Dictionary<string, object?>` values are serialized through reflection-based `JsonSerializer` overloads, which are not compatible with native AOT or IL trimming unless every value's runtime type is registered via `BiDiDriver.RegisterTypeInfoResolverAsync` |
| **BIDI023** | Warning | Module command (e.g., `NavigateAsync`, `EvaluateAsync`) called inside an `AddObserver` event handler without `RunHandlerAsynchronously`. The driver's command pipeline dispatches events synchronously by default; calling a module command from within the handler can deadlock or produce unexpected behavior. As with BIDI007, the option only suppresses the diagnostic for an `Action<T>` handler, an `async` lambda, or an `async` method group; a non-`async` `Task`-returning handler is still reported |

## Code Fixes

Many analyzers provide automatic code fixes. In Visual Studio or VS Code, use the lightbulb or quick-action menu on the diagnostic to apply the suggested fix.

The following analyzers have code fix providers:

- **BIDI001** — Moves `RegisterModule()` call before `StartAsync()`
- **BIDI002** — Moves `RegisterEvent()` call before `StartAsync()`
- **BIDI003** — Moves `RegisterTypeInfoResolverAsync()` call before `StartAsync()`
- **BIDI004** — Adds `CancellationToken` parameter to long-running operations
- **BIDI005** — Adds missing event name to `Session.SubscribeAsync()` call (a single-event constructor argument is rewritten into a collection expression holding both events)
- **BIDI006** — Adds a `using` declaration for the `EventObserver`
- **BIDI007** — For an `Action<T>` handler or an `async` lambda, adds the `ObservableEventHandlerOptions.RunHandlerAsynchronously` option to the `AddObserver` call. For a non-`async` `Task`-returning lambda, converts it to an `async` lambda whose first statement is `await Task.Yield();` (rewriting `return Task.CompletedTask;` / `return <task>;` accordingly) and adds the option if it is missing. No fix is offered when the handler is a method group, because the method declaration itself would have to change
- **BIDI008** — Replaces unsafe cast with pattern matching
- **BIDI009** — Adds `await driver.StartAsync()` before command execution
- **BIDI012** — Adds `await driver.StopAsync()` before `DisposeAsync()`; for `await using` forms, at the end of the scope that disposes the driver
- **BIDI014** — Replaces parameterless constructor with `.Reset*` property (qualified by the type that declares it; a local declared with the derived type is retyped to match)
- **BIDI015** — Replaces string literal with `ObservableEvent.EventName` property
- **BIDI017** — Adds null-coalescing assignment before adding to nullable list
- **BIDI020** — Inserts `observer.StartCapturingTasks()` before the offending `WaitForCapturedTasksAsync` or `WaitForCapturedTasksCompleteAsync` call
- **BIDI023** — Same fix as BIDI007: adds `ObservableEventHandlerOptions.RunHandlerAsynchronously` to the `AddObserver` call, converting a non-`async` `Task`-returning lambda to an `async` one first; no fix for method-group handlers

## Related Documentation

| Analyzer Topic | See Also |
|----------------|----------|
| Registration timing (BIDI001, BIDI002, BIDI003) | [Common Pitfalls - Module Registration Timing](../common-pitfalls.md#module-registration-timing) |
| Event subscription (BIDI005) | [Common Pitfalls - Event Subscription](../common-pitfalls.md#event-subscription) |
| Blocking handlers (BIDI007, BIDI016) | [Common Pitfalls - Blocking the Transport Thread](../common-pitfalls.md#pitfall-blocking-the-transport-thread-with-synchronous-handlers) |
| Module commands in event handlers (BIDI023) | [Common Pitfalls - Blocking the Transport Thread](../common-pitfalls.md#pitfall-blocking-the-transport-thread-with-synchronous-handlers) |
| Observer disposal (BIDI006) | [Common Pitfalls - Resource Cleanup](../common-pitfalls.md#resource-cleanup) |
| Collect mode and disposal (BIDI012) | [Error Handling - Collect Mode](error-handling.md#collect-mode) |
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
