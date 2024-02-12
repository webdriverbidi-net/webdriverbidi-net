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
        if (rootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException($"RealmInfo JSON must be an object, but was {rootElement.ValueKind}");
        }

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

        // We have already determined the value is a string, and
        // therefore cannot be null.
        RealmType type = typeElement.Deserialize<RealmType>();
        switch (type)
        {
            case RealmType.Window:
                realmInfo = this.ProcessWindowRealmInfo(rootElement);
                break;

            case RealmType.DedicatedWorker:
                realmInfo = this.ProcessDedicatedWorkerRealmInfo(rootElement);
                break;

            case RealmType.SharedWorker:
                realmInfo = new SharedWorkerRealmInfo();
                break;

            case RealmType.ServiceWorker:
                realmInfo = new ServiceWorkerRealmInfo();
                break;

            case RealmType.AudioWorklet:
                realmInfo = new AudioWorkletRealmInfo();
                break;

            case RealmType.PaintWorklet:
                realmInfo = new PaintWorkletRealmInfo();
                break;

            default:
                realmInfo = new RealmInfo();
                break;
        }

        realmInfo.Type = type;

        // We have already determined the value is a string, and
        // therefore cannot be null.
        string realmId = realmIdElement.GetString()!;
        realmInfo.RealmId = realmId;

        // We have already determined the value is a string, and
        // therefore cannot be null.
        string origin = originElement.GetString()!;
        realmInfo.Origin = origin;
        return realmInfo;
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

    private RealmInfo ProcessWindowRealmInfo(JsonElement rootElement)
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

        // We have already determined the value is a string, and
        // therefore cannot be null.
        string contextId = contextIdElement.GetString()!;
        windowRealmInfo.BrowsingContext = contextId;

        if (rootElement.TryGetProperty("sandbox", out JsonElement sandboxElement))
        {
            if (sandboxElement.ValueKind != JsonValueKind.String)
            {
                throw new JsonException("WindowRealmInfo 'sandbox' property must be a string");
            }

            // We have already determined the value is a string, and
            // therefore cannot be null.
            string sandbox = sandboxElement.GetString()!;
            windowRealmInfo.Sandbox = sandbox;
        }

        return windowRealmInfo;
    }

    private RealmInfo ProcessDedicatedWorkerRealmInfo(JsonElement rootElement)
    {
        if (!rootElement.TryGetProperty("owners", out JsonElement ownersElement))
        {
            throw new JsonException($"DedicatedWorkerRealmInfo 'owners' property is required");
        }

        if (ownersElement.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException($"DedicatedWorkerRealmInfo 'owners' property must be an array");
        }

        List<string> owners = new();
        foreach (JsonElement ownerElement in ownersElement.EnumerateArray())
        {
            if (ownerElement.ValueKind != JsonValueKind.String)
            {
                throw new JsonException($"All elements of DedicatedWorkerRealmInfo 'owners' property array must be strings");
            }

            string? ownerId = ownerElement.GetString();

            if (string.IsNullOrEmpty(ownerId))
            {
                throw new JsonException($"All elements of DedicatedWorkerRealmInfo 'owners' property array must be non-null and non-empty strings");
            }

            // Can use the null-forgiving operator, since we already checked for null.
            owners.Add(ownerId!);
        }

        DedicatedWorkerRealmInfo dedicatedWorkerRealmInfo = new();
        dedicatedWorkerRealmInfo.SerializableOwners.AddRange(owners);
        return dedicatedWorkerRealmInfo;
    }
}
