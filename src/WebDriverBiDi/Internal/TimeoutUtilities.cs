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
    /// <returns>
    /// <see langword="true"/> if <paramref name="timeout"/> is <see cref="Timeout.InfiniteTimeSpan"/>,
    /// or is non-negative and no greater than <see cref="MaxTimeout"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsValidTimeout(TimeSpan timeout)
    {
        return timeout == Timeout.InfiniteTimeSpan || (timeout >= TimeSpan.Zero && timeout <= MaxTimeout);
    }

    /// <summary>
    /// Creates the message for an <see cref="ArgumentOutOfRangeException"/> describing an invalid timeout.
    /// </summary>
    /// <param name="description">A description of the timeout, used as the start of the message (for example, "Command timeout").</param>
    /// <returns>The exception message.</returns>
    public static string GetInvalidTimeoutMessage(string description)
    {
        return $"{description} must be a non-negative TimeSpan value no greater than {MaxTimeout} ({(long)MaxTimeout.TotalMilliseconds} milliseconds), or Timeout.InfiniteTimeSpan";
    }
}
