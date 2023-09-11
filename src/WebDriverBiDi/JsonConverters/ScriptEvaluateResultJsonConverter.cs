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