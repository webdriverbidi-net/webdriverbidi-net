# Your First WebDriverBiDi Application

This tutorial walks you through creating a complete WebDriverBiDi.NET-Relaxed application from scratch.

## Prerequisites

- .NET SDK 6.0 or higher installed
- A browser with WebDriver BiDi support (Chrome, Edge, or Firefox)
- Basic knowledge of C# and async/await

## Step 1: Create the Project

Open a terminal and create a new console application:

```bash
mkdir MyFirstBiDiApp
cd MyFirstBiDiApp
dotnet new console
```

## Step 2: Add the NuGet Package

Add the WebDriverBiDi package:

```bash
dotnet add package WebDriverBiDi
```

## Step 3: Launch the Browser

Before running the application, launch Chrome with WebDriver BiDi enabled:

```bash
# Windows
chrome.exe --remote-debugging-port=9222 --user-data-dir=C:\temp\chrome-profile

# macOS
/Applications/Google\ Chrome.app/Contents/MacOS/Google\ Chrome \
  --remote-debugging-port=9222 \
  --user-data-dir=/tmp/chrome-profile

# Linux
google-chrome --remote-debugging-port=9222 --user-data-dir=/tmp/chrome-profile
```

## Step 4: Find the WebSocket URL

1. Open a new tab in the launched browser
2. Navigate to `http://localhost:9222/json/version`
3. Copy the `webSocketDebuggerUrl` value (it will look like `ws://localhost:9222/devtools/browser/...`)

## Step 5: Write the Application

Replace the contents of `Program.cs`:

```csharp
using System;
using System.Threading.Tasks;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;
using WebDriverBiDi.Log;
using WebDriverBiDi.Session;

namespace MyFirstBiDiApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Replace with your WebSocket URL from step 4
            string webSocketUrl = "ws://localhost:9222/devtools/browser/YOUR-ID-HERE";

            // Create driver with 30 second timeout
            BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

            try
            {
                Console.WriteLine("Connecting to browser...");
                await driver.StartAsync(webSocketUrl);
                Console.WriteLine("Connected!");

                // Set up console log monitoring
                driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
                {
                    Console.WriteLine($"[Browser Console] {e.Level}: {e.Text}");
                });

                SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
                subscribe.Events.Add(driver.Log.OnEntryAdded.EventName);
                await driver.Session.SubscribeAsync(subscribe);

                // Get the current browsing context
                Console.WriteLine("\nGetting browsing contexts...");
                GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                    new GetTreeCommandParameters());

                string contextId = tree.ContextTree[0].BrowsingContextId;
                Console.WriteLine($"Active context: {contextId}");

                // Navigate to a website
                Console.WriteLine("\nNavigating to example.com...");
                NavigateCommandParameters navParams = new NavigateCommandParameters(
                    contextId,
                    "https://example.com")
                {
                    Wait = ReadinessState.Complete
                };

                NavigateCommandResult navResult = await driver.BrowsingContext.NavigateAsync(navParams);
                Console.WriteLine($"Navigation complete! URL: {navResult.Url}");

                // Get the page title
                Console.WriteLine("\nGetting page title...");
                EvaluateCommandParameters evalParams = new EvaluateCommandParameters(
                    "document.title",
                    new ContextTarget(contextId),
                    true);

                EvaluateResult scriptResult = await driver.Script.EvaluateAsync(evalParams);

                if (scriptResult is EvaluateResultSuccess success)
                {
                    string title = success.Result.ValueAs<string>() ?? "No title";
                    Console.WriteLine($"Page title: {title}");
                }

                // Get page information
                Console.WriteLine("\nGetting page information...");
                string infoScript = @"
                {
                    url: window.location.href,
                    linkCount: document.querySelectorAll('a').length,
                    headingCount: document.querySelectorAll('h1, h2, h3, h4, h5, h6').length,
                    paragraphCount: document.querySelectorAll('p').length
                }";

                EvaluateCommandParameters infoParams = new EvaluateCommandParameters(
                    infoScript,
                    new ContextTarget(contextId),
                    true);

                EvaluateResult infoResult = await driver.Script.EvaluateAsync(infoParams);

                if (infoResult is EvaluateResultSuccess infoSuccess)
                {
                    var info = infoSuccess.Result.ValueAs<Dictionary<string, object>>();
                    Console.WriteLine("Page Analysis:");
                    Console.WriteLine($"  URL: {info["url"]}");
                    Console.WriteLine($"  Links: {info["linkCount"]}");
                    Console.WriteLine($"  Headings: {info["headingCount"]}");
                    Console.WriteLine($"  Paragraphs: {info["paragraphCount"]}");
                }

                // Take a screenshot
                Console.WriteLine("\nCapturing screenshot...");
                CaptureScreenshotCommandParameters screenshotParams =
                    new CaptureScreenshotCommandParameters(contextId);

                CaptureScreenshotCommandResult screenshot =
                    await driver.BrowsingContext.CaptureScreenshotAsync(screenshotParams);

                byte[] imageBytes = Convert.FromBase64String(screenshot.Data);
                await File.WriteAllBytesAsync("example-screenshot.png", imageBytes);
                Console.WriteLine("Screenshot saved to example-screenshot.png");

                Console.WriteLine("\n✓ All operations completed successfully!");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            catch (WebDriverBiDiException ex)
            {
                Console.WriteLine($"\n✗ WebDriver BiDi Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Unexpected Error: {ex.Message}");
            }
            finally
            {
                // Disconnect from the browser
                Console.WriteLine("\nDisconnecting from browser...");
                await driver.StopAsync();
                Console.WriteLine("Disconnected!");
            }
        }
    }
}
```

