# Core Concepts

Understanding the fundamental concepts of WebDriverBiDi.NET-Relaxed will help you use the library effectively. This guide covers the key architectural elements and design patterns.

## The BiDiDriver

The `BiDiDriver` class is the central entry point for all WebDriver BiDi operations.

```csharp
// Create a driver with default timeout (infinite)
BiDiDriver driver = new BiDiDriver();

// Create a driver with a specific command timeout
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

// Start the connection
await driver.StartAsync("ws://localhost:9222/session");

// Stop the connection when done
await driver.StopAsync();
```

### Key Responsibilities

- Manages the WebSocket connection to the browser
- Provides access to all protocol modules
- Handles command execution and response correlation
- Dispatches events to registered observers

## Modules

WebDriver BiDi organizes functionality into modules, each representing a specific area of browser control.

### Available Modules

WebDriverBiDi.NET-Relaxed includes the following modules:

| Module | Purpose | Access Property |
|--------|---------|----------------|
| **Browser** | Browser-level operations | `driver.Browser` |
| **BrowsingContext** | Tab/window management and navigation | `driver.BrowsingContext` |
| **Script** | JavaScript execution | `driver.Script` |
| **Network** | Network traffic monitoring and control | `driver.Network` |
| **Input** | User input simulation | `driver.Input` |
| **Log** | Console and browser log access | `driver.Log` |
| **Session** | Session management and subscriptions | `driver.Session` |
| **Storage** | Cookies and storage management | `driver.Storage` |
| **Emulation** | Device and media emulation | `driver.Emulation` |
| **Permissions** | Permission management | `driver.Permissions` |
| **Bluetooth** | Web Bluetooth API control | `driver.Bluetooth` |
| **WebExtension** | Browser extension management | `driver.WebExtension` |
| **Speculation** | Navigation prefetching | `driver.Speculation` |

### Accessing Modules

```csharp
// Access a module through the driver
BrowsingContextModule browsingContext = driver.BrowsingContext;
NetworkModule network = driver.Network;
ScriptModule script = driver.Script;
```

## Commands and Responses

WebDriver BiDi uses a command-response pattern for browser operations.

### Command Structure

All commands follow this pattern:

1. Create a command parameters object
2. Execute the command through the appropriate module
3. Receive a typed response object

```csharp
// 1. Create parameters
NavigateCommandParameters parameters = new NavigateCommandParameters(
    contextId, 
    "https://example.com")
{
    Wait = ReadinessState.Complete
};

// 2. Execute command
NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(parameters);

// 3. Use the result
Console.WriteLine($"Navigated to: {result.Url}");
```

### Command Parameters

- **Mutable**: You can set properties on parameter objects before sending
- **Required Parameters**: Passed through the constructor
- **Optional Parameters**: Set through properties with initializers

```csharp
// Required parameters in constructor
var params = new NavigateCommandParameters(contextId, url);

// Optional parameters via properties
params.Wait = ReadinessState.Complete;
params.TimeoutSeconds = 30;
```

### Command Results

- **Immutable**: Properties are read-only
- **Strongly Typed**: Each command returns a specific result type
- **Error Handling**: Errors throw `WebDriverBiDiException`

```csharp
try
{
    NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(params);
    // Result properties are read-only
    string url = result.Url;
    string navigationId = result.NavigationId;
}
catch (WebDriverBiDiException ex)
{
    Console.WriteLine($"Command failed: {ex.Message}");
}
```

## Events and Observable Events

WebDriver BiDi is event-driven, allowing you to react to browser events as they occur.

### Observable Events

Each event is exposed through an `ObservableEvent<T>` property on the relevant module:

```csharp
// Events on BrowsingContext module
driver.BrowsingContext.OnLoad
driver.BrowsingContext.OnDomContentLoaded
driver.BrowsingContext.OnNavigationStarted

// Events on Network module
driver.Network.OnBeforeRequestSent
driver.Network.OnResponseCompleted

// Events on Log module
driver.Log.OnEntryAdded
```

### Event Subscription

Before receiving events, you must subscribe to them through the Session module:

