# Dramaturge Documentation

**Dramaturge** is browser automation for .NET, built on the W3C WebDriver BiDi protocol through
[WebDriverBiDi.NET](https://webdriverbidi-net.github.io/webdriverbidi-net/). It drives Chrome, Firefox, and Edge
with one API, and waits for the page so that tests do not have to.

> **Pre-release:** Dramaturge is at version 0.0.x, and its API may change in any release.

## What It Does

- **Launches browsers**: downloads Chrome or Firefox on first use, or uses the browser installed, a remote grid, or a
  browser already running (`Dramaturge.Browsers`).
- **Finds elements**: by CSS, XPath, text, accessible role and name, label, placeholder, and test ID, chained and
  filtered, through frames and shadow roots.
- **Waits automatically**: an action waits until its element is visible, stable, enabled, and not covered, and an
  element removed while it waits is looked up again.
- **Acts like a user**: clicks, typing, key presses, form filling, drag and drop, files, and touch, through
  WebDriver BiDi's input actions.
- **Asserts with retries**: `Expect` checks a condition again until it holds or its time runs out, and reports what
  it last saw.
- **Handles the rest of a page**: navigation, dialogs, popups, downloads, screenshots, PDFs, routes that answer or
  change requests, cookies, and network capture to HAR.

## Packages

| Package | Contents |
| --- | --- |
| `Dramaturge` | The automation API |
| `Dramaturge.Browsers` | Locates, downloads, and launches browsers |
| `Dramaturge.Tool` | The `dramaturge` command-line tool, which installs and manages the browsers Dramaturge downloads |

## Where to Start

- [Getting Started](articles/getting-started.md): a first program, and how Dramaturge waits
- [Browser Setup](articles/browser-setup.md): getting a browser to drive
- [Configuration](articles/configuration.md): timeouts and per-browser settings
- [Network Capture](articles/network-capture.md): recording a page's traffic
- [API Reference](api/index.md)
