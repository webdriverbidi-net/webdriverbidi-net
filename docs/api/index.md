# API Reference

Welcome to the WebDriverBiDi.NET-Relaxed API reference documentation.

## Overview

This section contains detailed API documentation for all public types, methods, and properties in WebDriverBiDi.NET-Relaxed. The documentation is automatically generated from XML comments in the source code.

## Namespaces

### WebDriverBiDi

The root namespace containing the main `BiDiDriver` class and core types.

**Key Classes:**
- `BiDiDriver` - Main entry point for WebDriver BiDi operations
- `Module` - Base class for all protocol modules
- `CommandParameters` - Base class for command parameters
- `CommandResult` - Base class for command results
- `ObservableEvent<T>` - Event subscription and notification
- `EventObserver<T>` - Event observation and synchronization

### WebDriverBiDi.Browser

Browser-level operations including user context and window management.

**Key Classes:**
- `BrowserModule` - Browser module implementation
- `ClientWindowInfo` - Browser window information
- `UserContextInfo` - User context information

### WebDriverBiDi.BrowsingContext

Tab, window, and iframe management, navigation, and page interaction.

**Key Classes:**
- `BrowsingContextModule` - Browsing context module implementation
- `BrowsingContextInfo` - Browsing context information
- `NavigateCommandParameters` - Navigation parameters
- `NavigateCommandResult` - Navigation results
- `CaptureScreenshotCommandParameters` - Screenshot parameters

### WebDriverBiDi.Script

JavaScript execution, preload scripts, and value marshalling.

**Key Classes:**
- `ScriptModule` - Script module implementation
- `EvaluateCommandParameters` - Script evaluation parameters
- `CallFunctionCommandParameters` - Function call parameters
- `RemoteValue` - JavaScript value from browser
- `LocalValue` - JavaScript value to browser
- `AddPreloadScriptCommandParameters` - Preload script parameters

### WebDriverBiDi.Network

Network traffic monitoring, interception, and modification.

**Key Classes:**
- `NetworkModule` - Network module implementation
- `RequestData` - HTTP request information
- `ResponseData` - HTTP response information
- `AddInterceptCommandParameters` - Network intercept parameters
- `ProvideResponseCommandParameters` - Custom response parameters

### WebDriverBiDi.Input

User input simulation including mouse, keyboard, touch, and wheel events.

**Key Classes:**
- `InputModule` - Input module implementation
- `PerformActionsCommandParameters` - Action parameters
- `PointerSource` - Mouse/touch action source
- `KeySource` - Keyboard action source

### WebDriverBiDi.Log

Console log and browser error monitoring.

**Key Classes:**
- `LogModule` - Log module implementation
- `LogEntry` - Log entry base class
- `ConsoleLogEntry` - Console log entry
- `EntryAddedEventArgs` - Log entry event arguments

### WebDriverBiDi.Session

Session management and event subscriptions.

**Key Classes:**
- `SessionModule` - Session module implementation
- `SubscribeCommandParameters` - Subscription parameters
- `StatusCommandParameters` - Status check parameters

### WebDriverBiDi.Storage

Cookie and storage management.

**Key Classes:**
- `StorageModule` - Storage module implementation
- `GetCookiesCommandParameters` - Cookie retrieval parameters
- `SetCookieCommandParameters` - Cookie setting parameters
- `PartialCookie` - Cookie definition

### WebDriverBiDi.Emulation

Device and media emulation.

**Key Classes:**
- `EmulationModule` - Emulation module implementation
- Viewport settings
- Media feature emulation

### WebDriverBiDi.Permissions

Browser permission management.

**Key Classes:**
- `PermissionsModule` - Permissions module implementation
- Permission descriptor types

### WebDriverBiDi.Bluetooth

Web Bluetooth API control.

**Key Classes:**
- `BluetoothModule` - Bluetooth module implementation

### WebDriverBiDi.WebExtension

Browser extension management.

**Key Classes:**
- `WebExtensionModule` - Web extension module implementation

### WebDriverBiDi.Speculation

Navigation prefetching and speculation.

**Key Classes:**
- `SpeculationModule` - Speculation module implementation

### WebDriverBiDi.Protocol

Low-level protocol communication types.

**Key Classes:**
- `Transport` - WebSocket communication handler
- `Command` - Command representation
- `Message` - Protocol message base class

## Using the API Reference

### Generating Documentation

To generate the full API documentation locally:

```bash
# Install DocFX if not already installed
dotnet tool install -g docfx

# Generate documentation
cd docs
docfx build docfx.json

# Serve locally
docfx serve _site
```

Then open your browser to `http://localhost:8080`.

### Documentation Conventions

#### Command Parameters

All command parameter classes follow this pattern:

```csharp
public class CommandNameCommandParameters : CommandParameters<CommandNameCommandResult>
{
    // Required parameters in constructor
    public CommandNameCommandParameters(string requiredParam);
    
    // Optional parameters as properties
    public string? OptionalParam { get; set; }
}
```

#### Command Results

All command result classes follow this pattern:

```csharp
public class CommandNameCommandResult : CommandResult
{
    // Properties are read-only (immutable)
    public string ResultProperty { get; }
}
```

#### Event Arguments

All event argument classes inherit from `WebDriverBiDiEventArgs`:

```csharp
public class EventNameEventArgs : WebDriverBiDiEventArgs
{
    // Properties are read-only (immutable)
    public string EventData { get; }
}
```

### Common Patterns in API

#### Async Methods

All methods that communicate with the browser are async:

```csharp
public async Task<TResult> MethodNameAsync(TParameters parameters)
```

#### Module Access

All modules are accessed through the `BiDiDriver`:

```csharp
BiDiDriver driver = new BiDiDriver();
BrowsingContextModule browsingContext = driver.BrowsingContext;
ScriptModule script = driver.Script;
NetworkModule network = driver.Network;
```

#### Event Subscription

Events use the observable pattern:

```csharp
// Access observable event
ObservableEvent<TEventArgs> observableEvent = module.OnEventName;

// Add observer
EventObserver<TEventArgs> observer = observableEvent.AddObserver(handler);

// Subscribe through Session
await driver.Session.SubscribeAsync(subscribeParams);
```

## API Design Principles

### Immutability

- **Response objects** are immutable - properties are read-only
- **Command parameters** are mutable - properties are settable
- This prevents accidental modification of data from the browser

### Type Safety

- Strong typing throughout the API
- Generic type parameters for command/result correlation
- Enumerations for fixed value sets

### Async/Await

- All I/O operations are asynchronous
- No blocking operations
- Proper `ConfigureAwait` usage in library code

### Error Handling

- Protocol errors throw `WebDriverBiDiException`
- Script errors return `EvaluateResultException`
- Timeouts throw `WebDriverBiDiException`

## Examples

See the [Examples](../articles/examples/common-scenarios.md) section for practical usage of the API.

## Contributing

If you find errors in the API documentation or have suggestions for improvements:

1. Check the XML comments in the source code
2. Submit an issue on [GitHub](https://github.com/hardkoded/webdriverbidi-net-relaxed/issues)
3. Submit a pull request with improvements

## Additional Resources

- [Getting Started Guide](../articles/getting-started.md)
- [Core Concepts](../articles/core-concepts.md)
- [Module Guides](../articles/modules/browsing-context.md)
- [GitHub Repository](https://github.com/hardkoded/webdriverbidi-net-relaxed)
