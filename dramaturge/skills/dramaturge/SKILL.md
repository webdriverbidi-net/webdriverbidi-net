---
name: dramaturge
description: Writing C# browser automation or browser tests with Dramaturge (the Dramaturge NuGet package), built on WebDriver BiDi: launching Chrome, Firefox, or Edge with BrowserGroup, finding elements with locators (GetByRole, GetByLabel, GetByText, GetByTestId), acting on them (ClickAsync, FillAsync), asserting with Expect, routes, dialogs, downloads, and network capture. Use when creating, reviewing, or debugging such code or tests.
---

# Dramaturge

Dramaturge is browser automation for .NET over the W3C WebDriver BiDi protocol, built on WebDriverBiDi.NET. It launches the browser, finds elements, acts on them as a user would, and checks the page, **waiting at each step** until the page is ready.

- The documentation's index for language models is https://webdriverbidi-net.github.io/dramaturge/llms.txt, with every article and its code in llms-full.txt beside it. Consult them for anything this skill does not cover.
- Packages: `Dramaturge` (with `Dramaturge.Browsers`, which downloads and launches browsers). It is at 0.0.x; any release may change its API.
- For raw protocol access, `BrowserGroup.Driver` is the underlying WebDriverBiDi.NET `BiDiDriver`.

## Launching

A `BrowserGroup` is a browser process and its session; disposing it closes the browser. A `Browser` is a user context (its own cookies, storage, and cache), and a `Page` is a tab.

<!-- readme-csharp: docs/code/skill/SkillSamples.cs#Launch -->
```csharp
using Dramaturge;
using Dramaturge.Browsers;

DramaturgeOptions options = new() { ActionTimeout = TimeSpan.FromSeconds(10) };
await using BrowserGroup group = await BrowserGroup.LaunchAsync(BrowserLauncher.Configure(BrowserKind.Chrome).WithHeadlessOption(), options);

// A browser of its own: cookies, storage, and cache no other browser in the group shares.
Browser browser = await group.CreateBrowserAsync();
Page page = await browser.NewPageAsync();
await page.NavigateAsync("https://example.com/login");
```

- Launch once per test class or suite, not per test; give **each test a browser of its own** with `CreateBrowserAsync()`, and close it after the test. See [references/test-setup.md](references/test-setup.md).
- The first launch downloads Chrome for Testing or Firefox into a cache. In CI, install them ahead with the `dramaturge` tool, or point at installed browsers with `AtLocation` or `CHROME_EXECUTABLE` / `FIREFOX_EXECUTABLE`.
- `DramaturgeOptions`: `ActionTimeout` and `NavigationTimeout` (30 s), `ExpectTimeout` (5 s), `TestIdAttribute` (`data-testid`), `PierceShadowRoots`.

## Locators

Prefer what a user sees, in this order:

1. `GetByRole(role, name)`: `button`, `link`, `heading`, `textbox`, `checkbox`, `row`, `dialog`, …; the name must match exactly.
2. `GetByLabel`, `GetByText`, `GetByPlaceholder`, `GetByAltText`, `GetByTitle`: contain the text, ignoring case, unless `exact: true`.
3. `GetByTestId`.
4. `Locate(new CssLocator(…))` or `XPathLocator`, last, and short.

<!-- readme-csharp: docs/code/skill/SkillSamples.cs#Locators -->
```csharp
using Dramaturge;

await page.GetByRole("button", "Sign in").ClickAsync();
await page.GetByLabel("Email address").FillAsync("ada@example.com");
await page.GetByPlaceholder("Search").FillAsync("lovelace");
await page.GetByTestId("checkout").ClickAsync();

// Narrow by chaining and filtering rather than by position or a long CSS path.
ElementLocator row = page.GetByRole("row").Filter(hasText: "Ada");
await row.GetByRole("button", "Delete").ClickAsync();
```

A locator finds nothing until it is used, and is looked up again each time. An action or single-element assertion needs **exactly one** match, else `AmbiguousElementException`: narrow it with chaining or `Filter`, or choose with `First()`, `Nth(i)` (zero-based), `Last()`. See [references/locators.md](references/locators.md).

## Waiting: Never Sleep

Actions wait for their element to be visible, stable, enabled, and not covered; reads wait for it to exist; navigations wait for the load. **Never add `Thread.Sleep`, `Task.Delay`, or polling loops.** Check the page with `Expect`, which retries, never with a read and a test framework assertion, which checks once:

<!-- inline-csharp: an anti-pattern, shown so that it is recognized -->
```csharp
// Wrong: reads once, races the page, and needs a sleep to pass.
await Task.Delay(1000);
Assert.Equal("Saved", await page.GetByRole("status").TextContentAsync());
```

