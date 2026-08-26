# Browser Setup Guide

This guide explains how to obtain a WebDriver BiDi endpoint for each browser and connect WebDriverBiDi.NET to it.

## Overview

`BiDiDriver.StartAsync` needs a WebSocket URL that **speaks WebDriver BiDi**. Which URL that is depends on the browser:

| Browser | Speaks BiDi natively? | How to get a BiDi endpoint |
|---------|-----------------------|----------------------------|
| Chrome / Chromium / Edge | **No.** `--remote-debugging-port` and `--remote-debugging-pipe` expose the Chrome DevTools Protocol (CDP) only | Create a session through **chromedriver** / **msedgedriver** with the `webSocketUrl` capability (recommended), or inject a BiDi-over-CDP mapper (advanced) |
| Firefox | **Yes.** `--remote-debugging-port` exposes BiDi at `/session` | Connect directly, or go through **geckodriver** |

> **Important:** the `ws://localhost:9222/devtools/browser/<id>` URL that Chrome prints at startup and reports from `http://localhost:9222/json/version` is a **CDP** endpoint. A `BiDiDriver` connected to it will open the socket successfully and then fail on its first command, because the browser does not understand WebDriver BiDi messages on that endpoint. Do not use it.

The library supports two connection types, both usable with any of the above:

