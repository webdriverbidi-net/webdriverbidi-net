// <copyright file="AddDataCollectorCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Result for adding a data collector for network traffic using the network.addDataCollector command.
/// </summary>
public record AddDataCollectorCommandResult : CommandResult
{
    [JsonConstructor]
    private AddDataCollectorCommandResult()
    {
    }

    /// <summary>
    /// Gets the ID of the added network data collector.
    /// </summary>
    [JsonPropertyName("collector")]
    [JsonRequired]
    [JsonInclude]
    public string CollectorId { get; internal set; } = string.Empty;
}
