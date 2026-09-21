// <copyright file="NonNullElementListJsonConverter{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WebDriverBiDi.Internal;

/// <summary>
/// Converts a JSON array to and from a <see cref="List{T}"/>, rejecting a JSON <see langword="null"/>
/// element rather than admitting a null reference into the list.
/// </summary>
/// <typeparam name="T">The type of the elements of the list.</typeparam>
/// <remarks>
/// <para>
/// Every array the WebDriver BiDi specification defines is a homogeneous list of values (<c>[*x]</c> or
/// <c>[+x]</c>); no array production admits a <c>null</c> element.
/// <see cref="System.Text.Json.JsonSerializer"/> does not enforce that by itself: nullable annotations
/// are not honored for collection elements even when
/// <see cref="JsonSerializerOptions.RespectNullableAnnotations"/> is set, so a <c>null</c> element is
/// admitted into a list whose element type is non-nullable, and the violation surfaces only later, as a
/// <see cref="NullReferenceException"/> in whichever consumer code first dereferences the element.
/// </para>
/// <para>
/// Applying this converter to such a member turns the protocol violation into a
/// <see cref="JsonException"/> at the point of deserialization, where the transport already contains it:
/// as a <see cref="WebDriverBiDiSerializationException"/> for the command whose response carried the
/// array, or, for an event, as a protocol error governed by
/// <see cref="Protocol.Transport.ProtocolErrorBehavior"/>. Neither path can leave a null element where a
/// value is required.
/// </para>
/// <para>
/// Unlike the library's other inbound-only converters, this one implements serialization rather than
/// throwing, and must continue to. Of the members it is applied to, exactly one is also serialized:
/// <c>ManualProxyConfiguration.SerializableNoProxyAddresses</c>, the shim behind
/// <see cref="Session.ManualProxyConfiguration.NoProxyAddresses"/>, is received as part of a
/// <see cref="Session.CapabilitiesResult"/> and sent as part of
/// <see cref="Session.CapabilityRequest.Proxy"/>, so a converter that refused to write would fail every
/// <c>session.new</c> that configures a manual proxy. Writing is a transparent pass-through, producing
/// exactly what each element would produce without the converter.
/// </para>
/// </remarks>
public class NonNullElementListJsonConverter<T> : JsonConverter<List<T>>
    where T : class
{
    /// <summary>
    /// Deserializes a JSON array to a <see cref="List{T}"/>.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializerOptions used for deserializing the JSON.</param>
    /// <returns>The deserialized list.</returns>
    /// <exception cref="JsonException">
    /// Thrown when the JSON value is not an array, or when any element of the array is <c>null</c>.
    /// </exception>
    public override List<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"JSON value for a list of {typeof(T).Name} must be an array, but the token was {reader.TokenType}");
        }

        JsonTypeInfo<T> elementTypeInfo = (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T));
        List<T> elements = [];
        reader.Read();
        while (reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                throw new JsonException($"JSON array element for a list of {typeof(T).Name} may not be null; the protocol defines no array whose elements may be null");
            }

            // The element is not a JSON null, and no type in this library configures its converter to
            // return null for a non-null token (see DiscriminatorPropertyMissingValueBehavior.ReturnNull,
            // which nothing applies), so the deserialized element cannot be null. The null-forgiving
            // operator is therefore appropriate here. The element is read through its converter rather
            // than by re-entering the serializer; see JsonConverterUtilities.ReadNestedValue for why.
            elements.Add(JsonConverterUtilities.ReadNestedValue(ref reader, elementTypeInfo, options)!);
            reader.Read();
        }

        return elements;
    }

    /// <summary>
    /// Serializes a <see cref="List{T}"/> to a JSON array. This is a pass-through to the serialization
    /// each element would receive without this converter.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The list to be serialized.</param>
    /// <param name="options">The JsonSerializerOptions used for serializing the object.</param>
    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
    {
        // Use the JsonSerializer.Serialize() overload that takes a JsonTypeInfo
        // to remove warnings when publishing AOT compiled applications.
        JsonTypeInfo<T> elementTypeInfo = (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T));
        writer.WriteStartArray();
        foreach (T element in value)
        {
            JsonSerializer.Serialize(writer, element, elementTypeInfo);
        }

        writer.WriteEndArray();
    }
}
