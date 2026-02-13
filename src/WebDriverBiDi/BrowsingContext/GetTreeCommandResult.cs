// <copyright file="GetTreeCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Result for getting the tree of browsing contexts using the browserContext.getTree command.
/// </summary>
public record GetTreeCommandResult : CommandResult
{
    [JsonConstructor]
    internal GetTreeCommandResult()
    {
    }

    /// <summary>
    /// Gets the read-only tree of browsing contexts.
    /// </summary>
    public IList<BrowsingContextInfo> ContextTree => this.SerializableContextTree.AsReadOnly();

    /// <summary>
    /// Gets or sets the tree of browsing contexts for serialization purposes.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonRequired]
    [JsonInclude]
    internal List<BrowsingContextInfo> SerializableContextTree { get; set; } = [];
}
