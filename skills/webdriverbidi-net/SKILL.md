---
name: webdriverbidi-net
description: Writing C# code that uses the WebDriverBiDi.NET library (the WebDriverBiDi NuGet package) to drive a browser over the W3C WebDriver BiDi protocol, with BiDiDriver, its modules (BrowsingContext, Script, Network, Session, Log, Input, ...), their commands, and their events. Use when creating, reviewing, or debugging such code, including connecting to Chrome, Firefox, or Edge, subscribing to events, running scripts, and handling errors.
---

# WebDriverBiDi.NET

WebDriverBiDi.NET is a protocol-level .NET client for [WebDriver BiDi](https://w3c.github.io/webdriver-bidi/): each command takes a parameters object and returns a result object, and each event is an observable on its module. It does not wait for page state, retry, or launch browsers.

- For high-level automation or tests (locators, auto-waiting, assertions), use **Dramaturge**, which is built on this library, instead.
- To download and launch browsers, use **Dramaturge.Browsers**.
- The documentation's index for language models is https://webdriverbidi-net.github.io/webdriverbidi-net/llms.txt, and every article with its code is in llms-full.txt beside it. Consult them for anything this skill does not cover.

## Packages

- `WebDriverBiDi`: the client. Targets .NET Standard 2.0 and .NET 8, 9, and 10; AOT-compatible on .NET 8+.
- `WebDriverBiDi.Analyzers`: Roslyn analyzers that catch the mistakes below at compile time. Always add it.
- `WebDriverBiDi.Extensions`: one-call shortcuts, such as `NavigateAsync(contextId, url)`, and `InputBuilder` for input actions.
- `WebDriverBiDi.Logging`: `ILogger` integration.

The library is at 0.x: any release may change its API. Pin an exact version.

## Connecting

The URL must speak WebDriver BiDi. Which one, and whether to create a session, depends on how the browser was started:

| Endpoint | Example | `NewSessionAsync`? |
| --- | --- | --- |
| Firefox with `--remote-debugging-port` | `ws://localhost:9222/session` | Yes, after `StartAsync` |
| geckodriver's BiDi endpoint | `ws://localhost:4444/session` | Yes |
| `webSocketUrl` from a driver's (chromedriver, msedgedriver, geckodriver) classic new-session response with `"webSocketUrl": true` | `ws://localhost:9515/session/<id>` | **No**: the session exists |
| Chrome's `ws://…/devtools/browser/<id>` | | **Never use it**: it is the Chrome DevTools Protocol, not BiDi; the first command fails |

<!-- readme-csharp: docs/code/skill/SkillSamples.cs#ConnectWithNewSession -->
```csharp
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Session;

// Firefox started with --remote-debugging-port=9222 serves WebDriver BiDi at /session, with no session yet.
await using BiDiDriver driver = new(TimeSpan.FromSeconds(30));
await driver.StartAsync("ws://localhost:9222/session");
await driver.Session.NewSessionAsync(new NewCommandParameters());
try
{
    GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
    string contextId = tree.ContextTree[0].BrowsingContextId;
    await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters(contextId, "https://example.com") { Wait = ReadinessState.Complete });
}
finally
{
    await driver.StopAsync();
}
```

<!-- readme-csharp: docs/code/skill/SkillSamples.cs#ConnectToDriverSession -->
```csharp
using WebDriverBiDi;

// The session already exists, so NewSessionAsync must not be called.
await using BiDiDriver driver = new(TimeSpan.FromSeconds(30));
await driver.StartAsync(webSocketUrl);
```

Chrome and Edge speak BiDi only through their driver executables, or through a BiDi-over-CDP mapper (as Dramaturge.Browsers' `ChromiumTransport` does). See [references/connections.md](references/connections.md).

## Commands

Every module method takes its parameters object, an optional `timeoutOverride`, and a `CancellationToken`. The driver's default timeout is 60 seconds (`BiDiDriver.DefaultCommandWaitTimeout`) unless one is given to its constructor.

<!-- readme-csharp: docs/code/skill/SkillSamples.cs#Commands -->
```csharp
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;

NavigateCommandParameters navigate = new(contextId, "https://example.com/report") { Wait = ReadinessState.Interactive };
NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(navigate, TimeSpan.FromMinutes(2), cancellationToken);
Console.WriteLine(result.Url);
```

Optional lists on parameters are read-only and already initialized: add to them (`parameters.Contexts.Add(id)`), never assign them. The exceptions are the nullable `Headers` and `Cookies` of the network continue and provide commands, and the `ClientHintsMetadata` lists, where `null` omits the field and an empty list sends `[]`: initialize those with `??=` before adding.

## Events

Receiving an event takes **two steps**: add an observer, then subscribe with `Session.SubscribeAsync`. An observer alone receives nothing.

<!-- readme-csharp: docs/code/skill/SkillSamples.cs#Events -->
```csharp
using WebDriverBiDi;
using WebDriverBiDi.Network;
using WebDriverBiDi.Session;

// 1. Add the observer. A handler that awaits anything runs asynchronously, or it blocks every message.
using EventObserver<BeforeRequestSentEventArgs> observer = driver.Network.OnBeforeRequestSent.AddObserver(
    async e =>
    {
        await Task.Yield();
        await File.AppendAllTextAsync("requests.log", e.Request.Url + Environment.NewLine);
    },
    ObservableEventHandlerOptions.RunHandlerAsynchronously);

// 2. Subscribe, naming the event through its observable event; without this, the browser sends nothing.
SubscribeCommandResult subscription = await driver.Session.SubscribeAsync(new SubscribeCommandParameters(driver.Network.OnBeforeRequestSent.EventName));
```

- **Handlers run on the transport's single reader thread by default.** A synchronous handler that blocks, or awaits anything, stops all message processing, and a handler that awaits a command response deadlocks. Use `ObservableEventHandlerOptions.RunHandlerAsynchronously` for any handler that does I/O, calls commands, or takes more than a moment, and make its first statement `await Task.Yield();` (the code before the first real `await` still runs on the transport thread).
- Name events with `module.OnSomething.EventName`, never string literals.
- Add observers before subscribing, so that no event is missed. Register custom modules and events before `StartAsync`; observers may be added at any time.
- Dispose observers (`using`) when done.

Asynchronous handlers finish after the command that caused their events. To wait for them, capture their tasks:

<!-- readme-csharp: docs/code/skill/SkillSamples.cs#CapturedTasks -->
```csharp
using WebDriverBiDi;
using WebDriverBiDi.Log;
using WebDriverBiDi.Session;

using EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(
    async e =>
    {
        await Task.Yield();
        await File.AppendAllTextAsync("console.log", e.Text + Environment.NewLine);
    },
    ObservableEventHandlerOptions.RunHandlerAsynchronously);
await driver.Session.SubscribeAsync(new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName));

observer.StartCapturingTasks();
await driver.BrowsingContext.NavigateAsync(navigate);

// True if two entries arrived and their handlers finished within ten seconds.
bool done = await observer.WaitForCapturedTasksCompleteAsync(2, TimeSpan.FromSeconds(10));
observer.StopCapturingTasks();
```

See [references/events-and-observers.md](references/events-and-observers.md).

## Scripts and Remote Values

<!-- readme-csharp: docs/code/skill/SkillSamples.cs#Scripts -->
```csharp
using WebDriverBiDi.Script;

EvaluateResult result = await driver.Script.EvaluateAsync(new EvaluateCommandParameters("document.title", new ContextTarget(contextId), true));
if (result is EvaluateResultException failure)
{
    throw new InvalidOperationException($"The script threw: {failure.ExceptionDetails.Text}");
}

RemoteValue value = ((EvaluateResultSuccess)result).Result;

// As<T>() converts, or throws if the value is not that type; TryAs<T>() tests without throwing.
string title = value.As<StringRemoteValue>().Value;
if (value.TryAs(out NumberRemoteValue? number))
{
    Console.WriteLine(number.Value);
}
```

A script result is a `RemoteValue`. Convert it with `As<T>()` (throws if it is not that type) or `TryAs<T>(out T?)`, to a `RemoteValue` type such as `StringRemoteValue`, `NumberRemoteValue`, `BooleanRemoteValue`, `NodeRemoteValue`, or `CollectionRemoteValue`. A thrown script is an `EvaluateResultException`, not a .NET exception. Pass arguments as `LocalValue`s (`LocalValue.String("…")`, `node.ToSharedReference()`). See [references/remote-values.md](references/remote-values.md).

## Errors

- `WebDriverBiDiCommandException`: the browser rejected a command; `ErrorCode` says why.
- `WebDriverBiDiTimeoutException`: no response in time.
- Exceptions thrown by event handlers, and malformed messages, are **ignored by default**: they are reported only through the driver's diagnostic events. During development, make them throw:

<!-- readme-csharp: docs/code/skill/SkillSamples.cs#ErrorBehaviors -->
```csharp
using WebDriverBiDi;
using WebDriverBiDi.Protocol;

BiDiDriver driver = new(TimeSpan.FromSeconds(30));

// By default these errors are only reported through the driver's diagnostic events. Terminate makes the next
// command throw them.
driver.TransportConfiguration.EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate;
driver.TransportConfiguration.ProtocolErrorBehavior = TransportErrorBehavior.Terminate;
driver.TransportConfiguration.UnknownMessageBehavior = TransportErrorBehavior.Terminate;
driver.TransportConfiguration.UnexpectedErrorBehavior = TransportErrorBehavior.Terminate;
```

<!-- readme-csharp: docs/code/skill/SkillSamples.cs#CommandErrors -->
```csharp
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;

try
{
    await driver.BrowsingContext.NavigateAsync(navigate);
}
catch (WebDriverBiDiCommandException ex) when (ex.ErrorCode == ErrorCode.NoSuchFrame)
{
    // The browser rejected the command; ErrorCode is the protocol's error.
    Console.WriteLine(ex.Message);
}
catch (WebDriverBiDiTimeoutException)
{
    // No response came within the timeout.
}
```

With `TransportErrorBehavior.Collect`, errors are thrown only by `StopAsync()`; `DisposeAsync()` discards them, so call `StopAsync()` explicitly.

## Rules

Before finishing code that uses this library, check that it:

1. Adds an observer **and** calls `Session.SubscribeAsync` for every event it handles.
2. Uses `RunHandlerAsynchronously` (with `await Task.Yield();` first) for any handler that does I/O, sends commands, or blocks.
3. Waits for asynchronous handlers with the capture API before relying on their results.
4. Registers custom modules, events, and type resolvers before `StartAsync`.
5. Connects to a BiDi endpoint, never a `/devtools/` one, and calls `NewSessionAsync` only where no session exists.
6. Adds to optional lists rather than assigning them, and initializes the nullable ones first.
7. Gives slow commands a `timeoutOverride`, and passes a `CancellationToken` through.
8. Sets the transport error behaviors to `Terminate` or `Collect` while developing.
9. Protects shared state that several handlers change.
10. Stops the driver and disposes it and its observers (`await using`, `using`).

[references/pitfalls.md](references/pitfalls.md) explains each rule.
