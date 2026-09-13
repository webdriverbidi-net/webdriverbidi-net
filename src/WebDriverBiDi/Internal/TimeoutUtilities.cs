// <copyright file="TimeoutUtilities.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Internal;

/// <summary>
/// A utility class for validating timeout values before they are handed to
/// <see cref="Task.Delay(TimeSpan, CancellationToken)"/> or a <see cref="CancellationTokenSource"/>.
/// </summary>
/// <remarks>
/// This class is intentionally marked as internal, as it only contains utility
/// properties and methods used within this library. Validating up front lets the
/// library throw an <see cref="ArgumentOutOfRangeException"/> that names the public
/// parameter, rather than letting the runtime throw later (at first use) with a
/// message that refers to a parameter the caller never saw.
/// </remarks>
internal static class TimeoutUtilities
{
    /// <summary>
    /// Gets the largest finite timeout the runtime's timers support.
    /// </summary>
    /// <remarks>
    /// .NET 6 and later accept up to <c>uint.MaxValue - 1</c> milliseconds (about 49.7 days).
    /// The .NET Standard 2.0 build may run on .NET Framework, whose timers accept at most
    /// <c>int.MaxValue</c> milliseconds (about 24.8 days), so the lower limit is used there.
    /// </remarks>
#if NETSTANDARD2_0
    public static readonly TimeSpan MaxTimeout = TimeSpan.FromMilliseconds(int.MaxValue);
#else
    public static readonly TimeSpan MaxTimeout = TimeSpan.FromMilliseconds(uint.MaxValue - 1);
#endif

    /// <summary>
    /// Gets a value indicating whether the specified timeout is usable with the runtime's timers.
    /// </summary>
    /// <param name="timeout">The timeout to validate.</param>
    /// <param name="allowInfinite">
    /// <see langword="true"/> to accept <see cref="Timeout.InfiniteTimeSpan"/> as a valid "wait
    /// indefinitely" value; <see langword="false"/> to reject it. Callers whose consumption of the
    /// timeout cannot represent an infinite wait (for example a budget computed by subtracting elapsed
    /// time, where the negative <see cref="Timeout.InfiniteTimeSpan"/> would read as an already-expired
    /// budget) should pass <see langword="false"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="timeout"/> is <see cref="Timeout.InfiniteTimeSpan"/>
    /// and <paramref name="allowInfinite"/> is <see langword="true"/>, or if it is non-negative and no
    /// greater than <see cref="MaxTimeout"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsValidTimeout(TimeSpan timeout, bool allowInfinite = true)
    {
        if (allowInfinite && timeout == Timeout.InfiniteTimeSpan)
        {
            return true;
        }

        return timeout >= TimeSpan.Zero && timeout <= MaxTimeout;
    }

    /// <summary>
    /// Creates the message for an <see cref="ArgumentOutOfRangeException"/> describing an invalid timeout.
    /// </summary>
    /// <param name="description">A description of the timeout, used as the start of the message (for example, "Command timeout").</param>
    /// <param name="allowInfinite">
    /// <see langword="true"/> if <see cref="Timeout.InfiniteTimeSpan"/> is a permitted value (the message
    /// then mentions it); <see langword="false"/> if it is not. Must match the value passed to the
    /// corresponding <see cref="IsValidTimeout(TimeSpan, bool)"/> call.
    /// </param>
    /// <returns>The exception message.</returns>
    public static string GetInvalidTimeoutMessage(string description, bool allowInfinite = true)
    {
        string infiniteSuffix = allowInfinite ? ", or Timeout.InfiniteTimeSpan" : string.Empty;
        return $"{description} must be a non-negative TimeSpan value no greater than {MaxTimeout} ({(long)MaxTimeout.TotalMilliseconds} milliseconds){infiniteSuffix}";
    }

