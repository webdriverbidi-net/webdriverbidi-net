// <copyright file="RealmInfoJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using WebDriverBiDi.Script;

/// <summary>
/// The JSON converter for the RealmInfo object.
/// </summary>
public class RealmInfoJsonConverter : JsonConverter<RealmInfo>
{
    /// <summary>
    /// Gets a value indicating whether this converter can read JSON values.
    /// Returns true for this converter (converter used for deserialization
    /// only).
    /// </summary>
    // public override bool CanRead => true;

    /// <summary>
    /// Reads a JSON string and deserializes it to an object.
    /// </summary>
    /// <param name="reader">The JSON reader to use during deserialization.</param>
    /// <param name="objectType">The type of object to which to deserialize.</param>
    /// <param name="existingValue">The existing value of the object.</param>
    /// <param name="hasExistingValue">A value indicating whether the existing value is null.</param>
    /// <param name="serializer">The JSON serializer to use in deserialization.</param>
    /// <returns>The deserialized object created from JSON.</returns>
    // public override RealmInfo ReadJson(JsonReader reader, Type objectType, RealmInfo? existingValue, bool hasExistingValue, JsonSerializer serializer)
    // {
    //     JObject jsonObject = JObject.Load(reader);
    //     if (jsonObject.ContainsKey("type") && jsonObject["type"] is not null && jsonObject["type"]!.Type == JTokenType.String && jsonObject["type"]!.Value<string>() == "window")
    //     {
    //         WindowRealmInfo windowInfo = new();
    //         serializer.Populate(jsonObject.CreateReader(), windowInfo);
    //         return windowInfo;
    //     }

    //     RealmInfo realmInfo = new();
    //     serializer.Populate(jsonObject.CreateReader(), realmInfo);
    //     return realmInfo;
    // }

    public override RealmInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        RealmInfo? realmInfo;
        JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement rootElement = doc.RootElement;
        if (rootElement.ValueKind == JsonValueKind.Object)
        {
            if (!rootElement.TryGetProperty("type", out JsonElement typeElement))
            {
                throw new JsonException("RealmInfo 'type' property is required");
            }

            if (typeElement.ValueKind != JsonValueKind.String)
            {
                throw new JsonException("RealmInfo 'type' property must be a string");
            }

            if (!rootElement.TryGetProperty("realm", out JsonElement realmIdElement))
            {
                throw new JsonException("RealmInfo 'realm' property is required");
            }
            
            if (realmIdElement.ValueKind != JsonValueKind.String)
            {
                throw new JsonException("RealmInfo 'realm' property must be a string");
            }

            if (!rootElement.TryGetProperty("origin", out JsonElement originElement))
            {
                throw new JsonException("RealmInfo 'origin' property is required");
            }

            if (originElement.ValueKind != JsonValueKind.String)
            {
                throw new JsonException("RealmInfo 'origin' property must be a string");
            }

            string typeString = typeElement.GetString() ?? throw new JsonException("RealmInfo 'type' property must not be null");
            RealmType type = typeElement.Deserialize<RealmType>();
            if (type == RealmType.Window)
            {
                WindowRealmInfo windowRealmInfo = new();
                if (!rootElement.TryGetProperty("context", out JsonElement contextIdElement))
                {
                    throw new JsonException("WindowRealmInfo 'context' property is required");
                }

                if (contextIdElement.ValueKind != JsonValueKind.String)
                {
                    throw new JsonException("WindowRealmInfo 'context' property must be a string");
                }

                string contextId = contextIdElement.GetString() ?? throw new JsonException("WindowRealmInfo 'context' property must not be null");
                windowRealmInfo.BrowsingContext = contextId;

                if (rootElement.TryGetProperty("sandbox", out JsonElement sandboxElement))
                {
                    if (sandboxElement.ValueKind != JsonValueKind.String)
                    {
                        throw new JsonException("WindowRealmInfo 'sandbox' property must be a string");
                    }

                    string sandbox = sandboxElement.GetString() ?? throw new JsonException("WindowRealmInfo 'sandbox' property must not be null");
                    windowRealmInfo.Sandbox = sandbox;
                }

                realmInfo = windowRealmInfo;
            }
            else
            {
                realmInfo = new RealmInfo();
            }

            realmInfo.Type = type;

            string? realmId = realmIdElement.GetString() ?? throw new JsonException("RealmInfo 'realm' property must not be null");
            realmInfo.RealmId = realmId;

            string? origin = originElement.GetString() ?? throw new JsonException("RealmInfo 'type' property must not be null");
            realmInfo.Origin = origin;
            return realmInfo;
        }

        throw new JsonException("JSON could not be parsed");
    }

    public override void Write(Utf8JsonWriter writer, RealmInfo value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Serializes an object and writes it to a JSON string.
    /// </summary>
    /// <param name="writer">The JSON writer to use during serialization.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializer">The JSON serializer to use in serialization.</param>
    // public override void WriteJson(JsonWriter writer, RealmInfo? value, JsonSerializer serializer)
    // {
    //     throw new NotImplementedException();
    // }
}