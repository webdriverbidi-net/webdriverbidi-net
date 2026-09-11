# Roslyn Analyzers

WebDriverBiDi.NET includes optional Roslyn analyzers that catch common usage errors at compile time. The analyzers run in your IDE and during build, helping you avoid pitfalls before they cause runtime failures.

## Installation

The analyzers are distributed in a separate NuGet package. Add it to your project:

```bash
dotnet add package WebDriverBiDi.Analyzers
```

The analyzer package is marked as a development dependency, so it will not be included in your application's output. To pin an exact version instead of tracking the current release, add a `<PackageReference>` to your `.csproj` the same way as the main package — see [Pinning an exact version](../getting-started.md#pinning-an-exact-version).

### Minimum SDK

The package ships builds of the analyzers for multiple Roslyn versions (under `analyzers/dotnet/roslyn<version>/cs`), and your compiler automatically loads the highest build at or below its own Roslyn version. The lowest build targets Roslyn 4.8, which ships with the .NET 8.0 SDK (and the corresponding Visual Studio 2022 17.8 / VS Code C# tooling). Any SDK from .NET 8.0 onward — including newer SDKs used to target `net8.0`, `net9.0`, or `net10.0` — loads a matching build and runs the analyzers. Toolchains older than the .NET 8.0 SDK match no build and silently skip the analyzers (`CS9057` if they see a newer-only build); upgrade the SDK to enable them.

## Available Analyzers

When an analyzer fires, your IDE will show a diagnostic with a suggestion or code fix where applicable. The following analyzers are available:

| ID | Severity | When It Fires |
|----|----------|----------------|
| **BIDI001** | Error | `RegisterModule()` called on the driver after `StartAsync()`. A `StopAsync()` call returns the driver to the not-started state, so registration after a stop is not reported. The started state is tracked through `if`/`else`, `switch` and `try`/`catch`/`finally`, through the conditional (`?:`) and `switch` expressions, and through `for`, `foreach` and `while` loops, whose body may run zero times: a registration is reported only when the driver is started on every path that reaches it, so one in a `catch` after a `StartAsync()` in the `try` (which may have failed and rolled the driver back), one in a `switch` section other than the one that started the driver, one after a start that appears in only one arm of a conditional expression, or one after a loop whose body started the driver, is not reported (a `do`/`while` body runs at least once and is tracked straight through). Only a call on the driver variable itself counts: a same-named method on something reached through the driver (`driver.Custom.RegisterModule(...)`) belongs to that object, not to the driver. Assigning to the variable replaces the tracked state: a newly constructed driver counts as not started, and a driver that comes from anywhere else stops being tracked. A driver declared by a classic `await using (BiDiDriver driver = ...)` statement is tracked like a local |
| **BIDI002** | Error | Custom event registered (via `RegisterEvent()` on the driver) after `StartAsync()`; not reported after a `StopAsync()`, and tracked through branches, switches, try statements and loops as BIDI001 is, with the same restriction to calls on the driver itself. Adding observers with `AddObserver()` is not reported: observers may be added to an observable event at any time, including while the driver is running |
| **BIDI003** | Error | `RegisterTypeInfoResolverAsync()` called on the driver after `StartAsync()`; not reported after a `StopAsync()`, and tracked through branches, switches, try statements and loops as BIDI001 is, with the same restriction to calls on the driver itself |
| **BIDI004** | Info | Cancellable operation (`ExecuteCommandAsync`, `EvaluateAsync`, `CallFunctionAsync`, `GetTreeAsync`, `LocateNodesAsync`) called without `CancellationToken`; suggests passing one. `NavigateAsync` is reported by BIDI013 instead, never by both |
| **BIDI005** | Warning | Event observer added but event name not included in `Session.SubscribeAsync()`. Both the list constructor (`new SubscribeCommandParameters(["a", "b"])`) and the single-event constructor (`new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName)`) are recognized; string literals, constants and `EventName` property accesses all resolve. When any part of the subscription cannot be read at the call site — the parameters object held in a variable, an events list held in a variable, a spread element such as `[..events]`, a non-constant array element, or an object initializer that adds to `Events` — the subscribed set is unknowable and no warning is reported, because reporting from an incomplete set would accuse code that does subscribe. The driver the event is reached through may be spelled any way that has the driver type — a local, a parameter, a field accessed through `this`, a property of another object, or the result of a call — and expression-bodied members are analyzed like block-bodied ones |
| **BIDI006** | Warning | Event subscription handle not disposed. The handle is whatever `AddObserver` (an `EventObserver`), `AddDataCollector` (an `EventDataCollector`), or `Subscribe` on a `ToObservable()` sequence (an `ObservableEventSubscription`) returns. A `using` declaration, a classic `using (handle) { ... }` statement, `handle.Dispose()`, `handle?.Dispose()`, `DisposeAsync()`, `Unobserve()`, `RemoveObserver(observer.Id)`, and returning or storing the handle all count as handling it, including when the handle reaches the return or the assignment through parentheses, a cast, the null-forgiving operator, or a conditional expression |
| **BIDI007** | Warning | Blocking operation in event handler: `Thread.Sleep`/`Join`, `Task.Wait`, `.Result` on a `Task<T>` or `ValueTask<T>`, and `GetAwaiter().GetResult()`. The synchronization primitives BIDI016 lists (`lock`, `Monitor.Enter`/`TryEnter`, `SemaphoreSlim.Wait`, `ManualResetEventSlim.Wait`, `CountdownEvent.Wait`, `WaitHandle.WaitOne`, `Task.WaitAll`/`WaitAny`) are reported here as well, since they block the dispatching thread. Only an inline `async` handler is excepted, because there they are BIDI016's; a handler passed as a method group is covered here whether or not the method is `async`, so each is reported by exactly one rule. A method-group handler declared in another file (a partial-class part, a base class, a static helper) is walked too; its blocking operations are then reported at the handler argument of the `AddObserver` call rather than inside the other file. `RunHandlerAsynchronously` suppresses the diagnostic only when the handler actually runs off the dispatching thread: an `Action<T>` handler, an `async` lambda, or an `async` method group. A non-`async` `Task`-returning handler is still reported (with a message saying the option cannot help), because the option detaches the returned `Task` rather than moving where the handler starts |
| **BIDI008** | Warning | Unsafe cast of `EvaluateResult`; suggests pattern matching. An `as` conversion is always reported: the exemptions below apply to the cast form only, since `as` yields `null` rather than throwing and the rule points at the conversion regardless of how the result is guarded. A cast is not reported when it cannot fail: inside a `try` whose `catch` covers `InvalidCastException`, or guarded by the discriminator (`result.ResultType == EvaluateResultType.Success`) or a type test (`result is EvaluateResultSuccess`) in the enclosing `if`, conditional expression, `&&`, `switch` section, or an early-exit guard (`if (result.ResultType != EvaluateResultType.Success) return;`) earlier in the block |
| **BIDI009** | Error | Module command or `driver.ExecuteCommandAsync()` called before `StartAsync()`, or after `StopAsync()` without a new `StartAsync()`. A driver handed to something else that could start it is not tracked (see [Known Limitations](#known-limitations)), whether the mention is bare or wrapped in parentheses, a cast, the null-forgiving operator or a conditional expression (`StartHelperAsync((IBiDiCommandExecutor)driver)`); passing it to a module's constructor, as `driver.RegisterModule(new CustomModule(driver))` does, is not such a hand-off. A driver declared by a classic `await using (BiDiDriver driver = ...)` statement is tracked like a local |
| **BIDI010** | Error | Async module command, `driver.ExecuteCommandAsync()`, or one of the driver's own lifecycle operations (`StartAsync`, `StopAsync`, `RegisterTypeInfoResolverAsync`, `DisposeAsync`) not awaited (fire-and-forget), including one made through a null-conditional receiver (`driver?.Session.StatusAsync();`). A module is any type deriving from `Module`, whatever it is named |
| **BIDI012** | Info / Warning | `DisposeAsync()` called without `StopAsync()` first, including the implicit disposal of `await using var driver = ...` (in a block or as a top-level statement) and `await using (driver) { ... }`; suggests calling `StopAsync`. The driver may be a local or a field accessed through `this.`, and the `StopAsync()` is matched by the same name. The statements of a `switch` section count as a scope of their own, so a `StopAsync()` written next to the disposal there is seen. Reported as a **Warning** when the same method also assigns `TransportErrorBehavior.Collect` to any of the four error-behavior properties, because `DisposeAsync()` logs and discards collected errors—only `StopAsync()` throws them |
| **BIDI013** | Warning | Long-running operation (`NavigateAsync`, `PrintAsync`, `ReloadAsync`, `StartAsync`, `WaitForCapturedTasksAsync`, `WaitForCapturedTasksCompleteAsync`) called without `CancellationToken` |
| **BIDI014** | Warning | Parameterless constructor used for a command with a command-level reset property (i.e., a `public static Reset*` property, declared on the class or inherited from a base class, that returns the constructed `CommandParameters` type or one of its base types — this covers `SetGeolocationOverrideCoordinatesCommandParameters`, whose reset helper lives on `SetGeolocationOverrideCommandParameters`); suggests using `.Reset*`. Does not apply to property-level sentinel classes such as `SetViewportCommandParameters`, whose `Reset*` members return unrelated types. Assigning a property, calling a method on the object, or handing it to a method outside the library (which may configure it) all count as configuring it; only passing it to the library command that sends it does not. |
| **BIDI015** | Warning | String literal used for event name instead of `ObservableEvent.EventName`, in either `SubscribeCommandParameters` constructor form. The receiver is resolved from the identifier it roots in, which must be a local, a parameter, or a member reached through `this.`; a driver reached through another object, as in `fixture.Driver.Session.SubscribeAsync(...)`, is not matched and nothing is reported. Expression-bodied members are analyzed like block-bodied ones |
| **BIDI016** | Warning | Deadlock-prone synchronization in an `async` event handler: `lock`, `Monitor.Enter`/`TryEnter`, `SemaphoreSlim.Wait`, `ManualResetEventSlim.Wait`, `CountdownEvent.Wait`, `WaitHandle.WaitOne` (which covers `Mutex`, `Semaphore`, `ManualResetEvent` and `AutoResetEvent`), `SynchronizationContext.Send`, `Task.WaitAll`/`WaitAny`. Blocking calls such as `.Result` and `.Wait()` are BIDI007. `RunHandlerAsynchronously` suppresses the diagnostic |
| **BIDI017** | Warning | Adding to nullable list property without `??= new List<T>()` |
| **BIDI020** | Error | `WaitForCapturedTasksAsync()` or `WaitForCapturedTasksCompleteAsync()` called without a prior `StartCapturingTasks()` in the same method |
| **BIDI021** | Warning | `StartCapturingTasks()` called but no read method (`WaitForCapturedTasksAsync`, `WaitForCapturedTasksCompleteAsync`, `GetCapturedTasks`) follows in the same method. A read inside a lambda or local function in that method (one handed to `Task.Run`, say) counts as a read, wherever the lambda is written |
| **BIDI022** | Warning | Writing a value into `CommandParameters.AdditionalData` (via `Add`, `TryAdd`, indexer assignment, or the object-initializer spellings `AdditionalData = { ["key"] = value }` and `AdditionalData = { { "key", value } }`). The `Dictionary<string, object?>` values are serialized through reflection-based `JsonSerializer` overloads, which are not compatible with native AOT or IL trimming unless every value's runtime type is registered via `BiDiDriver.RegisterTypeInfoResolverAsync` |
| **BIDI023** | Warning | Module command (e.g., `NavigateAsync`, `EvaluateAsync`, or `ExecuteCommandAsync`, on any type deriving from `Module` whatever it is named) called inside an `AddObserver` event handler without `RunHandlerAsynchronously`. The driver's command pipeline dispatches events synchronously by default; calling a module command from within the handler can deadlock or produce unexpected behavior. As with BIDI007, the option only suppresses the diagnostic for an `Action<T>` handler, an `async` lambda, or an `async` method group; a non-`async` `Task`-returning handler is still reported, and a method-group handler declared in another file is walked and reported at the `AddObserver` handler argument |
| **BIDI024** | Error | `StartAsync()` called a second time on the same driver without an intervening `StopAsync()`. The transport is already connected, so the second call throws `WebDriverBiDiConnectionException`. Tracked per local variable within a method, constructor, or top-level program, through branches, switches, try statements and loops as BIDI001 is; a `StopAsync()` returns the driver to the not-started state, so a start / stop / start sequence is not reported, and so does assigning a newly constructed driver to the variable |
| **BIDI025** | Warning | An `async void` method is passed as an `AddObserver` handler. It binds to the `Action<T>` overload (not `Func<T, Task>`), so it runs fire-and-forget: exceptions thrown after its first `await` are unobserved async-void faults that can crash the process, and the observer is considered complete before the handler's async work finishes. An `async` lambda or `async Task` method group binds to `Func<T, Task>` and is not reported |
| **BIDI026** | Error | An explicit `ExecuteCommandAsync<T>` type argument disagrees with the command's result type (e.g., `ExecuteCommandAsync<WrongResult>(new StatusCommandParameters())`). The generic `CommandParameters<T>` overload no longer applies, so the call binds to the non-generic `CommandParameters` overload, compiles, and then throws `WebDriverBiDiException` at runtime because the response cannot be converted to `T`. A matching or base type argument, or an inferred one, is not reported. Skipped when either type is an open generic type parameter |
| **BIDI027** | Error | `RegisterEvent()` called with a built-in protocol event name (e.g., `RegisterEvent<T>("log.entryAdded", …)`). Modules register those names in their constructors, so `RegisterEvent` throws `ArgumentException` at runtime. The built-in names are read from the library's `[ObservableEventName]` attributes. Only a compile-time-constant name argument is checked; observe a built-in event through its `ObservableEvent` property instead |
| **BIDI028** | Warning | A compile-time-constant value assigned to a command-parameter property is outside the WebDriver BiDi specification range declared by `[SpecRange]` (e.g., `new ImageFormat { Quality = 1.5 }`, where `Quality` is `[0.0, 1.0]`). The range is read from the property's `[SpecRange]` attribute; a bound may be open (`±∞`), and an upper bound may be exclusive (the specification's CDDL `...` operator — e.g., `GeolocationCoordinates.Heading` is `[0.0, 360.0)`, so a constant `360.0` is flagged). The library does not validate the range at runtime, so a conforming remote end rejects the value. A property's declared reset sentinel (such as `-1`) is treated as valid, and runtime or dynamic (non-constant) values are never flagged |
| **BIDI029** | Error | A driver member that throws once the driver is disposed (`StartAsync`, `ExecuteCommandAsync`, `RegisterEvent`, `RegisterModule`, `GetModule`, `RegisterTypeInfoResolverAsync`, or any module command) is called after `DisposeAsync()`, including the implicit disposal of `await using (driver) { ... }`. Disposal is terminal: the driver cannot be restarted. `StopAsync()` and a second `DisposeAsync()` are not reported, because neither throws on a disposed driver, and neither is reaching an observable event through a module property. Tracked per local variable, through `if`/`else`, `switch`, `try`/`catch`/`finally` and `for`/`foreach`/`while` loops (whose body may run zero times), and reported only when the driver is disposed on every path that reaches the use; reassigning the variable clears the state |
| **BIDI030** | Warning | `StartCapturingTasks()` called on an `EventObserver` whose capture session is already active, which throws `WebDriverBiDiException`. The companion of BIDI020. A `StopCapturingTasks()` ends the session; so may a `WaitForCapturedTasksAsync` or `WaitForCapturedTasksCompleteAsync`, which ends it when it collects its full batch, so a start after either is never reported |
| **BIDI031** | Info | The handle returned by `AddObserver`, `AddDataCollector`, or `Subscribe` on a `ToObservable()` sequence is discarded (the call is a bare expression statement), leaving nothing with which to remove the subscription: `Dispose()` is the handle's member, an observer also accepts `Unobserve()`, and `RemoveObserver` needs an observer's `Id`. Reported at `Info` because a subscription intended to last the life of the driver is a legitimate design; an explicit discard (`_ = ...`) is never reported |

## Code Fixes

Many analyzers provide automatic code fixes. In Visual Studio or VS Code, use the lightbulb or quick-action menu on the diagnostic to apply the suggested fix.

The following analyzers have code fix providers:

- **BIDI001** — Moves the `RegisterModule()` statement before `StartAsync()`, together with any local declarations it depends on that were declared after the start (`CustomModule module = new(driver);`). No fix is offered when such a declaration awaits something (it may need the started driver), when a statement between the start and the registration assigns a variable the moved statements read (an assignment, or an `out` or `ref` argument), since it could not move with them, or when the registration is not a statement of the same block as the `StartAsync()` — inside an `if`, a loop or a `try`, braced or not — since hoisting it out would change whether or how often it runs
- **BIDI002** — Moves the `RegisterEvent()` statement before `StartAsync()`, with the same dependency handling as BIDI001
- **BIDI003** — Moves the `RegisterTypeInfoResolverAsync()` statement before `StartAsync()`, with the same dependency handling as BIDI001
- **BIDI004** — Adds a `CancellationToken` argument: either `CancellationToken.None`, or an existing token in scope, offered by its own name. The second action appears only when such a token exists, and the type name is qualified when the file has no `using System.Threading`. The argument is named after the token parameter of the overload the rule resolved, which matters on a user-written `Module` subclass, where that parameter need not be called `cancellationToken`
- **BIDI005** — Adds missing event name to `Session.SubscribeAsync()` call. A single-event constructor argument is rewritten into a collection expression holding both events on C# 12 and later, and into an implicit array (`new[] { ... }`) on earlier language versions, which means the same thing and compiles everywhere
- **BIDI006** — Adds a `using` declaration for the handle. Offered only on C# 8 and later, since that is when using declarations arrived; below it the diagnostic still reports and the handle is left to be disposed in whichever way suits the code. Not offered where the keyword alone would not compile: a declaration directly in a switch section (CS8647), or a variable the code assigns to later (CS1656)
- **BIDI007** — For an `Action<T>` handler or an `async` lambda, adds the `ObservableEventHandlerOptions.RunHandlerAsynchronously` option to the `AddObserver` call. For a non-`async` `Task`-returning lambda, converts it to an `async` lambda whose first statement is `await Task.Yield();` (rewriting `return Task.CompletedTask;` / `return <task>;` accordingly) and adds the option if it is missing. No fix is offered when the handler is a method group, because the method declaration itself would have to change
- **BIDI008** — Replaces unsafe cast with pattern matching. A declaration initialized by the cast becomes the pattern variable, and every later statement through the last one that uses it (or a local declared by a moved statement) moves into the `if` block; a cast nested inside a declaration's initializer moves the declaration into the block the same way, so the declared variable stays in scope for its uses. No fix is offered where the wrap would change the code: when the statements that would move always leave the member, so that wrapping them costs the method its return on every path (CS0161); when an `as` conversion is followed by a null guard that leaves the member, which the pattern variable would turn into dead code; or when the cast operand names something declared inside the statement being wrapped, such as the parameter of an expression-bodied lambda (`results.Select(r => ((EvaluateResultSuccess)r).Result)`), which would not be in scope where the pattern is placed. A cast inside a statement lambda is converted within the lambda's own block. The pattern variable a fix introduces (`success`, `exception`) is numbered (`success1`, ...) when that name is already in use in the member
- **BIDI009** — Moves the command execution after the existing `StartAsync` call on the same driver. The command may move into a block nested around the start that is entered unconditionally and exactly once, such as a `try` or a `using` body, but not into an `if`, a loop or a `switch` section, and not into an unbraced embedded statement, which is no statement list at all. A command that declares a local is moved only when every use of that local still follows the declaration and stays inside the block it lands in
- **BIDI010** — Adds `await` to the fire-and-forget command, offered only where `await` is legal: the nearest enclosing function must be `async`, or the call must be a top-level statement
- **BIDI012** — Adds `await driver.StopAsync()` before `DisposeAsync()` (using the receiver as written, so `this.driver` stays `this.driver`); for `await using` forms, at the end of the scope that disposes the driver, including a top-level program. Offered only where `await` is legal, so not for a blocking `DisposeAsync().AsTask().GetAwaiter().GetResult()` in a synchronous `Dispose()`, nor in a member that takes no `async` modifier such as a constructor or a property accessor. When the disposal is the unbraced embedded statement of an `if` or a loop, braces are added around both statements so the stop stays inside the branch. Where a scope ends in a `return` or a `throw`, the stop goes in front of it so that it runs; it is not offered at all when that statement's expression reads the driver, because there is then no position that both runs and leaves the result alone
- **BIDI013** — Same fix as BIDI004; the two rules report the same shape and share a provider
- **BIDI014** — Replaces parameterless constructor with `.Reset*` property (qualified by the type that declares it). A local is retyped only when the property's own return type cannot be assigned to the declared type; where an inherited `.Reset*` returns the derived type, as the library's does, the declaration is left as written
- **BIDI015** — Replaces string literal with `ObservableEvent.EventName` property, using the receiver as written, so `this.driver` stays `this.driver`
- **BIDI017** — Adds null-coalescing assignment before adding to nullable list; the element type is qualified when the file does not import its namespace
- **BIDI020** — Inserts `observer.StartCapturingTasks()` before the offending `WaitForCapturedTasksAsync` or `WaitForCapturedTasksCompleteAsync` call, adding braces around both statements when the call is the unbraced embedded statement of an `if` or a loop
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

<!-- inline-csharp: a suppression example whose point is the diagnostic it suppresses -->
```csharp
#pragma warning disable BIDI009 // command executed before StartAsync (set up in a helper)
await driver.BrowsingContext.NavigateAsync(navParams);
#pragma warning restore BIDI009
```

**Suppress on a member** with an attribute:

<!-- inline-csharp: a suppression example whose point is the diagnostic it suppresses -->
```csharp
[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "BIDI006:Event subscription handle should be disposed", Justification = "Observer lifetime is managed by the test fixture.")]
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

**Warning.** An event subscription handle is neither disposed nor unobserved. The handle is whatever `AddObserver`, `AddDataCollector`, or `Subscribe` on a `ToObservable()` sequence returns; each of the three keeps its subscription alive until it is disposed, and a collector or a subscription goes on queueing events in the meantime. See [Common Pitfalls — Resource Cleanup](../common-pitfalls.md#resource-cleanup).

### BIDI007

**Warning.** A blocking operation appears in an event handler. See [Common Pitfalls — Blocking the Transport Thread](../common-pitfalls.md#pitfall-blocking-the-transport-thread-with-synchronous-handlers).

### BIDI008

**Warning.** An `EvaluateResult` is cast unsafely; pattern matching is suggested.

### BIDI009

**Error.** A module command or `ExecuteCommandAsync()` is called before `StartAsync()` (or after `StopAsync()` without a new `StartAsync()`).

### BIDI010

**Error.** An async module command is not awaited (fire-and-forget). The driver's own lifecycle
operations — `StartAsync`, `StopAsync`, `RegisterTypeInfoResolverAsync` and `DisposeAsync` — are
reported on the same footing: an un-awaited `StartAsync` leaves the connect racing the next command,
and an un-awaited `StopAsync` discards the `AggregateException` carrying everything collected under
[`TransportErrorBehavior.Collect`](error-handling.md#collect-mode). A call whose task is consumed —
awaited, assigned, passed on, or wrapped by `AsTask`, `Preserve` or `ConfigureAwait` and then
awaited — is not reported.

### BIDI012

**Info / Warning.** `DisposeAsync()` is called without a prior `StopAsync()`. See [Error Handling — Collect Mode](error-handling.md#collect-mode).

### BIDI013

**Warning.** A long-running operation (`NavigateAsync`, `PrintAsync`, `ReloadAsync`, `StartAsync`, `WaitForCapturedTasksAsync`, `WaitForCapturedTasksCompleteAsync`) is called without a `CancellationToken`.

### BIDI014

**Warning.** A parameterless constructor is used for a command that exposes a `.Reset*` property. See [API Design Guide — Required vs Optional Parameters](api-design.md#required-vs-optional-parameters).

### BIDI015

**Warning.** A string literal is used for an event name instead of `ObservableEvent.EventName`. The driver is
resolved from the `Session.SubscribeAsync` receiver, so it may be a local, a parameter, a field or a property,
written plainly or through `this.`, and a type deriving from `BiDiDriver` resolves its inherited module
properties. The suggested replacement is built from the receiver as written, so `this.driver.Session` yields
`this.driver.Log.OnEntryAdded.EventName`. The rule stays silent when the receiver does not root in a name at
all (`GetDriver().Session.SubscribeAsync(...)`), when the session module is held on its own with no driver to
name, and when the driver is a generic type parameter.

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

### BIDI028

**Warning.** A compile-time-constant value assigned to a command-parameter property falls outside the WebDriver BiDi specification range that the property's `[SpecRange]` attribute declares. A range's upper bound may be exclusive (the specification's CDDL `...` operator), in which case a constant equal to that bound is also flagged. The library deliberately does not validate these ranges at run time—the value is representable on the wire—but a conforming remote end rejects it when the command executes. Only compile-time constants are checked; runtime or dynamic values, `null`, and a property's declared reset sentinel value are never flagged.

<!-- inline-csharp: written to trip the analyzer under discussion, so it cannot compile in the snippets project -->
```csharp
// Flagged: Quality's specification range is [0.0, 1.0].
ImageFormat format = new ImageFormat { Quality = 1.5 };

// Not flagged: within range.
ImageFormat format = new ImageFormat { Quality = 0.9 };

// Not flagged: MaxDomDepth's range is [0, ∞) with a reset sentinel of -1.
SerializationOptions options = new SerializationOptions { MaxDomDepth = -1 };
```

This is a `Warning` by design so it never blocks a build; downgrade or suppress it (see [Configuration and Suppression](#configuration-and-suppression)) if you intend to send an out-of-range constant.

### BIDI029

**Error.** A driver is used after it has been disposed. `BiDiDriver.DisposeAsync` marks the driver disposed before it releases anything, and `StartAsync`, `ExecuteCommandAsync`, `RegisterEvent`, `RegisterModule`, `GetModule` and `RegisterTypeInfoResolverAsync` all throw `ObjectDisposedException` from that point on — as does every module command, which reaches the same guard through `ExecuteCommandAsync`. Disposal is terminal: unlike `StopAsync()`, it cannot be undone by starting again, so the only remedy is a new driver.

<!-- inline-csharp: written to trip the analyzer under discussion, so it cannot compile in the snippets project -->
```csharp
BiDiDriver driver = new BiDiDriver();
await driver.StartAsync(url);
await driver.DisposeAsync();

// Flagged: the driver is disposed.
await driver.Session.StatusAsync();

// Also flagged: a disposed driver cannot be restarted.
await driver.StartAsync(url);
```

`StopAsync()` and a second `DisposeAsync()` are not flagged, because neither throws on a disposed driver, and neither is reaching an observable event through a module property (`driver.Log.OnEntryAdded.AddObserver(...)` touches no disposal guard). The implicit disposal of `await using (driver) { ... }` counts as a disposal for the statements that follow it. Rebinding the variable clears the state:

<!-- inline-csharp: written to trip the analyzer under discussion, so it cannot compile in the snippets project -->
```csharp
await driver.DisposeAsync();
driver = new BiDiDriver();

// Not flagged: the name refers to a different driver now.
await driver.StartAsync(url);
```

The disposal state is tracked per local variable through `if`/`else`, `switch` and `try`/`catch`/`finally`, and a use is reported only when the driver is disposed on every path that reaches it — so a dispose in one branch, or one in a `try` whose `catch` uses the driver, is not reported. A driver disposed or reassigned inside a lambda or local function is not tracked at all, since a nested function runs when its delegate is invoked rather than where it is written.

### BIDI030

**Warning.** `StartCapturingTasks()` is called on an `EventObserver` that already has an active capture session. An observer permits one session at a time; the second call throws `WebDriverBiDiException`. This is the companion of [BIDI020](#bidi020), which reports the opposite mistake.

<!-- inline-csharp: written to trip the analyzer under discussion, so it cannot compile in the snippets project -->
```csharp
observer.StartCapturingTasks();

// Flagged: a session is already active.
observer.StartCapturingTasks();
```

`StopCapturingTasks()` ends the session, and so may a wait: `WaitForCapturedTasksAsync` and `WaitForCapturedTasksCompleteAsync` end it themselves when they collect the full batch they were asked for. Because whether that happened is a runtime outcome, a start after either is never reported:

<!-- inline-csharp: written to trip the analyzer under discussion, so it cannot compile in the snippets project -->
```csharp
observer.StartCapturingTasks();
Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));

