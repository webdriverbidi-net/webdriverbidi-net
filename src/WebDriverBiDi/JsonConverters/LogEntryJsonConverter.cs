// <copyright file="LogEntryJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Log;
using WebDriverBiDi.Script;

/// <summary>
/// The JSON converter for the LogEntry object.
/// </summary>
public class LogEntryJsonConverter : JsonConverter<LogEntry>
{
    /// <summary>
    /// Deserializes the JSON string to an LogEntry value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>A LogEntry, including the proper subclasses.</returns>
    /// <exception cref="JsonException">Thrown when invalid JSON is encountered.</exception>
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

                string method = methodElement.GetString() ?? throw new JsonException("ConsoleLogEntry 'method' property must not be null");
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
                    RemoteValue? value = arg.Deserialize<RemoteValue>(options);
                    if (value is null)
                    {
                        throw new JsonException("ConsoleLogEntry 'args' property array must contain valid RemoteValue items");
                    }

                    args.Add(value);
                }

                consoleEntry.SerializableArgs = args;
                entry = consoleEntry;
            }
            else
            {
                entry = new LogEntry();
            }

            entry.Type = type;
            entry.Text = textElement.GetString();
            entry.Level = levelElement.Deserialize<LogLevel>(options);
            entry.Source = sourceElement.Deserialize<Source>(options) ?? throw new JsonException("LogEntry 'source' property could not be deserialized");
            entry.EpochTimestamp = timestampElement.GetInt64();
            if (hasStackTrace)
            {
                entry.StackTrace = stackTraceElement.Deserialize<StackTrace>(options);
            }

            return entry;
        }

        throw new JsonException("JSON could not be parsed");
    }

    /// <summary>
    /// Serializes a LogEntry object to a JSON string.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The Command to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    /// <exception cref="NotImplementedException">Thrown when called, as this converter is only used for deserialization.</exception>
    public override void Write(Utf8JsonWriter writer, LogEntry value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
