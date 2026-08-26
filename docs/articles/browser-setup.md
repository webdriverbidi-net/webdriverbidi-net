# Browser Setup Guide

This guide explains how to set up different browsers for use with WebDriverBiDi.NET.

## Overview

WebDriverBiDi.NET requires a browser with WebDriver BiDi support running with remote debugging enabled. The library supports two connection types:

- **WebSocket Connection** (default): Connects to browsers via `--remote-debugging-port`
- **Pipe Connection**: Connects to browsers via `--remote-debugging-pipe`

This guide covers both approaches, with WebSocket being the recommended starting point for most users.

## Chrome / Chromium

### Windows

```cmd
# Basic launch
chrome.exe --remote-debugging-port=9222

# With custom profile
chrome.exe --remote-debugging-port=9222 --user-data-dir=C:\temp\chrome-profile

# Headless mode
chrome.exe --remote-debugging-port=9222 --headless=new
```

### macOS

```bash
# Basic launch
/Applications/Google\ Chrome.app/Contents/MacOS/Google\ Chrome \
  --remote-debugging-port=9222

# With custom profile
/Applications/Google\ Chrome.app/Contents/MacOS/Google\ Chrome \
  --remote-debugging-port=9222 \
  --user-data-dir=/tmp/chrome-profile

# Headless mode
/Applications/Google\ Chrome.app/Contents/MacOS/Google\ Chrome \
  --remote-debugging-port=9222 \
  --headless=new
```

### Linux

```bash
# Basic launch
google-chrome --remote-debugging-port=9222

# With custom profile
google-chrome --remote-debugging-port=9222 --user-data-dir=/tmp/chrome-profile

# Headless mode
google-chrome --remote-debugging-port=9222 --headless=new
```

### Getting the WebSocket URL

After launching Chrome:

1. Open a browser tab
2. Navigate to `http://localhost:9222/json/version`
3. Copy the `webSocketDebuggerUrl` value

Example response:
```json
{
  "Browser": "Chrome/121.0.6167.85",
  "Protocol-Version": "1.3",
  "User-Agent": "Mozilla/5.0...",
  "V8-Version": "12.1.285.27",
  "WebKit-Version": "537.36",
  "webSocketDebuggerUrl": "ws://localhost:9222/devtools/browser/abc-123-def"
}
```

Use the `webSocketDebuggerUrl` value to connect:

