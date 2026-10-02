# Configuration

## Group Options

`DramaturgeOptions` applies to a `BrowserGroup` and everything it creates. It is read when the group is launched or connected:

[!code-csharp[Group Options](../code/ConfigurationSamples.cs#GroupOptions)]

| Option | Default | Effect |
| --- | --- | --- |
| `ActionTimeout` | 30 seconds | How long an action or read waits for its element |
| `NavigationTimeout` | 30 seconds | How long a navigation, or a wait for a load state or URL, waits |
| `ExpectTimeout` | 5 seconds | How long an expectation retries |
| `PollInterval` | 100 milliseconds | The interval between checks while waiting |
| `TestIdAttribute` | `data-testid` | The attribute `GetByTestId` reads |
| `PierceShadowRoots` | `false` | Whether lookups also search open shadow roots, as if their elements were part of the document |
| `SandboxName` | `dramaturge` | The name of the sandbox in which Dramaturge's own scripts run, isolated from the page's scripts |
| `TimeProvider` | The system clock | The clock that times waits; a test can give a fake one |

`PierceShadowRoots` costs a round trip per lookup to find the open shadow roots, and does not extend XPath lookups, which browsers do not evaluate within a shadow root. A closed shadow root is reached explicitly, with `ShadowRoot()` on the locator of its host.

## Browser Options

`BrowserOptions` sets up a browser, a user context, when `CreateBrowserAsync` creates it. Its settings hold for every page the browser opens, and a setting left unset keeps the browser's default:

[!code-csharp[Browser Options](../code/ConfigurationSamples.cs#BrowserOptions)]

| Option | Effect |
| --- | --- |
| `Viewport`, `DevicePixelRatio` | The size of each page's viewport, and its device pixel ratio |
| `Locale`, `TimeZone` | The locale and time zone pages see |
| `UserAgent` | The user agent string |
| `MediaFeatures` | CSS media features, such as `prefers-color-scheme` |
| `Geolocation` | The position the Geolocation API reports |
| `Permissions` | Permissions granted or denied to an origin |
| `AcceptInsecureCerts` | Whether untrusted and self-signed TLS certificates are accepted |
| `Proxy` | The proxy for the browser's requests |
| `UnhandledPromptBehavior` | What the browser does with a dialog nobody answers |

The default browser always exists and is not created, so it takes no options. If the browser rejects a setting, `CreateBrowserAsync` throws, and removes the user context it created.

Settings for the browser process itself, such as its channel, its version, headless mode, and its arguments, are given to the launcher; see [Browser Setup](browser-setup.md).

## Timeouts for One Call

Every method that waits takes a timeout of its own, which overrides the group's for that call:

[!code-csharp[Per-Call Timeouts](../code/ConfigurationSamples.cs#PerCallTimeouts)]

Every method that waits also takes a `CancellationToken`, which stops the wait and the commands it is sending.
