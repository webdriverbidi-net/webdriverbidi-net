// <copyright file="LogEntryJsonConverter.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.JsonConverters;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebDriverBidi.Log;

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
    public override bool CanRead => true;

    /// <summary>
    /// Reads a JSON string and deserializes it to an object.
    /// </summary>
    /// <param name="reader">The JSON reader to use during deserialization.</param>
    /// <param name="objectType">The type of object to which to deserialize.</param>
    /// <param name="existingValue">The existing value of the object.</param>
    /// <param name="hasExistingValue">A value indicating whether the existing value is null.</param>
    /// <param name="serializer">The JSON serializer to use in deserialization.</param>
    /// <returns>The deserialized object created from JSON.</returns>
    public override LogEntry ReadJson(JsonReader reader, Type objectType, LogEntry? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        if (!jsonObject.ContainsKey("text"))
        {
            throw new JsonSerializationException("LogEntry must have a 'text' property");
        }

        if (jsonObject.ContainsKey("type") && jsonObject["type"] is not null && jsonObject["type"]!.Type == JTokenType.String && jsonObject["type"]!.Value<string>() == "console")
        {
            ConsoleLogEntry consoleLogEntry = new();
            serializer.Populate(jsonObject.CreateReader(), consoleLogEntry);
            return consoleLogEntry;
        }

        LogEntry logEntry = new();
        serializer.Populate(jsonObject.CreateReader(), logEntry);
        return logEntry;
    }

    /// <summary>
    /// Serializes an object and writes it to a JSON string.
    /// </summary>
    /// <param name="writer">The JSON writer to use during serialization.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializer">The JSON serializer to use in serialization.</param>
    public override void WriteJson(JsonWriter writer, LogEntry? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}