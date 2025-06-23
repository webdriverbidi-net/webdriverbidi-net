// <copyright file="AddDataCollectorCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the network.addDataCollector command.
/// </summary>
public class AddDataCollectorCommandParameters : CommandParameters<AddDataCollectorCommandResult>
{
    private readonly List<string> contextList = [];
    private readonly List<string> userContextList = [];
    private List<DataType> dataTypes = [DataType.Response];
    private ulong maxEncodedDataSize = 0;
    private CollectorType? collectorType;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddDataCollectorCommandParameters" /> class.
    /// </summary>
    public AddDataCollectorCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "network.addDataCollector";

    /// <summary>
    /// Gets or sets the list of <see cref="DataType"/> values associated with this data collector.
    /// </summary>
    [JsonPropertyName("dataTypes")]
    [JsonInclude]
    [JsonRequired]
    public List<DataType> DataTypes { get => this.dataTypes; set => this.dataTypes = value; }

    /// <summary>
    /// Gets or sets the maximum encoded data size for this collector in bytes.
    /// </summary>
    [JsonPropertyName("maxEncodedDataSize")]
    [JsonInclude]
    [JsonRequired]
    public ulong MaxEncodedDataSize { get => this.maxEncodedDataSize; set => this.maxEncodedDataSize = value; }

    /// <summary>
    /// Gets or sets the type of this data collector. If unset, defaults to <see cref="CollectorType.Blob"/>.
    /// </summary>
    [JsonPropertyName("collectorType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public CollectorType? CollectorType { get => this.collectorType; set => this.collectorType = value; }

    /// <summary>
    /// Gets the list of browsing context IDs for which to collect network data.
    /// </summary>
    [JsonIgnore]
    public List<string> BrowsingContexts => this.contextList;

    /// <summary>
    /// Gets the list of user context IDs for which to collect network data.
    /// </summary>
    [JsonIgnore]
    public List<string> UserContexts => this.userContextList;

    /// <summary>
    /// Gets the list of browsing context IDs for which to collect network data for serialization purposes.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<string>? SerializableContexts
    {
        get
        {
            if (this.contextList.Count == 0)
            {
                return null;
            }

            return this.contextList;
        }
    }

    /// <summary>
    /// Gets the list of user context IDs for which to collect network data for serialization purposes.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<string>? SerializableUserContexts
    {
        get
        {
            if (this.userContextList.Count == 0)
            {
                return null;
            }

            return this.userContextList;
        }
    }
}
