// <copyright file="ObservableEvent{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Implementation of a subject in the Observer pattern for events. It can optionally be limited
/// to a specific number of observers.
/// </summary>
/// <typeparam name="T">The type of event arguments containing information about the observable event.</typeparam>
public class ObservableEvent<T>
    where T : WebDriverBiDiEventArgs
{
    private readonly object observerLock = new();
    private readonly Dictionary<string, EventObserver<T>> observers = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableEvent{T}"/> class.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    public ObservableEvent(string eventName)
        : this(eventName, 0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableEvent{T}"/> class.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="maxObserverCount">The maximum number of handlers that may observe this event.</param>
    public ObservableEvent(string eventName, int maxObserverCount)
    {
        this.EventName = eventName;
        this.MaxObserverCount = maxObserverCount;
    }

    /// <summary>
    /// Gets the name of this observable event.
    /// </summary>
    public string EventName { get; }

    /// <summary>
    /// Gets the maximum number of observers that may observe this event.
    /// A value of zero (0) indicates an unlimited number of observers.
    /// </summary>
    public int MaxObserverCount { get; }

    /// <summary>
    /// Gets the current number of observers that are observing this event.
    /// </summary>
    public int CurrentObserverCount
    {
        get
        {
            lock (this.observerLock)
            {
                return this.observers.Count;
            }
        }
    }

    /// <summary>
    /// Adds a function to observe the event that takes an argument of type T and returns void.
    /// It will be wrapped in a Task so that it can be awaited.
    /// </summary>
    /// <param name="handler">An action that handles the observed event.</param>
    /// <param name="handlerOptions">
    /// The options for executing the handler. Defaults to ObservableEventHandlerOptions.None,
    /// meaning the handler will attempt to execute synchronously, awaiting the result of execution.
    /// Handlers that perform I/O tasks, long-running operations, or execute driver commands during
    /// the event handling should be added with the ObservableEventHandlerOptions.RunHandlerAsynchronously
    /// option.
    /// </param>
    /// <param name="description">An optional description for this observer.</param>
    /// <returns>An observer for this observable event.</returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown when the user attempts to add more observers than this event allows.
    /// </exception>
    public EventObserver<T> AddObserver(Action<T> handler, ObservableEventHandlerOptions handlerOptions = ObservableEventHandlerOptions.None, string description = "")
    {
        Func<T, Task> wrappedHandler = (T args) =>
        {
            TaskCompletionSource<bool> taskCompletionSource = new();
            handler(args);
            taskCompletionSource.SetResult(true);
            return taskCompletionSource.Task;
        };

        return this.AddObserver(wrappedHandler, handlerOptions, description);
    }

    /// <summary>
    /// Adds a function to observe the event that takes an argument of type T and returns a Task.
    /// </summary>
    /// <param name="handler">A function returning a Task that handles the observed event.</param>
    /// <param name="handlerOptions">
    /// The options for executing the handler. Defaults to ObservableEventHandlerOptions.None,
    /// meaning the handler will attempt to execute synchronously, awaiting the result of execution.
    /// Handlers that perform I/O tasks, long-running operations, or execute driver commands during
    /// the event handling should be added with the ObservableEventHandlerOptions.RunHandlerAsynchronously
    /// option.
    /// </param>
    /// <param name="description">An optional description for this observer.</param>
    /// <returns>An observer for this observable event.</returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown when the user attempts to add more observers than this event allows.
    /// </exception>
    public EventObserver<T> AddObserver(Func<T, Task> handler, ObservableEventHandlerOptions handlerOptions = ObservableEventHandlerOptions.None, string description = "")
    {
        lock (this.observerLock)
        {
            if (this.MaxObserverCount > 0 && this.observers.Count == this.MaxObserverCount)
            {
                throw new WebDriverBiDiException($"""This observable event only allows {this.MaxObserverCount} {(this.MaxObserverCount == 1 ? "handler" : "handlers")}.""");
            }

            EventObserver<T> observer = new(this, handler, handlerOptions, description);
            this.observers.Add(observer.Id, observer);
            return observer;
        }
    }

    /// <summary>
    /// Removes a handler for this observable event.
    /// </summary>
    /// <param name="observerId">The ID of the handler handling the event.</param>
    public void RemoveObserver(string observerId)
    {
        lock (this.observerLock)
        {
            this.observers.Remove(observerId);
        }
    }

    /// <summary>
    /// Asynchronously notifies observers when this observable event occurs. Each observer is
    /// notified independently; an exception thrown by one observer does not prevent subsequent
    /// observers from being notified. If exactly one observer throws, the original exception is
    /// rethrown. If multiple observers throw, an <see cref="AggregateException"/> containing all
    /// caught exceptions is thrown after all observers have been notified.
    /// </summary>
    /// <param name="notifyData">The data of the event.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="AggregateException">Thrown when multiple observer handlers throw an exception.</exception>
    public async Task NotifyObserversAsync(T notifyData)
    {
        // Snapshot the observers under the lock so that iteration is safe
        // even if observers are added or removed concurrently. The lock is
        // released before invoking any handlers, so long-running handlers
        // do not block observer registration. The copy is cheap because
        // observer counts are typically very small (1–15 references).
        EventObserver<T>[] snapshot;
        lock (this.observerLock)
        {
            snapshot = [.. this.observers.Values];
        }

        List<Exception>? exceptions = null;
        foreach (EventObserver<T> observer in snapshot)
        {
            try
            {
                await observer.Notify(notifyData);
            }
            catch (Exception ex)
            {
                exceptions ??= [];
                exceptions.Add(ex);
            }
        }

        if (exceptions is not null)
        {
            if (exceptions.Count == 1)
            {
                throw exceptions[0];
            }

            throw new AggregateException("One or more observer handlers threw an exception.", exceptions);
        }
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        // Make a copy of the observers under the lock to avoid holding the
        // lock while building the string.
        List<EventObserver<T>> observerList;
        lock (this.observerLock)
        {
            observerList = [.. this.observers.Values];
        }

        return $"ObservableEvent<{typeof(T).Name}> with observers:\n    {string.Join("\n    ", observerList)}";
    }
}
