// <copyright file="BrowsingContextPartitionDescriptor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Storage;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing a descriptor for a partition key for a browser cookie using a browsing context.
/// </summary>
public class BrowsingContextPartitionDescriptor : PartitionDescriptor
{
    private readonly string type = "context";

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowsingContextPartitionDescriptor"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for this partition key descriptor.</param>
    public BrowsingContextPartitionDescriptor(string browsingContextId)
        : base()
    {
        this.BrowsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the type of the partition key descriptor.
    /// </summary>
    [JsonPropertyName("type")]
    public override string Type => this.type;

    /// <summary>
    /// Gets or sets the ID of the browsing context for this partition key descriptor.
    /// </summary>
    [JsonPropertyName("context")]
    public string BrowsingContextId { get; set; }
}
