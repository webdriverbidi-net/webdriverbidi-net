// <copyright file="LocatorChainEntry.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Elements;

using WebDriverBiDi.BrowsingContext;

/// <summary>
/// Represents an entry in the locator chain for element resolution.
/// </summary>
internal sealed class LocatorChainEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocatorChainEntry"/> class.
    /// </summary>
    /// <param name="locator">The locator strategy for this entry.</param>
    /// <param name="specificIndex">The specific index to select from multiple matches, if any.</param>
    public LocatorChainEntry(Locator locator, int? specificIndex)
    {
        this.Locator = locator;
        this.SpecificIndex = specificIndex;
    }

    /// <summary>
    /// Gets the locator strategy for this entry.
    /// </summary>
    public Locator Locator { get; }

    /// <summary>
    /// Gets the specific index to select from multiple matches, if any.
    /// </summary>
    public int? SpecificIndex { get; }
}
