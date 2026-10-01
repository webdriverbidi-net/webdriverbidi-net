# WebDriverBiDi.Extensions Package

Conveniences for code that works with the WebDriver BiDi protocol directly: one-call extension methods and an input action builder.

## Overview

The core `WebDriverBiDi` package mirrors the protocol: each command takes a parameters object and returns a result object. That keeps it complete and predictable, and it makes common tasks verbose. This package adds shortcuts on top, without hiding the protocol:

- **Extension methods** on the modules send one command, or a short fixed sequence, and return natural .NET types.
- **`InputBuilder`** assembles `input.performActions` sequences.

Every type and method lives in the namespace of the module it belongs with, such as `WebDriverBiDi.BrowsingContext` and `WebDriverBiDi.Network`, so no extra `using` directive is needed.

Nothing here waits for page state, retries, or keeps state between calls; for that, see [Dramaturge](https://www.nuget.org/packages/Dramaturge), which also records network traffic as HAR files. For browsers to drive, see the [Browser Setup Guide](../browser-setup.md).

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

## See Also

- [Input Module](../modules/input.md): the protocol commands these conveniences send
- [WebDriverBiDi.Extensions NuGet package](https://www.nuget.org/packages/WebDriverBiDi.Extensions)
