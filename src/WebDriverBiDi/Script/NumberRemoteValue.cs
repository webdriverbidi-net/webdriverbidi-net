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
    /// for easy access to the numeric value of this remote value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator long(NumberRemoteValue value) => value.ToLong();

    /// <summary>
    /// Defines an implicit conversion from a NumberRemoteValue to an integer, allowing
    /// for easy access to the numeric value of this remote value.
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
    public int ToInt() => Convert.ToInt32(this.Value);

    /// <summary>
    /// Gets the numeric value of this remote value as a long, converting from double to long as needed.
    /// </summary>
    /// <returns>The numeric value as a long.</returns>
    public long ToLong() => Convert.ToInt64(this.Value);
}
