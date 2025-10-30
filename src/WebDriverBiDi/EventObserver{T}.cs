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
    private readonly ObservableEvent<T> observableEvent;
    private readonly string id = Guid.NewGuid().ToString();
    private readonly Func<T, Task> handler;
    private readonly ObservableEventHandlerOptions handlerOptions;
    private readonly string description;

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
            this.description = $"EventObserver<{typeof(T).Name}> (id: {this.id})";
        }
        else
        {
            this.description = description;
        }
    }

    /// <summary>
    /// Gets the internal unique identifier of this observer.
    /// </summary>
    internal string Id => this.id;

    /// <summary>
    /// Stops observing the event.
    /// </summary>
    public void Unobserve()
    {
        this.observableEvent.RemoveObserver(this.id);
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
    }
}
