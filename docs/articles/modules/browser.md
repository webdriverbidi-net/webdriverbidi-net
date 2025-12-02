# Browser Module

The Browser module provides browser-level operations including managing user contexts and controlling browser windows.

## Overview

The Browser module allows you to:

- Manage user contexts (profiles/incognito)
- Control browser window state
- Set download behavior
- Close the browser

## Accessing the Module

```csharp
BrowserModule browser = driver.Browser;
```

## User Contexts

User contexts represent isolated browsing sessions (similar to browser profiles or incognito windows).

### Create User Context

```csharp
CreateUserContextCommandParameters params = new CreateUserContextCommandParameters();
CreateUserContextCommandResult result = await driver.Browser.CreateUserContextAsync(params);

string userContextId = result.UserContextId;
Console.WriteLine($"Created user context: {userContextId}");
```

### Get User Contexts

```csharp
GetUserContextsCommandParameters params = new GetUserContextsCommandParameters();
GetUserContextsCommandResult result = await driver.Browser.GetUserContextsAsync(params);

foreach (UserContextInfo context in result.UserContexts)
{
    Console.WriteLine($"User context: {context.UserContextId}");
}
```

### Remove User Context

```csharp
RemoveUserContextCommandParameters params = 
    new RemoveUserContextCommandParameters(userContextId);

await driver.Browser.RemoveUserContextAsync(params);
```

### Create Tab in User Context

```csharp
// First create user context
CreateUserContextCommandResult userContextResult = 
    await driver.Browser.CreateUserContextAsync(new CreateUserContextCommandParameters());

// Create browsing context in that user context
CreateCommandParameters createTabParams = new CreateCommandParameters(ContextType.Tab)
{
    UserContext = userContextResult.UserContextId
};

CreateCommandResult tabResult = await driver.BrowsingContext.CreateAsync(createTabParams);
```

## Client Windows

Client windows represent the browser window frames.

### Get Client Windows

```csharp
GetClientWindowsCommandParameters params = new GetClientWindowsCommandParameters();
GetClientWindowsCommandResult result = await driver.Browser.GetClientWindowsAsync(params);

foreach (ClientWindowInfo window in result.ClientWindows)
{
    Console.WriteLine($"Window: {window.ClientWindowId}");
    Console.WriteLine($"  State: {window.State}");
    Console.WriteLine($"  Width: {window.Width}");
    Console.WriteLine($"  Height: {window.Height}");
}
```

### Set Window State

```csharp
// Get the client window
GetClientWindowsCommandResult windowsResult = 
    await driver.Browser.GetClientWindowsAsync(new GetClientWindowsCommandParameters());

string clientWindowId = windowsResult.ClientWindows[0].ClientWindowId;

// Maximize window
SetClientWindowStateCommandParameters params = 
    new SetClientWindowStateCommandParameters(clientWindowId, ClientWindowState.Maximized);

await driver.Browser.SetClientWindowStateAsync(params);

// Other states: Minimized, Fullscreen, Normal
```

### Set Window Size

```csharp
SetClientWindowStateCommandParameters params = 
    new SetClientWindowStateCommandParameters(clientWindowId, ClientWindowState.Normal)
    {
        Width = 1280,
        Height = 720
    };

await driver.Browser.SetClientWindowStateAsync(params);
```

## Download Behavior

Control how the browser handles downloads.

### Allow Downloads

```csharp
SetDownloadBehaviorCommandParameters params = new SetDownloadBehaviorCommandParameters();
params.DownloadBehavior = new DownloadBehaviorAllowed("/path/to/downloads");

await driver.Browser.SetDownloadBehaviorAsync(params);
```

### Deny Downloads

```csharp
SetDownloadBehaviorCommandParameters params = new SetDownloadBehaviorCommandParameters();
params.DownloadBehavior = new DownloadBehaviorDenied();

await driver.Browser.SetDownloadBehaviorAsync(params);
```

## Closing Browser

### Close Browser

```csharp
CloseCommandParameters params = new CloseCommandParameters();
await driver.Browser.CloseAsync(params);
```

Note: This closes the entire browser, not just a tab. To close a tab, use `BrowsingContext.CloseAsync()`.

## Common Patterns

### Isolated Session Pattern

```csharp
// Create isolated user context
CreateUserContextCommandResult userContext = 
    await driver.Browser.CreateUserContextAsync(new CreateUserContextCommandParameters());

// Create tab in isolated context
CreateCommandParameters tabParams = new CreateCommandParameters(ContextType.Tab)
{
    UserContext = userContext.UserContextId
};
CreateCommandResult tab = await driver.BrowsingContext.CreateAsync(tabParams);

// Use the tab...
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(tab.BrowsingContextId, "https://example.com"));

// Clean up: close tab and remove context
await driver.BrowsingContext.CloseAsync(
    new CloseCommandParameters(tab.BrowsingContextId));
await driver.Browser.RemoveUserContextAsync(
    new RemoveUserContextCommandParameters(userContext.UserContextId));
```

### Multi-Window Testing

```csharp
// Create multiple windows
List<string> windowIds = new List<string>();

for (int i = 0; i < 3; i++)
{
    CreateCommandResult window = await driver.BrowsingContext.CreateAsync(
        new CreateCommandParameters(ContextType.Window)
        {
            Width = 800,
            Height = 600
        });
    windowIds.Add(window.BrowsingContextId);
}

// Get client windows and manipulate them
GetClientWindowsCommandResult clientWindows = 
    await driver.Browser.GetClientWindowsAsync(new GetClientWindowsCommandParameters());

foreach (var window in clientWindows.ClientWindows)
{
    // Tile windows side by side
    await driver.Browser.SetClientWindowStateAsync(
        new SetClientWindowStateCommandParameters(window.ClientWindowId, ClientWindowState.Normal)
        {
            Width = 640,
            Height = 480
        });
}
```

## Best Practices

1. **Clean up user contexts**: Remove user contexts when done to free resources
2. **Use default context**: The default user context exists automatically
3. **Test window states**: Not all window states work on all platforms
4. **Handle downloads carefully**: Set download behavior before triggering downloads

## Next Steps

- [Browsing Context Module](browsing-context.md): Working with tabs and windows
- [Storage Module](storage.md): Managing cookies and storage
- [API Reference](../../api/index.md): Complete API documentation

