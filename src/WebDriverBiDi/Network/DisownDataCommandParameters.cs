// <copyright file="DisownDataCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the network.disownData command.
/// </summary>
public class DisownDataCommandParameters : CommandParameters<EmptyResult>
{
    private DataType dataType = DataType.Response;
    private string collectorId;
    private string requestId;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisownDataCommandParameters" /> class.
    /// </summary>
    /// <param name="collectorId">The ID of the collector collecting network data to be released.</param>
    /// <param name="requestId">The ID of the network request for which to release network data from the collector.</param>
    public DisownDataCommandParameters(string collectorId, string requestId)
    {
        this.collectorId = collectorId;
        this.requestId = requestId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "network.disownData";

    /// <summary>
    /// Gets or sets the <see cref="DataType"/> of collected network data to release.
    /// </summary>
    [JsonPropertyName("dataType")]
    [JsonInclude]
    [JsonRequired]
    public DataType DataType { get => this.dataType; set => this.dataType = value; }

    /// <summary>
    /// Gets or sets the ID of the data collector for which to release collected network data.
    /// </summary>
    [JsonPropertyName("collector")]
    [JsonInclude]
    [JsonRequired]
    public string CollectorId { get => this.collectorId; set => this.collectorId = value; }

    /// <summary>
    /// Gets or sets the ID of the request for which to release collected network data.
    /// </summary>
    [JsonPropertyName("request")]
    [JsonInclude]
    [JsonRequired]
    public string RequestId { get => this.requestId; set => this.requestId = value; }
}
