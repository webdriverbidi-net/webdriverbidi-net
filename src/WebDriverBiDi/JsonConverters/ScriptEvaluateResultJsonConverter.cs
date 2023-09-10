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
    /// Gets a value indicating whether this converter can read JSON values.
    /// Returns true for this converter (converter used for deserialization
    /// only).
    /// </summary>
    // public override bool CanRead => true;

    /// <summary>
    /// Serializes an object and writes it to a JSON string.
    /// </summary>
    /// <param name="writer">The JSON writer to use during serialization.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializer">The JSON serializer to use in serialization.</param>
    // public override void WriteJson(JsonWriter writer, EvaluateResult? value, JsonSerializer serializer)
    // {
    //     throw new NotImplementedException();
    // }

    /// <summary>
    /// Reads a JSON string and deserializes it to an object.
    /// </summary>
    /// <param name="reader">The JSON reader to use during deserialization.</param>
    /// <param name="objectType">The type of object to which to deserialize.</param>
    /// <param name="existingValue">The existing value of the object.</param>
    /// <param name="hasExistingValue">A value indicating whether the existing value is null.</param>
    /// <param name="serializer">The JSON serializer to use in deserialization.</param>
    /// <returns>The deserialized object created from JSON.</returns>
    // public override EvaluateResult ReadJson(JsonReader reader, Type objectType, EvaluateResult? existingValue, bool hasExistingValue, JsonSerializer serializer)
    // {
    //     JObject jsonObject = JObject.Load(reader);
    //     if (jsonObject.TryGetValue("type", out JToken? typeToken))
    //     {
    //         if (typeToken.Type == JTokenType.String)
    //         {
    //             string resultType = typeToken.Value<string>()!;
    //             if (resultType == "success")
    //             {
    //                 EvaluateResultSuccess successResult = new();
    //                 serializer.Populate(jsonObject.CreateReader(), successResult);
    //                 return successResult;
    //             }
    //             else if (resultType == "exception")
    //             {
    //                 EvaluateResultException exceptionResult = new();
    //                 serializer.Populate(jsonObject.CreateReader(), exceptionResult);
    //                 return exceptionResult;
    //             }
    //             else
    //             {
    //                 throw new WebDriverBiDiException($"Malformed response: unknown type '{resultType}' for script result");
    //             }
    //         }
    //     }

    //     throw new WebDriverBiDiException("Malformed response: Script response must contain a 'type' property that contains a non-null string value");
    // }

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
                    throw new WebDriverBiDiException("Script response must contain a 'type' property that contains a non-null string value");
                }

                string resultType = typeNode.GetValue<string>();
                if (resultType == "success")
                {
                    return jsonObject.Deserialize<EvaluateResultSuccess>();
                }
                else if (resultType == "exception")
                {
                    return jsonObject.Deserialize<EvaluateResultException>();
                }

                throw new WebDriverBiDiException($"Malformed response: unknown type '{resultType}' for script result");
            }

            throw new WebDriverBiDiException("Malformed response: Script response must contain a 'type' property that contains a non-null string value");
        }

        throw new JsonException("JSON could not be parsed");
    }

    public override void Write(Utf8JsonWriter writer, EvaluateResult value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}