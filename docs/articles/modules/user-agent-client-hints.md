# UserAgentClientHints Module

The UserAgentClientHints module provides functionality for overriding user agent client hints in the browser. Client hints are a set of HTTP request headers that allow servers to request device and browser information, enabling responsive design and feature detection without relying solely on the User-Agent string.

## Overview

The UserAgentClientHints module allows you to:

- Override user agent client hints (brands, platform, architecture, mobile flag, etc.)
- Emulate different browser brands and versions for testing
- Scope overrides to specific browsing contexts or user contexts
- Reset overrides to restore default browser behavior

This module complements the [Emulation Module](emulation.md), which provides user agent string override. Client hints offer a more granular, structured way to control the information websites receive about the browser environment.

## Accessing the Module

```csharp
UserAgentClientHintsModule userAgentClientHints = driver.UserAgentClientHints;
```

## Setting Client Hints Override

### Basic Override

```csharp
SetClientHintsOverrideCommandParameters parameters = new SetClientHintsOverrideCommandParameters();
parameters.ClientHints = new ClientHintsMetadata
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

await driver.UserAgentClientHints.SetClientHintsOverrideAsync(parameters);
Console.WriteLine("Client hints override set");
```

### Common Browser Brands

```csharp
// Chrome on Windows
parameters.ClientHints = new ClientHintsMetadata
{
    Brands = new List<BrandVersion>
    {
        new BrandVersion("Chromium", "120.0"),
        new BrandVersion("Google Chrome", "120.0")
    },
    FullVersionList = new List<BrandVersion>
    {
        new BrandVersion("Chromium", "120.0.6099.109"),
        new BrandVersion("Google Chrome", "120.0.6099.109")
    },
    Platform = "Windows",
    PlatformVersion = "10.0",
    Architecture = "x86",
    Model = "",
    Mobile = false,
    Bitness = "64"
};

// Firefox on macOS
parameters.ClientHints = new ClientHintsMetadata
{
    Brands = new List<BrandVersion>
    {
        new BrandVersion("Not_A Brand", "8"),
        new BrandVersion("Firefox", "121.0")
    },
    Platform = "macOS",
    PlatformVersion = "14.0",
    Architecture = "arm",
    Mobile = false
};

// Mobile Chrome (Android)
parameters.ClientHints = new ClientHintsMetadata
{
    Brands = new List<BrandVersion>
    {
        new BrandVersion("Chromium", "120.0"),
        new BrandVersion("Google Chrome", "120.0")
    },
    Platform = "Android",
    PlatformVersion = "14.0",
    Architecture = "arm",
    Model = "Pixel 7",
    Mobile = true
};

await driver.UserAgentClientHints.SetClientHintsOverrideAsync(parameters);
```

## Client Hints Metadata Properties

The `ClientHintsMetadata` class supports the following properties:

| Property | Type | Description |
|----------|------|-------------|
| `Brands` | `List<BrandVersion>?` | Browser brand and version pairs (e.g., Chromium/120.0) |
| `FullVersionList` | `List<BrandVersion>?` | Full version strings for each brand |
| `Platform` | `string?` | Platform name (Windows, macOS, Android, Linux) |
| `PlatformVersion` | `string?` | Platform version |
| `Architecture` | `string?` | CPU architecture (x86, arm) |
| `Model` | `string?` | Device model (for mobile) |
| `Mobile` | `bool?` | Whether the device is mobile |
| `Bitness` | `string?` | Pointer size (32, 64) |
| `Wow64` | `bool?` | Whether running under WOW64 (Windows) |
| `FormFactors` | `List<string>?` | Device form factors |

All properties are optional. Omitted properties are not sent in the command and retain their default behavior.

## Resetting Client Hints

To clear the client hints override and restore default browser behavior, use the `ResetClientHintsOverride` static property:

