// <copyright file="EventObserver{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Implementation of an observer in the Observer pattern for events.
/// </summary>
/// <typeparam name="T">The type of event arguments containing information about the observable event.</typeparam>
public class EventObserver<T> : IDisposable
    where T : WebDriverBiDiEventArgs
{
    private readonly object checkpointLock = new();
    private readonly string description;
    private readonly Func<T, Task> handler;
    private readonly ObservableEventHandlerOptions handlerOptions;
    private readonly ObservableEvent<T> observableEvent;
    private readonly List<Task> capturedTasks = [];
    private TaskCompletionSource<bool>? checkpointCompletionSource;
    private CountdownEvent synchronizationCounter = new(0);
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventObserver{T}"/> class.
    /// </summary>
    /// <param name="observableEvent">The observable event being observed.</param>
    /// <param name="handler">A function taking type T and returning a Task that is executed every time the event is observed.</param>
    /// <param name="handlerOptions">The options to use when executing the event handler.</param>
    /// <param name="description">The optional description of this observer.</param>
    internal EventObserver(ObservableEvent<T> observableEvent, Func<T, Task> handler, ObservableEventHandlerOptions handlerOptions, string description)
    {
        this.observableEvent = observableEvent;
        this.handler = handler;
        this.handlerOptions = handlerOptions;
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
    /// Gets a value indicating whether a checkpoint is set for this observer.
    /// </summary>
    public bool IsCheckpointSet { get; internal set; } = false;

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
    /// Sets a checkpoint that can be satisfied after the specified number of notifications this observer receives,
    /// capturing the <see cref="Task"/> objects created while waiting for the checkpoint to be fulfilled.
    /// </summary>
    /// <param name="numberOfNotifications">The number of notifications to wait for. If unspecified, defaults to 1.</param>
    /// <exception cref="ArgumentException">Thrown when the number of notifications specified is less than 1.</exception>
    /// <exception cref="WebDriverBiDiException">Thrown when a checkpoint is already established and not yet fulfilled.</exception>
    public void SetCheckpoint(uint numberOfNotifications = 1)
    {
        if (numberOfNotifications < 1)
        {
            throw new ArgumentException("Number of notifications must be greater than 0.", nameof(numberOfNotifications));
        }

        lock (this.checkpointLock)
        {
            if (this.IsCheckpointSet)
            {
                throw new WebDriverBiDiException("This observer already has a checkpoint set. It must be satisfied or unset before setting another.");
            }

            this.synchronizationCounter.Dispose();
            this.synchronizationCounter = new CountdownEvent(Convert.ToInt32(numberOfNotifications));
            this.checkpointCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.IsCheckpointSet = true;
        }
    }

    /// <summary>
    /// Waits for a checkpoint to be satisfied by having this event observer notified the number of
    /// times specified when the checkpoint was set. If the wait is successful, it means only that
    /// this observer was notified to execute the handler, not that the handler has necessarily
    /// completed execution.
    /// </summary>
    /// <param name="timeout">A <see cref="TimeSpan"/> representing the timeout to wait for the checkpoint to be satisfied.</param>
    /// <returns><see langword="true"/> if this observer has been notified the expected number of times before the timeout expires; otherwise, <see langword="false"/>.</returns>
    public bool WaitForCheckpoint(TimeSpan timeout)
    {
        CountdownEvent counter;
        lock (this.checkpointLock)
        {
            if (!this.IsCheckpointSet)
            {
                return true;
            }

            counter = this.synchronizationCounter;
        }

        bool waitSucceeded = counter.Wait(timeout);
        if (waitSucceeded)
        {
            this.UnsetCheckpoint();
        }

        return waitSucceeded;
    }

    /// <summary>
    /// Asynchronously waits for a checkpoint to be satisfied by having this event observer notified the number of
    /// times specified when the checkpoint was set. If the wait is successful, it means only that
    /// this observer was notified to execute the handler, not that the handler has necessarily
    /// completed execution.
    /// </summary>
    /// <param name="timeout">A <see cref="TimeSpan"/> representing the timeout to wait for the checkpoint to be satisfied.</param>
    /// <returns><see langword="true"/> if this observer has been notified the expected number of times before the timeout expires; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> WaitForCheckpointAsync(TimeSpan timeout)
    {
        Task<bool> completionTask;
        lock (this.checkpointLock)
        {
            if (!this.IsCheckpointSet)
            {
                return true;
            }

            // Note use of the null-forgiving operator (!) here, as SetCheckpoint always
            // assigns checkpointCompletionSource before setting IsCheckpointSet to true.
            completionTask = this.checkpointCompletionSource!.Task;
        }

        using CancellationTokenSource timeoutTokenSource = new();
        Task timeoutTask = Task.Delay(timeout, timeoutTokenSource.Token);
        Task completedTask = await Task.WhenAny(completionTask, timeoutTask).ConfigureAwait(false);
        bool checkpointFulfilled = completedTask == completionTask;
        if (checkpointFulfilled)
        {
            timeoutTokenSource.Cancel();
            this.UnsetCheckpoint();
        }

        return checkpointFulfilled;
    }

    /// <summary>
    /// Gets the <see cref="Task"/> objects captured while waiting for the checkpoint to be fulfilled.
    /// Calling this method unsets the checkpoint, and transfers the ownership of the captured
    ///  <see cref="Task"/>s to the calling method.
    /// </summary>
    /// <returns>An array of <see cref="Task"/> objects captured while waiting for the checkpoints to be fulfilled.</returns>
    public Task[] GetCheckpointTasks()
    {
        lock (this.checkpointLock)
        {
            this.ResetCheckpointState();
            Task[] capturedTasks = this.capturedTasks.ToArray();
            this.capturedTasks.Clear();
            return capturedTasks;
        }
    }

    /// <summary>
    /// Unsets an established checkpoint for this observer.
    /// </summary>
    public void UnsetCheckpoint()
    {
        lock (this.checkpointLock)
        {
            this.ResetCheckpointState();
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
    internal async Task Notify(T notifyData)
    {
        Task executingTask = this.handler(notifyData);
        bool isFireAndForget = this.handlerOptions.HasFlag(ObservableEventHandlerOptions.RunHandlerAsynchronously);

        // Capture the faulted state immediately after the handler returns,
        // before any other code runs, to distinguish synchronous failures
        // (e.g., Task.FromException or throwing before the first await)
        // from truly asynchronous ones that complete later.
        bool isSynchronouslyFaulted = false;
        if (!isFireAndForget)
        {
            await executingTask.ConfigureAwait(false);
        }
        else if (executingTask.IsFaulted)
        {
            isSynchronouslyFaulted = true;
        }
        else if (!executingTask.IsCompleted)
        {
            // The handler is still running asynchronously. Attach a continuation
            // to observe any eventual exception, preventing UnobservedTaskException
            // from being raised when the task is garbage-collected.
            _ = executingTask.ContinueWith(
                static t => { _ = t.Exception; },
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        lock (this.checkpointLock)
        {
            if (this.IsCheckpointSet && !this.synchronizationCounter.IsSet)
            {
                this.capturedTasks.Add(executingTask);
                this.synchronizationCounter.Signal();
                if (this.synchronizationCounter.IsSet)
                {
                    // Note use of the null-forgiving operator (!) here, as SetCheckpoint always
                    // assigns checkpointCompletionSource before setting IsCheckpointSet to true.
                    this.checkpointCompletionSource!.TrySetResult(true);
                }
            }
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
                this.Unobserve();
                lock (this.checkpointLock)
                {
                    this.ResetCheckpointState();
                }
            }

            this.isDisposed = true;
        }
    }

    /// <summary>
    /// Resets the checkpoint state, disposing the synchronization counter and
    /// canceling the completion source. Must be called while holding the
    /// <see cref="checkpointLock"/>.
    /// </summary>
    private void ResetCheckpointState()
    {
        this.IsCheckpointSet = false;
        this.synchronizationCounter.Dispose();
        this.checkpointCompletionSource?.TrySetCanceled();
        this.checkpointCompletionSource = null;
    }
}
