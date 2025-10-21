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
    private string? userContextId;
    private string? sourceOrigin;
    private Dictionary<string, JsonElement> writableAdditionalData = new();
    private ReceivedDataDictionary additionalData = ReceivedDataDictionary.EmptyDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartitionKey"/> class.
    /// </summary>
    [JsonConstructor]
    internal PartitionKey()
    {
    }

    /// <summary>
    /// Gets the ID of the user context of the cookie partition key.
    /// </summary>
    [JsonPropertyName("userContext")]
    [JsonInclude]
    public string? UserContextId { get => this.userContextId; private set => this.userContextId = value; }

    /// <summary>
    /// Gets the source origin of the cookie partition key.
    /// </summary>
    [JsonPropertyName("sourceOrigin")]
    [JsonInclude]
    public string? SourceOrigin { get => this.sourceOrigin; private set => this.sourceOrigin = value; }

    /// <summary>
    /// Gets read-only dictionary of additional properties deserialized with this message.
    /// </summary>
    [JsonIgnore]
    public ReceivedDataDictionary AdditionalData
    {
        get
        {
            if (this.writableAdditionalData.Count > 0 && this.additionalData.Count == 0)
            {
                this.additionalData = JsonConverterUtilities.ConvertIncomingExtensionData(this.writableAdditionalData);
            }

            return this.additionalData;
        }
    }

    /// <summary>
    /// Gets additional properties deserialized with this message.
    /// </summary>
    [JsonExtensionData]
    [JsonInclude]
    internal Dictionary<string, JsonElement> SerializableAdditionalData { get => this.writableAdditionalData; private set => this.writableAdditionalData = value; }
}
