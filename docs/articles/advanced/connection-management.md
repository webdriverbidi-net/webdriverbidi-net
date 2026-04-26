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

For local automation, use a browser launcher to manage the process and connection. **WebDriverBiDi.NET does not ship a launcher**—you must implement one. The pattern:

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

**StartupTimeout**: Connection establishment timeout. WebSocket connections retry every 500ms until timeout.

**ShutdownTimeout**: Graceful shutdown timeout for the underlying connection (e.g., the WebSocket close handshake). Ensures resources are released properly. Note that this is distinct from `Transport.ShutdownTimeout`, described below.

**DataTimeout**: Send/receive operation timeout. Protects against hung connections.

### Transport Shutdown Timeout

`Transport.ShutdownTimeout` is a separate, transport-level timeout (default: 10 seconds) that controls how long `Transport.DisconnectAsync` waits for its in-memory message-processing task to drain before proceeding. If the processing task does not finish within this window, `DisconnectAsync` logs a warning and proceeds; messages still in the queue will not be processed, and any pending commands are canceled.

Most users never need to tune this. Consider adjusting it only in specialized scenarios:

- **Reduce** (e.g., to 1–2 seconds) for tests that want fail-fast behavior when a misbehaving handler would otherwise hold shutdown open.
- **Increase** when you expect legitimately long-running event handlers and want them to be given more time to finish during shutdown.

[!code-csharp[Transport Shutdown Timeout](../../code/advanced/ConnectionManagementSamples.cs#TransportShutdownTimeout)]

`Transport.ShutdownTimeout` only affects the message-processing drain; it does not affect the underlying connection's close handshake, which is governed by `Connection.ShutdownTimeout`. Both timeouts may apply during `driver.StopAsync()`, at different stages of teardown.

### Buffer Size

Connection buffer size is fixed at 1 MB (2²⁰ bytes):

[!code-csharp[Buffer Size](../../code/advanced/ConnectionManagementSamples.cs#BufferSize)]

This is suitable for typical WebDriver BiDi messages. Large transfers (screenshots, DOM snapshots) are split across multiple messages.

## Connection Diagnostics

Connections provide observable events for diagnostics. This is useful for monitoring and debugging.

### OnDataReceived Event

Monitors raw data received from the browser:

[!code-csharp[OnDataReceived Event](../../code/advanced/ConnectionManagementSamples.cs#OnDataReceivedEvent)]

**Use cases:** Protocol debugging, traffic analysis, performance monitoring.

### OnConnectionError Event

Monitors connection errors:

[!code-csharp[OnConnectionError Event](../../code/advanced/ConnectionManagementSamples.cs#OnConnectionErrorEvent)]

**Error scenarios:** Disconnections, network failures, protocol violations, timeouts.

### OnLogMessage Event

Internal connection logging:

[!code-csharp[OnLogMessage Event](../../code/advanced/ConnectionManagementSamples.cs#OnLogMessageEvent)]

**Levels:** Info (normal operations), Warning (non-critical issues), Error (connection errors).

## Transport Diagnostics

In addition to the `Connection`-level observable events above, the `Transport` itself exposes two read-only diagnostic properties you can sample at any time. These are intended for operators and frameworks that want to understand backlog and in-flight state without subscribing to an `EventSource`. Both are safe to read concurrently with command send and response processing; the returned values are snapshots and may be stale by the time the caller observes them.

### IncomingQueueDepth

`Transport.IncomingQueueDepth` reports the number of raw messages received from the connection that are waiting to be processed by the transport's reader task. A persistently growing value indicates that event handlers are not keeping up with the incoming message rate; consider using `ObservableEventHandlerOptions.RunHandlerAsynchronously` for I/O-heavy handlers so that they do not block the reader.

The counter is reset to zero on each call to `ConnectAsync` alongside the channel replacement. Reading it before `ConnectAsync` has ever been called, or after `DisconnectAsync`, returns the depth of the remaining (possibly drained) queue rather than throwing.

[!code-csharp[Transport IncomingQueueDepth Diagnostic](../../code/advanced/ConnectionManagementSamples.cs#TransportIncomingQueueDepthDiagnostic)]

### PendingCommandCount

`Transport.PendingCommandCount` reports the number of commands that have been sent to the remote end and are still awaiting a response. A persistently high value suggests that the remote end is not responding promptly, or that a burst of commands is in flight without corresponding responses yet.

The pending-command collection is cleared during `DisconnectAsync`, so reads after a disconnect typically return zero. Like `IncomingQueueDepth`, this property may be safely read before `ConnectAsync` is called and after `DisconnectAsync`; it returns the current count rather than throwing.

[!code-csharp[Transport PendingCommandCount Diagnostic](../../code/advanced/ConnectionManagementSamples.cs#TransportPendingCommandCountDiagnostic)]

Both properties pair well with the EventSource-based diagnostics described in [Observability](observability.md): the properties let you poll current state, while the `EventSource` stream gives you lifecycle events.

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

WebDriverBiDi.NET does not ship a launcher. Implement `IPipeServerProcessProvider` to launch the browser with `--remote-debugging-pipe` and provide the `Transport`:

[!code-csharp[Using Pipes with Launcher](../../code/advanced/ConnectionManagementSamples.cs#UsingPipeswithLauncher)]

### Protocol Details

Pipes use null-terminated JSON messages:
- Browser reads from file descriptor 3 (Unix) or pipe handle (Windows)
- Browser writes to file descriptor 4 (Unix) or pipe handle (Windows)
- Each message ends with `\0`

### Limitations

**Browser Support:** Only Chromium-based browsers support pipes.

**Deployment:** Browser and tests must be on the same machine. No remote debugging.

## Error Handling

### Common Connection Errors

[!code-csharp[Connection Error Handling](../../code/advanced/ConnectionManagementSamples.cs#ConnectionErrorHandling)]

### Monitoring Connection Health

Monitor connection health with diagnostics:

[!code-csharp[Monitoring Connection Health](../../code/advanced/ConnectionManagementSamples.cs#MonitoringConnectionHealth)]

## Very Advanced: Custom Connection Implementations

**Warning**: This section is for extremely specialized scenarios. 99.9% of users will never need this.

You can create custom connection implementations for experimental transports:

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

**Pipes:** Uses named pipes. Process must have matching user privileges.

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

Implement your own launcher to manage the browser process and connection. Launch with `--remote-debugging-port`, discover the WebSocket URL from `/json/version`, then connect:

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
