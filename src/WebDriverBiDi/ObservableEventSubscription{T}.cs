// <copyright file="ObservableEventSubscription{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// The handle for a subscription created by calling <see cref="IObservable{T}.Subscribe"/> on the
/// <see cref="IObservable{T}"/> that <see cref="ObservableEventExtensions.ToObservable{T}"/> returns.
/// Disposing the handle ends the subscription; <see cref="CompletionTask"/> reports when delivery to the
/// observer has ended.
/// </summary>
/// <typeparam name="T">The type of event arguments containing information about the observable event.</typeparam>
/// <remarks>
/// <see cref="IObservable{T}.Subscribe"/> is declared to return <see cref="IDisposable"/>, so the
/// handle arrives statically typed as that interface; cast it to this type to reach
/// <see cref="CompletionTask"/>. Each subscription owns an <see cref="EventDataCollector{T}"/> on the
/// source event and a background delivery loop that drains it into the observer.
/// </remarks>
public sealed class ObservableEventSubscription<T> : IDisposable
    where T : WebDriverBiDiEventArgs
{
    private readonly EventDataCollector<T> collector;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableEventSubscription{T}"/> class.
    /// </summary>
    /// <param name="collector">The collector that buffers the source event's items for this subscription.</param>
    /// <param name="completion">The delivery loop's task.</param>
    internal ObservableEventSubscription(EventDataCollector<T> collector, Task completion)
    {
        this.collector = collector;
        this.CompletionTask = completion;
    }

    /// <summary>
    /// Gets a task that completes once delivery to the observer has ended: after
    /// <see cref="IObserver{T}.OnCompleted"/> has returned following <see cref="Dispose"/>, or after
    /// <see cref="IObserver{T}.OnError"/> has returned when <see cref="IObserver{T}.OnNext"/> threw.
    /// </summary>
    /// <remarks>
    /// The task completes successfully in both cases; an exception thrown by
    /// <see cref="IObserver{T}.OnCompleted"/> or <see cref="IObserver{T}.OnError"/> is discarded, because
    /// the Rx grammar makes both calls terminal. Await it after disposing the subscription to be certain
    /// the observer will receive no further calls, for example before tearing down state the observer uses.
    /// </remarks>
    public Task CompletionTask { get; }

    /// <summary>
    /// Removes the subscription from the source event and completes its buffer. Items already buffered
    /// are still delivered, after which <see cref="IObserver{T}.OnCompleted"/> is called and
    /// <see cref="CompletionTask"/> completes. Disposing more than once has no further effect.
    /// </summary>
    public void Dispose()
    {
        this.collector.Dispose();
    }
}
