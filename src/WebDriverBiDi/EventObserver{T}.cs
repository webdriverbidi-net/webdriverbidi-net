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
    private readonly string observerId;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventObserver{T}"/> class.
    /// </summary>
    /// <param name="observableEvent">The observable event being observed.</param>
    /// <param name="observerId">The ID of the handler for the observable event.</param>
    internal EventObserver(ObservableEvent<T> observableEvent, string observerId)
    {
        this.observableEvent = observableEvent;
        this.observerId = observerId;
    }

    /// <summary>
    /// Stops observing the event.
    /// </summary>
    public void Unobserve()
    {
        this.observableEvent.RemoveObserver(this.observerId);
    }
}
