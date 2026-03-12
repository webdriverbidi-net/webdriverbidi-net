# Getting Started with WebDriverBiDi.NET

This guide will walk you through installing WebDriverBiDi.NET and setting up your first browser automation project.

## Prerequisites

- **.NET SDK**: Runtime compatible with .NET Standard 2.0, supporting .NET Framework 4.6.1+, .NET Core 2.0+, or .NET 5.0+
- **IDE**: Visual Studio, Visual Studio Code, or JetBrains Rider
- **Browser**: A browser with WebDriver BiDi support (Chrome, Edge, Firefox)

## Installation

### Using NuGet Package Manager

Install the WebDriverBiDi package from NuGet:

```bash
dotnet add package WebDriverBiDi
```

Or using the Package Manager Console in Visual Studio:

```powershell
Install-Package WebDriverBiDi
```

### Using Package Reference

Add this to your `.csproj` file:

```xml
<PackageReference Include="WebDriverBiDi" Version="*" />
```

### Optional: Roslyn Analyzers

For compile-time help catching common usage errors, add the [WebDriverBiDi.Analyzers](advanced/analyzers.md) package. See [Roslyn Analyzers](advanced/analyzers.md) for the full list of available analyzers.

## Browser Setup

WebDriverBiDi.NET requires a browser with WebDriver BiDi support running and listening on a WebSocket endpoint.

### Chrome/Chromium

Launch Chrome with the `--remote-debugging-port` flag:

```bash
# Windows
chrome.exe --remote-debugging-port=9222

# macOS
/Applications/Google\ Chrome.app/Contents/MacOS/Google\ Chrome --remote-debugging-port=9222

# Linux
google-chrome --remote-debugging-port=9222
```

For Chromium-based browsers, use the browser-level WebSocket URL returned by `/json/version`, for example `ws://localhost:9222/devtools/browser/<browser-id>`

### Microsoft Edge

Launch Edge similarly:

```bash
# Windows
msedge.exe --remote-debugging-port=9222

# macOS
/Applications/Microsoft\ Edge.app/Contents/MacOS/Microsoft\ Edge --remote-debugging-port=9222
```

### Firefox

Firefox requires geckodriver with WebDriver BiDi support:

```bash
# Download geckodriver from https://github.com/mozilla/geckodriver/releases
geckodriver --port 4444

# Then launch Firefox through geckodriver
```

### Discovering the WebSocket URL

Chromium-based browsers provide HTTP endpoints to discover WebSocket URLs programmatically.

#### Method 1: Browser-Level WebSocket URL

The `/json/version` endpoint returns the browser-level WebSocket URL:

```bash
# Query the endpoint
curl http://localhost:9222/json/version
```

