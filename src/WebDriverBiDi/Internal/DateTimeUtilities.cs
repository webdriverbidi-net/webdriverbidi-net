// <copyright file="DateTimeUtilities.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Internal;

/// <summary>
/// A utility class for System.DateTime values.
/// </summary>
/// <remarks>
/// This class is intentionally marked as internal, as it only contains utility
/// properties and methods used within this library. They should have little use
/// for users outside of this library. Should that change, this class could be
/// moved to another namespace and exposed for external use.
/// </remarks>
internal static class DateTimeUtilities
{
    /// <summary>
    /// Gets a DateTime representing the start of the Unix epoch (1 January 1970 12:00 AM UTC).
    /// </summary>
    /// <remarks>
    /// The DateTime struct introduces this property in .NET 5, but is unavailable for libraries
    /// targeting .NET Standard 2.0. This property is provided to minimize mistakes in calculating
    /// dates that are offsets from the Unix zero date.
    /// </remarks>
    public static DateTime UnixEpoch => new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Gets <see cref="DateTime.MinValue"/> with a <see cref="DateTime.Kind"/> of <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    /// <remarks>
    /// Every in-range date the library produces from protocol data is UTC. A value clamped to the ends of the
    /// range must be UTC as well: <see cref="DateTime.MinValue"/> itself is <see cref="DateTimeKind.Unspecified"/>,
    /// which <see cref="DateTime.ToUniversalTime"/> treats as local time, so a clamped value passed back to the
    /// library would otherwise be shifted by the machine's offset from UTC.
    /// </remarks>
    public static DateTime MinValueUtc => DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

    /// <summary>
    /// Gets <see cref="DateTime.MaxValue"/> with a <see cref="DateTime.Kind"/> of <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    /// <remarks>
    /// See <see cref="MinValueUtc"/> for why a clamped value must be UTC.
    /// </remarks>
    public static DateTime MaxValueUtc => DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

    /// <summary>
    /// Gets the largest number of whole seconds after the Unix epoch that can be represented by a
    /// <see cref="DateTime"/> (the last second of 31 December 9999 UTC).
    /// </summary>
    public static ulong MaxUnixEpochSeconds => 253402300799;

    /// <summary>
    /// Converts a number of seconds since the Unix epoch to a UTC <see cref="DateTime"/>, clamping
    /// values beyond the representable range to <see cref="MaxValueUtc"/> rather than throwing.
    /// The protocol's <c>js-uint</c> type permits values far larger than <see cref="DateTime"/> can hold.
    /// </summary>
    /// <param name="seconds">The number of seconds since the Unix epoch.</param>
    /// <returns>The corresponding UTC <see cref="DateTime"/>, or <see cref="MaxValueUtc"/> if out of range.</returns>
    public static DateTime FromUnixEpochSeconds(ulong seconds)
    {
        return seconds > MaxUnixEpochSeconds ? MaxValueUtc : UnixEpoch.AddSeconds(seconds);
    }

    /// <summary>
    /// Converts a UTC <see cref="DateTime"/> at or after the Unix epoch to the number of whole seconds since the
    /// epoch, truncating any fraction of a second.
    /// </summary>
    /// <param name="utcValue">The UTC value to convert, at or after <see cref="UnixEpoch"/>.</param>
    /// <returns>The number of whole seconds since the Unix epoch.</returns>
    /// <remarks>
    /// Truncating, rather than rounding, means the result never exceeds <see cref="MaxUnixEpochSeconds"/>: the
    /// last representable instant, a fraction of a second before the end of 9999, converts to the last whole
    /// second rather than to one past it, and so converts back with <see cref="FromUnixEpochSeconds"/> without
    /// clamping.
    /// </remarks>
    public static ulong ToUnixEpochSeconds(DateTime utcValue)
    {
        return (ulong)((utcValue - UnixEpoch).Ticks / TimeSpan.TicksPerSecond);
    }

    /// <summary>
    /// Converts a number of milliseconds since the Unix epoch to a UTC <see cref="DateTime"/>, clamping
    /// values beyond the representable range to <see cref="MaxValueUtc"/> rather than throwing.
    /// The protocol's <c>js-uint</c> type permits values far larger than <see cref="DateTime"/> can hold.
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds since the Unix epoch.</param>
    /// <returns>The corresponding UTC <see cref="DateTime"/>, or <see cref="MaxValueUtc"/> if out of range.</returns>
    public static DateTime FromUnixEpochMilliseconds(ulong milliseconds)
    {
        return milliseconds > (MaxUnixEpochSeconds * 1000) + 999 ? MaxValueUtc : UnixEpoch.AddMilliseconds(milliseconds);
    }
}
