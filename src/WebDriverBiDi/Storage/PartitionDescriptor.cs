// <copyright file="PartitionDescriptor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Storage;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing a descriptor for a partition key for a browser cookie using a browsing context.
/// </summary>
[JsonDerivedType(typeof(BrowsingContextPartitionDescriptor))]
[JsonDerivedType(typeof(StorageKeyPartitionDescriptor))]
public abstract class PartitionDescriptor
{
    /// <summary>
    /// Gets the type of the partition key descriptor.
    /// </summary>
    [JsonPropertyName("type")]
    public abstract string Type { get; }
}
