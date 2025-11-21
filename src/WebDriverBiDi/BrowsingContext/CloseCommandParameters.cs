// <copyright file="CloseCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browsingContext.close command.
/// </summary>
public class CloseCommandParameters : CommandParameters<CloseCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CloseCommandParameters" /> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context to close.</param>
    public CloseCommandParameters(string browsingContextId)
    {
        this.BrowsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browsingContext.close";

    /// <summary>
    /// Gets or sets the ID of the browsing context to close.
    /// </summary>
    [JsonPropertyName("context")]
    public string BrowsingContextId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to prompt for unloading the page when closing the browsing context.
    /// </summary>
    [JsonPropertyName("promptUnload")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? PromptUnload { get; set; }
}
