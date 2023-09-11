// <copyright file="EnumValueJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Converts an enumerated type to and from JSON strings.
/// </summary>
/// <typeparam name="T">The enum to convert.</typeparam>
public class EnumValueJsonConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    private readonly Dictionary<T, string> enumValuesToStrings = new();
    private readonly Dictionary<string, T> stringToEnumValues = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumValueJsonConverter{T}"/> class.
    /// </summary>
    public EnumValueJsonConverter()
    {
        Type enumType = typeof(T);
        T[] values = (T[])Enum.GetValues(typeof(T));

        foreach (T value in values)
        {
            string valueAsString = value.ToString().ToLowerInvariant();
            MemberInfo member = enumType.GetMember(value.ToString())[0];
            JsonEnumValueAttribute? attribute = member.GetCustomAttribute<JsonEnumValueAttribute>();
            if (attribute is not null)
            {
                valueAsString = attribute.Value;
            }

            this.stringToEnumValues[valueAsString] = value;
            this.enumValuesToStrings[value] = valueAsString;
        }
    }

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
            throw new WebDriverBiDiException("Deserialization error reading enumerated string value");
        }

        // We can rely on the string not being null because we know the
        // token type is explicitly a string type.
        string stringValue = reader.GetString()!;
        if (this.stringToEnumValues.TryGetValue(stringValue, out T enumValue))
        {
            return enumValue;
        }

        throw new WebDriverBiDiException($"Deserialization error: value '{stringValue}' is not valid for enum type {typeof(T)}");
    }

    /// <summary>
    /// Serializes an enum value to a JSON string.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The enum value to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(this.enumValuesToStrings[value]);
    }
}
