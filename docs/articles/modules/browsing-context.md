# Browsing Context Module

The Browsing Context module provides functionality for managing browser tabs, windows, and iframes, as well as navigating and interacting with pages.

## Overview

A **browsing context** represents a document environment in the browser. This can be:

- A browser tab
- A browser window
- An iframe within a page

Each browsing context has a unique identifier used to target operations.

## Accessing the Module

```csharp
BrowsingContextModule browsingContext = driver.BrowsingContext;
```

## Getting Browsing Contexts

### Get All Contexts

```csharp
GetTreeCommandParameters params = new GetTreeCommandParameters();
GetTreeCommandResult result = await driver.BrowsingContext.GetTreeAsync(params);

foreach (BrowsingContextInfo context in result.ContextTree)
{
    Console.WriteLine($"Context ID: {context.BrowsingContextId}");
    Console.WriteLine($"URL: {context.Url}");
    Console.WriteLine($"Parent: {context.ParentBrowsingContextId ?? "none"}");
    Console.WriteLine($"Children: {context.Children.Count}");
}
```

### Get Specific Context

```csharp
GetTreeCommandParameters params = new GetTreeCommandParameters()
{
    Root = contextId  // Only get this context and its descendants
};
GetTreeCommandResult result = await driver.BrowsingContext.GetTreeAsync(params);
```

### Get Only Top-Level Contexts

```csharp
GetTreeCommandParameters params = new GetTreeCommandParameters()
{
    MaxDepth = 0  // Don't include child contexts (iframes)
};
GetTreeCommandResult result = await driver.BrowsingContext.GetTreeAsync(params);
```

## Creating Contexts

### Create a New Tab

```csharp
CreateCommandParameters params = new CreateCommandParameters(ContextType.Tab);
CreateCommandResult result = await driver.BrowsingContext.CreateAsync(params);

string newTabId = result.BrowsingContextId;
Console.WriteLine($"Created tab: {newTabId}");
```

### Create a New Window

```csharp
CreateCommandParameters params = new CreateCommandParameters(ContextType.Window);
CreateCommandResult result = await driver.BrowsingContext.CreateAsync(params);

string newWindowId = result.BrowsingContextId;
```

### Create Context in User Context

```csharp
// First create a user context
CreateUserContextCommandResult userContext = 
    await driver.Browser.CreateUserContextAsync(new CreateUserContextCommandParameters());

// Create tab in that user context
CreateCommandParameters params = new CreateCommandParameters(ContextType.Tab)
{
    UserContext = userContext.UserContextId
};
CreateCommandResult result = await driver.BrowsingContext.CreateAsync(params);
```

### Set Window Size on Creation

```csharp
CreateCommandParameters params = new CreateCommandParameters(ContextType.Window)
{
    Width = 1280,
    Height = 720
};
CreateCommandResult result = await driver.BrowsingContext.CreateAsync(params);
```

## Navigation

### Basic Navigation

```csharp
NavigateCommandParameters params = new NavigateCommandParameters(
    contextId,
    "https://example.com");

NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(params);

Console.WriteLine($"Navigation ID: {result.NavigationId}");
Console.WriteLine($"URL: {result.Url}");
```

### Wait for Page Load

```csharp
NavigateCommandParameters params = new NavigateCommandParameters(
    contextId,
    "https://example.com")
{
    Wait = ReadinessState.Complete  // Wait for full page load
};
await driver.BrowsingContext.NavigateAsync(params);
```

Readiness states:
- `ReadinessState.None`: Return immediately after navigation starts
- `ReadinessState.Interactive`: Wait for DOM ready
- `ReadinessState.Complete`: Wait for full page load (including images, CSS)

### Navigation with Timeout

```csharp
NavigateCommandParameters params = new NavigateCommandParameters(
    contextId,
    "https://example.com")
{
    Wait = ReadinessState.Complete,
    TimeoutSeconds = 30  // Fail if not loaded in 30 seconds
};
await driver.BrowsingContext.NavigateAsync(params);
```

### Back/Forward Navigation

```csharp
// Navigate back
TraverseHistoryCommandParameters backParams = 
    new TraverseHistoryCommandParameters(contextId, -1);
await driver.BrowsingContext.TraverseHistoryAsync(backParams);

// Navigate forward
TraverseHistoryCommandParameters forwardParams = 
    new TraverseHistoryCommandParameters(contextId, 1);
await driver.BrowsingContext.TraverseHistoryAsync(forwardParams);
```

### Reload Page

```csharp
ReloadCommandParameters params = new ReloadCommandParameters(contextId);
await driver.BrowsingContext.ReloadAsync(params);

// Or wait for complete reload
ReloadCommandParameters params = new ReloadCommandParameters(contextId)
{
    Wait = ReadinessState.Complete
};
await driver.BrowsingContext.ReloadAsync(params);
```

## Closing Contexts

### Close a Tab

```csharp
CloseCommandParameters params = new CloseCommandParameters(contextId);
await driver.BrowsingContext.CloseAsync(params);
```

