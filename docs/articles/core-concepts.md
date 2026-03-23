# Core Concepts

Understanding the fundamental concepts of WebDriverBiDi.NET will help you use the library effectively. This guide covers the key architectural elements and design patterns.

## The BiDiDriver

The `BiDiDriver` class is the central entry point for all WebDriver BiDi operations.

[!code-csharp[Driver Creation and Lifecycle](../code/core-concepts/CoreConceptsSamples.cs#DriverCreationandLifecycle)]

### Key Responsibilities

- Manages the connection to the browser (WebSocket or Pipes)
- Provides access to all protocol modules
- Handles command execution and response correlation
- Dispatches events to registered observers

### Driver Lifecycle

The `BiDiDriver` has a well-defined lifecycle with important timing restrictions:

[!code-csharp[Driver Lifecycle](../code/core-concepts/CoreConceptsSamples.cs#DriverLifecycle)]

#### IsStarted Property

The `IsStarted` property indicates whether the driver is currently connected:

[!code-csharp[Driver Lifecycle with Check](../code/core-concepts/CoreConceptsSamples.cs#DriverLifecyclewithCheck)]

**Use Cases:**
- Check driver state before operations
- Verify connection before executing commands
- Safe disposal patterns

#### Timing Restrictions

**Critical:** Module and event registration must happen BEFORE calling `StartAsync()`:

[!code-csharp[Timing Restrictions - Correct](../code/core-concepts/CoreConceptsSamples.cs#TimingRestrictions-Correct)]

[!code-csharp[Timing Restrictions - Wrong](../code/core-concepts/CoreConceptsSamples.cs#TimingRestrictions-Wrong)]

**Why This Restriction Exists:**
- Ensures all handlers are in place before events start flowing
- Prevents race conditions
- Maintains predictable initialization order

**Thread Safety:**

The `RegisterModule` method is thread-safe and can be called concurrently from multiple threads:

[!code-csharp[Thread Safe Registration](../code/core-concepts/CoreConceptsSamples.cs#ThreadSafeRegistration)]

Thread safety is enforced using an internal lock that ensures the check against `IsStarted` and the module addition are atomic operations. However, the timing restriction still applies—all registrations must complete before `StartAsync()` is called.

#### Command Timeout Configuration

Configure command timeout when creating the driver:

[!code-csharp[Command Timeout Configuration](../code/core-concepts/CoreConceptsSamples.cs#CommandTimeoutConfiguration)]

The timeout applies to all command executions by default, but can be overridden per-command. **Prefer the `timeoutOverride` parameter on module methods** (e.g., `NavigateAsync(parameters, TimeSpan.FromSeconds(60))`) as the standard way to set per-command timeouts:

[!code-csharp[Per-Command Timeout Override](../code/core-concepts/CoreConceptsSamples.cs#Per-CommandTimeoutOverride)]

For timeout patterns (returning `null` instead of throwing, custom retry logic), see [Error Handling - Timeout Handling](advanced/error-handling.md#timeout-handling).

#### Proper Disposal

Always dispose of the driver when done:

[!code-csharp[Proper Disposal - try/finally](../code/core-concepts/CoreConceptsSamples.cs#ProperDisposal-tryfinally)]

[!code-csharp[Proper Disposal - await using](../code/core-concepts/CoreConceptsSamples.cs#ProperDisposal-awaitusing)]

#### Complete Lifecycle Example

[!code-csharp[Complete Lifecycle Example](../code/core-concepts/CoreConceptsSamples.cs#CompleteLifecycleExample)]

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
| **UserAgentClientHints** | Emulates browser, platform, and device reporting | `driver.UserAgentClientHints` |

### Accessing Modules

[!code-csharp[Accessing Modules](../code/core-concepts/CoreConceptsSamples.cs#AccessingModules)]

## Commands and Responses

WebDriver BiDi uses a command-response pattern for browser operations.

### Command Structure

All commands follow this pattern:

1. Create a command parameters object
2. Execute the command through the appropriate module
3. Receive a typed response object

[!code-csharp[Command Structure](../code/core-concepts/CoreConceptsSamples.cs#CommandStructure)]

### Command Parameters

- **Mutable**: You can set properties on parameter objects before sending
- **Required Parameters**: Passed through the constructor
- **Optional Parameters**: Set through properties with initializers

[!code-csharp[Command Parameters](../code/core-concepts/CoreConceptsSamples.cs#CommandParameters)]

### When Parameters Are Optional

Some module commands accept an optional `CommandParameters` object—you can pass `null` or omit it, and the library will use default parameters. Other commands always require a parameters object. The rule depends on whether the command has a "reset" capability.

**Optional parameters** (no required properties, no reset property):

These commands allow omitting parameters when you want default behavior:

[!code-csharp[Optional Parameters](../code/core-concepts/CoreConceptsSamples.cs#OptionalParameters)]

**Required parameters** (has a reset property):

Some commands can reset a value on the remote end to its original state. These commands **always require** a parameters object so the intent is explicit. Passing no parameters would be ambiguous—are you setting a value or resetting it?

[!code-csharp[Required Parameters](../code/core-concepts/CoreConceptsSamples.cs#RequiredParameters)]

**Commands with optional parameters** include: `Browser.CloseAsync`, `Browser.CreateUserContextAsync`, `Browser.GetClientWindowsAsync`, `Browser.GetUserContextsAsync`, `BrowsingContext.GetTreeAsync`, `Script.GetRealmsAsync`, `Session.EndAsync`, `Session.StatusAsync`, `Storage.DeleteCookiesAsync`, `Storage.GetCookiesAsync`.

**Commands that require parameters** (because they have reset properties) include: `UserAgentClientHints.SetClientHintsOverrideAsync`, `Browser.SetDownloadBehaviorAsync`, `BrowsingContext.SetViewportAsync`, and all Emulation `Set*OverrideAsync` commands (e.g., `SetUserAgentOverrideAsync`, `SetGeolocationOverrideAsync`).

### Command Results

- **Immutable**: Properties are read-only
- **Strongly Typed**: Each command returns a specific result type
- **Error Handling**: Errors throw `WebDriverBiDiException`

[!code-csharp[Command Results Error Handling](../code/core-concepts/CoreConceptsSamples.cs#CommandResultsErrorHandling)]

## Events and Observable Events

WebDriver BiDi is event-driven, allowing you to react to browser events as they occur.

### Observable Events

Each event is exposed through an `ObservableEvent<T>` property on the relevant module:

[!code-csharp[Observable Event Names](../code/core-concepts/CoreConceptsSamples.cs#ObservableEventNames)]

### Event Subscription

Before receiving events, you must subscribe to them through the Session module:

[!code-csharp[Event Subscription](../code/core-concepts/CoreConceptsSamples.cs#EventSubscription)]

### Event Observers

Add observers to handle events:

[!code-csharp[Event Observers](../code/core-concepts/CoreConceptsSamples.cs#EventObservers)]

### Event Observer Pattern

Observers can be managed and synchronized:

[!code-csharp[Event Observer Pattern](../code/core-concepts/CoreConceptsSamples.cs#EventObserverPattern)]

## Browsing Contexts

A browsing context represents a document environment (tab, window, or iframe).

### Context IDs

Every operation that interacts with a page requires a context ID:

[!code-csharp[Context IDs](../code/core-concepts/CoreConceptsSamples.cs#ContextIDs)]

### Context Tree

Contexts form a tree structure (windows contain iframes):

[!code-csharp[Context Tree](../code/core-concepts/CoreConceptsSamples.cs#ContextTree)]

### Creating Contexts

[!code-csharp[Creating Contexts](../code/core-concepts/CoreConceptsSamples.cs#CreatingContexts)]

## Remote Values

JavaScript values are represented as `RemoteValue` objects when returned from the browser.

### Value Types

Remote values have a `Type` property indicating their JavaScript type:

- `string`, `number`, `boolean`, `undefined`, `null`
- `object`, `array`, `function`, `promise`
- `node` (DOM elements)
- `window`, `regexp`, `date`, `map`, `set`

### Accessing Values

Pattern match or use `As<T>()` to cast to the concrete type and access the `Value` property:

[!code-csharp[Accessing Values](../code/core-concepts/CoreConceptsSamples.cs#AccessingValues)]

### Working with DOM Elements

DOM elements have a `SharedId` that allows them to be referenced in subsequent commands:

[!code-csharp[Working with DOM Elements](../code/core-concepts/CoreConceptsSamples.cs#WorkingwithDOMElements)]

### Creating Local Values

When passing values to JavaScript, create `LocalValue` instances:

[!code-csharp[Creating Local Values](../code/core-concepts/CoreConceptsSamples.cs#CreatingLocalValues)]

## Async/Await Pattern

All WebDriverBiDi.NET operations are asynchronous.

### Best Practices

[!code-csharp[Best Practices Await](../code/core-concepts/CoreConceptsSamples.cs#BestPracticesAwait)]

### Parallel Operations

You can execute multiple independent commands in parallel:

[!code-csharp[Parallel Operations](../code/core-concepts/CoreConceptsSamples.cs#ParallelOperations)]

## Error Handling

WebDriver BiDi operations can fail for various reasons.

### WebDriverBiDiException

All protocol errors are wrapped in `WebDriverBiDiException`:

[!code-csharp[WebDriverBiDiException Handling](../code/core-concepts/CoreConceptsSamples.cs#WebDriverBiDiExceptionHandling)]

### Script Exceptions

JavaScript errors are returned as `EvaluateResultException`:

[!code-csharp[Script Exceptions](../code/core-concepts/CoreConceptsSamples.cs#ScriptExceptions)]

### Timeouts

Commands that exceed the timeout will throw an exception. Use the `timeoutOverride` parameter on module methods to set per-command timeouts:

[!code-csharp[Timeout Handling](../code/core-concepts/CoreConceptsSamples.cs#TimeoutHandling)]

## Immutability Principle

A key design principle: **data from the browser is immutable, data to the browser is mutable**.

### Immutable (From Browser)

[!code-csharp[Immutable Result](../code/core-concepts/CoreConceptsSamples.cs#ImmutableResult)]

### Mutable (To Browser)

[!code-csharp[Mutable Parameters](../code/core-concepts/CoreConceptsSamples.cs#MutableParameters)]

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

[!code-csharp[Advanced Abstractions](../code/core-concepts/CoreConceptsSamples.cs#AdvancedAbstractions)]

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

[!code-csharp[Creating a Custom Module](../code/core-concepts/CoreConceptsCustomModuleSamples.cs#CreatingaCustomModule)]

**Registering and Using a Custom Module:**

[!code-csharp[Registering and Using a Custom Module](../code/core-concepts/CoreConceptsSamples.cs#RegisteringandUsingaCustomModule)]

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

[!code-csharp[Custom JSON Type Resolvers](../code/core-concepts/CoreConceptsCustomModuleSamples.cs#CustomJSONTypeResolvers)]

[!code-csharp[Register Custom Resolver Before Starting](../code/core-concepts/CoreConceptsSamples.cs#RegisterCustomResolverBeforeStarting)]

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

