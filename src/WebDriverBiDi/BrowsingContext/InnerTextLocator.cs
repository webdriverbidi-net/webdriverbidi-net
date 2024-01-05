// <copyright file="InnerTextLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a locator for locating nodes via their visible text
/// as defined by the innerText DOM property.
/// </summary>
public class InnerTextLocator : Locator
{
    private readonly string type = "innerText";
    private bool? ignoreCase;
    private InnerTextMatchType? matchType;
    private ulong? maxDepth;

    /// <summary>
    /// Initializes a new instance of the <see cref="InnerTextLocator"/> class.
    /// </summary>
    /// <param name="value">The text to use in locating nodes.</param>
    public InnerTextLocator(string value)
        : base(value)
    {
    }

    /// <summary>
    /// Gets the type of locator.
    /// </summary>
    [JsonPropertyName("type")]
    public override string Type => this.type;

    /// <summary>
    /// Gets or sets a value indicating whether the locator should ignore case when matching.
    /// When omitted or <see langword="null"/>, the match is case-sensitive.
    /// </summary>
    [JsonPropertyName("ignoreCase")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]

    public bool? IgnoreCase { get => this.ignoreCase; set => this.ignoreCase = value; }

    /// <summary>
    /// Gets or sets a value indicating the type of match for the text, partial or full.
    /// When omitted or <see langword="null"/>, the match is a full-text match.
    /// </summary>
    [JsonPropertyName("matchType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InnerTextMatchType? MatchType { get => this.matchType; set => this.matchType = value; }

    /// <summary>
    /// Gets or sets a value indicating the maximum depth to which to search for nodes.
    /// When omitted or <see langword="null"/>, the locator will return matches to an
    /// infinite depth.
    /// </summary>
    [JsonPropertyName("maxDepth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? MaxDepth { get => this.maxDepth; set => this.maxDepth = value; }
}