### Close All Tabs in User Context

```csharp
// Get all contexts in user context
GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
    new GetTreeCommandParameters());

foreach (var context in tree.ContextTree)
{
    if (context.UserContext == userContextId)
    {
        await driver.BrowsingContext.CloseAsync(
            new CloseCommandParameters(context.BrowsingContextId));
    }
}
```

## Locating Elements

The Browsing Context module provides element location functionality.

### Locate by CSS Selector

```csharp
LocateNodesCommandParameters params = new LocateNodesCommandParameters(
    contextId,
    new CssLocator("button.submit"));

LocateNodesCommandResult result = await driver.BrowsingContext.LocateNodesAsync(params);

foreach (RemoteValue node in result.Nodes)
{
    Console.WriteLine($"Found element: {node.SharedId}");
}
```

### Locate by XPath

```csharp
LocateNodesCommandParameters params = new LocateNodesCommandParameters(
    contextId,
    new XPathLocator("//button[@type='submit']"));

LocateNodesCommandResult result = await driver.BrowsingContext.LocateNodesAsync(params);
```

### Locate with Maximum Results

```csharp
LocateNodesCommandParameters params = new LocateNodesCommandParameters(
    contextId,
    new CssLocator("input"))
{
    MaxNodeCount = 5  // Return at most 5 elements
};
LocateNodesCommandResult result = await driver.BrowsingContext.LocateNodesAsync(params);
```

### Locate Within Element

```csharp
// First find parent element
LocateNodesCommandResult parentResult = await driver.BrowsingContext.LocateNodesAsync(
    new LocateNodesCommandParameters(contextId, new CssLocator("#container")));

RemoteValue parent = parentResult.Nodes[0];

// Find children within parent
LocateNodesCommandParameters params = new LocateNodesCommandParameters(
    contextId,
    new CssLocator("button"))
{
    StartNodes = new List<SharedReference> { parent.ToSharedReference() }
};
LocateNodesCommandResult result = await driver.BrowsingContext.LocateNodesAsync(params);
```

## Capturing Screenshots

### Screenshot of Entire Viewport

```csharp
CaptureScreenshotCommandParameters params = 
    new CaptureScreenshotCommandParameters(contextId);

CaptureScreenshotCommandResult result = 
    await driver.BrowsingContext.CaptureScreenshotAsync(params);

// result.Data is base64-encoded PNG
byte[] imageBytes = Convert.FromBase64String(result.Data);
await File.WriteAllBytesAsync("screenshot.png", imageBytes);
```

### Screenshot of Specific Element

```csharp
// First locate the element
LocateNodesCommandResult locateResult = await driver.BrowsingContext.LocateNodesAsync(
    new LocateNodesCommandParameters(contextId, new CssLocator("#chart")));

RemoteValue element = locateResult.Nodes[0];

// Capture element screenshot
CaptureScreenshotCommandParameters params = 
    new CaptureScreenshotCommandParameters(contextId)
    {
        Origin = new ElementOrigin(element.ToSharedReference())
    };

CaptureScreenshotCommandResult result = 
    await driver.BrowsingContext.CaptureScreenshotAsync(params);
```

### Clipped Screenshot

```csharp
CaptureScreenshotCommandParameters params = 
    new CaptureScreenshotCommandParameters(contextId)
    {
        Clip = new BoxClipRectangle()
        {
            X = 100,
            Y = 100,
            Width = 800,
            Height = 600
        }
    };

CaptureScreenshotCommandResult result = 
    await driver.BrowsingContext.CaptureScreenshotAsync(params);
```

## Printing to PDF

```csharp
PrintCommandParameters params = new PrintCommandParameters(contextId);
PrintCommandResult result = await driver.BrowsingContext.PrintAsync(params);

// result.Data is base64-encoded PDF
byte[] pdfBytes = Convert.FromBase64String(result.Data);
await File.WriteAllBytesAsync("page.pdf", pdfBytes);
```

### PDF with Custom Settings

```csharp
PrintCommandParameters params = new PrintCommandParameters(contextId)
{
    Orientation = PrintOrientation.Landscape,
    Scale = 0.8,
    Background = true,  // Print background colors/images
    PageWidth = 8.5,    // Inches
    PageHeight = 11,
    MarginTop = 0.5,
    MarginBottom = 0.5,
    MarginLeft = 0.5,
    MarginRight = 0.5
};
PrintCommandResult result = await driver.BrowsingContext.PrintAsync(params);
```

## Handling User Prompts

### Accept Alert/Confirm

```csharp
// Listen for prompt opened event
driver.BrowsingContext.OnUserPromptOpened.AddObserver((e) =>
{
    Console.WriteLine($"Prompt: {e.Message}");
});

// Subscribe
SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.BrowsingContext.OnUserPromptOpened.EventName);
await driver.Session.SubscribeAsync(subscribe);

// When prompt appears, handle it
HandleUserPromptCommandParameters params = 
    new HandleUserPromptCommandParameters(contextId);
params.Accept = true;  // Click OK/Accept

await driver.BrowsingContext.HandleUserPromptAsync(params);
```

