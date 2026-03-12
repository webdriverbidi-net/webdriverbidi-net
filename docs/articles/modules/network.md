# Network Module

The Network module provides comprehensive control over HTTP/HTTPS traffic, including monitoring requests and responses, intercepting network calls, and capturing response bodies.

## Overview

The Network module enables you to:

- Monitor all network requests and responses
- Intercept requests before they're sent
- Modify or block network traffic
- Capture request and response bodies
- Handle authentication challenges
- Track network errors

## Accessing the Module

[!code-csharp[Accessing Module](../../code/modules/NetworkModuleSamples.cs#AccessingModule)]

## Timeout and Cancellation

All commands in this module accept optional `timeoutOverride` and `CancellationToken` parameters. Use `timeoutOverride` to set a per-command timeout (defaults to `BiDiDriver.DefaultCommandTimeout` when omitted). Use `CancellationToken` for cooperative cancellation. See the [API Design Guide](../advanced/api-design.md#timeout-and-cancellation) for details and examples.

## Monitoring Network Traffic

### Basic Response Monitoring

[!code-csharp[Basic Response Monitoring](../../code/modules/NetworkModuleSamples.cs#BasicResponseMonitoring)]

### Monitor Request Details

[!code-csharp[Monitor Request Details](../../code/modules/NetworkModuleSamples.cs#MonitorRequestDetails)]

### Filter by Content Type

[!code-csharp[Filter by Content Type](../../code/modules/NetworkModuleSamples.cs#FilterbyContentType)]

### Filter by URL Pattern

[!code-csharp[Filter by URL Pattern](../../code/modules/NetworkModuleSamples.cs#FilterbyURLPattern)]

## Network Events

### BeforeRequestSent

Fired when a request is about to be sent:

[!code-csharp[BeforeRequestSent](../../code/modules/NetworkModuleSamples.cs#BeforeRequestSent)]

### ResponseStarted

Fired when response headers are received:

[!code-csharp[ResponseStarted](../../code/modules/NetworkModuleSamples.cs#ResponseStarted)]

### ResponseCompleted

Fired when response is fully received:

[!code-csharp[ResponseCompleted](../../code/modules/NetworkModuleSamples.cs#ResponseCompleted)]

### FetchError

Fired when a network error occurs:

[!code-csharp[FetchError](../../code/modules/NetworkModuleSamples.cs#FetchError)]

### AuthRequired

Fired when authentication is needed:

[!code-csharp[AuthRequired](../../code/modules/NetworkModuleSamples.cs#AuthRequired)]

## Intercepting Network Traffic

Network interception allows you to block, modify, or replace network requests.

### Add Intercept

[!code-csharp[Add Intercept](../../code/modules/NetworkModuleSamples.cs#AddIntercept)]

### Intercept Specific URLs

[!code-csharp[Intercept Specific URLs](../../code/modules/NetworkModuleSamples.cs#InterceptSpecificURLs)]

### Block Requests

[!code-csharp[Block Requests](../../code/modules/NetworkModuleSamples.cs#BlockRequests)]

### Continue Requests

[!code-csharp[Continue Request](../../code/modules/NetworkModuleSamples.cs#ContinueRequest)]

### Provide Custom Response

[!code-csharp[Provide Custom Response](../../code/modules/NetworkModuleSamples.cs#ProvideCustomResponse)]

### Remove Intercept

[!code-csharp[Remove Intercept](../../code/modules/NetworkModuleSamples.cs#RemoveIntercept)]

## Capturing Response Bodies

To capture response bodies, you must set up a data collector.

### Create Data Collector

[!code-csharp[Create Data Collector](../../code/modules/NetworkModuleSamples.cs#CreateDataCollector)]

### Get Response Body

[!code-csharp[Get Response Body](../../code/modules/NetworkModuleSamples.cs#GetResponseBody)]

### Remove Data Collector

[!code-csharp[Remove Data Collector](../../code/modules/NetworkModuleSamples.cs#RemoveDataCollector)]

## Request/Response Headers

### Reading Headers

[!code-csharp[Reading Headers](../../code/modules/NetworkModuleSamples.cs#ReadingHeaders)]

### Setting Custom Headers

The intercept-based approach adds or modifies headers for individual requests as they are intercepted. Use this when you need per-request control, conditional header injection, or to modify existing headers:

[!code-csharp[Setting Custom Headers](../../code/modules/NetworkModuleSamples.cs#SettingCustomHeaders)]

### Setting Global Extra Headers

Use `SetExtraHeadersAsync` to add headers to *every* request without intercepting traffic. This is simpler and more efficient when you need the same headers (e.g., `Authorization`, `X-API-Key`) on all requests. No intercept setup is required.

[!code-csharp[Set Global Extra Headers](../../code/modules/NetworkModuleSamples.cs#SetGlobalExtraHeaders)]

### Clear Extra Headers

To remove all extra headers, use `SetExtraHeadersCommandParameters.ResetExtraHeaders`:

[!code-csharp[Clear Extra Headers](../../code/modules/NetworkModuleSamples.cs#ClearExtraHeaders)]

## Cookies

### Add Cookie

[!code-csharp[Add Cookie](../../code/modules/NetworkModuleSamples.cs#AddCookie)]

### Get Cookies

[!code-csharp[Get Cookies](../../code/modules/NetworkModuleSamples.cs#GetCookies)]

## Timing Information

[!code-csharp[Timing Information](../../code/modules/NetworkModuleSamples.cs#TimingInformation)]

## Common Patterns

### Pattern 1: Collect All Requests

[!code-csharp[Collect All Requests](../../code/modules/NetworkModuleSamples.cs#CollectAllRequests)]

### Pattern 2: Block Ad Domains

[!code-csharp[Block Ad Domains](../../code/modules/NetworkModuleSamples.cs#BlockAdDomains)]

### Pattern 3: Mock API Responses

[!code-csharp[Mock API Responses](../../code/modules/NetworkModuleSamples.cs#MockAPIResponses)]

### Pattern 4: Capture Complete HTTP Transaction

[!code-csharp[Capture Complete HTTP Transaction](../../code/modules/NetworkModuleSamples.cs#CaptureCompleteHTTPTransaction)]

## Best Practices

1. **Use data collectors wisely**: They consume memory; remove when done
2. **Run async handlers**: Network event handlers often need to call commands
3. **Filter events**: Don't process every request if you only need specific ones
4. **Clean up intercepts**: Remove intercepts when no longer needed
5. **Handle errors**: Network operations can fail; use try-catch
6. **Consider timing**: Some network events happen very quickly

## Troubleshooting

### Events Not Firing

- Ensure you've subscribed to events through Session module
- Check that navigation has actually started
- Verify URL patterns in intercepts are correct

### Missing Response Bodies

- Data collector must be added before navigation
- Ensure sufficient memory allocated
- Check that `DisownCollectedData` is set appropriately

### Intercepts Not Working

- Verify intercept was added before navigation
- Check URL patterns match the requests
- Ensure `IsBlocked` is true in event handler

## Next Steps

- [Examples: Network Interception](../examples/network-interception.md): Complete examples
- [Examples: Network Monitoring](../examples/common-scenarios.md): Practical scenarios
- [Storage Module](storage.md): Working with cookies
- [API Reference](../../api/index.md): Complete API documentation

## API Reference

See the [API documentation](../../api/index.md) for complete details on all classes and methods in the Network module.

