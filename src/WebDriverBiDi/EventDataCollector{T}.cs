// <copyright file="EventDataCollector{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Threading.Channels;

/// <summary>
/// Specialized observer of an event that collects event data each time the event is raised,
/// for on-demand batch inspection or async streaming rather than immediate per-event reaction.
/// </summary>
/// <typeparam name="T">The type of event arguments containing information about the observable event.</typeparam>
/// <remarks>
/// <para>
/// Use <see cref="EventDataCollector{T}"/> when you want to accumulate event data and examine it
/// at a convenient point in your code. Use <see cref="GetCollectedEventData"/> to drain all events
/// that have accumulated since the last call; the internal buffer is cleared on each drain so
/// subsequent calls return only new events. Use <see cref="Events"/> to stream events one at a
/// time via <see langword="await foreach"/>; the sequence ends when the collector is disposed.
/// </para>
/// <para>
/// The collector counts as one observer against the event's
/// <see cref="ObservableEvent{T}.MaxObserverCount"/>. Create it through
/// <see cref="ObservableEvent{T}.AddDataCollector"/> and dispose it (or <c>await using</c> it)
/// when collection is no longer needed to avoid memory leaks.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong>
/// All public members are thread-safe. <see cref="GetCollectedEventData"/> and <see cref="Events"/>
/// may be called concurrently with event notifications. Do not call <see cref="GetCollectedEventData"/>
/// concurrently with <see cref="Events"/>; use one reading approach at a time.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// await using EventDataCollector&lt;BeforeRequestSentEventArgs&gt; collector =
///     driver.Network.OnBeforeRequestSent.AddDataCollector();
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
    private readonly Channel<T> channel;
    private readonly EventObserver<T> observer;
    private readonly Func<T, bool>? filter;
    private int isDisposedFlag;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventDataCollector{T}"/> class.
    /// </summary>
    /// <param name="observableEvent">The <see cref="ObservableEvent{T}"/> on which to collect data.</param>
    /// <param name="filter">An optional function that filters the event data captured by this collector.</param>
    /// <param name="description">The optional description of this observer.</param>
    public EventDataCollector(ObservableEvent<T> observableEvent, Func<T, bool>? filter = null, string description = "")
    {
        this.filter = filter;
        this.channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
        });
        this.observer = observableEvent.AddDataCollectingEventObserver(this.CollectDataAsync);
        this.observer.Description = string.IsNullOrEmpty(description)
            ? $"EventDataCollector<{typeof(T).Name}> (id: {this.observer.Id})"
            : description;
    }

    /// <summary>
    /// Gets an <see cref="IAsyncEnumerable{T}"/> that yields each collected event as it arrives.
    /// The sequence ends when the collector is disposed.
    /// </summary>
    public IAsyncEnumerable<T> Events => this.channel.Reader.ReadAllAsync();

    private bool IsDisposed
    {
        get => Interlocked.CompareExchange(ref this.isDisposedFlag, 0, 0) == 1;
        set => Interlocked.Exchange(ref this.isDisposedFlag, value ? 1 : 0);
    }

    /// <summary>
    /// Returns all event data accumulated since the last call and clears the internal buffer.
    /// </summary>
    /// <returns>
    /// A read-only list of all <typeparamref name="T"/> instances collected since the previous
    /// call (or since the collector was created if this is the first call). Returns an empty
    /// list when no events occurred in that interval.
    /// </returns>
    /// <exception cref="ObjectDisposedException">Thrown when calling this method after the collector is disposed.</exception>
    public IReadOnlyList<T> GetCollectedEventData()
    {
        if (this.IsDisposed)
        {
            throw new ObjectDisposedException(this.GetType().FullName);
        }

        List<T> collected = [];
        while (this.channel.Reader.TryRead(out T? item))
        {
            collected.Add(item);
        }

        return collected.AsReadOnly();
    }

    /// <summary>
    /// Gets the string representation of this event data collector.
    /// </summary>
    /// <returns>The string representation of this event data collector.</returns>
    public override string ToString() => this.observer.ToString();

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
                this.channel.Writer.TryComplete();
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
        if (!this.IsDisposed)
        {
            await this.observer.DisposeAsync().ConfigureAwait(false);
            this.channel.Writer.TryComplete();
        }
    }

    private Task CollectDataAsync(T eventData)
    {
        if (this.filter is null || this.filter(eventData))
        {
            this.channel.Writer.TryWrite(eventData);
        }

        return Task.CompletedTask;
    }
}
