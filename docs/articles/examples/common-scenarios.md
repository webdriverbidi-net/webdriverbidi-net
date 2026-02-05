# Common Scenarios

This guide demonstrates common browser automation scenarios using WebDriverBiDi.NET-Relaxed.

## Basic Page Navigation

```csharp
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
await driver.StartAsync("ws://localhost:9222/session");

try
{
    // Get the active context
    GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
        new GetTreeCommandParameters());
    string contextId = tree.ContextTree[0].BrowsingContextId;

    // Navigate and wait for page load
    NavigateCommandParameters navParams = new NavigateCommandParameters(
        contextId,
        "https://example.com")
    {
        Wait = ReadinessState.Complete
    };
    
    NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(navParams);
    Console.WriteLine($"Navigated to: {result.Url}");
}
finally
{
    await driver.StopAsync();
}
```

## Form Submission

```csharp
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;
using WebDriverBiDi.Script;

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
await driver.StartAsync("ws://localhost:9222/session");

try
{
    GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
        new GetTreeCommandParameters());
    string contextId = tree.ContextTree[0].BrowsingContextId;

    // Navigate to form page
    await driver.BrowsingContext.NavigateAsync(
        new NavigateCommandParameters(contextId, "https://example.com/form")
        { Wait = ReadinessState.Complete });

    // Find the input field
    string findInputScript = "document.querySelector('input[name=\"username\"]')";
    EvaluateResult inputResult = await driver.Script.EvaluateAsync(
        new EvaluateCommandParameters(findInputScript, new ContextTarget(contextId), true));

    if (inputResult is EvaluateResultSuccess inputSuccess)
    {
        RemoteValue inputElement = inputSuccess.Result;
        
        // Click the input to focus it
        PerformActionsCommandParameters clickParams = new PerformActionsCommandParameters(contextId);
        PointerSource mouse = new PointerSource("mouse", PointerType.Mouse);
        mouse.CreatePointerMoveToElement(inputElement.ToSharedReference(), 0, 0, TimeSpan.Zero);
        mouse.CreatePointerDown(MouseButton.Left);
        mouse.CreatePointerUp(MouseButton.Left);
        clickParams.Actions.Add(mouse);
        await driver.Input.PerformActionsAsync(clickParams);

        // Type into the field
        PerformActionsCommandParameters typeParams = new PerformActionsCommandParameters(contextId);
        KeySource keyboard = new KeySource("keyboard");
        
        string username = "testuser";
        foreach (char c in username)
        {
            keyboard.CreateKeyDown(c.ToString());
            keyboard.CreateKeyUp(c.ToString());
        }
        
        typeParams.Actions.Add(keyboard);
        await driver.Input.PerformActionsAsync(typeParams);

        // Submit the form (press Enter)
        PerformActionsCommandParameters submitParams = new PerformActionsCommandParameters(contextId);
        KeySource submitKey = new KeySource("keyboard");
        submitKey.CreateKeyDown(Keys.Enter);
        submitKey.CreateKeyUp(Keys.Enter);
        submitParams.Actions.Add(submitKey);
        await driver.Input.PerformActionsAsync(submitParams);
    }
}
finally
{
    await driver.StopAsync();
}
```

## Monitoring Console Logs

```csharp
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Log;
using WebDriverBiDi.Session;

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
await driver.StartAsync("ws://localhost:9222/session");

try
{
    // Set up log monitoring
    List<string> consoleMessages = new List<string>();
    
    driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
    {
        string message = $"[{e.Timestamp:HH:mm:ss}] {e.Level}: {e.Text}";
        consoleMessages.Add(message);
        Console.WriteLine(message);
    });

    // Subscribe to log events
    SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
    subscribe.Events.Add(driver.Log.OnEntryAdded.EventName);
    await driver.Session.SubscribeAsync(subscribe);

    // Navigate to page
    GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
        new GetTreeCommandParameters());
    string contextId = tree.ContextTree[0].BrowsingContextId;

    await driver.BrowsingContext.NavigateAsync(
        new NavigateCommandParameters(contextId, "https://example.com")
        { Wait = ReadinessState.Complete });

    // Wait for logs to arrive
    await Task.Delay(2000);

    Console.WriteLine($"\nTotal console messages: {consoleMessages.Count}");
}
finally
{
    await driver.StopAsync();
}
```

## Capturing Screenshots

