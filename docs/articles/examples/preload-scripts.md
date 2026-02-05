# Preload Scripts Example

This example demonstrates how to use preload scripts to inject JavaScript into pages before any other scripts execute using WebDriverBiDi.NET-Relaxed.

## Overview

Preload scripts allow you to:
- Inject utilities available on every page
- Monitor page behavior before it starts
- Modify or intercept page functionality
- Wait for specific page conditions
- Communicate with your test code via channels

## Example 1: Basic Preload Script

```csharp
using System;
using System.Threading.Tasks;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

namespace PreloadScriptsExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string webSocketUrl = "ws://localhost:9222/devtools/browser/YOUR-ID-HERE";
            BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

            try
            {
                await driver.StartAsync(webSocketUrl);
                Console.WriteLine("Connected to browser");

                GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                    new GetTreeCommandParameters());
                string contextId = tree.ContextTree[0].BrowsingContextId;

                // Add a preload script that creates a utility object
                Console.WriteLine("Adding preload script...");
                string preloadScript = @"
                () => {
                    window.myUtils = {
                        getElementText: (selector) => {
                            const element = document.querySelector(selector);
                            return element ? element.textContent : null;
                        },
                        clickElement: (selector) => {
                            const element = document.querySelector(selector);
                            if (element) {
                                element.click();
                                return true;
                            }
                            return false;
                        }
                    };
                    console.log('Preload script: utilities injected');
                }";

                AddPreloadScriptCommandParameters preloadParams = 
                    new AddPreloadScriptCommandParameters(preloadScript);

                AddPreloadScriptCommandResult preloadResult = 
                    await driver.Script.AddPreloadScriptAsync(preloadParams);
                
                Console.WriteLine($"Preload script added: {preloadResult.PreloadScriptId}");

                // Navigate - the preload script will run before page scripts
                Console.WriteLine("\nNavigating to example.com...");
                await driver.BrowsingContext.NavigateAsync(
                    new NavigateCommandParameters(contextId, "https://example.com")
                    { Wait = ReadinessState.Complete });

                // Use the injected utilities
                Console.WriteLine("\nUsing preload script utilities...");
                
                EvaluateCommandParameters evalParams = new EvaluateCommandParameters(
                    "window.myUtils.getElementText('h1')",
                    new ContextTarget(contextId),
                    true);

                EvaluateResult result = await driver.Script.EvaluateAsync(evalParams);
                
                if (result is EvaluateResultSuccess success)
                {
                    string? text = success.Result.ValueAs<string>();
                    Console.WriteLine($"Page heading: {text}");
                }

                // Clean up
                await driver.Script.RemovePreloadScriptAsync(
                    new RemovePreloadScriptCommandParameters(preloadResult.PreloadScriptId));

                Console.WriteLine("\n✓ Preload script example complete");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                await driver.StopAsync();
            }
        }
    }
}
```

## Example 2: Preload Script with Channel Communication

```csharp
// Subscribe to script messages
SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.Script.OnMessage.EventName);
await driver.Session.SubscribeAsync(subscribe);

// Set up message handler
TaskCompletionSource<string> pageLoadedSignal = new TaskCompletionSource<string>();

driver.Script.OnMessage.AddObserver((MessageEventArgs e) =>
{
    if (e.ChannelId == "pageLoadChannel")
    {
        Console.WriteLine($"📨 Received message from preload script");
        
        if (e.Data.Type == "object")
        {
            var data = e.Data.ValueAs<Dictionary<string, object>>();
            Console.WriteLine($"Page ready: {data["ready"]}");
            Console.WriteLine($"Load time: {data["loadTime"]}ms");
            
            pageLoadedSignal.SetResult("complete");
        }
    }
});

// Add preload script with channel
string preloadScript = @"
(channel) => {
    const startTime = Date.now();
    
    window.addEventListener('load', () => {
        const loadTime = Date.now() - startTime;
        channel({
            ready: true,
            loadTime: loadTime,
            url: window.location.href
        });
    });
}";

ChannelValue channel = new ChannelValue(new ChannelProperties("pageLoadChannel"));

AddPreloadScriptCommandParameters preloadParams = 
    new AddPreloadScriptCommandParameters(preloadScript);
preloadParams.Arguments.Add(channel);

AddPreloadScriptCommandResult preloadResult = 
    await driver.Script.AddPreloadScriptAsync(preloadParams);

Console.WriteLine("Preload script with channel added");

// Navigate
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com")
    { Wait = ReadinessState.Complete });

// Wait for signal from preload script
await pageLoadedSignal.Task;
Console.WriteLine("✅ Page load detected by preload script");
```

## Example 3: Wait for Element with Preload Script

