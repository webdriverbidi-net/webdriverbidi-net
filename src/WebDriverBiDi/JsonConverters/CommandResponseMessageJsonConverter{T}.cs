// <copyright file="CommandResponseMessageJsonConverter{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WebDriverBiDi.Protocol;

/// <summary>
/// Deserializes the envelope of a <see cref="CommandResponseMessage{T}"/>, delegating only the
/// <c>result</c> payload to the serializer's type info resolvers.
/// </summary>
/// <typeparam name="T">The type of the command result carried by the response.</typeparam>
/// <remarks>
/// The envelope's members are internal to this library, so a source-generated
/// <see cref="JsonSerializerContext"/> in a consumer assembly cannot produce working metadata for the
/// closed generic wrapper. This converter reads the envelope itself and asks the
/// <see cref="JsonSerializerOptions"/> only for <typeparamref name="T"/>, which a consumer can
/// register in their own context. <see cref="CommandParameters{T}"/> pairs it with the wrapper type
/// through <see cref="JsonMetadataServices.CreateValueInfo{T}(JsonSerializerOptions, JsonConverter)"/>.
/// </remarks>
public class CommandResponseMessageJsonConverter<T> : JsonConverter<CommandResponseMessage<T>>
    where T : CommandResult
{
    /// <summary>
    /// Deserializes a command response envelope.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>The deserialized <see cref="CommandResponseMessage{T}"/>.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is not an object, or the <c>type</c>, <c>id</c> or <c>result</c> property is missing or malformed.</exception>
    public override CommandResponseMessage<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Command response must be a JSON object");
        }

        JsonTypeInfo resultTypeInfo = options.GetTypeInfo(typeof(T));
        string? type = null;
        long? id = null;
        T? result = null;
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
                    type = reader.TokenType == JsonTokenType.String ? reader.GetString() : throw new JsonException("Command response 'type' property must be a string");
                    break;
                case "id":
                    id = reader.TokenType == JsonTokenType.Number ? reader.GetInt64() : throw new JsonException("Command response 'id' property must be a number");
                    break;
                case "result":
                    result = (T?)JsonSerializer.Deserialize(ref reader, resultTypeInfo) ?? throw new JsonException("Command response 'result' property must not be null");
                    break;
                default:
                    extensionData[propertyName] = JsonElement.ParseValue(ref reader);
                    break;
            }

            reader.Read();
        }

        return new CommandResponseMessage<T>
        {
            Type = type ?? throw new JsonException("Command response is missing the required 'type' property"),
            Id = id ?? throw new JsonException("Command response is missing the required 'id' property"),
            SerializableResult = result ?? throw new JsonException("Command response is missing the required 'result' property"),
            SerializableAdditionalData = extensionData,
        };
    }

    /// <summary>
    /// Serialization of command responses is not supported; responses only ever flow from the remote end.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The value to serialize.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    /// <exception cref="NotSupportedException">Thrown when called, as this converter is only used for deserialization.</exception>
    public override void Write(Utf8JsonWriter writer, CommandResponseMessage<T> value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("CommandResponseMessageJsonConverter does not support serialization; command responses are only ever received.");
    }
}
