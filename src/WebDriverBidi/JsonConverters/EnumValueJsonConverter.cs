// <copyright file="EnumValueJsonConverter.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.JsonConverters;

using System;
using System.Reflection;
using Newtonsoft.Json;

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
    /// Reads a JSON string and deserializes it to an object.
    /// </summary>
    /// <param name="reader">The JSON reader to use during deserialization.</param>
    /// <param name="objectType">The type of object to which to deserialize.</param>
    /// <param name="existingValue">The existing value of the object.</param>
    /// <param name="hasExistingValue">A value indicating whether the existing value is null.</param>
    /// <param name="serializer">The JSON serializer to use in deserialization.</param>
    /// <returns>The deserialized object created from JSON.</returns>
    public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.Value is not string stringValue)
        {
            throw new WebDriverBidiException("Deserialization error reading enumerated string value");
        }

        if (this.stringToEnumValues.TryGetValue(stringValue, out T enumValue))
        {
            return enumValue;
        }

        throw new WebDriverBidiException($"Deserialization error: value '{stringValue}' is not valid for enum type {typeof(T)}");
    }

    /// <summary>
    /// Serializes an object and writes it to a JSON string.
    /// </summary>
    /// <param name="writer">The JSON writer to use during serialization.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializer">The JSON serializer to use in serialization.</param>
    public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
    {
        writer.WriteValue(this.enumValuesToStrings[value]);
    }
}