// <copyright file="LocateNodesCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;
using WebDriverBiDi.Script;

/// <summary>
/// Result for locating nodes using the browserContext.locateNodes command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class LocateNodesCommandResult : CommandResult
{
    private List<RemoteValue> resultNodes = new();

    [JsonConstructor]
    private LocateNodesCommandResult()
    {
    }

    /// <summary>
    /// Gets the read-only list of located nodes.
    /// </summary>
    public IList<RemoteValue> Nodes => this.resultNodes.AsReadOnly();

    /// <summary>
    /// Gets or sets the list of located nodes for serialization purposes.
    /// </summary>
    [JsonProperty("nodes")]
    [JsonRequired]
    internal List<RemoteValue> SerializableNodes { get => this.resultNodes; set => this.resultNodes = value; }
}
