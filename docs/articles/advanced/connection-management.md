# Connection Management

This guide provides an in-depth look at connection management in WebDriverBiDi.NET, covering connection lifecycle, diagnostics, error recovery, and advanced scenarios.

## Overview

WebDriverBiDi.NET uses an abstract `Connection` class as the foundation for browser communication. This abstraction allows the library to support multiple transport mechanisms while providing a consistent API.

**Important**: Most users will never need to create or manage connections directly. The `BiDiDriver` class handles connection management automatically. This document is for advanced users who need to understand the underlying architecture or implement custom transports.

## Architecture Layers

The library uses a three-layer architecture for browser communication:

1. **Connection** (Transport Layer): Manages the raw communication channel (WebSocket or Pipes)
2. **Transport** (Protocol Layer): Handles JSON serialization, command/response correlation, and event dispatching
3. **BiDiDriver** (API Layer): Provides the high-level WebDriver BiDi API

```
BiDiDriver (← Most users interact here)
    ↓
Transport (← Rarely customized)
    ↓
Connection (← Very rarely customized)
```

## Typical Usage (Recommended)

### Simple WebSocket Connection

Most users should use `BiDiDriver` directly without worrying about connections:

[!code-csharp[Simple WebSocket Connection](../../code/advanced/ConnectionManagementSamples.cs#SimpleWebSocketConnection)]

This is sufficient for 95% of use cases. The driver creates a WebSocket connection internally.

### Using a Browser Launcher (Best for Local Automation)

For local automation, use a browser launcher to manage the process and connection. **The `WebDriverBiDi` package does not ship a launcher.** The samples in this article use `BrowserLauncher` from the repository's `WebDriverBiDi.Client` demonstration library, which is not published to NuGet; in your own project you implement the equivalent (see [Browser Setup — Implementing Your Own Launcher](../browser-setup.md#implementing-your-own-launcher)). The pattern:

[!code-csharp[Using a Browser Launcher](../../code/advanced/ConnectionManagementSamples.cs#UsingaBrowserLauncher)]

## When You Might Need Connection Management

You only need to manage connections directly in these rare scenarios:

1. **Custom Transport Implementation**: Implementing a new transport mechanism (e.g., HTTP/2, gRPC)
2. **Connection Monitoring**: Deep diagnostics and monitoring of connection-level events
3. **Custom Timeout Configuration**: Fine-tuning connection-specific timeouts beyond driver defaults
4. **Connection Reuse**: Advanced connection pooling or sharing scenarios

**If none of these apply to you, use the simple patterns above and skip the rest of this document.**

## Connection Architecture (Advanced)

### The Connection Abstraction

For the rare cases where you need it, the `Connection` base class defines the transport contract. See the `Connection` class in the WebDriverBiDi.Protocol namespace for the full API.

### Available Implementations

**WebSocketConnection** (Primary)
- Uses `System.Net.WebSockets.ClientWebSocket`
- Requires WebSocket URL (`ws://` or `wss://`)
- Supported by all browsers with WebDriver BiDi
- Created automatically by `BiDiDriver` unless you specify a custom transport

**PipeConnection** (Specialized)
- Uses `System.IO.Pipes.AnonymousPipeServerStream`
- Requires browser process with `--remote-debugging-pipe` flag
- Currently limited to Chromium-based browsers
- Requires your own implementation of `IPipeServerProcessProvider` to manage the browser process

## Advanced Usage: Custom Connection Configuration

Only use this pattern if you need custom connection timeout configuration:

[!code-csharp[Custom Connection Configuration](../../code/advanced/ConnectionManagementSamples.cs#CustomConnectionConfiguration)]

**Note**: Most users should configure the driver timeout instead:

[!code-csharp[Driver timeout](../../code/advanced/ConnectionManagementSamples.cs#Drivertimeout)]

**Command timeouts vs. connection timeouts:** The settings above (StartupTimeout, DataTimeout, etc.) control connection-level behavior. For command execution timeouts (e.g., how long to wait for a navigation or script to complete), use the `timeoutOverride` parameter on module methods or `BiDiDriver`'s default command timeout. See [Error Handling - Timeout Handling](error-handling.md#timeout-handling).

## Connection Configuration

### Connection Timeout Settings

Connections have three timeout properties (default: 10 seconds each):

[!code-csharp[Timeout Settings](../../code/advanced/ConnectionManagementSamples.cs#TimeoutSettings)]

**StartupTimeout**: Connection establishment timeout. WebSocket connections retry every 500ms until the timeout elapses, and each individual connect attempt is bounded by the time remaining in the budget, so a host that never completes the handshake cannot hold `StartAsync` open past the timeout. When the budget is exhausted, `StartAsync` throws `WebDriverBiDiTimeoutException`.

**ShutdownTimeout**: Bounds shutting the connection down. It bounds the transport's own close (e.g., the WebSocket close handshake), and it separately bounds the wait for the connection's receive loop to finish. That wait happens at both ends of a session: `StopAsync` waits for the loop it is ending, and `StartAsync` waits for a loop a previous `StopAsync` had to abandon, since a second loop must not run alongside it. A receive-loop wait that is not satisfied does not throw — it raises a `Warn` message on `OnLogMessage` and proceeds, leaving the loop running in the background; `StartAsync` then refuses to begin a new session while it is, throwing `WebDriverBiDiConnectionException`. Note that this is distinct from `Transport.ShutdownTimeout`, described below.

**DataTimeout**: How long a send waits for exclusive access to the connection while another send is in progress. It does not bound the send or the receive itself, so it is not a guard against a hung connection; a send that waits longer than this fails to acquire access and throws a `WebDriverBiDiTimeoutException` instead of queueing behind the send ahead of it. A zero value keeps its non-blocking meaning: access is taken only if it is free right now.

### Transport Shutdown Timeout

`ShutdownTimeout`, reached through `BiDiDriver.TransportConfiguration` (or on a `Transport` directly), is a separate, transport-level timeout (default: 10 seconds) that controls how long `Transport.DisconnectAsync` waits for its in-memory message-processing task to drain before proceeding. If the processing task does not finish within this window, `DisconnectAsync` logs a warning and proceeds, and any pending commands are canceled. Giving up on the wait does not stop the reader: messages already delivered to the queue go on being processed in the background, and their handlers go on running. What the timeout bounds is how long shutdown waits for them, not whether they run — and a subsequent `StartAsync` waits for that processing to finish, bounded by this same timeout, before opening a new connection.

Most users never need to tune this. Consider adjusting it only in specialized scenarios:

- **Reduce** (e.g., to 1–2 seconds) for tests that want fail-fast behavior when a misbehaving handler would otherwise hold shutdown open.
- **Increase** when you expect legitimately long-running event handlers and want them to be given more time to finish during shutdown.

[!code-csharp[Transport Shutdown Timeout](../../code/advanced/ConnectionManagementSamples.cs#TransportShutdownTimeout)]

`Transport.ShutdownTimeout` governs the transport's own waits: the message-processing drain during `DisconnectAsync`, the wait for the previous connection's processing to finish when `StartAsync` reconnects, and, during disposal, the wait for an in-flight connect attempt to complete before resources are released (that last wait goes through the connection lock, so `ConnectionLockTimeout`, described below, bounds it as well; whichever elapses first ends it). It does not affect the underlying connection's close handshake, which is governed by `Connection.ShutdownTimeout`. Both timeouts may apply during `driver.StopAsync()`, at different stages of teardown.

An event handler that is still running while `StopAsync()` drains the queue may itself try to send a command. Such a command fails immediately with `WebDriverBiDiConnectionException` ("Transport must be connected to a remote end to execute commands") rather than waiting out the shutdown timeout, so a handler cannot deadlock shutdown by sending; it simply observes the exception.

### Transport Connection Lock Timeout

`ConnectionLockTimeout`, reached through `BiDiDriver.TransportConfiguration` (default: 60 seconds), bounds how long an operation waits for exclusive access to the connection while another operation holds it. `ConnectAsync`, `DisconnectAsync`, `SendCommandAsync` and `RegisterTypeInfoResolverAsync` each take that access for the duration of their work, so one of them waits while another is in progress. Ordinary concurrent use never notices the bound: commands issued from several threads at once contend only for as long as a send takes.

The bound exists for one situation, which is otherwise unrecoverable. The transport dispatches observers while it holds the access, so an observer that calls back into the driver asks for access its own caller holds, and the two would wait on each other indefinitely. This is not confined to `Trace`: the connection raises its `SEND >>>` traffic message from inside the send, but the connection and the transport also log around connect and disconnect at the default `Info` level, so an observer that reconnects when it sees "Transport disconnected" reaches the same point.

With the bound in place, the re-entrant operation fails with `WebDriverBiDiTimeoutException` naming the cause, and the operation it interrupted goes on to complete. This is a diagnosable failure rather than a hang, not a licence to re-enter: register any observer that drives the driver with `ObservableEventHandlerOptions.RunHandlerAsynchronously`, so that it no longer runs inside the operation that dispatched it.

[!code-csharp[Transport Connection Lock Timeout](../../code/advanced/ConnectionManagementSamples.cs#TransportConnectionLockTimeout)]

Where the failure surfaces depends on which event the observer was added to. Through `driver.OnLogMessage` or `transport.OnLogMessage` the transport catches the observer's exception and routes it through [`EventHandlerExceptionBehavior`](error-handling.md#transport-error-behavior-configuration), so the interrupted operation still completes. Added directly to `connection.OnLogMessage`, the exception propagates into the send, which then fails as well — as any throwing observer of that event does.

The default is deliberately longer than the longest legitimate hold, so that lowering it is a deliberate choice. A disconnect that exhausts every wait it is allowed holds the access for the connection's close handshake and its receive-loop wait (each bounded by `Connection.ShutdownTimeout`) and then for the message-queue drain (bounded by `Transport.ShutdownTimeout`), roughly 30 seconds at the default settings. Reduce it when you would rather find a re-entrant observer quickly than wait a minute for it; set it to `Timeout.InfiniteTimeSpan` to restore an unbounded wait, and `TimeSpan.Zero` to never wait at all.

Handling a lost connection waits for the access as well, and it is the one waiter with no caller to report a failure to, because it runs on the connection's receive loop. If that wait is abandoned — a lowered bound elapsing while a command send still holds the access — the loss is not applied to the session: the transport stays connected, its in-flight commands end at their own command timeouts rather than failing at once, and a `Warn` message naming the cause is raised on `OnLogMessage`. Call `StopAsync()` to tear the session down. Applying the loss without the access is the alternative, and a worse one: each step of that teardown is thread-safe on its own, but performing them outside the access could interleave with a reconnect and close the pending-command collection of a session the loss has nothing to do with.

### Buffer Size

Connection buffer size is fixed at 1 MB (2²⁰ bytes):

[!code-csharp[Buffer Size](../../code/advanced/ConnectionManagementSamples.cs#BufferSize)]

This is suitable for typical WebDriver BiDi messages. Larger payloads (screenshots, DOM snapshots) arrive as multiple WebSocket frames of a single message; the connection reassembles the frames and hands the complete message to the transport.

## Connection Diagnostics

Connections provide observable events for diagnostics. This is useful for monitoring and debugging.

### Log Level

`OnLogMessage` is filtered by a minimum level that defaults to `WebDriverBiDiLogLevel.Info`. The same
setting is exposed at all three layers — `BiDiDriver.TransportConfiguration.LogLevel`,
`Transport.LogLevel` and `Connection.LogLevel` — and they read and write one value, so setting it
anywhere sets it everywhere:

[!code-csharp[Set Log Level](../../code/advanced/ConnectionManagementSamples.cs#SetLogLevel)]

| Level | What it adds |
|-------|--------------|
| `Warn` and above | Shutdown timeouts, disposal problems, connection errors |
| `Info` (default) | Connection and transport lifecycle |
| `Debug` | A message per command sent, answered, or discarded as a late response |
| `Trace` | Every message exchanged with the remote end (`SEND >>>` / `RECV <<<`) |
| `Off` | Nothing at all, including `Fatal` |

`Debug` and `Trace` are excluded by default because they are the voluminous ones, and because a `Trace`
traffic message is built by decoding the whole payload into a string. While `Trace` is disabled that
decode never happens, so the cost is paid only once you ask for it. If you need to know whether a level
is enabled before composing an expensive message of your own — in a custom `Connection` or `Transport`,
for instance — call `IsLogLevelEnabled`, which accounts for both the level and whether anything is
observing.

### Inspecting Protocol Traffic

To see the raw messages exchanged with the browser, set `LogLevel` to `Trace` and observe
`OnLogMessage`. Connections emit every message they send and receive at that level, prefixed
with `SEND >>>` or `RECV <<<`. The level must be raised explicitly: it defaults to `Info`, and a
traffic message is not composed at all — the payload is not even decoded from UTF-8 — while `Trace`
is disabled, so an observer alone will not show any traffic:

[!code-csharp[Protocol Traffic Logging](../../code/advanced/ConnectionManagementSamples.cs#ProtocolTrafficLogging)]

**Use cases:** Protocol debugging, traffic analysis, performance monitoring.

> [!IMPORTANT]
> Do not observe `OnDataReceived` for this purpose. That event hands over ownership of a
> pooled buffer rather than broadcasting a copy of the data, so it admits **one, and only one,
> observer** — the `Transport`, which claims it when constructed. A second observer would read
> a buffer the first one may already have released. The event enforces this: adding a second
> observer throws, and constructing a `Transport` over a connection that already has one throws
> `ArgumentException`.

> [!NOTE]
> A received message is logged only when something is observing `OnDataReceived` to consume it.
> A `Connection` driven on its own, with no `Transport` wrapped around it, has no such observer:
> it discards each received message and returns the message's buffer to the pool without logging
> it, so only `SEND >>>` entries appear. Wrapping the connection in a `Transport` — the normal
> arrangement, and what `BiDiDriver` does for you — restores the `RECV <<<` entries.

### OnConnectionError Event

Monitors connection errors:

[!code-csharp[OnConnectionError Event](../../code/advanced/ConnectionManagementSamples.cs#OnConnectionErrorEvent)]

**Error scenarios:** Disconnections, network failures, protocol violations, timeouts.

### OnLogMessage Event

Internal connection logging:

[!code-csharp[OnLogMessage Event](../../code/advanced/ConnectionManagementSamples.cs#OnLogMessageEvent)]

**Levels:** `Info` (normal operations), `Warn` (non-critical issues), `Error` (connection errors); `Debug` and `Trace` carry per-message detail and are excluded by the default `LogLevel` of `Info`.

## Transport Diagnostics

In addition to the `Connection`-level observable events above, the transport exposes read-only diagnostic properties you can sample at any time. Reach them through `BiDiDriver.TransportDiagnostics`, which is an `ITransportDiagnostics` and needs no hand-built transport, so a driver created with `new BiDiDriver()` can be observed like any other. These are intended for operators and frameworks that want to understand lifecycle, backlog, and in-flight state without subscribing to an `EventSource`. All are safe to read concurrently with command send and response processing; none of them throws at any point of the lifecycle; and the returned values are snapshots that may be stale by the time the caller observes them.

### State

`TransportDiagnostics.State` reports where the transport is in its connection lifecycle as a `TransportState` value: `Disconnected` (the initial state, the state after a completed disconnect, and the state a failed connection attempt rolls back to), `Connecting` (a connection attempt is in flight but not yet complete), or `Connected` (the transport can exchange messages with the remote end). `BiDiDriver.IsStarted` derives from it (it is `true` exactly when the state is `Connected`), and so does registration legality: `RegisterModule` and `RegisterEvent` are rejected once the transport has left `Disconnected`, and become legal again when a stop returns it there.

[!code-csharp[Transport State Diagnostic](../../code/advanced/ConnectionManagementSamples.cs#TransportStateDiagnostic)]

### IncomingQueueDepth

`TransportDiagnostics.IncomingQueueDepth` reports the number of raw messages received from the connection that are waiting to be processed by the transport's reader task. A persistently growing value indicates that event handlers are not keeping up with the incoming message rate; consider using `ObservableEventHandlerOptions.RunHandlerAsynchronously` for I/O-heavy handlers so that they do not block the reader.

Each call to `ConnectAsync` installs a fresh queue whose depth begins at zero, and every message is counted against the queue it was written to. The value therefore reports only the current connection's backlog, even when a reconnect gave up waiting for the previous connection's reader and that reader is still draining what remains of its own queue. Reading it before `ConnectAsync` has ever been called, or after `DisconnectAsync`, returns the depth of the remaining (possibly drained) queue rather than throwing.

[!code-csharp[Transport IncomingQueueDepth Diagnostic](../../code/advanced/ConnectionManagementSamples.cs#TransportIncomingQueueDepthDiagnostic)]

### PendingCommandCount

`TransportDiagnostics.PendingCommandCount` reports the number of commands that have been sent to the remote end and are still awaiting a response. A persistently high value suggests that the remote end is not responding promptly, or that a burst of commands is in flight without corresponding responses yet.

The pending-command collection is cleared during `DisconnectAsync`, so reads after a disconnect typically return zero. Like `IncomingQueueDepth`, this property may be safely read before `ConnectAsync` is called and after `DisconnectAsync`; it returns the current count rather than throwing.

[!code-csharp[Transport PendingCommandCount Diagnostic](../../code/advanced/ConnectionManagementSamples.cs#TransportPendingCommandCountDiagnostic)]

These properties pair well with the EventSource-based diagnostics described in [Observability](observability.md): the properties let you poll current state, while the `EventSource` stream gives you lifecycle events.

## WebSocket Connection Details

WebSocket connections are the standard transport mechanism.

### URL Requirements

Valid WebSocket URLs use `ws://` or `wss://` schemes:

[!code-csharp[WebSocket URL Requirements](../../code/advanced/ConnectionManagementSamples.cs#WebSocketURLRequirements)]

### Automatic Retry

WebSocket connections retry during startup if the browser isn't ready:

[!code-csharp[Automatic Retry](../../code/advanced/ConnectionManagementSamples.cs#AutomaticRetry)]

This handles cases where the browser is still launching.

### Connection State

Check if a connection is active:

[!code-csharp[Connection State](../../code/advanced/ConnectionManagementSamples.cs#ConnectionState)]

## Pipe Connection Details

Pipe connections are an advanced feature for specialized scenarios.

### When to Consider Pipes

Use pipes only when:
- Running extensive local test suites
- Absolute minimum latency is critical
- Using Chromium-based browsers exclusively

**For most users**: Use WebSocket connections. They're simpler, more flexible, and universally supported.

### Requirements

- Chromium-based browser (Chrome, Edge)
- Browser launched with `--remote-debugging-pipe` flag
- Process lifecycle management

### Using Pipes with a Custom Launcher

The sample below uses the demonstration `BrowserLauncher` (see above); to build your own, implement `IPipeServerProcessProvider` to launch the browser with `--remote-debugging-pipe` and provide the `Transport`:

[!code-csharp[Using Pipes with Launcher](../../code/advanced/ConnectionManagementSamples.cs#UsingPipeswithLauncher)]

### Protocol Details

Pipes use null-terminated JSON messages:
- Browser reads from file descriptor 3 (Unix) or pipe handle (Windows)
- Browser writes to file descriptor 4 (Unix) or pipe handle (Windows)
- Each message ends with `\0`

Because that terminator is the only frame boundary, a message and its terminator are written as one
operation. Canceling a command therefore either sends the whole message or sends none of it; cancellation
is honored up to the moment the first byte is written, and not after. A canceled send can never leave a
partial message in the pipe, which would otherwise put every message after it one frame out of step for
the rest of the session.

### Limitations

**Browser Support:** Only Chromium-based browsers support pipes.

**Deployment:** Browser and tests must be on the same machine. No remote debugging.

## Reconnection and Recovery

### Reconnecting After StopAsync

A `BiDiDriver` is reusable: after `StopAsync()` completes, `IsStarted` is `false` and `StartAsync()` may be called again, with the same or a different connection string. `WebSocketConnection` supports this for any URL. A `PipeConnection` can also be restarted, but only against the browser process it was created for (its pipes are inherited by that process) and only until the connection is disposed; connecting to a different browser process requires a new `PipeConnection` and `Transport`. Between the stop and the restart the driver is in the same state as before its first start, so modules, custom events and type-info resolvers may be registered again. Observers added to `ObservableEvent<T>` properties are kept across the restart, but event subscriptions live in the browser session and must be re-established with `Session.SubscribeAsync` after reconnecting.

[!code-csharp[Reconnect After Stop](../../code/advanced/ConnectionManagementSamples.cs#ReconnectAfterStop)]

### Recovering From a Remote Disconnect

When the browser closes the connection (or a read fails), the connection raises `OnRemoteDisconnected` (or `OnConnectionError`) and the transport immediately marks itself disconnected: every in-flight command fails with `WebDriverBiDiConnectionException`, later commands throw the same exception without waiting, and `driver.IsStarted` becomes `false`. The library does not reconnect automatically.

To recover, call `StopAsync()` — it returns promptly because the transport is already disconnected, and throws the `AggregateException` of any errors accumulated under `TransportErrorBehavior.Collect` — and then call `StartAsync()` again. Registration of modules, custom events and type-info resolvers is already permitted from the moment the transport publishes `Disconnected`, so nothing has to be cleared first. `StartAsync` waits (bounded by `Transport.ShutdownTimeout`) for the previous connection's message processing to finish before opening the new connection, so a handler that was still running when the connection dropped cannot interleave with the new session. At `Debug` log level the transport logs when it begins that wait, and at `Warn` level when the wait times out.

The `StopAsync()` call is not optional if you use `Collect` mode. Because the driver is already stopped after a remote disconnect, `StartAsync()` accepts the call directly, but starting a new session clears the previous session's collected errors without throwing them: only `StopAsync()` throws collected errors, exactly as with [`DisposeAsync()`](error-handling.md#collect-mode). Reconnecting without stopping first silently discards everything collected up to the disconnect.

[!code-csharp[Recover From Remote Disconnect](../../code/advanced/ConnectionManagementSamples.cs#RecoverFromRemoteDisconnect)]

### Losing the connection while connecting

A connection can also be lost during `StartAsync`, before the session is ever established. The connection's receive loop is already running by the time it reports that the connection is open, so a remote end that accepts the connection and then immediately closes it — a browser shutting down, an endpoint that rejects the session after the handshake, a Chromium process that exits just after inheriting the pipes — is reported while the transport is still connecting.

`StartAsync` fails in that case with `WebDriverBiDiConnectionException`, carrying whatever the connection reported as its inner exception. The transport is left disconnected, so the attempt can simply be retried; nothing needs to be stopped first. This is deliberately a failure rather than a success followed by a disconnect, because a session that never started is not one a caller can use, and reporting it as started would leave `IsStarted` describing a connection that is already gone.


## Error Handling

### Common Connection Errors

[!code-csharp[Connection Error Handling](../../code/advanced/ConnectionManagementSamples.cs#ConnectionErrorHandling)]

### Monitoring Connection Health

Monitor connection health with diagnostics:

[!code-csharp[Monitoring Connection Health](../../code/advanced/ConnectionManagementSamples.cs#MonitoringConnectionHealth)]

## Very Advanced: Custom Connection Implementations

**Warning**: This section is for extremely specialized scenarios. 99.9% of users will never need this.

You can create custom connection implementations for experimental transports.

`StartAsync`, `StopAsync` and `DisposeAsync` are implemented by `Connection` itself and cannot be
overridden: the sequence each performs is the same for every transport, and several of its steps are
required in a particular order, so the base class owns all of it. A custom connection supplies only
the transport-specific parts, as `protected` overrides:

| Member | What it supplies |
| --- | --- |
| `ResolveConnectionString` | Interprets the connection string, rejecting a value this transport could never connect to. Optional; the default accepts every value. |
| `StartConnectionAsync` | Establishes the connection. Returns only once it is established, throws if it cannot be. |
| `StopConnectionAsync` | Whatever this transport must exchange with the remote end to close by agreement. Called on every stop, including when the connection is not active. |
| `SendConnectionDataAsync` | Writes one message to the transport. |
| `ReceiveDataAsync` | The receive loop, started for you once the connection is established. |
| `DisposeAsyncCore` | Releases the resources the custom connection owns. |

A custom connection also inherits members it does not have to supply, but will generally use:

| Member | Purpose |
| --- | --- |
| `NotifyDataReceivedObserverAsync` | `protected`. Call from the receive loop to deliver a completed message, transferring ownership of its pooled memory. Does nothing when the accumulator is empty, so it is safe to call at every point a message may have completed. |
| `ConnectionCancellationToken` | `protected`, read-only. The token cancelled when the connection stops. Use it rather than the cancellation source, which is disposed while the receive loop may still be running. |
| `DataReceiveTask` | `protected`, read-only. The task the receive loop runs on, for a shutdown that needs to observe it. |
| `IsLogLevelEnabled` | `public`. Test before composing any log message that is not free to build; the `SEND` and `RECV` traffic messages decode the whole payload, so they are guarded by it. |
| `TimeProvider` | `protected`, settable. The clock `StartupTimeout`, `ShutdownTimeout` and `DataTimeout` are measured on. Substitute one to drive them with virtual time in a test. |

The public `MessageBuffer` class is the pooled accumulator for a message that arrives in more than one
read. `Append` adds a piece, `TakeOwnership` hands the completed block to the consumer and leaves the
buffer ready for the next message, and `Discard` returns a partial message to the pool. Both shipped
connections own one for the life of their receive loop.

`WebSocketConnection` and `PipeConnection` each expose a further seam of their own for the operation
they perform on the wire — `ConnectWebSocketAsync`, `WriteWebSocketDataAsync`, `ReceiveWebSocketDataAsync`,
`CloseClientWebSocketAsync` and `DelayBeforeRetryAsync` on the first; `WritePipeDataAsync`,
`WriteToPipeAsync` and `ReadPipeDataAsync` on the second — so a derived connection can substitute the
single call rather than reimplement the loop around it.

`ResolveConnectionString` is the only place the connection string is interpreted — the base class
carries the value but never reads it, because what counts as a usable connection string is exactly
what a transport knows and nothing above it does. It rejects a value it could never connect to by
throwing `ArgumentException`, and that exception type is the distinction a caller acts on: an
`ArgumentException` out of `StartAsync` means the connection string must be corrected, where any other
failure means the attempt did not succeed.

Validating a connection string and deriving what a connect needs from it are usually the same act —
parsing a URL both proves it is one and produces the value to connect with — so `ResolveConnectionString`
does both, and a transport that must parse keeps the result for `StartConnectionAsync` instead of
parsing again there. That is safe to rely on because `StartAsync` is not overridable: it calls
`ResolveConnectionString` on every path that reaches `StartConnectionAsync`, and nothing in between can
invalidate what was resolved. The reverse does not hold — a start can still be refused between the two,
for instance because a previous session's receive loop is still running — so what is kept is good for
the next connect rather than a promise that one follows. It is also why `StartConnectionAsync` takes no
connection string: by the time it runs, the string has already been interpreted, and a transport that
wants the raw value reads `ConnectionString`.

The split exists for a second reason: `ResolveConnectionString` runs before `StartAsync` does anything
that can take time. Folded into `StartConnectionAsync`, a malformed connection string would be reported
only after the bounded wait for a previous session's receive loop, which on a reconnect can be as long
as `ShutdownTimeout` — a wait to be told about a fault that costs nothing to detect.

[!code-csharp[Custom Connection Implementation](../../code/advanced/ConnectionManagementSamples.cs#CustomConnectionImplementation)]

Usage: create the custom connection, wrap in transport, and pass to BiDiDriver:

[!code-csharp[Custom Connection Usage](../../code/advanced/ConnectionManagementSamples.cs#CustomConnectionUsage)]

**Use cases for custom connections:**
- Research into new transport protocols
- Integration with non-standard browser implementations
- Proxy or tunnel scenarios

**If you're not sure you need this, you don't need it.**

## Platform-Specific Considerations

### Windows

**WebSocket:** Full support, no special considerations. Firewall may prompt.

**Pipes:** Uses anonymous pipes whose handles the browser process inherits. Process must have matching user privileges.

### macOS / Linux

**WebSocket:** Full support, no special considerations.

**Pipes:** Uses file descriptors 3 and 4. Process must have file permissions.

### Docker / Containers

**WebSocket:** Recommended. Expose port and connect via container IP.

**Pipes:** Not recommended. Requires shared process namespace and complex orchestration.

[!code-csharp[Docker WebSocket Connection](../../code/advanced/ConnectionManagementSamples.cs#DockerWebSocketConnection)]

## Best Practices

### 1. Use the Simplest Pattern That Works

[!code-csharp[Best Practice Simplest](../../code/advanced/ConnectionManagementSamples.cs#BestPracticeSimplest)]

### 2. Use a Browser Launcher for Local Automation

Use a launcher to manage the browser process and connection (the sample uses the demonstration `BrowserLauncher`; your own would start a driver executable such as chromedriver, create a session with the `webSocketUrl: true` capability, then connect to the returned URL — see [Browser Setup](../browser-setup.md)):

[!code-csharp[Best Practice Browser Launcher](../../code/advanced/ConnectionManagementSamples.cs#BestPracticeBrowserLauncher)]

### 3. Always Clean Up

[!code-csharp[Best Practice Cleanup](../../code/advanced/ConnectionManagementSamples.cs#BestPracticeCleanup)]

### 4. Prefer WebSocket Unless You Have a Specific Reason

WebSocket connections are:
- Universally supported
- Simpler to set up
- More flexible
- Better for debugging

### 5. Configure Timeouts at the Driver Level

[!code-csharp[Best Practice Timeouts](../../code/advanced/ConnectionManagementSamples.cs#BestPracticeTimeouts)]

## Summary

- **Most users**: Use `new BiDiDriver()` and call `StartAsync()`. That's it.
- **Local automation**: Implement a launcher to manage the browser process and connection
- **Advanced users only**: Manage connections directly for custom timeouts or diagnostics
- **Very advanced users only**: Implement custom connections for experimental transports
- WebSocket connections are standard and recommended for all scenarios
- Pipe connections are specialized for high-performance local Chromium automation
- Always clean up connections properly

## See Also

- [Getting Started](../getting-started.md): Simple usage patterns
- [Architecture](../architecture.md): Connection abstraction design
- [Browser Setup](../browser-setup.md): Launching browsers
- [Error Handling](error-handling.md): Handling connection errors
- [Performance](performance.md): Connection performance characteristics
