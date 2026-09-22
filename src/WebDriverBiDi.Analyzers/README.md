# WebDriverBiDi.Analyzers

Roslyn analyzers for [WebDriverBiDi.NET](https://github.com/webdriverbidi-net/webdriverbidi-net) that catch
common misuse of the library at compile time, rather than as a hang, a deadlock or a protocol error at run
time.

## Installation

```shell
dotnet add package WebDriverBiDi.Analyzers
```

The package is a development dependency: it contributes no assembly to your output and flows to consumers of
your own package only if you ask it to.

## What it checks

Thirty-six rules, in four groups:

- **Lifecycle (Error).** Registering modules, events or type-info resolvers after `StartAsync()`; executing a
  command before `StartAsync()` or after `StopAsync()`; using a driver after `DisposeAsync()`; calling
  `StartAsync()` twice; a fire-and-forget module command; reading captured tasks that were never captured; a
  mismatched `ExecuteCommandAsync<T>` result type; registering a custom event under a built-in event name;
  asking `ToEventArgs<T>()` for a type other than the event's own; an extension-data entry named for a property
  its object already sends; using an observer's task-capture members, or reading a data collector's events,
  after it has been disposed.
- **Event handling (Warning).** Adding an observer for an event that was never subscribed; leaking an
  `EventObserver`; blocking calls and deadlock-prone synchronization inside a handler; issuing a module
  command from a handler that runs on the dispatching thread; an `async void` handler; opening a capture
  session that is never read; starting a capture session while one is already active; observing
  `Connection.OnDataReceived` on a connection that a `Transport` owns.
- **Correctness (Warning).** Unsafe casts of `EvaluateResult`; adding to a nullable list without
  initializing it; mutating `AdditionalData`, which is not AOT-safe; a value outside the range the
  specification defines for a property; registering a library envelope type in a serializer context; a
  connection string that is not a WebSocket URL for a driver with the default transport; a received type's
  `[JsonPropertyName]` property that the serializer cannot set.
- **Style and cost (Info/Warning).** Omitting a `CancellationToken`, on any command that accepts one and on
  the long-running operations in particular; using a parameterless constructor where a `Reset*` property
  exists; a string literal where `ObservableEvent.EventName` would do; disposing a driver without stopping it
  first; discarding the observer that `AddObserver` returns.

Most rules ship with a code fix.

## Documentation

The [analyzer reference](https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html)
documents every rule individually: what it reports, what its code fix does, and — under **Known
Limitations** — the analysis scope each rule works within, which explains why a given rule may not fire in a
particular situation.

## Configuring severity

Change a rule's severity, or turn it off, from an `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.BIDI009.severity = warning
dotnet_diagnostic.BIDI004.severity = none
```

Single occurrences can be suppressed with `#pragma warning disable BIDI0nn` or a `[SuppressMessage]`
attribute.

## License

MIT. See the [LICENSE](https://github.com/webdriverbidi-net/webdriverbidi-net/blob/main/LICENSE) file.
