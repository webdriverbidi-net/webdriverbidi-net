// <copyright file="CssLocator.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Represents a locator for locating nodes via CSS selector.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class CssLocator : Locator
{
    private readonly string type = "css";

    /// <summary>
    /// Initializes a new instance of the <see cref="CssLocator"/> class.
    /// </summary>
    /// <param name="value">The CSS selector to use in locating nodes.</param>
    public CssLocator(string value)
        : base(value)
    {
    }

    /// <summary>
    /// Gets the type of locator.
    /// </summary>
    [JsonProperty("type")]
    public override string Type => this.type;
}
