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

[!code-csharp[Optional Parameters](../../code/api-design/TimeoutAndCancellationSamples.cs#OptionalParameters)]

**Commands with required parameters** always require a parameters object. These commands have a static "reset" property that clears a value on the remote end. Passing no parameters would be ambiguous—are you setting or resetting?

[!code-csharp[Required Parameters](../../code/api-design/TimeoutAndCancellationSamples.cs#RequiredParameters)]

### Complete Lists

| Optional Parameters | Required Parameters (Reset Property) |
|--------------------|--------------------------------------|
| `Browser.CloseAsync` | `UserAgentClientHints.SetClientHintsOverrideAsync` |
| `Browser.CreateUserContextAsync` | `Browser.SetDownloadBehaviorAsync` |
| `Browser.GetClientWindowsAsync` | `BrowsingContext.SetBypassCSPAsync` |
| `Browser.GetUserContextsAsync` | `BrowsingContext.SetViewportAsync` |
| `BrowsingContext.GetTreeAsync` | `Network.SetExtraHeadersAsync` |
| `Script.GetRealmsAsync` | All Emulation `Set*OverrideAsync` commands |
| | `Emulation.SetNetworkConditionsAsync` |
| | `Emulation.SetScriptingEnabledAsync` |
| `Session.EndAsync` | |
| `Session.NewSessionAsync` | |
| `Session.StatusAsync` | |
| `Storage.DeleteCookiesAsync` | |
| `Storage.GetCookiesAsync` | |

### Reset Property Variants

There are two distinct patterns for reset helpers on `CommandParameters` classes. Understanding the difference is important when reading XML documentation and when working with BIDI014.

#### Command-level reset

The static property returns a pre-configured instance of the `CommandParameters` class itself. You pass it directly to the command method. This is the most common pattern.

[!code-csharp[Command Level Reset](../../code/api-design/AdditionalDataSamples.cs#CommandLevelReset)]

BIDI014 detects when you use `new SomeCommandParameters()` without setting properties and the class has a command-level reset property, since that is almost certainly a mistake.

#### Property-level sentinel

The static property returns a typed *value* to assign to a specific property on the `CommandParameters` object. When the property is serialized, the sentinel value is written as JSON `null`, instructing the remote end to reset that individual field.

`SetViewportCommandParameters` uses this pattern because viewport dimensions and device pixel ratio can each be reset independently:

[!code-csharp[Property Level Sentinel](../../code/api-design/AdditionalDataSamples.cs#PropertyLevelSentinel)]

Assigning C# `null` to either property omits it from the JSON payload entirely, leaving the current value on the remote end unchanged. The sentinel is the only way to emit an explicit JSON `null` for these fields.

BIDI014 does **not** apply to property-level sentinel classes. Using `new SetViewportCommandParameters()` without any properties is valid — it sends a command that leaves both viewport and device pixel ratio at their current values.

### Optional List Properties

Optional lists follow the cardinality the protocol gives them. The rules apply to every type serialized into a command payload, not only to the `CommandParameters` roots: a list nested inside a sent object (`CapabilitiesRequest.FirstMatch`, `ScanRecord.UUIDs`, `ClientHintsMetadata.Brands`) follows the same rules as one declared on the parameters type itself.

- **Optional lists** (`Contexts`, `UserContexts`, `StartNodes`, `Arguments`, `UrlPatterns`, `PageRanges`, ...): the property is read-only and always initialized (`List<string> Contexts { get; }`). Populate it with a collection initializer or `.Add()`. While empty it is omitted from the JSON payload; an empty array is never sent. Where the CDDL is `[+x]` the browser would reject an empty array; where it is `[*x]` the specification treats an absent field and an empty array identically.
- **The exception** — `Headers` and `Cookies` on `ContinueRequest`, `ContinueResponse` and `ProvideResponse`, and `Brands`, `FullVersionList` and `FormFactors` on `ClientHintsMetadata`: the property is nullable and settable, because the protocol gives a present-but-empty array a distinct meaning. For the network lists, `[]` replaces the headers or cookies with none while omission keeps the originals; for the client hints, `[]` overrides the browser's own value with an empty one while omission leaves the browser's value in place. `null` omits the property; an empty list sends `[]`.
- **Required lists** (`Phases`, `Actions`, `Handles`, `Headers` on `SetExtraHeaders`, ...): read-only and always initialized, never settable.

Always check the XML documentation on the property for the rationale. See [Core Concepts - Command Parameters](../core-concepts.md#command-parameters) for details.

### Protocol Extensions via AdditionalData

The WebDriver BiDi protocol allows implementations to support additional command properties beyond the specification. The `AdditionalData` dictionary on `CommandParameters` lets you inject these extra fields into the JSON payload while keeping the strongly-typed API for standard parameters.

Use `AdditionalData` when:

- A browser or driver supports a pre-standard or vendor-specific parameter
- You need to pass extension data that the library does not yet model as a typed property
- You are integrating with a custom BiDi implementation that expects extra fields

Entries in `AdditionalData` are serialized as additional properties *inside the `params` object* of the command message, alongside the command's typed parameters. They do not appear at the envelope level next to `id`, `method`, and `params`. Values must be JSON-serializable (strings, numbers, booleans, null, arrays, or dictionaries).

For example, adding `parameters.AdditionalData["customOption"] = "customValue"` to a navigate command produces:

```json
{
  "id": 1,
  "method": "browsingContext.navigate",
  "params": {
    "context": "...",
    "url": "https://example.com",
    "customOption": "customValue"
  }
}
```

If you need extra properties at the envelope level (a sibling of `id`, `method`, and `params`), override `Transport.CreateCommand` in a custom transport and populate `Command.AdditionalCommandProperties` on the `Command` it returns. See [Custom Modules — Custom Transport](custom-modules.md#custom-transport) for how to supply a custom `Transport` to `BiDiDriver`.

An extension entry may not reuse a property name its object already writes. That holds for the envelope, where `id`, `method` and `params` are reserved, for the parameters root, and for every nested object with its own dictionary, such as `PartialCookie.AdditionalData` or `CapabilityRequest.AdditionalCapabilities`. It also holds for your own types sent through a registered resolver. Sending such a command throws `WebDriverBiDiSerializationException` naming the command and the entry, instead of emitting a JSON object with a duplicate property name. Set the typed property instead.

[!code-csharp[Protocol Extensions via AdditionalData](../../code/api-design/AdditionalDataSamples.cs#ProtocolExtensionsviaAdditionalData)]

### Reading vendor extension data

Received messages mirror the two outbound positions, and add a third for nested objects. Extension
properties are exposed where they were found and are never merged across positions:

| Position | Outbound | Command response | Event |
|----------|----------|------------------|-------|
| **Envelope** — beside `type`, `id`/`method`, `result`/`params` | `Command.AdditionalCommandProperties` | `CommandResult.AdditionalResponseProperties` | event args `AdditionalEventProperties` |
| **Payload root** — beside the specified members of `result`/`params` | `CommandParameters.AdditionalData` | `CommandResult.AdditionalData` | event args `AdditionalData` |
| **Nested object** | e.g. `CookieFilter.AdditionalData` | e.g. `Cookie.AdditionalData` | e.g. `RequestData.AdditionalData` |

The envelope and payload-root positions are captured by the transport for *every* command result and
event — built-in or custom, under reflection or native AOT — with no attribute on the type: any property
of the `result`/`params` object that the type does not define is extension data. (A member marked
`[JsonIgnore]` does not define a wire property, so a same-named property still counts as extension data.)
Chromium, for example, echoes a subscription's `goog:channel` on the envelope. Nested objects that the
protocol marks `Extensible` capture their own: `Cookie`, `CapabilitiesResult` (as `AdditionalCapabilities`), the
storage partition types, and `SharedReferenceInfo` (the `element` of `input.fileDialogOpened`, whose
`ToSharedReference()` carries the properties back to the remote end). `RequestData` and `ResponseData` capture
theirs too, although the specification does not mark them `Extensible`, because Chromium places members there in
practice: `goog:postData`, `goog:hasPostData`, `goog:resourceType`, `goog:resourceInitiator` and
`goog:securityDetails`.

Values are exposed as `ReceivedDataDictionary` entries: strings, `bool`, `long` or `double` numbers, nested
`ReceivedDataDictionary` objects and `ReceivedDataList` arrays, or `null`.

[!code-csharp[Reading Vendor Extension Data](../../code/api-design/AdditionalDataSamples.cs#ReadingVendorExtensionData)]

**Note:** The remote end must support the extension fields you send. Sending unknown properties may be ignored or cause an error depending on the implementation. Consult the protocol specification or your browser/driver documentation for supported extensions.

**AOT and trimming:** Because `AdditionalData` is typed as `Dictionary<string, object?>`, values stored in it are serialized through reflection-based `JsonSerializer` overloads rather than the source-generated context. This is not compatible with native AOT or IL trimming unless every value's runtime type is registered via [`BiDiDriver.RegisterTypeInfoResolverAsync`](../../api/WebDriverBiDi.BiDiDriver.yml) before the command is sent. The [BIDI022](analyzers.md#available-analyzers) analyzer flags every write to `AdditionalData` as a reminder. See [AOT Compatibility](aot-compatibility.md) for the pattern.

## Timeout and Cancellation

Every module command accepts two optional parameters. **This is the preferred way to set per-command timeouts** when using the module API (e.g., `driver.BrowsingContext.NavigateAsync`). Prefer this over `ExecuteCommandAsync` when you need per-command timeout control:

<!-- inline-csharp: a method signature sketch, not a callable method -->
```csharp
Task<T> CommandAsync(
    CommandParameters? parameters,
    TimeSpan? timeoutOverride = null,
    CancellationToken cancellationToken = default)
```

- **`timeoutOverride`**: When `null`, the driver uses `BiDiDriver.DefaultCommandTimeout` (60 seconds by default). Pass a value to override for long-running or quick-fail scenarios.
- **`cancellationToken`**: Propagates cancellation. Use for cooperative cancellation (e.g., user cancel, test timeout).

[!code-csharp[Timeout and Cancellation Examples](../../code/api-design/TimeoutAndCancellationSamples.cs#TimeoutandCancellationExamples)]

For timeout patterns (e.g., returning `null` instead of throwing) and connection-level timeout configuration, see [Error Handling - Timeout Handling](error-handling.md#timeout-handling).

## Error Handling Configuration

The library uses `TransportErrorBehavior` (Ignore, Collect, Terminate) to control how transport-level errors are handled. Four properties on `ITransportConfiguration`, reached through `BiDiDriver.TransportConfiguration`, provide fine-grained control:

| Property | Default | Controls |
|----------|---------|----------|
| `EventHandlerExceptionBehavior` | Ignore | Exceptions thrown by event handlers |
| `ProtocolErrorBehavior` | Ignore | An error response or registered event whose payload cannot be deserialized; an unexpected failure while processing a message |
| `UnknownMessageBehavior` | Ignore | A message that is not valid JSON, or not a command response, error response or registered event |
| `UnexpectedErrorBehavior` | Ignore | Error response with no corresponding command |

See [Error Handling](error-handling.md) for detailed guidance on when to use each mode.

## Versioning and Compatibility

### Package Versioning

WebDriverBiDi.NET uses [Semantic Versioning](https://semver.org/) (SemVer) version numbers, and is currently in the **0.x** series. SemVer makes no compatibility promise for major version zero, and this project does not make one either: **while the major version is 0, any release — including a patch increment — may change or remove public API.** Removals have already shipped in patch releases (for example, analyzer rule BIDI018 was removed in 0.0.48 and BIDI011/BIDI019 in 0.0.51). Pin an exact package version, and review what changed before updating. The project publishes no GitHub releases and keeps no changelog file, so the commit history and the pull request descriptions are the record.

Once the package reaches 1.0, the usual SemVer contract applies:

- **Major**: Breaking API changes
- **Minor**: New features, backward compatible
- **Patch**: Bug fixes, backward compatible

### Framework Support

The main library multi-targets `netstandard2.0`, `net8.0`, `net9.0`, and `net10.0`. The .NET Standard 2.0 target ensures compatibility with:

- .NET Framework 4.6.1+
- .NET Core 2.0+
- .NET 5, 6, 7, 8, 9, 10

Projects targeting .NET 8 or later bind to the corresponding assembly, which is the build marked `IsAotCompatible` and therefore the one that supports trimming and native AOT publishing. The published API reference is generated from the `netstandard2.0` assembly.

### Protocol Compatibility

The WebDriver BiDi protocol is evolving. The library defaults to `TransportErrorBehavior.Ignore` for protocol errors and unknown messages to support:

- **Forward compatibility**: Older library versions working with newer browsers that send new message types
- **Graceful degradation**: Automation continuing when protocol versions diverge slightly

When strict conformance is required (e.g., production with known protocol versions), consider `ProtocolErrorBehavior.Terminate` and `UnknownMessageBehavior.Terminate`.

### Breaking Changes

While the major version is 0, a breaking change may appear in any release, and there is no separate document announcing it: compare the commit history between the version you are on and the one you are moving to, and read the pull request descriptions. Look in particular for:

- Removed or renamed types and members
- Changed method signatures
- Changed default behavior
- Removed analyzer rules, which stop reporting rather than failing the build

## IObservable&lt;T&gt; Integration

Any `ObservableEvent<T>` can be adapted to the standard BCL `IObservable<T>` interface via the `ToObservable()` extension method. This enables integration with [Reactive Extensions (Rx)](https://github.com/dotnet/reactive) operators and any code that consumes `IObservable<T>`/`IObserver<T>`.

<!-- inline-csharp: uses a placeholder observer type that the reader supplies -->
```csharp
IDisposable subscription = driver.Network.OnBeforeRequestSent
    .ToObservable()
    .Subscribe(new MyObserver());
```

The adapter only partially satisfies the full Rx push-stream contract. Be aware of these differences:

- **`OnCompleted`** is called only after the subscription handle returned by `Subscribe` is disposed and the internal buffer drains. It is **not** called when the `BiDiDriver` is stopped or disposed — dispose the subscription handle explicitly to trigger completion.
- **`OnError`** is called if `OnNext` throws. It is **not** called for transport errors or exceptions thrown by other observers on the same event.
- Each `Subscribe` call creates an independent buffered subscription that counts as one observer against `ObservableEvent<T>.MaxObserverCount`. Dispose the returned handle when done to avoid resource leaks. The handle is an `ObservableEventSubscription<T>`, whose `CompletionTask` completes once delivery to the observer has ended.

For full details, code samples, and Rx operator usage, see [Events and Observables — IObservable&lt;T&gt; Support](../events-observables.md#iobservablet-support).

## Related Documentation

- [Core Concepts](../core-concepts.md): Command parameters, events, lifecycle
- [Error Handling](error-handling.md): TransportErrorBehavior, exception handling, timeout patterns, troubleshooting
- [Architecture](../architecture.md): Transport, connection, error configuration
- [Events and Observables](../events-observables.md): Observer pattern, data collectors, IObservable&lt;T&gt; integration
- [Quick Reference](../quick-reference.md): Common commands at a glance
