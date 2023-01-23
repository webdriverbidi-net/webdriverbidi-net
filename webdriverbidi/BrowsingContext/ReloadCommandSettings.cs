// <copyright file="ReloadCommandSettings.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using System;
using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the browsingContext.reload command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class ReloadCommandSettings : CommandData<BrowsingContextNavigateResult>
{
    private string browsingContextId;
    private bool? ignoreCache;
    private ReadinessState? wait;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReloadCommandSettings" /> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context to reload.</param>
    public ReloadCommandSettings(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "browsingContext.reload";

    /// <summary>
    /// Gets or sets the ID of the browsing context to reload.
    /// </summary>
    [JsonProperty("context")]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the browser cache should be ignored when reloading.
    /// </summary>
    [JsonProperty("ignoreCache", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IgnoreCache { get => this.ignoreCache; set => this.ignoreCache = value; }

    /// <summary>
    /// Gets or sets the <see cref="ReadinessState" /> value for which to wait during the reload.
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