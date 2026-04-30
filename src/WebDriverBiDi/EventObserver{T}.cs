// <copyright file="EventObserver{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Diagnostics;
using System.Threading.Channels;
using WebDriverBiDi.Internal;
using WebDriverBiDi.Protocol;

/// <summary>
/// Implementation of an observer in the Observer pattern for events.
/// </summary>
/// <typeparam name="T">The type of event arguments containing information about the observable event.</typeparam>
/// <remarks>
/// <para>
/// Capture methods (<see cref="StartCapturingTasks"/>, <see cref="StopCapturingTasks"/>,
/// <see cref="WaitForCapturedTasksAsync"/>, <see cref="WaitForCapturedTasksCompleteAsync"/>, and
/// <see cref="GetCapturedTasks"/>) are thread-safe. Only one capture session may be active
/// at a time per observer.
/// </para>
/// <para>
/// <strong>Typical capture flow:</strong> Call <see cref="StartCapturingTasks"/> before triggering
/// the action that produces events, then use <see cref="WaitForCapturedTasksAsync"/>
/// to wait for a specific number of handler tasks, or <see cref="WaitForCapturedTasksCompleteAsync"/> to wait for
/// handler tasks to complete, or <see cref="GetCapturedTasks"/> to collect whatever has arrived so far.
/// When <see cref="WaitForCapturedTasksAsync"/> or <see cref="WaitForCapturedTasksCompleteAsync"/>
/// collects the full requested batch, the capture session ends automatically; <see cref="StopCapturingTasks"/>
/// becomes a no-op and need not be called. Call <see cref="StopCapturingTasks"/> explicitly only when ending
/// the session early (e.g., on timeout or cancellation).
/// </para>
/// <example>
/// <code>
/// EventObserver&lt;NavigationEventArgs&gt; observer = driver.BrowsingContext.OnLoad.AddObserver(
///     e => Console.WriteLine($"Loaded: {e.Url}"));
/// observer.StartCapturingTasks();
/// await driver.BrowsingContext.NavigateAsync(navParams);
/// Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(30));
/// // When tasks.Length == 1, the capture session was automatically ended.
/// if (tasks.Length == 1) { /* page load event received */ }
/// </code>
/// </example>
/// </remarks>
public class EventObserver<T> : IDisposable, IAsyncDisposable
    where T : WebDriverBiDiEventArgs
{
    private readonly object captureLock = new();
    private readonly SemaphoreSlim captureReadSemaphore = new(1, 1);
    private readonly string description;
    private readonly Func<T, Task> handler;
    private readonly ObservableEventHandlerOptions handlerOptions;
    private readonly ObservableEvent<T> observableEvent;
    private readonly Func<EventObserverErrorInfo, Task>? observerErrorReporter;
    private readonly TimeProvider timeProvider;
    private Channel<Task>? capturedTaskQueue;
    private int waitingReaderCount;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventObserver{T}"/> class.
    /// </summary>
    /// <param name="observableEvent">The observable event being observed.</param>
    /// <param name="handler">A function taking type T and returning a Task that is executed every time the event is observed.</param>
    /// <param name="handlerOptions">The options to use when executing the event handler.</param>
    /// <param name="description">The optional description of this observer.</param>
    /// <param name="timeProvider">The <see cref="TimeProvider"/> used to calculate timeouts.</param>
    /// <param name="observerErrorReporter">The callback used to report late observer execution errors, if any.</param>
    internal EventObserver(ObservableEvent<T> observableEvent, Func<T, Task> handler, ObservableEventHandlerOptions handlerOptions, string description, TimeProvider timeProvider, Func<EventObserverErrorInfo, Task>? observerErrorReporter)
    {
        this.observableEvent = observableEvent;
        this.handler = handler;
        this.handlerOptions = handlerOptions;
        this.timeProvider = timeProvider;
        this.observerErrorReporter = observerErrorReporter;
        if (string.IsNullOrEmpty(description))
        {
            this.description = $"EventObserver<{typeof(T).Name}> (id: {this.Id})";
        }
        else
        {
            this.description = description;
        }
    }

    /// <summary>
    /// Gets a value indicating whether a capture session is active on this observer.
    /// </summary>
    public bool IsCapturing
    {
        get
        {
            lock (this.captureLock)
            {
                return this.capturedTaskQueue is not null;
            }
        }
    }

    /// <summary>
    /// Gets the internal unique identifier of this observer.
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Stops observing the event.
    /// </summary>
    public void Unobserve()
    {
        this.observableEvent.RemoveObserver(this.Id);
    }

    /// <summary>
    /// Removes this observer from its observable event and releases all resources.
    /// Equivalent to calling <see cref="Unobserve"/> followed by resource cleanup.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously removes this observer from its observable event and releases all resources.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsyncCore().ConfigureAwait(false);
        this.Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Begins an unbounded capture session on this observer. Every handler invocation
    /// that occurs while the capture is active produces a <see cref="Task"/> that can be
    /// retrieved via <see cref="WaitForCapturedTasksAsync"/>, <see cref="WaitForCapturedTasksCompleteAsync"/>,
    /// or <see cref="GetCapturedTasks"/>. The session ends automatically when
    /// <see cref="WaitForCapturedTasksAsync"/> or <see cref="WaitForCapturedTasksCompleteAsync"/> collects the
    /// full requested batch; call <see cref="StopCapturingTasks"/> to end the session early.
    /// </summary>
    /// <exception cref="WebDriverBiDiException">Thrown when a capture session is already active on this observer.</exception>
    /// <remarks>
    /// <para>
    /// This method is thread-safe. Only one capture session may be active at a time.
    /// </para>
    /// <para>
    /// Ownership of each captured <see cref="Task"/> transfers to the caller when it is read via
    /// <see cref="WaitForCapturedTasksAsync"/> or <see cref="GetCapturedTasks"/>.
    /// The caller is responsible for observing the result of each task, including any exceptions, to
    /// avoid unobserved task exceptions.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// observer.StartCapturingTasks();
    /// await driver.BrowsingContext.NavigateAsync(navParams);
    /// Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
    /// // When tasks.Length == 1, the capture session was automatically ended.
    /// if (tasks.Length == 1) { await Task.WhenAll(tasks); }
    /// </code>
    /// </example>
    public void StartCapturingTasks()
    {
        lock (this.captureLock)
        {
            if (this.capturedTaskQueue is not null)
            {
                throw new WebDriverBiDiException("This observer already has an active capture session. Call StopCapturingTasks before starting a new one.");
            }

            this.capturedTaskQueue = Channel.CreateUnbounded<Task>(new UnboundedChannelOptions()
            {
                SingleReader = true,
                SingleWriter = true,
                AllowSynchronousContinuations = false,
            });
        }
    }

    /// <summary>
    /// Ends the active capture session. Any tasks still in the capture buffer can no longer
    /// be retrieved via <see cref="WaitForCapturedTasksAsync"/> or <see cref="GetCapturedTasks"/> after this call.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is thread-safe and is idempotent: calling it when no capture is active does nothing.
    /// </para>
    /// <para>
    /// If captured tasks that have not yet been retrieved may fault, call <see cref="GetCapturedTasks"/> before
    /// calling this method to transfer ownership to the caller and avoid unobserved task exceptions.
    /// </para>
    /// </remarks>
    public void StopCapturingTasks()
    {
        lock (this.captureLock)
        {
            this.CloseCaptureChannel();
        }
    }

    /// <summary>
    /// Asynchronously waits until the specified number of handler tasks have been captured,
    /// then returns them. Ownership of the returned tasks transfers to the caller.
    /// </summary>
    /// <param name="count">The number of handler tasks to wait for. Must be at least 1.</param>
    /// <param name="timeout">How long to wait before giving up.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the wait.</param>
    /// <returns>
    /// <para>
    /// An array of captured handler <see cref="Task"/> objects. The length of the returned array
    /// indicates whether the wait was fulfilled:
    /// </para>
    /// <list type="bullet">
    /// <item><description>
    /// If the array length equals <paramref name="count"/>, the wait was fulfilled — all expected
    /// handler invocations were captured before the timeout expired. The capture session is
    /// automatically ended; a subsequent <see cref="StopCapturingTasks"/> call is a no-op.
    /// </description></item>
    /// <item><description>
    /// If the array length is less than <paramref name="count"/>, the wait timed out — only the
    /// tasks that arrived before the timeout are returned. The capture session remains active and
    /// subsequent calls will continue collecting tasks.
    /// </description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="count"/> is zero.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no capture session is active.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is cancelled.</exception>
    /// <example>
    /// <code>
    /// observer.StartCapturingTasks();
    /// await TriggerThreeEventsAsync();
    /// Task[] tasks = await observer.WaitForCapturedTasksAsync(3, TimeSpan.FromSeconds(10));
    /// // When tasks.Length == count, the capture session is automatically ended.
    /// if (tasks.Length == 3) { await Task.WhenAll(tasks); }
    /// </code>
    /// </example>
    public async Task<Task[]> WaitForCapturedTasksAsync(uint count, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (count < 1)
        {
            throw new ArgumentException("Count must be greater than 0.", nameof(count));
        }

        Channel<Task> channel;
        lock (this.captureLock)
        {
            if (this.capturedTaskQueue is null)
            {
                throw new InvalidOperationException("No capture session is active. Call StartCapturingTasks before calling WaitForCapturedTasksAsync or WaitForCapturedTasksCompleteAsync.");
            }

            channel = this.capturedTaskQueue;
            this.waitingReaderCount++;
        }

        List<Task> collected = [];

#if NETSTANDARD2_0
        using CancellationTokenSource timeoutCancellationTokenSource = this.timeProvider.CreateCancellationTokenSource(timeout);
#else
        using CancellationTokenSource timeoutCancellationTokenSource = new(timeout, this.timeProvider);
#endif
        using CancellationTokenSource linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationTokenSource.Token);

        try
        {
            await this.captureReadSemaphore.WaitAsync(linkedCancellationTokenSource.Token).ConfigureAwait(false);
            try
            {
                while (collected.Count < count)
                {
                    bool hasData = await channel.Reader.WaitToReadAsync(linkedCancellationTokenSource.Token).ConfigureAwait(false);
                    if (!hasData)
                    {
                        // Channel was completed externally (StopCapturingTasks called while we were
                        // blocked waiting). WaitToReadAsync returns false only when the buffer is
                        // empty AND the writer is completed, so there is nothing left to remove.
                        break;
                    }

                    while (collected.Count < count && channel.Reader.TryRead(out Task? task))
                    {
                        collected.Add(task);
                    }
                }

                // If we collected the requested number of tasks, auto-close the channel so that
                // subsequent handler invocations are not queued into it.
                if (collected.Count == count)
                {
                    lock (this.captureLock)
                    {
                        // Only close if it is still the same channel we started with
                        // (StopCapturingTasks might have already closed it).
                        // Note: We are using ReferenceEquals here, though the equality
                        // operator (`==`) would be semantically equivalent. ReferenceEquals
                        // explicitly tells us we are checking for the same instance.
                        if (ReferenceEquals(this.capturedTaskQueue, channel))
                        {
                            this.CloseCaptureChannel();
                        }
                    }

                    // Remove tasks that were added into the buffer in the window between the last
                    // TryRead and TryComplete. These tasks are not returned to the caller; their
                    // original fault continuation has ShouldReportAsyncFault = false (because they
                    // were captured). We attach a new task continuation so any fault surfaces
                    // through the normal error pipeline instead of being silently swallowed.
                    // Only do this when we are the sole active reader: if another WaitForCapturedTasksAsync
                    // caller is queued behind the semaphore, those tasks are legitimately theirs to consume.
                    if (this.waitingReaderCount == 1)
                    {
                        string eventName = this.observableEvent.EventName;
                        while (channel.Reader.TryRead(out Task? racedTask))
                        {
                            this.AttachFaultContinuation(racedTask, eventName, true);
                        }
                    }
                }
            }
            finally
            {
                this.captureReadSemaphore.Release();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException("Wait cancelled waiting for captured event handler tasks.", cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Timeout — fall through and return however many tasks arrived before the timeout
        }
        finally
        {
            lock (this.captureLock)
            {
                this.waitingReaderCount--;
            }
        }

        return [.. collected];
    }

    /// <summary>
    /// Asynchronously waits until the specified number of handler tasks have been captured and all of them have
    /// completed execution. This method discards the tasks after completion. If you need to inspect the tasks,
    /// use either <see cref="WaitForCapturedTasksAsync"/> or <see cref="GetCapturedTasks"/> instead.
    /// Exceptions from captured handler tasks remain owned by the caller and are propagated by this method
    /// through the returned task rather than being re-surfaced through transport-level event handler error behavior.
    /// </summary>
    /// <param name="count">The number of handler tasks to wait for. Must be at least 1.</param>
    /// <param name="timeout">How long to wait for the handler tasks to be captured and for the handlers to complete
    /// their execution.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the wait.</param>
    /// <returns><see langword="true"/> if the expected number of handler tasks were captured and completed before
    /// the timeout expired; otherwise, <see langword="false"/>. When <see langword="true"/> is returned, the
    /// capture session is automatically ended; a subsequent <see cref="StopCapturingTasks"/> call is a no-op.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="count"/> is zero.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no capture session is active.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is cancelled.</exception>
    /// <example>
    /// <code>
    /// EventObserver&lt;BeforeRequestSentEventArgs&gt; observer = driver.Network.OnBeforeRequestSent.AddObserver(
    ///     async (e) => await ProcessRequestAsync(e),
    ///     ObservableEventHandlerOptions.RunHandlerAsynchronously);
    /// observer.StartCapturingTasks();
    /// await driver.BrowsingContext.NavigateAsync(navParams);
    /// bool occurred = await observer.WaitForCapturedTasksCompleteAsync(3, TimeSpan.FromSeconds(10));
    /// // When occurred is true, the capture session is automatically ended.
    /// if (occurred) { /* all 3 events received and handlers completed */ }
    /// </code>
    /// </example>
    public async Task<bool> WaitForCapturedTasksCompleteAsync(uint count, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        long startTimestamp = this.timeProvider.GetTimestamp();
        Task[] tasksToWait = await this.WaitForCapturedTasksAsync(count, timeout, cancellationToken).ConfigureAwait(false);
        if (tasksToWait.Length == count)
        {
            TimeSpan remainingTime = timeout - this.timeProvider.GetElapsedTime(startTimestamp);
            if (remainingTime <= TimeSpan.Zero)
            {
                return false;
            }

            using CancellationTokenSource linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Task whenAllTask = Task.WhenAll(tasksToWait);
            Task cancellationTask = Task.Delay(remainingTime, linkedCancellationTokenSource.Token);
            Task completedTask = await Task.WhenAny(whenAllTask, cancellationTask).ConfigureAwait(false);
            linkedCancellationTokenSource.Cancel();
            if (completedTask == cancellationTask)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return false;
            }

            // Propagate exceptions in event handlers.
            await whenAllTask.ConfigureAwait(false);
        }

        return tasksToWait.Length == count;
    }

    /// <summary>
    /// Synchronously gets all tasks currently available in the capture buffer and returns them.
    /// Does not wait for additional tasks to arrive. Ownership of the returned tasks transfers to the caller.
    /// </summary>
    /// <returns>All handler tasks available in the capture buffer at the time of the call, or an empty array if no capture session is active.</returns>
    /// <example>
    /// <code>
    /// observer.StartCapturingTasks();
    /// await TriggerEventsAsync();
    /// Task[] tasks = observer.GetCapturedTasks();
    /// await Task.WhenAll(tasks);
    /// observer.StopCapturingTasks();
    /// </code>
    /// </example>
    public Task[] GetCapturedTasks()
    {
        Channel<Task>? channel;
        lock (this.captureLock)
        {
            channel = this.capturedTaskQueue;
        }

        if (channel is null)
        {
            return [];
        }

        this.captureReadSemaphore.Wait();
        try
        {
            List<Task> collected = [];
            while (channel.Reader.TryRead(out Task? task))
            {
                collected.Add(task);
            }

            return [.. collected];
        }
        finally
        {
            this.captureReadSemaphore.Release();
        }
    }

    /// <summary>
    /// Gets the string representation of this event observer.
    /// </summary>
    /// <returns>The string representation of this event observer.</returns>
    public override string ToString()
    {
        return this.description;
    }

    /// <summary>
    /// Notifies this observer that the observed event has occurred.
    /// </summary>
    /// <param name="notifyData">The object containing information about the event.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    internal async Task NotifyAsync(T notifyData)
    {
        Task executingTask = this.handler(notifyData);
        bool isHandlerRunAsynchronously = this.handlerOptions == ObservableEventHandlerOptions.RunHandlerAsynchronously;

        // Capture the faulted state immediately after the handler returns,
        // before any other code runs, to distinguish synchronous failures
        // (e.g., Task.FromException or throwing before the first await)
        // from truly asynchronous ones that complete later.
        bool isSynchronouslyFaulted = false;
        if (!isHandlerRunAsynchronously)
        {
            await executingTask.ConfigureAwait(false);
        }
        else if (executingTask.IsFaulted)
        {
            isSynchronouslyFaulted = true;
        }

        bool isCaptured = this.CaptureTask(executingTask);
        if (isHandlerRunAsynchronously && !executingTask.IsCompleted)
        {
            // Track this still-running handler task so operators can observe backlog
            // via WebDriverBiDiEventSource.AsyncHandlerTaskCount. The increment must
            // precede attaching the decrement continuation: if the task completes
            // between this line and ContinueWith, the continuation still runs (it
            // schedules on already-completed tasks), so the counter remains balanced.
            AsyncHandlerTaskMetrics.IncrementInFlight();
            _ = executingTask.ContinueWith(
                static (_, _) => AsyncHandlerTaskMetrics.DecrementInFlight(),
                state: null,
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

            // The handler is still running asynchronously. Attach a continuation
            // to observe any eventual exception, preventing UnobservedTaskException
            // from being raised when the task is garbage-collected. Faults are
            // only forwarded into a higher-level error pipeline when the invocation
            // is not already owned by checkpoint or capture.
            string reportedEventName = GetObserverErrorEventName(notifyData, this.observableEvent.EventName);
            this.AttachFaultContinuation(executingTask, reportedEventName, !isCaptured);
        }

        if (!isSynchronouslyFaulted)
        {
            return;
        }

        // The handler was already faulted when it returned (synchronous
        // failure); propagate so NotifyObserversAsync can capture it through
        // its normal exception path. Truly asynchronous failures are observed
        // by the continuation above to prevent UnobservedTaskException.
        await executingTask.ConfigureAwait(false);
    }

    /// <summary>
    /// Releases the resources used by this observer.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            if (disposing)
            {
                this.DisposeObserver();
            }

            this.isDisposed = true;
        }
    }

    /// <summary>
    /// Asynchronously releases the managed resources used by this observer.
    /// Override this method in derived classes to add custom async cleanup logic.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    protected virtual ValueTask DisposeAsyncCore()
    {
        if (!this.isDisposed)
        {
            this.DisposeObserver();
        }

        return default;
    }

    private static string GetObserverErrorEventName(T notifyData, string observableEventName)
    {
        if (notifyData is EventReceivedEventArgs eventReceivedEventArgs)
        {
            return eventReceivedEventArgs.EventName;
        }

        return observableEventName;
    }

    private void CloseCaptureChannel()
    {
        this.capturedTaskQueue?.Writer.TryComplete();
        this.capturedTaskQueue = null;
    }

    private void DisposeObserver()
    {
        this.Unobserve();
        lock (this.captureLock)
        {
            this.CloseCaptureChannel();
        }

        this.captureReadSemaphore.Dispose();
    }

    private void AttachFaultContinuation(Task task, string eventName, bool shouldReport)
    {
        _ = task.ContinueWith(
            this.ReportObserverErrorContinuation,
            new AsynchronousFaultState(eventName, shouldReport),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private bool CaptureTask(Task executingTask)
    {
        lock (this.captureLock)
        {
            return this.capturedTaskQueue is not null && this.capturedTaskQueue.Writer.TryWrite(executingTask);
        }
    }

    private void ReportObserverErrorContinuation(Task faultedTask, object? faultStateObject)
    {
        // The state object is always passed to the continuation by the Notify method,
        // so the null-forgiving operator is appropriate here.
        AsynchronousFaultState state = (AsynchronousFaultState)faultStateObject!;

        // This continuation only runs for faulted tasks, and faulted Task.Exception is
        // guaranteed by the Task Parallel Library to be non-null, so the use of the
        // null-forgiving operator is appropriate here.
        AggregateException aggregateException = faultedTask.Exception!;
        Exception exception = aggregateException.InnerExceptions.Count == 1 ? aggregateException.InnerExceptions[0] : aggregateException;
        try
        {
            if (!state.ShouldReportAsyncFault)
            {
                return;
            }

            this.observerErrorReporter?.Invoke(new EventObserverErrorInfo()
            {
                ObservableEventName = state.ReportedEventName,
                ObserverId = this.Id,
                ObserverDescription = this.description,
                Exception = exception,
                IsAsynchronousHandler = true,
                FaultOccurredAfterHandlerReturned = true,
            });
        }
        finally
        {
            _ = faultedTask.Exception;
        }
    }

    /// <summary>
    /// Internal data structure used to pass state into the asynchronous fault reporting continuation,
    /// allowing it to determine whether to report the fault and what event name to report it under.
    /// </summary>
    private class AsynchronousFaultState
    {
        public AsynchronousFaultState(string reportedEventName, bool shouldReportAsyncFault)
        {
            this.ReportedEventName = reportedEventName;
            this.ShouldReportAsyncFault = shouldReportAsyncFault;
        }

        public string ReportedEventName { get; }

        public bool ShouldReportAsyncFault { get; }
    }
}