```csharp
// Create subscription parameters
SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.Log.OnEntryAdded.EventName);
subscribe.Events.Add(driver.Network.OnResponseCompleted.EventName);

// Subscribe to events
await driver.Session.SubscribeAsync(subscribe);
```

### Event Observers

Add observers to handle events:

```csharp
// Add a simple observer
driver.Log.OnEntryAdded.AddObserver((EntryAddedEventArgs e) =>
{
    Console.WriteLine($"Console log: {e.Text}");
});

// Add an async observer
driver.Network.OnBeforeRequestSent.AddObserver(
    async (BeforeRequestSentEventArgs e) =>
    {
        Console.WriteLine($"Request to: {e.Request.Url}");
        await Task.Delay(100); // Can perform async operations
    },
    ObservableEventHandlerOptions.RunHandlerAsynchronously
);
```

### Event Observer Pattern

Observers can be managed and synchronized:

```csharp
// Create an observer with reference
EventObserver<EntryAddedEventArgs> observer = 
    driver.Log.OnEntryAdded.AddObserver((e) => 
    {
        Console.WriteLine(e.Text);
    });

// Set a checkpoint to wait for N events
observer.SetCheckpoint(5); // Wait for 5 events

// Perform operations that trigger events
await driver.BrowsingContext.NavigateAsync(navParams);

// Wait for the checkpoint
bool fulfilled = observer.WaitForCheckpoint(TimeSpan.FromSeconds(10));

// Remove the observer
observer.Unobserve();
```

## Browsing Contexts

A browsing context represents a document environment (tab, window, or iframe).

### Context IDs

Every operation that interacts with a page requires a context ID:

```csharp
// Get all contexts
GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
    new GetTreeCommandParameters());

// Get the first context ID
string contextId = tree.ContextTree[0].BrowsingContextId;

// Navigate in that context
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, url));
```

### Context Tree

Contexts form a tree structure (windows contain iframes):

```csharp
GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
    new GetTreeCommandParameters());

foreach (var context in tree.ContextTree)
{
    Console.WriteLine($"Context: {context.BrowsingContextId}");
    Console.WriteLine($"  URL: {context.Url}");
    Console.WriteLine($"  Children: {context.Children.Count}");
}
```

### Creating Contexts

```csharp
// Create a new tab
CreateCommandParameters createParams = new CreateCommandParameters(
    ContextType.Tab);
CreateCommandResult newContext = await driver.BrowsingContext.CreateAsync(createParams);

string newContextId = newContext.BrowsingContextId;
```

## Remote Values

JavaScript values are represented as `RemoteValue` objects when returned from the browser.

### Value Types

Remote values have a `Type` property indicating their JavaScript type:

- `string`, `number`, `boolean`, `undefined`, `null`
- `object`, `array`, `function`, `promise`
- `node` (DOM elements)
- `window`, `regexp`, `date`, `map`, `set`

### Accessing Values

Use the `ValueAs<T>()` method to convert to .NET types:

```csharp
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters("42", target, true));

if (result is EvaluateResultSuccess success)
{
    RemoteValue remoteValue = success.Result;
    
    // Convert to appropriate .NET type
    long number = remoteValue.ValueAs<long>(); // JavaScript number -> long
    
    // Check the type
    Console.WriteLine($"Type: {remoteValue.Type}"); // "number"
}
```

### Working with DOM Elements

DOM elements have a `SharedId` that allows them to be referenced in subsequent commands:

```csharp
// Get an element
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters("document.querySelector('button')", target, true));

if (result is EvaluateResultSuccess success)
{
    RemoteValue element = success.Result;
    
    // Get node properties
    NodeProperties? nodeProps = element.ValueAs<NodeProperties>();
    Console.WriteLine($"Tag: {nodeProps?.LocalName}");
    
    // Create a reference to use in other commands
    SharedReference elementRef = element.ToSharedReference();
    
    // Use the reference in another script call
    CallFunctionCommandParameters clickParams = new CallFunctionCommandParameters(
        "(element) => element.click()",
        target,
        false);
    clickParams.Arguments.Add(elementRef);
    await driver.Script.CallFunctionAsync(clickParams);
}
```

