// <copyright file="ReceivedDataJsonConverter.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.JsonConverters;

using Newtonsoft.Json;

/// <summary>
/// Converts the response to JSON.
/// </summary>
internal class ReceivedDataJsonConverter : JsonConverter
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
        serializer?.Serialize(writer, value);
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
                Dictionary<string, object?> dictionaryValue = new();
                while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                {
                    string elementKey = reader.Value?.ToString() ?? string.Empty;
                    reader.Read();
                    dictionaryValue.Add(elementKey, this.ProcessToken(reader));
                }

                processedObject = dictionaryValue;
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                List<object?> arrayValue = new();
                while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                {
                    arrayValue.Add(this.ProcessToken(reader));
                }

                processedObject = arrayValue;
            }
            else
            {
                processedObject = reader.Value;
            }
        }

        return processedObject;
    }
}
