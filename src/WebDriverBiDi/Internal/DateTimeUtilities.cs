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
    /// Gets the largest number of whole seconds after the Unix epoch that can be represented by a
    /// <see cref="DateTime"/> (the last second of 31 December 9999 UTC).
    /// </summary>
    public static ulong MaxUnixEpochSeconds => 253402300799;

    /// <summary>
    /// Converts a number of seconds since the Unix epoch to a UTC <see cref="DateTime"/>, clamping
    /// values beyond the representable range to <see cref="DateTime.MaxValue"/> rather than throwing.
    /// The protocol's <c>js-uint</c> type permits values far larger than <see cref="DateTime"/> can hold.
    /// </summary>
    /// <param name="seconds">The number of seconds since the Unix epoch.</param>
    /// <returns>The corresponding <see cref="DateTime"/>, or <see cref="DateTime.MaxValue"/> if out of range.</returns>
    public static DateTime FromUnixEpochSeconds(ulong seconds)
    {
        return seconds > MaxUnixEpochSeconds ? DateTime.MaxValue : UnixEpoch.AddSeconds(seconds);
    }
}
