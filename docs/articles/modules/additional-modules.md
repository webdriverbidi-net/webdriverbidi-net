# Additional Modules

This guide provides an overview of additional specialized modules in WebDriverBiDi.NET that extend functionality beyond the core WebDriver BiDi specification.

## Overview

WebDriverBiDi.NET includes support for several W3C specifications that use the WebDriver BiDi protocol:

- **[Permissions Module](permissions.md)** - Browser permission management
- **[Bluetooth Module](bluetooth.md)** - Web Bluetooth API control
- **[WebExtension Module](webextension.md)** - Browser extension management
- **[Speculation Module](speculation.md)** - Navigation prefetching and prerendering
- **[User Agent Client Hints Module](user-agent-client-hints.md)** - User agent client hints

## Permissions Module

The [Permissions module](permissions.md) allows you to manage browser permissions for features like geolocation, notifications, and camera access.

### Quick Example

```csharp
// Grant geolocation permission
SetPermissionsCommandParameters params = new SetPermissionsCommandParameters();
params.Descriptor = new PermissionsDescriptor("geolocation");
params.State = PermissionState.Granted;

await driver.Permissions.SetPermissionsAsync(params);
```

**[View full Permissions module documentation →](permissions.md)**

## Bluetooth Module

The [Bluetooth module](bluetooth.md) provides control over the Web Bluetooth API for testing Bluetooth-enabled web applications.

### Quick Example

```csharp
// Simulate a Bluetooth device
SimulateDeviceCommandParameters params = new SimulateDeviceCommandParameters();
params.DeviceAddress = "AA:BB:CC:DD:EE:FF";
params.DeviceName = "Heart Rate Monitor";
params.Services = new List<string> { "heart_rate" };

await driver.Bluetooth.SimulateDeviceAsync(params);
```

**Note**: Bluetooth module support varies by browser and requires specific browser flags to be enabled.

**[View full Bluetooth module documentation →](bluetooth.md)**

## WebExtension Module

The [WebExtension module](webextension.md) allows you to manage browser extensions programmatically.

### Quick Example

```csharp
// Install an extension
InstallCommandParameters params = new InstallCommandParameters();
params.ExtensionPath = "/path/to/extension.crx";

InstallCommandResult result = await driver.WebExtension.InstallAsync(params);
string extensionId = result.ExtensionId;

Console.WriteLine($"Extension installed: {extensionId}");
```

**Note**: Extension installation support varies by browser. Chrome and Edge support CRX files, while Firefox uses different formats.

**[View full WebExtension module documentation →](webextension.md)**

## Speculation Module

The [Speculation module](speculation.md) provides control over navigation prefetching and prerendering based on the [Speculation Rules API](https://wicg.github.io/nav-speculation/prefetch.html).

### Quick Example

```csharp
// Add prefetch rules
AddSpeculationRulesCommandParameters params =
    new AddSpeculationRulesCommandParameters(contextId);

params.Rules = @"{
    ""prefetch"": [
        {
            ""source"": ""list"",
            ""urls"": [""https://example.com/page1"", ""https://example.com/page2""]
        }
    ]
}";

AddSpeculationRulesCommandResult result =
    await driver.Speculation.AddSpeculationRulesAsync(params);
```

**Note**: Speculation Rules support is experimental and may not be available in all browsers.

**[View full Speculation module documentation →](speculation.md)**

## User Agent Client Hints Module

The [User Agent Client Hints module](user-agent-client-hints.md) allows you to override user agent client hints in the browser, enabling responsive design and feature detection testing without relying solely on the User-Agent string.

### Quick Example

```csharp
// Override client hints for cross-browser brand testing
SetClientHintsOverrideCommandParameters params = new SetClientHintsOverrideCommandParameters();
params.ClientHints = new ClientHintsMetadata
{
    Brands = new List<BrandVersion>
    {
        new BrandVersion("Chromium", "120.0"),
        new BrandVersion("Google Chrome", "120.0")
    },
    Platform = "Windows",
    PlatformVersion = "10.0",
    Architecture = "x86",
    Mobile = false
};

await driver.UserAgentClientHints.SetClientHintsOverrideAsync(params);
```

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
- **[Speculation Module Documentation](speculation.md)** - Full prefetch and prerender guide
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
- [Speculation Rules API](https://wicg.github.io/nav-speculation/prefetch.html)
- [User-Agent Client Hints](https://wicg.github.io/ua-client-hints/)

