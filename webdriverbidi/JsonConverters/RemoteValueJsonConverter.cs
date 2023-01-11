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

        RemoteValue result = new(valueTypeString);
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
            result.Value = ProcessNumber(token);
        }

        if (valueType == "bigint")
        {
            if (token.Type != JTokenType.String)
            {
                throw new JsonSerializationException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is a string");
            }

            string? bigintString = token.Value<string>();
            if (!BigInteger.TryParse(bigintString, out BigInteger bigintValue))
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
            if (!DateTime.TryParse(dateString, out DateTime dateTimeValue))
            {
                throw new JsonSerializationException($"RemoteValue cannot parse invalid value '{dateString}' for {valueType}");
            }

            result.Value = dateTimeValue;
        }

        if (valueType == "regexp")
        {
            if (token is not JObject regexObject)
            {
                throw new JsonSerializationException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an object");
            }

            RegularExpressionValue regexProperties = new("");
            serializer.Populate(regexObject.CreateReader(), regexProperties);
            result.Value = regexProperties;
        }

        if (valueType == "node")
        {
            if (token is not JObject nodeObject)
            {
                throw new JsonSerializationException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an object");
            }

            NodeProperties nodeProperties = new();
            serializer.Populate(nodeObject.CreateReader(), nodeProperties);
            result.Value = nodeProperties;
        }

        if (valueType == "array" || valueType == "set" || valueType == "nodelist" || valueType == "htmlcollection")
        {
            if (token is not JArray arrayObject)
            {
                throw new JsonSerializationException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an array");
            }

            result.Value = this.ProcessList(arrayObject, serializer);
        }

        if (valueType == "map" || valueType == "object")
        {
            if (token is not JArray mapArray)
            {
                throw new JsonSerializationException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an array");
            }

            result.Value = this.ProcessMap(mapArray, serializer);
        }
    }

    private object ProcessMapKey(JToken keyToken, JsonSerializer serializer)
    {
        object pairKey;
        if (keyToken.Type == JTokenType.String)
        {
            // Note: Use the null coalescing operator here to suppress
            // the compiler warning, but the value should never be
            // null.
            pairKey = keyToken.Value<string>() ?? "";
        }
        else
        {
            if (keyToken is not JObject keyObject)
            {
                throw new JsonSerializationException($"RemoteValue array key token indicated string or object, but could not be cast to either");
            }

            var keyRemoteValue = this.ProcessObject(keyObject, serializer);
            if ((keyRemoteValue.IsPrimitive || keyRemoteValue.Type == "date" || keyRemoteValue.Type == "regexp") && keyRemoteValue.Value is not null)
            {
                pairKey = keyRemoteValue.Value;
            }
            else if (keyRemoteValue.Handle is not null)
            {
                pairKey = keyRemoteValue.Handle;
            }
            else if (keyRemoteValue.InternalId is not null)
            {
                pairKey = keyRemoteValue.InternalId;
            }
            else
            {
                pairKey = keyRemoteValue;
            }
        }

        return pairKey;
    }

    private RemoteValueDictionary ProcessMap(JArray mapArray, JsonSerializer serializer)
    {
        Dictionary<object, RemoteValue> remoteValueDictionary = new();
        foreach(var mapElementToken in mapArray)
        {
            if (mapElementToken is not JArray mapKeyValuePairArray)
            {
                throw new JsonSerializationException($"RemoteValue array element for dictionary must be an array");
            }

            if (mapKeyValuePairArray.Count != 2)
            {
                throw new JsonSerializationException($"RemoteValue array element for dictionary must be an array with two elements");
            }

            var keyToken = mapKeyValuePairArray[0];
            if (keyToken.Type != JTokenType.String && keyToken.Type != JTokenType.Object)
            {
                throw new JsonSerializationException($"RemoteValue array element for dictionary must have a first element (key) that is either a string or an object");
            }

            object pairKey = this.ProcessMapKey(keyToken, serializer);

            var valueToken = mapKeyValuePairArray[1];
            if (valueToken.Type != JTokenType.Object)
            {
                throw new JsonSerializationException($"RemoteValue array element for dictionary must have a second element (value) that is an object");
            }

            if (valueToken is not JObject valueObject)
            {
                // This should never be reached, but is here for the sake of completeness.
                throw new JsonSerializationException("RemoteValue array value token indicated object, but could not be cast to object");
            }

            var pairValue = this.ProcessObject(valueObject, serializer);
            remoteValueDictionary[pairKey] = pairValue;
        }

        return new RemoteValueDictionary(remoteValueDictionary);
    }

    private RemoteValueList ProcessList(JArray arrayObject, JsonSerializer serializer)
    {
        List<RemoteValue> remoteValueList = new();
        foreach (var arrayItem in arrayObject)
        {
            if (arrayItem is not JObject arrayItemObject)
            {
                throw new JsonSerializationException($"RemoteValue each element for list must be an object");
            }

            remoteValueList.Add(this.ProcessObject(arrayItemObject, serializer));
        }

        return new RemoteValueList(remoteValueList);
    }

    private static object ProcessNumber(JToken token)
    {
        if (token.Type == JTokenType.String)
        {
            string specialValue = token.Value<string>()!;
            if (specialValue == "Infinity")
            {
                return double.PositiveInfinity;
            }
            else if (specialValue == "-Infinity")
            {
                return double.NegativeInfinity;
            }
            else if (specialValue == "NaN")
            {
                return double.NaN;
            }
            else if (specialValue == "-0")
            {
                return decimal.Negate(decimal.Zero);
            }
            else
            {
                throw new JsonSerializationException($"RemoteValue invalid value '{specialValue}' for 'value' property of number");
            }
        }
        else if (token.Type == JTokenType.Integer)
        {
            return token.Value<long>();
        }
        else if (token.Type == JTokenType.Float)
        {
            return token.Value<double>();
        }
        else
        {
            throw new JsonSerializationException($"RemoteValue invalid type {token.Type} for 'value' property of number");
        }
    }
}