<!-- readme-csharp: docs/code/skill/SkillSamples.cs#Assertions -->
```csharp
using System.Text.RegularExpressions;
using Dramaturge;
using static Dramaturge.Assertions;

await page.GetByRole("button", "Save").ClickAsync();

// Each is checked again until it holds, or fails after five seconds saying what it last saw.
await Expect(page.GetByRole("status")).ToHaveTextAsync("Saved");
await Expect(page.GetByRole("progressbar")).Not.ToBeVisibleAsync();
await Expect(page).ToHaveUrlAsync(new Regex("/orders/\\d+$"));
await Expect(page.GetByRole("listitem")).ToHaveCountAsync(3);
```

`Expect` needs `using static Dramaturge.Assertions;`. It throws `ExpectationFailedException`, so it works with any test framework. See [references/assertions.md](references/assertions.md).

## Actions

`ClickAsync`, `DblClickAsync`, `HoverAsync`, `TapAsync`, `DragToAsync`, `FillAsync` (replaces text), `PressSequentiallyAsync` (types key by key), `PressAsync(Keys.Enter)`, `CheckAsync`/`UncheckAsync`/`SetCheckedAsync`, `SelectOptionAsync`, `SetInputFilesAsync`. An action that runs out of time throws `WebDriverBiDiTimeoutException` naming what it last saw. **`Force = true` skips the readiness checks; use it only for a specific, understood reason.** See [references/actions.md](references/actions.md).

## Pages, Navigation, and Events

<!-- readme-csharp: docs/code/skill/SkillSamples.cs#Waits -->
```csharp
using Dramaturge;

// A click that navigates does not wait for the new page; wrap it when the next step needs that page.
await page.RunAndWaitForNavigationAsync(() => page.GetByRole("link", "Checkout").ClickAsync());

Download download = await page.RunAndWaitForDownloadAsync(() => page.GetByRole("link", "Export").ClickAsync());
DownloadOutcome outcome = await download.WaitForEndAsync();
```

- `NavigateAsync`, `ReloadAsync`, `GoBackAsync` wait for the load. A click does not: use `RunAndWaitForNavigationAsync`, `WaitForUrlAsync`, or `Expect(page).ToHaveUrlAsync`.
- Dialogs: the browser dismisses them unless the user prompt handler is `Ignore` (`BrowserOptions.UnhandledPromptBehavior`, or on Firefox the session capability); then answer them in `page.OnDialog`, without awaiting the action that opened them.
- `OnPopup`, `OnConsoleMessage`, `OnPageError`, `OnDownload`; `EvaluateAsync<T>` runs JavaScript in the page; frames come from `locator.ContentFrameAsync()`.

## Network

<!-- readme-csharp: docs/code/skill/SkillSamples.cs#Routes -->
```csharp
using Dramaturge;

// Give a body or a header: Chrome sends a status-only response on to the network instead.
RouteRegistration route = await page.RouteAsync(
    "https://api.example.com/v1/user",
    r => r.FulfillAsync(200, """{"name":"Ada"}""", new Dictionary<string, string>() { ["Content-Type"] = "application/json" }));
await page.NavigateAsync("https://example.com/profile");
await route.RemoveAsync();
```

Routes answer (`FulfillAsync`), change (`ContinueAsync`), or fail (`AbortAsync`) a page's requests, newest route first. `RunAndWaitForRequestAsync`/`RunAndWaitForResponseAsync` wait for traffic an action causes; `NetworkTrafficMonitor` records it as HAR. See [references/network.md](references/network.md).

## Known Browser Gaps

- Firefox: `GetByText` is unsupported (its innerText locator, bug 1869538); native HTML5 drag-and-drop fires only `dragstart` (bug 1515879); the dialog handler given to a new browser is ignored (bug 1975279).
- Chrome: a route's `FulfillAsync` with only a status code is sent to the network instead; give a body or header. Screenshots of elements inside iframes fail.

## Rules

Before finishing code that uses Dramaturge, check that it:

1. Uses role, label, or text locators before test IDs, and CSS or XPath only as a short last resort.
2. Contains no `Thread.Sleep`, `Task.Delay`, or retry loops.
3. Asserts with `Expect`, not a read followed by a test framework assertion.
4. Makes each single-element locator match one element.
5. Waits for a navigation a click causes before relying on the new page.
6. Uses `Force` only with a stated reason.
7. Launches the browser once per class or suite, gives each test its own `Browser`, and disposes the group.
8. Gives fulfilled responses a body or header.