### Dismiss Prompt

```csharp
HandleUserPromptCommandParameters params = 
    new HandleUserPromptCommandParameters(contextId);
params.Accept = false;  // Click Cancel
await driver.BrowsingContext.HandleUserPromptAsync(params);
```

### Enter Text in Prompt

```csharp
// For prompt() dialogs that accept user input
HandleUserPromptCommandParameters params = 
    new HandleUserPromptCommandParameters(contextId);
params.Accept = true;
params.UserText = "My input text";
await driver.BrowsingContext.HandleUserPromptAsync(params);
```

## Activation

### Bring Tab to Foreground

```csharp
ActivateCommandParameters params = new ActivateCommandParameters(contextId);
await driver.BrowsingContext.ActivateAsync(params);
```

## Events

### Navigation Events

```csharp
// Page load complete
driver.BrowsingContext.OnLoad.AddObserver((NavigationEventArgs e) =>
{
    Console.WriteLine($"Page loaded: {e.Url}");
});

// DOM ready
driver.BrowsingContext.OnDomContentLoaded.AddObserver((NavigationEventArgs e) =>
{
    Console.WriteLine($"DOM ready: {e.Url}");
});

// Navigation started
driver.BrowsingContext.OnNavigationStarted.AddObserver((NavigationEventArgs e) =>
{
    Console.WriteLine($"Navigation started to: {e.Url}");
});

// Navigation failed
driver.BrowsingContext.OnNavigationFailed.AddObserver((NavigationEventArgs e) =>
{
    Console.WriteLine($"Navigation failed: {e.Url}");
});
```

### Context Lifecycle Events

```csharp
// New tab/window/iframe created
driver.BrowsingContext.OnContextCreated.AddObserver((ContextCreatedEventArgs e) =>
{
    Console.WriteLine($"Context created: {e.BrowsingContextId}");
    Console.WriteLine($"URL: {e.Url}");
    Console.WriteLine($"Type: {e.OriginalOpener ?? "user-initiated"}");
});

// Tab/window closed
driver.BrowsingContext.OnContextDestroyed.AddObserver((ContextDestroyedEventArgs e) =>
{
    Console.WriteLine($"Context destroyed: {e.BrowsingContextId}");
});
```

### User Prompt Events

```csharp
// Alert/confirm/prompt opened
driver.BrowsingContext.OnUserPromptOpened.AddObserver((UserPromptOpenedEventArgs e) =>
{
    Console.WriteLine($"Prompt type: {e.Type}");
    Console.WriteLine($"Message: {e.Message}");
});

// Prompt closed
driver.BrowsingContext.OnUserPromptClosed.AddObserver((UserPromptClosedEventArgs e) =>
{
    Console.WriteLine($"Prompt closed with accept={e.Accepted}");
    if (e.UserText != null)
    {
        Console.WriteLine($"User entered: {e.UserText}");
    }
});
```

## Common Patterns

### Wait for Page Load Pattern

```csharp
SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.BrowsingContext.OnLoad.EventName);
await driver.Session.SubscribeAsync(subscribe);

EventObserver<NavigationEventArgs> observer = 
    driver.BrowsingContext.OnLoad.AddObserver((e) => { });

observer.SetCheckpoint();
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, url));

bool loaded = observer.WaitForCheckpoint(TimeSpan.FromSeconds(30));
if (!loaded)
{
    Console.WriteLine("Page load timeout!");
}
```

### Multi-Tab Pattern

```csharp
// Open multiple tabs
List<string> contextIds = new List<string>();
for (int i = 0; i < 3; i++)
{
    CreateCommandResult result = await driver.BrowsingContext.CreateAsync(
        new CreateCommandParameters(ContextType.Tab));
    contextIds.Add(result.BrowsingContextId);
}

// Navigate each tab
foreach (string contextId in contextIds)
{
    await driver.BrowsingContext.NavigateAsync(
        new NavigateCommandParameters(contextId, $"https://example.com/page{contextIds.IndexOf(contextId)}"));
}

// Close all tabs
foreach (string contextId in contextIds)
{
    await driver.BrowsingContext.CloseAsync(
        new CloseCommandParameters(contextId));
}
```

## Best Practices

1. **Always wait for readiness**: Use `ReadinessState.Complete` for reliable automation
2. **Handle prompts**: Set up observers for user prompts before triggering actions that may create them
3. **Clean up contexts**: Close tabs when done to free resources
4. **Use appropriate locators**: CSS selectors are generally faster than XPath
5. **Cache context IDs**: Store context IDs rather than repeatedly calling GetTree

## Next Steps

- [Script Module](script.md): Execute JavaScript in browsing contexts
- [Input Module](input.md): Simulate user interactions
- [Network Module](network.md): Monitor navigation traffic
- [Examples](../examples/common-scenarios.md): See complete examples

## API Reference

See the [API documentation](../../api/index.md) for complete details on all classes and methods.

