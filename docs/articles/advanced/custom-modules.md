# Custom Modules

This guide explains how to create custom modules to extend WebDriverBiDi.NET with your own commands and functionality.

> **Advanced guide:** This article is for framework authors and library extenders. If you are building a typical automation application, use `BiDiDriver` and the built-in modules directly instead of creating custom modules.

## Overview

WebDriverBiDi.NET's module system is extensible, allowing you to:
- Implement custom WebDriver BiDi commands
- Create higher-level abstractions over protocol commands
- Integrate experimental or browser-specific features
- Build reusable automation patterns

## Module Basics

### Module Structure

All modules inherit from the `Module` base class:

[!code-csharp[Module Structure](../../code/advanced/CustomModulesSamples.cs#ModuleStructure)]

### Registering a Module

[!code-csharp[Register and Use Module](../../code/advanced/CustomModulesSamples.cs#RegisterandUseModule)]

After registration, retrieve a module by name using `GetModule<T>`:

[!code-csharp[Get Module By Name](../../code/advanced/CustomModulesSamples.cs#GetModuleByName)]

This is useful for reaching a module by name when you hold a reference to the driver but not to the module instance you registered. `GetModule<T>` throws `InvalidCastException` if the registered module cannot be cast to `T`, and `ArgumentException` if no module with that name has been registered.

> [!NOTE]
> `GetModule<T>` is declared on `BiDiDriver`, not on `IBiDiModuleHost`. The `Module` base class stores its host as `IBiDiModuleHost` (see the note under [Module Events](#module-events)), so calling `GetModule<T>` from inside a module requires a cast to `BiDiDriver`. Prefer passing any module a custom module depends on into its constructor instead.

## Creating Commands

### Command Parameters

Define parameters that extend `CommandParameters`:

[!code-csharp[Command Parameters](../../code/advanced/CustomModulesSamples.cs#CommandParameters)]

Override `MethodName` with the protocol method the parameters are sent as. It, and `ResponseType`, describe the command rather than carry its parameters, so the transport never writes either inside `params`; the override needs no `[JsonIgnore]`. The same holds for a parameters type whose metadata comes from a type-info resolver you register.

### Command Results

Define results that extend `CommandResult`:

[!code-csharp[Command Result](../../code/advanced/CustomModulesSamples.cs#CommandResult)]

A received member needs an accessor the serializer can set. A `private set` is not one: `System.Text.Json` sets only public accessors unless `[JsonInclude]` opts a non-public one in, which is why the members above pair `[JsonInclude]` with an `internal set`. Without both, the member silently keeps its default value, and what the remote end sent for it is diverted to `AdditionalData`. The same applies to the members of your event argument types.

### Command Method

Implement the command in your module:

[!code-csharp[Command Method](../../code/advanced/CustomModulesSamples.cs#CommandMethod)]

## Example: Page Utilities Module

Let's create a complete custom module for common page operations:

[!code-csharp[Page Utilities Module](../../code/advanced/CustomModulesSamples.cs#PageUtilitiesModule)]

### Using the Custom Module

[!code-csharp[Using Page Utilities Module](../../code/advanced/CustomModulesSamples.cs#UsingPageUtilitiesModule)]

## Example: Testing Utilities Module

Create a module with common testing helpers:

[!code-csharp[Test Utilities Module](../../code/advanced/CustomModulesSamples.cs#TestUtilitiesModule)]

## Example: Performance Monitoring Module

The Performance module pattern uses `Script.EvaluateAsync` to run `performance.getEntriesByType('navigation')` and `performance.getEntriesByType('resource')` in the browser. See the [Test Utilities Module](#example-testing-utilities-module) example for the pattern of wrapping `ExecuteCommandAsync` with custom methods.

## Module Events

You can also expose observable events from your custom module:

[!code-csharp[Custom Events Module](../../code/advanced/CustomModulesSamples.cs#CustomEventsModule)]

`RegisterObservableEvent(invocable)` registers the event with the driver and, each time the remote end sends it,
raises the `ObservableEventInvocable<T>` with the deserialized data as the event args. When the type you deserialize
is not the event args type you expose, use the overload
`RegisterObservableEvent<T, TEventArgs>(invocable, Func<T, TEventArgs> eventArgsConverter)`: it builds the event args
through the AOT-safe `ToEventArgs` factory overload described below. Both overloads also connect the invocable to the
driver's reporting of asynchronous observer failures, described below.

The invocable is also how a module raises an event it produces itself rather than one the remote end sends:
`InvokeNotifyObserversAsync(args)` notifies its observers, and `InvokeSetObserverErrorReporter(reporter)` sets the
callback that receives the failures of asynchronous observers, which the registration overloads set for you. Expose
the invocable to callers as its base type, `ObservableEvent<T>`, so that only your module can raise it.

> **Why `IBiDiModuleHost`, not `IBiDiDriverConfiguration`?**
> The `Module` base class constructor requires `IBiDiModuleHost` because event registration
> goes through that interface. When your module calls `this.RegisterObservableEvent<T>(...)` in its
> constructor, the base class calls `this.Driver.RegisterEvent<T>(...)` internally.
> `RegisterEvent<T>` is defined on `IBiDiModuleHost`; it is not present on
> `IBiDiDriverConfiguration`, which only exposes `RegisterModule` and `RegisterTypeInfoResolverAsync`.
> Passing a `BiDiDriver` instance satisfies both interfaces, so your module constructor always
> receives a `BiDiDriver` in practice.

> **Reporting observer failures from a custom executor.**
> A handler registered with `ObservableEventHandlerOptions.RunHandlerAsynchronously` has its task
> detached, so a failure in it cannot be thrown at the code that raised the event. `Module` routes such
> a failure to the executor it was constructed with, if that executor implements
> `IEventObserverErrorReporter`. `BiDiDriver` does, which is how the failure reaches
> `EventHandlerExceptionBehavior` and `OnEventHandlerErrorOccurred`. If you write your own
> `IBiDiModuleHost`, implement that interface too, or the failures of your modules' asynchronous
> observers are observed and then discarded — never thrown, never reported:
>
> <!-- inline-csharp: a member sketch, not a callable method -->
> ```csharp
> public Func<EventObserverErrorInfo, Task> EventObserverErrorReporter => this.ReportFaultAsync;
> ```
>
> `EventObserverErrorInfo` names the event, the observer, and the exception, and says whether the
> handler was asynchronous and whether the failure arrived after the handler returned.

### What the invoker receives

`RegisterEvent<T>` takes a `Func<EventInfo<T>, Task>`. `EventInfo<T>` carries three things:

| Member | Contents |
|---|---|
| `EventData` | The deserialized `T` — your event args type |
| `AdditionalData` | Extension properties the remote end put **inside** the event's `params` object |
| `AdditionalEventProperties` | Extension properties the remote end put on the **event envelope**, alongside `method` and `params` |

Both dictionaries are `ReceivedDataDictionary` and are empty rather than null when the remote end sent
nothing extra, so a vendor-prefixed field can be read without a null check. They are read-only;
`ToWritableCopy()` returns a `Dictionary<string, object?>` copy you can change, or whose entries you can add to a
command's `AdditionalData` to send them on.

`EventInfo<T>.ToEventArgs` packages all three into the event args you hand to your `ObservableEvent<T>`,
copying both dictionaries onto the result so the extension data survives the hop. The overload taking a
`Func<T, TEventArgs>` factory is the one to use in AOT or trimmed applications; the parameterless overload
uses `T` itself as the event args type and throws `WebDriverBiDiException` when `TEventArgs` is anything
else, which the [BIDI035](analyzers.md#bidi035) analyzer reports.

Mark each `ObservableEvent<T>` property on your module with `[ObservableEventName("your.event")]`, naming the
same string you passed to the `ObservableEventInvocable<T>` constructor, as the module sample above does. The library's analyzers read that attribute
from compiled metadata, which is what lets BIDI005 and BIDI015 recognise your module's events in a consuming
project that references your module as a package.

## Enum Wire Values

Enums used in command parameters, results, and event args serialize as JSON strings through `EnumValueJsonConverter<T>` (applied with a `JsonConverter` attribute on the enum). By default the wire value is the member name lowercased (`Enabled` becomes `"enabled"`), and deserialization is strict: an incoming string that matches no member throws a `JsonException` rather than mapping silently. Three attributes adjust this for a custom module's enums:

- `StringEnumValueAttribute` (on a member) sets the exact wire string when lowercasing the name is not enough, such as hyphenated protocol values.
- `StringEnumUnmatchedValueAttribute<T>` (on the enum) names the member to which any unmatched incoming string deserializes, opting that enum out of strict deserialization.
- `StringEnumNullSentinelValueAttribute<T>` (on the enum) names a member that serializes as JSON `null`, for protocol members where sending null means "reset to default".

[!code-csharp[Custom Enum Wire Values](../../code/advanced/CustomModulesSamples.cs#CustomEnumWireValues)]

## Best Practices

### 1. Namespace Your Commands

Use a clear module prefix for your custom commands:

[!code-csharp[Namespace Command](../../code/advanced/CustomModulesSamples.cs#NamespaceCommand)]

### 2. Provide Defaults

Make your modules easy to use with sensible defaults:

[!code-csharp[Optional Timeout Default](../../code/advanced/CustomModulesSamples.cs#OptionalTimeoutDefault)]

### 3. Document Your Module

Add XML documentation comments to your module class and methods. See the [Page Utilities Module](#example-page-utilities-module) for the structure.

### 4. Handle Errors Gracefully

[!code-csharp[Error Handling in GetElementText](../../code/advanced/CustomModulesSamples.cs#ErrorHandlinginGetElementText)]

### 5. Make Modules Testable

Extract an interface from your module (e.g., `IPageUtilities` with `WaitForElementAsync` and `GetElementTextAsync`), implement it in your module class, and create a `MockPageUtilities` for unit tests.

## Packaging Custom Modules

Create a NuGet package for reusable modules:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackageId>MyCompany.WebDriverBiDi.Extensions</PackageId>
    <Version>1.0.0</Version>
    <Authors>Your Name</Authors>
    <Description>Custom WebDriver BiDi modules</Description>
  </PropertyGroup>
</Project>
```

Then add the WebDriverBiDi dependency with the .NET CLI, pinning an exact version so an update stays deliberate (see [Pinning an exact version](../getting-started.md#pinning-an-exact-version)):

```bash
dotnet add package WebDriverBiDi
```

> **AOT support:** If your package will be used in AOT environments, include a source-generated `JsonSerializerContext` with `[JsonSerializable]` attributes for your custom types. See [AOT Compatibility](aot-compatibility.md) for details.

## Advanced: Implementing Protocol Extensions

For actual protocol extensions (not just helper methods):

[!code-csharp[Experimental Module](../../code/advanced/CustomModulesSamples.cs#ExperimentalModule)]

## Custom Transport

For scenarios requiring custom message processing — for example, injecting test doubles, logging
raw frames, or applying transformations to incoming messages — you can subclass `Transport` and
override `CreateIncomingMessage`:

[!code-csharp[Logging Transport](../../code/advanced/CustomModulesSamples.cs#LoggingTransport)]

Pass your custom transport to `BiDiDriver` via the constructor overload that accepts a `Transport`:

[!code-csharp[Use Custom Transport](../../code/advanced/CustomModulesSamples.cs#UseCustomTransport)]

### Other extension points on `Transport`

| Member | Purpose |
|---|---|
| `CreateIncomingMessage` | `protected virtual`. See the raw bytes of every inbound message before they are parsed |
| `CreateCommand` | `protected virtual`. Build the `Command` envelope — its id, method and parameters — for an outgoing command; override to stamp every command with an extra property |
| `SendCommandAsync` | `public virtual`. Send a command and get back the `Command` that was queued, without waiting for its response. `BiDiDriver.ExecuteCommandAsync` is the layer above it that waits and deserializes |
| `CancelCommand` | `public virtual`. Stop waiting for a command sent through `SendCommandAsync`, giving a `CommandCancellationReason`. Returns `false` when the command had already completed, in which case that outcome stands. The command is remembered so that a late response is recognized and discarded rather than reported as an unknown message (see [Error Handling — Transport Error Behavior Configuration](error-handling.md#transport-error-behavior-configuration)) |
| `RegisterEventMessage<T>` | `public virtual`. Teach the transport to deserialize an event name into `T`. `BiDiDriver.RegisterEvent<T>` calls it for you, so a module needs it only when the transport is driven directly; the envelope type it builds is AOT-safe, needing only `T` to be resolvable |
| `AddEventMessageType` | `protected virtual`. The lower half of `RegisterEventMessage<T>`: records the message type for an event name. Override to intercept or rewrite that registration |
| `SerializeCommand` | `protected virtual`. Turn a `Command` into the UTF-8 bytes put on the wire. Override to log or post-process the exact payload |
| `ProcessMessageAsync` | `protected virtual`. Handle one inbound message after it is read from the queue. Override to observe or delay individual messages |
| `ReadIncomingMessagesAsync` | `protected virtual`. The loop that drains the incoming message queue. Override only to replace the dispatch strategy wholesale |
| `CaptureUnhandledError` | `protected virtual`. The single point every non-command failure passes through, identified by its `UnhandledErrorKind`, before `TransportErrorBehavior` is applied. Override to observe failures without changing the behavior |
| `AcquireConnectionLockAsync` / `ReleaseConnectionLock` | `protected virtual`. Take and release the exclusive access that connecting, disconnecting, sending and registering a resolver each hold. Override to instrument contention |
| `PendingCommands` | `protected`, read-only. The current session's pending-command collection. Each session has its own, created when the transport connects, so the collection changes on reconnect. The size of the window of recent cancellations within which it remembers canceled commands is set by the public `MaxTrackedCanceledCommands` setting |
| `TimeProvider` | `protected` settable. The clock the transport's `ShutdownTimeout` waits and its commands' timeouts are measured on. Substitute one to drive those waits with virtual time in a test |
| `UnhandledErrors` | `protected`, read-only. The `UnhandledErrorCollection` the transport records failures in under its `TransportErrorBehavior` settings; see [Pending commands and unhandled errors](#pending-commands-and-unhandled-errors) |
| `LastCommandId` / `GetNextCommandId` | `protected`. The ID of the most recently created command, and the method that issues the next one. An override of `CreateCommand` that builds its own `Command` should take its ID from `GetNextCommandId`, or call the base implementation, so that IDs stay unique. IDs are unique for the lifetime of the transport and are not reset when it reconnects |

### Working with the `Command` you were handed

`SendCommandAsync` returns the `Command` as soon as it is sent. `BiDiDriver.ExecuteCommandAsync` is built from the
members below, and code that drives a transport directly uses the same ones:

| Member | Purpose |
|---|---|
| `WaitForCompletionAsync(timeout, cancellationToken)` | Returns `true` once the command has completed in any way (with a result, a fault or a cancellation) within the timeout, and `false` if the timeout elapses first. Throws `OperationCanceledException` if the token is canceled while the command is still pending. A timed-out command is still pending: cancel it with `Transport.CancelCommand` if you stop waiting |
| `TryGetResult(out CommandResult? result)` | Gets the result of a command that completed with one. An error response from the remote end is a result too: an `ErrorResult`, whose `IsError` is `true` |
| `ThrownException` | The exception a command faulted with inside the library, such as a response that could not be deserialized or a lost connection; `null` otherwise. It is not how the remote end's errors arrive |
| `IsCanceled` | Whether the command was canceled, by `Transport.CancelCommand` or by `DisconnectAsync` clearing the pending commands. A command still pending when the connection is lost faults instead, with a connection exception in `ThrownException` |
| `ElapsedMilliseconds` | The time since the transport sent the command, frozen when it completed |
| `SetResult`, `SetException`, `Cancel` | Complete the command. The transport calls them; a test double can too. Each does nothing once the command has completed, and `Cancel` returns whether it took effect |

`ExecuteCommandAsync` turns an `ErrorResult` into a `WebDriverBiDiCommandException`. To raise the same exception
from a custom command, or to complete a command in a test double with an error the remote end might send, build the
result with `ErrorResult.FromErrorInformation(errorType, errorMessage, stackTrace)`.

### Pending commands and unhandled errors

`PendingCommands` is a `PendingCommandCollection`. `AddPendingCommandAsync` adds a sent command and throws once the
collection is closed or when the ID is already present. `RemovePendingCommand` takes out a command whose response
arrived. `CancelPendingCommand` cancels one and remembers it, so that `TryRemoveCanceledCommand` can recognize its
late response. `CloseAsync` stops the collection accepting commands. `Clear` and `FailAllPendingCommands` both throw
`InvalidOperationException` unless `CloseAsync` has run first. `Clear` cancels the commands still pending and
remembers each one with `CommandCancellationReason.ConnectionClosed`, while `FailAllPendingCommands` faults each
with its own exception from the factory you pass. `TrackedCanceledCommandCount` reports how many canceled commands
are remembered; how long they are remembered is set by `Transport.MaxTrackedCanceledCommands` (reached through
`BiDiDriver.TransportConfiguration`), which defaults to `PendingCommandCollection.DefaultMaxTrackedCanceledCommands`
(1,024), as the class remarks describe.

`UnhandledErrors` is an `UnhandledErrorCollection`. It holds the four `TransportErrorBehavior` settings that the
transport's properties of the same names forward to. `AddUnhandledError(kind, exception)` records an
`UnhandledError`, with its `ErrorType` and `Exception`, unless the behavior for that kind is `Ignore`.
`HasUnhandledErrors(behavior)` and `TryGetExceptions(behavior, out exceptions)` ask about the errors recorded under
one behavior, `Exceptions` returns a snapshot of all of them, and `ClearUnhandledErrors` empties the collection. The
transport records its own failures through `CaptureUnhandledError`, which is the place to observe them.

### Filtering or rewriting inbound messages

`CreateIncomingMessage` hands you the raw bytes, which the `IncomingMessage` exposes as `MessageData` (with
`MessageLength`) or, decoded as UTF-8, as `MessageText`. The useful hook, though, is the `documentTransformer`
parameter of the `IncomingMessage` constructor: a transformer that receives the parsed `JsonDocument` and returns the
document to use instead, or `null` to discard the message entirely. `Parse()` runs it and sets `MessageKind`, which is
`IncomingMessageKind.Uninitialized` until then. A discarded message is marked
`IncomingMessageKind.Filtered` and is dropped silently — it is *not* reported as an unknown message.

[!code-csharp[Filtering Transport](../../code/advanced/CustomModulesSamples.cs#FilteringTransport)]

The message keeps ownership of the document it parsed, so don't hold on to the one your transformer
receives. It is disposed for you when you return `null`, return a different document, or throw. A
document you return is disposed with the message. An exception your transformer throws propagates out of
`IncomingMessage.Parse`. The transport handles a `JsonException` the same way as a message that is not
valid JSON, and captures any other exception as a `ProtocolError` unhandled error.

Overriding `CreateCommand` is the supported way to add a vendor extension property to every command; adding
it per call through `CommandParameters.AdditionalData` works too, but goes through reflection-based
serialization and so is flagged by BIDI022 for AOT and trimming.

## Next Steps

- [AOT Compatibility](aot-compatibility.md): Make custom modules work in AOT environments
- [Architecture](../architecture.md): Understand the module system
- [Core Concepts](../core-concepts.md): Learn about commands and events
- [Error Handling](error-handling.md): Implement robust error handling
- [Examples](../examples/common-scenarios.md): See modules in action

## Summary

Custom modules allow you to:
- Extend WebDriverBiDi.NET with reusable functionality
- Create domain-specific abstractions
- Implement experimental features
- Build shareable automation libraries

The module system is flexible and powerful, enabling you to build exactly the automation framework you need.

