# Emulation Module

The Emulation module provides functionality for emulating device characteristics and media features in the browser.

## Overview

The Emulation module allows you to:
- Emulate different devices and viewports
- Set user agent strings
- Simulate media features (prefers-color-scheme, prefers-reduced-motion)
- Override geolocation
- Emulate timezone and locale settings

## Accessing the Module

```csharp
EmulationModule emulation = driver.Emulation;
```

## Viewport Emulation

### Set Viewport Size

```csharp
SetViewportCommandParameters params = new SetViewportCommandParameters(contextId)
{
    Width = 375,    // iPhone width
    Height = 667,   // iPhone height
    DevicePixelRatio = 2.0
};

await driver.Emulation.SetViewportAsync(params);
Console.WriteLine("Viewport set to mobile size");
```

### Common Device Viewports

```csharp
// iPhone 12/13
await driver.Emulation.SetViewportAsync(new SetViewportCommandParameters(contextId)
{
    Width = 390,
    Height = 844,
    DevicePixelRatio = 3.0
});

// iPad Pro
await driver.Emulation.SetViewportAsync(new SetViewportCommandParameters(contextId)
{
    Width = 1024,
    Height = 1366,
    DevicePixelRatio = 2.0
});

// Desktop HD
await driver.Emulation.SetViewportAsync(new SetViewportCommandParameters(contextId)
{
    Width = 1920,
    Height = 1080,
    DevicePixelRatio = 1.0
});
```

## User Agent Override

### Set Custom User Agent

```csharp
SetUserAgentCommandParameters params = new SetUserAgentCommandParameters(contextId)
{
    UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 Mobile/15E148 Safari/604.1"
};

await driver.Emulation.SetUserAgentAsync(params);
Console.WriteLine("User agent set to iPhone");
```

### Common User Agents

```csharp
// Mobile Safari (iPhone)
string iPhoneUA = "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 Mobile/15E148 Safari/604.1";

// Mobile Chrome (Android)
string androidUA = "Mozilla/5.0 (Linux; Android 12; Pixel 6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.45 Mobile Safari/537.36";

// Desktop Safari (macOS)
string safariUA = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 Safari/605.1.15";

// Desktop Firefox
string firefoxUA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:95.0) Gecko/20100101 Firefox/95.0";

await driver.Emulation.SetUserAgentAsync(
    new SetUserAgentCommandParameters(contextId) { UserAgent = iPhoneUA });
```

## Media Features

### Emulate Dark Mode

```csharp
SetEmulatedMediaCommandParameters params = new SetEmulatedMediaCommandParameters(contextId);
params.Features.Add(new MediaFeature("prefers-color-scheme", "dark"));

await driver.Emulation.SetEmulatedMediaAsync(params);
Console.WriteLine("Dark mode enabled");

// Test dark mode styles
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "window.matchMedia('(prefers-color-scheme: dark)').matches",
        new ContextTarget(contextId),
        true));

if (result is EvaluateResultSuccess success)
{
    bool isDarkMode = success.Result.ValueAs<bool>();
    Console.WriteLine($"Dark mode active: {isDarkMode}");
}
```

### Emulate Reduced Motion

```csharp
SetEmulatedMediaCommandParameters params = new SetEmulatedMediaCommandParameters(contextId);
params.Features.Add(new MediaFeature("prefers-reduced-motion", "reduce"));

await driver.Emulation.SetEmulatedMediaAsync(params);
Console.WriteLine("Reduced motion preference set");
```

### Multiple Media Features

```csharp
SetEmulatedMediaCommandParameters params = new SetEmulatedMediaCommandParameters(contextId);
params.Features.Add(new MediaFeature("prefers-color-scheme", "dark"));
params.Features.Add(new MediaFeature("prefers-reduced-motion", "reduce"));
params.Features.Add(new MediaFeature("prefers-contrast", "high"));

await driver.Emulation.SetEmulatedMediaAsync(params);
Console.WriteLine("Multiple media features set");
```

### Clear Media Emulation

```csharp
SetEmulatedMediaCommandParameters params = new SetEmulatedMediaCommandParameters(contextId);
// Empty features list clears emulation
await driver.Emulation.SetEmulatedMediaAsync(params);
Console.WriteLine("Media emulation cleared");
```

## Geolocation Override

### Set Location