// Not flagged: the wait may have ended the session.
observer.StartCapturingTasks();
```

As with BIDI020, the state is tracked per local variable and merged across branches and `for`/`foreach`/`while` loops; a start is reported only when a session is certainly active on every path that reaches it.

### BIDI031

**Info.** The handle that `AddObserver`, `AddDataCollector`, or `Subscribe` on a `ToObservable()` sequence returns is discarded, so nothing can remove the subscription later: `Dispose()` is a member of that handle, an observer also accepts `Unobserve()`, and `RemoveObserver` needs an observer's `Id`.

<!-- inline-csharp: written to trip the analyzer under discussion, so it cannot compile in the snippets project -->
```csharp
// Flagged: nothing can remove this observer.
driver.Log.OnEntryAdded.AddObserver(entry => Console.WriteLine(entry.Text));

// Not flagged: the handle is kept, and disposal removes the observer.
using EventObserver<EntryAddedEventArgs> observer =
    driver.Log.OnEntryAdded.AddObserver(entry => Console.WriteLine(entry.Text));
```

This is reported at `Info` rather than as a warning because a subscription meant to last as long as the driver is a legitimate design, and one added that way never needs removing. An explicit discard (`_ = driver.Log.OnEntryAdded.AddObserver(...)`) records that intent and is never reported. A `Subscribe` call is recognised by its receiver rather than its return type, because `IObservable<T>` declares the method as returning `IDisposable`; only an observable over this library's event arguments is matched.

## Related Documentation

| Analyzer Topic | See Also |
|----------------|----------|
| Registration timing (BIDI001, BIDI002, BIDI003) | [Common Pitfalls - Module Registration Timing](../common-pitfalls.md#module-registration-timing) |
| Event subscription (BIDI005) | [Common Pitfalls - Event Subscription](../common-pitfalls.md#event-subscription) |
| Blocking handlers (BIDI007, BIDI016) | [Common Pitfalls - Blocking the Transport Thread](../common-pitfalls.md#pitfall-blocking-the-transport-thread-with-synchronous-handlers) |
| Module commands in event handlers (BIDI023) | [Common Pitfalls - Blocking the Transport Thread](../common-pitfalls.md#pitfall-blocking-the-transport-thread-with-synchronous-handlers) |
| Observer disposal (BIDI006, BIDI031) | [Common Pitfalls - Resource Cleanup](../common-pitfalls.md#resource-cleanup) |
| Driver lifecycle and disposal (BIDI012) | [Error Handling - Collect Mode](error-handling.md#collect-mode) |
| Use after disposal (BIDI029) | [Core Concepts - Proper Disposal](../core-concepts.md#proper-disposal) |
| Nullable collections (BIDI017) | [Common Pitfalls - Null vs Empty Collections](../common-pitfalls.md#null-vs-empty-collections) |
| Reset parameters (BIDI014) | [API Design Guide - Required vs Optional Parameters](api-design.md#required-vs-optional-parameters) |
| Capture session ordering (BIDI020, BIDI021, BIDI030) | [Events and Observables - Event Synchronization](../events-observables.md#event-synchronization) |
| AdditionalData and AOT (BIDI022) | [API Design Guide - Protocol Extensions via AdditionalData](api-design.md#protocol-extensions-via-additionaldata), [AOT Compatibility](aot-compatibility.md) |

## Known Limitations

No analyzer performs whole-program flow analysis; none of them correlate data across files, classes, or — for most of them — across method boundaries. Each rule falls into one of the scopes below. Understanding which scope a given rule uses helps explain why it may not fire in a particular situation.

### Analyzer scope by rule

| Scope | What the analyzer sees | Rules |
|-------|------------------------|-------|
| **Intra-procedural** — single method body | The analyzer walks one method at a time and correlates statements within that method (e.g., "was `StartAsync` called before this line?"). It cannot see into other methods. | BIDI001, BIDI002, BIDI003, BIDI005, BIDI006, BIDI009, BIDI012, BIDI014, BIDI015, BIDI020, BIDI021, BIDI024, BIDI029, BIDI030 |
| **Per-invocation** — single call site | The analyzer examines each matching invocation in isolation (argument list, surrounding expression). There is no correlation with other statements in the method. | BIDI004, BIDI010, BIDI013, BIDI017, BIDI022, BIDI025, BIDI026, BIDI027, BIDI031 |
| **Per-expression** — single expression | The analyzer examines each matching syntactic expression (e.g., a cast, an assignment) in isolation. | BIDI008, BIDI022, BIDI028 |
| **Per-invocation with handler-body descent** — call site plus the handler it passes | The analyzer inspects each matching `AddObserver(...)` call and also walks into the handler body to look for patterns. When the handler is an inline lambda, the body is right there. When the handler is passed as a method reference (e.g., `AddObserver(this.HandleEvent)`), BIDI007 and BIDI023 resolve the reference and walk that method body too, in whichever file it is declared; BIDI016 inspects only inline `async` lambda handlers and does not follow method references. None of them continue transitively into further methods that the handler body calls. | BIDI007, BIDI016, BIDI023 |

BIDI022 sits in two tiers because it registers two shapes: the `Add` and `TryAdd` invocations, and the indexer and object-initializer assignments. Each is judged on its own, with no correlation between them.

### What this means in practice

If you split setup across helper methods (common in test frameworks or automation wrappers), analyzers in the **intra-procedural** or **per-invocation** tiers will not correlate calls in different methods:

<!-- inline-csharp: a sketch of two helper methods with elided bodies, not compilable code -->
```csharp
// SetupAsync() — `driver` is a field, so nothing here is tracked at all: BIDI001 and
// BIDI009 begin tracking only at a local declaration whose initializer is a driver.
async Task SetupAsync() { driver = new BiDiDriver(); await driver.StartAsync(...); }

