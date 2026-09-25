# Your First WebDriverBiDi Application

This tutorial walks you through creating a complete WebDriverBiDi.NET application from scratch.

## Prerequisites

- .NET SDK 8.0 or higher installed, to build and run the console application this tutorial walks through. The library itself needs only a runtime compatible with .NET Standard 2.0
- A network connection the first time the application runs, when Chrome is downloaded (see [Using a Browser You Start Yourself](#using-a-browser-you-start-yourself) to use a browser you already have instead)
- Basic knowledge of C# and async/await

## Step 1: Create the Project

Open a terminal and create a new console application:

```bash
mkdir MyFirstBiDiApp
cd MyFirstBiDiApp
dotnet new console
```

## Step 2: Add the NuGet Packages

Add the WebDriverBiDi package, the protocol client, and the WebDriverBiDi.Browsers package, which downloads and launches the browser:

```bash
dotnet add package WebDriverBiDi
dotnet add package WebDriverBiDi.Browsers
```

## Step 3: Write the Application

Replace the contents of `Program.cs` with the code below, adding these `using` directives at the top of the file:

<!-- inline-csharp: the using directives the sample needs, quoted on their own -->
```csharp
using WebDriverBiDi;
using WebDriverBiDi.Browsers;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Log;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
```

[!code-csharp[Full First Application](../code/examples/FirstApplicationSamples.cs#FullFirstApplication)]

## Step 4: Run the Application

```bash
dotnet run
```

The first run downloads Chrome for Testing into a cache in your user profile, which takes a little while; later runs use the cached copy. You should see output similar to:

```
Launching Chrome...
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

### 1. Launching and Connecting

[!code-csharp[Launching and Connecting](../code/examples/FirstApplicationSamples.cs#DriverInitialization)]

`BrowserLauncher` finds Chrome, downloading it the first time, and launches it with a temporary profile. `CreateTransport()` gives the driver a transport that reaches Chrome, which speaks WebDriver BiDi through a mapper over its DevTools endpoint, so the driver then creates the session itself. A browser launched through its driver executable (`LaunchUsingDriver()`) already has a session, which `IsBiDiSessionInitialized` reports. Disposing the launcher closes the browser and deletes the profile. `BrowserLauncher.Configure` also takes Firefox, Edge, or Safari; see [Browser Setup](browser-setup.md#using-webdriverbidibrowsers) for the channels, versions, and launch options.

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

## Using a Browser You Start Yourself

WebDriverBiDi.Browsers is optional. The WebDriverBiDi package connects to any WebSocket URL that speaks WebDriver BiDi, so you can start the browser, or its driver, yourself: for example, run `chromedriver --port=9515` and create a session with the `webSocketUrl` capability. [Setting Up a Browser Manually](browser-setup.md#setting-up-a-browser-manually) walks through this for Chrome, Edge, and Firefox. Then, in place of the launcher, the application connects to the session's `webSocketUrl`:

[!code-csharp[Connecting to a Browser You Started](../code/examples/FirstApplicationSamples.cs#ManualConnection)]

The rest of the application is unchanged, and you remove only the launcher's lines and the `WebDriverBiDi.Browsers` package.

## Common Issues and Solutions

### The Browser Cannot Be Downloaded or Launched

**Problem**: `LaunchAsync` throws `BrowserDownloadException` when Chrome cannot be located or downloaded, for example
because the machine is offline and nothing is cached, or `BrowserLaunchException` when it cannot be started.

**Solution**:
- Check the network connection, or any proxy between you and `storage.googleapis.com`, for the first run
- For `BrowserLaunchException`, its `ExitCode` and `ProcessOutput` show what the browser reported
- See [Downloads and the Cache](browser-setup.md#downloads-and-the-cache) to download from a mirror, or to use a browser installed on the machine with `AtDefaultInstallationLocation()`

### "Could not connect to remote WebSocket server"

**Problem**: When you start the browser yourself, nothing is listening at the WebSocket URL, or the URL is wrong.
`StartAsync` retries the connection every 500 milliseconds until the startup timeout (10 seconds by default) runs
out, then throws `WebDriverBiDiTimeoutException`.

**Solution**: 
- Ensure chromedriver is running (`chromedriver --port=9515`) and the session was created
- Verify the driver is listening by visiting `http://localhost:9515/status`
- Check that no firewall is blocking port 9515
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
- ✓ Launch a browser and connect to it
- ✓ Navigate to websites
- ✓ Execute JavaScript
- ✓ Capture screenshots
- ✓ Handle errors gracefully

Continue exploring the documentation to unlock more powerful automation capabilities!

