// <copyright file="NavigateCommandSettings.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using System;
using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the browsingContext.navigate command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class NavigateCommandSettings : CommandData<BrowsingContextNavigateResult>
{
    private string browsingContextId;
    private string url;
    private ReadinessState? wait;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigateCommandSettings" /> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context to navigate.</param>
    /// <param name="url">The URL to which to navigate.</param>
    public NavigateCommandSettings(string browsingContextId, string url)
    {
        this.browsingContextId = browsingContextId;
        this.url = url;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "browsingContext.navigate";

    /// <summary>
    /// Gets or sets the ID of the browsing context to navigate.
    /// </summary>
    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets the URL to which to navigate.
    /// </summary>
    [JsonProperty("url")]
    [JsonRequired]
    public string Url { get => this.url; set => this.url = value; }

    /// <summary>
    /// Gets or sets the <see cref="ReadinessState" /> value for which to wait during the navigation.
    /// </summary>
    public ReadinessState? Wait { get => this.wait; set => this.wait = value; }

    /// <summary>
    /// Gets the string value of the readiness state for serialization purposes.
    /// </summary>
    [JsonProperty("wait", NullValueHandling = NullValueHandling.Ignore)]
    internal string? SerializableWait
    {
        get
        {
            if (this.wait is null)
            {
                return null;
            }

            return this.wait.Value.ToString().ToLowerInvariant();
        }
    }
}