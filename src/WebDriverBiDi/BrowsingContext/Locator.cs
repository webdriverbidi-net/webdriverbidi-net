// <copyright file="Locator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a locator for locating nodes.
/// </summary>
[JsonDerivedType(typeof(AccessibilityLocator))]
[JsonDerivedType(typeof(ContextLocator))]
[JsonDerivedType(typeof(CssLocator))]
[JsonDerivedType(typeof(InnerTextLocator))]
[JsonDerivedType(typeof(XPathLocator))]
public abstract class Locator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Locator"/> class.
    /// </summary>
    protected Locator()
    {
    }

    /// <summary>
    /// Gets the type of locator.
    /// </summary>
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    /// <summary>
    /// Gets the value of to use in locating nodes.
    /// </summary>
    [JsonPropertyName("value")]
    public abstract object Value { get; }
}
