// <copyright file="ConditionalNullPropertyJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.BrowsingContext;

/// <summary>
/// Converts an enumerated type to and from JSON strings.
/// </summary>
/// <typeparam name="T">The enum to convert.</typeparam>
public class ConditionalNullPropertyJsonConverter<T> : JsonConverter<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalNullPropertyJsonConverter{T}"/> class.
    /// </summary>
    public ConditionalNullPropertyJsonConverter()
    {
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Serializes an enum value to a JSON string.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The enum value to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is not null)
        {
            string json = "null";
            if (!this.ShouldSerializeToNull(value))
            {
                json = JsonSerializer.Serialize(value, value.GetType());
            }

            writer.WriteRawValue(json);
            writer.Flush();
        }
    }

    private bool ShouldSerializeToNull(T value)
    {
        if (value is Viewport viewport)
        {
            return viewport.IsResetViewport;
        }

        if (value is double doubleValue)
        {
            return doubleValue < 0;
        }

        throw new WebDriverBiDiException($"Converter cannot be used on type {typeof(T)}");
    }
}
