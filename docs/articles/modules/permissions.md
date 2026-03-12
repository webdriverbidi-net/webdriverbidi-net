# Permissions Module

The Permissions module allows you to manage browser permissions for features like geolocation, notifications, and camera access.

## Overview

The Permissions module allows you to:

- Grant or deny browser permissions
- Manage geolocation, notifications, camera, and microphone permissions
- Control permission prompts
- Test permission-dependent functionality

## Accessing the Module

[!code-csharp[Accessing Module](../../code/modules/PermissionsModuleSamples.cs#AccessingModule)]

## Timeout and Cancellation

All commands in this module accept optional `timeoutOverride` and `CancellationToken` parameters. Use `timeoutOverride` to set a per-command timeout (defaults to `BiDiDriver.DefaultCommandTimeout` when omitted). Use `CancellationToken` for cooperative cancellation. See the [API Design Guide](../advanced/api-design.md#timeout-and-cancellation) for details and examples.

## Setting Permissions

### Grant Geolocation Permission

[!code-csharp[Grant Geolocation Permission](../../code/modules/PermissionsModuleSamples.cs#GrantGeolocationPermission)]

### Deny Permission

[!code-csharp[Deny Permission](../../code/modules/PermissionsModuleSamples.cs#DenyPermission)]

## Permission Types

### Notifications

[!code-csharp[Grant Notifications Permission](../../code/modules/PermissionsModuleSamples.cs#GrantNotificationsPermission)]

### Camera

[!code-csharp[Grant Camera Permission](../../code/modules/PermissionsModuleSamples.cs#GrantCameraPermission)]

### Microphone

[!code-csharp[Grant Microphone Permission](../../code/modules/PermissionsModuleSamples.cs#GrantMicrophonePermission)]

### MIDI

[!code-csharp[Grant MIDI Permission](../../code/modules/PermissionsModuleSamples.cs#GrantMIDIPermission)]

## Permission States

Permissions can be set to three states:

- `PermissionState.Granted` - Permission is granted
- `PermissionState.Denied` - Permission is denied
- `PermissionState.Prompt` - User will be prompted

[!code-csharp[Set Permission to Prompt](../../code/modules/PermissionsModuleSamples.cs#SetPermissiontoPrompt)]

## Common Patterns

### Testing Geolocation Functionality

[!code-csharp[Testing Geolocation Functionality](../../code/modules/PermissionsModuleSamples.cs#TestingGeolocationFunctionality)]

### Testing Notification Permissions

[!code-csharp[Testing Notification Permissions](../../code/modules/PermissionsModuleSamples.cs#TestingNotificationPermissions)]

### Testing Permission Denial

[!code-csharp[Testing Permission Denial](../../code/modules/PermissionsModuleSamples.cs#TestingPermissionDenial)]

### Testing Multiple Permissions

[!code-csharp[Testing Multiple Permissions](../../code/modules/PermissionsModuleSamples.cs#TestingMultiplePermissions)]

## Available Permission Names

Common permission descriptor names:

- `"geolocation"` - Location services
- `"notifications"` - Push notifications
- `"camera"` - Camera access
- `"microphone"` - Microphone access
- `"midi"` - MIDI device access
- `"background-sync"` - Background sync
- `"clipboard-read"` - Clipboard read access
- `"clipboard-write"` - Clipboard write access
- `"persistent-storage"` - Persistent storage quota

**Note**: Available permissions vary by browser and version.

## Browser Support

| Browser | Support Level |
|---------|---------------|
| Chrome/Edge | ✅ Full support |
| Firefox | ✅ Full support |
| Safari | ⚠️ Limited support |

## Best Practices

1. **Set permissions before navigation**: Set permissions before navigating to the page that needs them
2. **Reset permissions between tests**: Clear permissions to ensure test isolation
3. **Test both granted and denied states**: Verify your application handles both cases correctly
4. **Combine with emulation**: Use with Emulation module for location testing
5. **Handle errors gracefully**: Not all permissions are available in all browsers

## Common Issues

### Permissions Not Taking Effect

**Problem**: Permission changes don't apply to the page.

**Solution**:
- Set permissions before navigating to the page
- Refresh the page after changing permissions
- Ensure you're using the correct permission name

### Permission Not Supported

**Problem**: Module throws "not supported" errors.

**Solution**:
- Check browser version and compatibility
- Enable experimental features if required
- Verify the permission type is available in your browser

### Permission Persists Between Tests

**Problem**: Permission state carries over to next test.

**Solution**:
- Reset permissions to `PermissionState.Prompt` after each test
- Use isolated user contexts for test isolation
- Clear browser state between test runs

### Invalid Permission Name

**Problem**: Permission descriptor name not recognized.

**Solution**:
- Use standard permission names from the Permissions API
- Check browser documentation for supported permission names
- Test permission name in browser console first

## Next Steps

- [Emulation Module](emulation.md): Device and geolocation emulation
- [Browser Module](browser.md): User context management
- [Bluetooth Module](bluetooth.md): Web Bluetooth API control
- [API Reference](../../api/index.md): Complete API documentation

## Further Reading

- [W3C Permissions Specification](https://www.w3.org/TR/permissions/)
- [Permissions API](https://developer.mozilla.org/en-US/docs/Web/API/Permissions_API)
- [Geolocation API](https://developer.mozilla.org/en-US/docs/Web/API/Geolocation_API)
- [MediaDevices API](https://developer.mozilla.org/en-US/docs/Web/API/MediaDevices)
