// <copyright file="EventObserver{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Implementation of an observer in the Observer pattern for events.
/// </summary>
/// <typeparam name="T">The type of event arguments containing information about the observable event.</typeparam>
public class EventObserver<T>
    where T : WebDriverBiDiEventArgs
{
    private readonly string description;
    private readonly Func<T, Task> handler;
    private readonly ObservableEventHandlerOptions handlerOptions;
    private readonly ObservableEvent<T> observableEvent;
    private readonly List<Task> capturedTasks = [];
    private CountdownEvent synchronizationCounter = new(0);

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
    internal string Id { get; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Stops observing the event.
    /// </summary>
    public void Unobserve()
    {
        this.observableEvent.RemoveObserver(this.Id);
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
            throw new ArgumentException("Number of notifications must be greater than 1.", nameof(numberOfNotifications));
        }

        if (this.IsCheckpointSet)
        {
            throw new WebDriverBiDiException("This observer already has a checkpoint set. It must be satisfied or unset before setting another.");
        }

        this.synchronizationCounter = new CountdownEvent(Convert.ToInt32(numberOfNotifications));
        this.IsCheckpointSet = true;
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
        if (!this.IsCheckpointSet)
        {
            return true;
        }

        bool waitSucceeded = this.synchronizationCounter.Wait(timeout);
        if (waitSucceeded)
        {
            this.UnsetCheckpoint();
        }

        return waitSucceeded;
    }

    /// <summary>
    /// Gets the <see cref="Task"/> objects captured while waiting for the checkpoint to be fulfilled.
    /// Calling this method unsets the checkpoint, and transfers the ownership of the captured
    ///  <see cref="Task"/>s to the calling method.
    /// </summary>
    /// <returns>An array of <see cref="Task"/> objects captured while waiting for the checkpoints to be fulfilled.</returns>
    public Task[] GetCheckpointTasks()
    {
        this.UnsetCheckpoint();
        Task[] capturedTasks = this.capturedTasks.ToArray();
        this.capturedTasks.Clear();
        return capturedTasks;
    }

    /// <summary>
    /// Unset an established checkpoint for this observer.
    /// </summary>
    public void UnsetCheckpoint()
    {
        this.IsCheckpointSet = false;
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
        if (!this.handlerOptions.HasFlag(ObservableEventHandlerOptions.RunHandlerAsynchronously))
        {
            await executingTask.ConfigureAwait(false);
        }

        if (this.IsCheckpointSet && !this.synchronizationCounter.IsSet)
        {
            this.capturedTasks.Add(executingTask);
            this.synchronizationCounter.Signal();
        }
    }
}
