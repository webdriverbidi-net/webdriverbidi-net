# Performance Considerations

This guide covers strategies for optimizing the performance of your WebDriverBiDi.NET automation.

## Overview

Performance in browser automation involves multiple factors:
- Command execution time
- Network communication overhead
- Event processing efficiency
- Resource management
- Browser responsiveness

Understanding and optimizing these factors can significantly improve automation speed and reliability.

## Connection Type Performance

WebDriverBiDi.NET primarily uses WebSocket connections for browser communication. An alternative pipe-based connection is available for specialized scenarios.

### WebSocket Connections (Recommended)

WebSocket connections are the standard transport mechanism for WebDriver BiDi:

**Characteristics:**
- **Universal compatibility**: Supported by all browsers with WebDriver BiDi
- **Network flexibility**: Connect to local or remote browsers
- **Simple setup**: Just provide a WebSocket URL
- **Low latency**: Typically 1-3ms per command for local connections

**Performance:**
- Command execution: 5-15ms average (including browser processing)
- Event delivery: Real-time with minimal overhead
- Suitable for all automation scenarios

[!code-csharp[WebSocket Connection](../../code/advanced/PerformanceSamples.cs#WebSocketConnection)]

### Pipe Connections (Advanced)

For specific scenarios where the browser and test runner are co-located, pipe connections provide an alternative transport:

**When to consider:**
- Browser implementation supports pipe protocol (currently only Chromium-based browsers)
- Both browser and tests run on the same machine
- Absolute minimum latency is critical

**Trade-offs:**
- Slightly lower latency (~0.5-1ms reduction per message)
- No network stack overhead
- Requires process lifecycle management
- Limited browser support
- No remote debugging capability

[!code-csharp[Pipe Connection](../../code/advanced/PerformanceSamples.cs#PipeConnection)]

**Recommendation**: Use WebSocket connections unless you have specific requirements for pipe-based transport and are using a compatible browser implementation.

## Command Optimization

### Parallel Command Execution

Execute independent commands in parallel:

[!code-csharp[Parallel Commands](../../code/advanced/PerformanceSamples.cs#ParallelCommands)]

### Batch Operations

[!code-csharp[Batch Navigation](../../code/advanced/PerformanceSamples.cs#BatchNavigation)]

## Navigation Optimization

### Use Appropriate Readiness States

[!code-csharp[Readiness States](../../code/advanced/PerformanceSamples.cs#ReadinessStates)]

### Smart Waiting

[!code-csharp[Smart Waiting](../../code/advanced/PerformanceSamples.cs#SmartWaiting)]

## Script Execution Optimization

### Minimize Script Calls

[!code-csharp[Minimize Script Calls](../../code/advanced/PerformanceSamples.cs#MinimizeScriptCalls)]

### Efficient Element Operations

[!code-csharp[Efficient Element Operations](../../code/advanced/PerformanceSamples.cs#EfficientElementOperations)]

## Event Processing Optimization

### Use Async Event Handlers

[!code-csharp[Async Event Handler](../../code/advanced/PerformanceSamples.cs#AsyncEventHandler)]

### Filter Events Early

[!code-csharp[Filter Events Early](../../code/advanced/PerformanceSamples.cs#FilterEventsEarly)]

### Selective Event Subscription

[!code-csharp[Selective Event Subscription](../../code/advanced/PerformanceSamples.cs#SelectiveEventSubscription)]

## Message Queue and High-Throughput Scenarios

### Understanding the Transport Message Queue

WebDriverBiDi.NET uses an **unbounded message queue** to buffer incoming messages from the browser. This design choice provides flexibility and avoids blocking the connection, but has important implications for high-throughput scenarios.

**Architecture:**
- Messages received from the WebSocket connection are queued immediately
- A dedicated reader task processes messages sequentially
- No limit on queue depth (unbounded)
- Single-reader, single-writer for optimal throughput

**Normal Operation:**
In typical usage, message processing is fast enough that the queue remains nearly empty. Messages arrive, get processed within milliseconds, and the queue clears immediately.

**High-Throughput Risks:**

[!code-csharp[Slow Handler Problem](../../code/advanced/PerformanceSamples.cs#SlowHandlerProblem)]

### Symptoms of Queue Backlog

Monitor for these indicators:

1. **Rising Incoming Queue Depth**: `Transport.IncomingQueueDepth` climbs and does not recover (see [Monitoring and Diagnostics](#monitoring-and-diagnostics))
2. **Rising In-Flight Handler Count**: The `AsyncHandlerTaskCount` EventSource event reports a persistently high value
3. **Increasing Memory Usage**: Process memory grows during high-event periods
4. **Event Lag**: Events processed long after they occurred
5. **Delayed Command Responses**: Commands take longer as queue backs up
6. **OutOfMemoryException**: In extreme cases with thousands of queued messages

### Preventing Queue Backlog

#### 1. Use Asynchronous Event Handlers

[!code-csharp[Async Handler Good](../../code/advanced/PerformanceSamples.cs#AsyncHandlerGood)]

**Impact:**
- Message processing thread continues immediately
- Queue stays near-empty even with slow handlers
- Multiple events can be processed concurrently

#### 2. Keep Event Handlers Fast

[!code-csharp[Offload Heavy Work](../../code/advanced/PerformanceSamples.cs#OffloadHeavyWork)]

#### 3. Reduce Event Subscriptions

[!code-csharp[Scope Subscriptions](../../code/advanced/PerformanceSamples.cs#ScopeSubscriptions)]

#### 4. Implement Event Throttling

[!code-csharp[Event Throttling](../../code/advanced/PerformanceSamples.cs#EventThrottling)]

#### 5. Use Event Sampling

[!code-csharp[Event Sampling](../../code/advanced/PerformanceSamples.cs#EventSampling)]

### Monitoring and Diagnostics

WebDriverBiDi.NET exposes two built-in signals for detecting message-processing backlog, plus process-level memory as a supplementary guardrail.

#### Incoming Queue Depth (Transport.IncomingQueueDepth)

`Transport.IncomingQueueDepth` returns the number of messages that have been received from the connection but not yet picked up by the reader task. Poll it on a timer to catch backlog directly:

[!code-csharp[Queue Depth Monitoring](../../code/advanced/PerformanceSamples.cs#QueueDepthMonitoring)]

The property is safe to read concurrently with message production and consumption. It is reset on each call to `ConnectAsync`, and reading it before the first connect or after disconnect returns `0` rather than throwing.

#### In-Flight Async Handler Tasks (AsyncHandlerTaskCount EventSource event)

When handlers are registered with `ObservableEventHandlerOptions.RunHandlerAsynchronously`, the reader task does not wait for them to complete — so a growing queue is not the only backlog symptom. The second symptom is a growing set of running async handler tasks. The `WebDriverBiDi` EventSource publishes an `AsyncHandlerTaskCount` event (verbose level) each time this count changes; subscribe with an `EventListener`:

[!code-csharp[Async Handler Backlog Monitoring](../../code/advanced/PerformanceSamples.cs#AsyncHandlerBacklogMonitoring)]

This counter is process-global across all `BiDiDriver` instances.

#### Process Memory (Supplementary)

Use process memory as an outer guardrail when the two signals above are not available to your diagnostic pipeline:

[!code-csharp[Memory Monitoring](../../code/advanced/PerformanceSamples.cs#MemoryMonitoring)]

### Practical Guidelines

**For Low-Traffic Applications** (< 100 events/second):
- Default configuration works well
- No special considerations needed
- Synchronous event handlers are acceptable for quick operations

**For Medium-Traffic Applications** (100-1000 events/second):
- Use `RunHandlerAsynchronously` for all handlers doing I/O
- Keep handlers under 10ms for synchronous execution
- Monitor memory usage during peak load

**For High-Traffic Applications** (> 1000 events/second):
- Use `RunHandlerAsynchronously` for ALL event handlers
- Implement event sampling or throttling
- Consider reducing event subscriptions
- Offload processing to background queues
- Monitor memory continuously
- Consider multiple driver instances to distribute load

### Example: High-Throughput Network Monitoring

[!code-csharp[NetworkMonitor Class](../../code/advanced/PerformanceSamples.cs#NetworkMonitorClass)]

## Resource Management

### Connection Pooling

[!code-csharp[DriverPool Class](../../code/advanced/PerformanceSamples.cs#DriverPoolClass)]

[!code-csharp[DriverPool Usage](../../code/advanced/PerformanceSamples.cs#DriverPoolUsage)]

### Memory Management

[!code-csharp[Memory Management Collectors](../../code/advanced/PerformanceSamples.cs#MemoryManagementCollectors)]

### Observer Cleanup

[!code-csharp[ManagedObserver Class](../../code/advanced/PerformanceSamples.cs#ManagedObserverClass)]

[!code-csharp[ManagedObserver Usage](../../code/advanced/PerformanceSamples.cs#ManagedObserverUsage)]

## Network Optimization

### Reduce Network Interception Overhead

[!code-csharp[Scoped Interception](../../code/advanced/PerformanceSamples.cs#ScopedInterception)]

### Block Unnecessary Resources

[!code-csharp[Block Unnecessary Resources](../../code/advanced/PerformanceSamples.cs#BlockUnnecessaryResources)]

## Browser Context Optimization

### Reuse Contexts

[!code-csharp[Reuse Context](../../code/advanced/PerformanceReuseContextSamples.cs#ReuseContext)]

### Use User Contexts for Isolation

[!code-csharp[User Contexts for Isolation](../../code/advanced/PerformanceSamples.cs#UserContextsforIsolation)]

## Measurement and Profiling

### Performance Tracking

[!code-csharp[PerformanceTracker Class](../../code/advanced/PerformanceSamples.cs#PerformanceTrackerClass)]

[!code-csharp[PerformanceTracker Usage](../../code/advanced/PerformanceSamples.cs#PerformanceTrackerUsage)]

### Bottleneck Identification

[!code-csharp[Analyze Page Load](../../code/advanced/PerformanceSamples.cs#AnalyzePageLoad)]

## Caching Strategies

### Cache Repeated Queries

[!code-csharp[CachedContextInfo Class](../../code/advanced/PerformanceSamples.cs#CachedContextInfoClass)]

[!code-csharp[CachedContextInfo Usage](../../code/advanced/PerformanceSamples.cs#CachedContextInfoUsage)]

## Best Practices Summary

1. **Parallelize Independent Operations**: Use `Task.WhenAll` for concurrent execution
2. **Minimize Round Trips**: Batch operations when possible
3. **Use Appropriate Wait States**: Don't wait for more than you need
4. **Filter Events Early**: Process only relevant events
5. **Clean Up Resources**: Remove observers, collectors, and intercepts
6. **Reuse Contexts**: Don't create/destroy contexts unnecessarily
7. **Block Unnecessary Resources**: Speed up loads by blocking images/CSS
8. **Use Async Handlers**: Don't block message processing
9. **Cache Repeated Queries**: Store frequently accessed data
10. **Profile Your Code**: Measure to find actual bottlenecks

## Performance Checklist

- [ ] Commands executed in parallel where possible?
- [ ] Appropriate navigation readiness state used?
- [ ] Event subscriptions limited to necessary events?
- [ ] Event handlers are async for long operations?
- [ ] Resources cleaned up properly?
- [ ] Contexts reused across tests?
- [ ] Network interception scoped appropriately?
- [ ] Data collectors removed when done?
- [ ] Observers unsubscribed when no longer needed?
- [ ] Performance metrics collected and analyzed?

## Next Steps

- [Error Handling](error-handling.md): Handle failures efficiently
- [Architecture](../architecture.md): Understand system design
- [Examples](../examples/common-scenarios.md): See optimized patterns in action

