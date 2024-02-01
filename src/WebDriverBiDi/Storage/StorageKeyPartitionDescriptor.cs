// <copyright file="StorageKeyPartitionDescriptor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Storage;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing a descriptor for a partition key for a browser cookie using a storage key.
/// </summary>
public class StorageKeyPartitionDescriptor : PartitionDescriptor
{
    private readonly string type = "storageKey";
    private readonly Dictionary<string, object?> additionalData = new();
    private string? userContextId;
    private string? sourceOrigin;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageKeyPartitionDescriptor"/> class.
    /// </summary>
    public StorageKeyPartitionDescriptor()
        : base()
    {
    }

    /// <summary>
    /// Gets the type of the partition key descriptor.
    /// </summary>
    [JsonPropertyName("type")]
    public override string Type => this.type;

    /// <summary>
    /// Gets or sets the ID of the user context for this partition key descriptor.
    /// </summary>
    [JsonPropertyName("userContext")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UserContextId { get => this.userContextId; set => this.userContextId = value; }

    /// <summary>
    /// Gets or sets the source origin for this partition key descriptor.
    /// </summary>
    [JsonPropertyName("sourceOrigin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SourceOrigin { get => this.sourceOrigin; set => this.sourceOrigin = value; }

    /// <summary>
    /// Gets the dictionary containing additional data associated with this partition key descriptor.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalData => this.additionalData;
}