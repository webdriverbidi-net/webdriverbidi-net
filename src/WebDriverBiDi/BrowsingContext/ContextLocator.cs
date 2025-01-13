// <copyright file="ContextLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a locator for locating context nodes of given browsing contexts like, for example, the iframe element
/// hosting a BrowsingContext in the iframe.
/// </summary>
public class ContextLocator : Locator
{
    private readonly string type = "context";
    private readonly Dictionary<string, string> contextAttributes = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextLocator"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to locate the context node.</param>
    public ContextLocator(string browsingContextId)
        : base()
    {
        this.contextAttributes["context"] = browsingContextId;
    }

    /// <summary>
    /// Gets the type of locator.
    /// </summary>
    [JsonPropertyName("type")]
    public override string Type => this.type;

    /// <summary>
    /// Gets a read-only version of a dictionary containing the accessibility attributes to use in locating nodes.
    /// </summary>
    [JsonPropertyName("value")]
    public override object Value => new ReadOnlyDictionary<string, string>(this.contextAttributes);

    /// <summary>
    /// Gets the browsing context for which to get the context node..
    /// </summary>
    [JsonIgnore]
    public string BrowsingContextId => this.contextAttributes["context"];
}
