// <copyright file="ObservableEvent{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Runtime.ExceptionServices;

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
/// internal lock, which each publishes a new array of the observers in notification order.
/// Notification reads that array without taking the lock and iterates it to completion, so a
/// long-running handler neither blocks registration nor is disturbed by one, and an observer added or
/// removed while an event is being dispatched takes effect from the next event. See
/// <see cref="EventObserver{T}"/> for thread-safety of checkpoint methods on observers.
/// </para>
/// </remarks>
public class ObservableEvent<T>
    where T : WebDriverBiDiEventArgs
{
    private readonly object observerLock = new();
    private readonly Dictionary<string, EventObserver<T>> observers = [];

    // The observers in notification order, rebuilt whenever one is added or removed and published as
    // a whole. It is never mutated in place, so a notification can iterate it without a lock and
    // without copying: the array it read stays intact for the whole dispatch even as registration
    // continues on other threads. Ordering can only change when the set changes, which is why the
    // sort belongs here rather than in the notification path it used to run on every event.
    //
    // Note: Interlocked operations provide necessary memory barriers; volatile keyword not required.
    // The comparand equals the value being exchanged, so a read neither changes the field nor needs
    // a value to write, exactly as the int flags elsewhere in this library are read.
    private EventObserver<T>[] sortedObservers = [];
    private uint observerSequence = 0;

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
    public int CurrentObserverCount => Interlocked.CompareExchange(ref this.sortedObservers, null!, null!).Length;

    /// <summary>
    /// Gets the reporter used to surface observer failures that occur after the handler has
    /// already returned to the caller, or <see langword="null"/> if no reporter is installed.
    /// </summary>
    /// <remarks>
    /// <see cref="EventObserver{T}"/> reads this at the moment it reports a fault, rather than
    /// capturing it when the observer is created, so that a producer which installs its reporter
    /// after an observer has already been added still has that observer's faults reported. A
    /// <see cref="Protocol.Transport"/> constructed around a <see cref="Protocol.Connection"/>
    /// that already carries observers of its events is the case that requires this.
    /// </remarks>
    internal Func<EventObserverErrorInfo, Task>? ObserverErrorReporter { get; private set; }

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
    /// meaning the action runs inline on the thread dispatching the event and notification waits for it to return.
    /// With <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/> the whole action is queued to the
    /// thread pool via <see cref="Task.Run(Action)"/>, so none of it runs on the dispatching thread; use that option
    /// for actions that perform I/O, long-running work, or execute driver commands.
    /// </param>
    /// <param name="description">An optional description for this observer.</param>
    /// <returns>An observer for this observable event.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown when the user attempts to add more observers than this event allows.</exception>
    /// <exception cref="ArgumentNullException">Thrown when a null handler is passed.</exception>
    public EventObserver<T> AddObserver(Action<T> handler, ObservableEventHandlerOptions handlerOptions = ObservableEventHandlerOptions.RunHandlerSynchronously, string description = "")
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler), "Handler cannot be null");
        }

        Func<T, Task> wrappedHandler = handlerOptions == ObservableEventHandlerOptions.RunHandlerAsynchronously
            ? args => Task.Run(() => handler(args))
            : args =>
            {
                handler(args);
                return Task.CompletedTask;
            };

        return this.AddObserver(wrappedHandler, handlerOptions, description);
    }

    /// <summary>
    /// Adds a function to observe the event that takes an argument of type T and returns a Task.
    /// </summary>
    /// <param name="handler">A function returning a Task that handles the observed event.</param>
    /// <param name="handlerOptions">
    /// The options for executing the handler. Defaults to <see cref="ObservableEventHandlerOptions.RunHandlerSynchronously"/>,
    /// meaning notification awaits the returned <see cref="Task"/> before continuing. With
    /// <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/> the returned <see cref="Task"/> is not awaited,
    /// but the handler is still <em>invoked</em> on the thread dispatching the event: in an <c>async</c> lambda everything
    /// up to the first <c>await</c> that does not complete synchronously runs on that thread, and a non-<c>async</c>
    /// handler that does its work before returning a completed task is not offloaded at all. Handlers that perform I/O,
    /// long-running work, or execute driver commands should use this option <em>and</em> be written as <c>async</c>
    /// handlers that <c>await</c> before doing the heavy work (for example <c>await Task.Yield()</c>), or wrap it in
    /// <c>Task.Run</c>. See <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/>.
    /// </param>
    /// <param name="description">An optional description for this observer.</param>
    /// <returns>An observer for this observable event.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown when the user attempts to add more observers than this event allows.</exception>
    /// <exception cref="ArgumentNullException">Thrown when a null handler is passed.</exception>
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
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler), "Handler cannot be null");
        }

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
            if (this.observers.Remove(observerId))
            {
                this.UpdateSortedObserverList();
            }
        }
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        // The published array is never mutated in place, so it can be read without the lock.
        IEnumerable<EventObserver<T>> observerList = Interlocked.CompareExchange(ref this.sortedObservers, null!, null!);
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
        this.ObserverErrorReporter = reporter;
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
        // Read the observers without taking the lock. The array is replaced wholesale when the set
        // changes and never written in place, so the one read here is a stable snapshot for the whole
        // dispatch, and registration on another thread neither blocks nor is blocked by it. It arrives
        // already in notification order, so there is nothing to copy and nothing to sort per event.
        EventObserver<T>[] observersToNotify = Interlocked.CompareExchange(ref this.sortedObservers, null!, null!);

        // Performance optimization: if there are no observers, we have nothing
        // to notify, so we can return early.
        if (observersToNotify.Length == 0)
        {
            return;
        }

        // Performance optimization: if there is one and only one observer, we
        // can notify it directly. Exceptions occurring during the notification
        // should propagate properly.
        if (observersToNotify.Length == 1)
        {
            await observersToNotify[0].NotifyAsync(notifyData).ConfigureAwait(false);
            return;
        }

        List<Exception>? exceptions = null;
        foreach (EventObserver<T> observer in observersToNotify)
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
            // A lone collected exception is rethrown via ExceptionDispatchInfo to preserve
            // the throwing handler's stack trace: rethrowing the caught object directly
            // (throw exceptions[0]) would reset the trace at this site, degrading
            // diagnostics exactly when a second observer is added (the single-observer
            // fast path above preserves the trace through the await machinery).
            //
            // Some code coverage tools do not recognize full coverage when using
            // ExceptionDispatchInfo.Throw() to throw the exception; they see a closing
            // brace as unreachable. To work around this, we can omit the braces for the
            // single-line if statement. However, this runs afoul of the coding style rules
            // for this project, which require braces for all control blocks. Therefore, we
            // disable those style rules for this block only.