## Step 6: Run the Application

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

### 1. Driver Initialization

```csharp
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.StartAsync(webSocketUrl);
```

Creates a driver with a 30-second command timeout and connects to the browser.

### 2. Event Subscription

```csharp
driver.Log.OnEntryAdded.AddObserver((e) => { /* handle log */ });
await driver.Session.SubscribeAsync(subscribe);
```

Sets up monitoring for browser console logs before they occur.

### 3. Getting the Context

```csharp
GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
    new GetTreeCommandParameters());
string contextId = tree.ContextTree[0].BrowsingContextId;
```

Retrieves the current browsing contexts (tabs). We use the first one.

### 4. Navigation

```csharp
NavigateCommandParameters navParams = new NavigateCommandParameters(
    contextId,
    "https://example.com")
{
    Wait = ReadinessState.Complete  // Wait for full page load
};
await driver.BrowsingContext.NavigateAsync(navParams);
```

Navigates to a URL and waits for the page to fully load.

### 5. JavaScript Execution

```csharp
EvaluateCommandParameters evalParams = new EvaluateCommandParameters(
    "document.title",
    new ContextTarget(contextId),
    true);  // awaitPromise
EvaluateResult result = await driver.Script.EvaluateAsync(evalParams);
```

Executes JavaScript and retrieves the result.

### 6. Screenshot Capture

```csharp
CaptureScreenshotCommandResult screenshot =
    await driver.BrowsingContext.CaptureScreenshotAsync(screenshotParams);
byte[] imageBytes = Convert.FromBase64String(screenshot.Data);
await File.WriteAllBytesAsync("screenshot.png", imageBytes);
```

Captures a screenshot and saves it to disk.

## Common Issues and Solutions

### "Connection refused"

**Problem**: The browser isn't running or the WebSocket URL is wrong.

**Solution**: 
- Ensure the browser is launched with `--remote-debugging-port=9222`
- Verify the WebSocket URL by visiting `http://localhost:9222/json/version`
- Check that no firewall is blocking port 9222

### "Timeout waiting for command"

**Problem**: The command took longer than the timeout period.

**Solution**:
- Increase the timeout: `new BiDiDriver(TimeSpan.FromSeconds(60))`
- Check your network connection
- Ensure the target website is accessible

### "Context not found"

**Problem**: The browsing context ID is invalid or the tab was closed.

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
- `src/WebDriverBiDi.Client/` - Helper utilities for common tasks

## Exercises

Try these modifications to deepen your understanding:

1. **Multi-page navigation**: Navigate to 3 different websites and collect titles
2. **Form interaction**: Find a form online and fill it out programmatically
3. **Network monitoring**: Subscribe to network events and log all requests
4. **Multi-tab**: Open 3 tabs and navigate each to a different site
5. **Error handling**: Navigate to an invalid URL and handle the error gracefully

## Summary

You've learned how to:
- ✓ Set up a WebDriverBiDi.NET-Relaxed project
- ✓ Connect to a browser
- ✓ Navigate to websites
- ✓ Execute JavaScript
- ✓ Capture screenshots
- ✓ Handle errors gracefully

Continue exploring the documentation to unlock more powerful automation capabilities!