### Creating Local Values

When passing values to JavaScript, create `LocalValue` instances:

```csharp
CallFunctionCommandParameters params = new CallFunctionCommandParameters(
    "(a, b, c) => a + b + c.length",
    new ContextTarget(contextId),
    true);

// Add arguments as local values
params.Arguments.Add(LocalValue.Number(5));
params.Arguments.Add(LocalValue.Number(10));
params.Arguments.Add(LocalValue.String("hello"));

EvaluateResult result = await driver.Script.CallFunctionAsync(params);
// Result: 20 (5 + 10 + 5)
```

## Async/Await Pattern

All WebDriverBiDi.NET-Relaxed operations are asynchronous.

### Best Practices

```csharp
// ✓ Good: Await async operations
NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(params);

// ✗ Bad: Don't block with .Result or .Wait()
var result = driver.BrowsingContext.NavigateAsync(params).Result; // Can deadlock

// ✓ Good: Use ConfigureAwait(false) in library code
await driver.BrowsingContext.NavigateAsync(params).ConfigureAwait(false);
```

### Parallel Operations

You can execute multiple independent commands in parallel:

```csharp
// Execute multiple navigations in parallel
Task<NavigateCommandResult> nav1 = driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId1, url1));
Task<NavigateCommandResult> nav2 = driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId2, url2));

await Task.WhenAll(nav1, nav2);

Console.WriteLine($"Context 1: {nav1.Result.Url}");
Console.WriteLine($"Context 2: {nav2.Result.Url}");
```

## Error Handling

WebDriver BiDi operations can fail for various reasons.

### WebDriverBiDiException

All protocol errors are wrapped in `WebDriverBiDiException`:

```csharp
try
{
    await driver.BrowsingContext.NavigateAsync(params);
}
catch (WebDriverBiDiException ex)
{
    Console.WriteLine($"BiDi error: {ex.Message}");
    // ex.Message contains the error type and message from the browser
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

### Script Exceptions

JavaScript errors are returned as `EvaluateResultException`:

```csharp
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters("throw new Error('Oops!')", target, true));

if (result is EvaluateResultException exception)
{
    Console.WriteLine($"Script error: {exception.ExceptionDetails.Text}");
    Console.WriteLine($"Line: {exception.ExceptionDetails.LineNumber}");
    Console.WriteLine($"Column: {exception.ExceptionDetails.ColumnNumber}");
}
```

### Timeouts

Commands that exceed the timeout will throw an exception:

```csharp
// Set a 5-second timeout for this driver
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(5));

try
{
    // This will timeout if it takes longer than 5 seconds
    await driver.BrowsingContext.NavigateAsync(params);
}
catch (WebDriverBiDiTimeoutException ex)
{
    Console.WriteLine("Navigation took too long");
}
```

## Immutability Principle

A key design principle: **data from the browser is immutable, data to the browser is mutable**.

### Immutable (From Browser)

```csharp
NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(params);

// ✗ Cannot modify - properties are read-only
// result.Url = "something else"; // Compilation error
```

### Mutable (To Browser)

```csharp
NavigateCommandParameters params = new NavigateCommandParameters(contextId, url);

// ✓ Can modify - properties are settable
params.Wait = ReadinessState.Complete;
params.TimeoutSeconds = 30;
```

## Next Steps

- **[Events and Observables](events-observables.md)**: Deep dive into event handling
- **[Remote Values](remote-values.md)**: Comprehensive guide to JavaScript value handling
- **[Module Guides](modules/browser.md)**: Learn about each module in detail
- **[Common Scenarios](examples/common-scenarios.md)**: See practical examples

## Summary

- `BiDiDriver` is the main entry point
- Modules organize functionality by area
- Commands use parameters (mutable) and return results (immutable)
- Events require subscription and use the observer pattern
- Browsing contexts represent tabs/windows/iframes
- Remote values represent JavaScript data
- All operations are async
- Errors are thrown as `WebDriverBiDiException`