```csharp
SetGeolocationCommandParameters params = new SetGeolocationCommandParameters(contextId)
{
    Latitude = 37.7749,   // San Francisco
    Longitude = -122.4194,
    Accuracy = 100
};

await driver.Emulation.SetGeolocationAsync(params);
Console.WriteLine("Geolocation set to San Francisco");

// Verify location in page
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        @"new Promise((resolve) => {
            navigator.geolocation.getCurrentPosition(
                (pos) => resolve({
                    lat: pos.coords.latitude,
                    lng: pos.coords.longitude
                })
            );
        })",
        new ContextTarget(contextId),
        true));

if (result is EvaluateResultSuccess success)
{
    var location = success.Result.ValueAs<Dictionary<string, object>>();
    Console.WriteLine($"Browser location: {location["lat"]}, {location["lng"]}");
}
```

### Common Locations

```csharp
// New York
await driver.Emulation.SetGeolocationAsync(
    new SetGeolocationCommandParameters(contextId)
    {
        Latitude = 40.7128,
        Longitude = -74.0060,
        Accuracy = 100
    });

// London
await driver.Emulation.SetGeolocationAsync(
    new SetGeolocationCommandParameters(contextId)
    {
        Latitude = 51.5074,
        Longitude = -0.1278,
        Accuracy = 100
    });

// Tokyo
await driver.Emulation.SetGeolocationAsync(
    new SetGeolocationCommandParameters(contextId)
    {
        Latitude = 35.6762,
        Longitude = 139.6503,
        Accuracy = 100
    });
```

## Timezone Emulation

### Set Timezone

```csharp
SetTimezoneCommandParameters params = new SetTimezoneCommandParameters(contextId)
{
    TimezoneId = "America/Los_Angeles"
};

await driver.Emulation.SetTimezoneAsync(params);
Console.WriteLine("Timezone set to Pacific Time");

// Verify timezone
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "Intl.DateTimeFormat().resolvedOptions().timeZone",
        new ContextTarget(contextId),
        true));

if (result is EvaluateResultSuccess success)
{
    string timezone = success.Result.ValueAs<string>();
    Console.WriteLine($"Browser timezone: {timezone}");
}
```

### Common Timezones

```csharp
// Pacific Time (US West Coast)
await driver.Emulation.SetTimezoneAsync(
    new SetTimezoneCommandParameters(contextId)
    {
        TimezoneId = "America/Los_Angeles"
    });

// Eastern Time (US East Coast)
await driver.Emulation.SetTimezoneAsync(
    new SetTimezoneCommandParameters(contextId)
    {
        TimezoneId = "America/New_York"
    });

// UTC
await driver.Emulation.SetTimezoneAsync(
    new SetTimezoneCommandParameters(contextId)
    {
        TimezoneId = "UTC"
    });

// Tokyo
await driver.Emulation.SetTimezoneAsync(
    new SetTimezoneCommandParameters(contextId)
    {
        TimezoneId = "Asia/Tokyo"
    });
```

## Locale Emulation

### Set Locale

```csharp
SetLocaleCommandParameters params = new SetLocaleCommandParameters(contextId)
{
    Locale = "fr-FR"
};

await driver.Emulation.SetLocaleAsync(params);
Console.WriteLine("Locale set to French");

// Verify locale
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "navigator.language",
        new ContextTarget(contextId),
        true));

if (result is EvaluateResultSuccess success)
{
    string locale = success.Result.ValueAs<string>();
    Console.WriteLine($"Browser locale: {locale}");
}
```

## Common Patterns

### Pattern: Mobile Device Emulation

```csharp
// Complete mobile device emulation
public async Task EmulateMobileDevice(
    BiDiDriver driver, 
    string contextId,
    string deviceName)
{
    switch (deviceName.ToLower())
    {
        case "iphone":
            await driver.Emulation.SetViewportAsync(
                new SetViewportCommandParameters(contextId)
                {
                    Width = 390,
                    Height = 844,
                    DevicePixelRatio = 3.0
                });
            
            await driver.Emulation.SetUserAgentAsync(
                new SetUserAgentCommandParameters(contextId)
                {
                    UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 Mobile/15E148 Safari/604.1"
                });
            break;

        case "android":
            await driver.Emulation.SetViewportAsync(
                new SetViewportCommandParameters(contextId)
                {
                    Width = 412,
                    Height = 915,
                    DevicePixelRatio = 2.625
                });
            
            await driver.Emulation.SetUserAgentAsync(
                new SetUserAgentCommandParameters(contextId)
                {
                    UserAgent = "Mozilla/5.0 (Linux; Android 12; Pixel 6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.45 Mobile Safari/537.36"
                });
            break;

        case "tablet":
            await driver.Emulation.SetViewportAsync(
                new SetViewportCommandParameters(contextId)
                {
                    Width = 1024,
                    Height = 1366,
                    DevicePixelRatio = 2.0
                });
            break;
    }
    
    Console.WriteLine($"Emulating {deviceName}");
}

// Usage
await EmulateMobileDevice(driver, contextId, "iphone");
```

