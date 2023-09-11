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
    /// Deserializes the JSON string to an double value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>A value of the specified double.</returns>
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TryGetDouble(out double doubleValue))
        {
            return doubleValue;
        }

        string? stringValue = reader.GetString();
        return string.IsNullOrWhiteSpace(stringValue) ? default : double.Parse(stringValue, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Serializes an double value to a JSON string, preserving decimal places for integer values.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The enum value to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        string numberAsString = value.ToString("0.0###########################", CultureInfo.InvariantCulture);
        writer.WriteRawValue(numberAsString);
    }
}
