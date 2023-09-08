// <copyright file="GetTreeCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System;
using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the browsingContext.create command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class GetTreeCommandParameters : CommandParameters<GetTreeCommandResult>
{
    private int? maxDepth;
    private string? rootBrowsingContextId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeCommandParameters" /> class.
    /// </summary>
    public GetTreeCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "browsingContext.getTree";

    /// <summary>
    /// Gets or sets the maximum depth to traverse the tree.
    /// </summary>
    [JsonProperty("maxDepth", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaxDepth { get => this.maxDepth; set => this.maxDepth = value; }

    /// <summary>
    /// Gets or sets the ID of the browsing context used as the root of the tree.
    /// </summary>
    [JsonProperty("root", NullValueHandling = NullValueHandling.Ignore)]
    public string? RootBrowsingContextId { get => this.rootBrowsingContextId; set => this.rootBrowsingContextId = value; }
}