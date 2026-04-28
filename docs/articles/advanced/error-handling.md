# Error Handling

This guide covers comprehensive error handling strategies for WebDriverBiDi.NET applications.

## Overview

WebDriverBiDi.NET operations can fail for various reasons:
- Network connectivity issues
- Browser crashes or disconnections
- Invalid command parameters
- Timeout waiting for responses
- JavaScript exceptions in the browser
- Protocol-level errors

Understanding how to handle these errors properly is crucial for building robust automation.

## Exception Types

### WebDriverBiDiException

The primary exception type thrown by WebDriverBiDi.NET for protocol-level errors:

[!code-csharp[WebDriverBiDiException](../../code/error-handling/ErrorHandlingSamples.cs#WebDriverBiDiException)]

### Common Error Scenarios

[!code-csharp[Common Error Scenarios](../../code/error-handling/ErrorHandlingSamples.cs#CommonErrorScenarios)]

## Transport Error Behavior Configuration

WebDriverBiDi.NET allows you to configure how transport-layer errors are handled using the `TransportErrorBehavior` enum. This controls errors that occur in event handlers and protocol-level errors (like invalid JSON or incorrect payloads).

**Important:** Command errors always throw exceptions immediately, regardless of this setting. This behavior only affects:
- Exceptions thrown by event handlers
- Protocol errors (invalid JSON, malformed messages)
- Unexpected error responses without matching commands

```csharp
public enum TransportErrorBehavior
{
    Ignore,     // Silently ignore transport errors (default)
    Collect,    // Store errors for later inspection
    Terminate   // Throw exception on next command
}
```

### Understanding Event Handler Error Propagation

Event handlers run on separate threads from your main application code. This means exceptions in event handlers don't directly propagate to the calling code. The transport error behavior determines what happens when event handler exceptions occur:

- **Ignore (default)**: Exception is discarded and logged
- **Collect**: Exception is stored in a list for later inspection
- **Terminate**: Exception is stored and thrown when you send the next command

For handlers registered with `ObservableEventHandlerOptions.RunHandlerAsynchronously`, this behavior also applies to exceptions that occur after the handler has returned control to the transport thread. In other words, exceptions from handlers being run asynchronously are not silently dropped.

The one important exception is capture session task capture. If you capture async handler tasks by using `WaitForCapturedTasksAsync()` or `WaitForCapturedTasksCompleteAsync()`, those task exceptions remain owned by your code. They are propagated through the captured task path rather than being surfaced again through `EventHandlerExceptionBehavior`.

### Why Ignore is the Default

WebDriverBiDi.NET defaults all error behaviors to `Ignore` for several important reasons:

**1. Protocol Stability During Evolution**
- The WebDriver BiDi protocol is actively evolving with new features being added regularly
- Browsers may send events or messages that aren't yet fully specified
- Unknown message types (valid JSON that doesn't match any known structure) are common during protocol transitions
- `Ignore` mode allows automation to continue working even when protocols diverge slightly between library and browser versions

**2. Event Handler Resilience**
- Event handlers are secondary to the main automation flow
- In many cases, a failing log observer or network monitor shouldn't halt critical automation workflows
- Users can explicitly handle errors within their handlers using try-catch blocks
- Forcing all users to handle potential event handler exceptions would add significant boilerplate

**3. Graceful Degradation**
- Libraries like this are often used for web scraping, testing, and monitoring where partial success is acceptable
- Stopping the entire driver on a malformed protocol message would be overly aggressive
- Users can opt-in to stricter error handling (Terminate mode) when needed

**4. Backward Compatibility**
- As browsers implement new WebDriver BiDi features, older library versions will encounter unknown protocol extensions
- `Ignore` allows older library versions to continue functioning with newer browsers
- This is especially important for long-running automation infrastructure

**When to Change from the Default:**
- **Development/Testing**: Use `Terminate` mode to catch issues early and ensure proper error handling
- **Diagnostics**: Use `Collect` mode to gather all errors for troubleshooting
- **Production (with mature code)**: Consider `Terminate` mode once your automation is stable and tested

### Ignore Mode (Default)

Ignore mode silently discards all transport errors. This is the default behavior for all error types:

[!code-csharp[Ignore Mode](../../code/error-handling/ErrorHandlingSamples.cs#IgnoreMode)]

**Use Ignore Mode When:**
- Working with evolving protocol versions where unknown messages are expected
- Event handler failures shouldn't stop critical automation workflows
- You handle all errors within event handlers themselves using try-catch
- Operating in scenarios where graceful degradation is preferred
- Backward compatibility is more important than strict error reporting

**⚠️ Note:** While this is the default, consider using `Terminate` mode during development to catch issues early. The default exists primarily for protocol stability and backward compatibility, not because it's always the best choice for your use case

### Collect Mode

Collect mode stores transport errors in a list, throwing them when the driver is stopped:

[!code-csharp[Collect Mode](../../code/error-handling/ErrorHandlingSamples.cs#CollectMode)]

**Use Collect Mode When:**
- Diagnosing flaky or failing event handlers
- You want to continue operation despite event handler errors
- Testing error resilience of your event handling code
- You need a complete error report after operations
- Protocol errors might be transient

**Important:** Your code continues normally—errors throw when driver stopped as an `AggregateException`.
This includes exceptions from handlers being run asynchronously unless you have explicitly captured those handler tasks via a capture session.

### Terminate Mode

Terminate mode stores exceptions from event handlers and throws them when you send the next command:

[!code-csharp[Terminate Mode](../../code/error-handling/ErrorHandlingSamples.cs#TerminateMode)]

**Note:** If an error log event occurs, the exception won't throw immediately because the event handler runs on a separate thread. The exception will be thrown when you send the next command (e.g., `NavigateAsync`), and your catch block will receive it.

**Why This Matters:**
- Event handlers execute asynchronously on the transport thread
- Your main code doesn't directly wait for event handlers to complete
- Terminate mode ensures errors are eventually reported to your code, including exceptions from handlers being run asynchronously
- The error surfaces when you send the next command, which is a natural synchronization point

**Capture-session-owned exceptions are different:** if you use `WaitForCapturedTasksAsync()` or `WaitForCapturedTasksCompleteAsync()` to take ownership of async handler tasks, exceptions from those tasks propagate through the returned task path instead of terminating on the next command.

**Use Terminate Mode When:**
- You want event handler errors to be reported (recommended for development)
- You need fast failure on protocol errors
- Operating in production with known stable protocol versions
- You prefer explicit error handling over silent failures

### Behavior Comparison

| Mode | Event Handler Exceptions | Protocol Errors | Command Errors | When Error Surfaces |
|------|-------------------------|-----------------|----------------|---------------------|
| **Ignore (default)** | Discarded and logged, including exceptions from asynchronously run handlers when those tasks are not capture-session-owned | Discarded and logged | Always throws immediately | Never |
| **Collect** | Stored in list, including exceptions from asynchronously run handlers when those tasks are not capture-session-owned | Stored in list | Always throws immediately | When driver stopped |
| **Terminate** | Throws on next command, including exceptions from asynchronously run handlers when those tasks are not capture-session-owned | Throws on next command | Always throws immediately | Synchronization point (next command) |

When async handler tasks are explicitly owned via a capture session, their exceptions are owned by the caller instead of being routed through the transport behavior above.

### Threading Model and Error Propagation

Understanding the threading model is crucial for error handling:

[!code-csharp[Threading Model](../../code/error-handling/ErrorHandlingSamples.cs#ThreadingModel)]

### Connection-Level Error Monitoring

For real-time error visibility without affecting behavior, use connection events:

[!code-csharp[Connection-Level Error Monitoring](../../code/error-handling/ErrorHandlingSamples.cs#Connection-LevelErrorMonitoring)]

### Complete Error Handling Pattern

Combine all approaches for comprehensive error management. See the [Collect Mode](#collect-mode) and [Complete Event Handler Pattern](#complete-event-handler-pattern) sections for the key patterns.

### Best Practices

1. **Use Terminate mode in production**: Ensures errors are reported
2. **Handle errors inside event handlers**: Use try-catch within handlers when possible
3. **Use Collect mode for diagnostics**: Helpful for troubleshooting event handler issues
4. **Monitor connection events**: Use OnConnectionError for real-time error visibility
5. **Remember the threading model**: Event handlers run on separate threads
6. **Check for errors at logical points**: With Collect mode, inspect errors after operations
7. **Never rely on Ignore mode**: Always prefer explicit error handling

## Script Execution Errors

### Handling JavaScript Exceptions

JavaScript errors are returned as `EvaluateResultException` rather than thrown:

[!code-csharp[Handling JavaScript Exceptions](../../code/script/ScriptSamples.cs#HandlingJavaScriptExceptions)]

### Safe Script Execution Pattern

[!code-csharp[TryEvaluateAsync](../../code/script/ScriptSamples.cs#TryEvaluateAsync)]

[!code-csharp[TryEvaluateAsync Usage](../../code/script/ScriptSamples.cs#TryEvaluateAsyncUsage)]

## Connection Errors

### Handling Disconnections

[!code-csharp[ResilientDriver Class](../../code/error-handling/ErrorHandlingSamples.cs#ResilientDriverClass)]

[!code-csharp[ResilientDriver Usage](../../code/error-handling/ErrorHandlingSamples.cs#ResilientDriverUsage)]

## Timeout Handling

For per-command timeouts, **prefer the built-in `timeoutOverride` parameter** over custom patterns (e.g., `Task.WhenAny` with `Task.Delay`). Use custom patterns only when you need different semantics than the built-in timeout.

### Preferred Approach: Use the Built-in Timeout Override

The simplest and recommended way to set a per-command timeout is the `timeoutOverride` parameter on module methods. Every module command accepts this as the second parameter:

[!code-csharp[Preferred Approach](../../code/error-handling/TimeoutSamples.cs#PreferredApproach)]

You can also use `ExecuteCommandAsync` when working at the driver level:

[!code-csharp[ExecuteCommandAsync](../../code/error-handling/TimeoutSamples.cs#ExecuteCommandAsync)]

### When to Use Custom Timeout Patterns

Use a custom pattern (e.g., `Task.WhenAny` with `Task.Delay`) only when you need different semantics than the built-in timeout—for example, returning `null` instead of throwing, or implementing custom retry logic:

[!code-csharp[NavigateWithTimeoutAsync](../../code/error-handling/TimeoutSamples.cs#NavigateWithTimeoutAsync)]

[!code-csharp[Usage](../../code/error-handling/TimeoutSamples.cs#Usage)]

**When to use each:**

| Need | Use |
|------|-----|
| Standard per-command timeout | `timeoutOverride` parameter on module methods |
| Return `null` on timeout instead of throwing | Custom wrapper that catches `WebDriverBiDiTimeoutException` |
| Different timeout per retry attempt | Custom retry loop with `timeoutOverride` |

## Element Not Found Handling

### Safe Element Location

[!code-csharp[FindElementSafelyAsync](../../code/error-handling/ErrorHandlingSamples.cs#FindElementSafelyAsync)]

[!code-csharp[FindElementSafelyAsync Usage](../../code/error-handling/ErrorHandlingSamples.cs#FindElementSafelyAsyncUsage)]

### Wait for Element Pattern

[!code-csharp[WaitForElementAsync](../../code/error-handling/ErrorHandlingSamples.cs#WaitForElementAsync)]

## Event Handler Errors

### Observable Event Handler Options

Event handlers can be configured with `ObservableEventHandlerOptions` to control execution behavior:

[!code-csharp[Observable Event Handler Options](../../code/error-handling/ErrorHandlingSamples.cs#ObservableEventHandlerOptions)]

```csharp
public enum ObservableEventHandlerOptions
{
    RunHandlerSynchronously = 0,  // Synchronous execution (default)
    RunHandlerAsynchronously = 1   // Asynchronous execution
}
```

### Synchronous vs. Asynchronous Handlers

By default (`RunHandlerSynchronously`), event handlers run synchronously on the transport thread, which blocks message processing:

[!code-csharp[Synchronous vs Asynchronous Handlers](../../code/error-handling/ErrorHandlingSamples.cs#SynchronousvsAsynchronousHandlers)]

**When to Use Asynchronous Handlers:**
- Handler performs I/O operations (file, network, database)
- Handler does CPU-intensive work
- Handler calls async APIs
- You want to avoid blocking message processing
- **Recommended for most scenarios where handler does more than trivial work**

### Exception Handling in Event Handlers

Event handler exceptions are subject to the `TransportErrorBehavior` setting. Unhandled exceptions depend on the mode (Terminate/Collect/Ignore). Handle exceptions within the handler for best results:

[!code-csharp[Exception Handling in Event Handlers](../../code/error-handling/ErrorHandlingSamples.cs#ExceptionHandlinginEventHandlers)]

### Handler Execution Behavior

[!code-csharp[Handler Execution Behavior](../../code/error-handling/ErrorHandlingSamples.cs#HandlerExecutionBehavior)]

### Multiple Handlers for Same Event

When multiple handlers are registered, they all execute in sequence. With synchronous handlers, Handler 1 executes completely, then Handler 2. With asynchronous handlers, both start and run concurrently:

[!code-csharp[Multiple Handlers Sync](../../code/error-handling/ErrorHandlingSamples.cs#MultipleHandlersSync)]

[!code-csharp[Multiple Handlers Async](../../code/error-handling/ErrorHandlingSamples.cs#MultipleHandlersAsync)]

### Complete Event Handler Pattern

[!code-csharp[Complete Event Handler Pattern](../../code/error-handling/ErrorHandlingSamples.cs#CompleteEventHandlerPattern)]

### Handler Options Decision Guide

| Handler Does | Use Option | Why |
|--------------|------------|-----|
| Quick in-memory work (<10ms) | RunHandlerSynchronously (default) | Minimal overhead, acceptable blocking |
| File I/O | RunHandlerAsynchronously | Don't block transport on disk operations |
| Network requests | RunHandlerAsynchronously | Don't block transport on network |
| Database queries | RunHandlerAsynchronously | Don't block transport on DB operations |
| CPU-intensive work | RunHandlerAsynchronously | Don't block transport thread |
| Logging to console | RunHandlerSynchronously | Quick operation, synchronous is fine |
| Updating counters/state | RunHandlerSynchronously | Quick operation, synchronous is fine |

### Best Practices for Event Handlers

1. **Use async handlers for I/O**: Prevent blocking the transport thread
2. **Handle exceptions internally**: Use try-catch within handlers when possible
3. **Keep handlers fast**: Even async handlers should complete quickly
4. **Don't perform long operations**: Offload heavy work to background services
5. **Test handler error paths**: Verify your error handling works correctly
6. **Monitor handler performance**: Track execution time to identify bottlenecks

## Validation and Defensive Programming

### Parameter Validation

[!code-csharp[Parameter Validation - SafeNavigateAsync](../../code/error-handling/ErrorHandlingSamples.cs#ParameterValidation-SafeNavigateAsync)]

### Context Validation

[!code-csharp[Context Validation - IsContextValidAsync](../../code/error-handling/ErrorHandlingSamples.cs#ContextValidation-IsContextValidAsync)]

[!code-csharp[IsContextValidAsync Usage](../../code/error-handling/ErrorHandlingSamples.cs#IsContextValidAsyncUsage)]

## Logging and Diagnostics

### Comprehensive Logging

[!code-csharp[Comprehensive Logging - ExecuteWithLoggingAsync](../../code/error-handling/ErrorHandlingSamples.cs#ComprehensiveLogging-ExecuteWithLoggingAsync)]

### Error Context Capture

[!code-csharp[ErrorContext Class](../../code/error-handling/ErrorHandlingSamples.cs#ErrorContextClass)]

[!code-csharp[ErrorContext Usage](../../code/error-handling/ErrorHandlingSamples.cs#ErrorContextUsage)]

## Recovery Strategies

### Graceful Degradation

[!code-csharp[Graceful Degradation - GetPageTitleAsync](../../code/error-handling/ErrorHandlingSamples.cs#GracefulDegradation-GetPageTitleAsync)]

### Circuit Breaker Pattern

[!code-csharp[CircuitBreaker Class](../../code/error-handling/ErrorHandlingSamples.cs#CircuitBreakerClass)]

[!code-csharp[CircuitBreaker Usage](../../code/error-handling/ErrorHandlingSamples.cs#CircuitBreakerUsage)]

## Testing Error Scenarios

### Simulating Errors

[!code-csharp[Test Navigation Timeout Pattern](../../code/error-handling/ErrorHandlingSamples.cs#TestNavigationTimeoutPattern)]

[!code-csharp[Test Invalid Context Pattern](../../code/error-handling/ErrorHandlingSamples.cs#TestInvalidContextPattern)]

## Best Practices

1. **Always handle WebDriverBiDiException**: Protocol errors should be caught and handled
2. **Check script result types**: Don't assume success, check for exceptions
3. **Validate inputs**: Check parameters before sending commands
4. **Log errors comprehensively**: Include context for debugging
5. **Implement retries**: Handle transient failures with retry logic
6. **Use timeouts**: Always specify appropriate timeouts
7. **Clean up resources**: Use try-finally or using statements
8. **Test error paths**: Write tests for failure scenarios
9. **Provide fallbacks**: Implement graceful degradation where possible
10. **Monitor health**: Track error rates and patterns

## Common Pitfalls

### Don't Swallow Exceptions

[!code-csharp[Don't Swallow Exceptions](../../code/error-handling/ErrorHandlingSamples.cs#DontSwallowExceptions)]

### Don't Use Generic Catch

[!code-csharp[Don't Use Generic Catch](../../code/error-handling/ErrorHandlingSamples.cs#DontUseGenericCatch)]

## Troubleshooting and FAQ

### Common Error Messages

| Error Message | Likely Cause | What to Do |
|---------------|--------------|------------|
| "Transport must be connected to a remote end to execute commands" | Commands sent before `StartAsync` or after disconnect | Ensure `StartAsync` has completed before sending commands. Check `IsStarted` before operations. |
| "no such frame" / "no such window" | Browsing context was closed or no longer exists | Verify the context ID is still valid. Use `GetTreeAsync` to refresh context list. |
| "Timed out executing command" | Command exceeded the timeout | Increase `timeoutOverride` for the command, or increase `DefaultCommandTimeout` on the driver. |
| "Cannot add command; pending command collection is closed" | Command sent during or after shutdown | Avoid sending commands from event handlers during `StopAsync` or `DisconnectAsync`. |
| "Cannot register a type info resolver after the transport is connected" | `RegisterTypeInfoResolver` called after `StartAsync` | Register type resolvers before calling `StartAsync`. |
| "This observable event only allows N handler(s)" | Too many observers added to an event with `MaxObserverCount` | Remove observers with `Unobserve()` or `Dispose()` before adding new ones. |

### Connection Diagnostics

When the connection fails or behaves unexpectedly:

1. **Verify the WebSocket URL**: Ensure the URL matches what your browser provides (e.g., `ws://localhost:9222/devtools/browser/...`).
2. **Check browser is running**: The remote end must be listening before you connect.
3. **Use connection events**: Subscribe to `connection.OnConnectionError` and `connection.OnLogMessage` for real-time diagnostics.
4. **Inspect `UnhandledErrors`**: When using a custom `Transport`, check `transport.UnhandledErrors` for collected protocol or event-handler errors (when using `TransportErrorBehavior.Collect`).

### Interpreting TransportErrorBehavior

| Mode | When to Use | Trade-off |
|------|-------------|-----------|
| **Ignore** (default) | Protocol evolution, backward compatibility, event handlers that should not stop automation | Errors may go unnoticed. Use for production when you handle errors in handlers. |
| **Collect** | Diagnostics, gathering all errors for post-mortem analysis | Errors surface only when you stop the driver. Operation continues despite errors. |
| **Terminate** | Development, strict protocol conformance, fail-fast behavior | First error stops the driver on the next command. Best for catching issues early. |

### Event Handlers Not Firing

If events are not received:

1. **Add observer before subscribing**: Call `AddObserver` before `Session.SubscribeAsync`.
2. **Subscribe to the event**: Adding an observer alone is not enough—you must call `Session.SubscribeAsync` with the event names.
3. **Use correct event names**: Prefer `driver.Log.OnEntryAdded.EventName` over string literals to avoid typos.
4. **Check context filtering**: If you passed `Contexts` or `UserContexts` to `SubscribeCommandParameters`, events are limited to those contexts.

### See Also

- [API Design Guide](api-design.md): Timeout and cancellation patterns
- [Architecture](../architecture.md): Transport, connection lifecycle, error configuration
- [Common Pitfalls](../common-pitfalls.md): Event subscription, blocking handlers, registration timing

## Next Steps

- [Performance Considerations](performance.md): Optimize your automation
- [Core Concepts](../core-concepts.md): Understanding the fundamentals
- [Architecture](../architecture.md): System design and patterns

