# Browsing Context Module

The Browsing Context module provides functionality for managing browser tabs, windows, and iframes, as well as navigating and interacting with pages.

## Overview

A **browsing context** represents a document environment in the browser. This can be:

- A browser tab
- A browser window
- An iframe within a page

Each browsing context has a unique identifier used to target operations.

## Accessing the Module

[!code-csharp[Accessing Module](../../code/modules/BrowsingContextModuleSamples.cs#AccessingModule)]

## Timeout and Cancellation

All commands in this module accept optional `timeoutOverride` and `CancellationToken` parameters. Use `timeoutOverride` to set a per-command timeout (defaults to `BiDiDriver.DefaultCommandTimeout` when omitted). Use `CancellationToken` for cooperative cancellation. The [Navigation with Timeout](#navigation-with-timeout) section below shows an example for `NavigateAsync`; the same parameters apply to all other commands (e.g., `ActivateAsync`, `CaptureScreenshotAsync`, `CreateAsync`). See the [API Design Guide](../advanced/api-design.md#timeout-and-cancellation) for more examples.

## Getting Browsing Contexts

### Get All Contexts

[!code-csharp[Get All Contexts](../../code/modules/BrowsingContextModuleSamples.cs#GetAllContexts)]

### Get Specific Context

[!code-csharp[Get Specific Context](../../code/modules/BrowsingContextModuleSamples.cs#GetSpecificContext)]

### Get Only Top-Level Contexts

[!code-csharp[Get Only Top-Level Contexts](../../code/modules/BrowsingContextModuleSamples.cs#GetOnlyTop-LevelContexts)]

## Creating Contexts

### Create a New Tab

[!code-csharp[Create New Tab](../../code/modules/BrowsingContextModuleSamples.cs#CreateNewTab)]

### Create a New Window

[!code-csharp[Create New Window](../../code/modules/BrowsingContextModuleSamples.cs#CreateNewWindow)]

### Create Context in User Context

[!code-csharp[Create Context in User Context](../../code/modules/UserContextSamples.cs#CreateContextinUserContext)]

## Navigation

### Basic Navigation

[!code-csharp[Basic Navigation](../../code/modules/BrowsingContextModuleSamples.cs#BasicNavigation)]

### Wait for Page Load

[!code-csharp[Wait for Page Load](../../code/modules/BrowsingContextModuleSamples.cs#WaitforPageLoad)]

Readiness states:
- `ReadinessState.None`: Return immediately after navigation starts
- `ReadinessState.Interactive`: Wait for DOM ready
- `ReadinessState.Complete`: Wait for full page load (including images, CSS)

### Navigation with Timeout

Use the `timeoutOverride` parameter (second argument to `NavigateAsync`) to fail fast when a page takes too long to load:

[!code-csharp[Navigation with Timeout](../../code/modules/BrowsingContextModuleSamples.cs#NavigationwithTimeout)]

### Back/Forward Navigation

[!code-csharp[Back/Forward Navigation](../../code/modules/BrowsingContextModuleSamples.cs#Back/ForwardNavigation)]

### Reload Page

[!code-csharp[Reload Page](../../code/modules/BrowsingContextModuleSamples.cs#ReloadPage)]

## Closing Contexts

### Close a Tab

[!code-csharp[Close Tab](../../code/modules/BrowsingContextModuleSamples.cs#CloseTab)]

### Close All Tabs in User Context

[!code-csharp[Close All Tabs in User Context](../../code/modules/BrowsingContextModuleSamples.cs#CloseAllTabsinUserContext)]

## Locating Elements

The Browsing Context module provides element location functionality.

### Locate by CSS Selector

[!code-csharp[Locate by CSS Selector](../../code/modules/BrowsingContextModuleSamples.cs#LocatebyCSSSelector)]

### Locate by XPath

[!code-csharp[Locate by XPath](../../code/modules/BrowsingContextModuleSamples.cs#LocatebyXPath)]

### Locate with Maximum Results

[!code-csharp[Locate with Maximum Results](../../code/modules/BrowsingContextModuleSamples.cs#LocatewithMaximumResults)]

### Locate Within Element

[!code-csharp[Locate Within Element](../../code/modules/BrowsingContextModuleSamples.cs#LocateWithinElement)]

## Capturing Screenshots

### Screenshot of Entire Viewport

[!code-csharp[Screenshot of Viewport](../../code/modules/BrowsingContextModuleSamples.cs#ScreenshotofViewport)]

### Screenshot of Specific Element

[!code-csharp[Screenshot of Element](../../code/modules/BrowsingContextModuleSamples.cs#ScreenshotofElement)]

### Clipped Screenshot

[!code-csharp[Clipped Screenshot](../../code/modules/BrowsingContextModuleSamples.cs#ClippedScreenshot)]

## Printing to PDF

[!code-csharp[Print to PDF](../../code/modules/BrowsingContextModuleSamples.cs#PrinttoPDF)]

### PDF with Custom Settings

[!code-csharp[PDF with Custom Settings](../../code/modules/BrowsingContextModuleSamples.cs#PDFwithCustomSettings)]

## Handling User Prompts

### Accept Alert/Confirm

[!code-csharp[Accept Alert](../../code/modules/BrowsingContextModuleSamples.cs#AcceptAlert)]

### Dismiss Prompt

[!code-csharp[Dismiss Prompt](../../code/modules/BrowsingContextModuleSamples.cs#DismissPrompt)]

### Enter Text in Prompt

[!code-csharp[Enter Text in Prompt](../../code/modules/BrowsingContextModuleSamples.cs#EnterTextinPrompt)]

## Activation

### Bring Tab to Foreground

[!code-csharp[Activate Tab](../../code/modules/BrowsingContextModuleSamples.cs#ActivateTab)]

## Events

### Navigation Events

[!code-csharp[Navigation Events](../../code/modules/BrowsingContextModuleSamples.cs#NavigationEvents)]

### Context Lifecycle Events

[!code-csharp[Context Lifecycle Events](../../code/modules/BrowsingContextModuleSamples.cs#ContextLifecycleEvents)]

### User Prompt Events

[!code-csharp[User Prompt Events](../../code/modules/BrowsingContextModuleSamples.cs#UserPromptEvents)]

## Common Patterns

### Wait for Page Load Pattern

[!code-csharp[Wait for Page Load Pattern](../../code/modules/BrowsingContextModuleSamples.cs#WaitforPageLoadPattern)]

### Multi-Tab Pattern

[!code-csharp[Multi-Tab Pattern](../../code/modules/BrowsingContextModuleSamples.cs#Multi-TabPattern)]

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

