// <copyright file="TimeBudget.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi;

/// <summary>
/// The time one operation may take across all its commands and waits, and the token that cancels it.
/// </summary>
internal readonly struct TimeBudget
{
    private readonly TimeProvider timeProvider;
    private readonly long startTimestamp;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeBudget"/> struct, starting now.
    /// </summary>
    /// <param name="duration">The time the operation may take.</param>
    /// <param name="timeProvider">The time provider that measures it.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    public TimeBudget(TimeSpan duration, TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        this.Duration = duration;
        this.timeProvider = timeProvider;
        this.startTimestamp = timeProvider.GetTimestamp();
        this.CancellationToken = cancellationToken;
    }

    private TimeBudget(TimeBudget budget, ActionTrace? trace)
    {
        this = budget;
        this.Trace = trace;
    }

    /// <summary>
    /// Gets the time the operation may take in all.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Gets the token that cancels the operation.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets the trace entry of the action the budget is for, or <see langword="null"/> when no trace is recording.
    /// </summary>
    public ActionTrace? Trace { get; }

    /// <summary>
    /// Gets the time left, which is never negative.
    /// </summary>
    public TimeSpan Remaining
    {
        get
        {
            TimeSpan remaining = this.Duration - this.timeProvider.GetElapsedTime(this.startTimestamp);
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Gets a value indicating whether no time is left.
    /// </summary>
    public bool IsExhausted => this.Remaining == TimeSpan.Zero;

    /// <summary>
    /// Creates a copy of the budget, with the same time left, for an action recorded in a trace.
    /// </summary>
    /// <param name="trace">The action's trace entry.</param>
    /// <returns>The copy.</returns>
    public TimeBudget WithTrace(ActionTrace trace) => new(this, trace);

    /// <summary>
    /// Creates a copy of the budget with all its time left again, from now, for an operation whose start was
    /// delayed by work that is not its own.
    /// </summary>
    /// <returns>The copy.</returns>
    public TimeBudget Restart() => new(new TimeBudget(this.Duration, this.timeProvider, this.CancellationToken), this.Trace);

    /// <summary>
    /// Waits for an interval, or for the rest of the budget if less is left.
    /// </summary>
    /// <param name="interval">The interval.</param>
    /// <returns>A task that completes when the wait ends.</returns>
    public Task DelayAsync(TimeSpan interval)
    {
        TimeSpan remaining = this.Remaining;
        TimeSpan delay = interval < remaining ? interval : remaining;
#if NET8_0_OR_GREATER
        return Task.Delay(delay, this.timeProvider, this.CancellationToken);
#else
        return this.timeProvider.Delay(delay, this.CancellationToken);
#endif
    }

    /// <summary>
    /// Waits for a task to complete within the rest of the budget.
    /// </summary>
    /// <typeparam name="T">The task's result type.</typeparam>
    /// <param name="task">The task.</param>
    /// <param name="awaited">What the task stands for, for a timeout's message, such as "a download to begin".</param>
    /// <returns>The task's result.</returns>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the task does not complete in time.</exception>
    public async Task<T> WaitAsync<T>(Task<T> task, string awaited)
    {
        using CancellationTokenSource delayCancellation = CancellationTokenSource.CreateLinkedTokenSource(this.CancellationToken);
#if NET8_0_OR_GREATER
        Task timedOut = Task.Delay(this.Remaining, this.timeProvider, delayCancellation.Token);
#else
        Task timedOut = this.timeProvider.Delay(this.Remaining, delayCancellation.Token);
#endif
        if (await Task.WhenAny(task, timedOut).ConfigureAwait(false) == task)
        {
            delayCancellation.Cancel();
            return await task.ConfigureAwait(false);
        }

        await timedOut.ConfigureAwait(false);
        throw new WebDriverBiDiTimeoutException($"Timed out after {this.Duration.TotalSeconds} seconds waiting for {awaited}.");
    }
}
