# Network

Full guides: https://webdriverbidi-net.github.io/dramaturge/articles/network.html and https://webdriverbidi-net.github.io/dramaturge/articles/network-capture.html

- `page.RouteAsync(url | Regex | Func<RequestData, bool>, handler, filter)` stops the page's (and its frames') requests and hands each to the handler, which calls one of:
  - `FulfillAsync(status, body, headers)`: answer without sending. **Give a body or a header**: Chrome sends a status-only response on to the network.
  - `ContinueAsync(new RouteOverrides { Url, Method, Headers, Body })`: send, changed. Firefox's page `fetch` rejects a request continued with another URL.
  - `AbortAsync()`: fail it as a network error would.
- Routes run newest first; a handler that decides nothing passes the request on, and an undecided request continues unchanged. A throwing handler is logged and its request continues.
- `RouteRegistration.RemoveAsync()`, `page.UnrouteAllAsync()`.
- A `UrlPattern` filter (`UrlPatternString` exact URL, or `UrlPatternPattern` with parts that match exactly) lets the browser stop only requests that might match, saving a round trip each.
- `RunAndWaitForRequestAsync(action, match)` / `RunAndWaitForResponseAsync(action, match)` return the first matching request or response the action causes.
- Cookies belong to a `Browser`: `AddCookiesAsync`, `GetCookiesAsync(domain, name)`, `ClearCookiesAsync`.
- `NetworkTrafficMonitor` (`Dramaturge.Network`) records requests with responses and bodies; `HarGenerator.Generate(traffic)` writes HAR.
