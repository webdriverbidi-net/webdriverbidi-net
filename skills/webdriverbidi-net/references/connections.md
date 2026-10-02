# Connections and Transports

## Getting a BiDi Endpoint

`BiDiDriver.StartAsync` connects to a WebSocket URL that speaks WebDriver BiDi.

- **Firefox** speaks BiDi natively. Start it with `--remote-debugging-port=9222` and connect to `ws://localhost:9222/session`, then call `Session.NewSessionAsync`.
- **Chrome and Edge** do not speak BiDi on `--remote-debugging-port`: that port serves the Chrome DevTools Protocol (CDP), and a driver connected to its `/devtools/` URL opens the socket and then fails on its first command. Use chromedriver or msedgedriver: create a classic session with the capability `"webSocketUrl": true` (`POST http://localhost:9515/session`), and connect to the `webSocketUrl` in the response. That session already exists, so do not call `NewSessionAsync`; end it with `Session.EndAsync()` or the driver's `DELETE /session/<id>`.
- **geckodriver** offers both: a classic session with `webSocketUrl: true` (no `NewSessionAsync`), or its BiDi-only `/session` endpoint (with `NewSessionAsync`).
- **Selenium Grid and Selenium** can create the session and report `webSocketUrl` the same way.

A connection that is refused is retried every 500 milliseconds until the connection's startup timeout, 10 seconds by default, and then `StartAsync` throws `WebDriverBiDiTimeoutException`.

## Transports

`BiDiDriver` sends commands through a `Transport`, which uses a `Connection`: `WebSocketConnection` by default, or `PipeConnection` for a Chromium browser launched with `--remote-debugging-pipe`. The pipe carries CDP, so it needs a `Transport` that installs a BiDi-over-CDP mapper, as `ChromiumTransport` in Dramaturge.Browsers does. Pass a custom transport to the constructor: `new BiDiDriver(timeout, transport)`.

`driver.TransportConfiguration` holds the error behaviors (`EventHandlerExceptionBehavior`, `ProtocolErrorBehavior`, `UnknownMessageBehavior`, `UnexpectedErrorBehavior`) and the log level of the driver's own diagnostics (`LogLevel`).

## Lifecycle

1. Create the `BiDiDriver`.
2. Register custom modules, events, and JSON type resolvers (only before `StartAsync`).
3. Add observers.
4. `StartAsync(url)`.
5. `NewSessionAsync` if the endpoint has no session.
6. `Session.SubscribeAsync` for the events observed.
7. Commands.
8. `StopAsync()`, then dispose (`await using` does it).

A stopped driver can be started again. More: https://webdriverbidi-net.github.io/webdriverbidi-net/articles/browser-setup.html and https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/connection-management.html
