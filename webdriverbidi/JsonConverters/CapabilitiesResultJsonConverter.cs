// <copyright file="CapabilitiesResultJsonConverter.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.JsonConverters;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebDriverBidi.Session;

/// <summary>
/// The JSON converter for the CapabilitiesResult object.
/// </summary>
public class CapabilitiesResultJsonConverter : JsonConverter<CapabilitiesResult>
{
    private static readonly List<string> KnownCapabilityNames = new()
    {
        "acceptInsecureCerts",
        "browserName",
        "browserVersion",
        "platformName",
        "proxy",
        "setWindowRect",
    };

    /// <summary>
    /// Gets a value indicating whether this converter can read JSON values.
    /// Returns false for this converter (converter used for serialization
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
    /// Reads a JSON string and deserializes it to an object.
    /// </summary>
    /// <param name="reader">The JSON reader to use during deserialization.</param>
    /// <param name="objectType">The type of object to which to deserialize.</param>
    /// <param name="existingValue">The existing value of the object.</param>
    /// <param name="hasExistingValue">A value indicating whether the existing value is null.</param>
    /// <param name="serializer">The JSON serializer to use in deserialization.</param>
    /// <returns>The deserialized object created from JSON.</returns>
    public override CapabilitiesResult ReadJson(JsonReader reader, Type objectType, CapabilitiesResult? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        CapabilitiesResult result = new();
        serializer.Populate(jsonObject.CreateReader(), result);
        Dictionary<string, object?> additionalCapabilities = new();
        foreach (var token in jsonObject)
        {
            if (!KnownCapabilityNames.Contains(token.Key))
            {
                if (token.Value is null)
                {
                    additionalCapabilities[token.Key] = null;
                }
                else
                {
                    additionalCapabilities[token.Key] = serializer.Deserialize(token.Value.CreateReader());
                }
            }
        }

        if (additionalCapabilities.Count > 0)
        {
            result.AdditionalCapabilities = new AdditionalCapabilities(additionalCapabilities);
        }

        return result;
    }

    /// <summary>
    /// Serializes an object and writes it to a JSON string.
    /// </summary>
    /// <param name="writer">The JSON writer to use during serialization.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializer">The JSON serializer to use in serialization.</param>
    public override void WriteJson(JsonWriter writer, CapabilitiesResult? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
