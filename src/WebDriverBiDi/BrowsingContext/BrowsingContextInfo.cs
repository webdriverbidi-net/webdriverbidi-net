// <copyright file="BrowsingContextInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Provides information about a browsing context.
/// </summary>
public record BrowsingContextInfo
{
    [JsonConstructor]
    private BrowsingContextInfo()
    {
    }

    /// <summary>
    /// Gets the ID of the browsing context.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string BrowsingContextId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the ID of the client window that contains this browsing context.
    /// </summary>
    [JsonPropertyName("clientWindow")]
    [JsonRequired]
    [JsonInclude]
    public string ClientWindowId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the browsing context ID of the original opener of this browsing context.
    /// </summary>
    [JsonPropertyName("originalOpener")]
    [JsonRequired]
    [JsonInclude]
    public string? OriginalOpener { get; internal set; } = null;

    /// <summary>
    /// Gets the URL of the browsing context.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonRequired]
    [JsonInclude]
    public string Url { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the ID of the user context of the browsing context.
    /// </summary>
    [JsonPropertyName("userContext")]
    [JsonRequired]
    [JsonInclude]
    public string UserContextId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the read-only list of child browsing contexts for this browsing context.
    /// </summary>
    public IList<BrowsingContextInfo> Children => this.SerializableChildren.AsReadOnly();

    /// <summary>
    /// Gets the ID of the parent browsing context of this browsing context.
    /// </summary>
    [JsonPropertyName("parent")]
    [JsonInclude]
    public string? Parent { get; internal set; }

    /// <summary>
    /// Gets or sets the list of child browsing contexts for this browsing context.
    /// </summary>
    [JsonPropertyName("children")]
    [JsonRequired]
    [JsonInclude]
    internal List<BrowsingContextInfo> SerializableChildren { get; set; } = [];
}
