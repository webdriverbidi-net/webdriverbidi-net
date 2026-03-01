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
/// This class is not thread-safe. Instances are created and consumed
/// on the single message-processing loop and should not be shared
/// across threads.
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
    /// Gets a read-only dictionary of additional properties deserialized with this message.
    /// The conversion from the raw JSON extension data is performed lazily on first access.
    /// This property is not thread-safe; however, in practice the owning <see cref="Message"/>
    /// is only accessed on the single message-processing loop, so concurrent access does not occur.
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
    /// Gets or sets additional properties deserialized with this message.
    /// </summary>
    [JsonExtensionData]
    [JsonInclude]
    internal Dictionary<string, JsonElement> SerializableAdditionalData { get; set; } = [];
}
