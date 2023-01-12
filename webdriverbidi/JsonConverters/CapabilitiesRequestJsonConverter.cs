// <copyright file="CapabilitiesRequestJsonConverter.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.JsonConverters;

using Newtonsoft.Json;
using WebDriverBidi.Session;

/// <summary>
/// The JSON converter for the CapabilitiesRequest object.
/// </summary>
public class CapabilitiesRequestJsonConverter : JsonConverter<CapabilitiesRequest>
{
    /// <summary>
    /// Gets a value indicating whether this converter can read JSON values.
    /// Returns false for this converter (converter not used for
    /// deserialization).
    /// </summary>
    public override bool CanRead => false;

    /// <summary>
    /// Gets a value indicating whether this converter can write JSON values.
    /// Returns false for this converter (converter used for serialization).
    /// </summary>
    public override bool CanWrite => true;

    /// <summary>
    /// Reads a JSON string and deserializes it to an object.
    /// </summary>
    /// <param name="reader">The JSON reader to use during deserialization.</param>
    /// <param name="objectType">The type of object to which to deserialize.</param>
    /// <param name="existingValue">The existing value of the object.</param>
    /// <param name="hasExistingValue">A value indicating whether the existing value is null.</param>
    /// <param name="serializer">The JSON serializer to use in deserialization.</param>
    /// <returns>The deserialized object created from JSON.</returns>
    public override CapabilitiesRequest ReadJson(JsonReader reader, Type objectType, CapabilitiesRequest? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Serializes an object and writes it to a JSON string.
    /// </summary>
    /// <param name="writer">The JSON writer to use during serialization.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializer">The JSON serializer to use in serialization.</param>
    public override void WriteJson(JsonWriter writer, CapabilitiesRequest? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            throw new JsonSerializationException("Cannot serialize null value");
        }

        writer.WriteStartObject();
        if (value.BrowserName is not null)
        {
            writer.WritePropertyName("browserName");
            writer.WriteValue(value.BrowserName);
        }

        if (value.BrowserVersion is not null)
        {
            writer.WritePropertyName("browserVersion");
            writer.WriteValue(value.BrowserVersion);
        }

        if (value.PlatformName is not null)
        {
            writer.WritePropertyName("platformName");
            writer.WriteValue(value.PlatformName);
        }

        if (value.AcceptInsecureCertificates is not null)
        {
            writer.WritePropertyName("acceptInsecureCerts");
            writer.WriteValue(value.AcceptInsecureCertificates);
        }

        if (value.Proxy is not null)
        {
            writer.WritePropertyName("proxy");
            serializer.Serialize(writer, value.Proxy);
        }

        foreach (var additionalProperty in value.AdditionalCapabilities)
        {
            // TODO: Validation of serializable values in additional
            // capabilites.
            writer.WritePropertyName(additionalProperty.Key);
            serializer.Serialize(writer, additionalProperty.Value);
        }

        writer.WriteEndObject();
    }
}
