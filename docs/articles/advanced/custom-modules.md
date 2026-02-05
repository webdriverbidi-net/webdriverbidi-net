# Custom Modules

This guide explains how to create custom modules to extend WebDriverBiDi.NET-Relaxed with your own commands and functionality.

## Overview

WebDriverBiDi.NET-Relaxed's module system is extensible, allowing you to:
- Implement custom WebDriver BiDi commands
- Create higher-level abstractions over protocol commands
- Integrate experimental or browser-specific features
- Build reusable automation patterns

## Module Basics

### Module Structure

All modules inherit from the `Module` base class:

```csharp
using WebDriverBiDi;

public class MyCustomModule : Module
{
    public MyCustomModule(BiDiDriver driver) 
        : base(driver, "myCustom")
    {
    }
}
```

### Registering a Module

```csharp
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.StartAsync(webSocketUrl);

// Register custom module
MyCustomModule customModule = new MyCustomModule(driver);
driver.RegisterModule(customModule);

// Access module
var myModule = driver.GetModule<MyCustomModule>("myCustom");
```

## Creating Commands

### Command Parameters

Define parameters that extend `CommandParameters`:

```csharp
using WebDriverBiDi;

public class MyCommandParameters : CommandParameters<MyCommandResult>
{
    public MyCommandParameters(string contextId, string value)
    {
        this.MethodName = "myCustom.myCommand";
        this.ContextId = contextId;
        this.Value = value;
    }

    [JsonPropertyName("context")]
    public string ContextId { get; }

    [JsonPropertyName("value")]
    public string Value { get; }
}
```

### Command Results

Define results that extend `CommandResult`:

```csharp
using System.Text.Json.Serialization;
using WebDriverBiDi;

public class MyCommandResult : CommandResult
{
    [JsonPropertyName("success")]
    public bool Success { get; private set; }

    [JsonPropertyName("data")]
    public string Data { get; private set; } = string.Empty;
}
```

### Command Method

Implement the command in your module:

```csharp
public class MyCustomModule : Module
{
    public MyCustomModule(BiDiDriver driver) 
        : base(driver, "myCustom")
    {
    }

    public async Task<MyCommandResult> MyCommandAsync(MyCommandParameters parameters)
    {
        return await this.Driver.ExecuteCommandAsync<MyCommandResult>(parameters);
    }
}
```

## Example: Page Utilities Module

Let's create a complete custom module for common page operations:

```csharp
using System.Text.Json.Serialization;
using WebDriverBiDi;
using WebDriverBiDi.Script;

namespace MyAutomation
{
    // Command Parameters
    public class WaitForElementCommandParameters : CommandParameters<WaitForElementCommandResult>
    {
        public WaitForElementCommandParameters(string contextId, string selector, int timeoutMs)
        {
            this.MethodName = "script.evaluate";  // Use existing protocol command
            this.ContextId = contextId;
            this.Selector = selector;
            this.TimeoutMs = timeoutMs;
            
            // Build JavaScript that waits for element
            this.Expression = $@"
                new Promise((resolve) => {{
                    const checkElement = () => {{
                        const element = document.querySelector('{selector}');
                        if (element) {{
                            resolve({{ found: true, tagName: element.tagName }});
                        }} else {{
                            setTimeout(checkElement, 100);
                        }}
                    }};
                    checkElement();
                    setTimeout(() => resolve({{ found: false }}), {timeoutMs});
                }})";
            
            this.Target = new ContextTarget(contextId);
            this.AwaitPromise = true;
        }

        [JsonPropertyName("expression")]
        public string Expression { get; }

        [JsonPropertyName("target")]
        public ContextTarget Target { get; }

        [JsonPropertyName("awaitPromise")]
        public bool AwaitPromise { get; }

        [JsonIgnore]
        public string ContextId { get; }

        [JsonIgnore]
        public string Selector { get; }

        [JsonIgnore]
        public int TimeoutMs { get; }
    }

    // Command Result (uses standard EvaluateResult)
    public class WaitForElementCommandResult : CommandResult
    {
        [JsonPropertyName("result")]
        public RemoteValue? Result { get; private set; }
    }

    // Custom Module
    public class PageUtilitiesModule : Module
    {
        public const string PageUtilitiesModuleName = "pageUtilities";

        public PageUtilitiesModule(BiDiDriver driver) 
            : base(driver, PageUtilitiesModuleName)
        {
        }

        public async Task<bool> WaitForElementAsync(
            string contextId, 
            string selector, 
            TimeSpan timeout)
        {
            WaitForElementCommandParameters parameters = 
                new WaitForElementCommandParameters(
                    contextId, 
                    selector, 
                    (int)timeout.TotalMilliseconds);

            EvaluateResult result = await this.Driver.ExecuteCommandAsync<EvaluateResult>(
                parameters);

            if (result is EvaluateResultSuccess success)
            {
                var data = success.Result.ValueAs<Dictionary<string, object>>();
                return (bool)data["found"];
            }

            return false;
        }

        public async Task<string?> GetElementTextAsync(string contextId, string selector)
        {
            EvaluateCommandParameters parameters = new EvaluateCommandParameters(
                $"document.querySelector('{selector}')?.textContent",
                new ContextTarget(contextId),
                true);

            EvaluateResult result = await this.Driver.Script.EvaluateAsync(parameters);

            if (result is EvaluateResultSuccess success)
            {
                return success.Result.ValueAs<string>();
            }

            return null;
        }

        public async Task<bool> IsElementVisibleAsync(string contextId, string selector)
        {
            string script = $@"
                (() => {{
                    const element = document.querySelector('{selector}');
                    if (!element) return false;
                    const style = window.getComputedStyle(element);
                    return style.display !== 'none' && 
                           style.visibility !== 'hidden' && 
                           element.offsetParent !== null;
                }})()";

            EvaluateCommandParameters parameters = new EvaluateCommandParameters(
                script,
                new ContextTarget(contextId),
                true);

            EvaluateResult result = await this.Driver.Script.EvaluateAsync(parameters);

            if (result is EvaluateResultSuccess success)
            {
                return success.Result.ValueAs<bool>();
            }

            return false;
        }

        public async Task<Dictionary<string, object>> GetPageMetricsAsync(string contextId)
        {
            string script = @"
            ({
                title: document.title,
                url: window.location.href,
                linkCount: document.querySelectorAll('a').length,
                imageCount: document.querySelectorAll('img').length,
                scriptCount: document.querySelectorAll('script').length,
                styleCount: document.querySelectorAll('link[rel=""stylesheet""]').length,
                readyState: document.readyState,
                height: document.documentElement.scrollHeight,
                width: document.documentElement.scrollWidth
            })";

            EvaluateCommandParameters parameters = new EvaluateCommandParameters(
                script,
                new ContextTarget(contextId),
                true);

            EvaluateResult result = await this.Driver.Script.EvaluateAsync(parameters);

            if (result is EvaluateResultSuccess success)
            {
                return success.Result.ValueAs<Dictionary<string, object>>();
            }

            return new Dictionary<string, object>();
        }
    }
}
```

### Using the Custom Module

```csharp
using MyAutomation;

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.StartAsync(webSocketUrl);

// Register custom module
PageUtilitiesModule pageUtils = new PageUtilitiesModule(driver);
driver.RegisterModule(pageUtils);

// Get context
GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new());
string contextId = tree.ContextTree[0].BrowsingContextId;

// Navigate
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com")
    { Wait = ReadinessState.Complete });

// Use custom module
bool elementFound = await pageUtils.WaitForElementAsync(
    contextId, 
    ".content", 
    TimeSpan.FromSeconds(10));

if (elementFound)
{
    string? text = await pageUtils.GetElementTextAsync(contextId, ".content");
    Console.WriteLine($"Content: {text}");
    
    bool isVisible = await pageUtils.IsElementVisibleAsync(contextId, ".content");
    Console.WriteLine($"Visible: {isVisible}");
}

// Get page metrics
var metrics = await pageUtils.GetPageMetricsAsync(contextId);
Console.WriteLine($"Links: {metrics["linkCount"]}");
Console.WriteLine($"Images: {metrics["imageCount"]}");
```

## Example: Testing Utilities Module

Create a module with common testing helpers:

