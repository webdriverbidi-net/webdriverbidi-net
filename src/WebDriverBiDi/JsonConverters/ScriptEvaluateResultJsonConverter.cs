// <copyright file="ScriptEvaluateResultJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using WebDriverBiDi.Script;

/// <summary>
/// The JSON converter for the ScriptEvaluateResult object.
/// </summary>
public class ScriptEvaluateResultJsonConverter : JsonConverter<EvaluateResult>
{
    /// <summary>
    /// Deserializes the JSON string to an EvaluateResult value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>An subclass of an EvaluateResult object as described by the JSON.</returns>
    /// <exception cref="JsonException">Thrown when invalid JSON is encountered.</exception>
    public override EvaluateResult? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonNode? node = JsonNode.Parse(ref reader);
        if (node is not null)
        {
            JsonObject jsonObject = node.AsObject();
            if (jsonObject.ContainsKey("type") && jsonObject["type"] is not null)
            {
                JsonNode typeNode = jsonObject["type"]!;
                if (typeNode.GetValueKind() != JsonValueKind.String)
                {
                    throw new JsonException("Script response must contain a 'type' property that contains a non-null string value");
                }

                string resultType = typeNode.GetValue<string>();
                if (resultType == "success")
                {
                    return jsonObject.Deserialize<EvaluateResultSuccess>(options);
                }
                else if (resultType == "exception")
                {
                    return jsonObject.Deserialize<EvaluateResultException>(options);
                }

                throw new JsonException($"Malformed response: unknown type '{resultType}' for script result");
            }

            throw new JsonException("Malformed response: Script response must contain a 'type' property that contains a non-null string value");
        }

        throw new JsonException("JSON could not be parsed");
    }

    /// <summary>
    /// Serializes an EvaluateResult object to a JSON string.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The Command to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    /// <exception cref="NotImplementedException">Thrown when called, as this converter is only used for deserialization.</exception>
    public override void Write(Utf8JsonWriter writer, EvaluateResult value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}