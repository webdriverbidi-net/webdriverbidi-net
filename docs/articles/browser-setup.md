# Browser Setup Guide

WebDriverBiDi.NET connects to a browser over a WebSocket (or, for Chromium, a pipe) that speaks WebDriver BiDi. There are two ways to get one:

- **The `WebDriverBiDi.Browsers` package** (recommended) downloads, launches, and connects to the browser for you, locally, on a remote grid, or already running.
- **Setting the browser up yourself**, with its driver executable or its own remote debugging endpoint, and connecting to the URL it reports. This is what the package does for you, described step by step.

## Using WebDriverBiDi.Browsers

```bash
dotnet add package WebDriverBiDi.Browsers
```

### Launching and Connecting

A `BrowserLauncher` starts the browser, and creates the `Transport` over which a `BiDiDriver` talks to it. The first launch downloads the browser into a local cache:

[!code-csharp[Launch and Connect](../code/PackageReadmeSamples.cs#BrowsersQuickStart)]

A browser launched through its driver executable already has a WebDriver BiDi session, so `IsBiDiSessionInitialized` tells you whether to create one. Disposing the launcher closes the browser and deletes its temporary profile.

### Choosing the Browser

`BrowserLauncher.Configure` takes the browser (Chrome, Firefox, or Safari), and the builder chooses how it is found and launched:

| Setting | Methods | Default |
| --- | --- | --- |
| Channel | `WithReleaseChannel`: `Stable`, `Beta`, `DeveloperPreview` (Chrome Dev, Firefox Developer Edition, Safari Technology Preview), `Alpha` (Chrome Canary, Firefox Nightly), `ExtendedSupport` (Firefox ESR) | `Stable` |
| Version | `WithVersion`: `BrowserVersion.Latest`, `BrowserVersion.Specific("…")`, or, for Chrome, `BrowserVersion.Milestone(131)` | The latest of the channel |
| Location | `AtAutomaticallyDownloadedLocation()`, `AtDefaultInstallationLocation()`, `AtLocation(path)` | Downloaded |
| Launch | Directly, or `LaunchUsingDriver()` through chromedriver, geckodriver, or safaridriver | Directly (Safari: always through safaridriver) |
| Connection | `WithConnection(ConnectionKind.Pipes)`, for Chrome launched directly | WebSocket |
| Browser settings | `WithHeadlessOption`, `WithArguments`, `WithoutDefaultArguments`, `WithEnvironmentVariable`, `WithUserDataDirectory`, `WithLaunchTimeout` | |
| Per-browser settings | `WithBrowserOptions(new ChromeLaunchOptions { … })` or `FirefoxLaunchOptions` | |

[!code-csharp[Choosing the Browser](../code/PackageReadmeSamples.cs#BrowsersChoosingBrowser)]

`Build()` checks that the settings can be used together, and throws `BrowserLauncherConfigurationException` if they cannot.

On Linux:

- A Firefox installed as a **Snap or Flatpak** (as Ubuntu's `/usr/bin/firefox` is) cannot read a profile in the temporary directory, so launching it without `WithUserDataDirectory` fails with an explanation. Use the downloaded Firefox, or a profile directory the sandbox can read.
- Running as **root**, Chrome refuses to start with its sandbox enabled, so `--no-sandbox` is added for you.

### Remote Grids and Running Browsers

`LaunchUsingRemoteGrid` creates the session on a Selenium Grid or a cloud service. The URL carries the grid's port and path prefix, credentials in it are sent as Basic authorization, and `RemoteGridOptions` holds the capabilities and headers of the request:

[!code-csharp[Remote Grid](../code/PackageReadmeSamples.cs#BrowsersRemoteGrid)]

`ConnectToExisting` attaches to a browser that is already listening, which the launcher neither starts nor stops: closing the `BrowserInstance` only detaches from it. Give it Firefox's WebDriver BiDi URL (`ws://127.0.0.1:9222/session`), or Chrome's DevTools URL, which is reached through the WebDriver BiDi mapper:

[!code-csharp[Connect to a Running Browser](../code/PackageReadmeSamples.cs#BrowsersConnectToExisting)]

### Locating Without Launching

`BrowserLocator.FindBrowserAsync` and `DriverLocator.FindDriverAsync` return the path of a browser or driver, downloading it if needed, for use with other tools:

[!code-csharp[Locating Without Launching](../code/PackageReadmeSamples.cs#BrowsersLocateOnly)]

### Downloads and the Cache

Downloads are cached per browser, channel, and version, and a cached browser is used without a network request. The version a channel resolves to is rechecked once a day, and if the service cannot be reached, the cached version is used. Failed requests are retried twice. Firefox and geckodriver downloads are verified against their published SHA-256 checksums; Chrome for Testing publishes none, so Chrome downloads are checked against the length and MD5 hash its storage service reports, which detects corruption in transit but not tampering.

`BrowserDownloadOptions` changes where browsers are cached and downloaded from:

[!code-csharp[Download Options](../code/PackageReadmeSamples.cs#BrowsersDownloadOptions)]

These environment variables set the defaults, so a CI system can configure every test run without code changes:

| Variable | Effect |
| --- | --- |
| `WEBDRIVERBIDI_BROWSERS_PATH` | The cache directory. The default is a `webdriverbidi-net` directory in the local application data directory (Windows), `~/Library/Caches` (macOS), or `$XDG_CACHE_HOME` or `~/.cache` (Linux). |
| `WEBDRIVERBIDI_SKIP_DOWNLOAD` | Set to `1` or `true` to use only what is already cached, making no network requests. |
| `WEBDRIVERBIDI_DOWNLOAD_MANIFEST` | The URL or file path of a mirror manifest (below). |
| `CHROME_EXECUTABLE`, `FIREFOX_EXECUTABLE`, `SAFARI_EXECUTABLE` | A browser executable to use in place of locating one. |
| `CHROMEDRIVER_EXECUTABLE`, `GECKODRIVER_EXECUTABLE`, `SAFARIDRIVER_EXECUTABLE` | A driver executable to use in place of locating one. |

### Mirroring Downloads

To download from an internal mirror rather than the vendors' services, set `BrowserDownloadOptions.ManifestUrl` (or `WEBDRIVERBIDI_DOWNLOAD_MANIFEST`) to a manifest listing the builds it holds. Every browser and driver is then resolved through the manifest; one it does not list cannot be downloaded. The manifest is JSON:

```json
{
  "schemaVersion": 1,
  "browsers": {
    "chrome": {
      "channels": { "stable": "131.0.6778.204" },
      "versions": {
        "131.0.6778.204": {
          "linux-x64": {
            "url": "chrome/131.0.6778.204/chrome-linux64.zip",
            "sha256": "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08",
            "size": 171234567
          }
        }
      }
    }
  },
  "drivers": {
    "chromedriver": {
      "versions": {
        "131.0.6778.204": {
          "linux-x64": { "url": "chrome/131.0.6778.204/chromedriver-linux64.zip", "sha256": "…" }
        }
      }
    },
    "geckodriver": {
      "latest": "0.36.0",
      "versions": {
        "0.36.0": {
          "linux-x64": { "url": "geckodriver/geckodriver-v0.36.0-linux64.tar.gz", "sha256": "…" }
        }
      }
    }
  }
}
```

- **Browsers** are `chrome`, `chrome-headless-shell`, and `firefox`; **drivers** are `chromedriver` and `geckodriver`.
- **Channels** are `stable`, `beta`, `dev`, `canary`, `nightly`, and `esr`. A milestone resolves to the highest version listed for it; chromedriver's version follows the browser's, and geckodriver's is its `latest`.
- **Platforms** are `linux-x64`, `linux-arm64`, `linux-x86`, `macos-x64`, `macos-arm64`, `windows-x64`, `windows-x86`, and `windows-arm64`.
- **Builds** are each vendor's archive, unchanged. Each lists its SHA-256 hash, which is verified, and optionally its size. A URL may be relative to the manifest, so a directory holding the manifest and the archives serves as a mirror with no server (`file:///…/manifest.json`).
- **Nightly builds** share version numbers, so give each mirrored Nightly build a distinct version string.

### Errors

Every exception the package throws derives from `WebDriverBiDiException`: `BrowserDownloadException` when a browser or driver cannot be located or downloaded, `BrowserLaunchException` (with the exit code and the end of the process output) when one cannot be started, and `BrowserLauncherConfigurationException` when `Build()` finds settings that cannot be used together.

## Setting Up a Browser Manually

This section explains how to obtain a WebDriver BiDi endpoint for each browser yourself and connect WebDriverBiDi.NET to it.

### Overview

`BiDiDriver.StartAsync` needs a WebSocket URL that **speaks WebDriver BiDi**. Which URL that is depends on the browser:

| Browser | Speaks BiDi natively? | How to get a BiDi endpoint |
|---------|-----------------------|----------------------------|
| Chrome / Chromium / Edge | **No.** `--remote-debugging-port` and `--remote-debugging-pipe` expose the Chrome DevTools Protocol (CDP) only | Create a session through **chromedriver** / **msedgedriver** with the `webSocketUrl` capability (recommended), or inject a BiDi-over-CDP mapper (advanced) |
| Firefox | **Yes.** `--remote-debugging-port` exposes BiDi at `/session` | Connect directly, or go through **geckodriver** |

> **Important:** the `ws://localhost:9222/devtools/browser/<id>` URL that Chrome prints at startup and reports from `http://localhost:9222/json/version` is a **CDP** endpoint. A `BiDiDriver` connected to it will open the socket successfully and then fail on its first command, because the browser does not understand WebDriver BiDi messages on that endpoint. Do not use it.

The library supports two connection types, both usable with any of the above:

- **WebSocket Connection** (default): connects to a WebSocket URL
- **Pipe Connection**: connects over anonymous pipes to a browser you launched with `--remote-debugging-pipe` (Chromium only; see [Connection Types](#connection-types))

### Chrome / Chromium

#### Through chromedriver (recommended)

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

#### Through a BiDi-over-CDP mapper (advanced)

The [chromium-bidi](https://github.com/GoogleChromeLabs/chromium-bidi) project provides a JavaScript "mapper" that implements WebDriver BiDi on top of CDP. Injecting it into a hidden tab lets a client talk BiDi over the browser's own CDP endpoint (WebSocket or pipe) with no driver executable. The `WebDriverBiDi.Browsers` package does exactly this in its `ChromiumTransport` (a `Transport` subclass that bootstraps the mapper during `ConnectAsync`), which its Chrome launcher uses; it is also the reference for building your own. This is the only route that works over a pipe connection, and it requires `Session.NewSessionAsync` after connecting because the mapper does not create a session.

### Microsoft Edge

Edge is Chromium-based and follows the chromedriver path exactly, using **msedgedriver** (from the [Edge WebDriver page](https://developer.microsoft.com/microsoft-edge/tools/webdriver/)) and `ms:edgeOptions` in place of `goog:chromeOptions`:

```bash
msedgedriver --port=9515
```

### Connection Types

WebDriverBiDi.NET supports two transport mechanisms for communicating with browsers:

#### WebSocket Connection (Default)

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

#### Pipe Connection

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

> **Note:** The `WebDriverBiDi` package does not include a browser launcher; the `WebDriverBiDi.Browsers` package does. Its Chrome launcher, configured with `WithConnection(ConnectionKind.Pipes)`, implements `IPipeServerProcessProvider` and returns a `ChromiumTransport`, and the example below uses it. To do this yourself, implement `IPipeServerProcessProvider`: launch the browser with `--remote-debugging-pipe` so that it inherits the two anonymous pipe handles `PipeConnection` creates — `PipeConnection.ReadPipeHandle` and `PipeConnection.WritePipeHandle` give you those handles as strings to pass to the child process, and both return an empty string once the connection has started, because the first start disposes the connection's local copies of those handles, so read them before calling `StartAsync` — and pass a mapper-installing `Transport` built over that `PipeConnection` to `BiDiDriver`:

[!code-csharp[Pipe Launcher Pattern](../code/examples/BrowserSetupSamples.cs#PipeLauncherPattern)]

The skeleton of your own `IPipeServerProcessProvider` implementation looks like this; `PipeServerProcess` returns the launched browser `Process`, and `CreateTransport` wraps a `PipeConnection` over `this`:

[!code-csharp[Implementing IPipeServerProcessProvider](../code/examples/BrowserSetupSamples.cs#ImplementingIPipeServerProcessProvider)]

#### Comparison

| Feature | WebSocket | Pipes |
|---------|-----------|-------|
| **Latency** | Moderate (TCP overhead) | Lower (direct IPC) |
| **Remote Access** | ✓ Yes | ✗ No |
| **Multi-Client** | ✓ Yes | ✗ No |
| **Setup Complexity** | Simple | Moderate (mapper required) |
| **Debugging** | Easy (inspect traffic) | Moderate |
| **Use Case** | Development, debugging | Automation, testing |

**Recommendation:** Start with WebSocket connections through a driver executable for simplicity; switch to pipes only if you need lower latency and are prepared to host the mapper.

### Firefox

Firefox implements WebDriver BiDi natively, so there are two ways in.

#### Direct: `--remote-debugging-port`

```bash
firefox --remote-debugging-port=9222
```

Firefox then serves WebDriver BiDi at `ws://localhost:9222/session`. No session exists yet, so create one after connecting:

[!code-csharp[Firefox Direct Connection](../code/examples/BrowserSetupSamples.cs#FirefoxDirectConnection)]

#### Through geckodriver

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

#### Note on Firefox Support

Firefox's WebDriver BiDi implementation is actively being developed. Some features may not be available or may behave differently than in Chromium-based browsers.

### Using with Selenium

If you already use Selenium, let it locate the driver and browser (Selenium Manager) and create the session; ask it for a BiDi WebSocket with `UseWebSocketUrl`, then connect a `BiDiDriver` to the `webSocketUrl` capability:

[!code-csharp[Selenium Integration](../code/examples/BrowserSetupSamples.cs#SeleniumManagerIntegration)]

WebDriverBiDi.NET does not depend on Selenium; the two share only the session. Stop the `BiDiDriver` before calling `Quit()` on the Selenium driver, which ends the session.

### Docker Container

Run the browser *and its driver* in the container and publish the driver's port; never publish a CDP port (`--remote-debugging-address=0.0.0.0`), which speaks CDP and exposes full browser control. The official Selenium images do this for you: `selenium/standalone-chrome` serves a WebDriver endpoint on port 4444 that accepts `webSocketUrl: true` and returns a BiDi URL routed through the container.

```bash
docker run -d -p 4444:4444 --shm-size=2g selenium/standalone-chrome:latest
```

Then create the session at `http://localhost:4444` exactly as in the chromedriver steps above and connect to the returned `webSocketUrl`.

### Common Launch Options

When the browser is launched by a driver, pass these as `goog:chromeOptions.args` / `ms:edgeOptions.args` / `moz:firefoxOptions.args` in the new-session capabilities; when you launch Firefox directly, put them on the command line.

#### Disable GPU

Useful for headless environments:
```
--disable-gpu
```

#### Window Size

Set initial window size:
```
--window-size=1920,1080
```

#### Disable Extensions

Start without extensions:
```
--disable-extensions
```

#### Incognito Mode

Start in incognito/private mode:
```
--incognito
```

#### No Sandbox (Docker/CI)

Disable sandboxing (needed in some containerized environments):
```
--no-sandbox
```

#### Disable Dev Shm (Docker)

Prevent shared memory issues in Docker:
```
--disable-dev-shm-usage
```

#### Example CI Launch

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

### Programmatic Browser Launch

You can start the driver executable yourself, create the session, and only then connect. The snippet below shows just the session and connection steps; the [WebSocket Launcher Pattern](#websocket-launcher-pattern) further down shows launching the driver process as well:

[!code-csharp[Programmatic Browser Launch](../code/examples/BrowserSetupSamples.cs#ProgrammaticBrowserLaunch)]

### Implementing Your Own Launcher

The `WebDriverBiDi` package itself provides only the protocol client; launching belongs to the `WebDriverBiDi.Browsers` package ([above](#using-webdriverbidibrowsers)). If you need a launcher of your own, for a browser or an environment that package does not cover, the patterns below sketch the two approaches.

#### WebSocket Launcher Pattern

Start the driver executable, wait for its `/status` endpoint, create a session with `webSocketUrl: true`, and connect to the returned URL. Ending the session closes the browser; then stop the driver process:

[!code-csharp[WebSocket Launcher Pattern](../code/examples/BrowserSetupSamples.cs#WebSocketLauncherPattern)]

#### Pipe Launcher Pattern

For pipe connections (Chromium only), implement `IPipeServerProcessProvider` to launch the browser with `--remote-debugging-pipe` and provide a `Transport` to `BiDiDriver`. Because the pipe carries CDP, that `Transport` must install a BiDi-over-CDP mapper — see `ChromiumTransport` in the `WebDriverBiDi.Browsers` library. See the `Transport` and `PipeConnection` types in the API reference for the interface contract.

### Troubleshooting

#### Port Already in Use

```
Error: Port 9515 already in use
```

**Solutions:**
- Stop the other driver instance
- Use a different port: `chromedriver --port=9516`
- Find and kill the process using the port

#### StartAsync Cannot Connect

```
WebDriverBiDiTimeoutException: Could not connect to remote WebSocket server within 10 seconds
```

A connection that is refused, or fails for any other reason, is retried every 500 milliseconds until the
connection's `StartupTimeout` (10 seconds by default) runs out, so an endpoint that is not listening surfaces as this
timeout rather than as a refusal.

**Solutions:**
- Verify the driver (or Firefox with `--remote-debugging-port`) is running
- Check the WebSocket URL is correct
- Ensure no firewall is blocking the port
- Try `http://localhost:9515/status` in a browser to verify the driver is listening

#### Browser Closes Immediately

**Solutions:**
- Use `--user-data-dir` to specify a profile
- Check for conflicting flags
- Run without `--headless` to debug

#### Connected, but the First Command Fails

If `StartAsync` succeeds and the first command (or `Session.StatusAsync`) fails or times out, the URL is almost certainly a CDP endpoint:

**Solutions:**
- A URL containing `/devtools/browser/` or `/devtools/page/` is Chrome's CDP endpoint; WebDriver BiDi is not spoken there
- Use the `webSocketUrl` from a driver's new-session response (`ws://localhost:9515/session/<id>`), or Firefox's `ws://localhost:PORT/session`

#### "session not created" After Connecting

**Cause:** `Session.NewSessionAsync` was called on a connection whose session already exists (any `webSocketUrl` returned by chromedriver, msedgedriver, geckodriver or Selenium).

**Solution:** Call `NewSessionAsync` only on the `/session` endpoints of Firefox or geckodriver, or after connecting through a mapper.

### Best Practices

1. **Use a dedicated profile**: `--user-data-dir` (or let the driver create a temporary one) prevents conflicts
2. **Port selection**: Let the launcher pick a free port. Fix one only when something outside the test must know it in advance, and never share a fixed port between concurrent sessions
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
