// <copyright file="EventMessageJsonConverter{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WebDriverBiDi.Protocol;

/// <summary>
/// Deserializes the envelope of an <see cref="EventMessage{T}"/>, delegating only the
/// <c>params</c> payload to the serializer's type info resolvers.
/// </summary>
/// <typeparam name="T">The type of the event data carried by the event.</typeparam>
/// <remarks>
/// See <see cref="CommandResponseMessageJsonConverter{T}"/> for why the envelope is read here rather
/// than through generated metadata; a consumer registers only <typeparamref name="T"/> in their context.
/// </remarks>
public class EventMessageJsonConverter<T> : JsonConverter<EventMessage<T>>
{
    /// <summary>
    /// Deserializes an event message envelope.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>The deserialized <see cref="EventMessage{T}"/>.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is not an object, or the <c>type</c> or <c>params</c> property is missing or malformed.</exception>
    public override EventMessage<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Event message must be a JSON object");
        }

        JsonTypeInfo eventDataTypeInfo = options.GetTypeInfo(typeof(T));
        string? type = null;
        string eventName = string.Empty;
        bool hasEventData = false;
        T? eventData = default;
        Dictionary<string, JsonElement> extensionData = [];

        // The serializer only hands a converter a complete, well-formed value, so every Read()
        // succeeds and the object always terminates in an EndObject token.
        reader.Read();
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            // The reader is positioned on a property name here, which is never null.
            string propertyName = reader.GetString()!;
            reader.Read();
            switch (propertyName)
            {
                case "type":
                    type = reader.TokenType == JsonTokenType.String ? reader.GetString() : throw new JsonException("Event message 'type' property must be a string");
                    break;
                case "method":
                    eventName = reader.TokenType == JsonTokenType.String ? reader.GetString()! : throw new JsonException("Event message 'method' property must be a string");
                    break;
                case "params":
                    hasEventData = true;
                    eventData = (T?)JsonSerializer.Deserialize(ref reader, eventDataTypeInfo);
                    break;
                default:
                    extensionData[propertyName] = JsonElement.ParseValue(ref reader);
                    break;
            }

            reader.Read();
        }

        if (!hasEventData)
        {
            throw new JsonException("Event message is missing the required 'params' property");
        }

        return new EventMessage<T>
        {
            Type = type ?? throw new JsonException("Event message is missing the required 'type' property"),
            EventName = eventName,
            SerializableData = eventData,
            SerializableAdditionalData = extensionData,
        };
    }

    /// <summary>
    /// Serialization of event messages is not supported; events only ever flow from the remote end.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The value to serialize.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    /// <exception cref="NotSupportedException">Thrown when called, as this converter is only used for deserialization.</exception>
    public override void Write(Utf8JsonWriter writer, EventMessage<T> value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("EventMessageJsonConverter does not support serialization; event messages are only ever received.");
    }
}
