# Pitfalls Behind the Rules

The rules in SKILL.md, with the reason for each. The WebDriverBiDi.Analyzers package reports many of these at compile time. Source: https://webdriverbidi-net.github.io/webdriverbidi-net/articles/common-pitfalls.html

1. **Observer without a subscription.** `AddObserver` only registers a handler in the process. The browser sends an event only after `Session.SubscribeAsync` asks for it.
2. **Blocking the transport thread.** Every message is read on one task. A synchronous handler that blocks stalls all of them; one that awaits a command's response deadlocks. `RunHandlerAsynchronously` detaches the handler's task, but an `async` handler still starts on the reading task, so yield first.
3. **Racing asynchronous handlers.** A command returns when the browser answers it; the handlers of the events it caused may still be running. Use `StartCapturingTasks` and `WaitForCapturedTasksCompleteAsync`.
4. **Registering too late.** `RegisterModule`, `RegisterEvent`, and `RegisterTypeInfoResolverAsync` throw `InvalidOperationException` after `StartAsync`, because every message must have a deserializer before messages flow. Observers may be added at any time, but before subscribing.
5. **Connecting to the wrong endpoint.** Chrome's `/devtools/` URLs speak CDP: the socket opens, then the first command fails or times out. Calling `NewSessionAsync` on a driver's `webSocketUrl`, whose session exists, fails with "session not created".
6. **Assigning optional lists.** Optional lists on parameters are read-only and omitted from the message while empty. Only the network `Headers`/`Cookies` and the `ClientHintsMetadata` lists are nullable, because the protocol gives an empty list its own meaning there; initialize them before adding (BIDI017).
7. **Timeouts.** The default is 60 seconds, deliberately long. Give a slow command a `timeoutOverride` on the module method, and pass a `CancellationToken` (BIDI004, BIDI013).
8. **Silent handler errors.** The transport ignores handler exceptions and bad messages by default. `Terminate` makes the next command throw them; `Collect` makes `StopAsync` throw them, and `DisposeAsync` discards them (BIDI012).
9. **Shared state.** Transport processing, commands, and observer registration are thread-safe; your handlers' shared state is not, and asynchronous handlers run concurrently.
10. **Leaks.** Undisposed observers keep handlers alive, and an unstopped driver keeps its connection open. Use `using` and `await using`, and stop before disposing.
