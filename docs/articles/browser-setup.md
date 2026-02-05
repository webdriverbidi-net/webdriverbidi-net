# Browser Setup Guide

This guide explains how to set up different browsers for use with WebDriverBiDi.NET-Relaxed.

## Overview

WebDriverBiDi.NET-Relaxed requires a browser with WebDriver BiDi support running with remote debugging enabled. The library connects to the browser via a WebSocket connection.

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

```csharp
await driver.StartAsync("ws://localhost:9222/devtools/browser/abc-123-def");
```

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

```csharp
await driver.StartAsync("ws://localhost:4444/session");
```

### Note on Firefox Support

Firefox's WebDriver BiDi implementation is actively being developed. Some features may not be available or may behave differently than in Chromium-based browsers.

## Using with Selenium Manager

If you're using Selenium, you can let Selenium Manager handle browser launching:

```csharp
// This is conceptual - WebDriverBiDi.NET-Relaxed doesn't include Selenium Manager
// But you can use them together
var seleniumDriver = new ChromeDriver(chromeOptions);
string wsUrl = (string)((ChromeDriver)seleniumDriver)
    .ExecuteCdpCommand("Target.getTargets", new Dictionary<string, object>())
    ["webSocketDebuggerUrl"];

await driver.StartAsync(wsUrl);
```

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

You can launch the browser programmatically before connecting:

```csharp
using System.Diagnostics;

// Launch Chrome
Process chromeProcess = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "chrome.exe",
        Arguments = "--remote-debugging-port=9222 --user-data-dir=C:\\temp\\chrome",
        UseShellExecute = false,
        RedirectStandardOutput = true
    }
};
chromeProcess.Start();

// Wait for Chrome to start
await Task.Delay(2000);

// Get WebSocket URL
using HttpClient client = new HttpClient();
string json = await client.GetStringAsync("http://localhost:9222/json/version");
// Parse JSON to get webSocketDebuggerUrl

// Connect
await driver.StartAsync(webSocketUrl);

// Later: clean up
chromeProcess.Kill();
```

## Helper Libraries

The `WebDriverBiDi.Client` project in this repository includes launcher helpers:

```csharp
using WebDriverBiDi.Client.Launchers;

// Launch Chrome and get WebSocket URL
ChromeLauncher launcher = new ChromeLauncher();
string wsUrl = await launcher.LaunchAsync(new ChromeLaunchOptions
{
    Port = 9222,
    Headless = true,
    UserDataDir = "/tmp/chrome-profile"
});

await driver.StartAsync(wsUrl);

// Clean up
await launcher.StopAsync();
```

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

