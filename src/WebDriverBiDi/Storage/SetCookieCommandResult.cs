// <copyright file="SetCookieCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Storage;

using System.Text.Json.Serialization;

/// <summary>
/// Result for setting a cookie using the storage.setCookie command.
/// </summary>
public record SetCookieCommandResult : CommandResult
{
    private PartitionKey partitionKey = new();

    [JsonConstructor]
    private SetCookieCommandResult()
    {
    }

    /// <summary>
    /// Gets the partition key for the list of returned cookies.
    /// </summary>
    [JsonPropertyName("partitionKey")]
    [JsonRequired]
    [JsonInclude]
    public PartitionKey PartitionKey { get => this.partitionKey; private set => this.partitionKey = value; }
}