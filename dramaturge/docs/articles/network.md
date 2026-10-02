# Network

## Routes

A route stops a page's requests before they are sent, and hands each to a handler that answers it, changes it, or aborts it. Routes cover the page and the frames within it.

[!code-csharp[Fulfill](../code/NetworkSamples.cs#Fulfill)]

`RouteAsync` takes a string, for an exact URL, a `Regex`, or a condition on the request. Its handler gets a `Route` to decide the request with:

- `FulfillAsync` answers it with a status, a body (text or bytes), and headers, without sending it.
- `ContinueAsync` sends it, optionally with a different URL, method, headers, or body.
- `AbortAsync` fails it, as a network error would.

[!code-csharp[Continue and Abort](../code/NetworkSamples.cs#ContinueAndAbort)]

Routes are tried newest first. A handler that decides nothing passes the request to the next route that matches it, and a request no route decides continues unchanged. A handler that throws is reported on the group's `OnLogMessage`, and its request continues. `RemoveAsync` removes a route, and `UnrouteAllAsync` removes all of a page's.

> **Firefox:** a request continued with another URL receives its response, but the page's `fetch` rejects it, in Firefox.

### Stopping Fewer Requests

While a route exists, every request of its page is stopped and handed over, and each one the routes do not decide is continued, which costs a round trip. A `UrlPattern` filter lets the browser stop only the requests that might match:

[!code-csharp[Filter](../code/NetworkSamples.cs#Filter)]

A filter is a WebDriver BiDi URL pattern: `UrlPatternString`, a URL that must match exactly, or `UrlPatternPattern`, whose parts each match exactly and match anything when left out. The route's own URL or condition still decides which stopped requests it handles.

## Waiting for Requests and Responses

[!code-csharp[Wait for Requests](../code/NetworkSamples.cs#WaitForRequests)]

`RunAndWaitForRequestAsync` and `RunAndWaitForResponseAsync` start listening, run the action, and return the first request sent, or response received, that matches: an exact URL, a `Regex`, or a condition. To record all of a page's traffic, see [Network Capture](network-capture.md).

## Cookies

Cookies belong to a browser, and so are shared by its pages:

[!code-csharp[Cookies](../code/NetworkSamples.cs#Cookies)]

`GetCookiesAsync` and `ClearCookiesAsync` take an optional domain and name to narrow them.