```csharp
await driver.UserAgentClientHints.SetClientHintsOverrideAsync(
    SetClientHintsOverrideCommandParameters.ResetClientHintsOverride);
Console.WriteLine("Client hints override cleared");
```

**Note**: This command always requires explicit parameters. You must pass either `ResetClientHintsOverride` to clear or a configured `SetClientHintsOverrideCommandParameters` instance to set overrides.

## Scoping Overrides

### Target Specific Browsing Contexts

```csharp
SetClientHintsOverrideCommandParameters parameters = new SetClientHintsOverrideCommandParameters();
parameters.ClientHints = new ClientHintsMetadata
{
    Brands = new List<BrandVersion> { new BrandVersion("Chromium", "120.0") },
    Mobile = true
};
parameters.Contexts = new List<string> { contextId };

await driver.UserAgentClientHints.SetClientHintsOverrideAsync(parameters);
```

### Target Specific User Contexts

```csharp
SetClientHintsOverrideCommandParameters parameters = new SetClientHintsOverrideCommandParameters();
parameters.ClientHints = new ClientHintsMetadata
{
    Platform = "Linux",
    Architecture = "x86"
};
parameters.UserContexts = new List<string> { userContextId };

await driver.UserAgentClientHints.SetClientHintsOverrideAsync(parameters);
```

When `Contexts` or `UserContexts` is `null`, the override applies to all contexts. When set to an empty list, it applies to no contexts (effectively clearing for those scopes).

## Common Patterns

### Pattern: Mobile Device Testing

```csharp
// Emulate mobile client hints for responsive design testing
SetClientHintsOverrideCommandParameters parameters = new SetClientHintsOverrideCommandParameters();
parameters.ClientHints = new ClientHintsMetadata
{
    Brands = new List<BrandVersion>
    {
        new BrandVersion("Chromium", "120.0"),
        new BrandVersion("Google Chrome", "120.0")
    },
    Platform = "Android",
    PlatformVersion = "14.0",
    Architecture = "arm",
    Model = "Pixel 7",
    Mobile = true,
    FormFactors = new List<string> { "Mobile" }
};

await driver.UserAgentClientHints.SetClientHintsOverrideAsync(parameters);

// Combine with Emulation module for full mobile emulation
await driver.Emulation.SetViewportAsync(
    new SetViewportCommandParameters(contextId)
    {
        Width = 412,
        Height = 915,
        DevicePixelRatio = 2.625
    });
```

### Pattern: Cross-Browser Brand Testing

```csharp
// Test how a site behaves with different browser brands
Dictionary<string, ClientHintsMetadata> browserConfigs = new()
{
    ["Chrome"] = new ClientHintsMetadata
    {
        Brands = new List<BrandVersion>
        {
            new BrandVersion("Chromium", "120.0"),
            new BrandVersion("Google Chrome", "120.0")
        },
        Platform = "Windows",
        Mobile = false
    },
    ["Edge"] = new ClientHintsMetadata
    {
        Brands = new List<BrandVersion>
        {
            new BrandVersion("Chromium", "120.0"),
            new BrandVersion("Microsoft Edge", "120.0")
        },
        Platform = "Windows",
        Mobile = false
    },
    ["Safari"] = new ClientHintsMetadata
    {
        Brands = new List<BrandVersion>
        {
            new BrandVersion("Safari", "17.0")
        },
        Platform = "macOS",
        Mobile = false
    }
};

foreach (var config in browserConfigs)
{
    Console.WriteLine($"\nTesting as {config.Key}");
    SetClientHintsOverrideCommandParameters parameters = new SetClientHintsOverrideCommandParameters();
    parameters.ClientHints = config.Value;
    await driver.UserAgentClientHints.SetClientHintsOverrideAsync(parameters);

    await driver.BrowsingContext.NavigateAsync(
        new NavigateCommandParameters(contextId, "https://example.com")
        { Wait = ReadinessState.Complete });

    // Verify site behavior for this browser brand
    EvaluateResult result = await driver.Script.EvaluateAsync(
        new EvaluateCommandParameters(
            "navigator.userAgentData?.brands?.map(b => b.brand).join(', ') ?? 'not supported'",
            new ContextTarget(contextId),
            true));

    if (result is EvaluateResultSuccess success)
    {
        string brands = success.Result.ValueAs<string>();
        Console.WriteLine($"Detected brands: {brands}");
    }
}
```

