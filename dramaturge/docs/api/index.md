# API Reference

The public types of the Dramaturge packages, generated from their XML documentation comments.

- **Dramaturge**: browsers, pages, frames, element locators and their actions, assertions (`Assertions.Expect`), and
  options (`DramaturgeOptions`).
- **Dramaturge.Network**: `NetworkTrafficMonitor`, which records a page's network traffic, and `HarGenerator`, which
  writes it as an HTTP Archive.
- **Dramaturge.Browsers**: `BrowserLauncher` and the types that locate, download, and launch browsers and their
  drivers.

Dramaturge is built on [WebDriverBiDi.NET](https://webdriverbidi-net.github.io/webdriverbidi-net/), whose types,
such as `BiDiDriver` and the protocol's modules, appear in some of these APIs and are documented there.
