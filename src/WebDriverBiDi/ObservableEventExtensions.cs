// <copyright file="ObservableEventExtensions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Extension methods for <see cref="ObservableEvent{T}"/>.
/// </summary>
public static class ObservableEventExtensions
{
    /// <summary>
    /// Returns an <see cref="IObservable{T}"/> adapter for this <see cref="ObservableEvent{T}"/>,
    /// enabling integration with Reactive Extensions (Rx) operators and any code that consumes
    /// the standard <see cref="IObservable{T}"/>/<see cref="IObserver{T}"/> interfaces.
    /// </summary>
    /// <typeparam name="T">The type of event arguments containing information about the observable event.</typeparam>
    /// <param name="source">The <see cref="ObservableEvent{T}"/> to adapt.</param>
    /// <returns>
    /// An <see cref="IObservable{T}"/> whose <see cref="IObservable{T}.Subscribe"/> method creates
    /// an independent channel-backed subscription for each caller. Each call to
    /// <see cref="IObservable{T}.Subscribe"/> counts as one observer against
    /// <see cref="ObservableEvent{T}.MaxObserverCount"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The returned <see cref="IObservable{T}"/> partially satisfies the Rx push-stream contract:
    /// <list type="bullet">
    /// <item><description>
    /// <see cref="IObserver{T}.OnNext"/> is called for each event on a background thread as items
    /// are drained from the subscription's internal channel.
    /// </description></item>
    /// <item><description>
    /// <see cref="IObserver{T}.OnCompleted"/> is called after the subscription handle returned by
    /// <see cref="IObservable{T}.Subscribe"/> is disposed, once all buffered items have been delivered.
    /// </description></item>
    /// <item><description>
    /// <see cref="IObserver{T}.OnError"/> is called if <see cref="IObserver{T}.OnNext"/> throws;
    /// it is not called for exceptions thrown by other observers on the same
    /// <see cref="ObservableEvent{T}"/>.
    /// </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The subscription handle returned by <see cref="IObservable{T}.Subscribe"/> is an
    /// <see cref="EventDataCollector{T}"/>. Disposing it removes the subscription from the
    /// source event, completes the internal channel, and triggers <see cref="IObserver{T}.OnCompleted"/>
    /// once the drain loop exits.
    /// </para>
    /// </remarks>
    public static IObservable<T> ToObservable<T>(this ObservableEvent<T> source)
        where T : WebDriverBiDiEventArgs
    {
        return new ObservableEventAdapter<T>(source);
    }

    /// <summary>
    /// Concrete <see cref="IObservable{T}"/> adapter for an <see cref="ObservableEvent{T}"/>.
    /// Each call to <see cref="Subscribe"/> creates an independent <see cref="EventDataCollector{T}"/>
    /// that receives its own copy of every event and drains it on a background task.
    /// </summary>
    /// <typeparam name="T">The type of event arguments containing information about the observable event.</typeparam>
    internal sealed class ObservableEventAdapter<T> : IObservable<T>
        where T : WebDriverBiDiEventArgs
    {
        private readonly ObservableEvent<T> source;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableEventAdapter{T}"/> class.
        /// </summary>
        /// <param name="source">The <see cref="ObservableEvent{T}"/> to adapt.</param>
        internal ObservableEventAdapter(ObservableEvent<T> source)
        {
            this.source = source;
        }

        /// <summary>
        /// Subscribes an <see cref="IObserver{T}"/> to this observable event.
        /// Creates an <see cref="EventDataCollector{T}"/> that buffers every event raised on the
        /// source and drains it on a background task, calling <see cref="IObserver{T}.OnNext"/>
        /// for each item. <see cref="IObserver{T}.OnCompleted"/> is called after the returned
        /// <see cref="IDisposable"/> is disposed and the drain loop exits.
        /// <see cref="IObserver{T}.OnError"/> is called if <see cref="IObserver{T}.OnNext"/> throws.
        /// </summary>
        /// <param name="observer">The observer to receive notifications.</param>
        /// <returns>
        /// An <see cref="IDisposable"/> (backed by <see cref="EventDataCollector{T}"/>) that, when
        /// disposed, stops event delivery and triggers <see cref="IObserver{T}.OnCompleted"/>.
        /// </returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            EventDataCollector<T> collector = this.source.AddDataCollector();
            _ = Task.Run(async () =>
            {
                try
                {
                    await foreach (T item in collector.Events.ConfigureAwait(false))
                    {
                        observer.OnNext(item);
                    }

                    observer.OnCompleted();
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }
            });
            return collector;
        }
    }
}
