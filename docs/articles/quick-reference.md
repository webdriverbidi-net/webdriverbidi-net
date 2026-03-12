# Quick Reference

A cheat sheet of common WebDriverBiDi.NET commands and patterns.

## Driver Lifecycle

| Operation | Code |
|-----------|------|
| Create driver | `BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));` |
| Start connection | `await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");` |
| Check if started | `if (driver.IsStarted) { ... }` |
| Stop connection | `await driver.StopAsync();` |
| Dispose | `await driver.DisposeAsync();` |

## Session

| Operation | Code |
|-----------|------|
| Check status | `await driver.Session.StatusAsync(null);` |
| Subscribe to events | `SubscribeCommandParameters sub = new SubscribeCommandParameters(driver.Network.OnBeforeRequestSent.EventName); await driver.Session.SubscribeAsync(sub);` |
| End session | `await driver.Session.EndAsync(null);` |

## Browsing Context

| Operation | Code |
|-----------|------|
| Get context tree | `GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(null);` |
| Create context | `CreateCommandResult ctx = await driver.BrowsingContext.CreateAsync(new CreateCommandParameters(CreateType.Tab));` |
| Navigate | `await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters(contextId, url) { Wait = ReadinessState.Complete });` |
| Close context | `await driver.BrowsingContext.CloseAsync(new CloseCommandParameters(contextId));` |
| Capture screenshot | `await driver.BrowsingContext.CaptureScreenshotAsync(new CaptureScreenshotCommandParameters(contextId));` |

## Script

| Operation | Code |
|-----------|------|
| Evaluate expression | `EvaluateResult r = await driver.Script.EvaluateAsync(new EvaluateCommandParameters(expression, new ContextTarget(contextId), true));` |
| Call function | `EvaluateResult r = await driver.Script.CallFunctionAsync(new CallFunctionCommandParameters(functionDeclaration, new ContextTarget(contextId), true));` |
| Add preload script | `AddPreloadScriptCommandResult r = await driver.Script.AddPreloadScriptAsync(new AddPreloadScriptCommandParameters(script));` |
| Get realms | `GetRealmsCommandResult realms = await driver.Script.GetRealmsAsync(null);` |

## Network

| Operation | Code |
|-----------|------|
| Add intercept | `AddInterceptCommandParameters p = new AddInterceptCommandParameters(); p.Phases.Add(InterceptPhase.BeforeRequestSent); await driver.Network.AddInterceptAsync(p);` |
| Add data collector | `AddDataCollectorCommandParameters p = new AddDataCollectorCommandParameters(); p.BrowsingContexts.Add(contextId); await driver.Network.AddDataCollectorAsync(p);` |
| Continue request | `await driver.Network.ContinueRequestAsync(new ContinueRequestCommandParameters(requestId));` |
| Provide response | `ProvideResponseCommandParameters p = new ProvideResponseCommandParameters(requestId) { Body = BytesValue.FromString(body) }; await driver.Network.ProvideResponseAsync(p);` |

## Storage

| Operation | Code |
|-----------|------|
| Get cookies | `GetCookiesCommandParameters p = new GetCookiesCommandParameters(); p.Partition = new BrowsingContextPartitionDescriptor(contextId); GetCookiesCommandResult r = await driver.Storage.GetCookiesAsync(p);` |
| Set cookie | `await driver.Storage.SetCookieAsync(new SetCookieCommandParameters(new PartialCookie(name, value) { Domain = domain }));` |
| Delete cookies | `DeleteCookiesCommandParameters p = new DeleteCookiesCommandParameters(); p.Partition = new BrowsingContextPartitionDescriptor(contextId); await driver.Storage.DeleteCookiesAsync(p);` |

## Input

| Operation | Code |
|-----------|------|
| Perform actions | `PerformActionsCommandParameters p = new PerformActionsCommandParameters(contextId); p.Actions.Add(new Action(p.AddPointerInput("mouse", PointerType.Mouse))); await driver.Input.PerformActionsAsync(p);` |

## Event Subscription Pattern

[!code-csharp[Event Subscription Pattern](../code/QuickReferenceSamples.cs#EventSubscriptionPattern)]

## Error Configuration

[!code-csharp[Error Configuration](../code/QuickReferenceSamples.cs#ErrorConfiguration)]

## See Also

- [API Design Guide](advanced/api-design.md): Parameter patterns, timeouts, versioning
- [Core Concepts](core-concepts.md): Full command and event documentation
- [API Reference](../api/index.md): Complete API documentation
