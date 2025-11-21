// <copyright file="NavigateCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browsingContext.navigate command.
/// </summary>
public class NavigateCommandParameters : CommandParameters<NavigateCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NavigateCommandParameters" /> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context to navigate.</param>
    /// <param name="url">The URL to which to navigate.</param>
    public NavigateCommandParameters(string browsingContextId, string url)
    {
        this.BrowsingContextId = browsingContextId;
        this.Url = url;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browsingContext.navigate";

    /// <summary>
    /// Gets or sets the ID of the browsing context to navigate.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    public string BrowsingContextId { get; set; }

    /// <summary>
    /// Gets or sets the URL to which to navigate.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonRequired]
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ReadinessState" /> value for which to wait during the navigation.
    /// </summary>
    [JsonPropertyName("wait")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ReadinessState? Wait { get; set; }
}
