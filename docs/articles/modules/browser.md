# Browser Module

The Browser module provides browser-level operations including managing user contexts and controlling browser windows.

## Overview

The Browser module allows you to:

- Manage user contexts (profiles/incognito)
- Control browser window state
- Set download behavior
- Close the browser

## Accessing the Module

[!code-csharp[Accessing Module](../../code/modules/BrowserModuleSamples.cs#AccessingModule)]

## User Contexts

User contexts represent isolated browsing sessions (similar to browser profiles or incognito windows).

### Create User Context

[!code-csharp[Create User Context](../../code/modules/BrowserModuleSamples.cs#CreateUserContext)]

### Get User Contexts

[!code-csharp[Get User Contexts](../../code/modules/BrowserModuleSamples.cs#GetUserContexts)]

### Remove User Context

[!code-csharp[Remove User Context](../../code/modules/BrowserModuleSamples.cs#RemoveUserContext)]

### Create Tab in User Context

[!code-csharp[Create Tab in User Context](../../code/modules/BrowserModuleSamples.cs#CreateTabinUserContext)]

## Client Windows

Client windows represent the browser window frames.

### Get Client Windows

[!code-csharp[Get Client Windows](../../code/modules/BrowserModuleSamples.cs#GetClientWindows)]

### Set Window State

[!code-csharp[Set Window State](../../code/modules/BrowserModuleSamples.cs#SetWindowState)]

### Set Window Size

[!code-csharp[Set Window Size](../../code/modules/BrowserModuleSamples.cs#SetWindowSize)]

## Download Behavior

Control how the browser handles downloads.

### Allow Downloads

[!code-csharp[Allow Downloads](../../code/modules/BrowserModuleSamples.cs#AllowDownloads)]

### Deny Downloads

[!code-csharp[Deny Downloads](../../code/modules/BrowserModuleSamples.cs#DenyDownloads)]

## Closing Browser

### Close Browser

[!code-csharp[Close Browser](../../code/modules/BrowserModuleSamples.cs#CloseBrowser)]

Note: This closes the entire browser, not just a tab. To close a tab, use `BrowsingContext.CloseAsync()`.

## Common Patterns

### Isolated Session Pattern

[!code-csharp[Isolated Session Pattern](../../code/modules/BrowserModuleSamples.cs#IsolatedSessionPattern)]

### Multi-Window Testing

[!code-csharp[Multi-Window Testing](../../code/modules/BrowserModuleSamples.cs#Multi-WindowTesting)]

## Best Practices

1. **Clean up user contexts**: Remove user contexts when done to free resources
2. **Use default context**: The default user context exists automatically
3. **Test window states**: Not all window states work on all platforms
4. **Handle downloads carefully**: Set download behavior before triggering downloads

## Next Steps

- [Browsing Context Module](browsing-context.md): Working with tabs and windows
- [Storage Module](storage.md): Managing cookies and storage
- [API Reference](../../api/index.md): Complete API documentation