[!code-csharp[Connect with WebSocket URL](../code/examples/BrowserSetupSamples.cs#ConnectwithWebSocketURL)]

## Microsoft Edge

Microsoft Edge is based on Chromium and uses the same commands.

### Windows

```cmd
# Basic launch
msedge.exe --remote-debugging-port=9222

# With custom profile
msedge.exe --remote-debugging-port=9222 --user-data-dir=C:\temp\edge-profile

# Headless mode
msedge.exe --remote-debugging-port=9222 --headless=new
```

### macOS

```bash
# Basic launch
/Applications/Microsoft\ Edge.app/Contents/MacOS/Microsoft\ Edge \
  --remote-debugging-port=9222

# With custom profile
/Applications/Microsoft\ Edge.app/Contents/MacOS/Microsoft\ Edge \
  --remote-debugging-port=9222 \
  --user-data-dir=/tmp/edge-profile
```

### Linux

```bash
# Basic launch
microsoft-edge --remote-debugging-port=9222

# With custom profile
microsoft-edge --remote-debugging-port=9222 --user-data-dir=/tmp/edge-profile
```

WebSocket URL discovery is the same as Chrome - visit `http://localhost:9222/json/version`.

## Connection Types

WebDriverBiDi.NET supports two transport mechanisms for communicating with browsers:

### WebSocket Connection (Default)

**How it Works:**
- Browser launches with `--remote-debugging-port=PORT`
- Browser listens on a TCP port for WebSocket connections
- Your application connects via `ws://localhost:PORT/devtools/browser/ID`
- Multiple clients can connect to the same browser

**Best For:**
- Development and debugging
- Remote browser control
- Flexible deployment scenarios
- When you need to connect from outside your process

**Example:**

[!code-csharp[WebSocket Connection](../code/examples/BrowserSetupSamples.cs#WebSocketConnection)]

### Pipe Connection

**How it Works:**
- Browser launches with flags to enable pipe communication
- Browser communicates via anonymous pipes (stdin/stdout)
- Protocol uses null-terminated JSON messages
- On Unix-like systems: File descriptors 3 (browser reads) and 4 (browser writes)
- Single client can connect (the process that launched the browser)

**Best For:**
- Automation frameworks
- Programmatic browser control
- Lower latency requirements
- When you control the browser lifecycle

**Platform Support:**
- Windows: Anonymous pipes
- macOS/Linux: File descriptor-based pipes

**Example:**

> **Note:** The `WebDriverBiDi` NuGet package does not include a browser launcher. The repository's `WebDriverBiDi.Client` demonstration library (not published to NuGet) provides a `BrowserLauncher` whose Chromium launcher implements `IPipeServerProcessProvider`, and the example below uses it. To do this yourself, implement `IPipeServerProcessProvider`: launch the browser with `--remote-debugging-pipe` so that it inherits the two anonymous pipe handles `PipeConnection` creates (file descriptors 3 and 4 on Unix), and pass a `Transport` built over that `PipeConnection` to `BiDiDriver`:

[!code-csharp[Pipe Launcher Pattern](../code/examples/BrowserSetupSamples.cs#PipeLauncherPattern)]

The skeleton of your own `IPipeServerProcessProvider` implementation looks like this; `PipeServerProcess` returns the launched browser `Process`, and `CreateTransport` wraps a `PipeConnection` over `this`:

[!code-csharp[Implementing IPipeServerProcessProvider](../code/examples/BrowserSetupSamples.cs#ImplementingIPipeServerProcessProvider)]

### Comparison

| Feature | WebSocket | Pipes |
|---------|-----------|-------|
| **Latency** | Moderate (TCP overhead) | Lower (direct IPC) |
| **Remote Access** | ✓ Yes | ✗ No |
| **Multi-Client** | ✓ Yes | ✗ No |
| **Setup Complexity** | Simple | Moderate |
| **Debugging** | Easy (inspect traffic) | Moderate |
| **Use Case** | Development, debugging | Automation, testing |

**Recommendation:** Start with WebSocket connections for simplicity, switch to Pipes if you need lower latency or are building automation frameworks.

## Firefox

Firefox support for WebDriver BiDi is still evolving. The setup is different from Chromium-based browsers.

### Using GeckoDriver

1. Download geckodriver from https://github.com/mozilla/geckodriver/releases
2. Launch geckodriver:

```bash
geckodriver --port 4444
```

3. Firefox will launch automatically when you connect

### WebSocket URL

Firefox uses a different URL format:
```
ws://localhost:4444/session
```

Connect with:

[!code-csharp[Firefox Connection](../code/examples/BrowserSetupSamples.cs#FirefoxConnection)]

> **Important:** When connecting via geckodriver, you must call `session.NewSessionAsync` explicitly
> after `StartAsync`. Geckodriver does not create a WebDriver BiDi session automatically on connect,
> unlike direct CDP connections to Chrome or Edge. Without this call, subsequent commands will fail
> because no session exists on the remote end:
>
> ```csharp
> await driver.StartAsync("ws://localhost:4444/session");
> NewCommandParameters sessionParams = new();
> NewCommandResult sessionResult = await driver.Session.NewSessionAsync(sessionParams);
> ```
>
> See the [Session Module guide](modules/session.md) for full details and capability negotiation options.

### Note on Firefox Support

Firefox's WebDriver BiDi implementation is actively being developed. Some features may not be available or may behave differently than in Chromium-based browsers.

## Using with Selenium Manager

If you're using Selenium, you can let Selenium Manager handle browser launching:

[!code-csharp[Selenium Manager Integration](../code/examples/BrowserSetupSamples.cs#SeleniumManagerIntegration)]

This is conceptual—WebDriverBiDi.NET doesn't include Selenium. Let Selenium's `ChromeDriver` launch the browser, ask the browser for its `webSocketDebuggerUrl` through Selenium's CDP bridge (`ExecuteCdpCommand("Target.getTargets", …)`, as the sample does; fetching `http://localhost:<port>/json/version` works too if you launched with `--remote-debugging-port`), then connect a `BiDiDriver` to that URL.

## Docker Container

You can run Chrome in a Docker container with remote debugging:

```dockerfile
FROM selenium/standalone-chrome:latest

# Expose remote debugging port
EXPOSE 9222

# Launch with remote debugging
CMD google-chrome \
  --remote-debugging-port=9222 \
  --remote-debugging-address=0.0.0.0 \
  --disable-gpu \
  --no-sandbox
```

```bash
docker run -p 9222:9222 my-chrome-debug
```

Connect to `ws://localhost:9222/devtools/browser/...`

## Common Launch Options

### Disable GPU

Useful for headless environments:
```
--disable-gpu
```

### Window Size

Set initial window size:
```
--window-size=1920,1080
```

### Disable Extensions

Start without extensions:
```
--disable-extensions
```

### Incognito Mode

Start in incognito/private mode:
```
--incognito
```

### No Sandbox (Docker/CI)

Disable sandboxing (needed in some containerized environments):
```
--no-sandbox
```

### Disable Dev Shm (Docker)

Prevent shared memory issues in Docker:
```
--disable-dev-shm-usage
```

### Example CI Launch

```bash
google-chrome \
  --remote-debugging-port=9222 \
  --headless=new \
  --disable-gpu \
  --no-sandbox \
  --disable-dev-shm-usage \
  --window-size=1920,1080
```

## Programmatic Browser Launch

You can launch the browser process yourself, discover its WebSocket URL, and only then connect. The snippet below shows just the connection step; the [WebSocket Launcher Pattern](#websocket-launcher-pattern) further down shows the launch and discovery steps:

[!code-csharp[Programmatic Browser Launch](../code/examples/BrowserSetupSamples.cs#ProgrammaticBrowserLaunch)]

## Implementing Your Own Launcher

The `WebDriverBiDi` NuGet package does **not** ship a browser launcher; the library only provides the protocol client. The repository's `WebDriverBiDi.Client` demonstration library shows one way to do it (see `BrowserLauncher` in `src/WebDriverBiDi.Client/Launchers`), but it is not published, so to automate browser launch in your own project you implement the launcher yourself. The patterns below sketch the two approaches.

### WebSocket Launcher Pattern

Launch the browser with `--remote-debugging-port`, then discover the WebSocket URL and connect. The snippet fetches `/json/version` but leaves parsing its `webSocketDebuggerUrl` property to you (any JSON library will do); in the example the parsed value is represented by the `webSocketUrl` parameter:

[!code-csharp[WebSocket Launcher Pattern](../code/examples/BrowserSetupSamples.cs#WebSocketLauncherPattern)]

### Pipe Launcher Pattern

For pipe connections, implement `IPipeServerProcessProvider` to launch the browser with pipe flags and provide a `Transport` to `BiDiDriver`. See the `Transport` and `PipeConnection` types in the API reference for the interface contract.

## Troubleshooting

### Port Already in Use

```
Error: Port 9222 already in use
```

**Solutions:**
- Close existing Chrome instances
- Use a different port: `--remote-debugging-port=9223`
- Find and kill the process using the port

### Connection Refused

```
WebDriverBiDiException: Connection refused
```

**Solutions:**
- Verify browser is running
- Check the WebSocket URL is correct
- Ensure no firewall is blocking the port
- Try `http://localhost:9222` in a browser to verify

### Browser Closes Immediately

**Solutions:**
- Use `--user-data-dir` to specify a profile
- Check for conflicting flags
- Run without `--headless` to debug

### Invalid WebSocket URL

**Solutions:**
- Don't use the URL from the initial tab (it's for that specific page)
- Always get the browser-level WebSocket URL from `/json/version`
- The URL should contain `/devtools/browser/`, not `/devtools/page/`

## Best Practices

1. **Use a dedicated profile**: `--user-data-dir` prevents conflicts
2. **Fixed port**: Always use the same port for consistency
3. **Launch before connect**: Ensure browser is fully started
4. **Clean shutdown**: Close connections before killing browser
5. **Headless for CI**: Use `--headless=new` in CI environments
6. **Log output**: Redirect stdout/stderr when launching programmatically

## Security Considerations

⚠️ **Warning**: Remote debugging exposes full browser control. Do not:
- Run with remote debugging on production systems
- Expose the debugging port to the internet
- Use with sensitive data without proper isolation

For production use:
- Run in isolated containers
- Use firewalls to restrict access
- Generate unique ports per session
- Clean up profiles after use

## Next Steps

- [Getting Started](getting-started.md): Create your first application
- [Your First Application](first-application.md): Complete tutorial
- [Core Concepts](core-concepts.md): Understand the library
- [Architecture](architecture.md): Deep dive into connection types
- [Connection Management](advanced/connection-management.md): Advanced connection scenarios
