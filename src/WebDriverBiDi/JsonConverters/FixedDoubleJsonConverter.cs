// <copyright file="FixedDoubleJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A converter to preserve double values format when serializing to JSON.
/// </summary>
public class FixedDoubleJsonConverter : JsonConverter<double>
{
    /// <summary>
    /// Deserializes the JSON string to a double value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>The deserialized double value.</returns>
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDouble();
    }

    /// <summary>
    /// Serializes a double value to a JSON string, preserving decimal places for integer values.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The double value to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    /// <exception cref="JsonException">Thrown when <paramref name="value"/> is NaN or infinite.</exception>
    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        // JSON has no representation for NaN or the infinities; report a clear error rather than
        // letting the writer reject the formatted text with a less descriptive exception.
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new JsonException($"The value {value} cannot be serialized; only finite numbers can be represented in JSON");
        }

        string numberAsString = value.ToString("0.0###########################", CultureInfo.InvariantCulture);
        writer.WriteRawValue(numberAsString);
    }
}
