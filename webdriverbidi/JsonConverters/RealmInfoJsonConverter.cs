namespace WebDriverBidi.JsonConverters;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Script;

public class RealmInfoJsonConverter : JsonConverter<RealmInfo>
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
    public override RealmInfo ReadJson(JsonReader reader, Type objectType, RealmInfo? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        if (jsonObject.ContainsKey("type") && jsonObject["type"] is not null && jsonObject["type"]!.Type == JTokenType.String && jsonObject["type"]!.Value<string>() == "window")
        {
            WindowRealmInfo windowInfo = new();
            serializer.Populate(jsonObject.CreateReader(), windowInfo); 
            return windowInfo;
        }

        RealmInfo realmInfo = new();
        serializer.Populate(jsonObject.CreateReader(), realmInfo); 
        return realmInfo;
    }

    /// <summary>
    /// Writes objects to JSON. Not implemented.
    /// </summary>
    /// <param name="writer">JSON Writer Object</param>
    /// <param name="value">Value to be written</param>
    /// <param name="serializer">JSON Serializer </param>
    public override void WriteJson(JsonWriter writer, RealmInfo? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}