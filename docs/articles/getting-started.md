# Getting Started with WebDriverBiDi.NET-Relaxed

This guide will walk you through installing WebDriverBiDi.NET-Relaxed and setting up your first browser automation project.

## Prerequisites

- **.NET SDK**: .NET Framework 4.6.1+, .NET Core 2.0+, or .NET 5.0+
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

## Browser Setup

WebDriverBiDi.NET-Relaxed requires a browser with WebDriver BiDi support running and listening on a WebSocket endpoint.

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

The WebSocket URL will typically be: `ws://localhost:9222/session`

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

For Chromium-based browsers, navigate to `http://localhost:9222/json/version` to find the `webSocketDebuggerUrl`.

## Creating Your First Application

### 1. Create a New Console Application

```bash
dotnet new console -n MyFirstBiDiApp
cd MyFirstBiDiApp
dotnet add package WebDriverBiDi
```

### 2. Write the Code

Replace the contents of `Program.cs`:

```csharp
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

// Set the WebSocket URL for your browser
string webSocketUrl = "ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID";

// Create a driver with a 10-second command timeout
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));

try
{
    // Connect to the browser
    Console.WriteLine("Connecting to browser...");
    await driver.StartAsync(webSocketUrl);
    Console.WriteLine("Connected!");

    // Get the current browsing contexts (tabs/windows)
    GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
        new GetTreeCommandParameters());
    
    string contextId = tree.ContextTree[0].BrowsingContextId;
    Console.WriteLine($"Active context ID: {contextId}");

    // Navigate to a webpage
    Console.WriteLine("Navigating to example.com...");
    NavigateCommandParameters navParams = new NavigateCommandParameters(
        contextId, 
        "https://example.com")
    {
        Wait = ReadinessState.Complete
    };
    
    NavigateCommandResult navResult = await driver.BrowsingContext.NavigateAsync(navParams);
    Console.WriteLine($"Navigation complete! URL: {navResult.Url}");

    // Execute JavaScript to get the page title
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

    Console.WriteLine("Press any key to close...");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
finally
{
    // Disconnect from the browser
    await driver.StopAsync();
    Console.WriteLine("Disconnected from browser");
}
```

### 3. Run the Application

```bash
dotnet run
```

## Understanding the Code

Let's break down what this code does:

### Creating the Driver

```csharp
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
```

The `BiDiDriver` is the main entry point for all WebDriver BiDi operations. The timeout parameter specifies how long to wait for command responses.

### Connecting to the Browser

```csharp
await driver.StartAsync(webSocketUrl);
```

This establishes a WebSocket connection to the browser. The browser must already be running with WebDriver BiDi enabled.

### Getting the Browsing Context

```csharp
GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
    new GetTreeCommandParameters());
string contextId = tree.ContextTree[0].BrowsingContextId;
```

A browsing context represents a tab, window, or iframe. You need the context ID to perform operations like navigation or script execution.

### Navigating

```csharp
NavigateCommandParameters navParams = new NavigateCommandParameters(
    contextId, 
    "https://example.com")
{
    Wait = ReadinessState.Complete
};
await driver.BrowsingContext.NavigateAsync(navParams);
```

The `Wait` property controls when the command returns:
- `ReadinessState.None`: Returns immediately after navigation starts
- `ReadinessState.Interactive`: Waits for DOM ready
- `ReadinessState.Complete`: Waits for page load complete (including images, stylesheets)

### Executing JavaScript

```csharp
EvaluateCommandParameters evalParams = new EvaluateCommandParameters(
    "document.title",
    new ContextTarget(contextId),
    true);
    
EvaluateResult scriptResult = await driver.Script.EvaluateAsync(evalParams);
```

The third parameter (`true`) indicates whether to await promises in the JavaScript code.

## Next Steps

Now that you have a working WebDriverBiDi.NET-Relaxed application, explore these topics:

1. **[Core Concepts](core-concepts.md)**: Understand modules, commands, and events
2. **[Browser Module](modules/browser.md)**: Learn about browser-level operations
3. **[Events and Observables](events-observables.md)**: Handle browser events asynchronously
4. **[Common Scenarios](examples/common-scenarios.md)**: See practical examples

## Troubleshooting

### "Connection refused" Error

- Ensure the browser is running with remote debugging enabled
- Verify the port number matches your browser's configuration
- Check that no firewall is blocking the connection

### "Timeout waiting for command" Error

- Increase the timeout when creating the `BiDiDriver`
- Check that the browser is responsive
- Ensure the command parameters are valid

### "Module not found" Error

- Verify that your browser supports the specific module
- Some modules are experimental and may require specific browser flags

## Additional Resources

- [WebDriver BiDi Specification](https://w3c.github.io/webdriver-bidi/)
- [GitHub Repository](https://github.com/hardkoded/webdriverbidi-net-relaxed)
- [NuGet Package](https://www.nuget.org/packages/WebDriverBiDi-Relaxed)