```csharp
public class TestUtilitiesModule : Module
{
    public const string TestUtilitiesModuleName = "testUtilities";

    public TestUtilitiesModule(BiDiDriver driver) 
        : base(driver, TestUtilitiesModuleName)
    {
    }

    public async Task<byte[]> TakeFullPageScreenshotAsync(string contextId)
    {
        // Get page dimensions
        string script = @"
        ({
            width: Math.max(
                document.documentElement.scrollWidth,
                document.body.scrollWidth
            ),
            height: Math.max(
                document.documentElement.scrollHeight,
                document.body.scrollHeight
            )
        })";

        EvaluateResult result = await this.Driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(script, new ContextTarget(contextId), true));

        if (result is EvaluateResultSuccess success)
        {
            var dimensions = success.Result.ValueAs<Dictionary<string, object>>();
            long width = Convert.ToInt64(dimensions["width"]);
            long height = Convert.ToInt64(dimensions["height"]);

            // Capture screenshot with full page dimensions
            CaptureScreenshotCommandParameters screenshotParams = 
                new CaptureScreenshotCommandParameters(contextId)
                {
                    Clip = new BoxClipRectangle
                    {
                        X = 0,
                        Y = 0,
                        Width = width,
                        Height = height
                    }
                };

            CaptureScreenshotCommandResult screenshot = 
                await this.Driver.BrowsingContext.CaptureScreenshotAsync(screenshotParams);

            return Convert.FromBase64String(screenshot.Data);
        }

        throw new Exception("Failed to get page dimensions");
    }

    public async Task<List<string>> GetAllLinksAsync(string contextId)
    {
        string script = "Array.from(document.querySelectorAll('a')).map(a => a.href)";

        EvaluateResult result = await this.Driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(script, new ContextTarget(contextId), true));

        if (result is EvaluateResultSuccess success)
        {
            var links = success.Result.ValueAs<List<object>>();
            return links.Select(l => l.ToString()).ToList();
        }

        return new List<string>();
    }

    public async Task HighlightElementAsync(string contextId, string selector)
    {
        string script = $@"
            (() => {{
                const element = document.querySelector('{selector}');
                if (element) {{
                    element.style.border = '3px solid red';
                    element.style.backgroundColor = 'yellow';
                    return true;
                }}
                return false;
            }})()";

        await this.Driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(script, new ContextTarget(contextId), false));
    }

    public async Task InjectCSSAsync(string contextId, string css)
    {
        string script = $@"
            (() => {{
                const style = document.createElement('style');
                style.textContent = `{css}`;
                document.head.appendChild(style);
            }})()";

        await this.Driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(script, new ContextTarget(contextId), false));
    }
}
```

## Example: Performance Monitoring Module

```csharp
public class PerformanceModule : Module
{
    public const string PerformanceModuleName = "performance";

    public PerformanceModule(BiDiDriver driver) 
        : base(driver, PerformanceModuleName)
    {
    }

    public async Task<Dictionary<string, double>> GetNavigationTimingAsync(string contextId)
    {
        string script = @"
        (() => {
            const timing = performance.getEntriesByType('navigation')[0];
            if (!timing) return {};
            
            return {
                dnsLookup: timing.domainLookupEnd - timing.domainLookupStart,
                tcpConnection: timing.connectEnd - timing.connectStart,
                requestTime: timing.responseStart - timing.requestStart,
                responseTime: timing.responseEnd - timing.responseStart,
                domParsing: timing.domInteractive - timing.responseEnd,
                domContentLoaded: timing.domContentLoadedEventEnd - timing.domContentLoadedEventStart,
                loadComplete: timing.loadEventEnd - timing.loadEventStart,
                totalTime: timing.loadEventEnd - timing.fetchStart
            };
        })()";

        EvaluateResult result = await this.Driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(script, new ContextTarget(contextId), true));

        if (result is EvaluateResultSuccess success)
        {
            var timing = success.Result.ValueAs<Dictionary<string, object>>();
            return timing.ToDictionary(
                kvp => kvp.Key,
                kvp => Convert.ToDouble(kvp.Value));
        }

        return new Dictionary<string, double>();
    }

    public async Task<List<ResourceTiming>> GetResourceTimingsAsync(string contextId)
    {
        string script = @"
        Array.from(performance.getEntriesByType('resource')).map(r => ({
            name: r.name,
            duration: r.duration,
            size: r.transferSize || 0,
            type: r.initiatorType
        }))";

        EvaluateResult result = await this.Driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(script, new ContextTarget(contextId), true));

        if (result is EvaluateResultSuccess success)
        {
            var resources = success.Result.ValueAs<List<object>>();
            return resources.Select(r =>
            {
                var dict = (Dictionary<string, object>)r;
                return new ResourceTiming
                {
                    Name = dict["name"].ToString(),
                    Duration = Convert.ToDouble(dict["duration"]),
                    Size = Convert.ToInt64(dict["size"]),
                    Type = dict["type"].ToString()
                };
            }).ToList();
        }

        return new List<ResourceTiming>();
    }
}

public class ResourceTiming
{
    public string Name { get; set; } = string.Empty;
    public double Duration { get; set; }
    public long Size { get; set; }
    public string Type { get; set; } = string.Empty;
}
```

## Module Events

You can also expose observable events from your custom module:

