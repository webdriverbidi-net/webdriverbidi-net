# WebDriverBiDi.NET-Relaxed Documentation

Welcome to the official documentation for **WebDriverBiDi.NET-Relaxed**, a comprehensive .NET client library for the WebDriver BiDi protocol.

## What is WebDriverBiDi.NET-Relaxed?

WebDriverBiDi.NET-Relaxed is a low-level .NET implementation of the [W3C WebDriver BiDi protocol specification](https://w3c.github.io/webdriver-bidi/). It provides a robust foundation for browser automation by enabling bidirectional communication between your .NET application and web browsers through WebSocket connections.

The library is built on .NET Standard 2.0, ensuring broad compatibility across .NET Framework, .NET Core, and modern .NET versions.

## Key Features

- **Full Protocol Support**: Complete implementation of the WebDriver BiDi protocol specification
- **Extended Modules**: Support for additional W3C specifications including:
  - [Permissions API](https://www.w3.org/TR/permissions/)
  - [Web Bluetooth](https://webbluetoothcg.github.io/web-bluetooth/)
  - [Navigation Speculation (Prefetch)](https://wicg.github.io/nav-speculation/prefetch.html)
- **Event-Driven Architecture**: Asynchronous event handling for browser events
- **Type-Safe API**: Strongly-typed commands and responses with full IntelliSense support
- **JSON Serialization**: Built on System.Text.Json for efficient communication
- **Extensible Design**: Module-based architecture allows for custom extensions

## What Can You Do?

WebDriverBiDi.NET-Relaxed enables sophisticated browser automation scenarios:

- 📝 **Console Logging**: Capture JavaScript console messages and errors
- 🌐 **Network Monitoring**: Intercept and modify network requests and responses
- 🖱️ **User Interactions**: Simulate mouse, keyboard, and touch inputs
- 📜 **Script Execution**: Execute JavaScript with full access to page context
- 🔄 **Navigation Events**: Track page loads, redirects, and navigation lifecycle
- 🎭 **Preload Scripts**: Inject JavaScript before any page scripts execute
- 🔐 **Permissions Control**: Manage browser permissions programmatically
- 💾 **Storage Management**: Interact with cookies, local storage, and session storage

## Quick Example

```csharp
using WebDriverBiDi;

// Connect to a browser with WebDriver BiDi enabled
BiDiDriver driver = new(TimeSpan.FromSeconds(10));
await driver.StartAsync("ws://localhost:9222");

// Get the active browsing context
var tree = await driver.BrowsingContext.GetTreeAsync(new());
string contextId = tree.ContextTree[0].BrowsingContextId;

// Navigate to a webpage
await driver.BrowsingContext.NavigateAsync(
    new(contextId, "https://example.com") 
    { 
        Wait = ReadinessState.Complete 
    });

// Execute JavaScript and get results
var result = await driver.Script.EvaluateAsync(
    new("document.title", new ContextTarget(contextId), true));

await driver.StopAsync();
```

## Documentation Structure

### Getting Started
- [Installation and Setup](articles/getting-started.md)
- [Your First WebDriverBiDi Application](articles/first-application.md)
- [Browser Setup Guide](articles/browser-setup.md)

### Core Concepts
- [Understanding the Architecture](articles/architecture.md)
- [Modules and Commands](articles/core-concepts.md)
- [Events and Observables](articles/events-observables.md)
- [Working with Remote Values](articles/remote-values.md)

### Module Guides
- [Browser Module](articles/modules/browser.md)
- [Browsing Context Module](articles/modules/browsing-context.md)
- [Script Module](articles/modules/script.md)
- [Network Module](articles/modules/network.md)
- [Input Module](articles/modules/input.md)
- [Log Module](articles/modules/log.md)
- [Session Module](articles/modules/session.md)
- [Storage Module](articles/modules/storage.md)
- [Emulation Module](articles/modules/emulation.md)
- [Additional Modules](articles/modules/additional-modules.md)

### Examples and Tutorials
- [Common Scenarios](articles/examples/common-scenarios.md)
- [Form Submission](articles/examples/form-submission.md)
- [Network Interception](articles/examples/network-interception.md)
- [Console Monitoring](articles/examples/console-monitoring.md)
- [Preload Scripts](articles/examples/preload-scripts.md)

### Advanced Topics
- [Error Handling](articles/advanced/error-handling.md)
- [Performance Considerations](articles/advanced/performance.md)
- [Custom Modules](articles/advanced/custom-modules.md)

### API Reference
- [Browse the complete API documentation](api/index.md)

## Important Notes

### What This Library Is NOT

- **Not a high-level automation framework**: WebDriverBiDi.NET-Relaxed is a protocol implementation, not a complete automation framework like Selenium, Puppeteer, or Playwright. It can serve as a foundation for such frameworks.
- **No browser management**: The library does not launch browsers or manage profiles. You must start the browser separately with WebDriver BiDi enabled.
- **Protocol-level API**: The API closely follows the protocol specification, which may require more code for common tasks compared to higher-level frameworks.

### Design Principles

- **Immutable Responses**: Objects received from the browser are immutable; their properties cannot be modified
- **Mutable Commands**: Command parameter objects have settable properties to configure the command before sending
- **Async by Design**: All operations are asynchronous and return `Task` or `Task<T>`

## Meet Winston 🐺

<img src="images/winston.png" alt="Winston, the WebDriver BiDi Wolf" class="mascot-image">

Our friendly mascot Winston, the WebDriver BiDi wolf, is here to guide you on your browser automation journey! Learn about his passion for open web standards and bidirectional communication by reading [Winston's story](winston.md).

<br clear="left">

## Getting Help

- **GitHub Issues**: Report bugs or request features at [github.com/hardkoded/webdriverbidi-net-relaxed](https://github.com/hardkoded/webdriverbidi-net-relaxed)
- **Protocol Specification**: Refer to the [W3C WebDriver BiDi specification](https://w3c.github.io/webdriver-bidi/)

## Next Steps

1. [Install the library and set up your development environment](articles/getting-started.md)
2. [Understand the core concepts](articles/core-concepts.md)
3. [Explore the module guides](articles/modules/browser.md)
4. [Try the example scenarios](articles/examples/common-scenarios.md)

---

**License**: MIT License  
**Package**: Available on [NuGet](https://www.nuget.org/packages/WebDriverBiDi-Relaxed)
