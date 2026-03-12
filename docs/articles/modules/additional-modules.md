# Additional Modules

This guide provides an overview of additional specialized modules in WebDriverBiDi.NET that extend functionality beyond the core WebDriver BiDi specification.

## Overview

WebDriverBiDi.NET includes support for several W3C specifications that use the WebDriver BiDi protocol. All module commands accept optional `timeoutOverride` and `CancellationToken` parameters—see the [API Design Guide](../advanced/api-design.md#timeout-and-cancellation) for details.

- **[Permissions Module](permissions.md)** - Browser permission management
- **[Bluetooth Module](bluetooth.md)** - Web Bluetooth API control
- **[WebExtension Module](webextension.md)** - Browser extension management
- **[Speculation Module](speculation.md)** - Prefetch status monitoring
- **[User Agent Client Hints Module](user-agent-client-hints.md)** - User agent client hints

## Permissions Module

The [Permissions module](permissions.md) allows you to manage browser permissions for features like geolocation, notifications, and camera access.

### Quick Example

[!code-csharp[Grant Geolocation Permission](../../code/modules/AdditionalModulesSamples.cs#GrantGeolocationPermission)]

**[View full Permissions module documentation →](permissions.md)**

## Bluetooth Module

The [Bluetooth module](bluetooth.md) provides control over the Web Bluetooth API for testing Bluetooth-enabled web applications.

### Quick Example

[!code-csharp[Simulate Bluetooth Device](../../code/modules/AdditionalModulesSamples.cs#SimulateBluetoothDevice)]

**Note**: Bluetooth module support varies by browser and requires specific browser flags to be enabled.

**[View full Bluetooth module documentation →](bluetooth.md)**

## WebExtension Module

The [WebExtension module](webextension.md) allows you to manage browser extensions programmatically.

### Quick Example

[!code-csharp[Install Extension](../../code/modules/AdditionalModulesSamples.cs#InstallExtension)]

**Note**: Extension installation support varies by browser. Chrome and Edge support CRX files, while Firefox uses different formats.

**[View full WebExtension module documentation →](webextension.md)**

## Speculation Module

The [Speculation module](speculation.md) provides **monitoring** of prefetch status updates. It subscribes to the `speculation.prefetchStatusUpdated` event defined in the [Prefetch spec's Automated testing section](https://wicg.github.io/nav-speculation/prefetch.html#automated-testing). WebDriver BiDi does not define commands to add or remove speculation rules—those are configured by the page via the Speculation Rules API.

### Quick Example

[!code-csharp[Subscribe to Prefetch Status](../../code/modules/SpeculationModuleSamples.cs#SubscribetoPrefetchStatus)]

**Note**: Prefetch status events are experimental and may not be available in all browsers.

**[View full Speculation module documentation →](speculation.md)**

## User Agent Client Hints Module

The [User Agent Client Hints module](user-agent-client-hints.md) allows you to override user agent client hints in the browser, enabling responsive design and feature detection testing without relying solely on the User-Agent string.

### Quick Example

[!code-csharp[Set Client Hints Override](../../code/modules/AdditionalModulesSamples.cs#SetClientHintsOverride)]

**Note**: User Agent Client Hints support varies by browser. This module complements the [Emulation Module](emulation.md) for user agent string overrides.

**[View full User Agent Client Hints module documentation →](user-agent-client-hints.md)**

## Module Availability

| Module | Chrome/Edge | Firefox | Safari |
|--------|-------------|---------|--------|
| Permissions | ✅ | ✅ | ⚠️ Limited |
| Bluetooth | ⚠️ Experimental | ❌ | ❌ |
| WebExtension | ✅ | ⚠️ Different API | ⚠️ Limited |
| Speculation | ⚠️ Experimental | ❌ | ❌ |
| User Agent Client Hints | ⚠️ Experimental | ❌ | ❌ |

## Getting Started

Each module has its own dedicated documentation page with comprehensive examples, best practices, and troubleshooting guides:

- **[Permissions Module Documentation](permissions.md)** - Complete guide to managing browser permissions
- **[Bluetooth Module Documentation](bluetooth.md)** - Full guide to Web Bluetooth API testing
- **[WebExtension Module Documentation](webextension.md)** - Complete extension management guide
- **[Speculation Module Documentation](speculation.md)** - Prefetch status monitoring
- **[User Agent Client Hints Module Documentation](user-agent-client-hints.md)** - Complete guide to client hints override

## Next Steps

- [Emulation Module](emulation.md): Device and media emulation
- [Browser Module](browser.md): Browser-level operations
- [Core Concepts](../core-concepts.md): Understanding modules
- [API Reference](../../api/index.md): Complete API documentation

## Further Reading

- [W3C Permissions Specification](https://www.w3.org/TR/permissions/)
- [Web Bluetooth Specification](https://webbluetoothcg.github.io/web-bluetooth/)
- [WebExtensions API](https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions)
- [Prefetch Spec – Automated testing](https://wicg.github.io/nav-speculation/prefetch.html#automated-testing)
- [User-Agent Client Hints](https://wicg.github.io/ua-client-hints/)

