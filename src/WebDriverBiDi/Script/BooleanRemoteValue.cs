// <copyright file="BooleanRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Represents a remote value for a boolean, providing type-safe access to the
/// value and the ability to convert to a local value for use as an argument for
/// script execution on the remote end.
/// </summary>
[JsonConverter(typeof(RemoteValueJsonConverter))]
public record BooleanRemoteValue : ValueHoldingRemoteValue<bool>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BooleanRemoteValue"/> class.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    internal BooleanRemoteValue(bool value)
        : base(RemoteValueType.Boolean)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the boolean value of this remote value is true.
    /// </summary>
    public override bool Value { get; protected set; }

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the boolean value.</returns>
    public override LocalValue ToLocalValue() => LocalValue.Boolean(this.Value);
}
