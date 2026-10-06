# Command-Line Tool

The `dramaturge` command-line tool, from the `Dramaturge.Tool` package, installs, lists, and removes the browsers and drivers that [Dramaturge.Browsers](browser-setup.md) keeps in its cache, and records what you do in a browser as C#. Installing is most useful in continuous integration: install the browsers while building a machine image or before the tests run, so that no test waits for a download.

It uses the same cache, download sources, and verification as the library, because it is the library: each release of the tool bundles the Dramaturge.Browsers of the same version.

## Installation

```bash
dotnet tool install --global Dramaturge.Tool
```

Or, with .NET 10 or later, run it once without installing it:

```bash
dnx Dramaturge.Tool install chrome
```

The tool runs on .NET 8 and later.

## Installing Browsers and Drivers

```bash
dramaturge install chrome chromedriver firefox@beta geckodriver
```

Each target is a name, optionally followed by `@` and a channel, a milestone, or a version:

| Name | Channels | Also accepts |
| --- | --- | --- |
| `chrome`, `chrome-headless-shell` | `stable` (the default), `beta`, `dev`, `canary` | A milestone (`chrome@131`, the newest release of Chrome 131) or a version (`chrome@131.0.6778.204`) |
| `chromedriver` | `stable`, `beta`, `dev`, `canary` | A milestone or version, of the Chrome it drives |
| `firefox` | `stable`, `beta`, `dev` (Developer Edition), `nightly`, `esr` | A version (`firefox@134.0`) |
| `geckodriver` | None | Always its latest release |
| `msedgedriver` | `stable`, `beta`, `dev`, `canary` | Always the version of the installed Edge of the channel |

Microsoft Edge and Safari are never downloaded: install Edge from Microsoft, and Safari comes with macOS. `msedgedriver` downloads the driver that matches the installed Edge.

The tool prints where each target is installed, and reports download progress on standard error. A target that is already cached is not downloaded again. Every target is checked before anything is downloaded, and a target that fails does not stop the others; the exit code is 1 if any failed.

`--dry-run` shows the version and URL each target resolves to, and whether it is already cached, without downloading anything:

```bash
dramaturge install --dry-run chrome@beta geckodriver
```

## Listing and Removing

```bash
dramaturge list
dramaturge clear chrome@canary chromedriver@131
dramaturge clear
```

`list` shows each cached browser and driver, as `name@version`, with its channel, size, the date a channel or driver lookup last used it, and its directory. `clear` removes what its targets select (a channel, a milestone, a version, or everything of a name) or, with no targets, everything; `--dry-run` shows what it would remove. Drivers are shared by every channel, so a driver is selected by version, not channel.

## Recording Code

```bash
dramaturge codegen https://example.com/ -o Recorded.cs
```

`codegen` opens a browser with a window and writes what you do in it as C#: a program, or, with `--target`, an xUnit, NUnit, MSTest, or TUnit test class. A toolbar in the page picks locators and adds assertions. [Code Generation](codegen.md) describes it.

## The Cache

Every command takes `--path` to choose the cache directory. Otherwise, the tool uses the same directory as the library: `DRAMATURGE_BROWSERS_PATH` if it is set, or else the per-user cache directory. The library's other environment variables apply too, such as `DRAMATURGE_DOWNLOAD_MANIFEST` to download from a mirror. [Browser Setup](browser-setup.md#downloads-and-the-cache) describes them.

A test run can then use what the tool installed:

```bash
export DRAMATURGE_BROWSERS_PATH="$PWD/.browsers"
dramaturge install chrome chromedriver
export DRAMATURGE_SKIP_DOWNLOAD=1
dotnet test
```

With `DRAMATURGE_SKIP_DOWNLOAD` set, a test that needs a browser the tool did not install fails at once rather than downloading it.
