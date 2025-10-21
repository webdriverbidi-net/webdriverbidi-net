// <copyright file="DeleteCookiesCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Storage;

using System.Text.Json.Serialization;

/// <summary>
/// Result for deleting cookies using the storage.deleteCookies command.
/// </summary>
public record DeleteCookiesCommandResult : CommandResult
{
    private PartitionKey partitionKey = new();

    [JsonConstructor]
    private DeleteCookiesCommandResult()
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