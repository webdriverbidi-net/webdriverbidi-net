# Permissions Module

The Permissions module allows you to manage browser permissions for features like geolocation, notifications, and camera access.

## Overview

The Permissions module allows you to:

- Grant or deny browser permissions
- Manage geolocation, notifications, camera, and microphone permissions
- Control permission prompts
- Test permission-dependent functionality

## Accessing the Module

```csharp
PermissionsModule permissions = driver.Permissions;
```

## Setting Permissions

### Grant Geolocation Permission

```csharp
SetPermissionsCommandParameters params = new SetPermissionsCommandParameters();
params.Descriptor = new PermissionsDescriptor("geolocation");
params.State = PermissionState.Granted;

await driver.Permissions.SetPermissionsAsync(params);
Console.WriteLine("Geolocation permission granted");
```

### Deny Permission

```csharp
SetPermissionsCommandParameters params = new SetPermissionsCommandParameters();
params.Descriptor = new PermissionsDescriptor("geolocation");
params.State = PermissionState.Denied;

await driver.Permissions.SetPermissionsAsync(params);
Console.WriteLine("Geolocation permission denied");
```

## Permission Types

### Notifications

```csharp
SetPermissionsCommandParameters params = new SetPermissionsCommandParameters();
params.Descriptor = new PermissionsDescriptor("notifications");
params.State = PermissionState.Granted;

await driver.Permissions.SetPermissionsAsync(params);
```

### Camera

```csharp
SetPermissionsCommandParameters params = new SetPermissionsCommandParameters();
params.Descriptor = new PermissionsDescriptor("camera");
params.State = PermissionState.Granted;

await driver.Permissions.SetPermissionsAsync(params);
```

### Microphone

```csharp
SetPermissionsCommandParameters params = new SetPermissionsCommandParameters();
params.Descriptor = new PermissionsDescriptor("microphone");
params.State = PermissionState.Granted;

await driver.Permissions.SetPermissionsAsync(params);
```

### MIDI

```csharp
SetPermissionsCommandParameters params = new SetPermissionsCommandParameters();
params.Descriptor = new PermissionsDescriptor("midi");
params.State = PermissionState.Granted;

await driver.Permissions.SetPermissionsAsync(params);
```

## Permission States

Permissions can be set to three states:

- `PermissionState.Granted` - Permission is granted
- `PermissionState.Denied` - Permission is denied
- `PermissionState.Prompt` - User will be prompted

```csharp
// Set permission to prompt user
SetPermissionsCommandParameters params = new SetPermissionsCommandParameters();
params.Descriptor = new PermissionsDescriptor("notifications");
params.State = PermissionState.Prompt;

await driver.Permissions.SetPermissionsAsync(params);
```

## Common Patterns

### Testing Geolocation Functionality

```csharp
// Grant geolocation permission
SetPermissionsCommandParameters params = new SetPermissionsCommandParameters();
params.Descriptor = new PermissionsDescriptor("geolocation");
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

### Testing Notification Permissions

```csharp
// Grant notification permission
SetPermissionsCommandParameters params = new SetPermissionsCommandParameters();
params.Descriptor = new PermissionsDescriptor("notifications");
params.State = PermissionState.Granted;
await driver.Permissions.SetPermissionsAsync(params);

// Navigate to page
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com")
    { Wait = ReadinessState.Complete });

// Check permission state in page
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "Notification.permission",
        new ContextTarget(contextId),
        true));

if (result is EvaluateResultSuccess success)
{
    string permissionState = success.Result.ValueAs<string>();
    Console.WriteLine($"Notification permission: {permissionState}");
}
```

### Testing Permission Denial

```csharp
// Deny camera permission
SetPermissionsCommandParameters params = new SetPermissionsCommandParameters();
params.Descriptor = new PermissionsDescriptor("camera");
params.State = PermissionState.Denied;
await driver.Permissions.SetPermissionsAsync(params);

// Try to access camera - should fail
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        @"navigator.mediaDevices.getUserMedia({ video: true })
            .then(() => 'granted')
            .catch(err => `denied: ${err.name}`)",
        new ContextTarget(contextId),
        true));

if (result is EvaluateResultSuccess success)
{
    string accessResult = success.Result.ValueAs<string>();
    Console.WriteLine($"Camera access: {accessResult}");
}
```

### Testing Multiple Permissions

```csharp
// Grant multiple permissions
string[] permissionTypes = { "camera", "microphone" };

foreach (var permType in permissionTypes)
{
    SetPermissionsCommandParameters params = new SetPermissionsCommandParameters();
    params.Descriptor = new PermissionsDescriptor(permType);
    params.State = PermissionState.Granted;
    await driver.Permissions.SetPermissionsAsync(params);
}

// Test accessing both camera and microphone
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        @"navigator.mediaDevices.getUserMedia({ video: true, audio: true })
            .then(() => 'both granted')
            .catch(err => `error: ${err.name}`)",
        new ContextTarget(contextId),
        true));

if (result is EvaluateResultSuccess success)
{
    string accessResult = success.Result.ValueAs<string>();
    Console.WriteLine($"Media access: {accessResult}");
}
```

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
