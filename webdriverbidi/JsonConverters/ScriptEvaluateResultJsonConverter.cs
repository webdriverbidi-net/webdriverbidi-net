namespace WebDriverBidi.JsonConverters;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Script;

public class ScriptEvaluateResultJsonConverter : JsonConverter<ScriptEvaluateResult>
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
    /// Writes objects to JSON. Not implemented.
    /// </summary>
    /// <param name="writer">JSON Writer Object</param>
    /// <param name="value">Value to be written</param>
    /// <param name="serializer">JSON Serializer </param>
    public override void WriteJson(JsonWriter writer, ScriptEvaluateResult? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Process the reader to return an object from JSON
    /// </summary>
    /// <param name="reader">A JSON reader</param>
    /// <param name="objectType">Type of the object</param>
    /// <param name="existingValue">The existing value of the object</param>
    /// <param name="hasExistingValue">A value indicating whether the existing value is null</param>
    /// <param name="serializer">JSON Serializer</param>
    /// <returns>Object created from JSON</returns>
    public override ScriptEvaluateResult ReadJson(JsonReader reader, Type objectType, ScriptEvaluateResult? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        if (jsonObject.ContainsKey("result"))
        {
            var successResult = new ScriptEvaluateResultSuccess("", new RemoteValue("null"));
            serializer.Populate(jsonObject.CreateReader(), successResult); 
            return successResult;
        }

        if (jsonObject.ContainsKey("exceptionDetails"))
        {
            var exceptionResult = new ScriptEvaluateResultException("", new ExceptionDetails("", 0, 0, new StackTrace(), new RemoteValue("null")));
            serializer.Populate(jsonObject.CreateReader(), exceptionResult); 
            return exceptionResult;
        }

        throw new WebDriverBidiException("Malformed response: ScriptTarget must contain either a 'result' or a 'exceptionDetails' property");
    }

}