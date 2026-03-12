# Emulation Module

The Emulation module provides functionality for emulating device characteristics and media features in the browser.

## Overview

The Emulation module allows you to:
- Emulate different devices and viewports (viewport is on BrowsingContext module)
- Set user agent strings
- Override forced colors mode theme (light/dark)
- Override geolocation
- Emulate timezone and locale settings

## Accessing the Module

[!code-csharp[Accessing Module](../../code/modules/EmulationModuleSamples.cs#AccessingModule)]

## Timeout and Cancellation

All commands in this module accept optional `timeoutOverride` and `CancellationToken` parameters. Use `timeoutOverride` to set a per-command timeout (defaults to `BiDiDriver.DefaultCommandTimeout` when omitted). Use `CancellationToken` for cooperative cancellation. See the [API Design Guide](../advanced/api-design.md#timeout-and-cancellation) for details and examples.

## Viewport Emulation

Viewport emulation is provided by the **BrowsingContext** module. See [Browsing Context Module](browsing-context.md) for details.

### Set Viewport Size

[!code-csharp[Set Viewport Size](../../code/modules/EmulationModuleSamples.cs#SetViewportSize)]

### Common Device Viewports

[!code-csharp[Common Device Viewports](../../code/modules/EmulationModuleSamples.cs#CommonDeviceViewports)]

## User Agent Override

### Set Custom User Agent

[!code-csharp[Set Custom User Agent](../../code/modules/EmulationModuleSamples.cs#SetCustomUserAgent)]

### Common User Agents

[!code-csharp[Common User Agents](../../code/modules/EmulationModuleSamples.cs#CommonUserAgents)]

## Forced Colors Mode Theme

The Emulation module provides `SetForcedColorsModeThemeOverrideAsync` to emulate light or dark forced colors mode.

### Emulate Dark Mode

[!code-csharp[Emulate Dark Mode](../../code/modules/EmulationModuleSamples.cs#EmulateDarkMode)]

### Clear Forced Colors Override

[!code-csharp[Clear Forced Colors Override](../../code/modules/EmulationModuleSamples.cs#ClearForcedColorsOverride)]

## Geolocation Override

### Set Location

[!code-csharp[Set Geolocation](../../code/modules/EmulationModuleSamples.cs#SetGeolocation)]

### Common Locations

[!code-csharp[Common Locations](../../code/modules/EmulationModuleSamples.cs#CommonLocations)]

## Timezone Emulation

### Set Timezone

[!code-csharp[Set Timezone](../../code/modules/EmulationModuleSamples.cs#SetTimezone)]

### Common Timezones

[!code-csharp[Common Timezones](../../code/modules/EmulationModuleSamples.cs#CommonTimezones)]

## Locale Emulation

### Set Locale

[!code-csharp[Set Locale](../../code/modules/EmulationModuleSamples.cs#SetLocale)]

## Common Patterns

### Pattern: Mobile Device Emulation

[!code-csharp[Mobile Device Emulation](../../code/modules/EmulationModuleSamples.cs#MobileDeviceEmulation)]

[!code-csharp[Mobile Device Emulation](../../code/modules/EmulationModuleSamples.cs#MobileDeviceEmulationUsage)]


### Pattern: Responsive Testing

[!code-csharp[Responsive Testing](../../code/modules/EmulationModuleSamples.cs#ResponsiveTesting)]

### Pattern: Dark Mode Testing

[!code-csharp[Dark Mode Testing](../../code/modules/EmulationModuleSamples.cs#DarkModeTesting)]

### Pattern: Location-Based Testing

[!code-csharp[Location-Based Testing](../../code/modules/EmulationModuleSamples.cs#Location-BasedTesting)]

## Best Practices

1. **Reset between tests**: Clear emulation settings between test cases
2. **Match device characteristics**: Set viewport, user agent, and pixel ratio together
3. **Test real devices**: Emulation is useful but test on real devices when possible
4. **Verify settings**: Check that emulation applied correctly
5. **Consider performance**: Some emulations may affect performance

## Limitations

- Emulation is not perfect - some device-specific behaviors may not be replicated
- Hardware features (camera, sensors) cannot be fully emulated
- Performance characteristics differ from real devices
- Some browser features may detect emulation

## Common Issues

### Viewport Not Changing

**Problem**: Viewport size doesn't change after setting.

**Solution**:
- Ensure the browsing context is valid
- Try refreshing the page after setting viewport
- Check that the page doesn't override viewport settings

### User Agent Not Applied

**Problem**: Websites detect wrong device despite user agent override.

**Solution**:
- Set user agent before navigation
- Also set viewport and device pixel ratio
- Some sites use other detection methods (touch events, etc.)

### Geolocation Permission Denied

**Problem**: Page can't access geolocation even after setting.

**Solution**:
- Grant geolocation permission through browser settings
- Use the Permissions module to grant access
- Check browser console for permission errors

## Next Steps

- [Browser Module](browser.md): Browser-level operations
- [Browsing Context Module](browsing-context.md): Viewport and navigation
- [Additional Modules](additional-modules.md): Other specialized modules
- [API Reference](../../api/index.md): Complete API documentation
