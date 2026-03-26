// <copyright file="BooleanRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a remote value for a boolean, providing type-safe access to the
/// value and the ability to convert to a local value for use as an argument for
/// script execution on the remote end.
/// </summary>
public record BooleanRemoteValue : ValueHoldingRemoteValue<bool>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BooleanRemoteValue"/> class.
    /// </summary>
    [JsonConstructor]
    internal BooleanRemoteValue()
        : base(RemoteValueType.Boolean)
    {
    }

    /// <summary>
    /// Gets a value indicating whether the boolean value of this remote value is true.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonInclude]
    public override bool Value { get; internal set; } = false;

    /// <summary>
    /// Defines an implicit conversion from a BooleanRemoteValue to a bool, allowing
    /// for easy access to the boolean value of this remote value.
    /// </summary>
    /// <param name="value">The boolean remote value.</param>
    public static implicit operator bool(BooleanRemoteValue value) => value.Value;

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the boolean value.</returns>
    public override LocalValue ToLocalValue() => LocalValue.Boolean(this.Value);
}
