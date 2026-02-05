# Additional Modules

This guide covers additional specialized modules in WebDriverBiDi.NET-Relaxed that extend functionality beyond the core WebDriver BiDi specification.

## Overview

WebDriverBiDi.NET-Relaxed includes support for several W3C specifications that use the WebDriver BiDi protocol:

- **Permissions Module** - Browser permission management
- **Bluetooth Module** - Web Bluetooth API control
- **WebExtension Module** - Browser extension management  
- **Speculation Module** - Navigation prefetching and prerendering

## Permissions Module

The Permissions module allows you to manage browser permissions for features like geolocation, notifications, and camera access.

### Accessing the Module

```csharp
PermissionsModule permissions = driver.Permissions;
```

### Setting Permissions

```csharp
// Grant geolocation permission
SetPermissionsCommandParameters params = new SetPermissionsCommandParameters();
params.Descriptor = new GeolocationPermissionDescriptor();
params.State = PermissionState.Granted;

await driver.Permissions.SetPermissionsAsync(params);
Console.WriteLine("Geolocation permission granted");
```

### Permission Types

```csharp
// Notifications
SetPermissionsCommandParameters notificationParams = 
    new SetPermissionsCommandParameters();
notificationParams.Descriptor = new NotificationPermissionDescriptor();
notificationParams.State = PermissionState.Granted;

await driver.Permissions.SetPermissionsAsync(notificationParams);

// Camera
SetPermissionsCommandParameters cameraParams = 
    new SetPermissionsCommandParameters();
cameraParams.Descriptor = new CameraPermissionDescriptor();
cameraParams.State = PermissionState.Granted;

await driver.Permissions.SetPermissionsAsync(cameraParams);

// Microphone
SetPermissionsCommandParameters micParams = 
    new SetPermissionsCommandParameters();
micParams.Descriptor = new MicrophonePermissionDescriptor();
micParams.State = PermissionState.Granted;

await driver.Permissions.SetPermissionsAsync(micParams);
```

### Permission States

Permissions can be set to three states:

- `PermissionState.Granted` - Permission is granted
- `PermissionState.Denied` - Permission is denied
- `PermissionState.Prompt` - User will be prompted

```csharp
// Deny permission
SetPermissionsCommandParameters params = new SetPermissionsCommandParameters();
params.Descriptor = new GeolocationPermissionDescriptor();
params.State = PermissionState.Denied;

await driver.Permissions.SetPermissionsAsync(params);
Console.WriteLine("Geolocation permission denied");
```

### Example: Testing Geolocation Permission

```csharp
// Grant geolocation permission
SetPermissionsCommandParameters params = new SetPermissionsCommandParameters();
params.Descriptor = new GeolocationPermissionDescriptor();
params.State = PermissionState.Granted;
await driver.Permissions.SetPermissionsAsync(params);

// Set location
SetGeolocationCommandParameters geoParams = new SetGeolocationCommandParameters(contextId)
{
    Latitude = 37.7749,
    Longitude = -122.4194,
    Accuracy = 100
};
await driver.Emulation.SetGeolocationAsync(geoParams);

// Test location access
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        @"new Promise((resolve) => {
            navigator.geolocation.getCurrentPosition(
                (pos) => resolve(`${pos.coords.latitude},${pos.coords.longitude}`),
                (err) => resolve(`Error: ${err.message}`)
            );
        })",
        new ContextTarget(contextId),
        true));

if (result is EvaluateResultSuccess success)
{
    string location = success.Result.ValueAs<string>();
    Console.WriteLine($"Location: {location}");
}
```

## Bluetooth Module

The Bluetooth module provides control over the Web Bluetooth API for testing Bluetooth-enabled web applications.

### Accessing the Module

```csharp
BluetoothModule bluetooth = driver.Bluetooth;
```

### Simulating Bluetooth Devices

```csharp
// Simulate a Bluetooth device
SimulateDeviceCommandParameters params = new SimulateDeviceCommandParameters();
params.DeviceAddress = "00:00:00:00:00:00";
params.DeviceName = "Test Device";
params.ManufacturerData = new Dictionary<ushort, byte[]>
{
    { 0x004C, new byte[] { 0x01, 0x02, 0x03 } }
};

await driver.Bluetooth.SimulateDeviceAsync(params);
Console.WriteLine("Bluetooth device simulated");
```

### Simulate Advertisement

```csharp
// Simulate device advertisement
SimulateAdvertisementCommandParameters params = 
    new SimulateAdvertisementCommandParameters();
params.DeviceAddress = "00:00:00:00:00:00";
params.Rssi = -50;  // Signal strength

await driver.Bluetooth.SimulateAdvertisementAsync(params);
Console.WriteLine("Bluetooth advertisement simulated");
```

### Example: Testing Bluetooth Scanner

```csharp
// Simulate a Bluetooth device
SimulateDeviceCommandParameters deviceParams = 
    new SimulateDeviceCommandParameters();
deviceParams.DeviceAddress = "AA:BB:CC:DD:EE:FF";
deviceParams.DeviceName = "Heart Rate Monitor";
deviceParams.Services = new List<string> { "heart_rate" };

await driver.Bluetooth.SimulateDeviceAsync(deviceParams);

// Trigger Bluetooth scan in page
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        @"navigator.bluetooth.requestDevice({
            filters: [{ services: ['heart_rate'] }]
        }).then(device => device.name)",
        new ContextTarget(contextId),
        true));

if (result is EvaluateResultSuccess success)
{
    string deviceName = success.Result.ValueAs<string>();
    Console.WriteLine($"Found device: {deviceName}");
}
```

