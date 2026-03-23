// <copyright file="LongRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Represents a remote value for a long, providing type-safe access to the
/// value and the ability to convert to a local value for use as an argument
/// for script execution on the remote end.
/// </summary>
[JsonConverter(typeof(RemoteValueJsonConverter))]
public record LongRemoteValue : ValueHoldingRemoteValue<long>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LongRemoteValue"/> class.
    /// </summary>
    /// <param name="value">The long value.</param>
    internal LongRemoteValue(long value)
        : base(RemoteValueType.Number)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets or sets the long value of this remote value.
    /// </summary>
    public override long Value { get; protected set; }

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the long value.</returns>
    public override LocalValue ToLocalValue() => LocalValue.Number(this.Value);
}