```csharp
// This preload script waits for a specific element to appear
string waitForElementScript = @"
(channel) => {
    const checkForElement = () => {
        const element = document.querySelector('.dynamic-content');
        if (element) {
            channel({
                found: true,
                text: element.textContent,
                timestamp: Date.now()
            });
        } else {
            // Check again in 100ms
            setTimeout(checkForElement, 100);
        }
    };
    
    // Start checking when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', checkForElement);
    } else {
        checkForElement();
    }
}";

TaskCompletionSource<Dictionary<string, object>> elementFoundSignal = 
    new TaskCompletionSource<Dictionary<string, object>>();

driver.Script.OnMessage.AddObserver((MessageEventArgs e) =>
{
    if (e.ChannelId == "elementWatcher")
    {
        var data = e.Data.ValueAs<Dictionary<string, object>>();
        elementFoundSignal.SetResult(data);
    }
});

ChannelValue channel = new ChannelValue(new ChannelProperties("elementWatcher"));

AddPreloadScriptCommandParameters preloadParams = 
    new AddPreloadScriptCommandParameters(waitForElementScript);
preloadParams.Arguments.Add(channel);

await driver.Script.AddPreloadScriptAsync(preloadParams);

// Navigate to page with dynamic content
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com/dynamic")
    { Wait = ReadinessState.Complete });

// Wait for element (with timeout)
Task<Dictionary<string, object>> elementTask = elementFoundSignal.Task;
Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));

if (await Task.WhenAny(elementTask, timeoutTask) == elementTask)
{
    var data = await elementTask;
    Console.WriteLine($"✅ Element found: {data["text"]}");
}
else
{
    Console.WriteLine("❌ Timeout waiting for element");
}
```

## Example 4: Sandbox Isolation

```csharp
// Add preload script in a sandbox to avoid polluting page's global scope
string preloadScript = @"
() => {
    window.testUtils = {
        getPageInfo: () => ({
            title: document.title,
            url: window.location.href,
            linkCount: document.querySelectorAll('a').length
        })
    };
}";

AddPreloadScriptCommandParameters preloadParams = 
    new AddPreloadScriptCommandParameters(preloadScript)
    {
        Sandbox = "testUtilsSandbox"  // Isolated from page
    };

AddPreloadScriptCommandResult preloadResult = 
    await driver.Script.AddPreloadScriptAsync(preloadParams);

Console.WriteLine("Sandboxed preload script added");

// Navigate
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com")
    { Wait = ReadinessState.Complete });

// Access from the same sandbox
ContextTarget target = new ContextTarget(contextId)
{
    Sandbox = "testUtilsSandbox"
};

EvaluateCommandParameters evalParams = new EvaluateCommandParameters(
    "window.testUtils.getPageInfo()",
    target,
    true);

EvaluateResult result = await driver.Script.EvaluateAsync(evalParams);

if (result is EvaluateResultSuccess success)
{
    var info = success.Result.ValueAs<Dictionary<string, object>>();
    Console.WriteLine($"Title: {info["title"]}");
    Console.WriteLine($"URL: {info["url"]}");
    Console.WriteLine($"Links: {info["linkCount"]}");
}

// Trying to access from default context will fail
ContextTarget defaultTarget = new ContextTarget(contextId);
EvaluateResult failResult = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "typeof window.testUtils",
        defaultTarget,
        true));

if (failResult is EvaluateResultSuccess failSuccess)
{
    string type = failSuccess.Result.ValueAs<string>();
    Console.WriteLine($"testUtils in default context: {type}");  // "undefined"
}
```

## Example 5: Intercept Page Behavior

```csharp
// Preload script that intercepts fetch calls
string interceptFetchScript = @"
(channel) => {
    const originalFetch = window.fetch;
    
    window.fetch = async function(...args) {
        const url = args[0];
        channel({
            type: 'fetch',
            url: url,
            timestamp: Date.now()
        });
        
        return originalFetch.apply(this, args);
    };
    
    console.log('Fetch interceptor installed');
}";

List<Dictionary<string, object>> fetchCalls = new List<Dictionary<string, object>>();

driver.Script.OnMessage.AddObserver((MessageEventArgs e) =>
{
    if (e.ChannelId == "fetchInterceptor")
    {
        var data = e.Data.ValueAs<Dictionary<string, object>>();
        fetchCalls.Add(data);
        Console.WriteLine($"🌐 Fetch intercepted: {data["url"]}");
    }
});

ChannelValue channel = new ChannelValue(new ChannelProperties("fetchInterceptor"));

AddPreloadScriptCommandParameters preloadParams = 
    new AddPreloadScriptCommandParameters(interceptFetchScript);
preloadParams.Arguments.Add(channel);

await driver.Script.AddPreloadScriptAsync(preloadParams);

// Navigate to page that makes fetch calls
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://jsonplaceholder.typicode.com")
    { Wait = ReadinessState.Complete });

await Task.Delay(2000);  // Wait for fetch calls

Console.WriteLine($"\n📊 Total fetch calls: {fetchCalls.Count}");
```

## Example 6: Performance Monitoring

