// <copyright file="Message.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Internal;

/// <summary>
/// Object containing data about a WebDriver Bidi message.
/// </summary>
public class Message
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Message"/> class.
    /// </summary>
    public Message()
    {
        this.AdditionalData = ReceivedDataDictionary.EmptyDictionary;
    }

    /// <summary>
    /// Gets the type of message.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("type")]
    [JsonInclude]
    public string Type { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets read-only dictionary of additional properties deserialized with this message.
    /// </summary>
    [JsonIgnore]
    public ReceivedDataDictionary AdditionalData
    {
        get
        {
            if (this.SerializableAdditionalData.Count > 0 && field.Count == 0)
            {
                field = JsonConverterUtilities.ConvertIncomingExtensionData(this.SerializableAdditionalData);
            }

            return field;
        }
    }

    /// <summary>
    /// Gets additional properties deserialized with this message.
    /// </summary>
    [JsonExtensionData]
    [JsonInclude]
    internal Dictionary<string, JsonElement> SerializableAdditionalData { get; set; } = [];
}
