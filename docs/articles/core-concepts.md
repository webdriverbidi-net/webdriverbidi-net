# Core Concepts

Understanding the fundamental concepts of WebDriverBiDi.NET will help you use the library effectively. This guide covers the key architectural elements and design patterns.

## The BiDiDriver

The `BiDiDriver` class is the central entry point for all WebDriver BiDi operations.

```csharp
// Create a driver with default timeout (60 seconds)
BiDiDriver driver = new BiDiDriver();

// Create a driver with a specific command timeout
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

// Start the connection
await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

// Stop the connection when done
await driver.StopAsync();
```

### Key Responsibilities

- Manages the connection to the browser (WebSocket or Pipes)
- Provides access to all protocol modules
- Handles command execution and response correlation
- Dispatches events to registered observers

### Driver Lifecycle

The `BiDiDriver` has a well-defined lifecycle with important timing restrictions:

```csharp
// 1. Create driver
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

// 2. Register modules and event handlers BEFORE starting
driver.RegisterModule(customModule);
driver.Log.OnEntryAdded.AddObserver((e) => Console.WriteLine(e.Text));

// 3. Start the driver
await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

// 4. Check if started
if (driver.IsStarted)
{
    // Execute commands...
    await driver.BrowsingContext.NavigateAsync(navParams);
}

// 5. Stop when done
await driver.StopAsync();
```

#### IsStarted Property

The `IsStarted` property indicates whether the driver is currently connected:

```csharp
if (!driver.IsStarted)
{
    await driver.StartAsync(webSocketUrl);
}

// Execute commands only when started
if (driver.IsStarted)
{
    await driver.BrowsingContext.NavigateAsync(navParams);
}
```

**Use Cases:**
- Check driver state before operations
- Verify connection before executing commands
- Safe disposal patterns

#### Timing Restrictions

**Critical:** Module and event registration must happen BEFORE calling `StartAsync()`:

```csharp
// ✅ CORRECT: Register before starting
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

driver.RegisterModule(new CustomModule(driver));
driver.Log.OnEntryAdded.AddObserver((e) => Console.WriteLine(e.Text));

await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

// ❌ WRONG: Cannot register after starting
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

// This will throw an exception!
driver.RegisterModule(new CustomModule(driver));
driver.Log.OnEntryAdded.AddObserver((e) => Console.WriteLine(e.Text));
```

**Why This Restriction Exists:**
- Ensures all handlers are in place before events start flowing
- Prevents race conditions
- Maintains predictable initialization order

**Thread Safety:**

The `RegisterModule` method is thread-safe and can be called concurrently from multiple threads:

```csharp
// This is safe - concurrent registration is handled properly
Parallel.Invoke(
    () => driver.RegisterModule(customModule1),
    () => driver.RegisterModule(customModule2)
);
```

Thread safety is enforced using an internal lock that ensures the check against `IsStarted` and the module addition are atomic operations. However, the timing restriction still applies—all registrations must complete before `StartAsync()` is called.

#### Command Timeout Configuration

Configure command timeout when creating the driver:

```csharp
// Default timeout (60 seconds)
BiDiDriver driver = new BiDiDriver();

// Custom timeout (30 seconds)
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

// Short timeout for fast operations
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(5));

// Long timeout for slow operations
BiDiDriver driver = new BiDiDriver(TimeSpan.FromMinutes(10));
```

The timeout applies to all command executions by default, but can be overridden per-command:

```csharp
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

// Use driver's default timeout (30 seconds)
await driver.BrowsingContext.NavigateAsync(navParams);

// Override with custom timeout for this command
await driver.ExecuteCommandAsync<NavigateCommandResult>(
    navParams,
    TimeSpan.FromSeconds(60)  // Use 60 seconds for this command
);
```

#### Proper Disposal

Always dispose of the driver when done:

```csharp
// Using statement (recommended)
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
try
{
    await driver.StartAsync(webSocketUrl);
    // Use driver...
}
finally
{
    await driver.StopAsync();
}

// Or with async disposal
await using BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.StartAsync(webSocketUrl);
// Use driver...
// Automatically disposed at end of scope
```

#### Complete Lifecycle Example