```csharp
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
await driver.StartAsync("ws://localhost:9222/session");

try
{
    GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
        new GetTreeCommandParameters());
    string contextId = tree.ContextTree[0].BrowsingContextId;

    // Navigate to page
    await driver.BrowsingContext.NavigateAsync(
        new NavigateCommandParameters(contextId, "https://example.com")
        { Wait = ReadinessState.Complete });

    // Capture full page screenshot
    CaptureScreenshotCommandParameters screenshotParams = 
        new CaptureScreenshotCommandParameters(contextId);
    
    CaptureScreenshotCommandResult result = 
        await driver.BrowsingContext.CaptureScreenshotAsync(screenshotParams);

    // Save to file
    byte[] imageBytes = Convert.FromBase64String(result.Data);
    await File.WriteAllBytesAsync("screenshot.png", imageBytes);
    
    Console.WriteLine("Screenshot saved to screenshot.png");
}
finally
{
    await driver.StopAsync();
}
```

## Network Traffic Monitoring

```csharp
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Network;
using WebDriverBiDi.Session;

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
await driver.StartAsync("ws://localhost:9222/session");

try
{
    // Track all network requests
    List<RequestData> requests = new List<RequestData>();
    
    driver.Network.OnResponseCompleted.AddObserver((ResponseCompletedEventArgs e) =>
    {
        requests.Add(e.Request);
        Console.WriteLine($"{e.Response.Status} - {e.Request.Method} {e.Request.Url}");
    });

    // Subscribe to network events
    SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
    subscribe.Events.Add(driver.Network.OnResponseCompleted.EventName);
    await driver.Session.SubscribeAsync(subscribe);

    // Navigate
    GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
        new GetTreeCommandParameters());
    string contextId = tree.ContextTree[0].BrowsingContextId;

    await driver.BrowsingContext.NavigateAsync(
        new NavigateCommandParameters(contextId, "https://example.com")
        { Wait = ReadinessState.Complete });

    // Wait for all resources to load
    await Task.Delay(2000);

    Console.WriteLine($"\nTotal requests: {requests.Count}");
    
    // Analyze requests
    int htmlCount = requests.Count(r => r.Url.Contains(".html") || r.Url.EndsWith("/"));
    int cssCount = requests.Count(r => r.Url.Contains(".css"));
    int jsCount = requests.Count(r => r.Url.Contains(".js"));
    int imageCount = requests.Count(r => r.Url.Contains(".jpg") || r.Url.Contains(".png") || r.Url.Contains(".gif"));
    
    Console.WriteLine($"HTML: {htmlCount}, CSS: {cssCount}, JS: {jsCount}, Images: {imageCount}");
}
finally
{
    await driver.StopAsync();
}
```

## Multi-Tab Management

```csharp
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
await driver.StartAsync("ws://localhost:9222/session");

try
{
    // Create multiple tabs
    List<string> tabIds = new List<string>();
    
    for (int i = 0; i < 3; i++)
    {
        CreateCommandResult result = await driver.BrowsingContext.CreateAsync(
            new CreateCommandParameters(ContextType.Tab));
        tabIds.Add(result.BrowsingContextId);
        Console.WriteLine($"Created tab {i + 1}: {result.BrowsingContextId}");
    }

    // Navigate each tab to different URL
    string[] urls = 
    {
        "https://example.com",
        "https://example.org",
        "https://example.net"
    };

    for (int i = 0; i < tabIds.Count; i++)
    {
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(tabIds[i], urls[i])
            { Wait = ReadinessState.Complete });
        Console.WriteLine($"Tab {i + 1} navigated to {urls[i]}");
    }

    // Get page titles from all tabs
    for (int i = 0; i < tabIds.Count; i++)
    {
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "document.title",
                new ContextTarget(tabIds[i]),
                true));
        
        if (result is EvaluateResultSuccess success)
        {
            string title = success.Result.ValueAs<string>();
            Console.WriteLine($"Tab {i + 1} title: {title}");
        }
    }

    // Close all tabs
    foreach (string tabId in tabIds)
    {
        await driver.BrowsingContext.CloseAsync(new CloseCommandParameters(tabId));
    }
    
    Console.WriteLine("All tabs closed");
}
finally
{
    await driver.StopAsync();
}
```

## Waiting for Elements

```csharp
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
await driver.StartAsync("ws://localhost:9222/session");

try
{
    GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
        new GetTreeCommandParameters());
    string contextId = tree.ContextTree[0].BrowsingContextId;

    await driver.BrowsingContext.NavigateAsync(
        new NavigateCommandParameters(contextId, "https://example.com")
        { Wait = ReadinessState.Complete });

    // Wait for element to appear (with polling)
    bool elementFound = false;
    int maxAttempts = 30;
    int attempt = 0;

    while (!elementFound && attempt < maxAttempts)
    {
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "document.querySelector('.dynamic-content') !== null",
                new ContextTarget(contextId),
                true));

        if (result is EvaluateResultSuccess success)
        {
            elementFound = success.Result.ValueAs<bool>();
            if (elementFound)
            {
                Console.WriteLine($"Element found after {attempt * 100}ms");
                break;
            }
        }

        await Task.Delay(100);
        attempt++;
    }

    if (!elementFound)
    {
        Console.WriteLine("Element not found after timeout");
    }
}
finally
{
    await driver.StopAsync();
}
```

