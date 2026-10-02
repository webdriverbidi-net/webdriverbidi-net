# Getting Started

Dramaturge automates Chrome, Firefox, and Edge from .NET. It launches the browser, finds elements, acts on them the way a user would, and checks the page, waiting at each step until the page is ready.

> **Pre-release:** Dramaturge is at version 0.0.x, and its API may change in any release.

## Installation

```bash
dotnet add package Dramaturge
```

Dramaturge runs on .NET Standard 2.0 (so on .NET Framework 4.6.2 and later) and .NET 10, and supports native AOT on .NET 10. The [Dramaturge.Browsers](browser-setup.md) package, which downloads and launches browsers, comes with it.

## A First Program

[!code-csharp[First Program](../code/GettingStartedSamples.cs#FirstProgram)]

The first launch downloads Chrome for Testing into a cache in your user profile, so it takes a little while; later launches use the cached copy. `Expect` is a static method of `Assertions`, brought in with `using static Dramaturge.Assertions;`.

## The Objects

- A **`BrowserGroup`** is one browser process and the WebDriver BiDi session that drives it. `BrowserGroup.LaunchAsync` starts both from a configured launcher; disposing the group ends the session and closes the browser.
- A **`Browser`** is a set of pages that share cookies, storage, and cache: a WebDriver BiDi user context. `DefaultBrowser` is the one every browser process has; `CreateBrowserAsync` adds another, isolated from the rest, with its own [options](configuration.md#browser-options).
- A **`Page`** is a tab or window, and its **`Frame`s** are its main document and the iframes within it. A page's navigation, script, and locator methods act on its main frame.
- An **`ElementLocator`** describes how to find elements. It finds nothing when it is created; each action or read looks the element up again, so a locator stays correct when the page changes underneath it.

[!code-csharp[Object Model](../code/GettingStartedSamples.cs#ObjectModel)]

## Waiting

Dramaturge waits so that your code does not have to:

- **Actions wait for their element.** A click waits until exactly one element matches, and until it is visible, stable (not moving), enabled, and not covered by another element, scrolling it into view if needed. Typing waits until the element can be edited. An element that is removed or replaced while an action waits is found again.
- **Reads wait for their element.** `TextContentAsync`, `InputValueAsync`, `GetAttributeAsync`, and the other reads wait until exactly one element matches. `CountAsync` and `IsVisibleAsync` answer at once.
- **Navigation waits for the page to load.** `NavigateAsync` and the other navigation methods wait until the document is loaded, or until the state you ask for.
- **Expectations retry.** `Expect(...)` checks its condition again until it holds, and fails after its timeout saying what it last saw. Use it, not a read and a .NET assertion, for anything the page may still be changing.

Nothing waits for a fixed time. The limits are 30 seconds for actions and navigation, and 5 seconds for expectations; [Configuration](configuration.md) changes them.

## When Something Fails

[!code-csharp[Failures](../code/GettingStartedSamples.cs#Failures)]

- `WebDriverBiDiTimeoutException`: an action, read, or wait ran out of time. The message names the locator and what the last check saw.
- `ExpectationFailedException`: an expectation was not met in time. Its `Expected` and `Actual` properties say what was required and what was seen.
- `AmbiguousElementException`: a locator for one element matched several. Narrow it, or choose one with `First()`, `Last()`, or `Nth(index)`.
- `InvalidOperationException`: the element cannot do what was asked, such as filling a `<div>`. This fails at once, because waiting would not help.

None of these depend on a test framework, so Dramaturge works with xUnit, NUnit, MSTest, or none.

## With a Driver You Connected Yourself

Dramaturge is built on [WebDriverBiDi.NET](https://webdriverbidi-net.github.io/webdriverbidi-net/), and can be added to a `BiDiDriver` that is already connected, to use both:

[!code-csharp[Connect to a Driver](../code/GettingStartedSamples.cs#ConnectToDriver)]

## Next Steps

- [Browser Setup](browser-setup.md): other browsers, channels, and versions, remote grids, and running browsers
- [Configuration](configuration.md): timeouts and per-browser settings
- [Network Capture](network-capture.md): recording a page's traffic as a HAR file
- [Command-Line Tool](command-line-tool.md): installing browsers ahead of time, for CI
