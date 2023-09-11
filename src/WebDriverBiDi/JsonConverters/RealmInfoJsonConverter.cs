// <copyright file="RealmInfoJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Script;

/// <summary>
/// The JSON converter for the RealmInfo object.
/// </summary>
public class RealmInfoJsonConverter : JsonConverter<RealmInfo>
{
    /// <summary>
    /// Deserializes the JSON string to an RealmInfo value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>A LogEntry, including the proper subclasses.</returns>
    /// <exception cref="JsonException">Thrown when invalid JSON is encountered.</exception>
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

    /// <summary>
    /// Serializes a RealmInfo object to a JSON string.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The Command to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    /// <exception cref="NotImplementedException">Thrown when called, as this converter is only used for deserialization.</exception>
    public override void Write(Utf8JsonWriter writer, RealmInfo value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}