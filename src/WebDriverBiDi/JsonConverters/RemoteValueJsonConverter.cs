// <copyright file="RemoteValueJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

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
    /// Deserializes the JSON string to an RemoteValue value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>A RemoteValue object as described by the JSON.</returns>
    /// <exception cref="JsonException">Thrown when invalid JSON is encountered.</exception>
    public override RemoteValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument doc = JsonDocument.ParseValue(ref reader);
        if (doc.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("RemoteValue must be an object");
        }

        return this.ProcessObject(doc.RootElement, options);
    }

    /// <summary>
    /// Serializes a RemoteValue object to a JSON string.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The Command to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    /// <exception cref="NotImplementedException">Thrown when called, as this converter is only used for deserialization.</exception>
    public override void Write(Utf8JsonWriter writer, RemoteValue value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
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

    private RemoteValue ProcessObject(JsonElement jsonObject, JsonSerializerOptions options)
    {
        if (!jsonObject.TryGetProperty("type", out JsonElement typeToken))
        {
            throw new JsonException("RemoteValue must contain a 'type' property");
        }

        if (typeToken.ValueKind != JsonValueKind.String)
        {
            throw new JsonException("RemoteValue type property must be a string");
        }

        // We have previously determined that the token exists and is a string, and must
        // contain a value, so therefore cannot be null.
        string valueTypeString = typeToken!.GetString()!;
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
            this.ProcessValue(result, valueTypeString, valueToken, options);
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

    private void ProcessValue(RemoteValue result, string valueType, JsonElement valueToken, JsonSerializerOptions options)
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

            RegularExpressionValue regexProperties = valueToken.Deserialize<RegularExpressionValue>(options);
            result.Value = regexProperties;
        }

        if (valueType == "node")
        {
            if (valueToken.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an object");
            }

            NodeProperties nodeProperties = valueToken.Deserialize<NodeProperties>(options);
            result.Value = nodeProperties;
        }

        if (valueType == "window")
        {
            if (valueToken.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an object");
            }

            WindowProxyProperties windowProxyProperties = valueToken.Deserialize<WindowProxyProperties>(options);
            result.Value = windowProxyProperties;
        }

        if (valueType == "array" || valueType == "set" || valueType == "nodelist" || valueType == "htmlcollection")
        {
            if (valueToken.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an array");
            }

            result.Value = this.ProcessList(valueToken, options);
        }

        if (valueType == "map" || valueType == "object")
        {
            if (valueToken.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException($"RemoteValue for {valueType} must have a non-null 'value' property whose value is an array");
            }

            result.Value = this.ProcessMap(valueToken, options);
        }
    }

    private RemoteValueList ProcessList(JsonElement arrayObject, JsonSerializerOptions options)
    {
        List<RemoteValue> remoteValueList = new();
        foreach (JsonElement arrayItem in arrayObject.EnumerateArray())
        {
            if (arrayItem.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"RemoteValue each element for list must be an object");
            }

            remoteValueList.Add(this.ProcessObject(arrayItem, options));
        }

        return new RemoteValueList(remoteValueList);
    }

    private RemoteValueDictionary ProcessMap(JsonElement mapArray, JsonSerializerOptions options)
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

            object pairKey = this.ProcessMapKey(keyToken, options);

            JsonElement valueToken = mapElementToken[1];
            if (valueToken.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"RemoteValue array element for dictionary must have a second element (value) that is an object");
            }

            RemoteValue pairValue = this.ProcessObject(valueToken, options);
            remoteValueDictionary[pairKey] = pairValue;
        }

        return new RemoteValueDictionary(remoteValueDictionary);
    }

    private object ProcessMapKey(JsonElement keyToken, JsonSerializerOptions options)
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
            RemoteValue keyRemoteValue = this.ProcessObject(keyToken, options);
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