    /// <summary>
    /// Creates a task that completes after the given delay, measured by the given
    /// <see cref="TimeProvider"/>, so that a caller supplying a non-system provider (a test using
    /// virtual time, say) controls when the delay elapses.
    /// </summary>
    /// <param name="timeProvider">The provider whose clock measures the delay.</param>
    /// <param name="delay">The delay, or <see cref="Timeout.InfiniteTimeSpan"/> to wait only for cancellation.</param>
    /// <param name="cancellationToken">A token that cancels the delay.</param>
    /// <returns>A task that completes when the delay elapses, or is canceled when the token is.</returns>
    public static Task DelayAsync(TimeProvider timeProvider, TimeSpan delay, CancellationToken cancellationToken)
    {
#if NETSTANDARD2_0
        return timeProvider.Delay(delay, cancellationToken);
#else
        return Task.Delay(delay, timeProvider, cancellationToken);
#endif
    }

    /// <summary>
    /// Creates a task that completes when the given task completes, when the given timeout elapses as measured
    /// by the given <see cref="TimeProvider"/>, or when the given token is canceled, whichever happens first.
    /// </summary>
    /// <param name="task">The task to wait for.</param>
    /// <param name="timeProvider">The provider whose clock measures the timeout.</param>
    /// <param name="timeout">The timeout, or <see cref="Timeout.InfiniteTimeSpan"/> to wait until the task completes or the token is canceled.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>
    /// A task that completes as <paramref name="task"/> does if it finishes first, faults with a
    /// <see cref="TimeoutException"/> if the timeout elapses first, or is canceled if the token is canceled first.
    /// </returns>
    /// <remarks>
    /// Unlike racing <paramref name="task"/> against <see cref="DelayAsync"/> with <see cref="Task.WhenAny(Task[])"/>,
    /// this needs no linked <see cref="CancellationTokenSource"/> to cancel the delay once the task wins, and returns
    /// <paramref name="task"/> itself when it has already completed.
    /// </remarks>
    public static Task WaitAsync(Task task, TimeProvider timeProvider, TimeSpan timeout, CancellationToken cancellationToken)
    {
#if NETSTANDARD2_0
        return TimeProviderTaskExtensions.WaitAsync(task, timeout, timeProvider, cancellationToken);
#else
        return task.WaitAsync(timeout, timeProvider, cancellationToken);
#endif
    }

    /// <summary>
    /// Creates a <see cref="CancellationTokenSource"/> that cancels after the given delay, measured by
    /// the given <see cref="TimeProvider"/>.
    /// </summary>
    /// <param name="timeProvider">The provider whose clock measures the delay.</param>
    /// <param name="delay">The delay, or <see cref="Timeout.InfiniteTimeSpan"/> for a source that never cancels on its own.</param>
    /// <returns>The cancellation token source; the caller disposes it.</returns>
    public static CancellationTokenSource CreateCancellationTokenSource(TimeProvider timeProvider, TimeSpan delay)
    {
#if NETSTANDARD2_0
        return timeProvider.CreateCancellationTokenSource(delay);
#else
        return new CancellationTokenSource(delay, timeProvider);
#endif
    }

    /// <summary>
    /// Gets the portion of a timeout budget that remains after the time already spent against it.
    /// </summary>
    /// <param name="timeout">The total budget, or <see cref="Timeout.InfiniteTimeSpan"/> for an unbounded one.</param>
    /// <param name="elapsed">The time already spent against the budget.</param>
    /// <returns>
    /// The remaining budget, never negative; <see cref="Timeout.InfiniteTimeSpan"/> when the budget is unbounded,
    /// since an unbounded budget cannot be consumed.
    /// </returns>
    public static TimeSpan GetRemainingTimeout(TimeSpan timeout, TimeSpan elapsed)
    {
        if (timeout == Timeout.InfiniteTimeSpan)
        {
            return Timeout.InfiniteTimeSpan;
        }

        // An exhausted budget yields zero, so prevent a negative TimeSpan from being returned.
        return TimeSpan.FromTicks(Math.Max(0L, (timeout - elapsed).Ticks));
    }
}
