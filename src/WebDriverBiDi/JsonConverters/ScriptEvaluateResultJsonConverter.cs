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
        JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement rootElement = doc.RootElement;
        if (rootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException($"Script response JSON must be an object, but was {rootElement.ValueKind}");
        }

        if (!rootElement.TryGetProperty("type", out JsonElement typeElement))
        {
            throw new JsonException("Script response must contain a 'type' property");
        }

        if (typeElement.ValueKind != JsonValueKind.String)
        {
            throw new JsonException("Script response 'type' property must be a string");
        }

        // We have previously determined that the token exists and is a string, and must
        // contain a value, so therefore cannot be null.
        string resultType = typeElement.GetString()!;
        if (resultType == "success")
        {
            return rootElement.Deserialize<EvaluateResultSuccess>(options);
        }
        else if (resultType == "exception")
        {
            return rootElement.Deserialize<EvaluateResultException>(options);
        }

        throw new JsonException($"Malformed response: unknown type '{resultType}' for script result");
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
