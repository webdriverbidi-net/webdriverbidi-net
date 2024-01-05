// <copyright file="Locator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a locator for locating nodes.
/// </summary>
[JsonDerivedType(typeof(CssLocator))]
[JsonDerivedType(typeof(InnerTextLocator))]
[JsonDerivedType(typeof(XPathLocator))]
public abstract class Locator
{
    private string value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Locator"/> class.
    /// </summary>
    /// <param name="value">The value to use in locating nodes for the specified locator type.</param>
    protected Locator(string value)
    {
        this.value = value;
    }

    /// <summary>
    /// Gets the type of locator.
    /// </summary>
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    /// <summary>
    /// Gets or sets the value of to use in locating nodes.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get => this.value; set => this.value = value; }
}
