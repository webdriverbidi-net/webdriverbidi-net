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
    public string? UserContextId { get; set; }

    /// <summary>
    /// Gets or sets the source origin for this partition key descriptor.
    /// </summary>
    [JsonPropertyName("sourceOrigin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SourceOrigin { get; set; }

    /// <summary>
    /// Gets the dictionary containing additional data associated with this partition key descriptor.
    /// </summary>
    /// <remarks>
    /// An entry may not reuse the name of a property this type already serializes; doing so would write that name
    /// twice in the same JSON object, and a JSON object with a duplicate name has no defined meaning. Sending a
    /// command that contains such an entry throws <see cref="WebDriverBiDiSerializationException"/> rather than
    /// emitting the ambiguous payload. Set the typed property instead.
    /// </remarks>
    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalData { get; } = [];
}
