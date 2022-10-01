namespace WebDriverBidi.JsonConverters;

using Newtonsoft.Json;
using Session;

public class CapabilitiesRequestJsonConverter : JsonConverter<CapabilitiesRequest>
{
    /// <summary>
    /// Gets a value indicating whether this converter can read JSON values.
    /// Returns false for this converter (converter not used for
    /// deserialization).
    /// </summary>
    public override bool CanRead => false;

    /// <summary>
    /// Gets a value indicating whether this converter can write JSON values.
    /// Returns false for this converter (converter used for serialization).
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
    public override CapabilitiesRequest ReadJson(JsonReader reader, Type objectType, CapabilitiesRequest? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Writes objects to JSON. Not implemented.
    /// </summary>
    /// <param name="writer">JSON Writer Object</param>
    /// <param name="value">Value to be written</param>
    /// <param name="serializer">JSON Serializer </param>
    public override void WriteJson(JsonWriter writer, CapabilitiesRequest? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            throw new JsonSerializationException("Cannot serialize null value");
        }

        writer.WriteStartObject();
        if (value.BrowserName is not null)
        {
            writer.WritePropertyName("browserName");
            writer.WriteValue(value.BrowserName);
        }

        if (value.BrowserVersion is not null)
        {
            writer.WritePropertyName("browserVersion");
            writer.WriteValue(value.BrowserVersion);
        }

        if (value.PlatformName is not null)
        {
            writer.WritePropertyName("platformName");
            writer.WriteValue(value.PlatformName);
        }

        if (value.AcceptInsecureCertificates is not null)
        {
            writer.WritePropertyName("acceptInsecureCertificates");
            writer.WriteValue(value.AcceptInsecureCertificates);
        }

        if (value.Proxy is not null)
        {
            writer.WritePropertyName("proxy");
            serializer.Serialize(writer, value.Proxy);
        }

        foreach (var additionalProperty in value.AdditionalCapabilities)
        {
            // TODO: Validation of serializable values in additional
            // capabilites.
            writer.WritePropertyName(additionalProperty.Key);
            serializer.Serialize(writer, additionalProperty.Value);
        }

        writer.WriteEndObject();
    }
}
