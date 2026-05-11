// <copyright file="ObservableEvent{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Implementation of a subject in the Observer pattern for events. It can optionally be limited
/// to a specific number of observers.
/// </summary>
/// <typeparam name="T">The type of event arguments containing information about the observable event.</typeparam>
/// <remarks>
/// <para>
/// <strong>Thread Safety:</strong>
/// This class is thread-safe. <see cref="AddObserver(Func{T, Task}, ObservableEventHandlerOptions, string)"/>,
/// <see cref="RemoveObserver"/>, <see cref="NotifyObserversAsync"/>, and <see cref="CurrentObserverCount"/> may be called
/// concurrently from multiple threads. Observer registration and removal are serialized via an
/// internal lock. Notification takes a snapshot of observers under the lock before invoking
/// handlers, so long-running handlers do not block registration. See <see cref="EventObserver{T}"/>
/// for thread-safety of checkpoint methods on observers.
/// </para>
/// </remarks>
public class ObservableEvent<T>
    where T : WebDriverBiDiEventArgs
{
    private readonly object observerLock = new();
    private readonly Dictionary<string, EventObserver<T>> observers = [];
    private Func<EventObserverErrorInfo, Task>? observerErrorReporter;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableEvent{T}"/> class.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="maxObserverCount">The maximum number of observers that may observe this event.</param>
    protected ObservableEvent(string eventName, uint maxObserverCount)
    {
        this.EventName = eventName;
        this.MaxObserverCount = maxObserverCount;
    }

    /// <summary>
    /// Gets the name of this observable event.
    /// </summary>
    public string EventName { get; }

    /// <summary>
    /// Gets the maximum number of observers, including data collectors, that may observe this event.
    /// A value of zero (0) indicates an unlimited number of observers.
    /// </summary>
    public uint MaxObserverCount { get; }

    /// <summary>
    /// Gets the current number of observers, including data collectors, that are observing this event.
    /// </summary>
    public int CurrentObserverCount
    {
        get
        {
            lock (this.observerLock)
            {
                return this.observers.Count;
            }
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="TimeProvider"/> used for time comparisons in observers.
    /// </summary>
    protected TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    /// <summary>
    /// Adds a function to observe the event that takes an argument of type T and returns void.
    /// It will be wrapped in a Task so that it can be awaited.
    /// </summary>
    /// <param name="handler">An action that handles the observed event.</param>
    /// <param name="handlerOptions">
    /// The options for executing the handler. Defaults to <see cref="ObservableEventHandlerOptions.RunHandlerSynchronously"/>,
    /// meaning the handler will attempt to execute synchronously, awaiting the result of execution.
    /// Handlers that perform I/O tasks, long-running operations, or execute driver commands during
    /// the event handling should be added with the <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/>.
    /// option.
    /// </param>
    /// <param name="description">An optional description for this observer.</param>
    /// <returns>An observer for this observable event.</returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown when the user attempts to add more observers than this event allows.
    /// </exception>
    public EventObserver<T> AddObserver(Action<T> handler, ObservableEventHandlerOptions handlerOptions = ObservableEventHandlerOptions.RunHandlerSynchronously, string description = "")
    {
        Task WrappedHandler(T args)
        {
            handler(args);
            return Task.CompletedTask;
        }

        return this.AddObserver(WrappedHandler, handlerOptions, description);
    }

    /// <summary>
    /// Adds a function to observe the event that takes an argument of type T and returns a Task.
    /// </summary>
    /// <param name="handler">A function returning a Task that handles the observed event.</param>
    /// <param name="handlerOptions">
    /// The options for executing the handler. Defaults to <see cref="ObservableEventHandlerOptions.RunHandlerSynchronously"/>,
    /// meaning the handler will attempt to execute synchronously, awaiting the result of execution.
    /// Handlers that perform I/O tasks, long-running operations, or execute driver commands during
    /// the event handling should be added with the <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/>.
    /// option.
    /// </param>
    /// <param name="description">An optional description for this observer.</param>
    /// <returns>An observer for this observable event.</returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown when the user attempts to add more observers than this event allows.
    /// </exception>
    /// <example>
    /// <code>
    /// // Synchronous handler (default) - for quick in-memory work
    /// driver.Log.OnEntryAdded.AddObserver(e => Console.WriteLine(e.Text));
    ///
    /// // Async handler with RunHandlerAsynchronously - for I/O or long-running work
    /// driver.Network.OnBeforeRequestSent.AddObserver(
    ///     async (e) => await SaveRequestToFileAsync(e.Request),
    ///     ObservableEventHandlerOptions.RunHandlerAsynchronously);
    /// </code>
    /// </example>
    public EventObserver<T> AddObserver(Func<T, Task> handler, ObservableEventHandlerOptions handlerOptions = ObservableEventHandlerOptions.RunHandlerSynchronously, string description = "")
    {
        return this.CreateObserver(handler, handlerOptions, description, EventObserverPriority.NormalObserverPriority);
    }

    /// <summary>
    /// Adds a data collector that accumulates event data for on-demand inspection rather than
    /// reacting to each event as it arrives. Each data collector counts as one observer
    /// against the event's <see cref="MaxObserverCount"/>.
    /// </summary>
    /// <param name="filter">An optional function that filters the data collected by this data collector.</param>
    /// <param name="description">An optional human-readable description for this data collector.</param>
    /// <returns>
    /// An <see cref="EventDataCollector{T}"/> that queues each event raised on this observable.
    /// Call <see cref="EventDataCollector{T}.GetCollectedEventData"/> to drain the queue.
    /// Dispose the collector when collection is no longer needed.
    /// </returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown when the user attempts to add more observers than this event allows.
    /// </exception>
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
    public EventDataCollector<T> AddDataCollector(Func<T, bool>? filter = null, string description = "")
    {
        return new EventDataCollector<T>(this, filter, description);
    }

    /// <summary>
    /// Removes a handler for this observable event.
    /// </summary>
    /// <param name="observerId">The ID of the handler handling the event.</param>
    public void RemoveObserver(string observerId)
    {
        lock (this.observerLock)
        {
            this.observers.Remove(observerId);
        }
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        // Make a copy of the observers under the lock to avoid holding the
        // lock while building the string.
        List<EventObserver<T>> observerList;
        lock (this.observerLock)
        {
            observerList = [.. this.observers.Values];
        }

        return $"ObservableEvent<{typeof(T).Name}> with observers:\n    {string.Join("\n    ", observerList)}";
    }

    /// <summary>
    /// Adds an observer solely for the purpose of collecting event data.
    /// </summary>
    /// <param name="handler">The handler to collect the event data.</param>
    /// <returns>The event observer for collecting the data.</returns>
    internal EventObserver<T> AddDataCollectingEventObserver(Func<T, Task> handler)
    {
        return this.CreateObserver(handler, ObservableEventHandlerOptions.RunHandlerSynchronously, string.Empty, EventObserverPriority.DataCollectorObserverPriority);
    }

    /// <summary>
    /// Sets the internal reporter used to surface observer failures that occur
    /// after the handler has already returned to the caller.
    /// </summary>
    /// <param name="reporter">The reporter callback.</param>
    protected void SetObserverErrorReporter(Func<EventObserverErrorInfo, Task> reporter)
    {
        this.observerErrorReporter = reporter;
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
    protected async Task NotifyObserversAsync(T notifyData)
    {
        // Snapshot the observers under the lock so that iteration is safe
        // even if observers are added or removed concurrently. The lock is
        // released before invoking any handlers, so long-running handlers
        // do not block observer registration. The copy is cheap because
        // observer counts are typically very small (1–15 references).
        EventObserver<T>[] snapshot;
        lock (this.observerLock)
        {
            snapshot = [.. this.observers.Values];
        }

        Array.Sort(snapshot);
        List<Exception>? exceptions = null;
        foreach (EventObserver<T> observer in snapshot)
        {
            try
            {
                await observer.NotifyAsync(notifyData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exceptions ??= [];
                exceptions.Add(ex);
            }
        }

        if (exceptions is not null)
        {
            if (exceptions.Count == 1)
            {
                throw exceptions[0];
            }

            throw new AggregateException("One or more observer handlers threw an exception.", exceptions);
        }
    }

    private EventObserver<T> CreateObserver(Func<T, Task> handler, ObservableEventHandlerOptions handlerOptions = ObservableEventHandlerOptions.RunHandlerSynchronously, string description = "", EventObserverPriority priority = EventObserverPriority.NormalObserverPriority)
    {
        lock (this.observerLock)
        {
            if (this.MaxObserverCount > 0 && this.observers.Count == this.MaxObserverCount)
            {
                throw new WebDriverBiDiException($"""This observable event only allows {this.MaxObserverCount} {(this.MaxObserverCount == 1 ? "observer" : "observers")}.""");
            }

            EventObserver<T> observer = new(this, handler, handlerOptions, description, this.TimeProvider, this.observerErrorReporter, priority);
            this.observers.Add(observer.Id, observer);
            return observer;
        }
    }
}
