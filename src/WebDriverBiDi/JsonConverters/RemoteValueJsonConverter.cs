// <copyright file="RemoteValueJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    // public override bool CanRead => true;

    /// <summary>
    /// Serializes an object and writes it to a JSON string.
    /// </summary>
    /// <param name="writer">The JSON writer to use during serialization.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializer">The JSON serializer to use in serialization.</param>
    // public override void WriteJson(JsonWriter writer, RemoteValue? value, JsonSerializer serializer)
    // {
    //     throw new NotImplementedException();
    // }

    /// <summary>
    /// Reads a JSON string and deserializes it to an object.
    /// </summary>
    /// <param name="reader">The JSON reader to use during deserialization.</param>
    /// <param name="objectType">The type of object to which to deserialize.</param>
    /// <param name="existingValue">The existing value of the object.</param>
    /// <param name="hasExistingValue">A value indicating whether the existing value is null.</param>
    /// <param name="serializer">The JSON serializer to use in deserialization.</param>
    /// <returns>The deserialized object created from JSON.</returns>
    // public override RemoteValue ReadJson(JsonReader reader, Type objectType, RemoteValue? existingValue, bool hasExistingValue, JsonSerializer serializer)
    // {
    //     reader.DateParseHandling = DateParseHandling.None;
    //     JObject jsonObject = JObject.Load(reader);

    //     return this.ProcessObject(jsonObject, serializer);
    // }

    public override void Write(Utf8JsonWriter writer, RemoteValue value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override RemoteValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument doc = JsonDocument.ParseValue(ref reader);
        if (doc.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("RemoteValue must be an object");
        }

        return this.ProcessObject(doc.RootElement);
    }

    private RemoteValue ProcessObject(JsonElement jsonObject)
    {
        if (!jsonObject.TryGetProperty("type", out JsonElement typeToken))
        {
            throw new JsonException("RemoteValue must contain a 'type' property");
        }

        if (typeToken.ValueKind != JsonValueKind.String)
        {
            throw new JsonException("RemoteValue type property must be a string");
        }

        // We have previously determined that the token is a string, and must
        // contain a value, so therefore cannot be null.
        string valueTypeString = typeToken.GetString();
        if (string.IsNullOrEmpty(valueTypeString))
        {
            throw new JsonException("RemoteValue must have a non-empty 'type' property that is a string");
        }

        if (!RemoteValue.IsValidRemoteValueType(valueTypeString))
        {
            throw new JsonException($"RemoteValue 'type' property value '{valueTypeString}' is not a valid RemoteValue type");
        }

        RemoteValue result = new(valueTypeString);
        if (jsonObject.TryGetProperty("value", out JsonElement valueToken))
        {
            this.ProcessValue(result, valueTypeString, valueToken);
        }

        if (jsonObject.TryGetProperty("handle", out JsonElement handleToken))
        {
            if (handleToken.ValueKind != JsonValueKind.String)
            {
                throw new JsonException($"RemoteValue 'handle' property, when present, must be a string");
            }

            string? handle = handleToken.GetString();
            result.Handle = handle;
        }

        if (jsonObject.TryGetProperty("internalId", out JsonElement internalIdToken))
        {
            if (!internalIdToken.TryGetUInt64(out ulong internalId))
            {
                throw new JsonException($"RemoteValue 'internalId' property, when present, must be an unsigned integer");
            }

            result.InternalId = internalId;
        }

        // The sharedId property is only valid for RemoteValue objects with type "node"
        if (result.Type == "node" && jsonObject.TryGetProperty("sharedId", out JsonElement sharedIdToken))
        {
            if (sharedIdToken.ValueKind != JsonValueKind.String)
            {
                throw new JsonException($"RemoteValue 'sharedId' property, when present, must be a string");
            }

            string? sharedId = sharedIdToken.GetString();
            result.SharedId = sharedId;
        }

        return result;        
    }

    private void ProcessValue(RemoteValue result, string valueType, JsonElement valueToken)
    {
        if (valueType == "string")
        {
            if (valueToken.ValueKind != JsonValueKind.String)
            {
                throw new JsonException($"RemoteValue 'value' property for {valueType} must be a non-null string");
            }

            string? stringValue = valueToken.GetString();
            result.Value = stringValue;
        }

        if (valueType == "boolean")
        {
            if (valueToken.ValueKind != JsonValueKind.True && valueToken.ValueKind != JsonValueKind.False)
            {
                throw new JsonException($"RemoteValue 'value' property for {valueType} must be a boolean value");
            }

            bool boolValue = valueToken.GetBoolean();
            result.Value = boolValue;
        }

        if (valueType == "number")
        {
            result.Value = ProcessNumber(valueToken);
        }

        if (valueType == "bigint")
        {
            if (valueToken.ValueKind != JsonValueKind.String)
            {
                throw new JsonException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is a string");
            }

            string? bigintString = valueToken.GetString();
            if (!BigInteger.TryParse(bigintString, out BigInteger bigintValue))
            {
                throw new JsonException($"RemoteValue cannot parse invalid value '{bigintString}' for {valueType}");
            }

            result.Value = bigintValue;
        }

        if (valueType == "date")
        {
            if (valueToken.ValueKind != JsonValueKind.String)
            {
                throw new JsonException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is a string");
            }

            string? dateString = valueToken.GetString();
            if (!DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime dateTimeValue))
            {
                throw new JsonException($"RemoteValue cannot parse invalid value '{dateString}' for {valueType}");
            }

            result.Value = dateTimeValue;
        }

        if (valueType == "regexp")
        {
            if (valueToken.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an object");
            }

            RegularExpressionValue regexProperties = valueToken.Deserialize<RegularExpressionValue>();
            result.Value = regexProperties;
        }

        if (valueType == "node")
        {
            if (valueToken.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an object");
            }

            NodeProperties nodeProperties = valueToken.Deserialize<NodeProperties>();
            result.Value = nodeProperties;
        }

        if (valueType == "window")
        {
            if (valueToken.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an object");
            }

            WindowProxyProperties windowProxyProperties = valueToken.Deserialize<WindowProxyProperties>();
            result.Value = windowProxyProperties;
        }

        if (valueType == "array" || valueType == "set" || valueType == "nodelist" || valueType == "htmlcollection")
        {
            if (valueToken.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an array");
            }

            result.Value = this.ProcessList(valueToken);
        }

        if (valueType == "map" || valueType == "object")
        {
            if (valueToken.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an array");
            }

            result.Value = this.ProcessMap(valueToken);
        }
    }

    private static object ProcessNumber(JsonElement token)
    {

        if (token.ValueKind == JsonValueKind.String)
        {
            string specialValue = token.GetString()!;
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

            throw new JsonException($"RemoteValue invalid value '{specialValue}' for 'value' property of number");
        }
        else if (token.ValueKind == JsonValueKind.Number)
        {
            if (token.TryGetInt64(out long longValue))
            {
                return longValue;
            }
            else if (token.TryGetDouble(out double doubleValue))
            {
                return doubleValue;
            }

            throw new JsonException($"Remote value could not be parsed as either long or double");
        }
        else
        {
            string tokenKind = token.ValueKind == JsonValueKind.True || token.ValueKind == JsonValueKind.False ? "Boolean" : token.ValueKind.ToString();
            throw new JsonException($"RemoteValue invalid type {tokenKind} for 'value' property of number");
        }
    }

    private RemoteValueList ProcessList(JsonElement arrayObject)
    {
        List<RemoteValue> remoteValueList = new();
        foreach (JsonElement arrayItem in arrayObject.EnumerateArray())
        {
            if (arrayItem.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"RemoteValue each element for list must be an object");
            }

            remoteValueList.Add(this.ProcessObject(arrayItem));
        }

        return new RemoteValueList(remoteValueList);
    }

    private RemoteValueDictionary ProcessMap(JsonElement mapArray)
    {
        Dictionary<object, RemoteValue> remoteValueDictionary = new();
        foreach (JsonElement mapElementToken in mapArray.EnumerateArray())
        {
            if (mapElementToken.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException($"RemoteValue array element for dictionary must be an array");
            }

            if (mapElementToken.GetArrayLength() != 2)
            {
                throw new JsonException($"RemoteValue array element for dictionary must be an array with two elements");
            }

            JsonElement keyToken = mapElementToken[0];
            if (keyToken.ValueKind != JsonValueKind.String && keyToken.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"RemoteValue array element for dictionary must have a first element (key) that is either a string or an object");
            }

            object pairKey = this.ProcessMapKey(keyToken);

            JsonElement valueToken = mapElementToken[1];
            if (valueToken.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"RemoteValue array element for dictionary must have a second element (value) that is an object");
            }

            RemoteValue pairValue = this.ProcessObject(valueToken);
            remoteValueDictionary[pairKey] = pairValue;
        }

        return new RemoteValueDictionary(remoteValueDictionary);
    }

    private object ProcessMapKey(JsonElement keyToken)
    {
        object pairKey;
        if (keyToken.ValueKind == JsonValueKind.String)
        {
            pairKey = keyToken.GetString();
        }
        else
        {
            // Previous caller has already determined the value must be either
            // a string or object. We will use the null forgiving operator since
            // the token must be an object, and therefore the cast cannot return
            // null.
            RemoteValue keyRemoteValue = this.ProcessObject(keyToken);
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
}