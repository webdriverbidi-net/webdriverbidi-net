# Digital Credentials Module

The Digital Credentials module allows you to simulate a virtual digital wallet during automated testing, enabling end-to-end verification of credential presentation flows without requiring a real wallet application.

## Overview

The Digital Credentials module allows you to:

- Simulate a wallet that presents a predefined credential response
- Simulate a wallet that declines or cancels a credential request
- Leave credential requests in a pending state to test timeouts
- Scope wallet behavior to a specific browsing context
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

`Protocol` and `Response` are a required pair for `VirtualWalletAction.Respond`: a conforming remote end
answers with `invalid argument` if either is missing. Every other action requires both to be *absent*, so
`Decline`, `Wait` and `Clear` are sent with neither.

The `Response` property accepts any `Dictionary<string, object?>` whose shape matches the credential
response your application expects for the protocol you name.

### Choosing the Credential Protocol

`Protocol` is not a filter. It names the protocol the simulated credential is presented under, and its
value is written into the credential the page receives as `DigitalCredential.protocol`:

[!code-csharp[Select Protocol](../../code/modules/DigitalCredentialsModuleSamples.cs#SelectProtocol)]

The value must be one of the identifiers enumerated by `DigitalCredentialProtocol` in the
[Digital Credentials API](https://www.w3.org/TR/digital-credentials/); anything else is rejected with
`invalid argument`. At the time of writing those are:

| Identifier | Kind |
|------------|------|
| `openid4vp-v1-unsigned` | Presentation |
| `openid4vp-v1-signed` | Presentation |
| `openid4vp-v1-multisigned` | Presentation |
| `org-iso-mdoc` | Presentation |
| `openid4vci-v1` | Issuance |

Identifiers are added to that enumeration as user agents adopt new protocols, so check the specification
rather than treating this list as closed.

### Leaving the Request Pending

Simulate an active, unresolved wallet prompt to test timeout handling or concurrent request behavior:

[!code-csharp[Simulate Wallet Wait](../../code/modules/DigitalCredentialsModuleSamples.cs#SimulateWalletWait)]

### Clearing the Active Behavior

Remove any previously configured virtual wallet behavior, returning to the browser's default handling:

[!code-csharp[Clear Wallet Behavior](../../code/modules/DigitalCredentialsModuleSamples.cs#ClearWalletBehavior)]

## Scoping Behavior

The browsing context is the only axis the command scopes by. Apply the wallet behavior to a specific tab
or frame by setting the `BrowsingContextId` property; omit it and the behavior becomes the session
default:

[!code-csharp[Scope to Context](../../code/modules/DigitalCredentialsModuleSamples.cs#ScopeToContext)]

## VirtualWalletAction Values

| Value | Description | `Protocol` and `Response` |
|-------|-------------|---------------------------|
| `VirtualWalletAction.Decline` | Simulates a user cancellation; the credential request is aborted | Both must be omitted |
| `VirtualWalletAction.Respond` | Returns the object supplied in `Response` as the credential data | Both are required |
| `VirtualWalletAction.Wait` | Leaves the active promise unsettled; useful for timeout or concurrency tests | Both must be omitted |
| `VirtualWalletAction.Clear` | Removes any active virtual wallet behavior | Both must be omitted |

Supplying either property with an action other than `Respond`, or omitting either one with `Respond`, is
answered with `invalid argument`.

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
4. **Match the response shape to your protocol**: `Respond` requires `Protocol` as well as `Response`, and the `Response` dictionary must conform to the structure expected by the protocol you name.
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
- Verify the `Response` dictionary matches the schema your application expects for the protocol named in `Protocol`.
- Confirm `Protocol` names the protocol the page actually requested; its value reaches the page as `DigitalCredential.protocol`.
- Check browser console errors for schema validation messages.

### Behavior Persists After Test

**Problem**: Wallet behavior configured in one test affects subsequent tests.

**Solution**:
- Call `SetVirtualWalletBehaviorAsync` with `VirtualWalletAction.Clear` in your test teardown.
- Use an isolated browsing context for each test; the command scopes only by browsing context, via the `BrowsingContextId` property.

## Next Steps

- [Permissions Module](permissions.md): Browser permission management
- [Emulation Module](emulation.md): Device and environment emulation
- [Browser Module](browser.md): User context management
- [API Reference](../../api/index.md): Complete API documentation

## Further Reading

- [W3C Digital Credentials Specification](https://www.w3.org/TR/digital-credentials/)
- [Credential Management API](https://developer.mozilla.org/en-US/docs/Web/API/Credential_Management_API)
- [W3C WebDriver BiDi Specification](https://w3c.github.io/webdriver-bidi/)
