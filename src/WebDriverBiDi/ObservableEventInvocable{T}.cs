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

    /// <inheritdoc cref="ObservableEvent{T}.SetObserverErrorReporter(Func{EventObserverErrorInfo, Task})"/>
    public void InvokeSetObserverErrorReporter(Func<EventObserverErrorInfo, Task> reporter) => this.SetObserverErrorReporter(reporter);

    /// <inheritdoc cref="ObservableEvent{T}.NotifyObserversAsync(T)"/>
    public Task InvokeNotifyObserversAsync(T notifyData) => this.NotifyObserversAsync(notifyData);
}
