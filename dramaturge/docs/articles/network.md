# Network

## Routes

A route stops requests before they are sent, and hands each to a handler that answers it, changes it, or aborts it. A page's routes cover the page and the frames within it; a [browser's](#routes-for-a-whole-browser) cover every page of the browser.

[!code-csharp[Fulfill](../code/NetworkSamples.cs#Fulfill)]

`RouteAsync` takes a string, for an exact URL, a `Regex`, or a condition on the request. Its handler gets a `Route` to decide the request with:

- `FulfillAsync` answers it with a status, a body (text or bytes), and headers, without sending it.
- `ContinueAsync` sends it, optionally with a different URL, method, headers, or body.
- `AbortAsync` fails it, as a network error would.

[!code-csharp[Continue and Abort](../code/NetworkSamples.cs#ContinueAndAbort)]

Routes are tried newest first. A handler that decides nothing passes the request to the next route that matches it, and a request no route decides continues unchanged. A handler that throws is reported on the group's `OnLogMessage`, and its request continues. `RemoveAsync` removes a route, and `UnrouteAllAsync` removes all of a page's.

> **Firefox:** a request continued with another URL receives its response, but the page's `fetch` rejects it, in Firefox.

> **Chrome:** a response given only a status code, with neither headers nor a body, is not provided: Chrome's WebDriver BiDi implementation continues the request to the network instead. Give a body, even an empty string, or a header.

### Stopping Fewer Requests

While a route exists, every request of its page is stopped and handed over, and each one the routes do not decide is continued, which costs a round trip. A `UrlPattern` filter lets the browser stop only the requests that might match:

[!code-csharp[Filter](../code/NetworkSamples.cs#Filter)]

A filter is a WebDriver BiDi URL pattern: `UrlPatternString`, a URL that must match exactly, or `UrlPatternPattern`, whose parts each match exactly and match anything when left out. The route's own URL or condition still decides which stopped requests it handles.

### Routes for a Whole Browser

`Browser.RouteAsync` takes the same URL, `Regex`, or condition, handler, and filter, and covers every page of the browser, including those opened later and popups from their first request, their frames, and the browser's workers:

[!code-csharp[Browser Routes](../code/NetworkSamples.cs#BrowserRoutes)]

A request is offered to its page's routes first, newest first, then to its browser's, newest first. `Route.Browser` is the browser that made the request; `Route.Page` is its page, or `null` for a request no page made, such as a worker's. `Browser.UnrouteAllAsync` removes the browser's routes, not its pages', and closing the browser removes them too.

WebDriver BiDi cannot limit stopping requests to one browser, so while a browser route exists, the matching requests of the group's other browsers are stopped too, and continued at once, at the cost of a round trip each. Give a browser route a filter.

> **Chrome:** Chrome neither stops nor reports a popup's first request, so a browser route does not see it; the popup's later requests are routed.

## Recording and Replaying HAR Files

An HTTP Archive (HAR) file holds requests and their responses. Recording a page's traffic to one, and answering the page's requests from it later, lets a test run against what a server once said, without the server:

[!code-csharp[Record a HAR](../code/NetworkSamples.cs#RecordHar)]

`RecordHarAsync`, on a page or a browser, records each request with its response and bodies. Disposing the recording waits for requests in flight, for at most the [navigation timeout](configuration.md#group-options), and writes the file; a request still in flight then is written as failed. `SaveAsync` writes the file sooner, with what has completed. `Include` chooses the requests written, and `CaptureBodies = false` leaves the bodies out. A browser's recording has the requests of all its pages and workers, and not those of other browsers.

[!code-csharp[Replay a HAR](../code/NetworkSamples.cs#ReplayHar)]

`RouteFromHarAsync`, on a page or a browser, adds a route that answers requests from a file. Like `RouteAsync`, it takes a URL, a `Regex`, or a condition, or covers every request, and `HarRouteOptions.Filter` is the URL pattern filter. The file is read when the route is added, and a file that cannot be read throws `InvalidDataException`.

- **Matching:** a request is answered by an entry of its method and URL, a fragment aside. If both the request and entries record a body, an entry with the same body is chosen, a multipart form's being compared without its boundary, which each browser makes anew; otherwise one that records no body, but never one with another body. Of several, the one recorded with the most of the request's headers wins, then the first in the file. An entry answers every request it matches, as often as asked.
- **Requests without an entry** are aborted, so that a replayed page never reaches the network unseen, unless `NotFound` is `HarNotFound.Fallback`, which passes them to the next route, or the network.
- **Redirects** are answered as recorded, with their `Location`, and the browser follows them to the next entry, so the page's URL, and a `fetch`'s, are those of the recording.
- **Responses** have the recorded status, reason, and headers, a repeated header such as `Set-Cookie` included. The body is given decoded, so the recorded `Content-Encoding` and `Content-Length` are replaced by the body's own length.
- **Formats:** a `.har` file, with bodies in it or in files beside it, or a `.zip` holding a `.har` and its body files, as Playwright writes them, can be replayed. Recordings are written as `.har` files with the bodies in them.

When the file records request bodies, the route reads each matching request's body through a WebDriver BiDi data collector, which keeps the bodies in the browser until the route is removed.

## Waiting for Requests and Responses

[!code-csharp[Wait for Requests](../code/NetworkSamples.cs#WaitForRequests)]

`RunAndWaitForRequestAsync` and `RunAndWaitForResponseAsync` start listening, run the action, and return the first request sent, or response received, that matches: an exact URL, a `Regex`, or a condition. To record all of a page's traffic, [record a HAR file](#recording-and-replaying-har-files), or, to inspect it in code, see [Network Capture](network-capture.md).

## Cookies

Cookies belong to a browser, and so are shared by its pages:

[!code-csharp[Cookies](../code/NetworkSamples.cs#Cookies)]

`GetCookiesAsync` and `ClearCookiesAsync` take an optional domain and name to narrow them.
