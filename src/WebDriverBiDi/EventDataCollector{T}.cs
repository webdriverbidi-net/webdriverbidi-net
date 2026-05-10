// <copyright file="EventDataCollector{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Specialized observer of an event that collects event data each time the event is raised,
/// for on-demand inspection rather than immediate per-event reaction.
/// </summary>
/// <typeparam name="T">The type of event arguments containing information about the observable event.</typeparam>
/// <remarks>
/// <para>
/// Use <see cref="EventDataCollector{T}"/> when you want to accumulate event data and examine it at a
/// convenient point in your code, instead of reacting to every individual event as it arrives.
/// Call <see cref="GetCollectedEventData"/> to drain all events that have accumulated since the last
/// call; the internal queue is cleared on each drain so subsequent calls return only new events.
/// </para>
/// <para>
/// The collector counts as one observer against the event's
/// <see cref="ObservableEvent{T}.MaxObserverCount"/>. Create it through
/// <see cref="ObservableEvent{T}.AddDataCollector"/> and dispose it (or <c>await using</c> it) when
/// collection is no longer needed to avoid memory leaks.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong>
/// <see cref="GetCollectedEventData"/> is safe to call concurrently with event notifications.
/// The drain operation is serialized via an internal lock, so no events are lost or double-counted
/// even when the event fires on a different thread.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Accumulate all network requests during a navigation, then inspect them afterwards
/// await using EventDataCollector&lt;BeforeRequestSentEventArgs&gt; collector =
///     driver.Network.OnBeforeRequestSent.AddDataCollector();
///
/// SubscribeCommandParameters subscribe =
///     new SubscribeCommandParameters(driver.Network.OnBeforeRequestSent.EventName);
/// await driver.Session.SubscribeAsync(subscribe);
///
/// await driver.BrowsingContext.NavigateAsync(navParams);
///
/// IReadOnlyList&lt;BeforeRequestSentEventArgs&gt; requests = collector.GetCollectedEventData();
/// Console.WriteLine($"Page made {requests.Count} network requests");
/// </code>
/// </example>
public class EventDataCollector<T> : IDisposable, IAsyncDisposable
    where T : WebDriverBiDiEventArgs
{
    private readonly object dataCollectionLock = new();
    private readonly EventObserver<T> observer;
    private readonly Queue<T> collectedDataQueue = new();
    private readonly Func<T, bool>? filter;
    private int isDisposedFlag = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventDataCollector{T}"/> class.
    /// </summary>
    /// <param name="observableEvent">The <see cref="ObservableEvent{T}"/> on which to collect data.</param>
    /// <param name="filter">An optional function that filters the event data captured by this collector.</param>
    /// <param name="description">The optional description of this observer.</param>
    public EventDataCollector(ObservableEvent<T> observableEvent, Func<T, bool>? filter = null, string description = "")
    {
        this.filter = filter;
        this.observer = observableEvent.AddDataCollectingEventObserver(this.AddCollectedDataAsync);
        this.observer.Description = string.IsNullOrEmpty(description)
            ? $"EventDataCollector<{typeof(T).Name}> (id: {this.observer.Id})"
            : description;
    }

    private bool IsDisposed
    {
        get
        {
            return Interlocked.CompareExchange(ref this.isDisposedFlag, 0, 0) == 1;
        }

        set
        {
            int flagValue = value ? 1 : 0;
            Interlocked.Exchange(ref this.isDisposedFlag, flagValue);
        }
    }

    /// <summary>
    /// Returns all event data accumulated since the last call and clears the internal queue.
    /// </summary>
    /// <returns>
    /// A read-only list of all <typeparamref name="T"/> instances that were collected since the
    /// previous call to this method (or since the collector was created if this is the first call).
    /// Returns an empty list when no events occurred in that interval.
    /// </returns>
    /// <remarks>
    /// Each call drains and resets the queue. Events that arrive after this call returns will be
    /// held until the next call.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when calling this method after the collector is disposed.</exception>
    public IReadOnlyList<T> GetCollectedEventData()
    {
        if (this.IsDisposed)
        {
            throw new ObjectDisposedException(this.GetType().FullName);
        }

        List<T> collectedEventData = [];
        lock (this.dataCollectionLock)
        {
            while (this.collectedDataQueue.Count > 0)
            {
                collectedEventData.Add(this.collectedDataQueue.Dequeue());
            }
        }

        return collectedEventData.AsReadOnly();
    }

    /// <summary>
    /// Gets the string representation of this event observer.
    /// </summary>
    /// <returns>The string representation of this event observer.</returns>
    public override string ToString()
    {
        return this.observer.ToString();
    }

    /// <summary>
    /// Removes this data collector from its observable event and releases all resources.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously removes this data collector from its observable event and releases all resources.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsyncCore().ConfigureAwait(false);
        this.Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the resources used by this data collector.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.IsDisposed)
        {
            if (disposing)
            {
                this.observer.Dispose();
            }

            this.IsDisposed = true;
        }
    }

    /// <summary>
    /// Asynchronously releases the managed resources used by this data collector.
    /// Override this method in derived classes to add custom async cleanup logic.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        await this.observer.DisposeAsync().ConfigureAwait(false);
    }

    private Task AddCollectedDataAsync(T eventData)
    {
        lock (this.dataCollectionLock)
        {
            if (this.filter is null || this.filter(eventData))
            {
                this.collectedDataQueue.Enqueue(eventData);
            }
        }

        return Task.CompletedTask;
    }
}
