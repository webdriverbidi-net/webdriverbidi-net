# Browser Setup

Dramaturge connects to browsers through the `Dramaturge.Browsers` package, which downloads, launches, and connects to Chrome, Firefox, Edge, and Safari, locally, on a remote grid, or already running. It creates the `Transport` over which a WebDriverBiDi.NET `BiDiDriver` talks to the browser, so it can be used on its own by code that works with the protocol directly. To set a browser up without it, see the WebDriverBiDi.NET [Browser Setup Guide](https://webdriverbidi-net.github.io/webdriverbidi-net/articles/browser-setup.html).

```bash
dotnet add package Dramaturge.Browsers
```

## Launching and Connecting

A `BrowserLauncher` starts the browser, and creates the `Transport` over which a `BiDiDriver` talks to it. The first launch downloads the browser into a local cache:

[!code-csharp[Launch and Connect](../code/BrowsersReadmeSamples.cs#BrowsersQuickStart)]

A browser launched through its driver executable already has a WebDriver BiDi session, so `IsBiDiSessionInitialized` tells you whether to create one. Disposing the launcher closes the browser and deletes its temporary profile.

## Choosing the Browser

`BrowserLauncher.Configure` takes the browser (Chrome, Firefox, Edge, or Safari), and the builder chooses how it is found and launched:

| Setting | Methods | Default |
| --- | --- | --- |
| Channel | `WithReleaseChannel`: `Stable`, `Beta`, `DeveloperPreview` (Chrome Dev, Firefox Developer Edition, Edge Dev, Safari Technology Preview), `Alpha` (Chrome Canary, Firefox Nightly, Edge Canary), `ExtendedSupport` (Firefox ESR) | `Stable` |
| Version | `WithVersion`: `BrowserVersion.Latest`, `BrowserVersion.Specific("…")`, or, for Chrome, `BrowserVersion.Milestone(131)`; not for Edge or Safari, which are never downloaded | The latest of the channel |
| Location | `AtAutomaticallyDownloadedLocation()`, `AtDefaultInstallationLocation()`, `AtLocation(path)` | Downloaded (Edge and Safari: installed) |
| Launch | Directly, or `LaunchUsingDriver()` through chromedriver, geckodriver, msedgedriver, or safaridriver | Directly (Safari: always through safaridriver) |
| Connection | `WithConnection(ConnectionKind.Pipes)`, for Chrome or Edge launched directly | WebSocket |
| Browser settings | `WithHeadlessOption`, `WithArguments`, `WithoutDefaultArguments`, `WithEnvironmentVariable`, `WithUserDataDirectory`, `WithLaunchTimeout` | |
| Per-browser settings | `WithBrowserOptions(new ChromeLaunchOptions { … })` or `FirefoxLaunchOptions` | |

[!code-csharp[Choosing the Browser](../code/BrowsersReadmeSamples.cs#BrowsersChoosingBrowser)]

`Build()` checks that the settings can be used together, and throws `BrowserLauncherConfigurationException` if they cannot.

A driver launched for an installed browser matches it: for an installed Chrome, or one at a location you give, the chromedriver of its version is downloaded, or, for a build that Chrome for Testing or a mirror does not list, the newest chromedriver of the same build, and then of the same major version. If the version cannot be read, the latest chromedriver of the channel is used.

Microsoft publishes Edge only as installers, so the package never downloads it: the installed Edge of the channel is used, or the executable given to `AtLocation`. Launched through its driver, Edge needs the msedgedriver of its own version, so the version of the installed Edge is read, and that msedgedriver is downloaded and checked against the MD5 hash its server reports. Microsoft publishes no Edge for Linux on Arm.

On Linux:

- A Firefox installed as a **Snap or Flatpak** (as Ubuntu's `/usr/bin/firefox` is) cannot read a profile in the temporary directory, so launching it without `WithUserDataDirectory` fails with an explanation. Use the downloaded Firefox, or a profile directory the sandbox can read.
- Running as **root**, Chrome refuses to start with its sandbox enabled, so `--no-sandbox` is added for you.

## Remote Grids and Running Browsers

`LaunchUsingRemoteGrid` creates the session on a Selenium Grid or a cloud service. The URL carries the grid's port and path prefix, credentials in it are sent as Basic authorization, `RemoteGridOptions` holds the headers of the requests, and `WithSessionCapability` adds the capabilities of the session:

[!code-csharp[Remote Grid](../code/BrowsersReadmeSamples.cs#BrowsersRemoteGrid)]

### Session Capabilities

`WithSessionCapability` adds a capability to the request for the new session. With `LaunchUsingDriver` and `LaunchUsingRemoteGrid`, the launcher sends it when it creates the session. A browser launched directly, or connected to with `ConnectToExisting`, gets it from the `session.new` command whoever starts the session sends: `BrowserLauncher.CreateCapabilityRequest()` returns the `CapabilityRequest` to send, and `BrowserGroup.LaunchAsync` in Dramaturge sends it for you. A capability's value is `null`, a string, a Boolean, a number, a dictionary with string keys, or a sequence of such values. The capabilities the core library types take their type: `proxy` a `ProxyConfiguration`, `unhandledPromptBehavior` a `UserPromptHandler`, `acceptInsecureCerts` a Boolean, and `browserName`, `browserVersion` and `platformName` a string; the first two are written as the core library writes them in `session.new`:

[!code-csharp[Session Capabilities](../code/BrowsersReadmeSamples.cs#BrowsersSessionCapabilities)]

The launcher sets `browserName` and `webSocketUrl` itself, and, through a driver, the browser's options object (`goog:chromeOptions`, `moz:firefoxOptions`, or Safari's), which it builds from `WithArguments`, `WithHeadlessOption`, `WithUserDataDirectory`, and `WithBrowserOptions`. Adding one of these fails when the launcher is built, as does a value that cannot be written.

`ConnectToExisting` attaches to a browser that is already listening, which the launcher neither starts nor stops: closing the `BrowserInstance` only detaches from it. Give it Firefox's WebDriver BiDi URL (`ws://127.0.0.1:9222/session`), or Chrome's DevTools URL, which is reached through the WebDriver BiDi mapper:

[!code-csharp[Connect to a Running Browser](../code/BrowsersReadmeSamples.cs#BrowsersConnectToExisting)]

## Locating Without Launching

`BrowserLocator.FindBrowserAsync` and `DriverLocator.FindDriverAsync` return the path of a browser or driver, downloading it if needed, for use with other tools:

[!code-csharp[Locating Without Launching](../code/BrowsersReadmeSamples.cs#BrowsersLocateOnly)]

## Downloads and the Cache

Downloads are cached per browser, channel, and version, and a cached browser is used without a network request. The version a channel resolves to is rechecked once a day, and if the service cannot be reached, the cached version is used. Failed requests are retried twice. Firefox and geckodriver downloads are verified against their published SHA-256 checksums; Chrome for Testing publishes none, so Chrome downloads are checked against the length and MD5 hash its storage service reports, which detects corruption in transit but not tampering.

`BrowserDownloadOptions` changes where browsers are cached and downloaded from:

[!code-csharp[Download Options](../code/BrowsersReadmeSamples.cs#BrowsersDownloadOptions)]

These environment variables set the defaults, so a CI system can configure every test run without code changes:

| Variable | Effect |
| --- | --- |
| `DRAMATURGE_BROWSERS_PATH` | The cache directory. The default is a `dramaturge` directory in the local application data directory (Windows), `~/Library/Caches` (macOS), or `$XDG_CACHE_HOME` or `~/.cache` (Linux). |
| `DRAMATURGE_SKIP_DOWNLOAD` | Set to `1` or `true` to use only what is already cached, making no network requests. |
| `DRAMATURGE_DOWNLOAD_MANIFEST` | The URL or file path of a mirror manifest (below). |
| `CHROME_EXECUTABLE`, `FIREFOX_EXECUTABLE`, `EDGE_EXECUTABLE`, `SAFARI_EXECUTABLE` | A browser executable to use in place of locating one. |
| `CHROMEDRIVER_EXECUTABLE`, `GECKODRIVER_EXECUTABLE`, `MSEDGEDRIVER_EXECUTABLE`, `SAFARIDRIVER_EXECUTABLE` | A driver executable to use in place of locating one. |

### Cleaning Up the Cache

The cache keeps every version it has downloaded. `BrowserCache.List` lists them as `CachedInstallation` objects, with their channel (none for a driver), size, and when a request for the latest version, or for the driver of a browser version, last resolved to them. `BrowserCache.RemoveAsync` removes one, waiting for any download into the same channel to finish:

[!code-csharp[Cache Management](../code/BrowsersReadmeSamples.cs#BrowsersCacheManagement)]

A cache is marked with the version of its layout, and a version of this package that cannot read a cache's layout throws `BrowserDownloadException` rather than misreading it, so a newer package can change the layout without an older one corrupting it.

### Installing Ahead of Time

The `dramaturge` command-line tool, from the `Dramaturge.Tool` package, installs browsers and drivers into the same cache before anything launches them, as when building a CI machine image or before a test run:

```bash
dotnet tool install --global Dramaturge.Tool
dramaturge install chrome chromedriver firefox@beta geckodriver
dramaturge list
dramaturge clear chrome@canary
```

Each target is a name (`chrome`, `chrome-headless-shell`, `firefox`, `chromedriver`, `geckodriver`, or `msedgedriver`), optionally followed by `@` and a channel (`chrome@beta`, `firefox@nightly`), a Chrome milestone (`chromedriver@131`), or a version (`firefox@134.0`). `install --dry-run` shows the version and URL each target resolves to without downloading it. The tool honors the environment variables above, and `--path` chooses another cache. With `DRAMATURGE_SKIP_DOWNLOAD` set for the test run, a test that needs a browser the tool did not install fails at once rather than downloading it. The [package README](https://www.nuget.org/packages/Dramaturge.Tool) describes every command.

The same abilities are in the library: `BrowserLocator.ResolveDownloadAsync` and `DriverLocator.ResolveDownloadAsync` resolve what `FindBrowserAsync` and `FindDriverAsync` would download, without downloading it.

## Mirroring Downloads

To download from an internal mirror rather than the vendors' services, set `BrowserDownloadOptions.ManifestUrl` (or `DRAMATURGE_DOWNLOAD_MANIFEST`) to a manifest listing the builds it holds. Every browser and driver is then resolved through the manifest; one it does not list cannot be downloaded. The manifest is JSON:

```json
{
  "schemaVersion": 1,
  "browsers": {
    "chrome": {
      "channels": { "stable": "131.0.6778.204" },
      "versions": {
        "131.0.6778.204": {
          "linux-x64": {
            "url": "chrome/131.0.6778.204/chrome-linux64.zip",
            "sha256": "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08",
            "size": 171234567
          }
        }
      }
    }
  },
  "drivers": {
    "chromedriver": {
      "versions": {
        "131.0.6778.204": {
          "linux-x64": { "url": "chrome/131.0.6778.204/chromedriver-linux64.zip", "sha256": "…" }
        }
      }
    },
    "geckodriver": {
      "latest": "0.36.0",
      "versions": {
        "0.36.0": {
          "linux-x64": { "url": "geckodriver/geckodriver-v0.36.0-linux64.tar.gz", "sha256": "…" }
        }
      }
    }
  }
}
```

- **Browsers** are `chrome`, `chrome-headless-shell`, and `firefox`; **drivers** are `chromedriver`, `geckodriver`, and `msedgedriver`.
- **Channels** are `stable`, `beta`, `dev`, `canary`, `nightly`, and `esr`. A milestone resolves to the highest version listed for it; chromedriver's version follows the browser's (for an installed Chrome whose own is not listed, the closest listed: the newest of the same build, then of the same major version), msedgedriver's is exactly the installed Edge's, and geckodriver's is its `latest`.
- **Platforms** are `linux-x64`, `linux-arm64`, `linux-x86`, `macos-x64`, `macos-arm64`, `windows-x64`, `windows-x86`, and `windows-arm64`.
- **Builds** are each vendor's archive, unchanged. Each lists its SHA-256 hash, which is verified, and optionally its size. A URL may be relative to the manifest, so a directory holding the manifest and the archives serves as a mirror with no server (`file:///…/manifest.json`).
- **Nightly builds** share version numbers, so give each mirrored Nightly build a distinct version string.

## Errors

Every exception the package throws derives from `WebDriverBiDiException`: `BrowserDownloadException` when a browser or driver cannot be located or downloaded, `BrowserLaunchException` (with the exit code and the end of the process output) when one cannot be started, and `BrowserLauncherConfigurationException` when `Build()` finds settings that cannot be used together.
