# WebDriverBiDi.Extensions Package

Conveniences for code that works with the WebDriver BiDi protocol directly: one-call extension methods, an input action builder, and network traffic capture with HAR output.

## Overview

The core `WebDriverBiDi` package mirrors the protocol: each command takes a parameters object and returns a result object. That keeps it complete and predictable, and it makes common tasks verbose. This package adds shortcuts on top, without hiding the protocol:

- **Extension methods** on the modules send one command, or a short fixed sequence, and return natural .NET types.
- **`InputBuilder`** assembles `input.performActions` sequences.
- **`NetworkTrafficMonitor`** records a page's traffic, and **`HarGenerator`** writes it as an HTTP Archive.

Every type and method lives in the namespace of the module it belongs with, such as `WebDriverBiDi.BrowsingContext` and `WebDriverBiDi.Network`, so no extra `using` directive is needed.

Nothing here waits for page state, retries, or keeps state between calls, except the monitor, which exists to record. For browsers to drive, see the [Browser Setup Guide](../browser-setup.md).

## Installation

```bash
dotnet add package WebDriverBiDi.Extensions
```

## Extension Methods

[!code-csharp[Extensions Quick Start](../../code/PackageReadmeSamples.cs#ExtensionsQuickStart)]

| Module | Method | Returns |
| --- | --- | --- |
| Session | `SubscribeAsync(eventNames, browsingContextIds)` | The subscription ID |
| Session | `UnsubscribeAsync(subscriptionId)` | |
| BrowsingContext | `GetTopLevelBrowsingContextsAsync()` | The tabs and windows, without their frames |
| BrowsingContext | `NavigateAsync(contextId, url, wait)` | The URL navigated to |
| BrowsingContext | `CaptureScreenshotAsync(contextId, origin)` | The PNG image, as `byte[]` |
| BrowsingContext | `LocateNodesByCssSelectorAsync`, `ByXPathAsync`, `ByAccessibleRoleAsync`, `ByVisibleTextAsync` | References to the matching nodes |
| BrowsingContext | `CloseAsync(contextId)` | |
| Script | `CallFunctionAsync(contextId, function)` and `CallFunctionAsync<T>` | The result, as a `RemoteValue` or the `RemoteValue` type `T` |
| Script | `AddPreloadScriptAsync(function)` / `RemovePreloadScriptAsync(id)` | The preload script ID |
| Input | `ClickElementAsync(contextId, element)`, `SendKeysAsync(contextId, text)` | |
| Input | `PerformActionsAsync(contextId, builder)`, `ReleaseActionsAsync(contextId)` | |

Every method takes the same optional timeout override and `CancellationToken` as the command it sends.

`CallFunctionAsync` awaits a returned promise. When the function throws, it throws a `ScriptException`:

[!code-csharp[Script That Throws](../../code/advanced/ExtensionsSamples.cs#ScriptThatThrows)]

`SendKeysAsync` types into whichever element has focus. To type into a particular element, click it first, or focus it with a script.

## Input

[!code-csharp[Input](../../code/PackageReadmeSamples.cs#ExtensionsInput)]

The helper methods cover the common gestures:

[!code-csharp[Input Helpers](../../code/advanced/ExtensionsSamples.cs#InputHelpers)]

### How Ticks Work

The builder follows the protocol's model:
- Each input source (a keyboard, a pointer, a wheel, or a `none` source that only pauses) has its own list of actions.
- The browser runs them in ticks: in each tick it takes the next action from every source.
- `AddAction` adds a tick in which one source acts and every other source pauses.
- `AddActions` adds a tick in which several sources act together, with at most one action for each source.

A source created partway through pauses for the ticks that came before it.

[!code-csharp[Simultaneous Sources](../../code/advanced/ExtensionsSamples.cs#InputSimultaneousSources)]

`DefaultKeyInputSource`, `DefaultPointerInputSource` (a mouse), and `DefaultWheelInputSource` are created the first time they are used. `CreatePointerInputSource` adds another pointer, such as a pen or a touch point for multi-touch, and that pointer never becomes the default. `Clear()` removes every source and action, including the defaults.

Pointer coordinates are fractional CSS pixels. `PointerActionProperties` sets the pen and touch properties the protocol defines: width, height, pressure, tangential pressure, twist, and altitude and azimuth angles.

Typed text is split into user-perceived characters, so an emoji (including a family, a flag, or one with a skin tone) or a letter with combining accents is sent as one key, on .NET Framework too.

## Network Capture

[!code-csharp[Network Capture](../../code/PackageReadmeSamples.cs#ExtensionsNetworkCapture)]

### What Is Captured

`GetCapturedTrafficAsync` returns the requests recorded since the previous call, in the order they started, once each one has completed or failed and its bodies have been retrieved. A request still in flight when the wait ends is kept for the next call. Stopping the monitor records such a request as failed.

Each hop of a redirect is a separate `NetworkRequest`, with its own `RedirectCount`. The request records:
- the headers, cookies, and sizes;
- the fetch timings, as reported when the response completed;
- the browsing context and navigation it belongs to;
- the outcome.

Request and response bodies are captured with a network data collector. A binary body is kept as base64, with `IsRequestBodyBase64Encoded` or `IsResponseBodyBase64Encoded` set. A body the browser no longer holds, or one larger than `MaxBodySize`, has the reason in `RequestBodyErrorText` or `ResponseBodyErrorText`. Browsers do not keep redirect bodies.

For a quick look, a request can be printed as HTTP text:

[!code-csharp[Request Text](../../code/advanced/ExtensionsSamples.cs#NetworkRequestText)]

### HAR Files

`HarGenerator.Generate` writes HAR 1.2, which browser developer tools and HAR viewers can open:
- **Pages:** each navigation becomes a page, and requests are attached to the page of the latest navigation in their browsing context.
- **Timings:** the phases come from the fetch timing marks. A phase that did not happen is -1, and the entry's `time` is the sum of the phases that did.
- **Bodies:** a text body is written as text and a binary body as base64.
- **Failed requests:** these have status 0 and the error in a custom `_error` field.

By default the creator is recorded as this package and its version. `Generate` takes another name and version for tools that wrap it.

### Modifying Requests and Answering Challenges

[!code-csharp[Request Modification](../../code/PackageReadmeSamples.cs#ExtensionsNetworkModification)]

A modification's pattern is a WebDriver BiDi URL pattern string, which the browser matches against each request before it is sent. A matching request is changed as described and continued. A request that cannot be continued is failed rather than left blocked.

The first credentials that match an authentication challenge's scheme and realm are offered. Credentials without a scheme or realm match any challenge. When the browser keeps rejecting the credentials, the challenge is canceled after `MaxAuthAttempts` answers. When no credentials match, the browser handles the challenge as it normally would.

### Limits

The monitor holds requests until they are retrieved. Two limits stop a long session, or one that is never read, from growing without bound:
- `MaxRetainedRequests`: requests beyond it are not recorded and are counted in `DroppedRequestCount`. A request that the monitor would modify is still modified and continued.
- `MaxBodySize`: the largest body the browser keeps for the monitor.

For a long session, call `GetCapturedTrafficAsync` periodically.

The options are read once, when monitoring starts. To change them, stop monitoring and start it again. Disposing the monitor stops it and removes the intercepts, data collector, and subscription it added to the browser.

## See Also

- [Input Module](../modules/input.md) and [Network Module](../modules/network.md): the protocol commands these conveniences send
- [WebDriverBiDi.Extensions NuGet package](https://www.nuget.org/packages/WebDriverBiDi.Extensions)