```csharp
using WebDriverBiDi;
using WebDriverBiDi.Protocol;

// Create driver with custom timeout
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

try
{
    // Register event handlers before starting
    driver.Log.OnEntryAdded.AddObserver((e) =>
    {
        Console.WriteLine($"[{e.Level}] {e.Text}");
    });

    driver.BrowsingContext.OnLoad.AddObserver((e) =>
    {
        Console.WriteLine($"Page loaded: {e.Url}");
    });

    // Start the driver
    await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");

    // Verify driver is started
    if (!driver.IsStarted)
    {
        throw new InvalidOperationException("Driver failed to start");
    }

    // Subscribe to events
    SubscribeCommandParameters subscribe = new(
        [
            driver.Log.OnEntryAdded.EventName,
            driver.BrowsingContext.OnLoad.EventName,
        ]
    );
    await driver.Session.SubscribeAsync(subscribe);

    // Execute commands
    GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
        new GetTreeCommandParameters());
    string contextId = tree.ContextTree[0].BrowsingContextId;

    NavigateCommandParameters navParams = new(contextId, "https://example.com")
    {
        Wait = ReadinessState.Complete
    };
    await driver.BrowsingContext.NavigateAsync(navParams);
}
finally
{
    // Always stop the driver
    if (driver.IsStarted)
    {
        await driver.StopAsync();
    }
}
```

## Modules

WebDriver BiDi organizes functionality into modules, each representing a specific area of browser control.

### Available Modules

WebDriverBiDi.NET includes the following modules:

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

// Timeout overrides are supplied when executing the command
await driver.BrowsingContext.NavigateAsync(
    params,
    TimeSpan.FromSeconds(30));
```

### When Parameters Are Optional

Some module commands accept an optional `CommandParameters` object—you can pass `null` or omit it, and the library will use default parameters. Other commands always require a parameters object. The rule depends on whether the command has a "reset" capability.

**Optional parameters** (no required properties, no reset property):

These commands allow omitting parameters when you want default behavior:

```csharp
// ✅ CORRECT: Parameters optional—use defaults
GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
// Or equivalently:
GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(null);

// ✅ CORRECT: Same for other optional-parameter commands
StatusCommandResult status = await driver.Session.StatusAsync(null);
GetCookiesCommandResult cookies = await driver.Storage.GetCookiesAsync(null);
```

**Required parameters** (has a reset property):

Some commands can reset a value on the remote end to its original state. These commands **always require** a parameters object so the intent is explicit. Passing no parameters would be ambiguous—are you setting a value or resetting it?

```csharp
// ✅ CORRECT: Use the reset property when resetting
await driver.UserAgentClientHints.SetClientHintsOverrideAsync(
    SetClientHintsOverrideCommandParameters.ResetClientHintsOverride);

// ✅ CORRECT: Pass explicit parameters when setting
SetClientHintsOverrideCommandParameters setParams = new SetClientHintsOverrideCommandParameters();
setParams.ClientHints = new ClientHintsMetadata
{
    Brands = new List<BrandVersion>() { new BrandVersion("MyBrowser", "120.0") }
};
await driver.UserAgentClientHints.SetClientHintsOverrideAsync(setParams);

// ❌ WRONG: SetClientHintsOverrideAsync always requires parameters
// The command name alone doesn't indicate whether you're setting or resetting
// await driver.UserAgentClientHints.SetClientHintsOverrideAsync(null);  // Not allowed
```

**Commands with optional parameters** include: `Browser.CloseAsync`, `Browser.CreateUserContextAsync`, `Browser.GetClientWindowsAsync`, `Browser.GetUserContextsAsync`, `BrowsingContext.GetTreeAsync`, `Script.GetRealmsAsync`, `Session.EndAsync`, `Session.StatusAsync`, `Storage.DeleteCookiesAsync`, `Storage.GetCookiesAsync`.

**Commands that require parameters** (because they have reset properties) include: `UserAgentClientHints.SetClientHintsOverrideAsync`, `Browser.SetDownloadBehaviorAsync`, `BrowsingContext.SetViewportAsync`, and all Emulation `Set*OverrideAsync` commands (e.g., `SetUserAgentOverrideAsync`, `SetGeolocationOverrideAsync`).

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
SubscribeCommandParameters subscribe = new SubscribeCommandParameters(
    [
        driver.Log.OnEntryAdded.EventName,
        driver.Network.OnResponseCompleted.EventName,
    ]
);

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
bool fulfilled = await observer.WaitForCheckpointAsync(TimeSpan.FromSeconds(10));

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
    CreateType.Tab);
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
    new EvaluateCommandParameters("42", new ContextTarget(contextId), true));

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
// Get a script target against which to run JavaScript
Target target = new ContextTarget(contextId);

// Get an element
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "document.querySelector('button')",
        target,
        true));

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

All WebDriverBiDi.NET operations are asynchronous.

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

// Timeout overrides are supplied when executing the command
await driver.BrowsingContext.NavigateAsync(
    params,
    TimeSpan.FromSeconds(30));
```

