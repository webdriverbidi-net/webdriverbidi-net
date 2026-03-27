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
    /// Returns a value indicating whether this converter can convert the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to check for conversion compatibility.</param>
    /// <returns><see langword="true"/> if the type is <see cref="RemoteValue"/> or a derived type; otherwise, <see langword="false"/>.</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(RemoteValue).IsAssignableFrom(typeToConvert);
    }

    /// <summary>
    /// Deserializes the JSON string to a RemoteValue value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>A RemoteValue object as described by the JSON.</returns>
    /// <exception cref="JsonException">Thrown when invalid JSON is encountered.</exception>
    public override RemoteValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement rootElement = doc.RootElement;
        if (rootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException($"RemoteValue JSON must be an object, but was {rootElement.ValueKind}");
        }

        return this.ProcessObject(rootElement, options);
    }

    /// <summary>
    /// Serializes a RemoteValue object to a JSON string.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The RemoteValue to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    /// <exception cref="NotImplementedException">Thrown when called, as this converter is only used for deserialization.</exception>
    public override void Write(Utf8JsonWriter writer, RemoteValue value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    private static RemoteValue ProcessNumber(JsonElement token)
    {
        if (token.ValueKind == JsonValueKind.String)
        {
            string specialValue = token.GetString()!;
            if (specialValue == "Infinity")
            {
                return new DoubleRemoteValue(double.PositiveInfinity);
            }
            else if (specialValue == "-Infinity")
            {
                return new DoubleRemoteValue(double.NegativeInfinity);
            }
            else if (specialValue == "NaN")
            {
                return new DoubleRemoteValue(double.NaN);
            }
            else if (specialValue == "-0")
            {
                // Should be able to use double.NegativeZero, but this is not available
                // in .NET Standard 2.0.
                return new DoubleRemoteValue(-0.0);
            }

            throw new JsonException($"RemoteValue invalid value '{specialValue}' for 'value' property of number");
        }
        else if (token.ValueKind == JsonValueKind.Number)
        {
            if (token.TryGetInt64(out long longValue))
            {
                return new LongRemoteValue(longValue);
            }

            return new DoubleRemoteValue(token.GetDouble());
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
        string valueTypeString = typeToken.GetString()!;
        if (string.IsNullOrEmpty(valueTypeString))
        {
            throw new JsonException("RemoteValue must have a non-empty 'type' property that is a string");
        }

        if (!Enum.TryParse(valueTypeString, true, out RemoteValueType valueType))
        {
            throw new JsonException($"RemoteValue 'type' property value '{valueTypeString}' is not a valid RemoteValue type");
        }

        RemoteValue result;
        if (jsonObject.TryGetProperty("value", out JsonElement valueToken))
        {
            result = this.ProcessValue(valueType, valueToken, options);
        }
        else if (valueTypeString == "null")
        {
            result = new NullRemoteValue();
        }
        else if (valueTypeString == "undefined")
        {
            result = new UndefinedRemoteValue();
        }
        else
        {
            result = new ObjectReferenceRemoteValue(valueType);
        }

        if (jsonObject.TryGetProperty("handle", out JsonElement handleToken))
        {
            if (handleToken.ValueKind != JsonValueKind.String)
            {
                throw new JsonException($"RemoteValue 'handle' property, when present, must be a string");
            }

            string? handle = handleToken.GetString();
            switch (result)
            {
                case ObjectReferenceRemoteValue orv: orv.Handle = handle; break;
                case CollectionRemoteValue crv: crv.Handle = handle; break;
                case KeyValuePairCollectionRemoteValue kvpcrv: kvpcrv.Handle = handle; break;
                case NodeRemoteValue nrv: nrv.Handle = handle; break;
                case RegExpRemoteValue rerv: rerv.Handle = handle; break;
            }
        }

        if (jsonObject.TryGetProperty("internalId", out JsonElement internalIdToken))
        {
            if (internalIdToken.ValueKind != JsonValueKind.String)
            {
                throw new JsonException($"RemoteValue 'internalId' property, when present, must be a string");
            }

            string? internalId = internalIdToken.GetString();
            switch (result)
            {
                case ObjectReferenceRemoteValue orv: orv.InternalId = internalId; break;
                case CollectionRemoteValue crv: crv.InternalId = internalId; break;
                case KeyValuePairCollectionRemoteValue kvpcrv: kvpcrv.InternalId = internalId; break;
                case NodeRemoteValue nrv: nrv.InternalId = internalId; break;
                case RegExpRemoteValue rerv: rerv.InternalId = internalId; break;
            }
        }

        // The sharedId property is only valid for RemoteValue objects with type "node"
        if (result is NodeRemoteValue nodeRemoteValue && jsonObject.TryGetProperty("sharedId", out JsonElement sharedIdToken))
        {
            if (sharedIdToken.ValueKind != JsonValueKind.String)
            {
                throw new JsonException($"RemoteValue 'sharedId' property, when present, must be a string");
            }

            string? sharedId = sharedIdToken.GetString();
            nodeRemoteValue.SharedId = sharedId;
        }

        return result;
    }

    private RemoteValue ProcessValue(RemoteValueType valueType, JsonElement valueToken, JsonSerializerOptions options)
    {
        if (valueType == RemoteValueType.Boolean)
        {
            if (valueToken.ValueKind != JsonValueKind.True && valueToken.ValueKind != JsonValueKind.False)
            {
                throw new JsonException($"RemoteValue 'value' property for {valueType.ToString().ToLowerInvariant()} must be a boolean value");
            }

            bool boolValue = valueToken.GetBoolean();
            return new BooleanRemoteValue(boolValue);
        }

        if (valueType == RemoteValueType.Number)
        {
            return ProcessNumber(valueToken);
        }

        if (valueType == RemoteValueType.BigInt)
        {
            if (valueToken.ValueKind != JsonValueKind.String)
            {
                throw new JsonException($"RemoteValue for {valueType.ToString().ToLowerInvariant()} must have a non-null 'value' property whose value is a string");
            }

            string? bigintString = valueToken.GetString();
            if (!BigInteger.TryParse(bigintString, out BigInteger bigintValue))
            {
                throw new JsonException($"RemoteValue cannot parse invalid value '{bigintString}' for {valueType.ToString().ToLowerInvariant()}");
            }

            return new BigIntegerRemoteValue(bigintValue);
        }

        if (valueType == RemoteValueType.Date)
        {
            if (valueToken.ValueKind != JsonValueKind.String)
            {
                throw new JsonException($"RemoteValue for {valueType.ToString().ToLowerInvariant()} must have a non-null 'value' property whose value is a string");
            }

            string? dateString = valueToken.GetString();
            if (!DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime dateTimeValue))
            {
                throw new JsonException($"RemoteValue cannot parse invalid value '{dateString}' for {valueType.ToString().ToLowerInvariant()}");
            }

            return new DateRemoteValue(dateTimeValue);
        }

        if (valueType == RemoteValueType.RegExp)
        {
            if (valueToken.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"RemoteValue for {valueType.ToString().ToLowerInvariant()} must have a non-null 'value' property whose value is an object");
            }

            // Deserialize will properly throw if the value is not a RegularExpressionValue,
            // and therefore will never be null.
            RegularExpressionValue regexProperties = valueToken.Deserialize<RegularExpressionValue>(options)!;
            return new RegExpRemoteValue(regexProperties);
        }

        if (valueType == RemoteValueType.Node)
        {
            if (valueToken.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"RemoteValue for {valueType.ToString().ToLowerInvariant()} must have a non-null 'value' property whose value is an object");
            }

            // Deserialize will properly throw if the value is not a NodeProperties,
            // and therefore will never be null.
            NodeProperties nodeProperties = valueToken.Deserialize<NodeProperties>(options)!;
            return new NodeRemoteValue(nodeProperties);
        }

        if (valueType == RemoteValueType.Window)
        {
            if (valueToken.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"RemoteValue for {valueType.ToString().ToLowerInvariant()} must have a non-null 'value' property whose value is an object");
            }

            // Deserialize will properly throw if the value is not a WindowProxyProperties,
            // and therefore will never be null.
            WindowProxyProperties windowProxyProperties = valueToken.Deserialize<WindowProxyProperties>(options)!;
            return new WindowProxyRemoteValue(windowProxyProperties);
        }

        if (valueType == RemoteValueType.Array || valueType == RemoteValueType.Set || valueType == RemoteValueType.NodeList || valueType == RemoteValueType.HtmlCollection)
        {
            if (valueToken.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException($"RemoteValue for {valueType.ToString().ToLowerInvariant()} must have a non-null 'value' property whose value is an array");
            }

            return new CollectionRemoteValue(valueType, this.ProcessList(valueToken, options));
        }

        if (valueType == RemoteValueType.Map || valueType == RemoteValueType.Object)
        {
            if (valueToken.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException($"RemoteValue for {valueType.ToString().ToLowerInvariant()} must have a non-null 'value' property whose value is an array");
            }

            return new KeyValuePairCollectionRemoteValue(valueType, this.ProcessMap(valueToken, options));
        }

        // Remaining types with a value property must be a string.
        if (valueToken.ValueKind != JsonValueKind.String)
        {
            throw new JsonException($"RemoteValue 'value' property for {valueType.ToString().ToLowerInvariant()} must be a non-null string");
        }

        // Platform invariant: We have already validated that the token is a string,
        // so GetString cannot return null.
        string stringValue = valueToken.GetString()!;
        return new StringRemoteValue(stringValue);
    }

    private RemoteValueList ProcessList(JsonElement arrayObject, JsonSerializerOptions options)
    {
        List<RemoteValue> remoteValueList = [];
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
        Dictionary<object, RemoteValue> remoteValueDictionary = [];
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
            // The token type is already guaranteed to be a string, and
            // therefore cannot be null.
            pairKey = keyToken.GetString()!;
        }
        else
        {
            // Previous caller has already determined the value must be either
            // a string or object. We will use the null forgiving operator since
            // the token must be an object, and therefore the cast cannot return
            // null.
            RemoteValue keyRemoteValue = this.ProcessObject(keyToken, options);
            pairKey = keyRemoteValue;
        }

        return pairKey;
    }
}
