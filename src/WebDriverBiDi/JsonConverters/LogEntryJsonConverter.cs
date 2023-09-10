// <copyright file="LogEntryJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using WebDriverBiDi.Log;
using WebDriverBiDi.Script;

/// <summary>
/// The JSON converter for the LogEntry object.
/// </summary>
public class LogEntryJsonConverter : JsonConverter<LogEntry>
{
    /// <summary>
    /// Gets a value indicating whether this converter can read JSON values.
    /// Returns true for this converter (converter used for deserialization
    /// only).
    /// </summary>
    //public override bool CanRead => true;

    /// <summary>
    /// Reads a JSON string and deserializes it to an object.
    /// </summary>
    /// <param name="reader">The JSON reader to use during deserialization.</param>
    /// <param name="objectType">The type of object to which to deserialize.</param>
    /// <param name="existingValue">The existing value of the object.</param>
    /// <param name="hasExistingValue">A value indicating whether the existing value is null.</param>
    /// <param name="serializer">The JSON serializer to use in deserialization.</param>
    /// <returns>The deserialized object created from JSON.</returns>
    // public override LogEntry ReadJson(JsonReader reader, Type objectType, LogEntry? existingValue, bool hasExistingValue, JsonSerializer serializer)
    // {
    //     JObject jsonObject = JObject.Load(reader);
    //     if (!jsonObject.ContainsKey("text"))
    //     {
    //         throw new JsonSerializationException("LogEntry must have a 'text' property");
    //     }

    //     if (jsonObject.ContainsKey("type") && jsonObject["type"] is not null && jsonObject["type"]!.Type == JTokenType.String && jsonObject["type"]!.Value<string>() == "console")
    //     {
    //         ConsoleLogEntry consoleLogEntry = new();
    //         serializer.Populate(jsonObject.CreateReader(), consoleLogEntry);
    //         return consoleLogEntry;
    //     }

    //     LogEntry logEntry = new();
    //     serializer.Populate(jsonObject.CreateReader(), logEntry);
    //     return logEntry;
    // }

    public override LogEntry? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        LogEntry? entry;
        JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement rootElement = doc.RootElement;
        if (rootElement.ValueKind == JsonValueKind.Object)
        {
            if (!rootElement.TryGetProperty("text", out JsonElement textElement))
            {
                throw new JsonException("LogEntry 'text' property is required");
            }

            if (!rootElement.TryGetProperty("type", out JsonElement typeElement))
            {
                throw new JsonException("LogEntry 'type' property is required");
            }

            if (!rootElement.TryGetProperty("level", out JsonElement levelElement))
            {
                throw new JsonException("LogEntry must have a 'level' property");
            }

            if (!rootElement.TryGetProperty("source", out JsonElement sourceElement))
            {
                throw new JsonException("LogEntry must have a 'source' property");
            }

            if (!rootElement.TryGetProperty("timestamp", out JsonElement timestampElement))
            {
                throw new JsonException("LogEntry must have a 'timestamp' property");
            }

            bool hasStackTrace = rootElement.TryGetProperty("stackTrace", out JsonElement stackTraceElement);

            if (typeElement.ValueKind != JsonValueKind.String)
            {
                throw new JsonException("LogEntry type property must be a string");
            }

            string? type = typeElement.GetString() ?? throw new JsonException("LogEntry 'type' property must not be null");
            if (type == "console")
            {
                ConsoleLogEntry consoleEntry = new();
                if (!rootElement.TryGetProperty("method", out JsonElement methodElement))
                {
                    throw new JsonException("ConsoleLogEntry 'method' property is required");
                }

                string? method = methodElement.GetString() ?? throw new JsonException("ConsoleLogEntry 'method' property must not be null");
                consoleEntry.Method = method;

                if (!rootElement.TryGetProperty("args", out JsonElement argsElement))
                {
                    throw new JsonException("ConsoleLogEntry 'args' property is required");
                }

                if (argsElement.ValueKind != JsonValueKind.Array)
                {
                    throw new JsonException("ConsoleLogEntry 'args' property value must be an array");
                }

                List<RemoteValue> args = new();
                foreach (JsonElement arg in argsElement.EnumerateArray())
                {
                    RemoteValue? value = arg.Deserialize<RemoteValue>();
                    args.Add(value);
                }

                consoleEntry.SerializableArgs = args ?? throw new JsonException("ConsoleLogEntry 'args property must not be null");
                entry = consoleEntry;
            }
            else
            {
                entry = new LogEntry();
            }

            entry.Type = type;
            entry.Text = textElement.GetString();
            entry.Level = levelElement.Deserialize<LogLevel>();
            entry.Source = sourceElement.Deserialize<Source>();
            entry.EpochTimestamp = timestampElement.GetInt64();
            if (hasStackTrace)
            {
                entry.StackTrace = stackTraceElement.Deserialize<StackTrace>();
            }

            return entry;
        }

        throw new JsonException("JSON could not be parsed");
    }

    public override void Write(Utf8JsonWriter writer, LogEntry value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Serializes an object and writes it to a JSON string.
    /// </summary>
    /// <param name="writer">The JSON writer to use during serialization.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializer">The JSON serializer to use in serialization.</param>
    // public override void WriteJson(JsonWriter writer, LogEntry? value, JsonSerializer serializer)
    // {
    //     throw new NotImplementedException();
    // }
}