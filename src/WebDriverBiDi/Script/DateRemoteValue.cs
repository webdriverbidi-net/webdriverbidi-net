// <copyright file="DateRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Represents a remote value for a date object from the remote end, providing
/// type-safe access to the value and the ability to convert to a local value
/// for use as an argument for script execution on the remote end.
/// </summary>
public record DateRemoteValue : ValueHoldingRemoteValue<DateTime>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateRemoteValue"/> class.
    /// </summary>
    [JsonConstructor]
    internal DateRemoteValue()
        : base(RemoteValueType.Date)
    {
    }

    /// <summary>
    /// Gets the DateTime value of this remote value.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonInclude]
    public override DateTime Value { get; internal set; } = DateTime.MinValue;

    /// <summary>
    /// Defines an implicit conversion from a DateRemoteValue to a DateTime, allowing
    /// for easy access to the date value of this remote value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator DateTime(DateRemoteValue value) => value.Value;

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the date value.</returns>
    public override LocalValue ToLocalValue() => LocalValue.Date(this.Value);
}
