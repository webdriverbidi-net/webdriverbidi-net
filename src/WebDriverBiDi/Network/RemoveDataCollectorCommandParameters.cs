// <copyright file="RemoveDataCollectorCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the network.disownData command.
/// </summary>
public class RemoveDataCollectorCommandParameters : CommandParameters<RemoveDataCollectorCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveDataCollectorCommandParameters" /> class.
    /// </summary>
    /// <param name="collectorId">The ID of the collector collecting network data to be released.</param>
    public RemoveDataCollectorCommandParameters(string collectorId)
    {
        this.CollectorId = collectorId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "network.removeDataCollector";

    /// <summary>
    /// Gets or sets the ID of the data collector to remove.
    /// </summary>
    [JsonPropertyName("collector")]
    [JsonInclude]
    [JsonRequired]
    public string CollectorId { get; set; }
}
