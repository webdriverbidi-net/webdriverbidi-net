// <copyright file="GetTreeCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browsingContext.create command.
/// </summary>
public class GetTreeCommandParameters : CommandParameters<GetTreeCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeCommandParameters" /> class.
    /// </summary>
    public GetTreeCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browsingContext.getTree";

    /// <summary>
    /// Gets or sets the maximum depth to traverse the tree.
    /// </summary>
    [JsonPropertyName("maxDepth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxDepth { get; set; }

    /// <summary>
    /// Gets or sets the ID of the browsing context used as the root of the tree.
    /// </summary>
    [JsonPropertyName("root")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RootBrowsingContextId { get; set; }
}
