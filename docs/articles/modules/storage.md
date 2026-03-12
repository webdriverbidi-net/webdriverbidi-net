# Storage Module

The Storage module provides functionality for managing cookies.

## Overview

The Storage module allows you to:

- Get, set, and delete cookies

## Accessing the Module

[!code-csharp[Accessing Module](../../code/modules/StorageModuleSamples.cs#AccessingModule)]

## Timeout and Cancellation

All commands in this module accept optional `timeoutOverride` and `CancellationToken` parameters. Use `timeoutOverride` to set a per-command timeout (defaults to `BiDiDriver.DefaultCommandTimeout` when omitted). Use `CancellationToken` for cooperative cancellation. See the [API Design Guide](../advanced/api-design.md#timeout-and-cancellation) for details and examples.

## Working with Cookies

### Get All Cookies

[!code-csharp[Get All Cookies](../../code/modules/StorageModuleSamples.cs#GetAllCookies)]

### Get Cookies by Filter

[!code-csharp[Get Cookies by Filter](../../code/modules/StorageModuleSamples.cs#GetCookiesbyFilter)]

### Set Cookie

[!code-csharp[Set Cookie](../../code/modules/StorageModuleSamples.cs#SetCookie)]

### Set Cookie with Expiry

[!code-csharp[Set Cookie with Expiry](../../code/modules/StorageModuleSamples.cs#SetCookiewithExpiry)]

### Delete Cookie

[!code-csharp[Delete Cookie](../../code/modules/StorageModuleSamples.cs#DeleteCookie)]

### Delete All Cookies

[!code-csharp[Delete All Cookies](../../code/modules/StorageModuleSamples.cs#DeleteAllCookies)]

## Common Patterns

### Save and Restore Session

[!code-csharp[Save and Restore Session](../../code/modules/StorageModuleSamples.cs#SaveandRestoreSession)]

### Clean State Between Tests

[!code-csharp[Clean State Between Tests](../../code/modules/StorageModuleSamples.cs#CleanStateBetweenTests)]

### Set Authentication Cookie

[!code-csharp[Set Authentication Cookie](../../code/modules/StorageModuleSamples.cs#SetAuthenticationCookie)]

## Best Practices

1. **Set cookies before navigation**: Set cookies before navigating to the domain
2. **Match domains correctly**: Cookie domain must match the current page domain
3. **Use appropriate SameSite**: Choose `Strict`, `Lax`, or `None` based on needs
4. **Clean up between tests**: Clear cookies and storage for test isolation
5. **Handle secure cookies**: Set `Secure` flag for HTTPS-only cookies

## Next Steps

- [Browser Module](browser.md): Managing user contexts
- [Network Module](network.md): Monitoring cookie-related network traffic
- [API Reference](../../api/index.md): Complete API documentation

