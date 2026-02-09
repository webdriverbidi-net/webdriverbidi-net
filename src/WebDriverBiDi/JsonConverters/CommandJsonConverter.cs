// <copyright file="CommandJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Protocol;

/// <summary>
/// A converter for a serializing a Command object.
/// </summary>
public class CommandJsonConverter : JsonConverter<Command>
{
    /// <summary>
    /// Deserializes the JSON string to a Command object.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>A Command object.</returns>
    /// <exception cref="NotImplementedException">Thrown when called, as this converter is only used for serialization.</exception>
    public override Command? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Serializes a Command object to a JSON string.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The Command to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    public override void Write(Utf8JsonWriter writer, Command value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("id");
        writer.WriteNumberValue(value.CommandId);
        writer.WritePropertyName("method");
        writer.WriteStringValue(value.CommandName);
        writer.WritePropertyName("params");
        string serializedParams = JsonSerializer.Serialize(value.CommandParameters, value.CommandParameters.GetType(), options);
        writer.WriteRawValue(serializedParams);
        foreach (KeyValuePair<string, object?> pair in value.AdditionalData)
        {
            writer.WritePropertyName(pair.Key);
            writer.WriteRawValue(JsonSerializer.Serialize(pair.Value, options));
        }

        writer.WriteEndObject();
    }
}
