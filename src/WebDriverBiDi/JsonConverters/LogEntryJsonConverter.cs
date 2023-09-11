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
}