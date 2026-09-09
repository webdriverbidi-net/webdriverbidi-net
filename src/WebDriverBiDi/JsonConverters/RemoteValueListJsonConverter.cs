// <copyright file="RemoteValueListJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Script;

/// <summary>
/// A read-only converter that deserializes the protocol's array representation of a
/// list of remote values to a <see cref="RemoteValueList"/>. Serialization is not
/// supported; the type is inbound-only.
/// </summary>
public class RemoteValueListJsonConverter : JsonConverter<RemoteValueList>
{
    private static readonly NonNullElementListJsonConverter<RemoteValue> ElementListConverter = new();

    /// <summary>
    /// Deserializes the JSON string to a RemoteValueList value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>The deserialized RemoteValueList value.</returns>
    public override RemoteValueList Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // The protocol's ListRemoteValue production is [*script.RemoteValue], which admits no null
        // element, so the elements are read through the converter that rejects one rather than through
        // the default List<RemoteValue> handling, which would admit a null into the list and defer the
        // failure to whichever consumer first dereferenced it.
        //
        // We can use the null-forgiving operator here, because that converter either returns a list or
        // throws; it never returns null. (Were the JSON value itself null, the deserializer would
        // short-circuit and return null before calling this converter at all.)
        List<RemoteValue> values = ElementListConverter.Read(ref reader, typeof(List<RemoteValue>), options)!;
        return new RemoteValueList(values);
    }

    /// <summary>
    /// Not implemented. This converter is read-only; <see cref="RemoteValueList"/> is an
    /// inbound-only type received from the browser and is never serialized.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The RemoteValueList value to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public override void Write(Utf8JsonWriter writer, RemoteValueList value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("RemoteValueListJsonConverter does not support serialization; RemoteValueList is an inbound-only type.");
    }
}
