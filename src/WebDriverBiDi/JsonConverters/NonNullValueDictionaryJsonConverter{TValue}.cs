// <copyright file="NonNullValueDictionaryJsonConverter{TValue}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WebDriverBiDi.Internal;

/// <summary>
/// Converts a JSON object to and from a <see cref="Dictionary{TKey, TValue}"/> keyed by
/// <see cref="string"/>, rejecting a JSON <see langword="null"/> value rather than admitting a null
/// reference into the dictionary.
/// </summary>
/// <typeparam name="TValue">The type of the values of the dictionary.</typeparam>
/// <remarks>
/// <para>
/// This is the map counterpart of <see cref="NonNullElementListJsonConverter{T}"/>, and exists for the
/// same reason: the specification's map productions (for example <c>attributes: {*text =&gt; text}</c> on
/// <c>script.NodeProperties</c>) give their values a non-nullable type, but
/// <see cref="JsonSerializerOptions.RespectNullableAnnotations"/> does not extend to the values of a
/// collection. It must not be applied to a member that captures extension data, where a <c>null</c> value
/// is legitimate and is preserved deliberately.
/// </para>
/// <para>
/// Unlike <see cref="NonNullElementListJsonConverter{T}"/>, this converter is inbound-only: every map the
/// specification defines with a non-nullable value type belongs to a payload the library only ever
/// receives, so serialization is not supported, as for the library's other inbound-only converters.
/// </para>
/// </remarks>
public class NonNullValueDictionaryJsonConverter<TValue> : JsonConverter<Dictionary<string, TValue>>
    where TValue : class
{
    /// <summary>
    /// Deserializes a JSON object to a <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializerOptions used for deserializing the JSON.</param>
    /// <returns>The deserialized dictionary.</returns>
    /// <exception cref="JsonException">
    /// Thrown when the JSON value is not an object, or when the value of any property is <c>null</c>.
    /// </exception>
    public override Dictionary<string, TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"JSON value for a dictionary of {typeof(TValue).Name} must be an object, but the token was {reader.TokenType}");
        }

        JsonTypeInfo<TValue> valueTypeInfo = (JsonTypeInfo<TValue>)options.GetTypeInfo(typeof(TValue));
        Dictionary<string, TValue> values = [];
        reader.Read();
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            // The reader is positioned on a property name here, which is never null.
            string propertyName = reader.GetString()!;
            reader.Read();
            if (reader.TokenType == JsonTokenType.Null)
            {
                throw new JsonException($"JSON value of property '{propertyName}' in a dictionary of {typeof(TValue).Name} may not be null; the protocol defines no map whose values may be null");
            }

            // As for the list converter, a non-null token cannot deserialize to null for any type in
            // this library, so the null-forgiving operator is appropriate here, and the value is read
            // through its converter rather than by re-entering the serializer.
            values[propertyName] = JsonConverterUtilities.ReadNestedValue(ref reader, valueTypeInfo, options)!;
            reader.Read();
        }

        return values;
    }

    /// <summary>
    /// Not implemented. This converter is read-only; it is applied only to members of payloads received
    /// from the remote end, which are never serialized.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The dictionary to be serialized.</param>
    /// <param name="options">The JsonSerializerOptions used for serializing the object.</param>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public override void Write(Utf8JsonWriter writer, Dictionary<string, TValue> value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("NonNullValueDictionaryJsonConverter does not support serialization; it is applied only to members of received payloads.");
    }
}
