// <copyright file="BrowsingContextNavigateResult.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Contains the result of a navigation.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class BrowsingContextNavigateResult : CommandResult
{
    private string? id;
    private string url = string.Empty;

    [JsonConstructor]
    private BrowsingContextNavigateResult()
    {
    }

    /// <summary>
    /// Gets the ID of the navigation.
    /// </summary>
    [JsonProperty("navigation")]
    public string? NavigationId { get => this.id; internal set => this.id = value; }

    /// <summary>
    /// Gets the URL of the navigation.
    /// </summary>
    [JsonProperty("url")]
    [JsonRequired]
    public string Url { get => this.url; internal set => this.url = value; }
}