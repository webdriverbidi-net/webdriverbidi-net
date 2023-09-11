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
        string json = JsonSerializer.Serialize(value, value.GetType());
        writer.WriteRawValue(json);
        writer.Flush();
    }
}