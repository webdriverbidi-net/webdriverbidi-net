// <copyright file="LocateNodesCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;
using WebDriverBiDi.Script;

/// <summary>
/// Result for locating nodes using the browserContext.locateNodes command.
/// </summary>
public record LocateNodesCommandResult : CommandResult
{
    [JsonConstructor]
    internal LocateNodesCommandResult()
    {
    }

    /// <summary>
    /// Gets the read-only list of located nodes.
    /// </summary>
    public IList<RemoteValue> Nodes => this.SerializableNodes.AsReadOnly();

    /// <summary>
    /// Gets or sets the list of located nodes for serialization purposes.
    /// </summary>
    [JsonPropertyName("nodes")]
    [JsonRequired]
    [JsonInclude]
    internal List<RemoteValue> SerializableNodes { get; set; } = [];
}
