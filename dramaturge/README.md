# Dramaturge

Browser automation for .NET, built on the W3C [WebDriver BiDi](https://w3c.github.io/webdriver-bidi/) protocol through
[WebDriverBiDi.NET](https://github.com/webdriverbidi-net/webdriverbidi-net). One API drives Chrome, Firefox, and Edge,
with locators, actions that wait for their element to be ready, and assertions that retry until they hold.

> **Pre-release:** Dramaturge is at version 0.0.x, and its API may change in any release.

<!-- readme-csharp: docs/code/DramaturgeReadmeSamples.cs#QuickStart -->
```csharp
using System.Text.RegularExpressions;
using Dramaturge;
using Dramaturge.Browsers;
using static Dramaturge.Assertions;

// Downloads Chrome for Testing on first use. Disposing the group closes the browser.
await using BrowserGroup group = await BrowserGroup.LaunchAsync(BrowserLauncher.Configure(BrowserKind.Chrome).WithHeadlessOption());
Page page = await group.DefaultBrowser.NewPageAsync();
await page.NavigateAsync("https://example.com/login");

// Each action waits until its element is ready: visible, stable, enabled, and not covered.
await page.GetByLabel("User name").FillAsync("ada");
await page.GetByLabel("Password").FillAsync("correct horse battery staple");
await page.GetByRole("button", "Sign in").ClickAsync();

// Each expectation is checked again until it holds, or fails after five seconds saying what it last saw.
await Expect(page).ToHaveUrlAsync(new Regex("/dashboard$"));
await Expect(page.GetByRole("heading", "Welcome, Ada")).ToBeVisibleAsync();
```

## Coding Agents

The repository publishes an agent skill, `dramaturge`, that teaches coding agents such as Claude Code to write reliable
tests with Dramaturge, and the documentation site publishes `llms.txt` and `llms-full.txt`. To install the skill in
Claude Code:

    claude plugin marketplace add webdriverbidi-net/dramaturge
    claude plugin install dramaturge@dramaturge

See [Using with Coding Agents](https://webdriverbidi-net.github.io/dramaturge/articles/coding-agents.html).

## Packages

| Package | Contents |
| --- | --- |
| [Dramaturge](https://www.nuget.org/packages/Dramaturge) | The automation API |
| [Dramaturge.Browsers](https://www.nuget.org/packages/Dramaturge.Browsers) | Locates, downloads, and launches browsers, locally, on a remote grid, or already running |
| [Dramaturge.Tool](https://www.nuget.org/packages/Dramaturge.Tool) | The `dramaturge` command-line tool, which installs and manages the browsers Dramaturge.Browsers downloads |
| [Dramaturge.Xunit](https://www.nuget.org/packages/Dramaturge.Xunit), [Dramaturge.NUnit](https://www.nuget.org/packages/Dramaturge.NUnit), [Dramaturge.MSTest](https://www.nuget.org/packages/Dramaturge.MSTest), [Dramaturge.TUnit](https://www.nuget.org/packages/Dramaturge.TUnit) | Base classes for tests in each framework: a browser launched once per run, an isolated browser and page for each test, and screenshots of failed tests |

## Documentation

The [documentation](https://webdriverbidi-net.github.io/dramaturge/) has guides and the API reference.

## Building

Building requires the .NET 10 SDK:

    dotnet build Dramaturge.sln
    dotnet test --project test/Dramaturge.Tests

The integration tests launch real browsers: Chrome and Firefox from the paths in `CHROME_EXECUTABLE` and
`FIREFOX_EXECUTABLE`, or, when those are not set, outside CI, the downloaded Canary and Nightly builds. The examples
run against the browser `DRAMATURGE_BROWSER` names, Chrome by default, at `CHROME_EXECUTABLE` or `FIREFOX_EXECUTABLE`
if set, or downloaded.

    dotnet test --project test/Dramaturge.IntegrationTests
    dotnet test --project samples/Dramaturge.Examples

To check Dramaturge under native AOT, publish the test application and run it against each browser; it prints a
`PASS` line and exits with 0. Building it, as the solution build does, already reports any trimming or AOT warning.

    dotnet publish test/Dramaturge.AotTestApplication --configuration Release --output artifacts/aot
    artifacts/aot/Dramaturge.AotTestApplication firefox
    artifacts/aot/Dramaturge.AotTestApplication chrome

Until Dramaturge moves to a repository of its own, it builds against the `WebDriverBiDi` and
`WebDriverBiDi.Extensions` projects of the repository around it, rather than their packages.

| Directory | Contents |
| --- | --- |
| `src` | The `Dramaturge`, `Dramaturge.Browsers`, and `Dramaturge.Tool` projects; the test framework packages, `Dramaturge.Xunit`, `Dramaturge.NUnit`, `Dramaturge.MSTest`, and `Dramaturge.TUnit`, with the sources they share in `Dramaturge.Testing.Shared` |
| `test` | Unit tests, which need no browser, including those of each test framework package; integration tests; and compatibility and .NET Framework tests of the launcher |
| `samples` | `Dramaturge.Examples`, an xUnit project of example tests built on `Dramaturge.Xunit`, which CI runs with the integration tests |
| `docs` | The documentation site; `docs/README.md` describes it |
| `third_party` | The vendored Acquiescence element-state library and chromium-bidi mapper, with their licenses |
| `scripts` | Scripts for coverage thresholds and merging coverage reports, checking the release's packages, and updating the chromium-bidi mapper |
| `.github` | The CI and release workflows, which take effect once this directory is a repository of its own |
| `skills` | The `dramaturge` agent skill, and the plugin that `.claude-plugin/marketplace.json` publishes it as |

## License

MIT; see [LICENSE](LICENSE).
