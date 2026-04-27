// <copyright file="EventDispatchBenchmarks.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BenchmarkDotNet.Attributes;

namespace WebDriverBiDi.Benchmarks;

/// <summary>
/// Benchmarks for <see cref="ObservableEvent{T}"/> notification. Notification
/// is on the hot path for every protocol event the browser emits: the
/// transport reads the message, locates the registered event type, and calls
/// <see cref="ObservableEvent{T}.NotifyObserversAsync"/>. This suite varies
/// the observer count and measures dispatch cost, which includes the
/// observer-snapshot lock and the per-observer handler invocation loop.
/// </summary>
[MemoryDiagnoser]
public class EventDispatchBenchmarks
{
    private ObservableEvent<BenchmarkEventArgs> observableEvent = null!;
    private BenchmarkEventArgs eventArgs = null!;
    private List<EventObserver<BenchmarkEventArgs>> observers = null!;

    /// <summary>
    /// Gets or sets the number of observers to register for each benchmark
    /// iteration. Covers a single-observer case (typical production use),
    /// a small fan-out (4, representative of concurrent consumers of a
    /// popular event), and a larger fan-out (16, a stress case).
    /// </summary>
    [Params(1, 4, 16)]
    public int ObserverCount { get; set; }

    /// <summary>
    /// Sets up the observable event and a pre-registered set of no-op
    /// observers. Runs once per parameter value.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this.eventArgs = new BenchmarkEventArgs();
        this.observableEvent = new ObservableEvent<BenchmarkEventArgs>("benchmark.event");
        this.observers = new List<EventObserver<BenchmarkEventArgs>>(this.ObserverCount);
        for (int i = 0; i < this.ObserverCount; i++)
        {
            // Synchronous no-op handler: exercises dispatch but adds no work
            // inside the handler itself, keeping the benchmark focused on
            // the ObservableEvent implementation rather than handler cost.
            this.observers.Add(this.observableEvent.AddObserver(_ => { }));
        }
    }

    /// <summary>
    /// Disposes the registered observers between parameter values.
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
    {
        foreach (EventObserver<BenchmarkEventArgs> observer in this.observers)
        {
            observer.Dispose();
        }
    }

    /// <summary>
    /// Notifies all registered observers of a single event.
    /// </summary>
    [Benchmark]
    public async Task NotifyObservers()
    {
        await this.observableEvent.NotifyObserversAsync(this.eventArgs).ConfigureAwait(false);
    }

    /// <summary>
    /// Minimal event-args subclass for benchmark use. The base record type
    /// carries an <see cref="WebDriverBiDiEventArgs.AdditionalData"/>
    /// property that defaults to an empty dictionary; no additional fields
    /// are needed for a dispatch benchmark.
    /// </summary>
    public sealed record BenchmarkEventArgs : WebDriverBiDiEventArgs;
}
