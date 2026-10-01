# Your First WebDriverBiDi Application

This tutorial walks you through creating a complete WebDriverBiDi.NET application from scratch.

## Prerequisites

- .NET SDK 8.0 or higher installed, to build and run the console application this tutorial walks through. The library itself needs only a runtime compatible with .NET Standard 2.0
- Firefox, which the application connects to (see [Using Chrome or Edge](#using-chrome-or-edge) for those browsers)
- Basic knowledge of C# and async/await

## Step 1: Create the Project

Open a terminal and create a new console application:

```bash
mkdir MyFirstBiDiApp
cd MyFirstBiDiApp
dotnet new console
```

## Step 2: Add the NuGet Packages

Add the WebDriverBiDi package, the protocol client:

```bash
dotnet add package WebDriverBiDi
```

## Step 3: Write the Application

Replace the contents of `Program.cs` with the code below, adding these `using` directives at the top of the file:

<!-- inline-csharp: the using directives the sample needs, quoted on their own -->
```csharp
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Log;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
```

[!code-csharp[Full First Application](../code/examples/FirstApplicationSamples.cs#FullFirstApplication)]

## Step 4: Run the Application

Start Firefox with its remote debugging port, on which it serves WebDriver BiDi:

```bash
firefox --remote-debugging-port=9222
```

Then run the application:

```bash
dotnet run
```

You should see output similar to:

```
Connecting to browser...
Connected!

Getting browsing contexts...
Active context: ABC123

Navigating to example.com...
Navigation complete! URL: https://example.com/

Getting page title...
Page title: Example Domain

Getting page information...
Page Analysis:
  URL: https://example.com/
  Links: 1
  Headings: 1
  Paragraphs: 2

Capturing screenshot...
Screenshot saved to example-screenshot.png

✓ All operations completed successfully!

Press any key to exit...
```

## Understanding the Code

### 1. Connecting

[!code-csharp[Connecting](../code/examples/FirstApplicationSamples.cs#DriverInitialization)]

The driver connects to the WebDriver BiDi endpoint Firefox serves at `/session` on its remote debugging port. No session exists there yet, so the application creates one.

The driver has a 30-second command timeout, overriding the library's default, `BiDiDriver.DefaultCommandWaitTimeout` (60 seconds); adjust the value to suit your environment.

> **Tip:** By default, event handler exceptions and problems with messages from the browser never throw.
> They are reported only through the driver's diagnostic events, which nothing may be watching. During
> development, set the error behaviors to `TransportErrorBehavior.Terminate` so that problems surface as
> exceptions:
>
> ```csharp
> driver.TransportConfiguration.EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate;
> driver.TransportConfiguration.ProtocolErrorBehavior = TransportErrorBehavior.Terminate;
> driver.TransportConfiguration.UnknownMessageBehavior = TransportErrorBehavior.Terminate;
> driver.TransportConfiguration.UnexpectedErrorBehavior = TransportErrorBehavior.Terminate;
> ```
>
> See [Error Handling](advanced/error-handling.md) for a full explanation of the four error behavior
> properties and the recommended settings for production use.

### 2. Event Subscription

[!code-csharp[Event Subscription](../code/examples/FirstApplicationSamples.cs#EventSubscription)]

Sets up monitoring for browser console logs before they occur.

### 3. Getting the Context

[!code-csharp[Getting Context](../code/examples/FirstApplicationSamples.cs#GettingContext)]

Retrieves the current browsing contexts (tabs). We use the first one.

### 4. Navigation

[!code-csharp[Navigation](../code/examples/FirstApplicationSamples.cs#Navigation)]

Navigates to a URL and waits for the page to fully load.

### 5. JavaScript Execution

[!code-csharp[JavaScript Execution](../code/examples/FirstApplicationSamples.cs#JavaScriptExecution)]

Executes JavaScript and retrieves the result.

### 6. Screenshot Capture

[!code-csharp[Screenshot Capture](../code/examples/FirstApplicationSamples.cs#ScreenshotCapture)]

Captures a screenshot and saves it to disk.

## Using Chrome or Edge

Chrome and Edge serve WebDriver BiDi through their driver executables rather than on their own debugging port: run `chromedriver --port=9515` (or msedgedriver) and create a session with the `webSocketUrl` capability. [Browser Setup](browser-setup.md) walks through this. Then the application connects to the session's `webSocketUrl`:

[!code-csharp[Connecting to a Browser You Started](../code/examples/FirstApplicationSamples.cs#ManualConnection)]

The session already exists, so the application does not create one; the rest of it is unchanged. To download and launch browsers from code, see [Dramaturge.Browsers](https://www.nuget.org/packages/Dramaturge.Browsers).

## Common Issues and Solutions

### "Could not connect to remote WebSocket server"

**Problem**: Nothing is listening at the WebSocket URL, or the URL is wrong.
`StartAsync` retries the connection every 500 milliseconds until the startup timeout (10 seconds by default) runs
out, then throws `WebDriverBiDiTimeoutException`.

**Solution**: 
- Ensure Firefox is running with `--remote-debugging-port=9222`, or, for Chrome or Edge, that the driver is running and the session was created
- For a driver, verify it is listening by visiting `http://localhost:9515/status`
- Check that no firewall is blocking the port
- Make sure the URL is the `webSocketUrl` from the session response, not Chrome's `/devtools/browser/…` CDP URL

### "Timed out executing command"

**Problem**: The command took longer than the timeout period, so it failed with `WebDriverBiDiTimeoutException`.

**Solution**:
- Increase the timeout: `new BiDiDriver(TimeSpan.FromSeconds(60))`
- Check your network connection
- Ensure the target website is accessible

### "no such frame"

**Problem**: The command failed with a `WebDriverBiDiCommandException` whose `ErrorCode` is `NoSuchFrame`: the
browsing context ID is invalid or the tab was closed.

**Solution**:
- Always get fresh context IDs before using them
- Don't cache context IDs across navigations that might close tabs

## Next Steps

Now that you have a working application, explore these topics:

1. **[Core Concepts](core-concepts.md)** - Understand modules, commands, and events
2. **[Events and Observables](events-observables.md)** - Master event handling
3. **[Module Guides](modules/browsing-context.md)** - Learn about specific modules
4. **[Common Scenarios](examples/common-scenarios.md)** - See more examples

## Full Example Repository

Find complete examples and more advanced scenarios in the project's demo applications:
- `src/WebDriverBiDi.Demo/` - Various demonstration scenarios

## Exercises

Try these modifications to deepen your understanding:

1. **Multi-page navigation**: Navigate to 3 different websites and collect titles
2. **Form interaction**: Find a form online and fill it out programmatically
3. **Network monitoring**: Subscribe to network events and log all requests
4. **Multi-tab**: Open 3 tabs and navigate each to a different site
5. **Error handling**: Navigate to an invalid URL and handle the error gracefully

## Summary

You've learned how to:
- ✓ Set up a WebDriverBiDi.NET project
- ✓ Connect to a browser
- ✓ Navigate to websites
- ✓ Execute JavaScript
- ✓ Capture screenshots
- ✓ Handle errors gracefully

Continue exploring the documentation to unlock more powerful automation capabilities!

