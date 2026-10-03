# Network

Full guides: https://webdriverbidi-net.github.io/dramaturge/articles/network.html and https://webdriverbidi-net.github.io/dramaturge/articles/network-capture.html

- `page.RouteAsync(url | Regex | Func<RequestData, bool>, handler, filter)` stops the page's (and its frames') requests and hands each to the handler, which calls one of:
  - `FulfillAsync(status, body, headers)`: answer without sending. **Give a body or a header**: Chrome sends a status-only response on to the network.
  - `ContinueAsync(new RouteOverrides { Url, Method, Headers, Body })`: send, changed. Firefox's page `fetch` rejects a request continued with another URL.
  - `AbortAsync()`: fail it as a network error would.
- Routes run newest first; a handler that decides nothing passes the request on, and an undecided request continues unchanged. A throwing handler is logged and its request continues.
- `RouteRegistration.RemoveAsync()`, `page.UnrouteAllAsync()`.
- `browser.RouteAsync(...)` takes the same arguments and covers every page of the browser (later ones and popups too), their frames, and its workers. A page's routes run before its browser's. `route.Browser` is the requester; `route.Page` is null for a request no page made (a worker's). It also stops matching requests of the group's other browsers (continued at once), so give it a filter. `browser.UnrouteAllAsync()`; closing the browser removes them. Chrome does not report a popup's first request, so no route sees it.
- A `UrlPattern` filter (`UrlPatternString` exact URL, or `UrlPatternPattern` with parts that match exactly) lets the browser stop only requests that might match, saving a round trip each.
- `RunAndWaitForRequestAsync(action, match)` / `RunAndWaitForResponseAsync(action, match)` return the first matching request or response the action causes.
- Cookies belong to a `Browser`: `AddCookiesAsync`, `GetCookiesAsync(domain, name)`, `ClearCookiesAsync`.
- HAR files: `await using HarRecording recording = await page.RecordHarAsync("x.har", new HarRecordingOptions { Include = request => ... })` records (also on `Browser`); disposing it writes the file, `SaveAsync()` writes sooner. `page.RouteFromHarAsync("x.har" [, url | Regex | condition], new HarRouteOptions { NotFound, Filter })` replays (also on `Browser`): the entry of the request's method and URL, one with the same body if both have one (multipart boundaries ignored), then the most matching headers, then the first; redirects replayed hop by hop; requests without an entry are aborted unless `NotFound = HarNotFound.Fallback`. Reads `.har` and Playwright's `.zip` / `_file` bodies; an unreadable file throws `InvalidDataException`. Prefer this to a hand-written route for each API call in a recorded flow.
- `NetworkTrafficMonitor` (`Dramaturge.Network`) records requests with responses and bodies for inspection in code, limited by `BrowsingContextIds` or `UserContextIds`; `HarGenerator.Generate(traffic)` writes HAR.
