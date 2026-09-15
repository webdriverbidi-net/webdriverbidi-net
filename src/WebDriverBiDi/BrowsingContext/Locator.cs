// <copyright file="Locator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a locator for locating nodes.
/// </summary>
/// <remarks>
/// Each subclass exposes the value it locates by with a type of its own: the selector, expression, or text of a
/// <see cref="CssLocator"/>, <see cref="XPathLocator"/>, or <see cref="InnerTextLocator"/> is its
/// <c>Value</c> string; an <see cref="AccessibilityLocator"/> locates by its <see cref="AccessibilityLocator.Name"/>
/// and <see cref="AccessibilityLocator.Role"/>; and a <see cref="ContextLocator"/> locates by its
/// <see cref="ContextLocator.BrowsingContextId"/>.
/// </remarks>
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
}
