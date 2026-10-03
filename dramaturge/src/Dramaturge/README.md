# Dramaturge

Browser automation for .NET, built on the W3C WebDriver BiDi protocol through the
[WebDriverBiDi](https://www.nuget.org/packages/WebDriverBiDi) library. One API drives Chrome, Firefox, and Edge, and
waits for the page so that your code does not have to.

> **Pre-release:** Dramaturge is at version 0.0.x, and its API may change in any release.

- **Locators** find elements by CSS, XPath, text, accessible role and name, label, placeholder, alt text, title, or
  test ID, chained and filtered, through frames and shadow roots. A locator is a description, looked up again each
  time it is used.
- **Actions** wait until their element is ready: visible, stable, enabled, and not covered by another element,
  scrolled into view if needed. An element replaced while an action waits is found again.
- **Assertions** (`Expect`) are checked again until they hold or their time runs out, and report what they last saw.
  They throw `ExpectationFailedException`, so they work with any test framework.
- **Accessibility snapshots** describe a page as assistive technology sees it, in Playwright's aria snapshot format,
  with refs that turn into locators, and `ToMatchAriaSnapshotAsync` to assert a page's structure.
- **Pages** wait for navigation and load states, and handle dialogs, popups, downloads, screenshots, PDFs, routes that
  answer or change requests, for a page or a whole browser, cookies, and network capture, with HAR files recorded and
  replayed.
- Works on .NET Standard 2.0 and .NET 10, and with native AOT.

## Installation

```bash
dotnet add package Dramaturge
```

The [Dramaturge.Browsers](https://www.nuget.org/packages/Dramaturge.Browsers) package, which downloads and launches
browsers, comes with it.

## Quick Start

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

Timeouts, the polling interval, and other settings are in `DramaturgeOptions`, passed to `LaunchAsync`.
`BrowserGroup.ConnectAsync` adds Dramaturge to a `BiDiDriver` you have already connected.

## Documentation

The [Dramaturge documentation](https://webdriverbidi-net.github.io/dramaturge/) has guides to each part of the API and the
[API reference](https://webdriverbidi-net.github.io/dramaturge/api/index.html).
