// <copyright file="NumberRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Represents a remote value for a number, providing type-safe access to the
/// value and the ability to convert to a local value for use as an argument
/// for script execution on the remote end.
/// </summary>
public record NumberRemoteValue : ValueHoldingRemoteValue<double>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NumberRemoteValue"/> class.
    /// </summary>
    [JsonConstructor]
    internal NumberRemoteValue()
        : base(RemoteValueType.Number)
    {
    }

    /// <summary>
    /// Gets the numeric value of this remote value.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonInclude]
    [JsonConverter(typeof(NumberJsonConverter))]
    [JsonRequired]
    public override double Value { get; internal set; } = 0;

    /// <summary>
    /// Defines an implicit conversion from a NumberRemoteValue to a double, allowing
    /// for easy access to the numeric value of this remote value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator double(NumberRemoteValue value) => value.Value;

    /// <summary>
    /// Defines an implicit conversion from a NumberRemoteValue to a long, allowing
    /// for easy access to the numeric value of this remote value. See <see cref="ToLong"/>
    /// for the narrowing behavior.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator long(NumberRemoteValue value) => value.ToLong();

    /// <summary>
    /// Defines an implicit conversion from a NumberRemoteValue to an integer, allowing
    /// for easy access to the numeric value of this remote value. See <see cref="ToInt"/>
    /// for the narrowing behavior.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator int(NumberRemoteValue value) => value.ToInt();

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the numeric value.</returns>
    public override LocalValue ToLocalValue() => LocalValue.Number(this.Value);

    /// <summary>
    /// Gets the numeric value of this remote value as an integer, converting from double to integer as needed.
    /// </summary>
    /// <returns>The numeric value as an integer.</returns>
    /// <remarks>
    /// A JavaScript number is a double, so this conversion is narrowing and never throws. The fractional
    /// part is truncated toward zero (for example, <c>2.7</c> becomes <c>2</c> and <c>-2.7</c> becomes
    /// <c>-2</c>). Values that have no <see cref="int"/> representation are mapped as follows: <c>NaN</c>
    /// becomes <c>0</c>, and any value greater than <see cref="int.MaxValue"/> (including <c>Infinity</c>)
    /// or less than <see cref="int.MinValue"/> (including <c>-Infinity</c>) saturates to
    /// <see cref="int.MaxValue"/> or <see cref="int.MinValue"/> respectively. Read
    /// <see cref="ValueHoldingRemoteValue{T}.Value"/> when the exact value must be preserved, including
    /// detecting <c>NaN</c> or an infinity.
    /// </remarks>
    public int ToInt() => (int)this.Value;

    /// <summary>
    /// Gets the numeric value of this remote value as a long, converting from double to long as needed.
    /// </summary>
    /// <returns>The numeric value as a long.</returns>
    /// <remarks>
    /// A JavaScript number is a double, so this conversion is narrowing and never throws. The fractional
    /// part is truncated toward zero (for example, <c>2.7</c> becomes <c>2</c> and <c>-2.7</c> becomes
    /// <c>-2</c>). Values that have no <see cref="long"/> representation are mapped as follows: <c>NaN</c>
    /// becomes <c>0</c>, and any value greater than <see cref="long.MaxValue"/> (including <c>Infinity</c>)
    /// or less than <see cref="long.MinValue"/> (including <c>-Infinity</c>) saturates to
    /// <see cref="long.MaxValue"/> or <see cref="long.MinValue"/> respectively. Read
    /// <see cref="ValueHoldingRemoteValue{T}.Value"/> when the exact value must be preserved, including
    /// detecting <c>NaN</c> or an infinity.
    /// </remarks>
    public long ToLong() => (long)this.Value;
}
