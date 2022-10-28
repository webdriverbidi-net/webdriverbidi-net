namespace WebDriverBidi.JsonConverters;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Session;

public class CapabilitiesResultJsonConverter : JsonConverter<CapabilitiesResult>
{
    private static readonly List<string> KnownCapabilityNames = new List<string>()
    {
        "acceptInsecureCerts",
        "browserName",
        "browserVersion",
        "platformName",
        "proxy",
        "setWindowRect"
    };

    /// <summary>
    /// Gets a value indicating whether this converter can read JSON values.
    /// Returns false for this converter (converter used for serialization
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
    public override CapabilitiesResult ReadJson(JsonReader reader, Type objectType, CapabilitiesResult? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        CapabilitiesResult result = new CapabilitiesResult();
        serializer.Populate(jsonObject.CreateReader(), result);
        Dictionary<string, object?> additionalCapabilities = new Dictionary<string, object?>();
        foreach (var token in jsonObject)
        {
            if (!KnownCapabilityNames.Contains(token.Key))
            {
                if (token.Value is null)
                {
                    additionalCapabilities[token.Key] = null;
                }
                else
                {
                    additionalCapabilities[token.Key] = serializer.Deserialize(token.Value.CreateReader());
                }
            }
        }

        if (additionalCapabilities.Count > 0)
        {
            result.AdditionalCapabilities = new AdditionalCapabilities(additionalCapabilities);
        }

        return result;
    }

    /// <summary>
    /// Writes objects to JSON. Not implemented.
    /// </summary>
    /// <param name="writer">JSON Writer Object</param>
    /// <param name="value">Value to be written</param>
    /// <param name="serializer">JSON Serializer </param>
    public override void WriteJson(JsonWriter writer, CapabilitiesResult? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
