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
/// Custom JSON serializer for properties that can be both missing and null, but with different semantics for each case.
/// </summary>
/// <typeparam name="T">The type to serialize.</typeparam>
public class ConditionalNullPropertyJsonConverter<T> : JsonConverter<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalNullPropertyJsonConverter{T}"/> class.
    /// </summary>
    public ConditionalNullPropertyJsonConverter()
    {
    }

    /// <summary>
    /// Deserializes the JSON string to an object value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>A value of the specified enum.</returns>
    /// <exception cref="NotImplementedException">Always thrown, as this converter does not support deserialization.</exception>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Conditionally serializes an object value to a JSON string, including null if the value meets criteria for the conversion.
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
                json = JsonSerializer.Serialize(value, value.GetType(), options);
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
