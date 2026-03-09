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

```csharp
using WebDriverBiDi;

// BiDiDriver handles connection automatically
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.StartAsync("ws://localhost:9222/devtools/browser/abc-123");

// Use driver...

await driver.StopAsync();
```

This is sufficient for 95% of use cases. The driver creates a WebSocket connection internally.

### Using a Browser Launcher (Best for Local Automation)

For local automation, use a browser launcher to manage everything:

```csharp
using WebDriverBiDi;
using WebDriverBiDi.Client.Launchers;

// Launcher manages browser process and connection
ChromeLauncher launcher = new ChromeLauncher("/path/to/chrome");

await launcher.StartAsync();
await launcher.LaunchBrowserAsync();

// Create driver with launcher's preconfigured transport
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), launcher.CreateTransport());
await driver.StartAsync(launcher.WebSocketUrl);

try
{
    // Use driver...
}
finally
{
    await driver.StopAsync();
    await launcher.QuitBrowserAsync();
    await launcher.StopAsync();
}
```

## When You Might Need Connection Management

You only need to manage connections directly in these rare scenarios:

1. **Custom Transport Implementation**: Implementing a new transport mechanism (e.g., HTTP/2, gRPC)
2. **Connection Monitoring**: Deep diagnostics and monitoring of connection-level events
3. **Custom Timeout Configuration**: Fine-tuning connection-specific timeouts beyond driver defaults
4. **Connection Reuse**: Advanced connection pooling or sharing scenarios

**If none of these apply to you, use the simple patterns above and skip the rest of this document.**

## Connection Architecture (Advanced)

### The Connection Abstraction

For the rare cases where you need it, the `Connection` base class defines the transport contract:

```csharp
public abstract class Connection : IAsyncDisposable
{
    public abstract bool IsActive { get; }
    public abstract ConnectionType ConnectionType { get; }
    public abstract Task StartAsync(string connectionString, CancellationToken cancellationToken = default);
    public abstract Task StopAsync(CancellationToken cancellationToken = default);
    public abstract Task SendDataAsync(byte[] data, CancellationToken cancellationToken = default);
}
```

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
- Managed by `ChromeLauncher` when using pipe connections

## Advanced Usage: Custom Connection Configuration

Only use this pattern if you need custom connection timeout configuration:

```csharp
using WebDriverBiDi;
using WebDriverBiDi.Protocol;

// Create and configure connection (rare need)
WebSocketConnection connection = new WebSocketConnection()
{
    StartupTimeout = TimeSpan.FromSeconds(30),  // Very slow environment
    DataTimeout = TimeSpan.FromSeconds(15)       // Slow network
};

// Wrap in transport
Transport transport = new Transport(connection);

// Create driver with custom transport
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);
await driver.StartAsync("ws://localhost:9222/devtools/browser/abc-123");

try
{
    // Use driver...
}
finally
{
    await driver.StopAsync();
}
```

**Note**: Most users should configure the driver timeout instead:

```csharp
// Simpler and sufficient for most cases
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(60)); // Long timeout
```

## Connection Configuration

### Timeout Settings

Connections have three timeout properties (default: 10 seconds each):

```csharp
WebSocketConnection connection = new WebSocketConnection()
{
    // How long to wait for initial connection
    StartupTimeout = TimeSpan.FromSeconds(15),

    // How long to wait for graceful shutdown
    ShutdownTimeout = TimeSpan.FromSeconds(10),

    // How long to wait for send/receive operations
    DataTimeout = TimeSpan.FromSeconds(10)
};
```

**StartupTimeout**: Connection establishment timeout. WebSocket connections retry every 500ms until timeout.

**ShutdownTimeout**: Graceful shutdown timeout. Ensures resources are released properly.

**DataTimeout**: Send/receive operation timeout. Protects against hung connections.

### Buffer Size

Connection buffer size is fixed at 1 MB (2²⁰ bytes):

```csharp
int bufferSize = connection.BufferSize; // 1048576 bytes (read-only)
```

This is suitable for typical WebDriver BiDi messages. Large transfers (screenshots, DOM snapshots) are split across multiple messages.

## Connection Diagnostics

Connections provide observable events for diagnostics. This is useful for monitoring and debugging.

### OnDataReceived Event

Monitors raw data received from the browser:

```csharp
connection.OnDataReceived.AddObserver((ConnectionDataReceivedEventArgs e) =>
{
    Console.WriteLine($"Received: {e.Data.Length} bytes");
    // e.Data contains the raw byte array
});
```

**Use cases:** Protocol debugging, traffic analysis, performance monitoring.

### OnConnectionError Event

Monitors connection errors:

```csharp
connection.OnConnectionError.AddObserver((ConnectionErrorEventArgs e) =>
{
    Console.WriteLine($"Connection error: {e.Exception.Message}");
    Logger.Error($"Exception: {e.Exception}");
});
```

**Error scenarios:** Disconnections, network failures, protocol violations, timeouts.

### OnLogMessage Event

Internal connection logging:

```csharp
connection.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
{
    Console.WriteLine($"[{e.Level}] {e.Source}: {e.Message}");
});
```

