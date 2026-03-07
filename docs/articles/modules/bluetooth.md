# Bluetooth Module

The Bluetooth module provides control over the Web Bluetooth API for testing Bluetooth-enabled web applications.

## Overview

The Bluetooth module allows you to:

- Simulate Bluetooth devices
- Control device advertisements
- Test Web Bluetooth API functionality
- Manage device characteristics and services

## Accessing the Module

```csharp
BluetoothModule bluetooth = driver.Bluetooth;
```

## Simulating Bluetooth Devices

### Basic Device Simulation

```csharp
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

### Device with Services

```csharp
SimulateDeviceCommandParameters params = new SimulateDeviceCommandParameters();
params.DeviceAddress = "AA:BB:CC:DD:EE:FF";
params.DeviceName = "Heart Rate Monitor";
params.Services = new List<string> { "heart_rate" };

await driver.Bluetooth.SimulateDeviceAsync(params);
Console.WriteLine("Bluetooth heart rate monitor simulated");
```

## Simulating Advertisements

### Basic Advertisement

```csharp
SimulateAdvertisementCommandParameters params =
    new SimulateAdvertisementCommandParameters();
params.DeviceAddress = "00:00:00:00:00:00";
params.Rssi = -50;  // Signal strength

await driver.Bluetooth.SimulateAdvertisementAsync(params);
Console.WriteLine("Bluetooth advertisement simulated");
```

### Advertisement with Signal Strength

```csharp
// Simulate weak signal
SimulateAdvertisementCommandParameters weakSignal =
    new SimulateAdvertisementCommandParameters();
weakSignal.DeviceAddress = "00:00:00:00:00:00";
weakSignal.Rssi = -80;  // Weak signal

await driver.Bluetooth.SimulateAdvertisementAsync(weakSignal);

// Simulate strong signal
SimulateAdvertisementCommandParameters strongSignal =
    new SimulateAdvertisementCommandParameters();
strongSignal.DeviceAddress = "00:00:00:00:00:00";
strongSignal.Rssi = -30;  // Strong signal

await driver.Bluetooth.SimulateAdvertisementAsync(strongSignal);
```

## Common Patterns

### Testing Bluetooth Scanner

```csharp
// Simulate a Bluetooth device
SimulateDeviceCommandParameters deviceParams =
    new SimulateDeviceCommandParameters();
deviceParams.DeviceAddress = "AA:BB:CC:DD:EE:FF";
deviceParams.DeviceName = "Heart Rate Monitor";
deviceParams.Services = new List<string> { "heart_rate" };

await driver.Bluetooth.SimulateDeviceAsync(deviceParams);

// Navigate to page with Bluetooth functionality
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com")
    { Wait = ReadinessState.Complete });

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

### Testing Multiple Devices

```csharp
// Simulate multiple devices
List<string> deviceAddresses = new List<string>
{
    "AA:BB:CC:DD:EE:01",
    "AA:BB:CC:DD:EE:02",
    "AA:BB:CC:DD:EE:03"
};

foreach (var address in deviceAddresses)
{
    SimulateDeviceCommandParameters params =
        new SimulateDeviceCommandParameters();
    params.DeviceAddress = address;
    params.DeviceName = $"Device {address.Substring(15)}";
    params.Services = new List<string> { "battery_service" };

    await driver.Bluetooth.SimulateDeviceAsync(params);
}

// Test device discovery
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        @"navigator.bluetooth.requestDevice({
            filters: [{ services: ['battery_service'] }],
            acceptAllDevices: false
        }).then(device => device.id)",
        new ContextTarget(contextId),
        true));
```

### Testing Device Connection

```csharp
// Simulate device
SimulateDeviceCommandParameters deviceParams =
    new SimulateDeviceCommandParameters();
deviceParams.DeviceAddress = "11:22:33:44:55:66";
deviceParams.DeviceName = "Temperature Sensor";
deviceParams.Services = new List<string> { "environmental_sensing" };

await driver.Bluetooth.SimulateDeviceAsync(deviceParams);

// Advertise device
SimulateAdvertisementCommandParameters adParams =
    new SimulateAdvertisementCommandParameters();
adParams.DeviceAddress = "11:22:33:44:55:66";
adParams.Rssi = -45;

await driver.Bluetooth.SimulateAdvertisementAsync(adParams);

// Test connection
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        @"navigator.bluetooth.requestDevice({
            filters: [{ services: ['environmental_sensing'] }]
        })
        .then(device => device.gatt.connect())
        .then(server => 'connected')
        .catch(err => `error: ${err.message}`)",
        new ContextTarget(contextId),
        true));

if (result is EvaluateResultSuccess success)
{
    string connectionStatus = success.Result.ValueAs<string>();
    Console.WriteLine($"Connection status: {connectionStatus}");
}
```

## Browser Support

| Browser | Support Level |
|---------|---------------|
| Chrome/Edge | ⚠️ Experimental (requires flags) |
| Firefox | ❌ Not supported |
| Safari | ❌ Not supported |

**Note**: Bluetooth module support is experimental and requires specific browser flags to be enabled.

## Enabling Bluetooth Support

### Chrome/Edge

Launch browser with experimental web platform features:

```csharp
// Enable experimental features when launching browser
// This is browser-specific and may require custom launch options
```

## Best Practices

1. **Check browser support**: Verify Bluetooth module is available before using
2. **Use realistic device addresses**: Follow MAC address format (XX:XX:XX:XX:XX:XX)
3. **Set appropriate RSSI values**: Use realistic signal strength values (-30 to -90)
4. **Test connection failures**: Simulate both successful and failed connections
5. **Clean up devices**: Clear simulated devices between tests

## Common Issues

### Bluetooth Not Supported

**Problem**: Module throws "not supported" errors.

**Solution**:
- Check browser version (Chrome 90+ recommended)
- Enable experimental web platform features
- Use Chrome or Edge (Firefox/Safari not supported)
- Launch browser with required flags

### Device Not Discovered

**Problem**: Simulated device is not found by Web Bluetooth API.

**Solution**:
- Ensure device services match the filter criteria
- Verify device address format is correct
- Call SimulateAdvertisementAsync after SimulateDeviceAsync
- Check that Web Bluetooth API is enabled in the browser

### Manufacturer Data Format

**Problem**: Manufacturer data not recognized.

**Solution**:
- Use correct company identifier codes
- Ensure byte array format is valid
- Reference [Bluetooth Company Identifiers](https://www.bluetooth.com/specifications/assigned-numbers/company-identifiers/)

## Next Steps

- [Permissions Module](permissions.md): Managing browser permissions
- [WebExtension Module](webextension.md): Browser extension management
- [Script Module](script.md): Executing JavaScript in pages
- [API Reference](../../api/index.md): Complete API documentation

## Further Reading

- [Web Bluetooth Specification](https://webbluetoothcg.github.io/web-bluetooth/)
- [Web Bluetooth API](https://developer.mozilla.org/en-US/docs/Web/API/Web_Bluetooth_API)
- [Bluetooth GATT Services](https://www.bluetooth.com/specifications/gatt/services/)