## Cookie Management

```csharp
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Storage;

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
await driver.StartAsync("ws://localhost:9222/session");

try
{
    GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
        new GetTreeCommandParameters());
    string contextId = tree.ContextTree[0].BrowsingContextId;

    // Set a cookie before navigating
    PartialCookie cookie = new PartialCookie(
        "userPreference",
        new BytesValue(BytesValueType.String, "darkMode"))
    {
        Domain = "example.com",
        Path = "/",
        Secure = false,
        HttpOnly = false
    };

    await driver.Storage.SetCookieAsync(new SetCookieCommandParameters(cookie));
    Console.WriteLine("Cookie set");

    // Navigate
    await driver.BrowsingContext.NavigateAsync(
        new NavigateCommandParameters(contextId, "https://example.com")
        { Wait = ReadinessState.Complete });

    // Retrieve cookies
    GetCookiesCommandParameters getCookiesParams = new GetCookiesCommandParameters();
    getCookiesParams.BrowsingContexts.Add(contextId);
    
    GetCookiesCommandResult cookies = await driver.Storage.GetCookiesAsync(getCookiesParams);

    Console.WriteLine($"\nAll cookies ({cookies.Cookies.Count}):");
    foreach (var c in cookies.Cookies)
    {
        Console.WriteLine($"  {c.Name} = {c.Value.Value}");
    }

    // Delete specific cookie
    DeleteCookiesCommandParameters deleteParams = new DeleteCookiesCommandParameters();
    deleteParams.Filter = new CookieFilter { Name = "userPreference" };
    await driver.Storage.DeleteCookiesAsync(deleteParams);
    
    Console.WriteLine("\nCookie deleted");
}
finally
{
    await driver.StopAsync();
}
```

## JavaScript Execution

```csharp
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(10));
await driver.StartAsync("ws://localhost:9222/session");

try
{
    GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
        new GetTreeCommandParameters());
    string contextId = tree.ContextTree[0].BrowsingContextId;

    await driver.BrowsingContext.NavigateAsync(
        new NavigateCommandParameters(contextId, "https://example.com")
        { Wait = ReadinessState.Complete });

    // Execute simple expression
    EvaluateResult titleResult = await driver.Script.EvaluateAsync(
        new EvaluateCommandParameters("document.title", new ContextTarget(contextId), true));

    if (titleResult is EvaluateResultSuccess titleSuccess)
    {
        Console.WriteLine($"Title: {titleSuccess.Result.ValueAs<string>()}");
    }

    // Call function with arguments
    string addFunction = "(a, b) => a + b";
    CallFunctionCommandParameters funcParams = new CallFunctionCommandParameters(
        addFunction,
        new ContextTarget(contextId),
        true);
    funcParams.Arguments.Add(LocalValue.Number(10));
    funcParams.Arguments.Add(LocalValue.Number(20));

    EvaluateResult sumResult = await driver.Script.CallFunctionAsync(funcParams);
    if (sumResult is EvaluateResultSuccess sumSuccess)
    {
        Console.WriteLine($"10 + 20 = {sumSuccess.Result.ValueAs<long>()}");
    }

    // Execute complex script
    string complexScript = @"
    {
        url: window.location.href,
        linkCount: document.querySelectorAll('a').length,
        imageCount: document.querySelectorAll('img').length,
        hasTitle: !!document.title
    }";

    EvaluateResult complexResult = await driver.Script.EvaluateAsync(
        new EvaluateCommandParameters(complexScript, new ContextTarget(contextId), true));

    if (complexResult is EvaluateResultSuccess complexSuccess)
    {
        var data = complexSuccess.Result.ValueAs<Dictionary<string, object>>();
        Console.WriteLine($"\nPage analysis:");
        Console.WriteLine($"  URL: {data["url"]}");
        Console.WriteLine($"  Links: {data["linkCount"]}");
        Console.WriteLine($"  Images: {data["imageCount"]}");
        Console.WriteLine($"  Has title: {data["hasTitle"]}");
    }
}
finally
{
    await driver.StopAsync();
}
```

## Next Steps

- [Form Submission Example](form-submission.md): Detailed form interaction
- [Network Interception Example](network-interception.md): Advanced network control
- [Console Monitoring Example](console-monitoring.md): Log collection patterns
- [Preload Scripts Example](preload-scripts.md): JavaScript injection

