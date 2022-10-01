namespace WebDriverBidi.JsonConverters;

using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Script;

public class RemoteValueJsonConverter : JsonConverter<RemoteValue>
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
    public override void WriteJson(JsonWriter writer, RemoteValue? value, JsonSerializer serializer)
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
    public override RemoteValue ReadJson(JsonReader reader, Type objectType, RemoteValue? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);

        return ProcessObject(jsonObject, serializer);
    }

    private RemoteValue ProcessObject(JObject jsonObject, JsonSerializer serializer)
    {
        if (!jsonObject.ContainsKey("type"))
        {
            throw new JsonSerializationException("RemoteValue must contain a 'type' property");
        }

        var typeToken = jsonObject["type"];
        if (typeToken is null)
        {
            throw new JsonSerializationException("RemoteValue 'type' property must not be null");
        }

        if (typeToken.Type != JTokenType.String)
        {
            throw new JsonSerializationException("RemoteValue type property must be a string");
        }

        string? valueTypeString = jsonObject["type"]!.Value<string>();
        if (valueTypeString is null)
        {
            throw new JsonSerializationException("RemoteValue must have a non-null 'type' property that is a string");
        }

        RemoteValue result = new RemoteValue(valueTypeString);
        if (jsonObject.ContainsKey("value"))
        {
            var valueToken = jsonObject["value"];
            if (valueToken is null)
            {
                throw new JsonSerializationException($"RemoteValue 'value' property must be non-null");
            }

            this.ProcessValue(result, valueTypeString, valueToken, serializer);
        }

        if (jsonObject.ContainsKey("handle"))
        {
            var handleToken = jsonObject["handle"];
            if (handleToken is null)
            {
                throw new JsonSerializationException($"RemoteValue 'handle' propert, when present,y must be non-null");
            }

            if (handleToken.Type != JTokenType.String)
            {
                throw new JsonSerializationException($"RemoteValue 'handle' property, when present, must be a string");
            }

            string? handle = handleToken.Value<string>();
            result.Handle = handle;
        }

        if (jsonObject.ContainsKey("internalId"))
        {
            var internalIdToken = jsonObject["internalId"];
            if (internalIdToken is null)
            {
                throw new JsonSerializationException($"RemoteValue 'internalId' property, when present, must be non-null");
            }

            if (internalIdToken.Type != JTokenType.Integer)
            {
                throw new JsonSerializationException($"RemoteValue 'internalId' property, when present, must be an unsigned integer");
            }

            uint internalId = internalIdToken.Value<uint>();

            result.InternalId = internalId;
        }

        return result;
    }

    private void ProcessValue(RemoteValue result, string valueType, JToken token, JsonSerializer serializer)
    {
        if (valueType == "string")
        {
            if (token.Type != JTokenType.String)
            {
                throw new JsonSerializationException($"RemoteValue 'value' property for {valueType} must be a non-null string");
            }

            var stringValue = token.Value<string>();
            result.Value = stringValue;
        }

        if (valueType == "boolean")
        {
            if (token.Type != JTokenType.Boolean)
            {
                throw new JsonSerializationException($"RemoteValue 'value' property for {valueType} must be a boolean value");
            }

            var boolValue = token.Value<bool>();
            result.Value = boolValue;
        }

        if (valueType == "number")
        {
            if (token.Type == JTokenType.String)
            {
                string specialValue = token.Value<string>()!;
                if (specialValue == "Infinity")
                {
                    result.Value = double.PositiveInfinity;
                }
                else if (specialValue == "-Infinity")
                {
                    result.Value = double.NegativeInfinity;
                }
                else if (specialValue == "NaN")
                {
                    result.Value = double.NaN;
                }
                else if (specialValue == "-0")
                {
                    result.Value = decimal.Negate(decimal.Zero);
                }
                else
                {
                    throw new JsonSerializationException($"RemoteValue invalid value '{specialValue}' for 'value' property of {valueType}");
                }
            }
            else if (token.Type == JTokenType.Integer)
            {
                result.Value = token.Value<long>();
            }
            else if (token.Type == JTokenType.Float)
            {
                result.Value = token.Value<double>();
            }
            else
            {
                throw new JsonSerializationException($"RemoteValue invalid type {token.Type} for 'value' property of {valueType}");
            }
        }

        if (valueType == "bigint")
        {
            if (token.Type != JTokenType.String)
            {
                throw new JsonSerializationException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is a string");
            }

            string? bigintString = token.Value<string>();
            BigInteger bigintValue;
            if (!BigInteger.TryParse(bigintString, out bigintValue))
            {
                throw new JsonSerializationException($"RemoteValue cannot parse invalid value '{bigintString}' for {valueType}");
            }

            result.Value = bigintValue;
        }

        if (valueType == "date")
        {
            if (token.Type != JTokenType.String)
            {
                throw new JsonSerializationException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is a string");
            }

            string? dateString = token.Value<string>();
            DateTime dateTimeValue;
            if (!DateTime.TryParse(dateString, out dateTimeValue))
            {
                throw new JsonSerializationException($"RemoteValue cannot parse invalid value '{dateString}' for {valueType}");
            }

            result.Value = dateTimeValue;
        }

        if (valueType == "regexp")
        {
            JObject? regexObject = token as JObject;
            if (regexObject is null)
            {
                throw new JsonSerializationException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an object");
            }

            RegularExpressionProperties regexProperties = new RegularExpressionProperties("");
            serializer.Populate(regexObject.CreateReader(), regexProperties);
            result.Value = regexProperties;
        }

        if (valueType == "node")
        {
            JObject? nodeObject = token as JObject;
            if (nodeObject is null)
            {
                throw new JsonSerializationException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an object");
            }

            NodeProperties nodeProperties = new NodeProperties(0, "", 0);
            serializer.Populate(nodeObject.CreateReader(), nodeProperties);
            result.Value = nodeProperties;
        }

        if (valueType == "array" || valueType == "set")
        {
            JArray? arrayObject = token as JArray;
            if (arrayObject is null)
            {
                throw new JsonSerializationException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an array");
            }

            List<RemoteValue> remoteValueList = new List<RemoteValue>();
            foreach (var arrayItem in arrayObject)
            {
                JObject? arrayItemObject = arrayItem as JObject;
                if (arrayItemObject is null)
                {
                    throw new JsonSerializationException($"RemoteValue each element for {valueType} must be an object");
                }

                remoteValueList.Add(this.ProcessObject(arrayItemObject, serializer));
            }

            result.Value = remoteValueList;
        }

        if (valueType == "map" || valueType == "object")
        {
            JObject? mapObject = token as JObject;
            if (mapObject is null)
            {
                throw new JsonSerializationException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an object");
            }

            Dictionary<string, RemoteValue> remoteValueDictionary = new Dictionary<string, RemoteValue>();
            foreach (var mapEntry in mapObject)
            {
                var mapEntryValueObject = mapEntry.Value as JObject;
                if (mapEntryValueObject is null)
                {
                    throw new JsonSerializationException($"RemoteValue each property value for {valueType} must be an object");
                }

                remoteValueDictionary[mapEntry.Key] = this.ProcessObject(mapEntryValueObject, serializer);
            }

            result.Value = remoteValueDictionary;
         }
    }
}