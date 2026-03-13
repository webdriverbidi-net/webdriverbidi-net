# Bluetooth Module

The Bluetooth module provides control over the Web Bluetooth API for testing Bluetooth-enabled web applications.

## Overview

The Bluetooth module allows you to:

- Simulate the Bluetooth adapter state (present, powered on/off, absent)
- Simulate Bluetooth peripherals and advertisements
- Simulate GATT services, characteristics, and descriptors
- Simulate GATT connection and disconnection responses
- Handle device selection prompts from `navigator.bluetooth.requestDevice`
- React to characteristic and descriptor events

## Accessing the Module

[!code-csharp[Accessing Module](../../code/modules/BluetoothModuleSamples.cs#AccessingModule)]

## Timeout and Cancellation

All commands in this module accept optional `timeoutOverride` and `CancellationToken` parameters. Use `timeoutOverride` to set a per-command timeout (defaults to `BiDiDriver.DefaultCommandTimeout` when omitted). Use `CancellationToken` for cooperative cancellation. See the [API Design Guide](../advanced/api-design.md#timeout-and-cancellation) for details and examples.

## Simulating the Bluetooth Adapter

### Simulate Adapter State

Use `SimulateAdapterAsync` to simulate the presence or absence of the Bluetooth adapter, along with its power state. Use `AdapterState.PoweredOn`, `AdapterState.PoweredOff`, or `AdapterState.Absent`:

[!code-csharp[Simulate Adapter](../../code/modules/BluetoothModuleSamples.cs#SimulateAdapter)]

### Disable Simulation

Use `DisableSimulationAsync` to turn off Bluetooth simulation for a browsing context:

[!code-csharp[Disable Simulation](../../code/modules/BluetoothModuleSamples.cs#DisableSimulation)]

## Simulating Bluetooth Devices

### Basic Device Simulation

Use `SimulatePreconnectedPeripheralAsync` to simulate a Bluetooth peripheral already connected to the page. The device appears as if it were pre-paired:

[!code-csharp[Basic Device Simulation](../../code/modules/BluetoothModuleSamples.cs#BasicDeviceSimulation)]

### Device with Services

Add known service UUIDs so the device can be discovered by `navigator.bluetooth.requestDevice` with service filters:

[!code-csharp[Device with Services](../../code/modules/BluetoothModuleSamples.cs#DevicewithServices)]

## Simulating Advertisements

### Basic Advertisement

Use `SimulateAdvertisementAsync` to simulate Bluetooth Low Energy advertisements. Create a `ScanRecord` and `SimulateAdvertisementScanEntry` with device address, RSSI (signal strength), and scan data:

[!code-csharp[Basic Advertisement](../../code/modules/BluetoothModuleSamples.cs#BasicAdvertisement)]

### Advertisement with Signal Strength

RSSI values typically range from -30 (strong) to -90 (weak). Use different values to test signal-strength-dependent behavior:

[!code-csharp[Advertisement with Signal Strength](../../code/modules/BluetoothModuleSamples.cs#AdvertisementwithSignalStrength)]

## Simulating GATT Services

Use `SimulateServiceAsync` to add or remove GATT services on a simulated device. Use `SimulateServiceType.Add` or `SimulateServiceType.Remove`:

[!code-csharp[Simulate Service](../../code/modules/BluetoothModuleSamples.cs#SimulateService)]

## Simulating GATT Characteristics

### Add or Remove Characteristics

Use `SimulateCharacteristicAsync` to add or remove characteristics. Optionally set `CharacteristicProperties` (read, write, notify, indicate, etc.):

[!code-csharp[Simulate Characteristic](../../code/modules/BluetoothModuleSamples.cs#SimulateCharacteristic)]

### Simulate Characteristic Responses

Use `SimulateCharacteristicResponseAsync` to simulate responses for read, write, subscribe, or unsubscribe operations. Use `SimulateCharacteristicResponseType.Read`, `Write`, `SubscribeToNotifications`, or `UnsubscribeFromNotifications`. Code 0 indicates success; other values indicate GATT error codes:

[!code-csharp[Simulate Characteristic Response](../../code/modules/BluetoothModuleSamples.cs#SimulateCharacteristicResponse)]

## Simulating GATT Descriptors

### Add or Remove Descriptors

Use `SimulateDescriptorAsync` to add or remove descriptors on a characteristic:

[!code-csharp[Simulate Descriptor](../../code/modules/BluetoothModuleSamples.cs#SimulateDescriptor)]

### Simulate Descriptor Responses

Use `SimulateDescriptorResponseAsync` to simulate read or write descriptor responses. Use `SimulateDescriptorResponseType.Read` or `Write`:

[!code-csharp[Simulate Descriptor Response](../../code/modules/BluetoothModuleSamples.cs#SimulateDescriptorResponse)]

## Simulating GATT Connection and Disconnection

### GATT Connection Response

When the page calls `device.gatt.connect()`, the `OnGattConnectionAttempted` event fires. Use `SimulateGattConnectionResponseAsync` to respond. Code 0 indicates success:

[!code-csharp[Simulate GATT Connection Response](../../code/modules/BluetoothModuleSamples.cs#SimulateGattConnectionResponse)]

### GATT Disconnection

Use `SimulateGattDisconnectionAsync` to simulate a device disconnecting:

[!code-csharp[Simulate GATT Disconnection](../../code/modules/BluetoothModuleSamples.cs#SimulateGattDisconnection)]

## Handling Device Selection Prompts

When a page calls `navigator.bluetooth.requestDevice()`, a prompt appears. Subscribe to `OnRequestDevicePromptUpdated` to receive prompt events, then use `HandleRequestDevicePromptAsync` to accept (selecting a device) or cancel:

[!code-csharp[Handle Request Device Prompt](../../code/modules/BluetoothModuleSamples.cs#HandleRequestDevicePrompt)]

Use `HandleRequestDevicePromptAcceptCommandParameters` with a device ID to accept, or `HandleRequestDevicePromptCancelCommandParameters` to cancel.

## Events

Subscribe to Bluetooth events via `Session.SubscribeAsync` after adding observers. Remember the two-step subscription pattern: add observer, then subscribe.

### RequestDevicePromptUpdated

Fired when a Bluetooth device selection prompt is shown or updated. Use the event to get the prompt ID and available devices, then call `HandleRequestDevicePromptAsync`:

[!code-csharp[Request Device Prompt Updated](../../code/modules/BluetoothModuleSamples.cs#RequestDevicePromptUpdated)]

### GattConnectionAttempted

Fired when the page attempts a GATT connection to a device. Use this to respond with `SimulateGattConnectionResponseAsync`:

[!code-csharp[GATT Connection Attempted](../../code/modules/BluetoothModuleSamples.cs#GattConnectionAttempted)]

### CharacteristicEventGenerated

Fired when the page reads, writes, or subscribes to a characteristic. Use this to respond with `SimulateCharacteristicResponseAsync`:

[!code-csharp[Characteristic Event Generated](../../code/modules/BluetoothModuleSamples.cs#CharacteristicEventGenerated)]

### DescriptorEventGenerated

Fired when the page reads or writes a descriptor. Use this to respond with `SimulateDescriptorResponseAsync`:

[!code-csharp[Descriptor Event Generated](../../code/modules/BluetoothModuleSamples.cs#DescriptorEventGenerated)]

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
5. **Clean up devices**: Call `DisableSimulationAsync` or remove simulated devices between tests
6. **Subscribe before actions**: Add observers and call `Session.SubscribeAsync` before triggering Bluetooth operations in the page

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
- Call `SimulateAdvertisementAsync` after `SimulatePreconnectedPeripheralAsync`
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
