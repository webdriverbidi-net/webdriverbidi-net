// <copyright file="TraverseHistoryCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browsingContext.traverseHistory command.
/// </summary>
public class TraverseHistoryCommandParameters : CommandParameters<EmptyResult>
{
    private string browsingContextId;
    private long delta;

    /// <summary>
    /// Initializes a new instance of the <see cref="TraverseHistoryCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to capture the screenshot.</param>
    /// <param name="delta">The number of positions, forward or backward, to move in the browser history.</param>
    public TraverseHistoryCommandParameters(string browsingContextId, long delta)
    {
        this.browsingContextId = browsingContextId;
        this.delta = delta;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browsingContext.traverseHistory";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to traverse the history.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonInclude]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets the number of entries in the history to traverse in the browser history. Positive
    /// values move forward in the history; negative values move backward in the history.
    /// </summary>
    [JsonPropertyName("delta")]
    [JsonInclude]
    public long Delta { get => this.delta; set => this.delta = value; }
}
