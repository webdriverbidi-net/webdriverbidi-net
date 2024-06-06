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
{
    private readonly List<Func<T, Task>> handlers = new();
    private readonly int maxHandlerCount;

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
    /// <param name="maxHandlerCount">The maximum number of handlers that may observe this event.</param>
    public ObservableEvent(int maxHandlerCount)
    {
        this.maxHandlerCount = maxHandlerCount;
    }

    /// <summary>
    /// Gets the maximum number of handlers that may observe this event.
    /// A value of zero (0) indicates an unlimited number of handlers.
    /// </summary>
    public int MaxHandlerCount => this.maxHandlerCount;

    /// <summary>
    /// Adds a function taking an argument of type T and returning void that handles observable event.
    /// This function will be wrapped in a function that returns a Task.
    /// </summary>
    /// <param name="handler">A function taking an argument of type T returning void that handles the event.</param>
    /// <returns>An observer for this observable event.</returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown when the user attempts to add more observers than this event allows.
    /// </exception>
    public EventObserver<T> AddHandler(Action<T> handler)
    {
        return this.AddHandler((T e) =>
        {
            handler(e);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Adds a function taking an argument of type T and returning a Task that handles observable event.
    /// </summary>
    /// <param name="handler">A function returning a Task that handles the event.</param>
    /// <returns>An observer for this observable event.</returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown when the user attempts to add more observers than this event allows.
    /// </exception>
    public EventObserver<T> AddHandler(Func<T, Task> handler)
    {
        if (this.maxHandlerCount > 0 && this.handlers.Count == this.maxHandlerCount)
        {
            throw new WebDriverBiDiException($"This observable event only allows {this.maxHandlerCount} handlers.");
        }

        this.handlers.Add(handler);
        return new EventObserver<T>(this, handler);
    }

    /// <summary>
    /// Removes a handler for this observable event.
    /// </summary>
    /// <param name="handler">The function handling the event.</param>
    public void RemoveHandler(Func<T, Task> handler)
    {
        this.handlers.Remove(handler);
    }

    /// <summary>
    /// Asynchronously notifies observers when this observable event occurs.
    /// </summary>
    /// <param name="notifyData">The data of the event.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task NotifyObserversAsync(T notifyData)
    {
        foreach (var handler in this.handlers)
        {
            await handler(notifyData);
        }
    }
}