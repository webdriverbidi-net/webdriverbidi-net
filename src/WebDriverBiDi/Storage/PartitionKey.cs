// <copyright file="PartitionKey.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Storage;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Internal;

/// <summary>
/// Object containing data about a partition key for a browser cookie.
/// </summary>
public record PartitionKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PartitionKey"/> class.
    /// </summary>
    [JsonConstructor]
    internal PartitionKey()
    {
        this.AdditionalData = ReceivedDataDictionary.EmptyDictionary;
    }

    /// <summary>
    /// Gets the ID of the user context of the cookie partition key.
    /// </summary>
    [JsonPropertyName("userContext")]
    [JsonInclude]
    public string? UserContextId { get; internal set; }

    /// <summary>
    /// Gets the source origin of the cookie partition key.
    /// </summary>
    [JsonPropertyName("sourceOrigin")]
    [JsonInclude]
    public string? SourceOrigin { get; internal set; }

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
