# Digital Credentials Module

The Digital Credentials module allows you to simulate a virtual digital wallet during automated testing, enabling end-to-end verification of credential presentation flows without requiring a real wallet application.

## Overview

The Digital Credentials module allows you to:

- Simulate a wallet that presents a predefined credential response
- Simulate a wallet that declines or cancels a credential request
- Leave credential requests in a pending state to test timeouts
- Scope wallet behavior to a specific browsing context or credential protocol
- Clear any active simulated wallet behavior

## Accessing the Module

[!code-csharp[Accessing Module](../../code/modules/DigitalCredentialsModuleSamples.cs#AccessingModule)]

## Timeout and Cancellation

All commands in this module accept optional `timeoutOverride` and `CancellationToken` parameters. Use `timeoutOverride` to set a per-command timeout (defaults to `BiDiDriver.DefaultCommandTimeout` when omitted). Use `CancellationToken` for cooperative cancellation. See the [API Design Guide](../advanced/api-design.md#timeout-and-cancellation) for details and examples.

## Setting the Virtual Wallet Behavior

The module exposes a single command, `SetVirtualWalletBehaviorAsync`, which configures how the browser's virtual wallet responds to a [`navigator.credentials.get()`](https://developer.mozilla.org/en-US/docs/Web/API/CredentialsContainer/get) call that uses the Digital Credentials API.

### Declining a Credential Request

Simulate a user who cancels or rejects the wallet prompt:

[!code-csharp[Decline Credential Request](../../code/modules/DigitalCredentialsModuleSamples.cs#DeclineCredentialRequest)]

### Responding with a Credential

Simulate a successful presentation by returning a predefined response object:

[!code-csharp[Respond with Credential](../../code/modules/DigitalCredentialsModuleSamples.cs#RespondWithCredential)]

The `Response` property accepts any `Dictionary<string, object?>` whose shape matches the protocol-specific credential response your application expects.

### Leaving the Request Pending

Simulate an active, unresolved wallet prompt to test timeout handling or concurrent request behavior:

[!code-csharp[Simulate Wallet Wait](../../code/modules/DigitalCredentialsModuleSamples.cs#SimulateWalletWait)]

### Clearing the Active Behavior

Remove any previously configured virtual wallet behavior, returning to the browser's default handling:

[!code-csharp[Clear Wallet Behavior](../../code/modules/DigitalCredentialsModuleSamples.cs#ClearWalletBehavior)]

## Scoping Behavior

### Scoping to a Browsing Context

Apply the wallet behavior only to a specific tab or frame by setting the `Context` property:

[!code-csharp[Scope to Context](../../code/modules/DigitalCredentialsModuleSamples.cs#ScopeToContext)]

### Scoping to a Protocol

Target a specific credential exchange protocol (e.g., `"openid4vp"`, `"preview"`) with the `Protocol` property:

[!code-csharp[Scope to Protocol](../../code/modules/DigitalCredentialsModuleSamples.cs#ScopeToProtocol)]

When `Protocol` is omitted the behavior applies to all credential protocols.

## VirtualWalletAction Values

| Value | Description |
|-------|-------------|
| `VirtualWalletAction.Decline` | Simulates a user cancellation; the credential request is aborted |
| `VirtualWalletAction.Respond` | Returns the object supplied in `Response` as the credential data |
| `VirtualWalletAction.Wait` | Leaves the active promise unsettled; useful for timeout or concurrency tests |
| `VirtualWalletAction.Clear` | Removes any active virtual wallet behavior |

## Common Patterns

### Testing a Declined Credential Request

[!code-csharp[Test Declined Credential](../../code/modules/DigitalCredentialsModuleSamples.cs#TestDeclinedCredential)]

### Testing a Successful Credential Flow

[!code-csharp[Test Successful Credential Flow](../../code/modules/DigitalCredentialsModuleSamples.cs#TestSuccessfulCredentialFlow)]

## Browser Support

| Browser | Support Level |
|---------|---------------|
| Chrome/Edge | ⚠️ Experimental |
| Firefox | ❌ Not supported |
| Safari | ❌ Not supported |

**Note**: The Digital Credentials API and its WebDriver BiDi test automation support are experimental. Check your browser's release notes for availability.

## Best Practices

1. **Set behavior before navigation**: Configure the virtual wallet before navigating to the page that triggers a credential request.
2. **Clear behavior between tests**: Call `SetVirtualWalletBehaviorAsync` with `VirtualWalletAction.Clear` after each test to avoid state leaking between test cases.
3. **Use context scoping for isolation**: When running multiple contexts in parallel, scope the wallet behavior to a specific context to prevent interference.
4. **Match the response shape to your protocol**: The `Response` dictionary must conform to the structure expected by the credential protocol your application uses.
5. **Test all action paths**: Verify your application handles `Decline`, `Respond`, and `Wait` (timeout) outcomes.

## Common Issues

### Credential Request Not Intercepted

**Problem**: The page's credential request is not intercepted by the virtual wallet.

**Solution**:
- Ensure the browser version supports the Digital Credentials API.
- Set the wallet behavior *before* the page issues the credential request.
- Confirm the browser was launched with any required experimental feature flags.

### Response Shape Rejected by the Page

**Problem**: The page throws an error when processing the credential response.

**Solution**:
- Verify the `Response` dictionary matches the protocol-specific schema your application expects.
- Use the `Protocol` property to scope the response to the correct credential exchange protocol.
- Check browser console errors for schema validation messages.

### Behavior Persists After Test

**Problem**: Wallet behavior configured in one test affects subsequent tests.

**Solution**:
- Call `SetVirtualWalletBehaviorAsync` with `VirtualWalletAction.Clear` in your test teardown.
- Use isolated browsing contexts or user contexts for each test.

## Next Steps

- [Permissions Module](permissions.md): Browser permission management
- [Emulation Module](emulation.md): Device and environment emulation
- [Browser Module](browser.md): User context management
- [API Reference](../../api/index.md): Complete API documentation

## Further Reading

- [W3C Digital Credentials Specification](https://www.w3.org/TR/digital-credentials/)
- [Credential Management API](https://developer.mozilla.org/en-US/docs/Web/API/Credential_Management_API)
- [W3C WebDriver BiDi Specification](https://w3c.github.io/webdriver-bidi/)