**Levels:** Info (normal operations), Warning (non-critical issues), Error (connection errors).

## WebSocket Connection Details

WebSocket connections are the standard transport mechanism.

### URL Requirements

Valid WebSocket URLs use `ws://` or `wss://` schemes:

```csharp
// ✅ Valid
await driver.StartAsync("ws://localhost:9222/devtools/browser/abc-123");
await driver.StartAsync("wss://remote-host:9222/devtools/browser/abc-123");

// ❌ Invalid
await driver.StartAsync("http://localhost:9222");  // Wrong scheme
await driver.StartAsync("localhost:9222");         // Not absolute
```

### Automatic Retry

WebSocket connections retry during startup if the browser isn't ready:

```csharp
// Will retry every 500ms for up to 30 seconds
WebSocketConnection connection = new WebSocketConnection()
{
    StartupTimeout = TimeSpan.FromSeconds(30)
};
```

This handles cases where the browser is still launching.

### Connection State

Check if a connection is active:

```csharp
if (connection.IsActive)
{
    // Connection is open and ready
}
```

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

### Using Pipes with ChromeLauncher

Let the launcher handle pipe setup:

```csharp
using WebDriverBiDi.Client.Launchers;

ChromeLauncher launcher = new ChromeLauncher()
{
    ConnectionType = ConnectionType.Pipes  // Enable pipes
};

await launcher.StartAsync();
await launcher.LaunchBrowserAsync();

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), launcher.CreateTransport());
await driver.StartAsync("pipes");

try
{
    // Use driver...
}
finally
{
    await driver.StopAsync();
    await launcher.QuitBrowserAsync();
    await launcher.StopAsync();
}
```

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

```csharp
try
{
    await driver.StartAsync(url);
}
catch (WebDriverBiDiTimeoutException ex)
{
    Console.WriteLine($"Connection timeout: {ex.Message}");
    // Browser may not be ready or URL incorrect
}
catch (WebDriverBiDiConnectionException ex)
{
    Console.WriteLine($"Connection failed: {ex.Message}");
    // Connection already in use or invalid state
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid URL: {ex.Message}");
    // URL format is incorrect
}
```

### Monitoring Connection Health

Monitor connection health with diagnostics:

```csharp
bool connectionHealthy = true;

connection.OnConnectionError.AddObserver((e) =>
{
    connectionHealthy = false;
    Logger.Error($"Connection error: {e.Exception.Message}");
});

// Check before critical operations
if (!connectionHealthy || !connection.IsActive)
{
    // Handle error or reconnect
}
```

## Very Advanced: Custom Connection Implementations

**Warning**: This section is for extremely specialized scenarios. 99.9% of users will never need this.

You can create custom connection implementations for experimental transports:

```csharp
public class CustomConnection : Connection
{
    public override bool IsActive => /* your logic */;

    public override ConnectionType ConnectionType => ConnectionType.WebSocket;

    public override async Task StartAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await LogAsync("Custom connection starting");
        // Your startup logic
    }

    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await LogAsync("Custom connection stopping");
        // Your shutdown logic
    }

    public override async Task SendDataAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        // Your send logic
    }

    protected override async Task ReceiveDataAsync()
    {
        // Your receive logic
        // Call OnDataReceived.NotifyObserversAsync() with received data
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        // Your cleanup logic
    }
}

// Usage
CustomConnection customConnection = new CustomConnection();
Transport transport = new Transport(customConnection);
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);
await driver.StartAsync(customConnectionString);
```

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

```csharp
// WebSocket to containerized browser
string url = $"ws://172.17.0.2:9222/devtools/browser/abc-123";
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.StartAsync(url);
```

## Best Practices

### 1. Use the Simplest Pattern That Works

```csharp
// ✅ Best: Let BiDiDriver handle everything
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.StartAsync(url);

// ❌ Avoid unless you have a specific need
WebSocketConnection connection = new WebSocketConnection();
Transport transport = new Transport(connection);
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);
```

### 2. Use Browser Launchers for Local Automation

```csharp
// ✅ Recommended for local testing
ChromeLauncher launcher = new ChromeLauncher();
await launcher.StartAsync();
await launcher.LaunchBrowserAsync();
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), launcher.CreateTransport());
```

### 3. Always Clean Up

```csharp
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
try
{
    await driver.StartAsync(url);
    // Use driver...
}
finally
{
    if (driver.IsStarted)
    {
        await driver.StopAsync();
    }
}
```

### 4. Prefer WebSocket Unless You Have a Specific Reason

WebSocket connections are:
- Universally supported
- Simpler to set up
- More flexible
- Better for debugging

### 5. Configure Timeouts at the Driver Level

```csharp
// ✅ Preferred: Simple and sufficient
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(60));

// ❌ Avoid unless needed: More complex
WebSocketConnection connection = new WebSocketConnection()
{
    StartupTimeout = TimeSpan.FromSeconds(60),
    DataTimeout = TimeSpan.FromSeconds(60)
};
```

## Summary

- **Most users**: Use `new BiDiDriver()` and call `StartAsync()`. That's it.
- **Local automation**: Use browser launchers (`ChromeLauncher`, etc.)
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