## Advanced Topics

> **⚠️ Note for Most Users:** The features described in this section are for advanced scenarios and library developers building on top of WebDriverBiDi.NET (such as Selenium, Puppeteer, or Playwright maintainers). **Most application developers will never need these features.** The standard `BiDiDriver` class with its built-in modules covers the vast majority of use cases.
>
> **Skip this section if you are:**
> - Building a typical browser automation application
> - Using WebDriverBiDi.NET for testing or web scraping
> - Learning the library for the first time
>
> **Read this section if you are:**
> - Building a higher-level automation framework or library
> - Extending the protocol with custom modules
> - Implementing protocol features not yet in the library
> - Need fine-grained control over serialization (AOT scenarios)

### Advanced Abstractions

For normal application code, use the concrete `BiDiDriver` class directly.

```csharp
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.StartAsync(webSocketUrl);
```

That is the intended experience for almost every consumer of this library. Most users should never need to reference any interface type directly.

For advanced framework, testing, and extensibility scenarios, `BiDiDriver` also implements three focused interfaces:

| Interface | Purpose | Typical advanced use |
|----------|---------|----------------------|
| `IBiDiCommandExecutor` | Core lifecycle and command execution | Custom modules, test doubles, framework internals that only need to start/stop the driver, execute commands, or register protocol events |
| `IBiDiDriverConfiguration` | Pre-start extensibility hooks | Registering custom modules and additional JSON type resolvers before `StartAsync()` |
| `IBiDiDriverEvents` | Driver observability and error-behavior configuration | Subscribing to top-level driver events and adjusting transport error behavior |

The hierarchy is intentionally split by capability rather than by end-user workflow:

- `BiDiDriver` is the primary type for applications.
- `IBiDiCommandExecutor` is the narrow execution surface used by modules and low-level abstractions.
- `IBiDiDriverConfiguration` covers advanced pre-start customization.
- `IBiDiDriverEvents` covers top-level events and transport error behavior.

If you are building a higher-level library on top of WebDriverBiDi.NET, choose the narrowest interface that matches the capability you need. If you are writing application code, ignore the interfaces and use `BiDiDriver`.

### Custom Modules

Custom modules allow you to extend WebDriverBiDi.NET with protocol features not yet implemented in the library, or to add proprietary browser-specific extensions.

> **⚠️ Advanced Feature:** Custom modules are only needed when:
> - Implementing cutting-edge protocol features before they're added to the library
> - Supporting browser-specific extensions to the WebDriver BiDi protocol
> - Building a framework that needs to expose additional capabilities
>
> **Most users should use the built-in modules** (Browser, BrowsingContext, Network, Script, etc.), which cover all standard protocol features.

**Creating a Custom Module:**

```csharp
using WebDriverBiDi;

public class CustomModule : Module
{
    // "custom" is the protocol module name
    public const string CustomModuleName = "custom";

    public CustomModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
    }

    public override string ModuleName => CustomModuleName;

    // Define custom commands
    public async Task<CustomCommandResult> MyCustomCommandAsync(
        CustomCommandParameters parameters)
    {
        return await this.Driver.ExecuteCommandAsync<CustomCommandResult>(
            parameters);
    }

    // Define custom events
    public ObservableEvent<CustomEventArgs> OnCustomEvent { get; } =
        new ObservableEvent<CustomEventArgs>("custom.eventOccurred");
}

// Command parameters (mutable - sent to browser)
public class CustomCommandParameters : CommandParameters<CustomCommandResult>
{
    public CustomCommandParameters()
        : base("custom.myCommand")  // Protocol method name
    {
    }

    public string CustomProperty { get; set; }
}

// Command result (immutable - received from browser)
public class CustomCommandResult : CommandResult
{
    public string ResultData { get; internal set; }
}

// Event arguments (immutable - received from browser)
public class CustomEventArgs : WebDriverBiDiEventArgs
{
    public string EventData { get; internal set; }
}
```

**Registering and Using a Custom Module:**

