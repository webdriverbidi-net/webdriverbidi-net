// <copyright file="RemoteValueJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Globalization;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Script;

/// <summary>
/// The JSON converter for the RemoteValue object.
/// </summary>
public class RemoteValueJsonConverter : JsonConverter<RemoteValue>
{
    /// <summary>
    /// Gets a value indicating whether this converter can read JSON values.
    /// Returns true for this converter (converter used for deserialization
    /// only).
    /// </summary>
    public override bool CanRead => true;

    /// <summary>
    /// Serializes an object and writes it to a JSON string.
    /// </summary>
    /// <param name="writer">The JSON writer to use during serialization.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializer">The JSON serializer to use in serialization.</param>
    public override void WriteJson(JsonWriter writer, RemoteValue? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Reads a JSON string and deserializes it to an object.
    /// </summary>
    /// <param name="reader">The JSON reader to use during deserialization.</param>
    /// <param name="objectType">The type of object to which to deserialize.</param>
    /// <param name="existingValue">The existing value of the object.</param>
    /// <param name="hasExistingValue">A value indicating whether the existing value is null.</param>
    /// <param name="serializer">The JSON serializer to use in deserialization.</param>
    /// <returns>The deserialized object created from JSON.</returns>
    public override RemoteValue ReadJson(JsonReader reader, Type objectType, RemoteValue? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        reader.DateParseHandling = DateParseHandling.None;
        JObject jsonObject = JObject.Load(reader);

        return this.ProcessObject(jsonObject, serializer);
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

    private RemoteValue ProcessObject(JObject jsonObject, JsonSerializer serializer)
    {
        if (!jsonObject.TryGetValue("type", out JToken? typeToken))
        {
            throw new JsonSerializationException("RemoteValue must contain a 'type' property");
        }

        if (typeToken.Type != JTokenType.String)
        {
            throw new JsonSerializationException("RemoteValue type property must be a string");
        }

        // We have previously determined that the token is a string, and must
        // contain a value, so therefore cannot be null.
        string valueTypeString = jsonObject["type"]!.Value<string>()!;
        if (string.IsNullOrEmpty(valueTypeString))
        {
            throw new JsonSerializationException("RemoteValue must have a non-empty 'type' property that is a string");
        }

        if (!RemoteValue.IsValidRemoteValueType(valueTypeString))
        {
            throw new JsonSerializationException($"RemoteValue 'type' property value '{valueTypeString}' is not a valid RemoteValue type");
        }

        RemoteValue result = new(valueTypeString);
        if (jsonObject.TryGetValue("value", out JToken? valueToken))
        {
            this.ProcessValue(result, valueTypeString, valueToken, serializer);
        }

        if (jsonObject.TryGetValue("handle", out JToken? handleToken))
        {
            if (handleToken.Type != JTokenType.String)
            {
                throw new JsonSerializationException($"RemoteValue 'handle' property, when present, must be a string");
            }

            string? handle = handleToken.Value<string>();
            result.Handle = handle;
        }

        if (jsonObject.TryGetValue("internalId", out JToken? internalIdToken))
        {
            if (internalIdToken.Type != JTokenType.Integer)
            {
                throw new JsonSerializationException($"RemoteValue 'internalId' property, when present, must be an unsigned integer");
            }

            ulong internalId = internalIdToken.Value<ulong>();
            result.InternalId = internalId;
        }

        // The sharedId property is only valid for RemoteValue objects with type "node"
        if (result.Type == "node" && jsonObject.TryGetValue("sharedId", out JToken? sharedIdToken))
        {
            if (sharedIdToken.Type != JTokenType.String)
            {
                throw new JsonSerializationException($"RemoteValue 'sharedId' property, when present, must be a string");
            }

            string? sharedId = sharedIdToken.Value<string>();
            result.SharedId = sharedId;
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

            string? stringValue = token.Value<string>();
            result.Value = stringValue;
        }

        if (valueType == "boolean")
        {
            if (token.Type != JTokenType.Boolean)
            {
                throw new JsonSerializationException($"RemoteValue 'value' property for {valueType} must be a boolean value");
            }

            bool? boolValue = token.Value<bool>();
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
            if (!DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime dateTimeValue))
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

            RegularExpressionValue regexProperties = new(string.Empty);
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

        if (valueType == "window")
        {
            if (token is not JObject windowProxyObject)
            {
                throw new JsonSerializationException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an object");
            }

            WindowProxyProperties windowProxyProperties = new();
            serializer.Populate(windowProxyObject.CreateReader(), windowProxyProperties);
            result.Value = windowProxyProperties;
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
            // Note: Use the null forgiving operator here to suppress
            // the compiler warning, but the value should never be
            // null.
            pairKey = keyToken.Value<string>()!;
        }
        else
        {
            // Previous caller has already determined the value must be either
            // a string or object. We will use the null forgiving operator since
            // the token must be an object, and therefore the cast cannot return
            // null.
            JObject? keyObject = keyToken as JObject;
            RemoteValue keyRemoteValue = this.ProcessObject(keyObject!, serializer);
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
        foreach (JToken mapElementToken in mapArray)
        {
            if (mapElementToken is not JArray mapKeyValuePairArray)
            {
                throw new JsonSerializationException($"RemoteValue array element for dictionary must be an array");
            }

            if (mapKeyValuePairArray.Count != 2)
            {
                throw new JsonSerializationException($"RemoteValue array element for dictionary must be an array with two elements");
            }

            JToken keyToken = mapKeyValuePairArray[0];
            if (keyToken.Type != JTokenType.String && keyToken.Type != JTokenType.Object)
            {
                throw new JsonSerializationException($"RemoteValue array element for dictionary must have a first element (key) that is either a string or an object");
            }

            object pairKey = this.ProcessMapKey(keyToken, serializer);

            JToken valueToken = mapKeyValuePairArray[1];
            if (valueToken.Type != JTokenType.Object)
            {
                throw new JsonSerializationException($"RemoteValue array element for dictionary must have a second element (value) that is an object");
            }

            // Since valueToken is of type object, it must be able to be cast
            // to JObject. Use the null forgiveness operator to suppress the
            // compiler warning.
            JObject? valueObject = valueToken as JObject;
            RemoteValue pairValue = this.ProcessObject(valueObject!, serializer);
            remoteValueDictionary[pairKey] = pairValue;
        }

        return new RemoteValueDictionary(remoteValueDictionary);
    }

    private RemoteValueList ProcessList(JArray arrayObject, JsonSerializer serializer)
    {
        List<RemoteValue> remoteValueList = new();
        foreach (JToken arrayItem in arrayObject)
        {
            if (arrayItem is not JObject arrayItemObject)
            {
                throw new JsonSerializationException($"RemoteValue each element for list must be an object");
            }

            remoteValueList.Add(this.ProcessObject(arrayItemObject, serializer));
        }

        return new RemoteValueList(remoteValueList);
    }
}
