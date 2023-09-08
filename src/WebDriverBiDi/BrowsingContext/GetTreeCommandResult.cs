// <copyright file="GetTreeCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Result for getting the tree of browsing contexts using the browserContext.getTree command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class GetTreeCommandResult : CommandResult
{
    private List<BrowsingContextInfo> contextTree = new();

    [JsonConstructor]
    private GetTreeCommandResult()
    {
    }

    /// <summary>
    /// Gets the read-only tree of browsing contexts.
    /// </summary>
    public IList<BrowsingContextInfo> ContextTree => this.contextTree.AsReadOnly();

    /// <summary>
    /// Gets or sets the tree of browsing contexts for serialization purposes.
    /// </summary>
    [JsonProperty("contexts")]
    [JsonRequired]
    internal List<BrowsingContextInfo> SerializableContextTree { get => this.contextTree; set => this.contextTree = value; }
}