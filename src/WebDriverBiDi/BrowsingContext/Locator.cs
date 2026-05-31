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
    /// Gets the value to use in locating nodes.
    /// </summary>
    /// <remarks>
    /// The runtime type of the returned value depends on the concrete <see cref="Locator"/> subclass:
    /// <list type="bullet">
    ///   <item><description><see cref="CssLocator"/> — <see cref="string"/> (the CSS selector)</description></item>
    ///   <item><description><see cref="XPathLocator"/> — <see cref="string"/> (the XPath expression)</description></item>
    ///   <item><description><see cref="InnerTextLocator"/> — <see cref="string"/> (the inner-text value)</description></item>
    ///   <item><description><see cref="AccessibilityLocator"/> — <see cref="System.Collections.ObjectModel.ReadOnlyDictionary{TKey, TValue}">ReadOnlyDictionary&lt;string, string&gt;</see> (accessibility attribute map)</description></item>
    ///   <item><description><see cref="ContextLocator"/> — <see cref="System.Collections.ObjectModel.ReadOnlyDictionary{TKey, TValue}">ReadOnlyDictionary&lt;string, string&gt;</see> (context attribute map)</description></item>
    /// </list>
    /// </remarks>
    [JsonPropertyName("value")]
    public abstract object Value { get; }
}