### Pattern: Verify Client Hints in Page

```csharp
// Set override
SetClientHintsOverrideCommandParameters parameters = new SetClientHintsOverrideCommandParameters();
parameters.ClientHints = new ClientHintsMetadata
{
    Brands = new List<BrandVersion> { new BrandVersion("TestBrowser", "1.0") },
    Platform = "TestOS",
    Mobile = true
};
await driver.UserAgentClientHints.SetClientHintsOverrideAsync(parameters);

// Verify via User-Agent Client Hints API (JavaScript)
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        @"(async () => {
            const ua = navigator.userAgentData;
            if (!ua) return 'User-Agent Client Hints API not supported';
            const hints = await ua.getHighEntropyValues(['brands', 'platform', 'mobile']);
            return JSON.stringify(hints);
        })()",
        new ContextTarget(contextId),
        true));

if (result is EvaluateResultSuccess success)
{
    string hintsJson = success.Result.ValueAs<string>();
    Console.WriteLine($"Client hints: {hintsJson}");
}
```

## Best Practices

1. **Combine with Emulation**: For full device emulation, use both the UserAgentClientHints module and the Emulation module (viewport, user agent string).
2. **Reset between tests**: Clear overrides between test cases to ensure isolation using `ResetClientHintsOverride`.
3. **Set before navigation**: Apply overrides before navigating to pages that may use client hints for feature detection.
4. **Match brand and version**: Ensure `Brands` and `FullVersionList` are consistent when both are used.
5. **Verify support**: The User-Agent Client Hints API is not supported in all browsers; check `navigator.userAgentData` before relying on hints in your tests.

## Limitations

- Client hints override affects HTTP request headers and the `navigator.userAgentData` JavaScript API; the traditional `navigator.userAgent` string is controlled by the Emulation module.
- Support varies by browser; Chrome and Edge have full support, Firefox and Safari have varying levels of support.
- Some websites may use additional detection methods beyond client hints.

## Common Issues

### Client Hints Not Applied

**Problem**: Website doesn't detect the overridden client hints.

**Solution**:
- Set the override before navigating to the page
- Ensure the page uses the User-Agent Client Hints API (`navigator.userAgentData`) or that the server reads the `Sec-CH-*` request headers
- Verify browser support for client hints

### Inconsistent with User-Agent String

**Problem**: Client hints and User-Agent string don't match.

**Solution**:
- Use the Emulation module's `SetUserAgentAsync` in conjunction with `SetClientHintsOverrideAsync`
- Ensure brands, platform, and mobile flag align between both overrides

### Override Not Clearing

**Problem**: Override persists after calling reset.

**Solution**:
- Use `SetClientHintsOverrideCommandParameters.ResetClientHintsOverride` explicitly
- Ensure you're not passing a parameters object with `ClientHints` set to an empty `ClientHintsMetadata` (use the reset property instead)

## Next Steps

- [Emulation Module](emulation.md): User agent string and viewport emulation
- [Browsing Context Module](browsing-context.md): Context management and navigation
- [Network Module](network.md): Network interception and headers
- [API Reference](../../api/index.md): Complete API documentation

## Further Reading

- [User-Agent Client Hints](https://developer.mozilla.org/en-US/docs/Web/HTTP/Client_hints#user-agent_client_hints)
- [Navigator.userAgentData](https://developer.mozilla.org/en-US/docs/Web/API/Navigator/userAgentData)
- [W3C User-Agent Client Hints](https://www.w3.org/TR/user-agent-client-hints/)
