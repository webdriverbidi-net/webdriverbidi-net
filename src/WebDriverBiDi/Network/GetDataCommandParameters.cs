// <copyright file="GetDataCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the network.getData command.
/// </summary>
public class GetDataCommandParameters : CommandParameters<GetDataCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDataCommandParameters" /> class.
    /// </summary>
    /// <param name="requestId">The ID of the network request for which to fetch network data from the collector.</param>
    public GetDataCommandParameters(string requestId)
    {
        this.RequestId = requestId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "network.getData";

    /// <summary>
    /// Gets or sets the ID of the request for which to get collected network data.
    /// </summary>
    [JsonPropertyName("request")]
    [JsonInclude]
    [JsonRequired]
    public string RequestId { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DataType"/> of collected network data to get.
    /// </summary>
    [JsonPropertyName("dataType")]
    [JsonInclude]
    [JsonRequired]
    public DataType DataType { get; set; } = DataType.Response;

    /// <summary>
    /// Gets or sets the ID of the data collector for which to get collected network data.
    /// </summary>
    [JsonPropertyName("collector")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CollectorId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the retrieved collected data should be removed from the collector after retrieval.
    /// </summary>
    [JsonPropertyName("disown")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? DisownCollectedData { get; set; }
}
