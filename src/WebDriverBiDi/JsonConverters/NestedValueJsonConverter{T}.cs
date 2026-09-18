// <copyright file="NestedValueJsonConverter{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WebDriverBiDi.Internal;

/// <summary>
/// Reads a member whose type can contain the member's declaring type, so that the member lies on a
/// recursive path, through the converter of the member's type, after testing that the stack can accommodate
/// another level. The value read is exactly what it would be without this converter.
/// </summary>
/// <typeparam name="T">The type of the member.</typeparam>
/// <remarks>
/// <para>
/// A recursive protocol structure can be nested as deeply as the transport's maximum JSON depth allows, and
/// every level of the recursion must test the remaining stack, so that a value too deep for the current
/// thread fails with a catchable exception instead of overflowing the stack. A recursive path that runs
/// through a library collection converter is tested there. This converter serves a recursive path that
/// runs through a plain member, which the serializer would otherwise read without any test, such as
/// <see cref="Script.NodeProperties.ShadowRoot"/>.
/// </para>
/// <para>
/// The converter is inbound-only, as the types it is applied to are received and never sent.
/// </para>
/// </remarks>
public class NestedValueJsonConverter<T> : JsonConverter<T>
    where T : class
{
    /// <summary>
    /// Deserializes the value through the converter of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The options used for deserializing the JSON.</param>
    /// <returns>The deserialized value.</returns>
    /// <exception cref="InsufficientExecutionStackException">Thrown when the current thread's stack cannot accommodate reading the value.</exception>
    /// <remarks>
    /// The serializer does not call this method for a JSON <c>null</c> token; it assigns <see langword="null"/>
    /// itself, as it would without this converter.
    /// </remarks>
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonConverterUtilities.ReadNestedValue(ref reader, (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T)), options);
    }

    /// <summary>
    /// Not implemented. This converter is read-only; it is applied only to members of payloads received from
    /// the remote end, which are never serialized.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The value to be serialized.</param>
    /// <param name="options">The options used for serializing the object.</param>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("NestedValueJsonConverter does not support serialization; it is applied only to members of received payloads.");
    }
}
