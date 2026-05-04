// <copyright file="ObservableEventInvocable{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Producer-side implementation of a subject in the Observer pattern for events, allowing a producer
/// to notify observers.
/// </summary>
/// <typeparam name="T">The type of event arguments containing information about the observable event.</typeparam>
public class ObservableEventInvocable<T> : ObservableEvent<T>
    where T : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableEventInvocable{T}"/> class.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    public ObservableEventInvocable(string eventName)
        : this(eventName, 0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableEventInvocable{T}"/> class.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="maxObserverCount">The maximum number of handlers that may observe this event.</param>
    public ObservableEventInvocable(string eventName, uint maxObserverCount)
        : base(eventName, maxObserverCount)
    {
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
    public new Task NotifyObserversAsync(T notifyData) => base.NotifyObserversAsync(notifyData);
}
