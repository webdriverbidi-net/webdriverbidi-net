# Bluetooth Module

The Bluetooth module provides control over the Web Bluetooth API for testing Bluetooth-enabled web applications.

## Overview

The Bluetooth module allows you to:

- Simulate Bluetooth devices
- Control device advertisements
- Test Web Bluetooth API functionality
- Manage device characteristics and services

## Accessing the Module

[!code-csharp[Accessing Module](../../code/modules/BluetoothModuleSamples.cs#AccessingModule)]

## Timeout and Cancellation

All commands in this module accept optional `timeoutOverride` and `CancellationToken` parameters. Use `timeoutOverride` to set a per-command timeout (defaults to `BiDiDriver.DefaultCommandTimeout` when omitted). Use `CancellationToken` for cooperative cancellation. See the [API Design Guide](../advanced/api-design.md#timeout-and-cancellation) for details and examples.

## Simulating Bluetooth Devices

### Basic Device Simulation

[!code-csharp[Basic Device Simulation](../../code/modules/BluetoothModuleSamples.cs#BasicDeviceSimulation)]

### Device with Services

[!code-csharp[Device with Services](../../code/modules/BluetoothModuleSamples.cs#DevicewithServices)]

## Simulating Advertisements

### Basic Advertisement

[!code-csharp[Basic Advertisement](../../code/modules/BluetoothModuleSamples.cs#BasicAdvertisement)]

### Advertisement with Signal Strength

[!code-csharp[Advertisement with Signal Strength](../../code/modules/BluetoothModuleSamples.cs#AdvertisementwithSignalStrength)]

## Common Patterns

### Testing Bluetooth Scanner

[!code-csharp[Testing Bluetooth Scanner](../../code/modules/BluetoothModuleSamples.cs#TestingBluetoothScanner)]

### Testing Multiple Devices

[!code-csharp[Testing Multiple Devices](../../code/modules/BluetoothModuleSamples.cs#TestingMultipleDevices)]

### Testing Device Connection

[!code-csharp[Testing Device Connection](../../code/modules/BluetoothModuleSamples.cs#TestingDeviceConnection)]

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
