// <copyright file="SentinelNullJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Custom JSON serializer for properties that can be both missing and null, but with
/// different semantics for each case. When the property value equals a type-specific
/// sentinel (as determined by <typeparamref name="TSentinelChecker"/>), the converter
/// writes JSON <c>null</c> instead of the normal serialized form. Pair with
/// <c>[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]</c> so that
/// an absent C# <see langword="null"/> omits the property entirely while the sentinel
/// value emits an explicit JSON <c>null</c>.
/// </summary>
/// <typeparam name="T">The type to serialize.</typeparam>
/// <typeparam name="TSentinelChecker">
/// A <see cref="SentinelValueChecker{T}"/> subclass that decides whether a
/// given value is the sentinel. Must have a parameterless constructor.
/// </typeparam>
public class SentinelNullJsonConverter<T, TSentinelChecker> : JsonConverter<T>
    where TSentinelChecker : SentinelValueChecker<T>, new()
{
    private readonly TSentinelChecker sentinelChecker = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SentinelNullJsonConverter{T, TSentinelChecker}"/> class.
    /// </summary>
    public SentinelNullJsonConverter()
    {
    }

    /// <summary>
    /// Deserializes the JSON string to an object value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>This method always throws; it does not return a value.</returns>
    /// <exception cref="NotImplementedException">Always thrown, as this converter does not support deserialization.</exception>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Conditionally serializes an object value to a JSON string, including null if the value meets criteria for the conversion.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The value to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is not null)
        {
            string json = "null";
            if (!this.sentinelChecker.IsSentinelValue(value))
            {
                json = JsonSerializer.Serialize(value, value.GetType(), options);
            }

            writer.WriteRawValue(json);
            writer.Flush();
        }
    }
}
