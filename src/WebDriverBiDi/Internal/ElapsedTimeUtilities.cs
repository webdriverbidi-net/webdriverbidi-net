// <copyright file="ElapsedTimeUtilities.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Internal;

using System.Diagnostics;

/// <summary>
/// A utility class for measuring elapsed time from the monotonic counter behind
/// <see cref="Stopwatch"/>, without allocating a <see cref="Stopwatch"/> to hold it.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="Stopwatch"/> is a class, so timing something that happens per command means an
/// allocation per command. Two readings of the counter measure the same interval and cost nothing,
/// which is why the objects this library creates per command hold timestamps rather than stopwatches.
/// </para>
/// <para>
/// This class is intentionally marked as internal, as it only contains utility properties and methods
/// used within this library. They should have little use for users outside of this library.
/// </para>
/// </remarks>
internal static class ElapsedTimeUtilities
{
    /// <summary>
    /// A timestamp value meaning "not recorded", for a field that is filled in later.
    /// </summary>
    /// <remarks>
    /// <see cref="Stopwatch.GetTimestamp"/> returns a counter that never takes this value, so it cannot
    /// be confused with a real reading. Zero would be a poor sentinel for the same reason it is a poor
    /// sentinel anywhere: it is a value the thing being sentineled could legitimately hold.
    /// </remarks>
    public const long TimestampNotSet = long.MinValue;

    /// <summary>
    /// The number of <see cref="TimeSpan"/> ticks in one tick of the underlying counter.
    /// </summary>
    /// <remarks>
    /// Scaling through a double is what <see cref="Stopwatch"/> itself does. Scaling with integer
    /// arithmetic would overflow a <see cref="long"/> for intervals well short of implausible on a
    /// counter running at a gigahertz.
    /// </remarks>
    private static readonly double TicksPerTimestamp = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;

    /// <summary>
    /// Gets the current reading of the monotonic counter.
    /// </summary>
    /// <returns>The current timestamp.</returns>
    public static long GetTimestamp()
    {
        return Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// Gets the time that has elapsed since the given timestamp was taken.
    /// </summary>
    /// <param name="startTimestamp">The timestamp the interval is measured from.</param>
    /// <returns>The elapsed time.</returns>
    public static TimeSpan GetElapsedTime(long startTimestamp)
    {
        return GetElapsedTime(startTimestamp, GetTimestamp());
    }

    /// <summary>
    /// Gets the time that elapsed between two timestamps.
    /// </summary>
    /// <param name="startTimestamp">The timestamp the interval is measured from.</param>
    /// <param name="endTimestamp">The timestamp the interval is measured to.</param>
    /// <returns>The elapsed time.</returns>
    public static TimeSpan GetElapsedTime(long startTimestamp, long endTimestamp)
    {
        return new TimeSpan((long)((endTimestamp - startTimestamp) * TicksPerTimestamp));
    }

    /// <summary>
    /// Gets the whole milliseconds that elapsed between two timestamps.
    /// </summary>
    /// <param name="startTimestamp">The timestamp the interval is measured from.</param>
    /// <param name="endTimestamp">The timestamp the interval is measured to.</param>
    /// <returns>The elapsed whole milliseconds.</returns>
    public static long GetElapsedMilliseconds(long startTimestamp, long endTimestamp)
    {
        return GetElapsedTime(startTimestamp, endTimestamp).Ticks / TimeSpan.TicksPerMillisecond;
    }
}