// TestAsync() — BIDI009 cannot detect that StartAsync was called in SetupAsync
async Task TestAsync() { driver.RegisterModule(new CustomModule(driver)); } // no diagnostic
```

BIDI007 and BIDI023 are the exceptions: they will follow a single hop from an `AddObserver(...)` call to a method reference used as the handler, but they will not walk further than that. BIDI016 analyzes only inline `async` handlers; a handler passed as a method reference is not analyzed. All three treat the three spellings of an inline handler alike — a simple lambda, a parenthesized lambda, and an anonymous method written with the `delegate` keyword.

Every intra-procedural rule takes its analysis roots from the statements of a method, a constructor, or a top-level program. The **state-tracking** rules — BIDI001, BIDI002, BIDI003, BIDI009, BIDI020, BIDI024, BIDI029 and BIDI030 — walk those statements in order and do not descend into a lambda, an anonymous method, or a local function, because that code runs when the delegate is invoked rather than where it is written. A driver or an observer *declared inside* one of those is therefore never tracked by them, and they report no diagnostic for it:

<!-- inline-csharp: written to show what the analyzer does not report, so it cannot compile in the snippets project -->
```csharp
// Nothing here is analyzed: the driver is declared inside the lambda.
Func<Task> run = async () =>
{
    BiDiDriver driver = new BiDiDriver();
    await driver.Session.StatusAsync();   // no BIDI009, though the driver was never started
};
```

The state-tracking rules treat the body of a `for`, `foreach` or `while` loop as a path that may not run, exactly like the branch of an `if` with no `else`: a start, a stop, a disposal or a capture session opened inside the loop is not certain for the code after it, while within one execution of the body the earlier statements of that body are certain. A `do`/`while` body runs at least once and is walked straight through.

The remaining intra-procedural rules do not track order, so they search the whole body and do descend into nested functions. BIDI005, BIDI006, BIDI014, BIDI015 and BIDI012's `await using` detection all work this way, which is why an observer declared inside a lambda and never disposed is still reported by BIDI006.

BIDI021 sits between the two groups. It tracks order like the state-tracking rules — the read has to follow the `StartCapturingTasks()` call — but it also looks inside nested functions, because handing the read to `Task.Run` is an ordinary way to write one. A read written inside a lambda, an anonymous method or a local function cannot be placed in the textual order, so it is treated as deferred and satisfies every `StartCapturingTasks()` call on that observer in the member, wherever the nested function is written. A `StartCapturingTasks()` call inside a nested function is not tracked at all, for the same reason.

BIDI009 goes further in the other direction. Because it reports an **Error**, it reports only when it is certain, so it stops tracking a driver that this method hands to something else — passed as an argument, returned, assigned to a field, aliased to another variable, or placed in a collection — since the code it was handed to may start it. The one argument position that does not count is a module's constructor: `driver.RegisterModule(new CustomModule(driver))` hands the driver to a module, which holds it to issue commands and cannot start it.

<!-- inline-csharp: calls an undefined helper to show where tracking stops -->
```csharp
BiDiDriver driver = new BiDiDriver();
await StartHelperAsync(driver);                 // the driver escapes here
await driver.Session.StatusAsync();             // no diagnostic: the state is unknown, not known-unstarted
```

A driver captured by a lambda or local function that calls `StartAsync` or `StopAsync`, or that assigns to the variable, is treated the same way, because a nested function runs when its delegate is invoked rather than where it is written. A nested function that only issues commands does not suppress the rule.

BIDI020, BIDI021 and BIDI030 stop tracking an observer on the same terms: one this method passes to a helper, returns, stores, aliases, or places in a collection may have a capture session opened, closed, or read by code these rules cannot see. BIDI020 and BIDI030 also stop tracking an observer whose `StartCapturingTasks` or `StopCapturingTasks` is called inside a nested function. Only the escaping name stops being tracked; other observers in the same method are still reported on.

**Runtime enforcement remains correct.** The library still throws `InvalidOperationException` or `ObjectDisposedException` at runtime when these patterns are violated. The analyzers provide compile-time guidance where they can; they do not replace runtime validation.

## See Also

- [Common Pitfalls](../common-pitfalls.md): Detailed explanations of the issues the analyzers catch
- [Error Handling](error-handling.md): Transport error behavior and exception handling
- [API Design Guide](api-design.md): Command parameter patterns and timeout/cancellation