#pragma warning disable IDE0011, SA1503
            if (exceptions.Count == 1)
                ExceptionDispatchInfo.Capture(exceptions[0]).Throw();
#pragma warning restore IDE0011, SA1503

            throw new AggregateException("One or more observer handlers threw an exception.", exceptions);
        }
    }

    /// <summary>
    /// Rebuilds and publishes the array of observers in notification order. Must be called while
    /// holding <see cref="observerLock"/>, after the backing dictionary has been changed.
    /// </summary>
    /// <remarks>
    /// A fresh array is built rather than the existing one being edited, so that a notification already
    /// iterating the previous array is unaffected by this change. The interlocked exchange that
    /// publishes it provides the barrier that pairs with the interlocked read in
    /// <see cref="NotifyObserversAsync"/>, so a reader on another thread sees a fully built array.
    /// </remarks>
    private void UpdateSortedObserverList()
    {
        EventObserver<T>[] updated = new EventObserver<T>[this.observers.Count];
        this.observers.Values.CopyTo(updated, 0);

        // Data collectors first, then by the sequence in which observers were added.
        Array.Sort(updated);
        Interlocked.Exchange(ref this.sortedObservers, updated);
    }

    private EventObserver<T> CreateObserver(Func<T, Task> handler, ObservableEventHandlerOptions handlerOptions = ObservableEventHandlerOptions.RunHandlerSynchronously, string description = "", EventObserverPriority priority = EventObserverPriority.NormalObserverPriority)
    {
        lock (this.observerLock)
        {
            if (this.MaxObserverCount > 0 && this.observers.Count == this.MaxObserverCount)
            {
                throw new WebDriverBiDiException($"""This observable event only allows {this.MaxObserverCount} {(this.MaxObserverCount == 1 ? "observer" : "observers")}.""");
            }

            this.observerSequence += 1;
            EventObserver<T> observer = new(this, handler, handlerOptions, description, this.TimeProvider, this.observerSequence, priority);
            this.observers.Add(observer.Id, observer);
            this.UpdateSortedObserverList();
            return observer;
        }
    }
}
