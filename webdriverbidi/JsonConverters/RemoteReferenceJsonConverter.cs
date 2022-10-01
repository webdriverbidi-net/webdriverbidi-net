namespace WebDriverBidi.JsonConverters;

using Newtonsoft.Json;
using Script;

public class RemoteReferenceJsonConverter : JsonConverter<RemoteReference>
{
    /// <summary>
    /// Gets a value indicating whether this converter can read JSON values.
    /// Returns false for this converter (converter used for serialization
    /// only).
    /// </summary>
    public override bool CanRead => false;

    /// <summary>
    /// Gets a value indicating whether this converter can write JSON values.
    /// Returns true for this converter (converter not used for
    /// deserialization).
    /// </summary>
    public override bool CanWrite => true;

    /// <summary>
    /// Process the reader to return an object from JSON
    /// </summary>
    /// <param name="reader">A JSON reader</param>
    /// <param name="objectType">Type of the object</param>
    /// <param name="existingValue">The existing value of the object</param>
    /// <param name="hasExistingValue">A value indicating whether the existing value is null</param>
    /// <param name="serializer">JSON Serializer</param>
    /// <returns>Object created from JSON</returns>
    public override RemoteReference ReadJson(JsonReader reader, Type objectType, RemoteReference? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Writes objects to JSON. Not implemented.
    /// </summary>
    /// <param name="writer">JSON Writer Object</param>
    /// <param name="value">Value to be written</param>
    /// <param name="serializer">JSON Serializer </param>
    public override void WriteJson(JsonWriter writer, RemoteReference? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            throw new JsonSerializationException("Cannot serialize null value");
        }

        writer.WriteStartObject();
        writer.WritePropertyName("handle");
        writer.WriteValue(value.Handle);
        
        foreach (var additionalProperty in value.AdditionalData)
        {
            writer.WritePropertyName(additionalProperty.Key);
            serializer.Serialize(writer, additionalProperty.Value);
        }

        writer.WriteEndObject();
    }
}