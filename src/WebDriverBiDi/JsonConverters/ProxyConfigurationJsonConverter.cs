// <copyright file="ProxyConfigurationJsonConverter.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Session;

/// <summary>
/// The JSON converter for the ProxyConfiguration object.
/// </summary>
public class ProxyConfigurationJsonConverter : JsonConverter<ProxyConfiguration>
{
    /// <summary>
    /// Deserializes the JSON string to an ProxyConfiguration value.
    /// </summary>
    /// <param name="reader">A Utf8JsonReader used to read the incoming JSON.</param>
    /// <param name="typeToConvert">The Type description of the type to convert.</param>
    /// <param name="options">The JsonSerializationOptions used for deserializing the JSON.</param>
    /// <returns>An subclass of a ProxyConfiguration object as described by the JSON.</returns>
    /// <exception cref="JsonException">Thrown when invalid JSON is encountered.</exception>
    public override ProxyConfiguration? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement rootElement = doc.RootElement;
        if (rootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException($"Proxy JSON must be an object, but was {rootElement.ValueKind}");
        }

        if (!rootElement.TryGetProperty("proxyType", out JsonElement typeElement))
        {
            // TODO (Issue #19): Uncomment the throw statement and remove the return statement
            // once https://bugzilla.mozilla.org/show_bug.cgi?id=1916463 is fixed.
            // throw new JsonException("Proxy response must have a 'proxyType' property");
            return null;
        }

        if (typeElement.ValueKind != JsonValueKind.String)
        {
            throw new JsonException("Proxy response 'proxyType' property must be a string");
        }

        // We have previously determined that the token exists and is a string, and must
        // contain a value, so therefore cannot be null. Moreover, if the value is not a
        // valid proxy type, this deserialization will throw.
        ProxyConfiguration? config = null;
        ProxyType proxyType = typeElement.Deserialize<ProxyType>();
        List<string> propertyNames = ["proxyType"];
        if (proxyType == ProxyType.AutoDetect)
        {
            config = rootElement.Deserialize<AutoDetectProxyConfiguration>();
        }
        else if (proxyType == ProxyType.Direct)
        {
            config = rootElement.Deserialize<DirectProxyConfiguration>();
        }
        else if (proxyType == ProxyType.Manual)
        {
            config = rootElement.Deserialize<ManualProxyConfiguration>();
            propertyNames.Add("httpProxy");
            propertyNames.Add("sslProxy");
            propertyNames.Add("socksProxy");
            propertyNames.Add("socksVersion");
            propertyNames.Add("noProxy");
        }
        else if (proxyType == ProxyType.ProxyAutoConfig)
        {
            config = rootElement.Deserialize<PacProxyConfiguration>();
            propertyNames.Add("proxyAutoconfigUrl");
        }
        else
        {
            config = rootElement.Deserialize<SystemProxyConfiguration>();
        }

        if (config is not null)
        {
            foreach (JsonProperty property in rootElement.EnumerateObject())
            {
                if (!propertyNames.Contains(property.Name))
                {
                    config.AdditionalData[property.Name] = property.Value;
                }
            }
        }

        return config;
    }

    /// <summary>
    /// Serializes an ProxyConfiguration object to a JSON string.
    /// </summary>
    /// <param name="writer">A Utf8JsonWriter used to write the JSON string.</param>
    /// <param name="value">The value to be serialized.</param>
    /// <param name="options">The JsonSerializationOptions used for serializing the object.</param>
    public override void Write(Utf8JsonWriter writer, ProxyConfiguration value, JsonSerializerOptions options)
    {
        string json = JsonSerializer.Serialize(value, value.GetType(), options);
        writer.WriteRawValue(json);
        writer.Flush();
    }
}
