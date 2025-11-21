// <copyright file="ObservableEvent{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Implementation of an subject in the Observer pattern for events. It can optionally be limited
/// to a specific number of observers.
/// </summary>
/// <typeparam name="T">The type of event arguments containing information about the observable event.</typeparam>
public class ObservableEvent<T>
    where T : WebDriverBiDiEventArgs
{
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
    public int CurrentObserverCount => this.observers.Count;

    /// <summary>
    /// Adds a function to observe the event that takes an argument of type T and returns void.
    /// It will be wrapped in a Task so that it can be awaited.
    /// </summary>
    /// <param name="handler">An action that handles the observed event.</param>
    /// <param name="handlerOptions">
    /// The options for executing the handler. Defaults to ObservableEventHandlerOptions.None,
    /// meaning the handler will attempt to execute synchronously, awaiting the result of execution.
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
            // Note that if an exception is thrown during the execution of
            // the handler, it will bubble up when NotifyObserversAsync is
            // called, and the Task will be set to Faulted, so no need to
            // add code for that here.
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
    /// </param>
    /// <param name="description">An optional description for this observer.</param>
    /// <returns>An observer for this observable event.</returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown when the user attempts to add more observers than this event allows.
    /// </exception>
    public EventObserver<T> AddObserver(Func<T, Task> handler, ObservableEventHandlerOptions handlerOptions = ObservableEventHandlerOptions.None, string description = "")
    {
        if (this.MaxObserverCount > 0 && this.observers.Count == this.MaxObserverCount)
        {
            throw new WebDriverBiDiException($"""This observable event only allows {this.MaxObserverCount} {(this.MaxObserverCount == 1 ? "handler" : "handlers")}.""");
        }

        EventObserver<T> observer = new(this, handler, handlerOptions, description);
        this.observers.Add(observer.Id, observer);
        return observer;
    }

    /// <summary>
    /// Removes a handler for this observable event.
    /// </summary>
    /// <param name="observerId">The ID of the handler handling the event.</param>
    public void RemoveObserver(string observerId)
    {
        this.observers.Remove(observerId);
    }

    /// <summary>
    /// Asynchronously notifies observers when this observable event occurs.
    /// </summary>
    /// <param name="notifyData">The data of the event.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task NotifyObserversAsync(T notifyData)
    {
        foreach (EventObserver<T> observer in this.observers.Values)
        {
            await observer.Notify(notifyData);
        }
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return $"ObservableEvent<{typeof(T).Name}> with observers:\n    {string.Join("\n    ", this.observers.Values)}";
    }
}
