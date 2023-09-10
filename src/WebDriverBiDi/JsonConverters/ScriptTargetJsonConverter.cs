// <copyright file="ScriptTargetJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using WebDriverBiDi.Script;

/// <summary>
/// The JSON converter for the ScriptTarget object.
/// </summary>
public class ScriptTargetJsonConverter : JsonConverter<Target>
{
    /// <summary>
    /// Gets a value indicating whether this converter can read JSON values.
    /// Returns true for this converter (converter used for deserialization
    /// only).
    /// </summary>
    // public override bool CanRead => true;

    /// <summary>
    /// Gets a value indicating whether this converter can write JSON values.
    /// Returns false for this converter (converter not used for
    /// serialization).
    /// </summary>
    // public override bool CanWrite => false;

    /// <summary>
    /// Reads a JSON string and deserializes it to an object.
    /// </summary>
    /// <param name="reader">The JSON reader to use during deserialization.</param>
    /// <param name="objectType">The type of object to which to deserialize.</param>
    /// <param name="existingValue">The existing value of the object.</param>
    /// <param name="hasExistingValue">A value indicating whether the existing value is null.</param>
    /// <param name="serializer">The JSON serializer to use in deserialization.</param>
    /// <returns>The deserialized object created from JSON.</returns>
    // public override Target ReadJson(JsonReader reader, Type objectType, Target? existingValue, bool hasExistingValue, JsonSerializer serializer)
    // {
    //     JObject jsonObject = JObject.Load(reader);
    //     Target target;
    //     if (jsonObject.ContainsKey("realm"))
    //     {
    //         target = new RealmTarget(string.Empty);
    //         serializer.Populate(jsonObject.CreateReader(), target);
    //         return target;
    //     }

    //     if (jsonObject.ContainsKey("context"))
    //     {
    //         target = new ContextTarget(string.Empty);
    //         serializer.Populate(jsonObject.CreateReader(), target);
    //         return target;
    //     }

    //     throw new WebDriverBiDiException("Malformed response: ScriptTarget must contain either a 'realm' or a 'context' property");
    // }

    public override Target? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonNode? node = JsonNode.Parse(ref reader);
        if (node is not null)
        {
            JsonObject jsonObject = node.AsObject();
            if (jsonObject.ContainsKey("realm"))
            {
                return jsonObject.Deserialize<RealmTarget>();
            }

            if (jsonObject.ContainsKey("context"))
            {
                return jsonObject.Deserialize<ContextTarget>();
            }

            throw new WebDriverBiDiException("Malformed response: ScriptTarget must contain either a 'realm' or a 'context' property");
        }

        throw new JsonException("JSON could not be parsed");
    }

    public override void Write(Utf8JsonWriter writer, Target value, JsonSerializerOptions options)
    {
        string json = JsonSerializer.Serialize(value);
        writer.WriteRawValue(json);
        writer.Flush();
    }

    /// <summary>
    /// Serializes an object and writes it to a JSON string.
    /// </summary>
    /// <param name="writer">The JSON writer to use during serialization.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializer">The JSON serializer to use in serialization.</param>
    // public override void WriteJson(JsonWriter writer, Target? value, JsonSerializer serializer)
    // {
    //     throw new NotImplementedException();
    // }
}