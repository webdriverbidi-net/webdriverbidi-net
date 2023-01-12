// <copyright file="RemoteReferenceJsonConverter.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.JsonConverters;

using Newtonsoft.Json;
using WebDriverBidi.Script;

/// <summary>
/// The JSON converter for the RemoteReference object.
/// </summary>
public class RemoteReferenceJsonConverter : JsonConverter<RemoteReference>
{
    /// <summary>
    /// Gets a value indicating whether this converter can read JSON values.
    /// Returns false for this converter (converter used for serialization
    /// only).
    /// </summary>
    public override bool CanRead => false;

    /// <summary>
    /// Gets a value indicating whether this converter can write JSON values.
    /// Returns true for this converter (converter not used for
    /// deserialization).
    /// </summary>
    public override bool CanWrite => true;

    /// <summary>
    /// Reads a JSON string and deserializes it to an object.
    /// </summary>
    /// <param name="reader">The JSON reader to use during deserialization.</param>
    /// <param name="objectType">The type of object to which to deserialize.</param>
    /// <param name="existingValue">The existing value of the object.</param>
    /// <param name="hasExistingValue">A value indicating whether the existing value is null.</param>
    /// <param name="serializer">The JSON serializer to use in deserialization.</param>
    /// <returns>The deserialized object created from JSON.</returns>
    public override RemoteReference ReadJson(JsonReader reader, Type objectType, RemoteReference? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Serializes an object and writes it to a JSON string.
    /// </summary>
    /// <param name="writer">The JSON writer to use during serialization.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializer">The JSON serializer to use in serialization.</param>
    public override void WriteJson(JsonWriter writer, RemoteReference? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            throw new JsonSerializationException("Cannot serialize null value");
        }

        writer.WriteStartObject();
        writer.WritePropertyName("handle");
        writer.WriteValue(value.Handle);

        foreach (var additionalProperty in value.AdditionalData)
        {
            writer.WritePropertyName(additionalProperty.Key);
            serializer.Serialize(writer, additionalProperty.Value);
        }

        writer.WriteEndObject();
    }
}