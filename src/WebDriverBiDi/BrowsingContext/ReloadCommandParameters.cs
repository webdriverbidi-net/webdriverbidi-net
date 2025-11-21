// <copyright file="ReloadCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browsingContext.reload command.
/// </summary>
public class ReloadCommandParameters : CommandParameters<ReloadCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReloadCommandParameters" /> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context to reload.</param>
    public ReloadCommandParameters(string browsingContextId)
    {
        this.BrowsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browsingContext.reload";

    /// <summary>
    /// Gets or sets the ID of the browsing context to reload.
    /// </summary>
    [JsonPropertyName("context")]
    public string BrowsingContextId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the browser cache should be ignored when reloading.
    /// </summary>
    [JsonPropertyName("ignoreCache")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IgnoreCache { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ReadinessState" /> value for which to wait during the reload.
    /// </summary>
    [JsonPropertyName("wait")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ReadinessState? Wait { get; set; }
}
