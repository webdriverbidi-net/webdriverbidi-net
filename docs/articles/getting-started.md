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

WebDriverBiDi.NET requires a WebSocket endpoint that speaks WebDriver BiDi. See the [Browser Setup Guide](browser-setup.md) for the full picture; the essentials are below.

### Chrome, Chromium and Edge

Chrome and Edge do **not** speak WebDriver BiDi on their `--remote-debugging-port` endpoint — that endpoint (`ws://localhost:9222/devtools/browser/<id>`, reported by `/json/version`) speaks the Chrome DevTools Protocol only, and a `BiDiDriver` connected to it fails on its first command. Use the browser's driver executable instead: chromedriver (from [Chrome for Testing](https://googlechromelabs.github.io/chrome-for-testing/)) or msedgedriver.

```bash
chromedriver --port=9515
```

Then create a WebDriver session that asks for a BiDi WebSocket:

```bash
curl -X POST http://localhost:9515/session \
  -H "Content-Type: application/json" \
  -d '{"capabilities":{"alwaysMatch":{"webSocketUrl":true}}}'
```

The response's `value.capabilities.webSocketUrl` — for example `ws://localhost:9515/session/8a4d1c2e-…` — is the URL for `BiDiDriver.StartAsync()`. chromedriver launches the browser as part of creating the session, so you do not start Chrome yourself; browser flags such as `--headless=new` go in `goog:chromeOptions.args` (`ms:edgeOptions.args` for Edge).

### Firefox

Firefox speaks WebDriver BiDi natively. Either launch it directly:

```bash
firefox --remote-debugging-port=9222
```

and connect to `ws://localhost:9222/session`, or run geckodriver (`geckodriver --port 4444`) and connect to `ws://localhost:4444/session`. On both of these endpoints you must call `driver.Session.NewSessionAsync(...)` after `StartAsync`, because no session exists yet. (geckodriver also accepts the `webSocketUrl: true` classic session shown above, in which case the session already exists.)

### Getting the WebSocket URL Programmatically

Creating the session from C# is a single HTTP request:

[!code-csharp[Create a BiDi Session](../code/examples/GettingStartedSamples.cs#DiscoverWebSocketURL)]

[!code-csharp[Create a BiDi Session Usage](../code/examples/GettingStartedSamples.cs#DiscoverWebSocketUrlUsage)]

#### Common Connection String Formats

**Session created through chromedriver, msedgedriver or geckodriver (recommended):**
```
ws://localhost:9515/session/<session-id>
```
The session already exists; do not call `NewSessionAsync`.

**Firefox launched directly, or geckodriver's BiDi-only endpoint:**
```
ws://localhost:9222/session
ws://localhost:4444/session
```
Call `NewSessionAsync` after connecting.

**Chrome's CDP endpoints (do not use):**
```
ws://localhost:9222/devtools/browser/<browser-id>
ws://localhost:9222/devtools/page/<page-id>
```

#### Complete Connection Example

[!code-csharp[Connect to Browser](../code/examples/GettingStartedSamples.cs#ConnecttoBrowser)]

[!code-csharp[Connect to Browser](../code/examples/GettingStartedSamples.cs#ConnectToBrowserUsage)]

**Best Practices:**
- Create the session through the driver and use its `webSocketUrl`; never a `/devtools/…` URL
- Include fallback logic for connection failures
- Handle HttpClient timeouts appropriately (the driver launches the browser while answering the new-session request)

### Connection Methods

WebDriverBiDi.NET supports two ways to connect to browsers:

- **WebSocket Connection** (used in this guide): a driver executable or Firefox listens on a port, your application connects via WebSocket URL
- **Pipe Connection**: a Chromium browser communicates via anonymous pipes for lower latency (requires a BiDi-over-CDP mapper; see Browser Setup)

For getting started, WebSocket connections are recommended as they're simpler to configure and supported by most browsers. See [Browser Setup](browser-setup.md#connection-types) for more details about connection methods.

## Creating Your First Application

### 1. Create a New Console Application

```bash
dotnet new console -n MyFirstBiDiApp
cd MyFirstBiDiApp
dotnet add package WebDriverBiDi
```

### 2. Write the Code

Replace the contents of `Program.cs` with the code below, adding these `using` directives at the top of the file:

```csharp
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;
```

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

> **Tip:** By default, the driver silently discards event handler exceptions and protocol errors. During
> development, set the error behaviors to `TransportErrorBehavior.Terminate` so that problems surface
> immediately rather than being swallowed:
>
> ```csharp
> BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
> driver.EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate;
> driver.ProtocolErrorBehavior = TransportErrorBehavior.Terminate;
> driver.UnknownMessageBehavior = TransportErrorBehavior.Terminate;
> driver.UnexpectedErrorBehavior = TransportErrorBehavior.Terminate;
> ```
>
> See [Error Handling](advanced/error-handling.md) for a full explanation of the four error behavior
> properties and the recommended settings for production use.

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

- Ensure the driver executable (or Firefox with `--remote-debugging-port`) is running and, for a driver, that the session was created
- Verify the URL is the session's `webSocketUrl` (or Firefox's `/session`), not a `/devtools/…` CDP URL
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