```csharp
// Create driver
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

// Register custom module BEFORE starting
CustomModule customModule = new CustomModule(driver);
driver.RegisterModule(customModule);

// Register custom event
driver.RegisterEvent<CustomEventArgs>(
    customModule.OnCustomEvent.EventName,
    async (eventInfo) =>
    {
        await customModule.OnCustomEvent.NotifyObserversAsync(eventInfo.ToEventArgs<CustomEventArgs>());
    });

// NOW start the driver
await driver.StartAsync(webSocketUrl);

// Use custom module like built-in modules
CustomCommandParameters params = new CustomCommandParameters
{
    CustomProperty = "value"
};
CustomCommandResult result = await customModule.MyCustomCommandAsync(params);
```

**Important Considerations:**
- Custom modules must be registered **before** `StartAsync()`
- The protocol method names must match what the browser expects
- JSON serialization must align with the protocol specification
- See [Custom Modules Guide](advanced/custom-modules.md) for complete details

### Custom JSON Type Resolvers (AOT Scenarios)

For ahead-of-time (AOT) compilation scenarios where reflection-based JSON serialization is unavailable, you can register custom `IJsonTypeInfoResolver` instances.

> **⚠️ Specialized Feature:** This is only needed for:
> - Native AOT deployment (e.g., NativeAOT in .NET 7+)
> - Custom module types that need explicit serialization metadata
> - Environments where reflection is restricted or disabled
>
> **Most users can ignore this** - the library handles JSON serialization automatically using reflection when available.

**Example:**

```csharp
using System.Text.Json.Serialization.Metadata;

// Define JSON source generation context for custom types
[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(CustomCommandParameters))]
[JsonSerializable(typeof(CustomCommandResult))]
[JsonSerializable(typeof(CustomEventArgs))]
internal partial class CustomJsonContext : JsonSerializerContext
{
}

// Register the custom resolver BEFORE starting
BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
await driver.RegisterTypeInfoResolver(CustomJsonContext.Default);

await driver.StartAsync(webSocketUrl);
```

**When This is Required:**
- Publishing with `<PublishAot>true</PublishAot>` in your .csproj
- Using custom modules with custom parameter/result types
- Running in restricted environments (iOS, WebAssembly, etc.)

**When You Don't Need This:**
- Regular .NET applications using JIT compilation
- Using only the built-in modules and types
- Any scenario where reflection is available (the default)

See [AOT Compatibility Guide](advanced/aot-compatibility.md) for complete details on AOT deployment.

### When to Use These Advanced Features

Use this decision tree to determine if you need these advanced features:

```
Are you building a higher-level framework on top of WebDriverBiDi.NET?
├─ YES → You may use custom modules, custom type resolvers, or one of the advanced capability interfaces internally
└─ NO  → Use BiDiDriver directly

Does the built-in library support the protocol feature you need?
├─ YES → Use the built-in modules (Browser, Network, etc.)
└─ NO  → You might need a custom module

Are you deploying with Native AOT compilation?
├─ YES → You might need custom type resolvers
└─ NO  → Reflection-based serialization works automatically

Are you implementing browser-specific extensions?
├─ YES → You need custom modules and possibly custom events
└─ NO  → Use the standard modules
```

**For almost all users:** You don't need any of these features. Use `BiDiDriver` with the built-in modules and you're set.

**For framework developers:** These features provide the extensibility needed to build rich automation libraries while maintaining type safety and performance.

## Next Steps

- **[Quick Reference](quick-reference.md)**: Cheat sheet of common commands
- **[API Design Guide](advanced/api-design.md)**: Parameter patterns, timeouts, versioning
- **[Events and Observables](events-observables.md)**: Deep dive into event handling
- **[Remote Values](remote-values.md)**: Comprehensive guide to JavaScript value handling
- **[Module Guides](modules/browser.md)**: Learn about each module in detail
- **[Common Scenarios](examples/common-scenarios.md)**: See practical examples
- **[Custom Modules Guide](advanced/custom-modules.md)**: Complete guide to creating custom modules (advanced)
- **[AOT Compatibility](advanced/aot-compatibility.md)**: Native AOT deployment guide (advanced)

## Summary

- `BiDiDriver` is the main entry point
- Modules organize functionality by area
- Commands use parameters (mutable) and return results (immutable)
- Events require subscription and use the observer pattern
- Browsing contexts represent tabs/windows/iframes
- Remote values represent JavaScript data
- All operations are async
- Errors are thrown as `WebDriverBiDiException`

