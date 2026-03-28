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
public record BigIntegerRemoteValue : ValueHoldingRemoteValue<BigInteger>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BigIntegerRemoteValue"/> class.
    /// </summary>
    [JsonConstructor]
    internal BigIntegerRemoteValue()
        : base(RemoteValueType.BigInt)
    {
    }

    /// <summary>
    /// Gets the BigInteger value of this remote value.
    /// </summary>
    [JsonConverter(typeof(BigIntegerJsonConverter))]
    [JsonPropertyName("value")]
    [JsonInclude]
    [JsonRequired]
    public override BigInteger Value { get; internal set; } = BigInteger.Zero;

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the bigint value.</returns>
    public override LocalValue ToLocalValue() => LocalValue.BigInt(this.Value);
}