### Pattern: Responsive Testing

```csharp
// Test at multiple viewport sizes
List<(int Width, int Height, string Name)> viewports = new()
{
    (320, 568, "Mobile Small"),
    (375, 667, "Mobile Medium"),
    (414, 896, "Mobile Large"),
    (768, 1024, "Tablet"),
    (1920, 1080, "Desktop")
};

foreach (var viewport in viewports)
{
    Console.WriteLine($"\nTesting {viewport.Name} ({viewport.Width}x{viewport.Height})");
    
    await driver.Emulation.SetViewportAsync(
        new SetViewportCommandParameters(contextId)
        {
            Width = viewport.Width,
            Height = viewport.Height,
            DevicePixelRatio = 1.0
        });

    // Take screenshot
    CaptureScreenshotCommandResult screenshot = 
        await driver.BrowsingContext.CaptureScreenshotAsync(
            new CaptureScreenshotCommandParameters(contextId));
    
    byte[] imageBytes = Convert.FromBase64String(screenshot.Data);
    await File.WriteAllBytesAsync(
        $"screenshot-{viewport.Name.Replace(" ", "-")}.png", 
        imageBytes);
}
```

### Pattern: Dark Mode Testing

```csharp
// Test both light and dark modes
string[] colorSchemes = { "light", "dark" };

foreach (string scheme in colorSchemes)
{
    Console.WriteLine($"\nTesting {scheme} mode");
    
    SetEmulatedMediaCommandParameters params = 
        new SetEmulatedMediaCommandParameters(contextId);
    params.Features.Add(new MediaFeature("prefers-color-scheme", scheme));
    
    await driver.Emulation.SetEmulatedMediaAsync(params);

    // Navigate to page
    await driver.BrowsingContext.NavigateAsync(
        new NavigateCommandParameters(contextId, "https://example.com")
        { Wait = ReadinessState.Complete });

    // Verify color scheme applied
    EvaluateResult result = await driver.Script.EvaluateAsync(
        new EvaluateCommandParameters(
            "getComputedStyle(document.body).backgroundColor",
            new ContextTarget(contextId),
            true));

    if (result is EvaluateResultSuccess success)
    {
        string bgColor = success.Result.ValueAs<string>();
        Console.WriteLine($"Background color: {bgColor}");
    }

    // Take screenshot
    CaptureScreenshotCommandResult screenshot = 
        await driver.BrowsingContext.CaptureScreenshotAsync(
            new CaptureScreenshotCommandParameters(contextId));
    
    byte[] imageBytes = Convert.FromBase64String(screenshot.Data);
    await File.WriteAllBytesAsync($"screenshot-{scheme}.png", imageBytes);
}
```

### Pattern: Location-Based Testing

```csharp
// Test application behavior in different locations
Dictionary<string, (double Lat, double Lng)> locations = new()
{
    { "New York", (40.7128, -74.0060) },
    { "London", (51.5074, -0.1278) },
    { "Tokyo", (35.6762, 139.6503) },
    { "Sydney", (-33.8688, 151.2093) }
};

foreach (var location in locations)
{
    Console.WriteLine($"\nTesting from {location.Key}");
    
    await driver.Emulation.SetGeolocationAsync(
        new SetGeolocationCommandParameters(contextId)
        {
            Latitude = location.Value.Lat,
            Longitude = location.Value.Lng,
            Accuracy = 100
        });

    // Navigate to location-aware page
    await driver.BrowsingContext.NavigateAsync(
        new NavigateCommandParameters(contextId, "https://example.com/location")
        { Wait = ReadinessState.Complete });

    // Check location detection
    EvaluateResult result = await driver.Script.EvaluateAsync(
        new EvaluateCommandParameters(
            "document.querySelector('#detected-location')?.textContent",
            new ContextTarget(contextId),
            true));

    if (result is EvaluateResultSuccess success)
    {
        string? detectedLocation = success.Result.ValueAs<string>();
        Console.WriteLine($"Detected: {detectedLocation}");
    }
}
```

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

