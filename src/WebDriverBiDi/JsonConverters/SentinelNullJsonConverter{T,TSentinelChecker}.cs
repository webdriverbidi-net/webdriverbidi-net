// <copyright file="SentinelNullJsonConverter{T,TSentinelChecker}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

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
/// <remarks>
/// A value that is not the sentinel is written with the serializer's own metadata for its type, so a
/// converter declared on the property is not consulted for it: a property can carry only one
/// <see cref="JsonConverterAttribute"/>, and this converter occupies it. Where the value itself needs a
/// converter, use <see cref="SentinelNullJsonConverter{T, TSentinelChecker, TValueConverter}"/>, which writes
/// every value that is not the sentinel through the converter it names.
/// </remarks>
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
    /// Not implemented. This converter is write-only; it is applied to outbound command
    /// parameter properties and is never used to deserialize incoming JSON.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializerOptions used for deserializing the JSON.</param>
    /// <returns>Never returns; always throws.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("SentinelNullJsonConverter does not support deserialization; it is applied only to outbound command parameter properties.");
    }

    /// <summary>
    /// Conditionally serializes an object value to a JSON string, including null if the value meets criteria for the conversion.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The value to be serialized.</param>
    /// <param name="options">The JsonSerializerOptions used for serializing the object.</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is not null)
        {
            if (this.sentinelChecker.IsSentinelValue(value))
            {
                writer.WriteNullValue();
            }
            else
            {
                this.WriteValue(writer, value, options);
            }
        }
    }

    /// <summary>
    /// Serializes a value that is not the sentinel.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The value to be serialized. It is never <see langword="null"/> and never the sentinel.</param>
    /// <param name="options">The JsonSerializerOptions used for serializing the object.</param>
    /// <remarks>
    /// The default implementation writes the value with the serializer's metadata for its runtime type.
    /// </remarks>
    protected virtual void WriteValue(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // Use the JsonSerializer.Serialize() overload that takes a JsonTypeInfo
        // to remove warnings when publishing AOT compiled applications. The caller
        // has already established that the value is not null, so the null-forgiving
        // operator is appropriate.
        JsonTypeInfo typeInfo = options.GetTypeInfo(value!.GetType());
        JsonSerializer.Serialize(writer, value, typeInfo);
    }
}