**Example JSON response:**
```json
{
  "Browser": "Chrome/120.0.6099.129",
  "Protocol-Version": "1.3",
  "User-Agent": "Mozilla/5.0...",
  "V8-Version": "12.0.267.17",
  "WebKit-Version": "537.36",
  "webSocketDebuggerUrl": "ws://localhost:9222/devtools/browser/a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

The `webSocketDebuggerUrl` field contains the URL to use with `BiDiDriver.StartAsync()`.

**Programmatic Discovery:**

[!code-csharp[Discover WebSocket URL](../code/examples/GettingStartedSamples.cs#DiscoverWebSocketURL)]

[!code-csharp[Discover WebSocket URL](../code/examples/GettingStartedSamples.cs#DiscoverWebSocketUrlUsage)]

#### Method 2: Page-Specific WebSocket URLs

The `/json` endpoint returns information about all open pages:

```bash
# Query all pages
curl http://localhost:9222/json
```

**Example JSON response:**
```json
[
  {
    "description": "",
    "devtoolsFrontendUrl": "/devtools/inspector.html?ws=localhost:9222/devtools/page/123",
    "id": "page-123",
    "title": "Example Domain",
    "type": "page",
    "url": "https://example.com/",
    "webSocketDebuggerUrl": "ws://localhost:9222/devtools/page/page-123"
  },
  {
    "id": "page-456",
    "title": "Google",
    "type": "page",
    "url": "https://www.google.com/",
    "webSocketDebuggerUrl": "ws://localhost:9222/devtools/page/page-456"
  }
]
```

Each page has its own `webSocketDebuggerUrl`. However, **for WebDriver BiDi, use the browser-level URL from `/json/version`**, not page-specific URLs.

#### Method 3: Browser Console Output

When launching Chrome with `--remote-debugging-port`, it prints the DevTools URL to the console:

```
DevTools listening on ws://127.0.0.1:9222/devtools/browser/a1b2c3d4-e5f6-7890-abcd-ef1234567890
```

You can parse this output programmatically when launching the browser.

#### Common Connection String Formats

**Browser-level (recommended for WebDriver BiDi):**
```
ws://localhost:9222/devtools/browser/<browser-id>
```

**Firefox via geckodriver:**
```
ws://localhost:4444/session
```

**Page-specific (not recommended for WebDriver BiDi):**
```
ws://localhost:9222/devtools/page/<page-id>
```

#### Complete Discovery Example

[!code-csharp[Connect to Browser](../code/examples/GettingStartedSamples.cs#ConnecttoBrowser)]

[!code-csharp[Connect to Browser](../code/examples/GettingStartedSamples.cs#ConnectToBrowserUsage)]

**Best Practices:**
- Use `/json/version` to get the browser-level WebSocket URL
- Include fallback logic for connection failures
- Validate the WebSocket URL format before connecting
- Handle HttpClient timeouts appropriately (browser might not be ready)

### Connection Methods

WebDriverBiDi.NET supports two ways to connect to browsers:

- **WebSocket Connection** (used in this guide): Browser listens on a port, your application connects via WebSocket URL
- **Pipe Connection**: Browser communicates via anonymous pipes for lower latency

For getting started, WebSocket connections are recommended as they're simpler to configure and supported by most browsers. See [Browser Setup](browser-setup.md#connection-types) for more details about connection methods.

## Creating Your First Application

### 1. Create a New Console Application

```bash
dotnet new console -n MyFirstBiDiApp
cd MyFirstBiDiApp
dotnet add package WebDriverBiDi
```

### 2. Write the Code

Replace the contents of `Program.cs`:

[!code-csharp[First Application](../code/examples/GettingStartedSamples.cs#FirstApplication)]

### 3. Run the Application

```bash
dotnet run
```

## Understanding the Code

Let's break down what this code does:

### Creating the Driver

[!code-csharp[Creating the Driver](../code/examples/GettingStartedSamples.cs#CreatingtheDriver)]

The `BiDiDriver` is the main entry point for all WebDriver BiDi operations. The timeout parameter specifies how long to wait for command responses.

### Connecting to the Browser

[!code-csharp[Connecting to Browser](../code/examples/GettingStartedSamples.cs#ConnectingtoBrowser)]

This establishes a WebSocket connection to the browser. The browser must already be running with WebDriver BiDi enabled.

### Getting the Browsing Context

[!code-csharp[Getting Browsing Context](../code/examples/GettingStartedSamples.cs#GettingBrowsingContext)]

A browsing context represents a tab, window, or iframe. You need the context ID to perform operations like navigation or script execution.

### Navigating

[!code-csharp[Navigating](../code/examples/GettingStartedSamples.cs#Navigating)]

The `Wait` property controls when the command returns:
- `ReadinessState.None`: Returns immediately after navigation starts
- `ReadinessState.Interactive`: Waits for DOM ready
- `ReadinessState.Complete`: Waits for page load complete (including images, stylesheets)

### Executing JavaScript

[!code-csharp[Executing JavaScript](../code/examples/GettingStartedSamples.cs#ExecutingJavaScript)]

The third parameter (`true`) indicates whether to await promises in the JavaScript code.

## Next Steps

Now that you have a working WebDriverBiDi.NET application, explore these topics:

1. **[Core Concepts](core-concepts.md)**: Understand modules, commands, and events
2. **[Browser Setup](browser-setup.md)**: Learn about connection types and browser configuration
3. **[Architecture](architecture.md)**: Understand the library's design and connection architecture
4. **[Browser Module](modules/browser.md)**: Learn about browser-level operations
5. **[Events and Observables](events-observables.md)**: Handle browser events asynchronously
6. **[Common Scenarios](examples/common-scenarios.md)**: See practical examples

## Troubleshooting

### "Connection refused" Error

- Ensure the browser is running with remote debugging enabled
- Verify the port number matches your browser's configuration
- Check that no firewall is blocking the connection

### "Timeout waiting for command" Error

- Increase the timeout when creating the `BiDiDriver`, or override for specific commands using the `timeoutOverride` parameter on module methods (e.g., `NavigateAsync(parameters, TimeSpan.FromSeconds(120))`)
- Check that the browser is responsive
- Ensure the command parameters are valid

### "Module not found" Error

- Verify that your browser supports the specific module
- Some modules are experimental and may require specific browser flags

## Additional Resources

- [WebDriver BiDi Specification](https://w3c.github.io/webdriver-bidi/)
- [GitHub Repository](https://github.com/webdriverbidi-net/webdriverbidi-net)
- [NuGet Package](https://www.nuget.org/packages/WebDriverBiDi)

