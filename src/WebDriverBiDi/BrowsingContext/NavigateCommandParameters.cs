// <copyright file="NavigateCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System;
using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides parameters for the browsingContext.navigate command.
/// </summary>
public class NavigateCommandParameters : CommandParameters<NavigationResult>
{
    private string browsingContextId;
    private string url;
    private ReadinessState? wait;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigateCommandParameters" /> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context to navigate.</param>
    /// <param name="url">The URL to which to navigate.</param>
    public NavigateCommandParameters(string browsingContextId, string url)
    {
        this.browsingContextId = browsingContextId;
        this.url = url;
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
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets the URL to which to navigate.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonRequired]
    public string Url { get => this.url; set => this.url = value; }

    /// <summary>
    /// Gets or sets the <see cref="ReadinessState" /> value for which to wait during the navigation.
    /// </summary>
    [JsonPropertyName("wait")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ReadinessState? Wait { get => this.wait; set => this.wait = value; }
}