- **WebSocket Connection** (default): connects to a WebSocket URL
- **Pipe Connection**: connects over anonymous pipes to a browser you launched with `--remote-debugging-pipe` (Chromium only; see [Connection Types](#connection-types))

## Chrome / Chromium

### Through chromedriver (recommended)

chromedriver hosts the BiDi implementation for Chrome: a classic WebDriver session created with the `webSocketUrl: true` capability comes back with a `webSocketUrl` that speaks WebDriver BiDi.

1. Get a chromedriver that matches your Chrome version from [Chrome for Testing](https://googlechromelabs.github.io/chrome-for-testing/).
2. Start it:

   ```bash
   chromedriver --port=9515
   ```

3. Create a session, requesting `webSocketUrl`. Browser flags (`--headless=new`, `--user-data-dir=…`, `--no-sandbox`, …) go in `goog:chromeOptions.args`; chromedriver launches the browser for you:

   ```bash
   curl -X POST http://localhost:9515/session \
     -H "Content-Type: application/json" \
     -d '{"capabilities":{"alwaysMatch":{"webSocketUrl":true,"goog:chromeOptions":{"args":["--headless=new"]}}}}'
   ```

   The response contains the endpoint:

   ```json
   {
     "value": {
       "sessionId": "8a4d1c2e-0b7f-4c9a-9d3e-5f6a7b8c9d0e",
       "capabilities": {
         "browserName": "chrome",
         "browserVersion": "131.0.6778.85",
         "webSocketUrl": "ws://localhost:9515/session/8a4d1c2e-0b7f-4c9a-9d3e-5f6a7b8c9d0e"
       }
     }
   }
   ```

   Doing the same from C#:

   [!code-csharp[Create Session Through Driver](../code/examples/BrowserSetupSamples.cs#CreateSessionThroughDriver)]

4. Connect to the `webSocketUrl`:

   [!code-csharp[Connect with WebSocket URL](../code/examples/BrowserSetupSamples.cs#ConnectwithWebSocketURL)]

The session already exists, so **do not call `Session.NewSessionAsync`** on this connection. To finish, call `driver.Session.EndAsync()` (which also closes the browser) or `DELETE http://localhost:9515/session/<sessionId>`, then stop chromedriver.

### Through a BiDi-over-CDP mapper (advanced)

The [chromium-bidi](https://github.com/GoogleChromeLabs/chromium-bidi) project provides a JavaScript "mapper" that implements WebDriver BiDi on top of CDP. Injecting it into a hidden tab lets a client talk BiDi over the browser's own CDP endpoint (WebSocket or pipe) with no driver executable. The repository's `WebDriverBiDi.Client` demonstration library does exactly this in its `ChromiumTransport` (a `Transport` subclass that bootstraps the mapper during `ConnectAsync`); that library is not published to NuGet, but it is the reference for building your own. This is the only route that works over a pipe connection, and it requires `Session.NewSessionAsync` after connecting because the mapper does not create a session.

## Microsoft Edge

Edge is Chromium-based and follows the chromedriver path exactly, using **msedgedriver** (from the [Edge WebDriver page](https://developer.microsoft.com/microsoft-edge/tools/webdriver/)) and `ms:edgeOptions` in place of `goog:chromeOptions`:

```bash
msedgedriver --port=9515
```

## Connection Types

WebDriverBiDi.NET supports two transport mechanisms for communicating with browsers:

### WebSocket Connection (Default)

**How it Works:**
- Your application connects to a WebDriver BiDi WebSocket URL: the `webSocketUrl` of a session created through chromedriver/msedgedriver/geckodriver, or Firefox's `ws://localhost:PORT/session`
- Works with local and remote endpoints
- Multiple clients can connect to the same driver or browser (each gets its own session)

**Best For:**
- Development and debugging
- Remote browser control
- Flexible deployment scenarios
- When you need to connect from outside your process

**Example:**

[!code-csharp[WebSocket Connection](../code/examples/BrowserSetupSamples.cs#WebSocketConnection)]

### Pipe Connection

**How it Works:**
- A Chromium browser is launched with `--remote-debugging-pipe`
- Browser communicates via anonymous pipes (file descriptors 3 and 4 on Unix-like systems)
- Protocol uses null-terminated JSON messages
- The pipe carries **CDP**, so a BiDi-over-CDP mapper is required (see above); Firefox has no pipe mode
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

> **Note:** The `WebDriverBiDi` NuGet package does not include a browser launcher. The repository's `WebDriverBiDi.Client` demonstration library (not published to NuGet) provides a `BrowserLauncher` whose Chromium launcher implements `IPipeServerProcessProvider` and returns a `ChromiumTransport`, and the example below uses it. To do this yourself, implement `IPipeServerProcessProvider`: launch the browser with `--remote-debugging-pipe` so that it inherits the two anonymous pipe handles `PipeConnection` creates, and pass a mapper-installing `Transport` built over that `PipeConnection` to `BiDiDriver`:

[!code-csharp[Pipe Launcher Pattern](../code/examples/BrowserSetupSamples.cs#PipeLauncherPattern)]

The skeleton of your own `IPipeServerProcessProvider` implementation looks like this; `PipeServerProcess` returns the launched browser `Process`, and `CreateTransport` wraps a `PipeConnection` over `this`:

[!code-csharp[Implementing IPipeServerProcessProvider](../code/examples/BrowserSetupSamples.cs#ImplementingIPipeServerProcessProvider)]

### Comparison

| Feature | WebSocket | Pipes |
|---------|-----------|-------|
| **Latency** | Moderate (TCP overhead) | Lower (direct IPC) |
| **Remote Access** | ✓ Yes | ✗ No |
| **Multi-Client** | ✓ Yes | ✗ No |
| **Setup Complexity** | Simple | Moderate (mapper required) |
| **Debugging** | Easy (inspect traffic) | Moderate |
| **Use Case** | Development, debugging | Automation, testing |

**Recommendation:** Start with WebSocket connections through a driver executable for simplicity; switch to pipes only if you need lower latency and are prepared to host the mapper.

## Firefox

Firefox implements WebDriver BiDi natively, so there are two ways in.

### Direct: `--remote-debugging-port`

```bash
firefox --remote-debugging-port=9222
```

Firefox then serves WebDriver BiDi at `ws://localhost:9222/session`. No session exists yet, so create one after connecting:

[!code-csharp[Firefox Direct Connection](../code/examples/BrowserSetupSamples.cs#FirefoxDirectConnection)]

### Through geckodriver

1. Download geckodriver from https://github.com/mozilla/geckodriver/releases
2. Launch geckodriver:

   ```bash
   geckodriver --port 4444
   ```

3. Either create a classic session with `webSocketUrl: true` exactly as for chromedriver (browser flags go in `moz:firefoxOptions.args`) and connect to the returned `webSocketUrl` — no `NewSessionAsync` needed — or connect to geckodriver's BiDi-only endpoint and create the session yourself:

   [!code-csharp[Firefox Connection](../code/examples/BrowserSetupSamples.cs#FirefoxConnection)]

> **Important:** On the `/session` endpoints (Firefox direct, or geckodriver without a classic session) you must call `Session.NewSessionAsync` after `StartAsync`; without it, subsequent commands fail because no session exists on the remote end. On a `webSocketUrl` returned by a classic new-session request the session already exists and `NewSessionAsync` must **not** be called.
>
> See the [Session Module guide](modules/session.md) for full details and capability negotiation options.

### Note on Firefox Support

Firefox's WebDriver BiDi implementation is actively being developed. Some features may not be available or may behave differently than in Chromium-based browsers.

## Using with Selenium

If you already use Selenium, let it locate the driver and browser (Selenium Manager) and create the session; ask it for a BiDi WebSocket with `UseWebSocketUrl`, then connect a `BiDiDriver` to the `webSocketUrl` capability:

[!code-csharp[Selenium Integration](../code/examples/BrowserSetupSamples.cs#SeleniumManagerIntegration)]

WebDriverBiDi.NET does not depend on Selenium; the two share only the session. Stop the `BiDiDriver` before calling `Quit()` on the Selenium driver, which ends the session.

## Docker Container

Run the browser *and its driver* in the container and publish the driver's port; never publish a CDP port (`--remote-debugging-address=0.0.0.0`), which speaks CDP and exposes full browser control. The official Selenium images do this for you: `selenium/standalone-chrome` serves a WebDriver endpoint on port 4444 that accepts `webSocketUrl: true` and returns a BiDi URL routed through the container.

```bash
docker run -d -p 4444:4444 --shm-size=2g selenium/standalone-chrome:latest
```

Then create the session at `http://localhost:4444` exactly as in the chromedriver steps above and connect to the returned `webSocketUrl`.

## Common Launch Options

When the browser is launched by a driver, pass these as `goog:chromeOptions.args` / `ms:edgeOptions.args` / `moz:firefoxOptions.args` in the new-session capabilities; when you launch Firefox directly, put them on the command line.

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

```json
{
  "capabilities": {
    "alwaysMatch": {
      "webSocketUrl": true,
      "goog:chromeOptions": {
        "args": ["--headless=new", "--disable-gpu", "--no-sandbox", "--disable-dev-shm-usage", "--window-size=1920,1080"]
      }
    }
  }
}
```

## Programmatic Browser Launch

You can start the driver executable yourself, create the session, and only then connect. The snippet below shows just the session and connection steps; the [WebSocket Launcher Pattern](#websocket-launcher-pattern) further down shows launching the driver process as well:

[!code-csharp[Programmatic Browser Launch](../code/examples/BrowserSetupSamples.cs#ProgrammaticBrowserLaunch)]

## Implementing Your Own Launcher

The `WebDriverBiDi` NuGet package does **not** ship a browser launcher; the library only provides the protocol client. The repository's `WebDriverBiDi.Client` demonstration library shows one way to do it (see `BrowserLauncher` and its `ChromeDriverLauncher`, `GeckoDriverLauncher`, `FirefoxLauncher` and `ChromeLauncher` in `src/WebDriverBiDi.Client/Launchers`), but it is not published, so to automate browser launch in your own project you implement the launcher yourself. The patterns below sketch the two approaches.

### WebSocket Launcher Pattern

Start the driver executable, wait for its `/status` endpoint, create a session with `webSocketUrl: true`, and connect to the returned URL. Ending the session closes the browser; then stop the driver process:

[!code-csharp[WebSocket Launcher Pattern](../code/examples/BrowserSetupSamples.cs#WebSocketLauncherPattern)]

### Pipe Launcher Pattern

For pipe connections (Chromium only), implement `IPipeServerProcessProvider` to launch the browser with `--remote-debugging-pipe` and provide a `Transport` to `BiDiDriver`. Because the pipe carries CDP, that `Transport` must install a BiDi-over-CDP mapper — see `ChromiumTransport` in the demonstration library. See the `Transport` and `PipeConnection` types in the API reference for the interface contract.

## Troubleshooting

### Port Already in Use

```
Error: Port 9515 already in use
```

**Solutions:**
- Stop the other driver instance
- Use a different port: `chromedriver --port=9516`
- Find and kill the process using the port

### Connection Refused

```
WebDriverBiDiException: Connection refused
```

**Solutions:**
- Verify the driver (or Firefox with `--remote-debugging-port`) is running
- Check the WebSocket URL is correct
- Ensure no firewall is blocking the port
- Try `http://localhost:9515/status` in a browser to verify the driver is listening

### Browser Closes Immediately

**Solutions:**
- Use `--user-data-dir` to specify a profile
- Check for conflicting flags
- Run without `--headless` to debug

### Connected, but the First Command Fails

If `StartAsync` succeeds and the first command (or `Session.StatusAsync`) fails or times out, the URL is almost certainly a CDP endpoint:

**Solutions:**
- A URL containing `/devtools/browser/` or `/devtools/page/` is Chrome's CDP endpoint; WebDriver BiDi is not spoken there
- Use the `webSocketUrl` from a driver's new-session response (`ws://localhost:9515/session/<id>`), or Firefox's `ws://localhost:PORT/session`

### "session not created" After Connecting

**Cause:** `Session.NewSessionAsync` was called on a connection whose session already exists (any `webSocketUrl` returned by chromedriver, msedgedriver, geckodriver or Selenium).

**Solution:** Call `NewSessionAsync` only on the `/session` endpoints of Firefox or geckodriver, or after connecting through a mapper.

## Best Practices

1. **Use a dedicated profile**: `--user-data-dir` (or let the driver create a temporary one) prevents conflicts
2. **Fixed port**: Always use the same driver port for consistency
3. **Launch before connect**: Wait for the driver's `/status` endpoint before creating a session
4. **Clean shutdown**: Close connections before killing browser
5. **Headless for CI**: Use `--headless=new` in CI environments
6. **Log output**: Redirect stdout/stderr when launching programmatically

## Security Considerations

⚠️ **Warning**: A driver port, a Firefox remote-debugging port, and above all a Chrome CDP port expose full browser control. Do not:
- Run them on production systems
- Expose them to the internet
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
