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
    where T : EventArgs
{
    private readonly Dictionary<string, ObservableEventHandler<T>> observers = new();
    private readonly int maxObserverCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableEvent{T}"/> class.
    /// </summary>
    public ObservableEvent()
        : this(0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableEvent{T}"/> class.
    /// </summary>
    /// <param name="maxObserverCount">The maximum number of handlers that may observe this event.</param>
    public ObservableEvent(int maxObserverCount)
    {
        this.maxObserverCount = maxObserverCount;
    }

    /// <summary>
    /// Gets the maximum number of observers that may observe this event.
    /// A value of zero (0) indicates an unlimited number of observers.
    /// </summary>
    public int MaxObserverCount => this.maxObserverCount;

    /// <summary>
    /// Adds a function to observe the event that takes an argument of type T and returns void.
    /// It will be wrapped in a Task so that it can be awaited.
    /// </summary>
    /// <param name="handler">An action that handles the observed event.</param>
    /// <param name="handlerOptions">
    /// The options for executing the handler. Defaults to ObservableEventHandlerOptions.None,
    /// meaning the handler will attempt to execute synchronously, awaiting the result of execution.
    /// </param>
    /// <returns>An observer for this observable event.</returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown when the user attempts to add more observers than this event allows.
    /// </exception>
    public EventObserver<T> AddObserver(Action<T> handler, ObservableEventHandlerOptions handlerOptions = ObservableEventHandlerOptions.None)
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

        return this.AddObserver(wrappedHandler, handlerOptions);
    }

    /// <summary>
    /// Adds a function to observe the event that takes an argument of type T and returns a Task.
    /// </summary>
    /// <param name="handler">A function returning a Task that handles the observed event.</param>
    /// <param name="handlerOptions">
    /// The options for executing the handler. Defaults to ObservableEventHandlerOptions.None,
    /// meaning the handler will attempt to execute synchronously, awaiting the result of execution.
    /// </param>
    /// <returns>An observer for this observable event.</returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown when the user attempts to add more observers than this event allows.
    /// </exception>
    public EventObserver<T> AddObserver(Func<T, Task> handler, ObservableEventHandlerOptions handlerOptions = ObservableEventHandlerOptions.None)
    {
        if (this.maxObserverCount > 0 && this.observers.Count == this.maxObserverCount)
        {
            throw new WebDriverBiDiException($"This observable event only allows {this.maxObserverCount} handlers.");
        }

        string observerId = Guid.NewGuid().ToString();
        this.observers.Add(observerId, new ObservableEventHandler<T>(handler, handlerOptions));
        return new EventObserver<T>(this, observerId);
    }

    /// <summary>
    /// Removes a handler for this observable event.
    /// </summary>
    /// <param name="observerId">The ID of the handler handling the event.</param>
    public void RemoveObserver(string observerId)
    {
        if (this.observers.ContainsKey(observerId))
        {
            this.observers.Remove(observerId);
        }
    }

    /// <summary>
    /// Asynchronously notifies observers when this observable event occurs.
    /// </summary>
    /// <param name="notifyData">The data of the event.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task NotifyObserversAsync(T notifyData)
    {
        foreach (ObservableEventHandler<T> observer in this.observers.Values)
        {
            if ((observer.Options & ObservableEventHandlerOptions.RunHandlerAsynchronously) == ObservableEventHandlerOptions.RunHandlerAsynchronously)
            {
                _ = Task.Run(() => observer.HandleObservedEvent(notifyData)).ConfigureAwait(false);
            }
            else
            {
                await observer.HandleObservedEvent(notifyData).ConfigureAwait(false);
            }
        }
    }

    private class ObservableEventHandler<TEventArgs>
    {
        private readonly Func<TEventArgs, Task> handler;
        private readonly ObservableEventHandlerOptions handlerOptions;

        public ObservableEventHandler(Func<TEventArgs, Task> handler, ObservableEventHandlerOptions handlerOptions)
        {
            this.handler = handler;
            this.handlerOptions = handlerOptions;
        }

        public Func<TEventArgs, Task> HandleObservedEvent => this.handler;

        public ObservableEventHandlerOptions Options => this.handlerOptions;
    }
}
