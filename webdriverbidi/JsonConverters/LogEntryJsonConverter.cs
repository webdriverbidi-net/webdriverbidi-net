namespace WebDriverBidi.JsonConverters;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Log;
using Script;

public class LogEntryJsonConverter : JsonConverter<LogEntry>
{
    /// <summary>
    /// Gets a value indicating whether this converter can read JSON values.
    /// Returns true for this converter (converter used for deserialization
    /// only).
    /// </summary>
    public override bool CanRead => true;

    /// <summary>
    /// Gets a value indicating whether this converter can write JSON values.
    /// Returns false for this converter (converter not used for
    /// serialization).
    /// </summary>
    public override bool CanWrite => false;

    /// <summary>
    /// Process the reader to return an object from JSON
    /// </summary>
    /// <param name="reader">A JSON reader</param>
    /// <param name="objectType">Type of the object</param>
    /// <param name="existingValue">The existing value of the object</param>
    /// <param name="hasExistingValue">A value indicating whether the existing value is null</param>
    /// <param name="serializer">JSON Serializer</param>
    /// <returns>Object created from JSON</returns>
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
    /// Writes objects to JSON. Not implemented.
    /// </summary>
    /// <param name="writer">JSON Writer Object</param>
    /// <param name="value">Value to be written</param>
    /// <param name="serializer">JSON Serializer </param>
    public override void WriteJson(JsonWriter writer, LogEntry? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}