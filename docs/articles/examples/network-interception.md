# Network Interception Example

This example demonstrates how to intercept, modify, and replace network requests and responses using WebDriverBiDi.NET.

## Overview

This example shows:
- Intercepting network requests before they're sent
- Blocking specific requests (ad blocking)
- Providing custom responses (mocking APIs)
- Modifying request headers
- Capturing request and response bodies

## Example 1: Basic Network Interception

[!code-csharp[Basic Network Interception](../../code/examples/NetworkInterceptionSamples.cs#BasicNetworkInterception)]

## Example 2: Blocking Ad Domains

[!code-csharp[Blocking Ad Domains](../../code/examples/NetworkInterceptionSamples.cs#BlockingAdDomains)]

## Example 3: Mocking API Responses

[!code-csharp[Mocking API Responses](../../code/examples/NetworkInterceptionSamples.cs#MockingAPIResponses)]

## Example 4: Adding Custom Headers

[!code-csharp[Adding Custom Headers](../../code/examples/NetworkInterceptionSamples.cs#AddingCustomHeaders)]

## Example 5: Capturing Response Bodies

[!code-csharp[Capturing Response Bodies](../../code/examples/NetworkInterceptionSamples.cs#CapturingResponseBodies)]

## Example 6: Slow Down Specific Resources

[!code-csharp[Slow Down Specific Resources](../../code/examples/NetworkInterceptionSamples.cs#SlowDownSpecificResources)]

## Example 7: Redirect Requests

[!code-csharp[Redirect Requests](../../code/examples/NetworkInterceptionSamples.cs#RedirectRequests)]

## Pattern: Conditional Interception

[!code-csharp[Conditional Interception](../../code/examples/NetworkInterceptionSamples.cs#ConditionalInterception)]

## Best Practices

1. **Use async handlers**: Network interception requires calling commands within handlers
2. **Handle all intercepted requests**: Always call Continue, Fail, or ProvideResponse
3. **Clean up intercepts**: Remove intercepts when done with `RemoveInterceptAsync`
4. **Manage data collectors**: Remove collectors to free memory
5. **Use URL patterns**: Limit interception scope for better performance
6. **Test error cases**: Handle network failures gracefully

## Common Issues

### Requests Hang

**Problem**: Intercepted requests never complete.

**Solution**: Ensure every intercepted request (when `IsBlocked` is true) calls Continue, Fail, or ProvideResponse.

### Memory Issues

**Problem**: Memory usage grows over time.

**Solution**: Set `DisownCollectedData = true` when getting response bodies.

### Timing Problems

**Problem**: Intercept not capturing requests.

**Solution**: Set up intercept before navigating to the page.

## Next Steps

- [Network Module](../modules/network.md): Complete network module guide
- [Events and Observables](../events-observables.md): Understanding event handling
- [Common Scenarios](common-scenarios.md): More examples

