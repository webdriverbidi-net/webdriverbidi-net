// <copyright file="NumberJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A converter to convert number values format when serializing to JSON.
/// </summary>
public class NumberJsonConverter : JsonConverter<double>
{
    /// <summary>
    /// Deserializes the JSON string to a BigInteger value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>The deserialized double value.</returns>
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string numberString = reader.GetString()!;
            return this.ReadSpecialNumber(numberString);
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetDouble();
        }
        else
        {
            throw new JsonException($"Unexpected token parsing number. Expected String or Number, got {reader.TokenType}.");
        }
    }

    /// <summary>
    /// Not implemented. This converter is read-only; numeric remote values are never
    /// serialized. Outbound numeric values are serialized via <see cref="WebDriverBiDi.Script.LocalArgumentValue"/>.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The double value to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("NumberJsonConverter does not support serialization; use LocalArgumentValue for outbound numeric values.");
    }

    private double ReadSpecialNumber(string specialValue)
    {
        if (specialValue == "Infinity")
        {
            return double.PositiveInfinity;
        }
        else if (specialValue == "-Infinity")
        {
            return double.NegativeInfinity;
        }
        else if (specialValue == "NaN")
        {
            return double.NaN;
        }
        else if (specialValue == "-0")
        {
            return -0.0;
        }

        throw new JsonException($"Invalid value '{specialValue}' for 'value' property of number");
    }
}
