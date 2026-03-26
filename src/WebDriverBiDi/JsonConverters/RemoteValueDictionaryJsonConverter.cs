// <copyright file="RemoteValueDictionaryJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Script;

/// <summary>
/// A converter to preserve RemoteValueDictionary values format when serializing to JSON.
/// </summary>
public class RemoteValueDictionaryJsonConverter : JsonConverter<RemoteValueDictionary>
{
    /// <summary>
    /// Deserializes the JSON string to a RemoteValueDictionary value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>The deserialized RemoteValueDictionary value.</returns>
    public override RemoteValueDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Implementation for deserializing RemoteValueDictionary
        RemoteValueDictionary remoteValueDictionary = this.ProcessMap(JsonDocument.ParseValue(ref reader).RootElement, options);
        return remoteValueDictionary;
    }

    /// <summary>
    /// Not implemented. This converter is read-only; <see cref="RemoteValueDictionary"/> is an
    /// inbound-only type received from the browser and is never serialized.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The RemoteValueDictionary value to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public override void Write(Utf8JsonWriter writer, RemoteValueDictionary value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("RemoteValueDictionaryJsonConverter does not support serialization; RemoteValueDictionary is an inbound-only type.");
    }

    private RemoteValueDictionary ProcessMap(JsonElement mapArray, JsonSerializerOptions options)
    {
        Dictionary<object, RemoteValue> remoteValueDictionary = [];
        foreach (JsonElement mapElementToken in mapArray.EnumerateArray())
        {
            if (mapElementToken.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException($"RemoteValue array element for dictionary must be an array");
            }

            if (mapElementToken.GetArrayLength() != 2)
            {
                throw new JsonException($"RemoteValue array element for dictionary must be an array with exactly two elements");
            }

            JsonElement keyToken = mapElementToken[0];
            if (keyToken.ValueKind != JsonValueKind.String && keyToken.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"RemoteValue array element for dictionary must have a first element (key) that is either a string or an object");
            }

            object pairKey = this.ProcessMapKey(keyToken, options);

            JsonElement valueToken = mapElementToken[1];
            if (valueToken.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"RemoteValue array element for dictionary must have a second element (value) that is an object");
            }

            RemoteValue pairValue = valueToken.Deserialize<RemoteValue>(options)!;
            remoteValueDictionary[pairKey] = pairValue;
        }

        return new RemoteValueDictionary(remoteValueDictionary);
    }

    private object ProcessMapKey(JsonElement keyToken, JsonSerializerOptions options)
    {
        object pairKey;
        if (keyToken.ValueKind == JsonValueKind.String)
        {
            // The token type is already guaranteed to be a string, and
            // therefore cannot be null.
            pairKey = keyToken.GetString()!;
        }
        else
        {
            // Previous caller has already determined the value must be either
            // a string or object. We will use the null forgiving operator since
            // the token must be an object, and therefore the cast cannot return
            // null.
            RemoteValue keyRemoteValue = keyToken.Deserialize<RemoteValue>(options)!;
            pairKey = keyRemoteValue;
        }

        return pairKey;
    }
}
