// <copyright file="BigIntegerJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A converter to preserve BigInteger values format when serializing to JSON.
/// </summary>
public class BigIntegerJsonConverter : JsonConverter<BigInteger>
{
    /// <summary>
    /// Deserializes the JSON string to a BigInteger value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>The deserialized BigInteger value.</returns>
    public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string bigintString = reader.GetString()!;
        if (!BigInteger.TryParse(bigintString, NumberStyles.Integer, CultureInfo.InvariantCulture, out BigInteger bigintValue))
        {
            throw new JsonException($"Cannot parse invalid value '{bigintString}' for bigint");
        }

        return bigintValue;
    }

    /// <summary>
    /// Not implemented. This converter is read-only; bigint remote values are never
    /// serialized. Outbound bigint values are serialized via <see cref="WebDriverBiDi.Script.LocalArgumentValue"/>.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The BigInteger value to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("BigIntegerJsonConverter does not support serialization; use LocalArgumentValue for outbound bigint values.");
    }
}
