# WebDriverBiDi.Extensions

Conveniences for the [WebDriverBiDi](https://www.nuget.org/packages/WebDriverBiDi) .NET client library, for code that works with the protocol directly.

- One-call extension methods for common commands, returning natural .NET types: a screenshot as `byte[]`, located nodes as a list of references, a script's result as the `RemoteValue` type you ask for.
- `InputBuilder`, which builds `input.performActions` sequences tick by tick, with helpers for clicking, typing, key chords, drag-and-drop, and scrolling.
- Works on .NET Standard 2.0, .NET 8 and later, and with native AOT.

Every type and extension method is in the namespace of the module it belongs with (`WebDriverBiDi.BrowsingContext`, `WebDriverBiDi.Input`, `WebDriverBiDi.Network`, `WebDriverBiDi.Script`, `WebDriverBiDi.Session`), so the `using` directives you already have bring them into scope. Each extension method sends one command, or a short fixed sequence, and takes the same timeout override and `CancellationToken` as the command it wraps.

## Installation

```bash
dotnet add package WebDriverBiDi.Extensions
```

## Quick Start

With a connected `BiDiDriver`:

<!-- readme-csharp: docs/code/PackageReadmeSamples.cs#ExtensionsQuickStart -->
```csharp
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

IReadOnlyList<BrowsingContextInfo> tabs = await driver.BrowsingContext.GetTopLevelBrowsingContextsAsync();
string contextId = tabs[0].BrowsingContextId;

await driver.BrowsingContext.NavigateAsync(contextId, "https://example.com", ReadinessState.Complete);
StringRemoteValue title = await driver.Script.CallFunctionAsync<StringRemoteValue>(contextId, "() => document.title");
byte[] png = await driver.BrowsingContext.CaptureScreenshotAsync(contextId);
File.WriteAllBytes("example.png", png);
```

A script that throws is reported as a `ScriptException`, whose `Details` carry the browser's description of the JavaScript exception.

## Input

<!-- readme-csharp: docs/code/PackageReadmeSamples.cs#ExtensionsInput -->
```csharp
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;
using WebDriverBiDi.Script;

IReadOnlyList<SharedReference> searchBoxes = await driver.BrowsingContext.LocateNodesByCssSelectorAsync(contextId, "input[name=q]");

InputBuilder builder = new InputBuilder()
    .AddClickOnElementAction(searchBoxes[0])
    .AddSendKeysToActiveElementAction("WebDriver BiDi")
    .AddSendKeysToActiveElementAction(Keys.Enter);
await driver.Input.PerformActionsAsync(contextId, builder);

// Releases any keys or buttons an action sequence left pressed.
await driver.Input.ReleaseActionsAsync(contextId);
```

Each action added is one tick, in which every other input source pauses; `AddActions` puts several sources' actions in one tick. Text is typed one user-perceived character at a time, so emoji and combining sequences arrive whole, on .NET Framework too. `Keys` names the special keys (Enter, Shift, the arrows, and so on). For finer control, create sources with `CreateKeyInputSource`, `CreatePointerInputSource` (mouse, pen, or touch), `CreateWheelInputSource`, and `CreateNoneInputSource`, and add the actions they create.

## Documentation

The [WebDriverBiDi.Extensions guide](https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/webdriverbidi-extensions.html) covers the package in more detail, and the [API reference](https://webdriverbidi-net.github.io/webdriverbidi-net/api/index.html) lists every member.
