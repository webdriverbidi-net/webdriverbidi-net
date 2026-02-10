// <copyright file="ScriptTargetJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Script;

/// <summary>
/// The JSON converter for the ScriptTarget object.
/// </summary>
public class ScriptTargetJsonConverter : JsonConverter<Target>
{
    /// <summary>
    /// Deserializes the JSON string to an Target value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>An subclass of a Target object as described by the JSON.</returns>
    /// <exception cref="JsonException">Thrown when invalid JSON is encountered.</exception>
    public override Target? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement rootElement = doc.RootElement;
        if (rootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException($"Script target JSON must be an object, but was {rootElement.ValueKind}");
        }

        if (rootElement.TryGetProperty("realm", out JsonElement realmTargetElement))
        {
            return rootElement.Deserialize<RealmTarget>();
        }

        if (rootElement.TryGetProperty("context", out JsonElement contextTargetElement))
        {
            return rootElement.Deserialize<ContextTarget>();
        }

        throw new JsonException("Malformed response: ScriptTarget must contain either a 'realm' or a 'context' property");
    }

    /// <summary>
    /// Serializes a subclass of a Target object to a JSON string.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The Command to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    public override void Write(Utf8JsonWriter writer, Target value, JsonSerializerOptions options)
    {
        string json = JsonSerializer.Serialize(value, value.GetType(), options);
        writer.WriteRawValue(json);
        writer.Flush();
    }
}