```csharp
public class CustomEventsModule : Module
{
    private const string CustomEventName = "custom.eventOccurred";

    public CustomEventsModule(BiDiDriver driver) 
        : base(driver, "customEvents")
    {
        // Register event with driver
        driver.RegisterEvent<CustomEventArgs>(CustomEventName);
    }

    public ObservableEvent<CustomEventArgs> OnCustomEvent { get; } = 
        new ObservableEvent<CustomEventArgs>(CustomEventName);
}

public class CustomEventArgs : WebDriverBiDiEventArgs
{
    [JsonPropertyName("data")]
    public string Data { get; private set; } = string.Empty;
}
```

## Best Practices

### 1. Namespace Your Commands

Use a clear module prefix for your custom commands:

```csharp
public MyCommandParameters(string value)
{
    this.MethodName = "myCompany.myModule.myCommand";  // Clear namespace
    this.Value = value;
}
```

### 2. Provide Defaults

Make your modules easy to use with sensible defaults:

```csharp
public async Task<bool> WaitForElementAsync(
    string contextId, 
    string selector, 
    TimeSpan? timeout = null)  // Optional timeout
{
    timeout = timeout ?? TimeSpan.FromSeconds(30);  // Default
    // ...
}
```

### 3. Document Your Module

```csharp
/// <summary>
/// Provides utility methods for common page operations.
/// </summary>
public class PageUtilitiesModule : Module
{
    /// <summary>
    /// Waits for an element to appear on the page.
    /// </summary>
    /// <param name="contextId">The browsing context ID.</param>
    /// <param name="selector">CSS selector for the element.</param>
    /// <param name="timeout">Maximum time to wait.</param>
    /// <returns>True if element found, false otherwise.</returns>
    public async Task<bool> WaitForElementAsync(
        string contextId, 
        string selector, 
        TimeSpan timeout)
    {
        // ...
    }
}
```

### 4. Handle Errors Gracefully

```csharp
public async Task<string?> GetElementTextAsync(string contextId, string selector)
{
    try
    {
        EvaluateResult result = await this.Driver.Script.EvaluateAsync(parameters);
        
        if (result is EvaluateResultSuccess success)
        {
            return success.Result.ValueAs<string>();
        }
        else if (result is EvaluateResultException exception)
        {
            Console.WriteLine($"Script error: {exception.ExceptionDetails.Text}");
        }
    }
    catch (WebDriverBiDiException ex)
    {
        Console.WriteLine($"Command error: {ex.Message}");
    }
    
    return null;  // Graceful fallback
}
```

### 5. Make Modules Testable

```csharp
public interface IPageUtilities
{
    Task<bool> WaitForElementAsync(string contextId, string selector, TimeSpan timeout);
    Task<string?> GetElementTextAsync(string contextId, string selector);
}

public class PageUtilitiesModule : Module, IPageUtilities
{
    // Implementation...
}

// Now you can mock in tests
public class MockPageUtilities : IPageUtilities
{
    // Mock implementation
}
```

## Packaging Custom Modules

Create a NuGet package for reusable modules:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackageId>MyCompany.WebDriverBiDi.Extensions</PackageId>
    <Version>1.0.0</Version>
    <Authors>Your Name</Authors>
    <Description>Custom WebDriver BiDi modules</Description>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="WebDriverBiDi" Version="*" />
  </ItemGroup>
</Project>
```

## Advanced: Implementing Protocol Extensions

For actual protocol extensions (not just helper methods):

```csharp
// Define new command in protocol
public class ExperimentalCommandParameters : CommandParameters<ExperimentalCommandResult>
{
    public ExperimentalCommandParameters(string parameter)
    {
        // Use actual protocol command name
        this.MethodName = "experimental.newCommand";
        this.Parameter = parameter;
    }

    [JsonPropertyName("parameter")]
    public string Parameter { get; }
}

public class ExperimentalCommandResult : CommandResult
{
    [JsonPropertyName("result")]
    public string Result { get; private set; } = string.Empty;
}

// Implement in module
public class ExperimentalModule : Module
{
    public ExperimentalModule(BiDiDriver driver) 
        : base(driver, "experimental")
    {
    }

    public async Task<ExperimentalCommandResult> NewCommandAsync(
        ExperimentalCommandParameters parameters)
    {
        return await this.Driver.ExecuteCommandAsync<ExperimentalCommandResult>(
            parameters);
    }
}
```

## Next Steps

- [Architecture](../architecture.md): Understand the module system
- [Core Concepts](../core-concepts.md): Learn about commands and events
- [Error Handling](error-handling.md): Implement robust error handling
- [Examples](../examples/common-scenarios.md): See modules in action

## Summary

Custom modules allow you to:
- Extend WebDriverBiDi.NET-Relaxed with reusable functionality
- Create domain-specific abstractions
- Implement experimental features
- Build shareable automation libraries

The module system is flexible and powerful, enabling you to build exactly the automation framework you need.

