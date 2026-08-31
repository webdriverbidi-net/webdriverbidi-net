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

### Minimum SDK

The package ships builds of the analyzers for multiple Roslyn versions (under `analyzers/dotnet/roslyn<version>/cs`), and your compiler automatically loads the highest build at or below its own Roslyn version. The lowest build targets Roslyn 4.8, which ships with the .NET 8.0 SDK (and the corresponding Visual Studio 2022 17.8 / VS Code C# tooling). Any SDK from .NET 8.0 onward — including newer SDKs used to target `net8.0`, `net9.0`, or `net10.0` — loads a matching build and runs the analyzers. Toolchains older than the .NET 8.0 SDK match no build and silently skip the analyzers (`CS9057` if they see a newer-only build); upgrade the SDK to enable them.

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
| **BIDI013** | Warning | Long-running operation (`NavigateAsync`, `PrintAsync`, `ReloadAsync`, `StartAsync`, `WaitForCapturedTasksAsync`, `WaitForCapturedTasksCompleteAsync`) called without `CancellationToken` |
| **BIDI014** | Warning | Parameterless constructor used for a command with a command-level reset property (i.e., a `public static Reset*` property, declared on the class or inherited from a base class, that returns the constructed `CommandParameters` type or one of its base types — this covers `SetGeolocationOverrideCoordinatesCommandParameters`, whose reset helper lives on `SetGeolocationOverrideCommandParameters`); suggests using `.Reset*`. Does not apply to property-level sentinel classes such as `SetViewportCommandParameters`, whose `Reset*` members return unrelated types. |
| **BIDI015** | Warning | String literal used for event name instead of `ObservableEvent.EventName`, in either `SubscribeCommandParameters` constructor form |
| **BIDI016** | Warning | Deadlock-prone synchronization in an `async` event handler: `lock`, `Monitor.Enter`, `Semaphore`/`SemaphoreSlim.Wait`, `WaitHandle.WaitOne`, `SynchronizationContext.Send`, `Task.WaitAll`/`WaitAny`. Blocking calls such as `.Result` and `.Wait()` are BIDI007. `RunHandlerAsynchronously` suppresses the diagnostic |
| **BIDI017** | Warning | Adding to nullable list property without `??= new List<T>()` |
| **BIDI020** | Error | `WaitForCapturedTasksAsync()` or `WaitForCapturedTasksCompleteAsync()` called without a prior `StartCapturingTasks()` in the same method |
| **BIDI021** | Warning | `StartCapturingTasks()` called but no read method (`WaitForCapturedTasksAsync`, `WaitForCapturedTasksCompleteAsync`, `GetCapturedTasks`) follows in the same method |
| **BIDI022** | Warning | Writing a value into `CommandParameters.AdditionalData` (via `Add`, `TryAdd`, or indexer assignment). The `Dictionary<string, object?>` values are serialized through reflection-based `JsonSerializer` overloads, which are not compatible with native AOT or IL trimming unless every value's runtime type is registered via `BiDiDriver.RegisterTypeInfoResolverAsync` |
| **BIDI023** | Warning | Module command (e.g., `NavigateAsync`, `EvaluateAsync`) called inside an `AddObserver` event handler without `RunHandlerAsynchronously`. The driver's command pipeline dispatches events synchronously by default; calling a module command from within the handler can deadlock or produce unexpected behavior. As with BIDI007, the option only suppresses the diagnostic for an `Action<T>` handler, an `async` lambda, or an `async` method group; a non-`async` `Task`-returning handler is still reported |
| **BIDI024** | Error | `StartAsync()` called a second time on the same driver without an intervening `StopAsync()`. The transport is already connected, so the second call throws `WebDriverBiDiConnectionException`. Tracked per local variable within a method, constructor, or top-level program; a `StopAsync()` returns the driver to the not-started state, so a start / stop / start sequence is not reported |
| **BIDI025** | Warning | An `async void` method is passed as an `AddObserver` handler. It binds to the `Action<T>` overload (not `Func<T, Task>`), so it runs fire-and-forget: exceptions thrown after its first `await` are unobserved async-void faults that can crash the process, and the observer is considered complete before the handler's async work finishes. An `async` lambda or `async Task` method group binds to `Func<T, Task>` and is not reported |
| **BIDI026** | Error | An explicit `ExecuteCommandAsync<T>` type argument disagrees with the command's result type (e.g., `ExecuteCommandAsync<WrongResult>(new StatusCommandParameters())`). The generic `CommandParameters<T>` overload no longer applies, so the call binds to the non-generic `CommandParameters` overload, compiles, and then throws `WebDriverBiDiException` at runtime because the response cannot be converted to `T`. A matching or base type argument, or an inferred one, is not reported. Skipped when either type is an open generic type parameter |
| **BIDI027** | Error | `RegisterEvent()` called with a built-in protocol event name (e.g., `RegisterEvent<T>("log.entryAdded", …)`). Modules register those names in their constructors, so `RegisterEvent` throws `ArgumentException` at runtime. The built-in names are read from the library's `[ObservableEventName]` attributes. Only a compile-time-constant name argument is checked; observe a built-in event through its `ObservableEvent` property instead |

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
- **BIDI026** — Two fixes: changes the `ExecuteCommandAsync<T>` type argument to the command's result type, or removes the explicit type argument so it is inferred

## Configuration and Suppression

The analyzers ship with the severities listed above. Several rules are **Error** severity and will fail the build, so you may need to downgrade or suppress a rule in code you cannot change or that the analyzer flags as a false positive (see [Known Limitations](#known-limitations)).

**Change a rule's severity project-wide** in an `.editorconfig` file:

```ini
# Downgrade BIDI009 from error to warning for the whole project
[*.cs]
dotnet_diagnostic.BIDI009.severity = warning

# Turn a rule off entirely
dotnet_diagnostic.BIDI004.severity = none
```

**Suppress a single occurrence** with a pragma:

```csharp
#pragma warning disable BIDI009 // command executed before StartAsync (set up in a helper)
driver.BrowsingContext.NavigateAsync(navParams);
#pragma warning restore BIDI009
```

**Suppress on a member** with an attribute:

```csharp
[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "BIDI006:EventObserver should be disposed", Justification = "Observer lifetime is managed by the test fixture.")]
public void RegisterObserver() { /* ... */ }
```

**Disable one or more rules for a whole project** with `NoWarn` in the `.csproj`:

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);BIDI004;BIDI013</NoWarn>
</PropertyGroup>
```

## Rule Reference

Each rule below is addressable by anchor (for example `#bidi004`) so its diagnostic can link here. See the [Available Analyzers](#available-analyzers) table for the full firing conditions and the [Code Fixes](#code-fixes) list for which rules offer an automatic fix.

### BIDI001

**Error.** `RegisterModule()` called after `StartAsync()`. See [Common Pitfalls — Module Registration Timing](../common-pitfalls.md#module-registration-timing) and [Core Concepts — Timing Restrictions](../core-concepts.md#timing-restrictions).

### BIDI002

**Error.** A custom event registered via `RegisterEvent()` after `StartAsync()`. See [Common Pitfalls — Module Registration Timing](../common-pitfalls.md#module-registration-timing).

### BIDI003

**Error.** `RegisterTypeInfoResolverAsync()` called after `StartAsync()`. See [Common Pitfalls — Module Registration Timing](../common-pitfalls.md#module-registration-timing).

### BIDI004

**Info.** A cancellable operation is called without a `CancellationToken`. See [API Design Guide](api-design.md).

### BIDI005

**Warning.** An event observer is added but its event name is not included in `Session.SubscribeAsync()`. See [Common Pitfalls — Event Subscription](../common-pitfalls.md#event-subscription).

### BIDI006

**Warning.** An `EventObserver` is neither disposed nor unobserved. See [Common Pitfalls — Resource Cleanup](../common-pitfalls.md#resource-cleanup).

### BIDI007

**Warning.** A blocking operation appears in an event handler. See [Common Pitfalls — Blocking the Transport Thread](../common-pitfalls.md#pitfall-blocking-the-transport-thread-with-synchronous-handlers).

### BIDI008

**Warning.** An `EvaluateResult` is cast unsafely; pattern matching is suggested.

### BIDI009

**Error.** A module command or `ExecuteCommandAsync()` is called before `StartAsync()` (or after `StopAsync()` without a new `StartAsync()`).

### BIDI010

**Error.** An async module command is not awaited (fire-and-forget).

### BIDI012

**Info / Warning.** `DisposeAsync()` is called without a prior `StopAsync()`. See [Error Handling — Collect Mode](error-handling.md#collect-mode).

### BIDI013

**Warning.** A long-running operation (`NavigateAsync`, `PrintAsync`, `ReloadAsync`, `StartAsync`, `WaitForCapturedTasksAsync`, `WaitForCapturedTasksCompleteAsync`) is called without a `CancellationToken`.

### BIDI014

**Warning.** A parameterless constructor is used for a command that exposes a `.Reset*` property. See [API Design Guide — Required vs Optional Parameters](api-design.md#required-vs-optional-parameters).

### BIDI015

**Warning.** A string literal is used for an event name instead of `ObservableEvent.EventName`.

### BIDI016

**Warning.** Deadlock-prone synchronization appears in an `async` event handler. See [Common Pitfalls — Blocking the Transport Thread](../common-pitfalls.md#pitfall-blocking-the-transport-thread-with-synchronous-handlers).

### BIDI017

**Warning.** A value is added to a nullable list property without a `??=` initializer. See [Common Pitfalls — Null vs Empty Collections](../common-pitfalls.md#null-vs-empty-collections).

### BIDI020

**Error.** `WaitForCapturedTasksAsync()` / `WaitForCapturedTasksCompleteAsync()` is called without a prior `StartCapturingTasks()`. See [Events and Observables — Event Synchronization](../events-observables.md#event-synchronization).

### BIDI021

**Warning.** `StartCapturingTasks()` is called but no read method follows. See [Events and Observables — Event Synchronization](../events-observables.md#event-synchronization).

### BIDI022

**Warning.** A value is written into `CommandParameters.AdditionalData`, which is not AOT/trimming safe unless every value's type is registered. See [API Design Guide — Protocol Extensions via AdditionalData](api-design.md#protocol-extensions-via-additionaldata) and [AOT Compatibility](aot-compatibility.md).

### BIDI023

**Warning.** A module command is called inside an `AddObserver` handler without `RunHandlerAsynchronously`. See [Common Pitfalls — Blocking the Transport Thread](../common-pitfalls.md#pitfall-blocking-the-transport-thread-with-synchronous-handlers).

### BIDI024

**Error.** `StartAsync()` is called a second time on the same driver without an intervening `StopAsync()`. The transport is already connected, so the call throws `WebDriverBiDiConnectionException`. Call `StopAsync()` before starting again.

### BIDI025

**Warning.** An `async void` method is passed as an `AddObserver` handler, binding it to the `Action<T>` overload. Its exceptions become unobserved async-void faults and its asynchronous work is not tracked. Declare the handler as `async Task` so it binds to the `Func<T, Task>` overload.

### BIDI026

**Error.** An explicit `ExecuteCommandAsync<T>` type argument does not match the result type of the supplied parameters object. The call binds to the non-generic `ExecuteCommandAsync(CommandParameters)` overload and throws `WebDriverBiDiException` at runtime because the response cannot be converted to `T`. Match the type argument to the command's result type, or let it be inferred.

### BIDI027

**Error.** `RegisterEvent()` is called with a built-in protocol event name (such as `"log.entryAdded"`). Modules register those names in their constructors, so `RegisterEvent` throws `ArgumentException` at runtime. Use `RegisterEvent` only for custom events; observe a built-in event through its `ObservableEvent` property and `Session.SubscribeAsync`.

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
