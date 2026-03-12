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

[!code-csharp[Accessing Module](../../code/modules/UserAgentClientHintsModuleSamples.cs#AccessingModule)]

## Setting Client Hints Override

### Basic Override

[!code-csharp[Basic Override](../../code/modules/UserAgentClientHintsModuleSamples.cs#BasicOverride)]

### Common Browser Brands

[!code-csharp[Common Browser Brands](../../code/modules/UserAgentClientHintsModuleSamples.cs#CommonBrowserBrands)]

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

[!code-csharp[Reset Client Hints](../../code/modules/UserAgentClientHintsModuleSamples.cs#ResetClientHints)]

**Note**: This command always requires explicit parameters. You must pass either `ResetClientHintsOverride` to clear or a configured `SetClientHintsOverrideCommandParameters` instance to set overrides.

## Scoping Overrides

### Target Specific Browsing Contexts

[!code-csharp[Target Specific Contexts](../../code/modules/UserAgentClientHintsModuleSamples.cs#TargetSpecificContexts)]

### Target Specific User Contexts

[!code-csharp[Target Specific User Contexts](../../code/modules/UserAgentClientHintsModuleSamples.cs#TargetSpecificUserContexts)]

When `Contexts` or `UserContexts` is `null`, the override applies to all contexts. When set to an empty list, it applies to no contexts (effectively clearing for those scopes).

## Common Patterns

### Pattern: Mobile Device Testing

[!code-csharp[Mobile Device Testing](../../code/modules/UserAgentClientHintsModuleSamples.cs#MobileDeviceTesting)]

### Pattern: Cross-Browser Brand Testing

[!code-csharp[Cross-Browser Brand Testing](../../code/modules/UserAgentClientHintsModuleSamples.cs#Cross-BrowserBrandTesting)]

### Pattern: Verify Client Hints in Page

[!code-csharp[Verify Client Hints in Page](../../code/modules/UserAgentClientHintsModuleSamples.cs#VerifyClientHintsinPage)]

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
