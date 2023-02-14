// <copyright file="BrowsingContextInfo.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Provides information about a browsing context.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class BrowsingContextInfo
{
    private string id = string.Empty;
    private string url = string.Empty;
    private List<BrowsingContextInfo> children = new();
    private string? parentId;

    [JsonConstructor]
    private BrowsingContextInfo()
    {
    }

    /// <summary>
    /// Gets the ID of the browsing context.
    /// </summary>
    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.id; internal set => this.id = value; }

    /// <summary>
    /// Gets the URL of the browsing context.
    /// </summary>
    [JsonProperty("url")]
    [JsonRequired]
    public string Url { get => this.url; internal set => this.url = value; }

    /// <summary>
    /// Gets the read-only list of child browsing contexts for this browsing context.
    /// </summary>
    public IList<BrowsingContextInfo> Children => this.children.AsReadOnly();

    /// <summary>
    /// Gets the ID of the parent browsing context of this browsing context.
    /// </summary>
    [JsonProperty("parent")]
    public string? Parent { get => this.parentId; internal set => this.parentId = value; }

    /// <summary>
    /// Gets or sets the list of child browsing contexts for this browsing context.
    /// </summary>
    [JsonProperty("children")]
    [JsonRequired]
    internal List<BrowsingContextInfo> SerializableChildren { get => this.children; set => this.children = value; }
}