```csharp
string performanceMonitorScript = @"
(channel) => {
    window.addEventListener('load', () => {
        const perfData = performance.getEntriesByType('navigation')[0];
        
        channel({
            domContentLoaded: perfData.domContentLoadedEventEnd - perfData.domContentLoadedEventStart,
            loadComplete: perfData.loadEventEnd - perfData.loadEventStart,
            domInteractive: perfData.domInteractive - perfData.fetchStart,
            totalTime: perfData.loadEventEnd - perfData.fetchStart
        });
    });
}";

Dictionary<string, object>? performanceData = null;

driver.Script.OnMessage.AddObserver((MessageEventArgs e) =>
{
    if (e.ChannelId == "performanceMonitor")
    {
        performanceData = e.Data.ValueAs<Dictionary<string, object>>();
    }
});

ChannelValue channel = new ChannelValue(new ChannelProperties("performanceMonitor"));

AddPreloadScriptCommandParameters preloadParams = 
    new AddPreloadScriptCommandParameters(performanceMonitorScript);
preloadParams.Arguments.Add(channel);

await driver.Script.AddPreloadScriptAsync(preloadParams);

// Navigate
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com")
    { Wait = ReadinessState.Complete });

await Task.Delay(1000);  // Wait for load event

if (performanceData != null)
{
    Console.WriteLine("\n⏱️ Performance Metrics:");
    Console.WriteLine($"  DOM Content Loaded: {performanceData["domContentLoaded"]}ms");
    Console.WriteLine($"  Load Complete: {performanceData["loadComplete"]}ms");
    Console.WriteLine($"  DOM Interactive: {performanceData["domInteractive"]}ms");
    Console.WriteLine($"  Total Time: {performanceData["totalTime"]}ms");
}
```

## Example 7: Multiple Preload Scripts

```csharp
// Script 1: Utilities
string utilitiesScript = @"
() => {
    window.testUtils = {
        highlight: (element) => {
            element.style.border = '2px solid red';
        }
    };
}";

// Script 2: Monitoring
string monitoringScript = @"
(channel) => {
    window.addEventListener('click', (e) => {
        channel({
            type: 'click',
            target: e.target.tagName,
            x: e.clientX,
            y: e.clientY
        });
    });
}";

// Add both scripts
AddPreloadScriptCommandResult utils = await driver.Script.AddPreloadScriptAsync(
    new AddPreloadScriptCommandParameters(utilitiesScript));

ChannelValue channel = new ChannelValue(new ChannelProperties("clickMonitor"));
AddPreloadScriptCommandParameters monitorParams = 
    new AddPreloadScriptCommandParameters(monitoringScript);
monitorParams.Arguments.Add(channel);

AddPreloadScriptCommandResult monitor = await driver.Script.AddPreloadScriptAsync(monitorParams);

Console.WriteLine("Multiple preload scripts added");

// Both scripts will run on every navigation
```

## Pattern: Conditional Preload Scripts

```csharp
// Add preload script only for specific contexts
AddPreloadScriptCommandParameters preloadParams = 
    new AddPreloadScriptCommandParameters(preloadScript);

// Limit to specific contexts
preloadParams.BrowsingContextIds = new List<string> { contextId };

AddPreloadScriptCommandResult result = 
    await driver.Script.AddPreloadScriptAsync(preloadParams);

// Script will only run in the specified context
```

## Pattern: Temporary Preload Script

```csharp
// Add preload script
AddPreloadScriptCommandResult preloadResult = 
    await driver.Script.AddPreloadScriptAsync(preloadParams);

try
{
    // Use preload script for several navigations
    await driver.BrowsingContext.NavigateAsync(navParams1);
    await driver.BrowsingContext.NavigateAsync(navParams2);
}
finally
{
    // Remove when done
    await driver.Script.RemovePreloadScriptAsync(
        new RemovePreloadScriptCommandParameters(preloadResult.PreloadScriptId));
}

// Future navigations won't have the preload script
```

## Best Practices

1. **Use channels**: Communicate with test code via channels
2. **Use sandboxes**: Isolate preload script from page scripts
3. **Keep scripts simple**: Complex logic should be in test code
4. **Remove when done**: Clean up preload scripts after use
5. **Handle timing**: Use load events to ensure DOM is ready
6. **Test isolation**: Each test should manage its own preload scripts

## Common Issues

### Script Not Running

**Problem**: Preload script doesn't seem to execute.

**Solution**:
- Add the script before navigation
- Check for JavaScript errors in the script
- Use `console.log()` in the script to verify execution

### Can't Access Objects

**Problem**: Objects created by preload script are undefined.

**Solution**:
- Ensure you're using the same sandbox
- Check that script actually executed
- Verify timing (DOM might not be ready)

### Channel Messages Not Received

**Problem**: Messages from preload script aren't arriving.

**Solution**:
- Subscribe to script messages before navigation
- Use correct channel ID
- Check that channel was passed as argument

## Next Steps

- [Script Module](../modules/script.md): Complete script module guide
- [Events and Observables](../events-observables.md): Understanding event handling
- [Common Scenarios](common-scenarios.md): More examples

