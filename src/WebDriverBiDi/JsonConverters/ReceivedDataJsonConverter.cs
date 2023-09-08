// <copyright file="ReceivedDataJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using Newtonsoft.Json;

/// <summary>
/// Converts the response to JSON.
/// </summary>
public class ReceivedDataJsonConverter : JsonConverter
{
    /// <summary>
    /// Checks if the object can be converted.
    /// </summary>
    /// <param name="objectType">The object to be converted.</param>
    /// <returns>True if it can be converted; otherwise, false.</returns>
    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    /// <summary>
    /// Reads a JSON string and deserializes it to an object.
    /// </summary>
    /// <param name="reader">The JSON reader to use during deserialization.</param>
    /// <param name="objectType">The type of object to which to deserialize.</param>
    /// <param name="existingValue">The existing value of the object.</param>
    /// <param name="serializer">The JSON serializer to use in deserialization.</param>
    /// <returns>The deserialized object created from JSON.</returns>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        return this.ProcessToken(reader);
    }

    /// <summary>
    /// Serializes an object and writes it to a JSON string.
    /// </summary>
    /// <param name="writer">The JSON writer to use during serialization.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializer">The JSON serializer to use in serialization.</param>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    private object? ProcessToken(JsonReader reader)
    {
        // Recursively processes a token. This is required for elements that nest other elements.
        object? processedObject = null;
        if (reader != null)
        {
            reader.DateParseHandling = DateParseHandling.None;
            if (reader.TokenType == JsonToken.StartObject)
            {
                if (!this.TryProcessObject(reader, out Dictionary<string, object?> dictionaryObject))
                {
                    throw new JsonSerializationException("Incomplete object in JSON");
                }

                processedObject = dictionaryObject;
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                if (!this.TryProcessArray(reader, out List<object?> arrayObject))
                {
                    throw new JsonSerializationException("Incomplete array in JSON");
                }

                processedObject = arrayObject;
            }
            else
            {
                processedObject = reader.Value;
            }
        }

        return processedObject;
    }

    private bool TryProcessArray(JsonReader reader, out List<object?> arrayValue)
    {
        arrayValue = new();
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.EndArray)
            {
                return true;
            }

            arrayValue.Add(this.ProcessToken(reader));
        }

        return false;
    }

    private bool TryProcessObject(JsonReader reader, out Dictionary<string, object?> dictionaryValue)
    {
        dictionaryValue = new();
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.EndObject)
            {
                return true;
            }

            string? elementKey = reader.Value!.ToString();
            if (string.IsNullOrEmpty(elementKey))
            {
                throw new JsonSerializationException("JSON object key cannot be null or the empty string");
            }

            // CAUTION: Blind read here. We are relying on the serializer to handle
            // invalidly formed JSON.
            reader.Read();
            dictionaryValue.Add(elementKey, this.ProcessToken(reader));
        }

        return false;
    }
}
