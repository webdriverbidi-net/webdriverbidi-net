# Dramaturge.Browsers

Locates, downloads, and launches browsers for automation with the [WebDriverBiDi](https://www.nuget.org/packages/WebDriverBiDi) .NET client library.

- Downloads Chrome (from Chrome for Testing) and Firefox, and their drivers, into a shared local cache. Firefox and geckodriver downloads are verified against their published SHA-256 checksums; Chrome for Testing publishes none, so Chrome downloads are checked for corruption in transit.
- Launches Chrome, Firefox, or the installed Microsoft Edge directly, or through chromedriver, geckodriver, msedgedriver, or safaridriver. msedgedriver is downloaded in the version of the installed Edge; Microsoft publishes Edge itself only as installers, so it is never downloaded.
- Connects to a browser on a remote WebDriver grid, or to one that is already running.
- Works on Windows, macOS, and Linux (x64 and Arm64), and with native AOT.

## Installation

```bash
dotnet add package Dramaturge.Browsers
```

## Quick Start

<!-- readme-csharp: docs/code/BrowsersReadmeSamples.cs#BrowsersQuickStart -->
```csharp
using WebDriverBiDi;
using Dramaturge.Browsers;
using WebDriverBiDi.Session;

// Downloads Chrome for Testing into a local cache on first use.
await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome)
    .WithHeadlessOption()
    .Build();
await using BrowserInstance browser = await launcher.LaunchAsync();

await using BiDiDriver driver = new(TimeSpan.FromSeconds(30), launcher.CreateTransport());
await driver.StartAsync(browser.ConnectionString);

// A browser launched through a driver executable already has a session.
if (!launcher.IsBiDiSessionInitialized)
{
    await driver.Session.NewSessionAsync(new NewCommandParameters());
}
```

Disposing the launcher closes the browser and deletes its temporary profile.

## Choosing the Browser

`BrowserLauncher.Configure` takes the browser (Chrome, Firefox, Edge, or Safari), and the builder chooses how it is found and launched:

- **Channel:** `WithReleaseChannel` picks Stable (the default), Beta, DeveloperPreview (Chrome Dev, Firefox Developer Edition, Edge Dev, Safari Technology Preview), Alpha (Chrome Canary, Firefox Nightly, Edge Canary), or ExtendedSupport (Firefox ESR).
- **Version:** `WithVersion` takes `BrowserVersion.Latest` (the default), `BrowserVersion.Specific("131.0.6778.204")`, or, for Chrome, `BrowserVersion.Milestone(131)`, the newest release of a major version.
- **Location:** the browser is downloaded by default; `AtDefaultInstallationLocation()` uses the one installed on the machine, and `AtLocation(path)` a particular executable. Edge and Safari are never downloaded, so the installed one of the channel is used unless `AtLocation` names another.
- **Launch:** the browser is launched directly by default; `LaunchUsingDriver()` launches it through its driver executable, which is downloaded too, in the version that matches the browser, including an installed Chrome or Edge. Safari is always launched through safaridriver, from its installed location.
- **Settings:** `WithHeadlessOption`, `WithArguments`, `WithoutDefaultArguments`, `WithEnvironmentVariable`, `WithUserDataDirectory`, and `WithLaunchTimeout` apply to any browser; `WithBrowserOptions` takes settings for one browser, such as `ChromeLaunchOptions.UseHeadlessShell` or `FirefoxLaunchOptions.Preferences`.

<!-- readme-csharp: docs/code/BrowsersReadmeSamples.cs#BrowsersChoosingBrowser -->
```csharp
using Dramaturge.Browsers;

// The newest release of Chrome 131, as the smaller chrome-headless-shell build.
await using BrowserLauncher chrome = BrowserLauncher.Configure(BrowserKind.Chrome)
    .WithVersion(BrowserVersion.Milestone(131))
    .WithBrowserOptions(new ChromeLaunchOptions() { UseHeadlessShell = true })
    .Build();

// Firefox ESR, with an extra argument and a preference.
FirefoxLaunchOptions firefoxOptions = new();
firefoxOptions.Preferences["browser.startup.page"] = 0;
await using BrowserLauncher firefox = BrowserLauncher.Configure(BrowserKind.Firefox)
    .WithReleaseChannel(BrowserReleaseChannel.ExtendedSupport)
    .WithArguments("--width=1280", "--height=800")
    .WithBrowserOptions(firefoxOptions)
    .Build();

// The Chrome installed on this machine, launched through a downloaded chromedriver.
await using BrowserLauncher installedChrome = BrowserLauncher.Configure(BrowserKind.Chrome)
    .AtDefaultInstallationLocation()
    .LaunchUsingDriver()
    .Build();
```

## Remote Grids and Running Browsers

`LaunchUsingRemoteGrid` creates the session on a Selenium Grid or a cloud service. The URL carries the grid's port and path, `RemoteGridOptions` the headers of the requests, and `WithSessionCapability` the capabilities of the session:

<!-- readme-csharp: docs/code/BrowsersReadmeSamples.cs#BrowsersRemoteGrid -->
```csharp
using Dramaturge.Browsers;

RemoteGridOptions gridOptions = new();
gridOptions.Headers["X-Build-Id"] = "nightly-1234";

// Credentials in the URL are sent as Basic authorization.
await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome)
    .LaunchUsingRemoteGrid(new Uri("https://user:access-key@grid.example.com/wd/hub"), gridOptions)
    .WithSessionCapability("browserVersion", "131")
    .WithSessionCapability("goog:chromeOptions", new Dictionary<string, object?>() { ["args"] = new[] { "--headless=new" } })
    .Build();
```

`WithSessionCapability` also adds capabilities when the browser is launched through its driver. A proxy is given as a `ProxyConfiguration`, and a user prompt handler as a `UserPromptHandler`. For a browser launched directly or connected to, the capabilities go in the `session.new` command that starts the session, from `BrowserLauncher.CreateCapabilityRequest()`:

<!-- readme-csharp: docs/code/BrowsersReadmeSamples.cs#BrowsersSessionCapabilities -->
```csharp
using Dramaturge.Browsers;
using WebDriverBiDi.Session;

ManualProxyConfiguration proxy = new() { HttpProxy = "proxy.example.com:3128", SslProxy = "proxy.example.com:3128" };
proxy.NoProxyAddresses.Add("localhost");

await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Firefox)
    .LaunchUsingDriver()
    .WithSessionCapability("proxy", proxy)
    .WithSessionCapability("acceptInsecureCerts", true)
    .Build();
```

The capabilities the launcher sets itself (`browserName`, `webSocketUrl`, and, through a driver, the browser's options object such as `goog:chromeOptions`) cannot be added; they come from the builder's other settings. A browser launched directly has no session until you send `session.new`, whose `CapabilityRequest` takes the capabilities instead.

`ConnectToExisting` attaches to a browser that is already listening, which the launcher neither starts nor stops: closing the `BrowserInstance` only detaches from it. Give it Firefox's WebDriver BiDi URL (`ws://127.0.0.1:9222/session`), or Chrome's DevTools URL:

<!-- readme-csharp: docs/code/BrowsersReadmeSamples.cs#BrowsersConnectToExisting -->
```csharp
using Dramaturge.Browsers;

// Chrome started with --remote-debugging-port=9222 reports this DevTools URL at
// http://127.0.0.1:9222/json/version; it is reached through the WebDriver BiDi mapper.
await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome)
    .ConnectToExisting(new Uri("ws://127.0.0.1:9222/devtools/browser/0b8e2c1a-6d9e-4f35-a1c2-3b4d5e6f7a8b"))
    .Build();
```

## Locating Without Launching

`BrowserLocator.FindBrowserAsync` and `DriverLocator.FindDriverAsync` return the path of a browser or driver, downloading it if needed, for use with other tools:

<!-- readme-csharp: docs/code/BrowsersReadmeSamples.cs#BrowsersLocateOnly -->
```csharp
using Dramaturge.Browsers;

string firefoxPath = await BrowserLocator.FindBrowserAsync(BrowserKind.Firefox, BrowserReleaseChannel.Beta);
string? geckodriverPath = await DriverLocator.FindDriverAsync(BrowserKind.Firefox);
```

## Downloads and the Cache

Downloads are cached per browser, channel, and version, and a cached browser is used without a network request. The version a channel resolves to is rechecked once a day, and if the service cannot be reached, the cached version is used. `BrowserDownloadOptions` changes where browsers are cached and downloaded from:

<!-- readme-csharp: docs/code/BrowsersReadmeSamples.cs#BrowsersDownloadOptions -->
```csharp
using Dramaturge.Browsers;

BrowserDownloadOptions downloadOptions = new()
{
    CacheDirectory = "/ci/cache/browsers",
    ManifestUrl = new Uri("https://mirror.example.com/browsers/manifest.json"),
    Progress = new Progress<BrowserDownloadProgress>(report => Console.WriteLine($"{report.Name}: {report.BytesReceived} bytes")),
};

await using BrowserLauncher launcher = BrowserLauncher.Configure(BrowserKind.Chrome)
    .WithDownloadOptions(downloadOptions)
    .Build();
```

These environment variables set the defaults, so a CI system can configure every test run without code changes:

| Variable | Effect |
| --- | --- |
| `DRAMATURGE_BROWSERS_PATH` | The cache directory. The default is a `dramaturge` directory in the local application data directory (Windows), `~/Library/Caches` (macOS), or `$XDG_CACHE_HOME` or `~/.cache` (Linux). |
| `DRAMATURGE_SKIP_DOWNLOAD` | Set to `1` or `true` to use only what is already cached, making no network requests. |
| `DRAMATURGE_DOWNLOAD_MANIFEST` | The URL or file path of a mirror manifest (below). |
| `CHROME_EXECUTABLE`, `FIREFOX_EXECUTABLE`, `EDGE_EXECUTABLE`, `SAFARI_EXECUTABLE` | A browser executable to use in place of locating one. |
| `CHROMEDRIVER_EXECUTABLE`, `GECKODRIVER_EXECUTABLE`, `MSEDGEDRIVER_EXECUTABLE`, `SAFARIDRIVER_EXECUTABLE` | A driver executable to use in place of locating one. |

### Cleaning Up the Cache

The cache keeps every version it has downloaded. `BrowserCache.List` lists them, with their channel, size, and when a request last resolved to them, and `BrowserCache.RemoveAsync` removes one, waiting for any download into the same channel to finish:

<!-- readme-csharp: docs/code/BrowsersReadmeSamples.cs#BrowsersCacheManagement -->
```csharp
using Dramaturge.Browsers;

foreach (CachedInstallation installation in BrowserCache.List())
{
    Console.WriteLine($"{installation} ({installation.Channel ?? "driver"}): {installation.Size / (1024 * 1024)} MB");

    // Keeps only what a channel or driver request has resolved to in the last week.
    if (installation.LastResolved is null || installation.LastResolved < DateTimeOffset.UtcNow.AddDays(-7))
    {
        await BrowserCache.RemoveAsync(installation);
    }
}
```

A cache is marked with the version of its layout, and a version of this package that cannot read a cache's layout refuses to use it, rather than misreading it.

To install browsers and drivers into the cache ahead of time, as in a CI machine image, use the `dramaturge` command-line tool from the [Dramaturge.Tool](https://www.nuget.org/packages/Dramaturge.Tool) package:

```bash
dotnet tool install --global Dramaturge.Tool
dramaturge install chrome chromedriver firefox geckodriver
```

### Mirroring Downloads

To download from an internal mirror rather than the vendors' services, set `ManifestUrl` (or `DRAMATURGE_DOWNLOAD_MANIFEST`) to a manifest listing the builds it holds. Every browser and driver is then resolved through the manifest; one it does not list cannot be downloaded. The manifest is JSON:

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

Every exception the package throws derives from `WebDriverBiDiException`: `BrowserDownloadException` when a browser or driver cannot be located or downloaded, `BrowserLaunchException` (with the exit code and the end of the output) when one cannot be started, and `BrowserLauncherConfigurationException` when `Build()` finds settings that cannot be used together.

## Documentation

See the Browser Setup article (`docs/articles/browser-setup.md`) in the Dramaturge documentation.
