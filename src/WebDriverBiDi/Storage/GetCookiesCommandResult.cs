// <copyright file="GetCookiesCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Storage;

using System.Text.Json.Serialization;
using WebDriverBiDi.Network;

/// <summary>
/// Result for getting cookies using the storage.getCookies command.
/// </summary>
public record GetCookiesCommandResult : CommandResult
{
    [JsonConstructor]
    private GetCookiesCommandResult()
    {
    }

    /// <summary>
    /// Gets the read-only list of cookies returned by the command.
    /// </summary>
    [JsonIgnore]
    public IList<Cookie> Cookies => this.SerializableCookies.AsReadOnly();

    /// <summary>
    /// Gets the partition key for the list of returned cookies.
    /// </summary>
    [JsonPropertyName("partitionKey")]
    [JsonRequired]
    [JsonInclude]
    public PartitionKey PartitionKey { get; internal set; } = new();

    /// <summary>
    /// Gets or sets the list of cookies returned by the command for serialization purposes.
    /// </summary>
    [JsonPropertyName("cookies")]
    [JsonRequired]
    [JsonInclude]
    internal List<Cookie> SerializableCookies { get; set; } = [];
}