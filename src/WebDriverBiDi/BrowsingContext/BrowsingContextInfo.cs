// <copyright file="BrowsingContextInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Provides information about a browsing context.
/// </summary>
public class BrowsingContextInfo
{
    private string id = string.Empty;
    private string clientWindowId = string.Empty;
    private string url = string.Empty;
    private string userContextId = string.Empty;
    private string? originalOpener = null;
    private List<BrowsingContextInfo> children = new();
    private string? parentId;

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
    public string BrowsingContextId { get => this.id; private set => this.id = value; }

    /// <summary>
    /// Gets the ID of the client window that contains this browsing context.
    /// </summary>
    [JsonPropertyName("clientWindow")]
    // TODO (Issue #31): Uncomment once https://bugzilla.mozilla.org/show_bug.cgi?id=1920952 is fixed.
    // [JsonRequired]
    [JsonInclude]
    public string ClientWindowId { get => this.clientWindowId; private set => this.clientWindowId = value; }

    /// <summary>
    /// Gets the browsing context ID of the original opener of this browsing context.
    /// </summary>
    [JsonPropertyName("originalOpener")]
    [JsonRequired]
    [JsonInclude]
    public string? OriginalOpener { get => this.originalOpener; private set => this.originalOpener = value; }

    /// <summary>
    /// Gets the URL of the browsing context.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonRequired]
    [JsonInclude]
    public string Url { get => this.url; private set => this.url = value; }

    /// <summary>
    /// Gets the ID of the user context of the browsing context.
    /// </summary>
    [JsonPropertyName("userContext")]
    [JsonRequired]
    [JsonInclude]
    public string UserContextId { get => this.userContextId; private set => this.userContextId = value; }

    /// <summary>
    /// Gets the read-only list of child browsing contexts for this browsing context.
    /// </summary>
    public IList<BrowsingContextInfo> Children => this.children.AsReadOnly();

    /// <summary>
    /// Gets the ID of the parent browsing context of this browsing context.
    /// </summary>
    [JsonPropertyName("parent")]
    [JsonInclude]
    public string? Parent { get => this.parentId; private set => this.parentId = value; }

    /// <summary>
    /// Gets or sets the list of child browsing contexts for this browsing context.
    /// </summary>
    [JsonPropertyName("children")]
    [JsonRequired]
    [JsonInclude]
    internal List<BrowsingContextInfo> SerializableChildren { get => this.children; set => this.children = value; }
}