**Note**: Bluetooth module support varies by browser and requires specific browser flags to be enabled.

## WebExtension Module

The WebExtension module allows you to manage browser extensions programmatically.

### Accessing the Module

```csharp
WebExtensionModule webExtension = driver.WebExtension;
```

### Installing Extensions

```csharp
// Install an extension
InstallCommandParameters params = new InstallCommandParameters();
params.ExtensionPath = "/path/to/extension.crx";

InstallCommandResult result = await driver.WebExtension.InstallAsync(params);
string extensionId = result.ExtensionId;

Console.WriteLine($"Extension installed: {extensionId}");
```

### Uninstalling Extensions

```csharp
// Uninstall an extension
UninstallCommandParameters params = new UninstallCommandParameters(extensionId);
await driver.WebExtension.UninstallAsync(params);

Console.WriteLine("Extension uninstalled");
```

### Example: Testing with Extension

```csharp
// Install extension
InstallCommandParameters installParams = new InstallCommandParameters();
installParams.ExtensionPath = "/path/to/my-extension";

InstallCommandResult installResult = 
    await driver.WebExtension.InstallAsync(installParams);
string extensionId = installResult.ExtensionId;

Console.WriteLine($"Extension installed: {extensionId}");

// Navigate and test
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com")
    { Wait = ReadinessState.Complete });

// Test extension functionality
// ... your test code ...

// Clean up
await driver.WebExtension.UninstallAsync(
    new UninstallCommandParameters(extensionId));
```

**Note**: Extension installation support varies by browser. Chrome and Edge support CRX files, while Firefox uses different formats.

## Speculation Module

The Speculation module provides control over navigation prefetching and prerendering based on the [Speculation Rules API](https://wicg.github.io/nav-speculation/prefetch.html).

### Accessing the Module

```csharp
SpeculationModule speculation = driver.Speculation;
```

### Adding Speculation Rules

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

Console.WriteLine($"Speculation rules added: {result.RulesId}");
```

### Prerendering

```csharp
// Add prerender rules
AddSpeculationRulesCommandParameters params = 
    new AddSpeculationRulesCommandParameters(contextId);

params.Rules = @"{
    ""prerender"": [
        {
            ""source"": ""list"",
            ""urls"": [""https://example.com/important-page""]
        }
    ]
}";

await driver.Speculation.AddSpeculationRulesAsync(params);
Console.WriteLine("Prerender rules added");
```

### Removing Speculation Rules

```csharp
// Remove rules
RemoveSpeculationRulesCommandParameters params = 
    new RemoveSpeculationRulesCommandParameters(rulesId);

await driver.Speculation.RemoveSpeculationRulesAsync(params);
Console.WriteLine("Speculation rules removed");
```

### Example: Testing Prefetch

```csharp
// Navigate to initial page
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com")
    { Wait = ReadinessState.Complete });

// Add prefetch rules
AddSpeculationRulesCommandParameters speculationParams = 
    new AddSpeculationRulesCommandParameters(contextId);
speculationParams.Rules = @"{
    ""prefetch"": [{
        ""source"": ""list"",
        ""urls"": [""https://example.com/next-page""]
    }]
}";

AddSpeculationRulesCommandResult speculationResult = 
    await driver.Speculation.AddSpeculationRulesAsync(speculationParams);

// Wait for prefetch
await Task.Delay(2000);

// Navigate to prefetched page (should be faster)
DateTime startTime = DateTime.Now;
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com/next-page")
    { Wait = ReadinessState.Complete });
TimeSpan navigationTime = DateTime.Now - startTime;

Console.WriteLine($"Navigation time: {navigationTime.TotalMilliseconds}ms");
```

**Note**: Speculation Rules support is experimental and may not be available in all browsers.

## Module Availability

| Module | Chrome/Edge | Firefox | Safari |
|--------|-------------|---------|--------|
| Permissions | ✅ | ✅ | ⚠️ Limited |
| Bluetooth | ⚠️ Experimental | ❌ | ❌ |
| WebExtension | ✅ | ⚠️ Different API | ⚠️ Limited |
| Speculation | ⚠️ Experimental | ❌ | ❌ |

## Best Practices

1. **Check browser support**: Not all modules work in all browsers
2. **Enable experimental features**: Some modules require browser flags
3. **Test on real devices**: Hardware features can't be fully emulated
4. **Reset state**: Clear permissions and settings between tests
5. **Handle errors gracefully**: Modules may throw errors if unsupported

## Common Issues

### Module Not Supported

**Problem**: Module methods throw "not supported" errors.

**Solution**:
- Check browser version
- Enable experimental features in browser
- Verify the feature is available in your browser

### Permissions Not Working

**Problem**: Permission changes don't take effect.

**Solution**:
- Set permissions before navigating to the page
- Refresh the page after changing permissions
- Check browser console for permission errors

### Extension Installation Fails

**Problem**: Cannot install browser extension.

**Solution**:
- Verify extension path is correct
- Check extension file format (CRX for Chrome/Edge)
- Ensure extension is properly packaged
- Try loading as unpacked extension for development

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

