// <copyright file="BigIntegerRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Numerics;
using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Represents a remote value for a BigInteger, providing type-safe access to the
/// value and the ability to convert to a local value for use as an argument for
/// script execution on the remote end..
/// </summary>
[JsonConverter(typeof(RemoteValueJsonConverter))]
public record BigIntegerRemoteValue : ValueHoldingRemoteValue<BigInteger>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BigIntegerRemoteValue"/> class.
    /// </summary>
    /// <param name="value">The BigInteger value.</param>
    internal BigIntegerRemoteValue(BigInteger value)
        : base(RemoteValueType.BigInt)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets or sets the BigInteger value of this remote value.
    /// </summary>
    public override BigInteger Value { get; protected set; }

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the bigint value.</returns>
    public override LocalValue ToLocalValue() => LocalValue.BigInt(this.Value);
}
