// <copyright file="RemoteValueListJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Script;

/// <summary>
/// A converter to preserve RemoteValueList values format when serializing to JSON.
/// </summary>
public class RemoteValueListJsonConverter : JsonConverter<RemoteValueList>
{
    /// <summary>
    /// Deserializes the JSON string to a RemoteValueList value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>The deserialized RemoteValueList value.</returns>
    public override RemoteValueList Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // We can use the null-forgiving operator here, because if the JSON is valid, the
        // only way for the deserialization to return null is if the JSON value is null,
        // and the JSON deserializer will short-circuit and return null before calling this
        // converter.
        List<RemoteValue> values = JsonSerializer.Deserialize<List<RemoteValue>>(ref reader, options)!;
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
