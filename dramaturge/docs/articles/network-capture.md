# Network Capture

`NetworkTrafficMonitor` records a page's network traffic: each request with its response and bodies, keeping each redirect hop separately. `HarGenerator` writes what it records as an HTTP Archive. The monitor can also modify matching requests and answer authentication challenges. It is built from the WebDriver BiDi network module's events, intercepts, and data collectors, which the WebDriverBiDi.NET [Network Module guide](https://webdriverbidi-net.github.io/webdriverbidi-net/articles/modules/network.html) describes.

The monitor is in the `Dramaturge` package, in the `Dramaturge.Network` namespace. To record a page's or a browser's traffic to a HAR file, and replay it, `RecordHarAsync` and `RouteFromHarAsync` are simpler; see [Recording and Replaying HAR Files](network.md#recording-and-replaying-har-files).

[!code-csharp[Network Capture](../code/NetworkCaptureSamples.cs#NetworkCapture)]

## What Is Monitored

`BrowsingContextIds` limits the monitor to pages, and `UserContextIds` to browsers: their pages, including those opened later, and their workers. One of them may be set, not both; with neither, every page is monitored. WebDriver BiDi cannot limit an intercept to a browser, so a monitor limited to browsers that modifies requests or answers challenges stops the matching requests of other browsers too, and continues them at once.

## What Is Captured

`GetCapturedTrafficAsync` returns the requests recorded since the previous call, in the order they started, once each one has completed or failed and its bodies have been retrieved. A request still in flight when the wait ends is kept for the next call. Stopping the monitor records such a request as failed.

Each hop of a redirect is a separate `NetworkRequest`, with its own `RedirectCount`. The request records:
- the headers, cookies, and sizes;
- the fetch timings, as reported when the response completed;
- the browsing context and navigation it belongs to;
- the outcome.

Request and response bodies are captured with a network data collector. A binary body is kept as base64, with `IsRequestBodyBase64Encoded` or `IsResponseBodyBase64Encoded` set. A body the browser no longer holds, or one larger than `MaxBodySize`, has the reason in `RequestBodyErrorText` or `ResponseBodyErrorText`. Browsers do not keep redirect bodies.

For a quick look, a request can be printed as HTTP text:

[!code-csharp[Request Text](../code/NetworkCaptureSamples.cs#NetworkRequestText)]

## HAR Files

`HarGenerator.Generate` writes HAR 1.2, which browser developer tools and HAR viewers can open:
- **Pages:** each navigation becomes a page, and requests are attached to the page of the latest navigation in their browsing context.
- **Timings:** the phases come from the fetch timing marks. A phase that did not happen is -1, and the entry's `time` is the sum of the phases that did.
- **Bodies:** a text body is written as text and a binary body as base64.
- **Failed requests:** these have status 0 and the error in a custom `_error` field.

By default the creator is recorded as the package that holds it, and its version. `Generate` takes another name and version for tools that wrap it.

## Modifying Requests and Answering Challenges

[!code-csharp[Request Modification](../code/NetworkCaptureSamples.cs#NetworkModification)]

A modification's pattern is a WebDriver BiDi URL pattern string, which the browser matches against each request before it is sent. A matching request is changed as described and continued. A request that cannot be continued is failed rather than left blocked.

The first credentials that match an authentication challenge's scheme and realm are offered. Credentials without a scheme or realm match any challenge. When the browser keeps rejecting the credentials, the challenge is canceled after `MaxAuthAttempts` answers. When no credentials match, the browser handles the challenge as it normally would.

## Limits

The monitor holds requests until they are retrieved. Two limits stop a long session, or one that is never read, from growing without bound:
- `MaxRetainedRequests`: requests beyond it are not recorded and are counted in `DroppedRequestCount`. A request that the monitor would modify is still modified and continued.
- `MaxBodySize`: the largest body the browser keeps for the monitor. A response's body is released in the browser once the monitor has it; a request's stays until monitoring stops, because releasing it makes Chrome forget a request that a route has stopped.

For a long session, call `GetCapturedTrafficAsync` periodically.

The options are read once, when monitoring starts. To change them, stop monitoring and start it again. Disposing the monitor stops it and removes the intercepts, data collector, and subscription it added to the browser.
