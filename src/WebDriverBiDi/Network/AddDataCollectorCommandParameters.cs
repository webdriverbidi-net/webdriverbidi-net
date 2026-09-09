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
    /// <summary>
    /// Initializes a new instance of the <see cref="AddDataCollectorCommandParameters" /> class.
    /// </summary>
    /// <param name="maxEncodedDataSize">
    /// The maximum size (in bytes) allocated to be collected for each request or response collected.
    /// Note carefully that zero (0) is an invalid value for maximum size.
    /// </param>
    /// <param name="collectionDataType">The <see cref="DataType"/> value specifying the type for which data will be collected.</param>
    /// <param name="additionalCollectionDataTypes">
    /// One or more additional <see cref="DataType"/> values specifying the type for which data will
    /// also be collected.
    /// </param>
    public AddDataCollectorCommandParameters(ulong maxEncodedDataSize, DataType collectionDataType, params DataType[] additionalCollectionDataTypes)
    {
        this.MaxEncodedDataSize = maxEncodedDataSize;
        this.DataTypes.Add(collectionDataType);
        foreach (DataType dataType in additionalCollectionDataTypes)
        {
            this.DataTypes.Add(dataType);
        }
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "network.addDataCollector";

    /// <summary>
    /// Gets the set of <see cref="DataType"/> values associated with this data collector.
    /// </summary>
    [JsonPropertyName("dataTypes")]
    public HashSet<DataType> DataTypes { get; } = [];

    /// <summary>
    /// Gets or sets the maximum encoded data size for this collector in bytes.
    /// </summary>
    [JsonPropertyName("maxEncodedDataSize")]
    public ulong MaxEncodedDataSize { get; set; }

    /// <summary>
    /// Gets or sets the type of this data collector. If unset, defaults to <see cref="CollectorType.Blob"/>.
    /// </summary>
    [JsonPropertyName("collectorType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CollectorType? CollectorType { get; set; }

    /// <summary>
    /// Gets the list of browsing context IDs for which to collect network data.
    /// </summary>
    /// <remarks>
    /// The protocol requires this property, when present, to contain at least one entry.
    /// An empty list therefore means "not specified": the property is omitted from the JSON
    /// payload entirely, and an empty array is never sent. Add entries to the list to scope
    /// the command.
    /// </remarks>
    [JsonIgnore]
    public List<string> Contexts { get; } = [];

    /// <summary>
    /// Gets the list of user context IDs for which to collect network data.
    /// </summary>
    /// <remarks>
    /// The protocol requires this property, when present, to contain at least one entry.
    /// An empty list therefore means "not specified": the property is omitted from the JSON
    /// payload entirely, and an empty array is never sent. Add entries to the list to scope
    /// the command.
    /// </remarks>
    [JsonIgnore]
    public List<string> UserContexts { get; } = [];

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
            if (this.Contexts.Count == 0)
            {
                return null;
            }

            return this.Contexts;
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
            if (this.UserContexts.Count == 0)
            {
                return null;
            }

            return this.UserContexts;
        }
    }
}
