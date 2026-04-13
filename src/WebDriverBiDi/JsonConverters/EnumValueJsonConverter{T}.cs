// <copyright file="EnumValueJsonConverter{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Converts an enumerated type to and from JSON strings.
/// </summary>
/// <typeparam name="T">The enum to convert.</typeparam>
public class EnumValueJsonConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    private static readonly Lazy<StringEnumValueConverter<T>> StringEnumConverter = new();

    /// <summary>
    /// Deserializes the JSON string to an enum value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>A value of the specified enum.</returns>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Deserialization error reading enumerated string value");
        }

        // We can rely on the string not being null because we know the
        // token type is explicitly a string type.
        string stringValue = reader.GetString()!;
        try
        {
            return StringEnumConverter.Value.GetValue(stringValue);
        }
        catch (ArgumentException)
        {
        }

        // There is no match, and no default value for unmatched
        // strings is provided. Throw an exception.
        throw new JsonException($"Deserialization error: value '{stringValue}' is not valid for enum type {typeof(T).Name}");
    }

    /// <summary>
    /// Serializes an enum value to a JSON string.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The enum value to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        try
        {
            string stringValue = StringEnumConverter.Value.GetString(value);
            writer.WriteStringValue(stringValue);
        }
        catch (ArgumentException)
        {
            throw new JsonException($"Serialization error: value {value} is not valid for the enum type {typeof(T).Name}");
        }
    }
}
